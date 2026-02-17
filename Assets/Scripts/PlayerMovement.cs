using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField]
    private float speed;
    [SerializeField]
    private InputActionAsset InputActions;

    private InputAction moveAction;

    private Vector2 movement;


    private void OnEnable()
    {
        InputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        InputActions.FindActionMap("Player").Disable();
    }

    private void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Move");
    }
    
    void Update()
    {
        if (!IsOwner) return;
        MovePlayerServer();
    }

    private void MovePlayerServer()
    {
        movement = moveAction.ReadValue<Vector2>();
        MovePlayerServerRpc(movement);
    }

    [ServerRpc]
    private void MovePlayerServerRpc(Vector2 movement)
    {
        transform.Translate(movement.x * Time.deltaTime * speed, 0, movement.y * Time.deltaTime * speed);
    }
}
