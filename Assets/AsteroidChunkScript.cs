using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidChunkScript : MonoBehaviour
{
    float m_fStartTime;
    float m_fLiveTime = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        m_fStartTime = Time.time;    
    }

    // Update is called once per frame
    void Update()
    {
        float fDelta = Time.time - m_fStartTime;
        if (fDelta >= m_fLiveTime)
        {
            Destroy(gameObject);
        }
    }
}
