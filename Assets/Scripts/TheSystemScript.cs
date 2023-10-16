using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheSystemScript : MonoBehaviour {

    public static TheSystemScript Singleton;

    // Use this for initialization
    void Awake ()
    {
        Singleton = this;
    }
    
    // Update is called once per frame
    void Update () {
        
    }
}
