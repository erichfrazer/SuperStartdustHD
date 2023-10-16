using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShipScript : OrbitThing, InputActions.IGameplayActions
{
    enum Method
    {
        Method1,
        Method2
    } ;

    float m_fXAxis;
    float m_fYAxis;
    float m_fForwardAccel;
    float m_fForwardVel;
    float m_fTurnAccel;
    float m_fTurnVel;
    float m_fDrag = 0.5f;
    float m_fDragDiv = 2.0f;
    float m_fMaxForwardVel = 5.0f;

    public AudioClip m_pLongLongThrustSound;
    public AudioClip m_pLongThrustSound;
    public AudioClip m_pThrustSound;
    public GameObject m_pSolarSystem;
    public GameObject m_pPlanet;
    public GameObject m_pBulletPrefab;
    public Camera m_pCamera;
    public GameObject m_pLight;
    public GameObject m_pGeoSyncCamParent;

    float m_fLastTimeThrustPlayed;

    static ShipScript m_sInstance;

    static public ShipScript Singleton
    {
        get
        {
            return m_sInstance;
        }
    }

    public ParticleSystem m_pEngineParticles;
    InputActions inputActions;

    private void Awake( )
    {
        m_sInstance = this;
    }

    // Use this for initialization
    void Start ()
    {
        Vector3 shipPos = transform.position;
        Camera.main.transform.position = shipPos.normalized * 10;
            
        m_bStayTangential = true;
        m_bAbsoluteDistance = true;

        if( inputActions == null )
        {
            inputActions = new InputActions();
            inputActions.gameplay.SetCallbacks(this);
            inputActions.gameplay.Enable();
        }
    }

    // Update is called once per frame
    void Update ()
    {
        float fNow = Time.time;

        bool bActivelyTurning = false;
        float f = m_fXAxis;
        if (f != 0)
        {
            m_fTurnAccel = f;
            bActivelyTurning = true;
        }
        else
        {
            if (Math.Abs(m_fTurnVel) > 0)
            {
                m_fTurnAccel /= m_fDragDiv;
            }
        }

        float fy = m_fYAxis;
        if (fy > 0 )
        {
            m_fForwardAccel = fy / 3.0f;
        }
        else
        {
            if( m_fForwardVel > 0 )
            {
                m_fForwardAccel = -m_fDrag;
            }
        }

        if (m_fForwardVel < 0)
        {
            m_fForwardVel = 0;
            m_fForwardAccel = 0;
        }

        if (!bActivelyTurning)
        {
            if (m_fTurnVel < 1)
            {
                m_fTurnVel = 0;
                m_fTurnAccel = 0;
            }
        }

        m_fTurnVel += m_fTurnAccel * Time.deltaTime;
        m_fForwardVel += m_fForwardAccel * Time.deltaTime;
        if (m_fForwardVel > m_fMaxForwardVel)
        {
            m_fForwardVel = m_fMaxForwardVel;
        }
        transform.position += transform.forward * m_fForwardVel * Time.deltaTime;

        // make sure this isn't subject to gimbal lock. How to rotate the ship around it's Y axis no matter what
        // its local rotation is.
        transform.localRotation *= Quaternion.Euler(0, m_fTurnVel * 1000.0f * Time.deltaTime, 0);

        if ( m_fForwardVel > 0 )
        {
            ParticleSystem.MainModule p = m_pEngineParticles.main;
            p.loop = false;
            m_pEngineParticles.Emit(1);

            if(m_fForwardVel > 150 )
            {
                if( fNow - m_fLastTimeThrustPlayed > 5 )
                {
                    m_fLastTimeThrustPlayed = fNow;
                    AudioSource.PlayClipAtPoint( m_pLongLongThrustSound, transform.position, 0.5f );
                }
            }
            else if(m_fForwardVel > 50 )
            {
                if ( fNow - m_fLastTimeThrustPlayed > 2 )
                {
                    m_fLastTimeThrustPlayed = fNow;
                    AudioSource.PlayClipAtPoint( m_pLongThrustSound, transform.position, 0.5f );
                }
            }
            else
            {
                if ( fNow - m_fLastTimeThrustPlayed > 1 )
                {
                    m_fLastTimeThrustPlayed = fNow;
                    AudioSource.PlayClipAtPoint( m_pThrustSound, transform.position, 0.5f );
                }
            }
        }
        else
        {
            ParticleSystem.MainModule p = m_pEngineParticles.main;
            p.loop = false;
        }

        SpinWorld();

        Quaternion parentLocalR = transform.parent.transform.localRotation;
        Quaternion localR = transform.localRotation;
        Quaternion finalR = transform.rotation;
        Quaternion finalR_calc = parentLocalR * localR;
        Matrix4x4 parentLocalM = transform.parent.localToWorldMatrix;
        Matrix4x4 thisWorldM = transform.localToWorldMatrix;
        Vector3 vTest = new Vector3(1, 0.5f, 0.2f);
        Vector3 vRotatedByQ = parentLocalR * vTest;
        Vector3 vRotatedByM = parentLocalM * vTest;

        int Stop = 1;
    }

    void SpinWorld()
    {
        // m_pPlanet is at the center of everything.
        // 'this' is the ship, and is child to the planet, but it's position pShipPos, is given in world space.
        // pCamPos is also in world space, but is not child of the planet.
        //
        // we should be able to cast two rays from the center of the planet, one to the ship,
        // and one to the camera, and calculate the rotation to bring the ray that goes to the ship,
        // onto the same ray that goes to the camera.
        //
        // Since camera is guaranteed at negative Z only compared to center of planet, there should be
        // NO component of z axis spin on the calculated rotation that brings the ship into alignment.
        // It should be x and y axis only

        Vector3 pShipPos = transform.position;
        Vector3 pCamPos = Camera.main.transform.position;
        Transform pWorldTransform = TheSystemScript.Singleton.transform;
        Vector3 pWorldPos = pWorldTransform.position;

        // ray 1
        Vector3 cam2Ship = pShipPos - pCamPos;
        // ray 2
        Vector3 cam2Planet = pWorldPos - pCamPos;
        // what's the angle between them
        float angle = Vector3.Angle(cam2Ship, cam2Planet);

        if (angle > 5.0f)
        {
            Quaternion q = Quaternion.FromToRotation(cam2Ship, cam2Planet);
            // rotate a little more to where we should be pointed, each time. The order of the multiplication for the 2nd arg
            // makes a difference. We want to take the original rotation, and stack the additional rotation on it AFTER the original
            // one. Quaternion multiplication order is right-to-left...
            m_pGeoSyncCamParent.transform.rotation = Quaternion.RotateTowards(
                m_pGeoSyncCamParent.transform.rotation, 
                q * m_pGeoSyncCamParent.transform.rotation, 10.0f * Time.deltaTime);

            Transform pCamTrans = Camera.main.transform;
            Vector3 spunVectorForwards = pCamTrans.parent.position - pCamTrans.position;
            pCamTrans.forward = spunVectorForwards;

#if false
            Quaternion q = Quaternion.FromToRotation(planet2Ship, planet2Camera);
            // rotate a little more to where we should be pointed, each time. The order of the multiplication for the 2nd arg
            // makes a difference. We want to take the original rotation, and stack the additional rotation on it AFTER the original
            // one. Quaternion multiplication order is right-to-left...
            Quaternion r = Quaternion.RotateTowards(pWorldTransform.rotation, q * pWorldTransform.rotation, 10.0f * Time.deltaTime);
            pWorldTransform.rotation = r;
#endif
        }
    }

    void ShootBullet( )
    {
        GameObject pNewBullet = Instantiate(
            m_pBulletPrefab,
            transform.position, // world space
            transform.rotation, // world space
            transform.parent );

        BulletScript p = pNewBullet.GetComponent<BulletScript>();
        p.m_fMaxDistance = 180;
        Rigidbody rb = pNewBullet.GetComponent<Rigidbody>( );
        rb.AddForce( pNewBullet.transform.forward * 500.0f, ForceMode.Force );
    }

    public void OnMoveVector2(InputAction.CallbackContext context)
    {
        Vector2 joyAxis = context.ReadValue<Vector2>();
        m_fXAxis = joyAxis.x;
        m_fYAxis = joyAxis.y;
    }

    float m_fLastTimeShot;

    public void OnFire(InputAction.CallbackContext context)
    {
        float fNow = Time.time;
        float fDelta = fNow - m_fLastTimeShot;
        if( fDelta < 0.1f )
        {
            return;
        }
        m_fLastTimeShot = fNow;
        ShootBullet();
    }
}
