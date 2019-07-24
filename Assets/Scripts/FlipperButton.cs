using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipperButton : MonoBehaviour
{
    private Animator anim,flipperAnim;
    private GameManager mGame;
    private bool active=false;
    public bool networkMode=false;
    public Transform flipperTransform;
    Rigidbody rb;
    public bool left;
    public float flipTorque;
    public float maxAngVal;
    public float flipperActivationTime;
    void Start()
    {
        
    }

    void OnEnable(){
        Console.WriteLine("{0} has been enabled:", flipperTransform.name);
        mGame = GameObject.FindGameObjectWithTag("GameController").transform.GetComponent<GameManager>();
        anim = transform.GetComponent<Animator>();
        if(left)
            flipperTransform=transform.parent.parent.parent.parent.GetChild(0).GetChild(0);
        else
            flipperTransform=transform.parent.parent.parent.parent.GetChild(1).GetChild(0);

        rb = flipperTransform.GetChild(0).GetComponent<Rigidbody>();
        
        rb.centerOfMass = Vector3.zero;
        if(!left)
            flipTorque*=-1;
        rb.maxAngularVelocity=maxAngVal;   
    }

    // Update is called once per frame
    void Update()
    {   if(!networkMode){
            if(Input.touchCount>0 || Input.GetKey(KeyCode.Space)){
                if(!active){
                    anim.SetBool("pressed", true);
                    //flipperAnim.SetBool("pressed", true);
                    active=true;
                    mGame.SendFlip(left,active);
                }
            }
            else{
                if(active){
                    anim.SetBool("pressed", false);
                    //flipperAnim.SetBool("pressed", false);
                    active=false;
                    mGame.SendFlip(left,active);
                }
            }
        }
    }

    private IEnumerator lastFlip;
    public void Flip(bool active){
        Console.WriteLine("Button press");
        if(anim!=null)
            anim.SetBool("pressed",active);
        //flipperAnim.SetBool("pressed", active);
        Console.WriteLine("Killing previous flip motion");
        if(lastFlip!=null){
            StopCoroutine(lastFlip);
        }
        Console.WriteLine("Prepping next coroutine");
        lastFlip = FlipFlipper(active);
        StartCoroutine(lastFlip);      
        Console.WriteLine("Flip routine started");
    }

    private IEnumerator FlipFlipper(bool up){
        float timer=0;
        print(flipperTransform.name);
        if(up){
            Console.WriteLine("flipper up");
            while(timer<flipperActivationTime){
                timer+=Time.deltaTime;
                if(rb!=null)
                    rb.AddRelativeTorque(Vector3.up*flipTorque);
                else
                    Console.WriteLine("Err: flipper rb not found");
                //flipperTransform.localEulerAngles = new Vector3(0,Mathf.Lerp(45f,-45f,timer*10),0);
                yield return null;
            }
        }
        else{
            Console.WriteLine("flipper down");
            while(timer<flipperActivationTime){
                if(rb!=null)
                    rb.AddRelativeTorque(Vector3.up*-1*flipTorque);
                else
                    Console.WriteLine("Err: flipper rb not found");
                timer+=Time.deltaTime;
                //flipperTransform.localEulerAngles = new Vector3(0,Mathf.Lerp(-45f,45f,timer*10), 0);
                yield return null;
            }
        }
    }
}
