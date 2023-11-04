using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControllerScript : MonoBehaviour {

    public GameObject m_pAsteroidType1_Big;
    public GameObject m_pAsteroidType1_BigMed;
    public GameObject m_pAsteroidType1_Med;
    public GameObject m_pAsteroidType1_Small;
    public GameObject m_pAsteroidExplosion;
    public GameObject m_pBonusAsteroid;
    public GameObject m_pPlanet;
    public GameObject m_pBlueBug;
    public GameObject m_pBonusPellet;
    public AudioClip m_pAsteroidExplodeSound;
    public AudioClip m_pBonusAsteroidExplodeSound;
    public CanvasScript m_Canvas;

    float m_fUpdateNowTime;
    int m_nWave;
    int m_nFrameCounter;
    int m_nDelayUntilAliens;
    int m_nRocksLeftToAppear;
    float m_fLastTimeRockSpawned;
    float m_fTimePerRock;
    int m_nTimeLeftUntilNextRockSpawn;
    int m_nWaveState;
    float m_fSecondsIntoWave;
    float m_fTimeAtStartOfWave;
    float m_fWaitTimeUntilFirstRock = 0;
    float m_fTimeOfLastAlienCreateWave2;
    float m_fTimeBetweenAliensWave2 = 0.1f;
    int m_nAliensCreatedWave2 = 0;
    int m_nAliensToCreateWave2 = 100;

    // here's how a single wave works...
    //
    // 1. The planet springs into view with a whoosh sound
    // 2. rocks start to fall in towards the planet, and materialize at the same time
    // 3. After a delay, the first wave of aliens appear
    // 4. after all the aliens are cleared, more rocks start to fall
    // 5. after a delay, the 2nd wave of aliens appear
    // 6. after all the aliens are cleared, more rocks start to fall
    // 7. after a delay, the boss appears

    public static GameControllerScript Singleton;

    private void Awake( )
    {
        Singleton = this;
    }

    void Start ()
    {
        m_nWave = 0;
        m_nWaveState = 1;
        m_nFrameCounter = 0;
        m_fTimePerRock = 1.0f;
        m_fLastTimeRockSpawned = Time.time;

        m_fTimeOfLastAlienCreateWave2 = Time.time - m_fTimeOfLastAlienCreateWave2;

        while ( true )
        {
            AsteroidScript pScript = m_pAsteroidType1_Big.GetComponentInChildren<AsteroidScript>( );
            if( pScript == null )
            {
                break;
            }
            Destroy(pScript);
        }

        // Start_Wave0( );
    }
    
    // Update is called once per frame
    void Update ()
    {
        m_fUpdateNowTime = Time.time;
        m_fSecondsIntoWave = m_fUpdateNowTime - m_fTimeAtStartOfWave;

        switch( m_nWave )
        {
            case 0:
                Update_Wave0( );
                break;
            case 1:
                Update_Wave1( );
                break;
            case 2:
                Update_Wave2( );
                break;
            case 3:
                Update_Wave3( );
                break;
            case 4:
                Update_Wave4( );
                break;
            case 5:
                Update_Wave5( );
                break;
            case 6:
                Update_Wave6( );
                break;
            case 7:
                Update_Wave7( );
                break;
            default:
                Debug.Assert( false );
                break;
        }

        m_nFrameCounter++;
    }

    void Start_Wave0( )
    {
        m_fSecondsIntoWave = 0;
    }

    void Start_Wave1( )
    {
        m_nWave = 1;
        m_fSecondsIntoWave = 0;
        m_nTimeLeftUntilNextRockSpawn = 0;
        m_nRocksLeftToAppear = 10;
    }

    void Start_Wave2( )
    {
        m_nWave = 2;
        m_fSecondsIntoWave = 0;
        m_nTimeLeftUntilNextRockSpawn = 0;
        m_fTimeOfLastAlienCreateWave2 = Time.time;
    }

    void Start_Wave3( )
    {
        m_nWave = 3;
        m_fSecondsIntoWave = 0;
        m_nTimeLeftUntilNextRockSpawn = 0;
    }

    void Start_Wave4( )
    {
        m_nWave = 4;
        m_fSecondsIntoWave = 0;
        m_nTimeLeftUntilNextRockSpawn = 0;
    }

    void Start_Wave5( )
    {
        m_nWave = 5;
        m_fSecondsIntoWave = 0;
        m_nTimeLeftUntilNextRockSpawn = 0;
    }

    void Start_Wave6( )
    {
        m_nWave = 6;
        m_fSecondsIntoWave = 0;
        m_nTimeLeftUntilNextRockSpawn = 0;
    }

    void Start_Wave7( )
    {
        m_nWave = 7;
        m_fSecondsIntoWave = 0;
        m_nTimeLeftUntilNextRockSpawn = 0;
        m_fTimeOfLastAlienCreateWave2 = Time.time - m_fTimeBetweenAliensWave2; // create an alien right away
    }

    void Update_Wave0( )
    {
        if( m_fSecondsIntoWave > 0.0f )
        {
            Start_Wave1( );
        }
    }

    void Update_Wave1( )
    {
        if( m_fSecondsIntoWave < m_fWaitTimeUntilFirstRock )
        {
            return;
        }

        // okay time to start laying down asteroids, every N seconds, we'll start an asteroid at a random location
        // and let it "fall" to the planet and fade in
        if( m_nTimeLeftUntilNextRockSpawn > 0 )
        {
            m_nTimeLeftUntilNextRockSpawn--;
            return;
        }

        if( m_nRocksLeftToAppear == 0 )
        {
            // go to next wave
            Start_Wave2( );
            return;
        }

        float fDelta = m_fUpdateNowTime - m_fLastTimeRockSpawned;
        if( fDelta < m_fTimePerRock )
        {
            return;
        }

        m_fLastTimeRockSpawned = m_fUpdateNowTime;

        // make a rock and start it fading in

        m_nRocksLeftToAppear--;

        GameObject pNewBigAsteroid = Instantiate(
                m_pAsteroidType1_Big,
                Random.onUnitSphere * 20,
                Random.rotation,
                TheSystemScript.Singleton.transform );

        Renderer r = pNewBigAsteroid.GetComponent<Renderer>( );
        Color c = r.material.color;
        c.a = 0.5f;
        r.material.color = c;

        // OrbitObjectContainer pOrbit = pNewBigAsteroid.AddComponent<OrbitObjectContainer>( );
        AsteroidScript pScript = pNewBigAsteroid.AddComponent<AsteroidScript>();
        pScript.AsteroidSize = 8;
        pScript.m_nAsteroidType = Assets.Scripts.WeaponType.GoldMelter;
    }

    void Update_Wave2( )
    {
        if( m_nAliensCreatedWave2 >= m_nAliensToCreateWave2 )
        {
            return;
        }

        float fNow = Time.time;
        float fElapsedSinceLastAlienCreate = fNow - m_fTimeOfLastAlienCreateWave2;
        if( fElapsedSinceLastAlienCreate < m_fTimeBetweenAliensWave2 )
        {
            return;
        }

        // time to create an alien
        m_fTimeOfLastAlienCreateWave2 = fNow;

        // create an alien. They start out in space, and dive towards the planet
        m_nAliensCreatedWave2++;

        GameObject pNewBlueBug = Instantiate(
                m_pBlueBug,
                Random.onUnitSphere * 20, // make it really far out. should be at 4
                Quaternion.Euler( 90, 0, 0 ),
                TheSystemScript.Singleton.transform);
    }

    void Update_Wave3( )
    {

    }

    void Update_Wave4( )
    {

    }

    void Update_Wave5( )
    {

    }

    void Update_Wave6( )
    {

    }

    void Update_Wave7( )
    {

    }

}
