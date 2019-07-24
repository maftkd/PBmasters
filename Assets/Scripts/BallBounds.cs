using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBounds : MonoBehaviour
{
    private Vector3 resetPos;
    public Transform ballSpawn;
    // Start is called before the first frame update
    void Start()
    {
        resetPos=ballSpawn.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerExit(Collider other){
        other.transform.position = resetPos;
    }
}
