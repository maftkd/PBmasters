using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipperDampener : MonoBehaviour
{
    public Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision other){
        rb.angularVelocity = Vector3.zero;
    }
}
