using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class spherescript : MonoBehaviour, InputActions.IGameplayActions
{
    bool m_bThrustDown;

    public void OnFire(InputAction.CallbackContext context)
    {
    }

    public void OnMoveJoystick(InputAction.CallbackContext context)
    {
    }

    public void OnMoveVector2(InputAction.CallbackContext context)
    {
    }

    public void OnThrustButton(InputAction.CallbackContext context)
    {
        m_bThrustDown = context.ReadValueAsButton();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (m_bThrustDown)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.AddForce(transform.up * 3, ForceMode.Acceleration);
        }
    }
}
