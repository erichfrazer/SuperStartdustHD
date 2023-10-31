﻿using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidScript : OrbitThing
{
    float m_fStartTime;
    float m_fAliveTime;
    float m_fFadeInTime = 2;
    internal int m_nAsteroidSize; // starts at 8, 4, 2, 1
    internal WeaponType m_nAsteroidType;
    internal bool m_bBonus;

    // Use this for initialization
    override internal void Start ()
    {
        base.Start ();

        ReachedOrbit += M_pParentOrbit_ReachedOrbit;

        // since we attach the script at runtime, we can't trap this
        m_fStartTime = Time.time;
    }

    private void M_pParentOrbit_ReachedOrbit( object sender, System.EventArgs e )
    {
        Renderer r = GetComponent<Renderer>( );
        Color c = r.material.color;
        c.a = 1.0f;
        r.material.color = c;

        Rigidbody rb = GetComponent<Rigidbody>( );
        Vector2 pForce = UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range( 10.0f, 100.0f );
        rb.AddForce( pForce.x, 0, pForce.y );
    }

    // Update is called once per frame
    override internal void FixedUpdate ()
    {
        base.FixedUpdate();

        m_fAliveTime = Time.time - m_fStartTime;

        if( !InOrbit )
        {
            if ( m_fAliveTime > m_fFadeInTime )
            {
                Renderer r = GetComponent<Renderer>( );
                Color c = r.material.color;
                c.a = 1.0f;
                r.material.color = c;
            }
            else
            {
                Renderer r = GetComponent<Renderer>( );
                Color c = r.material.color;
                // needs to go from 0 to 1
                c.a = ( m_fAliveTime / m_fFadeInTime );
                r.material.color = c;
            }
        }
    }

    // only bullets will hit us, other asteroids won't hit us, they'll just bounce
    private void OnCollisionEnter(Collision collision)
    {
        GameObject pHit = collision.gameObject;
        if (pHit.layer == 6 )
        {
            BulletHitUs(collision);
        }
    }

    void BulletHitUs(Collision collision)
    {
        if (!m_bInOrbit)
        {
            m_bInOrbit = true;
        }

        AudioClip ac = GameControllerScript.Singleton.m_pAsteroidExplodeSound;

        if (m_nAsteroidType == WeaponType.GoldMelter)
        {
            if (m_nAsteroidSize == 8)
            {
                GameObject pAsteroid1 = Instantiate(
                    GameControllerScript.Singleton.m_pAsteroidType1_BigMed,
                    transform.position,
                    UnityEngine.Random.rotation,
                    transform.parent.transform
                );

                AsteroidScript pScript = pAsteroid1.AddComponent<AsteroidScript>();
                pScript.m_nAsteroidSize = 4;

                GameObject pAsteroid2 = Instantiate(
                    GameControllerScript.Singleton.m_pAsteroidType1_BigMed,
                    transform.position,
                    UnityEngine.Random.rotation,
                    transform.parent.transform
                );

                AsteroidScript pScript2 = pAsteroid2.AddComponent<AsteroidScript>();
                pScript2.m_nAsteroidSize = 4;
            }
            else if (m_nAsteroidSize == 4)
            {
                GameObject pAsteroid1 = Instantiate(
                    GameControllerScript.Singleton.m_pAsteroidType1_Med,
                    transform.position,
                    UnityEngine.Random.rotation,
                    transform.parent.transform
                    );

                AsteroidScript pScript = pAsteroid1.AddComponent<AsteroidScript>();
                pScript.m_nAsteroidSize = 2;

                GameObject pAsteroid2 = Instantiate(
                    GameControllerScript.Singleton.m_pAsteroidType1_Med,
                    transform.position,
                    UnityEngine.Random.rotation,
                    transform.parent.transform
                    );

                AsteroidScript pScript2 = pAsteroid2.AddComponent<AsteroidScript>();
                pScript2.m_nAsteroidSize = 2;
            }
            else if (m_nAsteroidSize == 2)
            {
                GameObject pAsteroid1 = Instantiate(
                    GameControllerScript.Singleton.m_pAsteroidType1_Small,
                    transform.position,
                    UnityEngine.Random.rotation,
                    transform.parent.transform
                    );

                AsteroidScript pScript = pAsteroid1.AddComponent<AsteroidScript>();
                pScript.m_nAsteroidSize = 1;

                int r = (int) UnityEngine.Random.Range(1, 10);
                if (r == 1)
                {
                    GameObject pAsteroid2 = Instantiate(
                        GameControllerScript.Singleton.m_pBonusAsteroid,
                        transform.position,
                        UnityEngine.Random.rotation,
                        transform.parent.transform
                        );

                    AsteroidScript pScript2 = pAsteroid2.AddComponent<AsteroidScript>();
                    pScript2.m_nAsteroidSize = 1;
                    pScript2.m_bBonus = true;

                    ac = GameControllerScript.Singleton.m_pBonusAsteroidExplodeSound;
                }
                else
                {
                    GameObject pAsteroid2 = Instantiate(
                        GameControllerScript.Singleton.m_pAsteroidType1_Small,
                        transform.position,
                        UnityEngine.Random.rotation,
                        transform.parent.transform
                        );

                    AsteroidScript pScript2 = pAsteroid2.AddComponent<AsteroidScript>();
                    pScript2.m_nAsteroidSize = 1;
                }
            }
            else
            {
                // just die
            }
        }

        GameObject pNewExplosion = Instantiate(
            GameControllerScript.Singleton.m_pAsteroidExplosion,
            transform.position,
            transform.rotation,
            transform.parent.transform 
        );

        ParticleSystem pExplosionParticleSystem = pNewExplosion.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule pMain = pExplosionParticleSystem.main;
        pMain.stopAction = ParticleSystemStopAction.Destroy;
        pExplosionParticleSystem.Play();

        AudioSource pAudio = GameControllerScript.Singleton.gameObject.GetComponent<AudioSource>();
        pAudio.clip = ac;
        pAudio.Play();

        Destroy(this.gameObject);
        Destroy(collision.gameObject);
    }
}
