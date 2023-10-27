using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : OrbitThing
{
    public float m_fMaxDistance;
    public bool m_bDetectedHit;
    public bool m_bRotateSmoothly;

    Vector3 m_pLastPos;
    float m_fTravelDist = 0;
    public Transform m_pPriorBullet;
    public Transform m_pNextBullet;

    void Start ()
    {
        m_bInOrbit = true;
        m_bRotateSmoothly = true;

        m_pLastPos = transform.position;
        m_fTravelDist = 0;
        // m_pActualBulletT = transform.Find("bullet");
    }

    void _RotateSmoothly()
    {
        if (m_pPriorBullet != null && m_pNextBullet != null)
        {
            Vector3 spunVectorUp = transform.position.normalized;
#if false
            spunVectorUp.Normalize();
            Vector3 spunVectorForward_Wrong = transform.forward;
            Vector3 spunVectorRight = Vector3.Cross(spunVectorUp, spunVectorForward_Wrong);
            Vector3 spunVectorForward = Vector3.Cross(spunVectorRight, spunVectorUp);
            Quaternion q = Quaternion.LookRotation(spunVectorForward, spunVectorUp);
            m_pRB.MoveRotation(q);
#endif

            Quaternion q = Quaternion.LookRotation(m_pNextBullet.transform.position - m_pPriorBullet.transform.position, spunVectorUp);
            m_pRB.MoveRotation(q);
        }
    }

    void Update ()
    {
        if (m_bRotateSmoothly)
        {
            _RotateSmoothly();
        }

        Vector3 pos = transform.position;
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
