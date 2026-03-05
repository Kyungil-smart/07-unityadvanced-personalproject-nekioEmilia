using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAction : MonoBehaviour
{
    private PlayerActionInput playeInput;

    private void Awake()
    {
        playeInput = GetComponent<PlayerActionInput>();
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    private void HandleInput(InputAction.CallbackContext ctx)
    {
        
    }
}
