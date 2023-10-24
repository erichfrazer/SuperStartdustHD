using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

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
    double m_dJoystickPointDegrees;


    bool m_bFireDown;
    float m_fFireFrequency = 0.05f;

    public AudioClip m_pLongLongThrustSound;
    public AudioClip m_pLongThrustSound;
    public AudioClip m_pThrustSound;
    public AudioClip m_pFastLaserSound;
    public GameObject m_pBullet3;
    public GameObject m_pLight;
    public GameObject m_pGeoSyncCamParent;

    float m_fLastTimeThrustPlayed;
    AudioSource m_pAudioSource;

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
        m_pAudioSource = GetComponent<AudioSource>();
        m_pFastLaserSound.LoadAudioData();

        m_bStayTangential = true;
        m_bAbsoluteDistance = true;

        if( inputActions == null )
        {
            inputActions = new InputActions();
            inputActions.gameplay.SetCallbacks(this);
            inputActions.gameplay.Enable();
        }
    }

    void FixedUpdate()
    {
        CheckFireBullet();
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
                m_fTurnVel /= m_fDragDiv;
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

        m_fTurnVel += 2 * m_fTurnAccel * Time.deltaTime;
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
            if (m_pEngineParticles != null)
            {
                ParticleSystem.MainModule p = m_pEngineParticles.main;
                p.loop = false;
                m_pEngineParticles.Emit(1);
            }

            if (m_fForwardVel > 150 )
            {
                if( fNow - m_fLastTimeThrustPlayed > 5 )
                {
                    m_fLastTimeThrustPlayed = fNow;
                    if (m_pLongLongThrustSound != null)
                    {
                        AudioSource.PlayClipAtPoint(m_pLongLongThrustSound, transform.position, 0.5f);
                    }
                }
            }
            else if(m_fForwardVel > 50 )
            {
                if ( fNow - m_fLastTimeThrustPlayed > 2 )
                {
                    m_fLastTimeThrustPlayed = fNow;
                    if (m_pLongThrustSound != null)
                    {
                        AudioSource.PlayClipAtPoint(m_pLongThrustSound, transform.position, 0.5f);
                    }
                }
            }
            else
            {
                if ( fNow - m_fLastTimeThrustPlayed > 1 )
                {
                    m_fLastTimeThrustPlayed = fNow;
                    if (m_pThrustSound != null)
                    {
                        AudioSource.PlayClipAtPoint(m_pThrustSound, transform.position, 0.5f);
                    }
                }
            }
        }
        else
        {
            if (m_pEngineParticles != null)
            {
                ParticleSystem.MainModule p = m_pEngineParticles.main;
                p.loop = false;
            }
        }

        SpinWorld();
    }

    void SpinWorld()
    {
        // We rotate the ship around the planet, and rotate the camera to keep the ship in view.
        // The planet does not rotate.

        if( TheSystemScript.Singleton == null )
        {
            return;
        }

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
         }
    }

    Transform m_pLastBullet3 = null;

    void ShootBullet3()
    {
        // put the new bullet right where the ship is
        GameObject pNewBullet = Instantiate(
            m_pBullet3,
            transform.position, // world space
            transform.rotation, // world space
            null);

        BulletScript bs = pNewBullet.GetComponent<BulletScript>();

        // the axis of rotation for the bullet is the ship's 'right' axis
        Rigidbody rb = pNewBullet.GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * 300, ForceMode.Force);

        // connect new bullet to us
        SpringJoint sjNew = pNewBullet.AddComponent<SpringJoint>();
        sjNew.connectedBody = GetComponent<Rigidbody>();
        bs.m_pPriorBullet = m_pLastBullet3;
        bs.m_pNextBullet = transform;

        // link prior fired one to current one. set up the 'next'. Prior is earlier-fired.
        if (m_pLastBullet3 != null)
        {
            SpringJoint sj = m_pLastBullet3.GetComponent<SpringJoint>();
            sj.connectedBody = pNewBullet.GetComponent<Rigidbody>(); // used to point to the ship, now it's the new bullet

            BulletScript bsNext = m_pLastBullet3.GetComponent<BulletScript>();
            bsNext.m_pNextBullet = bs.transform; // used to point to the ship, now it's the new bullet
        }
        m_pLastBullet3 = pNewBullet.transform;

        m_pAudioSource.clip = m_pFastLaserSound;
        m_pAudioSource.Play();
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
        m_bFireDown = context.ReadValueAsButton();
        if( !m_bFireDown )
        {
            // unset the last bullet's spring so it can fly free
            BulletScript bs = m_pLastBullet3.GetComponent<BulletScript>();
            Destroy(bs.GetComponent<SpringJoint>());

            m_pLastBullet3 = null;
        }

        Debug.Log("FireDown=" + m_bFireDown + ", " + context.ReadValueAsButton());
    }

    public void CheckFireBullet()
    {
        if (!m_bFireDown)
        {
            return;
        }

        float fNow = Time.time;
        float fDelta = fNow - m_fLastTimeShot;
        if (fDelta < m_fFireFrequency)
        {
            return;
        }
        m_fLastTimeShot = fNow;
        ShootBullet3();
    }

    public void OnMoveJoystick(InputAction.CallbackContext context)
    {
        m_dJoystickPointDegrees = 0;
        Vector2 v2 = context.ReadValue<Vector2>();
        m_dJoystickPointDegrees = Math.Atan2(v2.y, v2.x);
    }
    
}
