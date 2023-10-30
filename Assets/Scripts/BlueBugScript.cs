using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueBugScript : MonoBehaviour
{
    public GameObject m_pShadowObject;
    public AudioClip m_pExplosionSound;
    public GameObject m_pTheShip;
    float m_fStartTime;
    float m_fAliveTime;
    float m_fFadeInTime = 2;
    public GameControllerScript m_pGameControllerScript;

    public static List<GameObject> BlueBugList = new List<GameObject>( );

    OrbitThing m_pOrbitThing;

    // Use this for initialization
    void Start ()
    {
        BoidBehaviour bb = GetComponent<BoidBehaviour>();
        m_pOrbitThing = bb as OrbitThing;

        m_pOrbitThing.m_bInOrbit = true;

        m_pGameControllerScript = GameControllerScript.Singleton;
        m_pTheShip = ShipScript.Singleton.gameObject;
        m_pOrbitThing.ReachedOrbit += M_pParentOrbit_ReachedOrbit;
        m_fStartTime = Time.time;
        BlueBugList.Add( this.gameObject );

        m_pShadowObject = Instantiate(
                m_pShadowObject,
                this.transform.position * 4.0f / this.transform.position.magnitude,
                this.transform.rotation,
                m_pGameControllerScript.gameObject.transform );
    }

    private void OnDestroy( )
    {
        BlueBugList.Remove( this.gameObject );
    }

    private void M_pParentOrbit_ReachedOrbit( object sender, System.EventArgs e )
    {
        DestroyImmediate( m_pShadowObject );

        Renderer r = GetComponent<Renderer>( );
        Color c = r.material.color;
        // needs to go from 0 to 1
        c.a = 1.0f;
        r.material.color = c;
    }

    // Update is called once per frame
    void Update ()
    {
        bool bInOrbit = m_pOrbitThing.InOrbit;

        if(!bInOrbit )
        {
            m_fAliveTime = Time.time - m_fStartTime;
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

            // keep it on the surface of the planet
            m_pShadowObject.transform.position = this.transform.position * 4.0f / this.transform.position.magnitude;
            m_pShadowObject.transform.rotation = this.transform.rotation * Quaternion.Euler( 90, 0, 0 );
            Renderer pShadowRenderer = m_pShadowObject.GetComponent<Renderer>( );
            Color cs = pShadowRenderer.material.color;
            float a = 1.0f - ( transform.position.magnitude - 4.0f ) / 16.0f;
            cs.r = a;
            pShadowRenderer.material.color = cs;

        }

        float d = Vector3.Distance( transform.position, m_pTheShip.transform.position );
        if( false ) // ( d < 1 )
        {
            // this block should be attracted to the ship!
            Vector3 pPointTowardsShip = m_pTheShip.transform.position - transform.position;
            Rigidbody rb = GetComponent<Rigidbody>( );
            rb.AddForce( pPointTowardsShip );
        }

        // transform.position = transform.parent.position;
    }

    private void OnEnable( )
    {
        // transform.position = transform.parent.transform.position;
    }

    private void OnCollisionEnter( Collision collision )
    {
        GameObject pHit = collision.gameObject;
        //Debug.Log( "bug hit object" + pHit.name );

        if ( pHit.layer == 6)
        {
            BulletHitUs( collision );
        }

    }

    void BulletHitUs( Collision collision )
    {
        BulletScript pBulletScript = collision.gameObject.GetComponent<BulletScript>();
        if (pBulletScript == null)
        {
            return;
        }

        GameObject pNewExplosion = Instantiate(
            m_pGameControllerScript.m_pAsteroidExplosion, 
            transform.position, 
            transform.rotation,
            transform );
        ParticleSystem pExplosionParticleSystem = pNewExplosion.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule pMain = pExplosionParticleSystem.main;
        pMain.stopAction = ParticleSystemStopAction.Destroy;
        pExplosionParticleSystem.Play( );

        AudioSource.PlayClipAtPoint( m_pExplosionSound, transform.position, 1.0f );

        Destroy( this.gameObject );
        Destroy( collision.gameObject );
    }
}
