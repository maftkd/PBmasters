//TestServer.cs
//
//Description: A light weight udp server for handling message passing and synchronizing player states
//

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;

public class TestServer : MonoBehaviour
{
    public int myPort;
    public enum PostCode {
        SHAKE = 0,
        QUEUE = 1,
		INVITE = 2,
		PLAY = 3,
		FLIP = 4,
		LOG = 5,
		TEST = 6,
		BALL = 7,
		FULL_SYNC = 8
    };

	private Queue<NetPeer> mQueue = new Queue<NetPeer>();

	private Dictionary<int, IEnumerator> gameLobbies = new Dictionary<int, IEnumerator>(); //dict { key: port, value: available}

	public Transform tables;

    void Start()
    {
		gameLobbies.Add(3001, null);
		gameLobbies.Add(3002, null);
		gameLobbies.Add(3003, null);
		gameLobbies.Add(3004, null);
		gameLobbies.Add(3005, null);
        StartCoroutine(ServeUDP(3000));			
    } 

    private IEnumerator ServeUDP(int port){
        int curCode = -1;
		NetDataWriter writer = new NetDataWriter();
		EventBasedNetListener listener = new EventBasedNetListener();
		NetManager server = new NetManager(listener);
		Transform ball=null;
		Rigidbody ballBody=null;
		bool syncBall=false;
		Console.WriteLine("Starting Server on {0}", port);
		Debug.Log("Starting Server on "+port);
		server.Start(port);

		//left and right flipper for this port
		Transform left, right;
		FlipperButton fbLeft=null, fbRight=null;
		NetPeer lClient=null, rClient=null;
		bool bothPlayers=true;

		Transform playField = null;

		IEnumerator timeout=null;;
		if(port!=3000){
			timeout = WaitForTimeout(port, server);
			StartCoroutine(timeout);
			
			Console.WriteLine("Finding play field");
			playField = tables.Find(port.ToString());
			Console.WriteLine("Finding ball");
			ball = playField.Find("Ball");
			ballBody = ball.GetComponent<Rigidbody>();
			Console.WriteLine("Finding FlipperButton left");
			fbLeft = playField.Find("PlayerInput").Find("PlayerLeft").Find("ButtonParent").GetChild(0).GetChild(0).GetComponent<FlipperButton>();
			fbLeft.enabled=true;
			Console.WriteLine("Finding FlipperButton right");
			fbRight = playField.Find("PlayerInput").Find("PlayerRight").Find("ButtonParent").GetChild(0).GetChild(0).GetComponent<FlipperButton>();
			fbRight.enabled=true;
			Console.WriteLine("Found both flippers");
		}
		

		listener.ConnectionRequestEvent += request =>
		{
			if(server.PeersCount < 10)
				request.AcceptIfKey("TestKey");
			else
				request.Reject();
		};

		listener.PeerConnectedEvent += peer =>
		{
			Console.WriteLine("PeerConnectedEvent");
			if(testMode){
				lClient=peer;
			}
		};

		listener.PeerDisconnectedEvent += (peer, info) =>
		{
			//remove player from queue
			for(int i=0; i<mQueue.Count; i++){
				NetPeer nxt = mQueue.Dequeue();
				if(nxt.ConnectionNum!=peer.ConnectionNum)
					mQueue.Enqueue(nxt);
				else
					Console.WriteLine("Dequeueing [client] due to disconnect event");
			}

			//remove player from game lobby
			if(lClient!=null && rClient != null){
				if(peer.ConnectionNum==lClient.ConnectionNum){
					if(bothPlayers){
						Console.WriteLine("dispatch lClient leave");
						fbLeft.enabled=false;
						bothPlayers=false;
					}
					else
						TeardownServer(server,port,1);
				}
				else if(peer.ConnectionNum==rClient.ConnectionNum){
					if(bothPlayers){
						Console.WriteLine("dispatch rClient leave");
						fbRight.enabled=false;
						bothPlayers=false;
					}
					else
						TeardownServer(server,port,1);
				}
			}
		};

		listener.NetworkReceiveEvent += (peer, reader, delivery) =>
		{
			curCode=-1;
			try{
				curCode = reader.GetInt();
			}
			catch(Exception e){
				Console.WriteLine("error. malformatted packet");
			}

			switch(curCode){
				case 0://handshake
					if(port==3000){
						Console.WriteLine("Client Handshake Received");	
						UnityEngine.Debug.Log("Client Handshake Received");
						PacketLib.SendMessage(peer,writer,(int)PostCode.SHAKE);	
					}			
					break;
				case 1://queue
					//add peer to queue
					mQueue.Enqueue(peer);
					StartCoroutine(ManageQueue(writer));
					PacketLib.SendMessage(peer,writer,(int)PostCode.QUEUE);
					break;
				case 2://invite
					Console.WriteLine("[client] has joined game lobby on port: "+port);
					Debug.Log("[client] has joined game lobby on port: "+port);
					//assign left and right flipper
					if(reader.GetBool()){
						lClient = peer;
						if(rClient!=null){
							StartCoroutine(StartGame(lClient,rClient,writer,timeout,port,ball));
							syncBall=true;
						}
					}
					else{
						rClient = peer;
						if(lClient!=null){
							StartCoroutine(StartGame(lClient,rClient,writer,timeout,port,ball));
							syncBall=true;
						}
					}			
					//get a handle to the table/playfield for this port
					break;
				case 4://flip
					//rotate server paddle
					if(port!=3000){
						bool isLeft=reader.GetBool();
						bool active=reader.GetBool();
						if(rClient!=null)
							PacketLib.SendMessage(rClient,writer,(int)PostCode.FLIP, isLeft, active);
						if(lClient!=null)
							PacketLib.SendMessage(lClient,writer,(int)PostCode.FLIP, isLeft, active);
						
						//move paddles
						Debug.Log("Animating paddle movement server-side");
						if(fbLeft!=null && isLeft)
							fbLeft.Flip(active);
						else if(fbRight!=null && !isLeft)
							fbRight.Flip(active);
					}
					break;
				case 5://log
					Console.WriteLine(reader.GetString());
					break;
				case 6://test
					if(timeout==null){
						SystemTest();
					}
					else{
						StopCoroutine(timeout);
					}
					break;
				case -1:
				default:
					break;
			}
				
		};

		while(!Console.KeyAvailable)
		{
			server.PollEvents();
			if(syncBall && port!=3000){
				PacketLib.SendMessage(lClient, writer, (int)PostCode.BALL, ball.position, ballBody.angularVelocity);
				PacketLib.SendMessage(rClient, writer, (int)PostCode.BALL, ball.position, ballBody.angularVelocity);
			}
			yield return new WaitForSeconds(0.015f);
		}

		Console.WriteLine("Gracefully killing server");
		server.Stop();
		yield return new WaitForSeconds(5f); // wait 5 seconds after stopping server to kill app
		Application.Quit();
    }   

	void OnDestroy(){
		
		StopAllCoroutines();
	}
	void OnApplicationQuit(){
		
	}

	private IEnumerator ManageQueue(NetDataWriter writer){
		bool roomReady=false;
		int roomPort = 0;
		while(mQueue.Count>1){
			roomReady=false;
			foreach(KeyValuePair<int, IEnumerator> kvp in gameLobbies){
				if(kvp.Value==null){
					roomReady=true;
					roomPort=kvp.Key;
					IEnumerator gameLobby = ServeUDP(roomPort);
					gameLobbies[kvp.Key]= gameLobby;
					StartCoroutine(gameLobby);
					break;
				}
			}
			NetPeer left = mQueue.Dequeue();
			NetPeer right = mQueue.Dequeue();
			PacketLib.SendMessage(left,writer,(int)PostCode.INVITE,roomPort,true);
			PacketLib.SendMessage(right,writer,(int)PostCode.INVITE,roomPort,false);
			Console.WriteLine("2x[client] Match is ready on port: {0}!",roomPort);

			yield return new WaitForSeconds(1);
		}
	}

	private bool testMode=false;
	private void SystemTest(){
		int roomPort = 3001;
		testMode=true;
		if(gameLobbies[roomPort]==null){
			gameLobbies[roomPort] = ServeUDP(roomPort);
			StartCoroutine(gameLobbies[roomPort]);
		}
	}

	private IEnumerator StartGame(NetPeer l, NetPeer r, NetDataWriter writer, IEnumerator timeoutRoutine, int port, Transform ball){
		StopCoroutine(timeoutRoutine);
		
		Console.WriteLine("Server starting game for port: {0}",port);	
		int startTimer=3;
		while(startTimer>=0){
			Console.WriteLine("PlayTimer: {0}", startTimer);
			writer.Reset();
			writer.Put((int)PostCode.PLAY);
			writer.Put(startTimer);
			l.Send(writer, DeliveryMethod.ReliableOrdered);
			r.Send(writer, DeliveryMethod.ReliableOrdered);
			startTimer--;
			yield return new WaitForSeconds(1f);
		}
		//game should be started
		SyncBalls(l,r,writer, ball);
	}

	private void SyncBalls(NetPeer l, NetPeer r, NetDataWriter writer, Transform ball){

		writer.Reset();
		Rigidbody rb = ball.GetComponent<Rigidbody>();
		Vector3 angVel = rb.angularVelocity;
		Vector3 vel = rb.velocity;
		writer.Put((int)PostCode.FULL_SYNC);
		writer.Put(ball.position.x);writer.Put(ball.position.y);writer.Put(ball.position.z);
		writer.Put(angVel.x);writer.Put(angVel.y);writer.Put(angVel.z);
		writer.Put(vel.x);writer.Put(vel.y);writer.Put(vel.z);
		l.Send(writer, DeliveryMethod.ReliableOrdered);
		r.Send(writer, DeliveryMethod.ReliableOrdered);

		
	}

	//runs a timeout on server setup. As soon as the queue pops and the lobby is alotted the users each have 20 seconds to join the game. If both players
	//fail to join within the 20 second invite window, this timeout function will kill the lobby server, and print a message to the console.
	private IEnumerator WaitForTimeout(int port, NetManager man){
		Console.WriteLine("New server allocated. Waiting 20 seconds for players to join");
		int timer=0;
		while(timer<22){
			timer++;
			yield return new WaitForSeconds(1);
		}
		TeardownServer(man,port,0);
	}

	private void TeardownServer(NetManager man, int port, int reason){
		man.Stop();
		foreach(KeyValuePair<int, IEnumerator> kvp in gameLobbies){
			if(kvp.Key==port){
				//kill game lobby on this port
				StopCoroutine(gameLobbies[kvp.Key]);
				gameLobbies[kvp.Key]=null;
				Console.WriteLine("Server on port {0} has been taken offline for reasonCode: {1}",port, reason);
				break;
			}
		}
		
	}
}