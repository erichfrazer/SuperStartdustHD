using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : OrbitThing
{
    public float m_fMaxDistance;
    public bool m_bDetectedHit;

    Vector3 m_pLastPos;
    float m_fTravelDist = 0;
    Transform m_pActualBulletT;

    void Start ()
    {
        base.m_bStayTangential = true;

        m_pLastPos = transform.position;
        m_fTravelDist = 0;
        m_pActualBulletT = transform.Find("bullet");
    }

    void Update ()
    {
        
        Vector3 pos = m_pActualBulletT.position;
        Vector3 delta = pos - m_pLastPos;
        m_pLastPos = pos;
        float distance = delta.magnitude;
        m_fTravelDist += distance;
        if (m_fTravelDist > 6 * Mathf.PI)
        {
             DestroyMe();
        }
    }

    void DestroyMe()
    {
        Destroy(gameObject);
    }
}
