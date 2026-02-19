using UnityEngine;
using UnityEngine.InputSystem;

public class UserInput : MonoBehaviour {

    public static Vector2 MoveInput;
    public static float JumpInput;
    public static Vector2 LookVector;

    private PlayerInput playerInput;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _lookAction;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        _moveAction = playerInput.actions["Move"];
        _jumpAction = playerInput.actions["Jump"];
        _lookAction = playerInput.actions["Look"];
    }
    void OnEnable()
    {
        playerInput.currentActionMap.Enable();
    }
    void Update()
    {
        MoveInput = _moveAction.ReadValue<Vector2>();
        JumpInput = _jumpAction.ReadValue<float>();
        LookVector = _lookAction.ReadValue<Vector2>();
    }
}