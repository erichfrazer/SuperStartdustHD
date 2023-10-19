using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidScript : OrbitThing
{
    GameControllerScript m_pGameControllerScript;
    float m_fStartTime;
    float m_fAliveTime;
    float m_fFadeInTime = 2;

    // Use this for initialization
    void Start ()
    {
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
    void Update ()
    {
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

    private void OnEnable()
    {
        GameObject pGameController = GameObject.Find("GameController");
        m_pGameControllerScript = pGameController.GetComponent<GameControllerScript>();
    }

    // only bullets will hit us, other asteroids won't hit us, they'll just bounce
    private void OnCollisionEnter(Collision collision)
    {
        GameObject pHit = collision.gameObject;
        if (pHit.name.StartsWith( "bullet" ) )
        {
            BulletHitUs(collision);
        }
    }

    void BulletHitUs(Collision collision)
    {
        BulletScript pBulletScript = collision.gameObject.GetComponent<BulletScript>();
        if (pBulletScript == null) return;

        if( pBulletScript.m_bDetectedHit )
        {
            return;
        }

        pBulletScript.m_bDetectedHit = true;

        if ( name.StartsWith( "Ateroid_White1") )
        {
            GameObject pNewMedAsteroid = Instantiate(
                m_pGameControllerScript.m_pAsteroidType1_Med,
                transform.position,
                UnityEngine.Random.rotation,
                transform.parent.transform
            );

            AsteroidScript pScript = pNewMedAsteroid.AddComponent<AsteroidScript>();

            GameObject pNewMedAsteroid2 = Instantiate(
                m_pGameControllerScript.m_pAsteroidType1_Med,
                transform.position,
                UnityEngine.Random.rotation,
                transform.parent.transform
            );

            AsteroidScript pScript2 = pNewMedAsteroid2.AddComponent<AsteroidScript>();
        }

        if (name.StartsWith("Ateroid_White2"))
        {
            GameObject pNewAsteroid = Instantiate(
                m_pGameControllerScript.m_pAsteroidType1_Small, // original
                transform.position,
                UnityEngine.Random.rotation,
                transform.parent.transform
                );

            AsteroidScript pScript = pNewAsteroid.AddComponent<AsteroidScript>();

            GameObject pNewAsteroid2 = Instantiate(
                m_pGameControllerScript.m_pAsteroidType1_Small,
                transform.position,
                UnityEngine.Random.rotation,
                transform.parent.transform
                );

            AsteroidScript pScript2 = pNewAsteroid2.AddComponent<AsteroidScript>();
        }

        GameObject pNewExplosion = Instantiate(
            m_pGameControllerScript.m_pAsteroidExplosion,
            transform.position,
            transform.rotation,
            transform.parent.transform 
        );

        ParticleSystem pExplosionParticleSystem = pNewExplosion.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule pMain = pExplosionParticleSystem.main;
        pMain.stopAction = ParticleSystemStopAction.Destroy;
        pExplosionParticleSystem.Play();

        AudioSource pAudio = m_pGameControllerScript.gameObject.GetComponent<AudioSource>();
        pAudio.Play();

        Destroy(this.gameObject);
        Destroy(collision.gameObject);
    }

    void OnParticleSystemStopped( )
    {
        
    }
}
