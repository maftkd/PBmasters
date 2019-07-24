using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallDrain : MonoBehaviour
{
    private Vector3 resetPos;
    public Transform ballSpawn;
    // Start is called before the first frame update
    void Start()
    {
        resetPos=ballSpawn.position;
    }

    void OnTriggerEnter(Collider other){
        other.transform.position = resetPos;
    }
}
