using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitThing : MonoBehaviour
{
    public event EventHandler ReachedOrbit;

    public bool m_bInOrbit;
    public bool m_bStayTangential;
    public float m_fSpringK = 100.0f;
    public float m_fSpringDrag = 0.05f;

    float OrbitRadius = 4.0f;
    protected Rigidbody m_pRB;

    public bool InOrbit
    {
        get
        {
            return m_bInOrbit;
        }
    }

    internal void Start()
    {
        m_pRB = transform.GetComponent<Rigidbody>();
    }

    internal void Awake()
    {
        m_pRB = transform.GetComponent<Rigidbody>();
    }

    void ForceAdjustOrbit()
    {
#if false
        // spring equation
        // Fs = -k * fDeltaOrbitDist

        Vector3 vPriorForce = m_pRB.GetAccumulatedForce();
        // we want the only force in the planet's direction to be a corrective force.
        float fPriorForceTowardsPlanet = Vector3.Dot(transform.position, vPriorForce);
        float fDistToPlanet = transform.position.magnitude;
        float fDeltaOrbitDist = fDistToPlanet - OrbitRadius; // > 0, object is too far out
        float fForceSpring = -m_fSpringK * fDeltaOrbitDist;
        float fForceToAdd = fForceSpring - fPriorForceTowardsPlanet;
       m_pRB.AddForce(transform.position.normalized * fForceToAdd, ForceMode.Force);

        // get the velocity in the planet direction and dampen it
        float fPriorVelTowardsPlanet = Vector3.Dot(transform.position, m_pRB.velocity);
        float fVelocityChange = -m_fSpringDrag * fPriorVelTowardsPlanet;
        m_pRB.AddForce(transform.position.normalized * fVelocityChange, ForceMode.VelocityChange);

        Debug.Log("prior force=" + fPriorForceTowardsPlanet + " Dist = " + fDeltaOrbitDist + ", Adding force " + transform.position.normalized * fForceToAdd);
#endif

    }

    //  Update is called once per frame
    internal void FixedUpdate()
    {
        // this routine does two things:
        // 1. It makes sure once it's in orbit, that it STAYS in orbit (by ensuring it's transform position is a fixed length from center of planet)
        // 2. It faces tangentially to its parent. This is tricky, but doable

        // this routine needs to take into account the object's parent is spinning and offset

        Vector3 newPos = m_pRB.position;
        Quaternion newRot = m_pRB.rotation;
        float fDistance = newPos.magnitude;

        if (false && m_bInOrbit)
        {
            // keep it in orbit, fixed distance. I'm assuming this doesn't change velocity
            newPos *= OrbitRadius / fDistance;
            Vector3 awayFromPlanetUnitVector = transform.position.normalized;
            Vector3 vVel = m_pRB.velocity;

            // get the velocity in the planet direction and cancel it
            float fPriorVelTowardsPlanet = Vector3.Dot(awayFromPlanetUnitVector, vVel);
            m_pRB.AddForce(-awayFromPlanetUnitVector * fPriorVelTowardsPlanet, ForceMode.VelocityChange);
            Vector3 vNewVel = vVel - awayFromPlanetUnitVector * fPriorVelTowardsPlanet;

            Debug.Log("vel mag = " + fPriorVelTowardsPlanet + ", vOrg = " + vVel + ", vNew = " + vNewVel);

            int stop = 1;
        }

        if (false && m_bStayTangential)
        {
            Vector3 spunVectorUp = transform.position;
            spunVectorUp.Normalize();
            Vector3 spunVectorForward_Wrong = transform.forward;
            Vector3 spunVectorRight = Vector3.Cross(spunVectorUp, spunVectorForward_Wrong);
            Vector3 spunVectorForward = Vector3.Cross(spunVectorRight, spunVectorUp);
            newRot = Quaternion.LookRotation(spunVectorForward, spunVectorUp);
            m_pRB.rotation = newRot;
        }

        // m_pRB.Move(newPos, newRot);

        return;

        if (m_bInOrbit)
        {
            ForceAdjustOrbit();
            return;
        }

        Vector3 pNewLocalPos = transform.localPosition;

        Rigidbody rb = GetComponent<Rigidbody>( );

        if (this is BlueBugScript)
        {
            int stop = 0;
        }
        if( this is BulletScript )
        {
            int stop = 0;
        }

        m_bInOrbit = true;

        if (m_bInOrbit)
        {
        }
        else
        {
            // Debug.Log( "distance = " + fDistance );

            // bring it back into orbit
            if (fDistance > OrbitRadius)
            {
                float fNewDistance = fDistance / 1.005f;
                pNewLocalPos *= fNewDistance / fDistance;
                transform.localPosition = pNewLocalPos;
                if (fNewDistance <= 4.0f)
                {
                    m_bInOrbit = true;
                    if (ReachedOrbit != null)
                    {
                        ReachedOrbit(this, null);

                        // Rigidbody rb = GetComponent<Rigidbody>( );
                        // rb.AddForce( 0, 100.0f, 0 );
                        // rb.AddRelativeForce( transform.forward, ForceMode.Force );
                    }
                }
            }
        }
    }
}


