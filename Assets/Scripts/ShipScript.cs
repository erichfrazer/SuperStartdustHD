using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class ShipScript : MonoBehaviour, InputActions.IGameplayActions
{
    enum Method
    {
        Method1,
        Method2
    } ;

    double m_dJoystickPointDegrees;


    bool m_bFireDown;
    bool m_bThrustDown;
    float m_fFireFrequency = 0.05f;

    public AudioClip m_pLongLongThrustSound;
    public AudioClip m_pLongThrustSound;
    public AudioClip m_pThrustSound;
    public AudioClip m_pFastLaserSound;
    public GameObject m_pBullet3;
    public GameObject m_pLight;

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
        // base.Awake();

        m_sInstance = this;
    }

    // Use this for initialization
    void Start ()
    {
        // base.Start();

        Vector3 shipPos = transform.position;
        Camera.main.transform.position = shipPos.normalized * 10;
        m_pAudioSource = GetComponent<AudioSource>();
        m_pFastLaserSound.LoadAudioData();

        // m_bStayTangential = true;
        // m_bInOrbit = true;

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

        Rigidbody rb = GetComponent<Rigidbody>();

        if (m_bThrustDown)
        {
            rb.AddForce(transform.forward * 3, ForceMode.Acceleration);
        }

        Vector3 velocity = rb.velocity;

        if ( velocity.magnitude > 0.1f )
        {

            if (m_pEngineParticles != null)
            {
                ParticleSystem.MainModule p = m_pEngineParticles.main;
                p.loop = false;
                m_pEngineParticles.Emit(1);
            }

            if (velocity.magnitude > 150 )
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
            else if(velocity.magnitude > 50 )
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

        CheckFireBullet();

        Vector3 camUp = Camera.main.transform.up;
        Vector3 shipForward = transform.forward;
        float fAngle = Vector3.Angle(camUp , shipForward);
        Debug.Log("fangle = " + fAngle);

        Quaternion q = Quaternion.AngleAxis((float)m_dJoystickPointDegrees, Camera.main.transform.position);
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, q, Time.deltaTime);
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
        Vector3 pWorldPos = Vector3.zero;

        // ray 1
        Vector3 planet2Ship = pShipPos - pWorldPos;
        // ray 2
        Vector3 planet2Cam = pCamPos - pWorldPos;
        Vector3 cam2Ship = pShipPos - pCamPos;

        // what's the angle between them
        float angle = Vector3.Angle(planet2Ship, planet2Cam);

        if (angle > 5.0f)
        {
            float fMoveFraction = (angle - 5.0f) / 5.0f;
            // move camera closer to ship
            Vector3 newCamPos = pCamPos + fMoveFraction * cam2Ship;
            newCamPos = newCamPos.normalized * 10.0f;
            Camera.main.transform.position = newCamPos;

            Vector3 spunVectorForward = -Camera.main.transform.position;
            spunVectorForward.Normalize();
            Vector3 spunVectorUp_Wrong = Camera.main.transform.up;
            Vector3 spunVectorRight = Vector3.Cross(spunVectorForward, spunVectorUp_Wrong);
            Vector3 spunVectorUp = Vector3.Cross(spunVectorRight, spunVectorForward);
            Quaternion q = Quaternion.LookRotation(spunVectorForward, spunVectorUp);
            Camera.main.transform.rotation = q;

#if false
            Quaternion q = Quaternion.FromToRotation(cam2Ship, cam2Planet);

            // rotations stack up like this: q' = world_spin * q, or q' = q * local_spin
            Camera.main.transform.rotation = Quaternion.RotateTowards(
                Camera.main.transform.rotation,
                q * Camera.main.transform.rotation,
                10.0f * Time.deltaTime);

            // rotate a little more to where we should be pointed, each time.
            // The order of the multiplication for the 2nd arg makes a difference.
            // We want to take the original rotation, and stack the additional rotation on it AFTER the original
            // one. Quaternion multiplication order is right-to-left...
            m_pGeoSyncCamParent.transform.rotation = Quaternion.RotateTowards(
                m_pGeoSyncCamParent.transform.rotation, 
                q * m_pGeoSyncCamParent.transform.rotation, 10.0f * Time.deltaTime);

            Transform pCamTrans = Camera.main.transform;
            Vector3 spunVectorForwards = pCamTrans.parent.position - pCamTrans.position;
            pCamTrans.forward = spunVectorForwards;
#endif
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
        m_dJoystickPointDegrees = Math.Atan2(v2.y, v2.x) * 180 / Math.PI;
        Debug.Log("Degrees = " + m_dJoystickPointDegrees);
    }

    public void OnThrustButton(InputAction.CallbackContext context)
    {
        m_bThrustDown = context.ReadValueAsButton();
    }
}
