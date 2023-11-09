using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AsteroidChunkScript : MonoBehaviour
{
    float m_fStartTime;
    float m_fLiveTime = 1.5f;
    Material m_Material;

    // Start is called before the first frame update
    void Start()
    {
        m_fStartTime = Time.time;
        m_Material = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        float fDelta = Time.time - m_fStartTime;
        if (fDelta >= m_fLiveTime)
        {
            // Destroy(gameObject);
            return;
        }
        if (m_Material != null)
        {
            float alpha = fDelta / m_fLiveTime;
            Color c = m_Material.color;
            c.a = alpha;
            m_Material.color = c;
        }
    }
}
