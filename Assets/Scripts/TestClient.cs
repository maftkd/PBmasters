//TestClient.cs
//
//Description: A lightweight client class to establish socket comms on the client device
//

using System;
using System.Threading;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using LiteNetLib;
using LiteNetLib.Utils;

public class TestClient : MonoBehaviour
{
    public Text debugText;
    private NetPeer server;
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

    NetManager client;

    public GameManager mGame;
    NetDataWriter writer;
    private bool left;
    public string matchMakeUrl;
    
    private int curCode;

    void Start()
    {
        StartCoroutine(Poll(3000));        
    }

    private IEnumerator Poll(int port){

        writer = new NetDataWriter();
        EventBasedNetListener listener = new EventBasedNetListener();
        client = new NetManager(listener);
        client.Start();
        client.Connect(matchMakeUrl /* host ip or name */, port /* port */, "TestKey" /* text key or NetDataWriter */);

        listener.PeerConnectedEvent += (fromPeer) =>
        {
            print("PeerConnectedEvent");
            server = fromPeer;
            PacketLib.SendMessage(server,writer,(int)PostCode.SHAKE);
        };

        listener.NetworkReceiveEvent += (peer, reader, deliveryMethod) =>
        {
            curCode = -1;
            try{
                curCode = reader.GetInt();
            }
            catch(Exception e){
                debugText.text="Error. malformatted packet";
            }

            switch(curCode){
                case 0:
                    debugText.text = "Success. Connected to Server";
                    JoinQueue();
                    break;
                case 1:
                    debugText.text = "Finding match...";
                    break;
                case 2:
                    debugText.text = "Match invite received";
                    mGame.ReceiveMatchInvite(reader.GetInt(), reader.GetBool());
                    break;
                case 3:
                    int startsIn = reader.GetInt();
                    if(startsIn>0)
                        debugText.text = "Game starting in: "+startsIn;
                    else
                        debugText.text = "Game started!";
                    break;
                case 4:
                    Log("Received flip message on side: "+left);
                    mGame.Flip(reader.GetBool(),reader.GetBool());
                    break;
                case 7:
                    mGame.UpdateBall(reader.GetFloat(),reader.GetFloat(),reader.GetFloat(),
                                    reader.GetFloat(),reader.GetFloat(),reader.GetFloat());
                    break;
                case 8:
                    mGame.FullSyncBall(reader.GetFloat(),reader.GetFloat(),reader.GetFloat(),
                                    reader.GetFloat(),reader.GetFloat(),reader.GetFloat(),
                                    reader.GetFloat(),reader.GetFloat(),reader.GetFloat());
                    break;
                case -1:
                default:
                    break;
            }          
            reader.Recycle();
        };

        while(true)
        {
            client.PollEvents();
            yield return new WaitForSeconds(0.015f);
        }
        client.Stop();
    }

    void Update()
    {
    }

    void OnDestroy()
    {
        Console.WriteLine("Gracefully killing client");
		StopAllCoroutines();
        
        client.DisconnectAll();

        client.Stop();
    }
    void OnApplicationQuit(){
	}

    public void JoinQueue(){
        PacketLib.SendMessage(server, writer, (int)PostCode.QUEUE);
    }

    public IEnumerator Accepted(int roomPort, bool side, bool testing=false){
        if(testing){
            PacketLib.SendMessage(server, writer, (int)PostCode.TEST);
            yield return new WaitForSeconds(0.5f);
        }
        left=side;
        Debug.Log("Joining match in testMode: "+testing);
        StopAllCoroutines();
        client.Stop();

        yield return new WaitForSeconds(.5f);

        //join lobby
        StartCoroutine(Poll(roomPort));

        yield return new WaitForSeconds(1f);
        if(!testing)
            PacketLib.SendMessage(server, writer, (int)PostCode.INVITE, side);
        else{
            PacketLib.SendMessage(server, writer, (int)PostCode.TEST);
        }

        //send test flips
        StartCoroutine(mGame.SendTestFlips());
    }

    public void ActivateFlipper(bool left, bool active){
        Debug.Log("Sending flipper activation message");
        PacketLib.SendMessage(server,writer, (int)PostCode.FLIP,left,active);
    }

    public void Log(string logLine){
        PacketLib.SendMessage(server,writer, (int)PostCode.LOG,logLine);
    }
}