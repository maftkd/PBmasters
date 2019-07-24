using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Diagnostics;

public class CustomBuilder
{
    [MenuItem("MyTools/Build Server")]
    public static void BuildServer(){
        string path = EditorUtility.SaveFolderPanel("Choose Server Location...", "/home/michael/Apps/UnityServerTest", ".");
        string[] levels = new string[] {"Assets/Scenes/SampleScene.unity"};

        //enable ball physics
        GameObject [] balls = GameObject.FindGameObjectsWithTag("Ball");
        foreach(GameObject b in balls){
            //b.transform.GetComponent<Rigidbody>().isKinematic = false;
            //b.transform.GetComponent<SphereCollider>().enabled=true;
        }

        //hide client
        GameObject [] clientelle = GameObject.FindGameObjectsWithTag("Client");
        UnityEngine.Debug.Log(clientelle.Length);
        foreach(GameObject g in clientelle){
            g.SetActive(false);
        }
        GameObject server = GameObject.FindGameObjectWithTag("Server");
        if(server)
            server.SetActive(true);
        else{
            UnityEngine.Debug.LogError("Server gameObject must be active to build server executable");
            return;
        }

        //build player
        BuildPipeline.BuildPlayer(levels, path + "/Server", BuildTarget.StandaloneLinux64, BuildOptions.EnableHeadlessMode);

        Process proc = new Process();
        proc.StartInfo.FileName = path + "/Server";
        proc.Start();
    }

    [MenuItem("MyTools/Build Client")]
    public static void BuildClient(){
        string path = EditorUtility.SaveFolderPanel("Choose Client Location...", "/home/michael/Apps/AndroidBuilds/", ".");
        string[] levels = new string[] {"Assets/Scenes/SampleScene.unity"};

        //disable ball physics
        GameObject [] balls = GameObject.FindGameObjectsWithTag("Ball");
        foreach(GameObject b in balls){
            //b.transform.GetComponent<Rigidbody>().isKinematic = true;
            //b.transform.GetComponent<SphereCollider>().enabled=false;
        }

        //hide server
        GameObject [] clientelle = GameObject.FindGameObjectsWithTag("Client");
        UnityEngine.Debug.Log(clientelle.Length);
        foreach(GameObject g in clientelle){
            g.SetActive(true);
        }
        GameObject server = GameObject.FindGameObjectWithTag("Server");
        if(server)
            server.SetActive(false);

        //build player
        BuildPipeline.BuildPlayer(levels, path + "/PinCas.apk", BuildTarget.Android, BuildOptions.None);

        Process proc = new Process();
        proc.StartInfo.FileName = path + "/PinCas.apk";
        proc.Start();
    }
}
