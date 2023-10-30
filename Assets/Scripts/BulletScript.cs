using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public bool m_bDetectedHit;

    Vector3 m_pLastPos;
    float m_fTravelDist = 0;
    float m_fStartTime;
    Transform m_pActualBulletT;
    Rigidbody m_pRB;

    public static Transform CreateNewBullet(GameObject prefab, Transform lastBulletT, Transform shipT)
    {
        GameObject pNewBulletPlanet = Instantiate(
            prefab,
            shipT.parent);
        pNewBulletPlanet.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        Rigidbody pNewBulletPlanetRB = pNewBulletPlanet.GetComponent<Rigidbody>();
        pNewBulletPlanetRB.Move(Vector3.zero, Quaternion.identity);

        BulletScript bs = pNewBulletPlanet.GetComponent<BulletScript>();
        Transform pNewBulletT = bs.BulletT;
        Rigidbody pNewBulletRB = bs.BulletRB;
        pNewBulletT.SetPositionAndRotation(shipT.position, shipT.rotation);
        pNewBulletRB.Move(shipT.position, shipT.rotation);

        // anchor the bullet to the bullet's planet center. This has to be done at runtime, not edit time
        FixedJoint fj = pNewBulletT.gameObject.AddComponent<FixedJoint>();
        fj.connectedBody = pNewBulletPlanet.GetComponent<Rigidbody>();
        fj.autoConfigureConnectedAnchor = true;
        fj.connectedAnchor = Vector3.zero;
        fj.anchor = -shipT.position; // opposite of ship's position, to get to 0,0,0

        // reset the spring joint on the previous bullet

        if (lastBulletT != null)
        {
            Rigidbody pLastBulletRB = lastBulletT.GetComponent<Rigidbody>();

            if (ShipScript.m_sInstance.m_nMainWeaponType == Assets.Scripts.WeaponType.GoldMelter)
            {
#if true
                // add a spring joint between new bullet and prior bullet to that
                SpringJoint priorSpringJoint = lastBulletT.gameObject.AddComponent<SpringJoint>();
                Vector3 lastBulletPos = pLastBulletRB.position;
                Vector3 vMiddle = (lastBulletPos + pNewBulletRB.position) / 2;
                Vector3 vDelta_NewBullet_worldpos = vMiddle - pNewBulletRB.position;
                Vector3 vDelta_PriorBullet_worldpos = vMiddle - lastBulletPos;
                Vector3 vDelta_NewBullet_localpos = pNewBulletT.worldToLocalMatrix * vDelta_NewBullet_worldpos;
                Vector3 vDelta_PriorBullet_localpos = lastBulletT.worldToLocalMatrix * vDelta_PriorBullet_worldpos;
                priorSpringJoint.spring = 1;
                priorSpringJoint.damper = 0.1f;

                priorSpringJoint.autoConfigureConnectedAnchor = false;
                priorSpringJoint.connectedBody = pNewBulletRB;
                priorSpringJoint.connectedAnchor = vDelta_NewBullet_localpos;
                priorSpringJoint.anchor = vDelta_PriorBullet_localpos;
                priorSpringJoint.minDistance = 0;
                priorSpringJoint.maxDistance = 0;
#endif
            }
        }

        return pNewBulletT;
    }

    public Rigidbody BulletRB
    {
        get
        {
            if (m_pRB == null)
            {
                m_pRB = BulletT.GetComponent<Rigidbody>();
            }
            return m_pRB;
        }
    }

    public Transform BulletT
    {
        get
        {
            if( m_pActualBulletT == null )
            {
                m_pActualBulletT = transform.GetChild(0);
            }
            return m_pActualBulletT;
        }
    }

    void Start ()
    {
        m_pLastPos = transform.position;
        m_fTravelDist = 0;
        m_fStartTime = Time.time;
        m_pActualBulletT = transform.GetChild(0);
        m_pRB = m_pActualBulletT.GetComponent<Rigidbody>();
    }

    void Update ()
    {

        Vector3 pos = transform.position;
        Vector3 delta = pos - m_pLastPos;
        m_pLastPos = pos;
        float distance = delta.magnitude;
        m_fTravelDist += distance;
        float deltaTime = Time.time - m_fStartTime;
        if (deltaTime > 5)
        {
            DestroyMe();
            return;
        }
        if (m_fTravelDist > 6 * Mathf.PI)
        {
             DestroyMe();
            return;
        }
    }

    void DestroyMe()
    {
        Destroy(gameObject);
    }
}
