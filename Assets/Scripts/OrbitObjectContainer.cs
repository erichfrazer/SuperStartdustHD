using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitThing : MonoBehaviour
{
    public event EventHandler ReachedOrbit;

    public bool m_bInOrbit;
    public bool m_bStayTangential;

    float OrbitRadius = 4.0f;
    protected Rigidbody m_pRB;

    public bool InOrbit
    {
        get
        {
            return m_bInOrbit;
        }
    }

    internal virtual void Start()
    {
        m_pRB = transform.GetComponent<Rigidbody>();
    }

    internal void Awake()
    {
        m_pRB = transform.GetComponent<Rigidbody>();
    }

    //  Update is called once per frame
    internal virtual void FixedUpdate()
    {
        // this routine does two things:
        // 1. It makes sure once it's in orbit, that it STAYS in orbit (by ensuring it's transform position is a fixed length from center of planet)
        // 2. It faces tangentially to its parent. This is tricky, but doable

        // this routine needs to take into account the object's parent is spinning and offset

        Vector3 newPos = m_pRB.position;
        Quaternion newRot = m_pRB.rotation;
        Vector3 vVel = m_pRB.velocity;
        float fDistance = newPos.magnitude;
        bool bAdjust = false;

        if (m_bInOrbit)
        {
            // keep it in orbit, fixed distance. I'm assuming this doesn't change velocity
            newPos *= OrbitRadius / fDistance;
            Vector3 awayFromPlanetUnitVector = m_pRB.position.normalized;

            // get the velocity in the planet direction and cancel it
            float fPriorVelTowardsPlanet = Vector3.Dot(awayFromPlanetUnitVector, vVel);
            m_pRB.AddForce(-awayFromPlanetUnitVector * fPriorVelTowardsPlanet, ForceMode.VelocityChange);
            Vector3 vNewVel = vVel - awayFromPlanetUnitVector * fPriorVelTowardsPlanet;

            bAdjust = true;
        }
        else
        {
            // bring it back into orbit
            if (fDistance > OrbitRadius)
            {
                float fMag1 = newPos.magnitude;
                float fMag2 = ( newPos + vVel * 0.01f ).magnitude;
                if (fMag2 > fMag1)
                {
                    m_pRB.velocity = Vector3.zero;
                }

                float fNewDistance = fDistance - 0.1f;
                newPos = newPos.normalized * fNewDistance;
                bAdjust = true;

                if (fNewDistance <= 4.0f)
                {
                    m_bInOrbit = true;
                    if (ReachedOrbit != null)
                    {
                        ReachedOrbit(this, null);
                    }
                }
            }
        }

        if (m_bStayTangential)
        {
            Vector3 spunVectorUp = transform.position;
            spunVectorUp.Normalize();
            Vector3 spunVectorForward_Wrong = transform.forward;
            Vector3 spunVectorRight = Vector3.Cross(spunVectorUp, spunVectorForward_Wrong);
            Vector3 spunVectorForward = Vector3.Cross(spunVectorRight, spunVectorUp);
            newRot = Quaternion.LookRotation(spunVectorForward, spunVectorUp);
            bAdjust = true;
        }

        if (bAdjust)
        {
            m_pRB.Move(newPos, newRot);
        }
    }
}


