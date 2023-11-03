using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetScript : MonoBehaviour
{
    bool m_bFadingIn;
    float m_fStartFadeInTime;
    GameObject m_pPlanet;

    // Use this for initialization
    void Start ()
    {
        m_pPlanet = transform.Find( "Planet" ).gameObject;
    }
    
    // Update is called once per frame
    void Update ()
    {
        if( m_bFadingIn )
        {
            float fElapsed = Time.time - m_fStartFadeInTime;
            if( fElapsed >= 5.0f )
            {
                fElapsed = 5.0f;
                m_bFadingIn = false;
            }
            float alpha = fElapsed / 5.0f;
            Renderer rWire = GetComponent<Renderer>( );
            Color c = rWire.material.color;
            c.a = alpha;
            rWire.material.color = c;
            Renderer rPlanet = m_pPlanet.GetComponent<Renderer>( );
            c = rPlanet.material.color;
            c.a = alpha;
            rPlanet.material.color = c;
        }
    }

    public void StartFadeIn( )
    {
        m_bFadingIn = true;
        m_fStartFadeInTime = Time.time;

    }
}
