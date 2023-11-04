using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidExplosionPrefabScript : MonoBehaviour
{
    int nChunks = 10;

    public GameObject m_pAsteroidChunkPrefab;

    // Start is called before the first frame update
    void Start()
    {
        // create a bunch of chunks that blow out and twist and fade out
        Vector3 pos = transform.position;
        for (int i = 0; i < nChunks; i++)
        {
            GameObject p = Instantiate(m_pAsteroidChunkPrefab,
                transform);
            // give them a random velocity
            Rigidbody rb = p.GetComponent<Rigidbody>();
            rb.AddForce(Random.insideUnitSphere * Random.Range(1, 3), ForceMode.Impulse);

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
