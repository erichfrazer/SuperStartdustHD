using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : OrbitThing
{
    public float m_fMaxDistance;
    public bool m_bDetectedHit;

    // Use this for initialization
    void Start ()
    {
        m_bStayTangential = true;
        m_bAbsoluteDistance = true;
    }
    
    // Update is called once per frame
    void Update ()
    {
    }
}
