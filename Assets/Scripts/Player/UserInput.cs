using UnityEngine;
using UnityEngine.InputSystem;

public class UserInput : MonoBehaviour
{
    public static Vector2 MoveInput;
    public static Vector2 LookInput;
    public static bool WasEscapePressed;
    public static bool WasEscapePauseMenuPressed;
    public static bool WasShootPressed;

    public static PlayerInput playerInput;
    
    private InputAction _moveAction;
    private InputAction _wasEscapePressed;
    private InputAction _lookAction;
    private InputAction _wasEscapePauseMenuPressed;
    private InputAction _shootAction;

    private bool inputEnabled = true;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        _moveAction = playerInput.actions["Move"];
        _lookAction = playerInput.actions["Look"];
        _wasEscapePressed = playerInput.actions["Escape"];
        _wasEscapePauseMenuPressed = playerInput.actions["EscapePauseMenu"];
        _shootAction = playerInput.actions["Attack"];
    }

    void OnEnable()
    {
        playerInput.currentActionMap.Enable();
    }

    public void ToggleInput(bool enabled)
    {
        if (enabled)
        {
            inputEnabled = true;
        }
        else
        {
            inputEnabled = false;
        }
    }

    void Update()
    {
        if (inputEnabled)
        {
            MoveInput = _moveAction.ReadValue<Vector2>();
            LookInput = _lookAction.ReadValue<Vector2>();
            WasShootPressed = _shootAction.WasPressedThisFrame();
        }
        WasEscapePressed = _wasEscapePressed.WasPressedThisFrame();
        WasEscapePauseMenuPressed = _wasEscapePauseMenuPressed.WasPressedThisFrame();
    }
}
