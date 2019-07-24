using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public bool testing=false;
    public CanvasGroup inviteCG;
    public TestClient client;
    public Text timerText, debugText, latencyText, leftText;
    private int roomPort;
    public bool left;
    public Transform tableParent;
    public Transform pTable;
    public Transform pPlayer;
    private IEnumerator inviteCountdown;
    private FlipperButton otherFlipper, myFlipper;
    private Transform ball;
    private Rigidbody ballBody;
    // Start is called before the first frame update
    void Start()
    {
        inviteCG.alpha=0;
        inviteCG.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //temp debug code
        if(Input.GetKeyUp(KeyCode.M)){
            //set testing flags
            this.left=false;
            testing=true;
            //spoof an accepted invite for socket 3001 with side left, and testing flag to true
            StartCoroutine(client.Accepted(3001, left, true));

            //assign game flags
            pTable = tableParent.Find("3001");
            if(GameObject.FindGameObjectsWithTag("Client").Length>0){
                Transform target = pTable.Find("TableCam");
                Camera.main.transform.GetComponent<Camera>().enabled=false;
                target.GetComponent<Camera>().enabled=true;
            }
            //hook up "left" flipper
            pTable = tableParent.Find("3001");
            myFlipper = pTable.Find("PlayerInput").Find("PlayerLeft").Find("ButtonParent").GetChild(0).GetChild(0).GetComponent<FlipperButton>();
            myFlipper.enabled=true;
            myFlipper.networkMode=false;
            otherFlipper = pTable.Find("PlayerInput").Find("PlayerRight").Find("ButtonParent").GetChild(0).GetChild(0).GetComponent<FlipperButton>();
            otherFlipper.enabled=true;
            otherFlipper.networkMode=true;

            latencyText=pTable.Find("Canvas").Find("Latency").GetComponent<Text>();
            leftText = pTable.Find("Canvas").Find("Left").GetComponent<Text>();
            ball = pTable.Find("Ball");      
            ballBody = ball.GetComponent<Rigidbody>();
        }
    }

    public void ReceiveMatchInvite(int port, bool side){
        //pop up button
        inviteCountdown = RevealInvite();
        StartCoroutine(inviteCountdown);
        roomPort=port;
        this.left=side;
    }

    private IEnumerator RevealInvite(){
        inviteCG.gameObject.SetActive(true);
        inviteCG.alpha=1;
        
        //count down invite timer
        float timer=20;
        while(timer>0){
            timer -= Time.deltaTime;
            timerText.text=Mathf.CeilToInt(timer).ToString();
            yield return null;
        }
        inviteCG.alpha=0;
        inviteCG.gameObject.SetActive(false);
        //client.JoinQueue();
    }

    public void AcceptInvite(){
        StopCoroutine(inviteCountdown);
        inviteCG.alpha=0;
        inviteCG.gameObject.SetActive(false);
        StartCoroutine(client.Accepted(roomPort, left));

        //move camera
        pTable = tableParent.Find(roomPort.ToString());
        Transform target = pTable.Find("TableCam");
        Camera.main.transform.GetComponent<Camera>().enabled=false;
        target.GetComponent<Camera>().enabled=true;
            

        //hook up "left" flipper
        myFlipper = left ? pTable.Find("PlayerInput").Find("PlayerLeft").Find("ButtonParent").GetChild(0).GetChild(0).GetComponent<FlipperButton>() :
                    pTable.Find("PlayerInput").Find("PlayerRight").Find("ButtonParent").GetChild(0).GetChild(0).GetComponent<FlipperButton>();
        myFlipper.enabled=true;
        
        //get reference to other flipper
        otherFlipper = left ? pTable.Find("PlayerInput").Find("PlayerRight").Find("ButtonParent").GetChild(0).GetChild(0).GetComponent<FlipperButton>() :
                    pTable.Find("PlayerInput").Find("PlayerLeft").Find("ButtonParent").GetChild(0).GetChild(0).GetComponent<FlipperButton>();
        otherFlipper.enabled=true;
        otherFlipper.networkMode=true;

        //get reference to ball
        ball = pTable.Find("Ball"); 

        latencyText=pTable.Find("Canvas").Find("Latency").GetComponent<Text>();

        leftText = pTable.Find("Canvas").Find("Left").GetComponent<Text>();
        leftText.text = "Left: "+left;
    }

    //a signal sent upon accepting invite to ensure flippers have been activated and deactivated and are in their resting state
    public IEnumerator SendTestFlips(){
        yield return new WaitForSeconds(1f);
        SendFlip(left,true);
        yield return new WaitForSeconds(0.25f);
        SendFlip(left,false);
    }

    public void Flip(bool inLeft, bool inActive){
        if(!testing){
            if(inLeft==this.left){
                myFlipper.Flip(inActive);
                if(testing)
                    otherFlipper.Flip(inActive);
            }
            else{
                otherFlipper.Flip(inActive);
            }
        }
        else{
            myFlipper.Flip(inActive);
            otherFlipper.Flip(inActive);
        }        
    }

    public void UpdateLatency(){

    }

    public void SendFlip(bool left, bool active){
        client.Log("Sending flip, l: "+left+", a: "+active);
        client.ActivateFlipper(left,active); 
    }

    public void UpdateBall(float x, float y, float z, float x1, float y1, float z1){
        ball.position = new Vector3(x,y,z);
        ballBody.angularVelocity=new Vector3(x1,y1,z1);
        //ball.position = Vector3.Lerp(ball.position,new Vector3(x,y,z),0.1f);
    }
    public void FullSyncBall(float x, float y, float z,
                            float x2, float y2, float z2,
                            float x3, float y3, float z3){

        ball.position = new Vector3(x,y,z);
        //ball.GetComponent<Rigidbody>().angularVelocity=new Vector3(x2,y2,z2);
        //ball.GetComponent<Rigidbody>().velocity=new Vector3(x3,y3,z3);
    }
}
