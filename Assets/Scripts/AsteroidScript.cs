using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidScript : OrbitThing
{
    float m_fStartTime;
    float m_fAliveTime;
    float m_fFadeInTime = 2;

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
        if ( name.StartsWith( "Ateroid_White1") )
        {
            GameObject pNewMedAsteroid = Instantiate(
                GameControllerScript.Singleton.m_pAsteroidType1_Med,
                transform.position,
                UnityEngine.Random.rotation,
                transform.parent.transform
            );

            AsteroidScript pScript = pNewMedAsteroid.AddComponent<AsteroidScript>();

            GameObject pNewMedAsteroid2 = Instantiate(
                GameControllerScript.Singleton.m_pAsteroidType1_Med,
                transform.position,
                UnityEngine.Random.rotation,
                transform.parent.transform
            );

            AsteroidScript pScript2 = pNewMedAsteroid2.AddComponent<AsteroidScript>();
        }

        if (name.StartsWith("Ateroid_White2"))
        {
            GameObject pNewAsteroid = Instantiate(
                GameControllerScript.Singleton.m_pAsteroidType1_Small, // original
                transform.position,
                UnityEngine.Random.rotation,
                transform.parent.transform
                );

            AsteroidScript pScript = pNewAsteroid.AddComponent<AsteroidScript>();

            GameObject pNewAsteroid2 = Instantiate(
                GameControllerScript.Singleton.m_pAsteroidType1_Small,
                transform.position,
                UnityEngine.Random.rotation,
                transform.parent.transform
                );

            AsteroidScript pScript2 = pNewAsteroid2.AddComponent<AsteroidScript>();
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
        pAudio.Play();

        Destroy(this.gameObject);
        Destroy(collision.gameObject);
    }
}
