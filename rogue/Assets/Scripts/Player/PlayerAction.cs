using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class PlayerAction : MonoBehaviour
{
    PlayerInput playerInput;
    Rigidbody2D rb;

    private Vector2 moveDir;
    [SerializeField] private float moveSpeed = 1;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        playerInput.onActionTriggered += HandleInput;
    }

    private void OnDisable()
    {
        playerInput.onActionTriggered -= HandleInput;
    }

    private void HandleInput(InputAction.CallbackContext ctx)
    {
        switch (ctx.action.name)
        {
            case "Move":
                moveDir = ctx.ReadValue<Vector2>();
                Debug.Log("[C# Event] Move: " + moveDir);
                break;
            
            case "Attack":
                if (ctx.performed)
                {
                    Debug.Log("[C# Event] 공격!");
                }
                break;
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveDir * moveSpeed;
    }
}
