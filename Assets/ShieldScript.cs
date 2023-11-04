using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldScript : MonoBehaviour
{
    ShipScript theShip;

    // Start is called before the first frame update
    void Start()
    {
        theShip = transform.parent.GetComponent<ShipScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        theShip.OnShieldCollisionEnter(collision);
    }
}
