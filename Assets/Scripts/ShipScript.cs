using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using Unity.Android.Types;
using Unity.Burst.CompilerServices;
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

    float m_fJoystickPointDegrees;


    bool m_bFireDown;
    bool m_bThrustDown;
    float m_fFireFrequency = 0.05f;

    internal WeaponType m_nMainWeaponType;
    List<bool> m_bHaveWeapon = new List<bool>((int) WeaponType.Count);
    List<int> m_nWeaponPower = new List<int>((int)WeaponType.Count);

    Dictionary<WeaponType, float> m_WeaponSpeedMultiplier = new Dictionary<WeaponType, float>();

    public AudioClip m_pLongLongThrustSound;
    public AudioClip m_pLongThrustSound;
    public AudioClip m_pThrustSound;
    public AudioClip m_pFastLaserSound;
    public AudioClip m_pChangeWeaponSound;

    public GameObject m_pGoldMelter_Prefab;
    public GameObject m_pRockBuster_Prefab;
    public GameObject m_pIceBlaster_Prefab;
    public GameObject m_pLight;

    float m_fLastTimeThrustPlayed;
    AudioSource m_pAudioSource;
    float m_fJoystickXAxis;

    Rigidbody m_pRB;

    internal static ShipScript m_sInstance;

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

#if false
        FixedJoint fj = gameObject.AddComponent<FixedJoint>();
        fj.connectedBody = transform.parent.GetComponent<Rigidbody>();
        fj.autoConfigureConnectedAnchor = true;
        fj.connectedAnchor = Vector3.zero;
        fj.anchor = -transform.position; // opposite of ship's position, to get to 0,0,0
        fj.axis = transform.up;
#endif

        // m_bStayTangential = true;
        // m_bInOrbit = true;

        if ( inputActions == null )
        {
            inputActions = new InputActions();
            inputActions.gameplay.SetCallbacks(this);
            inputActions.gameplay.Enable();
        }

        for( int i = 0; i < m_bHaveWeapon.Count; i++ )
        {
            m_bHaveWeapon[i] = true;
            m_nWeaponPower[i] = 10;
        }

        m_WeaponSpeedMultiplier.Add(WeaponType.GoldMelter, 1.0f);
        m_WeaponSpeedMultiplier.Add(WeaponType.Rock, 2.0f);
        m_WeaponSpeedMultiplier.Add(WeaponType.Ice, 3.0f);

        m_pRB = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update ()
    {
        float fNow = Time.time;

        Rigidbody rb = GetComponent<Rigidbody>();

        if (m_bThrustDown)
        {
            rb.AddForce(transform.forward * 10, ForceMode.Acceleration);
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

        m_fJoystickPointDegrees = 45.0f;
        Quaternion qCam = Camera.main.transform.rotation;
        Quaternion qCamSpunAroundY = qCam * Quaternion.Euler(0, m_fJoystickPointDegrees - 90.0f, 0);
        // this.transform.rotation = Quaternion.Slerp(this.transform.rotation, this.transform.rotation * Quaternion.Euler(0, 5, 0), Time.deltaTime);
        m_pRB.AddRelativeTorque(Vector2.up * 250 * m_fJoystickXAxis, ForceMode.Force);

        // the fixed joint just doesn't work. ship lifts off the planet for unknown reasons...
        m_pRB.position = m_pRB.position * 4 / m_pRB.position.magnitude;
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

    Transform m_pLastBullet = null;

    void ShootBullet()
    {
        if (m_nMainWeaponType == WeaponType.GoldMelter)
        {
            m_pLastBullet = BulletScript.CreateNewBullet(m_pGoldMelter_Prefab, m_pLastBullet, transform);
        }
        if (m_nMainWeaponType == WeaponType.Rock)
        {
            m_pLastBullet = BulletScript.CreateNewBullet(m_pRockBuster_Prefab, m_pLastBullet, transform);
        }

        if (m_nMainWeaponType == WeaponType.Ice)
        {
            m_pLastBullet = BulletScript.CreateNewBullet(m_pIceBlaster_Prefab, m_pLastBullet, transform);
        }


        Rigidbody pBulletRB = m_pLastBullet.GetComponent<Rigidbody>();
        float fSpeedMultiplier = m_WeaponSpeedMultiplier[m_nMainWeaponType];
        pBulletRB.AddForce(m_pLastBullet.forward * 300 * fSpeedMultiplier, ForceMode.Force);

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
            if (m_pLastBullet != null)
            {
                // unset the last bullet's spring so it can fly free
                ConfigurableJoint cj = m_pLastBullet.GetComponent<ConfigurableJoint>();
                if ( cj != null )
                {
                    Destroy(cj);
                }
                m_pLastBullet = null;
            }
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
        ShootBullet();
    }

    public void OnMoveJoystick(InputAction.CallbackContext context)
    {
        m_fJoystickPointDegrees = 0;
        Vector2 v2 = context.ReadValue<Vector2>();
        m_fJoystickPointDegrees = MathF.Atan2(v2.y, v2.x) * 180 / MathF.PI;
        Debug.Log("Degrees = " + m_fJoystickPointDegrees);
        m_fJoystickXAxis = v2.x;
    }

    public void OnThrustButton(InputAction.CallbackContext context)
    {
        m_bThrustDown = context.ReadValueAsButton();
    }

    public void OnSwitchWeapons(InputAction.CallbackContext context)
    {
    }

    bool m_bLeftShoulderDown;
    bool m_bRightShoulderDown;

    public void OnLeftShoulder(InputAction.CallbackContext context)
    {
        bool down = context.ReadValueAsButton();
        if (!down && m_bLeftShoulderDown)
        {
            // tap
            PreviousWeapon();
        }
        m_bLeftShoulderDown = down;
    }

    public void OnRightShoulder(InputAction.CallbackContext context)
    {
        bool down = context.ReadValueAsButton();
        if (!down && m_bRightShoulderDown)
        {
            // tap
            NextWeapon();
        }
        m_bRightShoulderDown = down;
    }

    void PreviousWeapon()
    {
        m_pAudioSource.clip = m_pChangeWeaponSound;
        m_pAudioSource.Play();
        m_nMainWeaponType--;
        if( m_nMainWeaponType < 0 )
        {
            m_nMainWeaponType = WeaponType.Count - 1;
        }
    }

    void NextWeapon()
    {
        m_pAudioSource.clip = m_pChangeWeaponSound;
        m_pAudioSource.Play();
        m_nMainWeaponType++;
        if (m_nMainWeaponType >= WeaponType.Count )
        {
            m_nMainWeaponType = 0;
        }
    }

}
