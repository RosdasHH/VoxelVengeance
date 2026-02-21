using UnityEngine;
using UnityEngine.InputSystem;

public class UserInput : MonoBehaviour
{
    public static Vector2 MoveInput;
    public static Vector2 LookInput;
    public static bool WasEscapePressed;
    public static bool WasEscapePauseMenuPressed;
    public static bool WasShootPressed;
    public static bool IsShootPressed;
    //weapon slots
    public static bool SlotPressed1;
    public static bool SlotPressed2;
    public static bool SlotPressed3;

    public static PlayerInput playerInput;
    
    private InputAction _moveAction;
    private InputAction _wasEscapePressed;
    private InputAction _lookAction;
    private InputAction _wasEscapePauseMenuPressed;
    private InputAction _shootAction;
    //weapon slots
    private InputAction _slotPressed1;
    private InputAction _slotPressed2;
    private InputAction _slotPressed3;

    private bool inputEnabled = true;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        _moveAction = playerInput.actions["Move"];
        _lookAction = playerInput.actions["Look"];
        _wasEscapePressed = playerInput.actions["Escape"];
        _wasEscapePauseMenuPressed = playerInput.actions["EscapePauseMenu"];
        _shootAction = playerInput.actions["Attack"];

        _slotPressed1 = playerInput.actions["Slot1"];
        _slotPressed2 = playerInput.actions["Slot2"];
        _slotPressed3 = playerInput.actions["Slot3"];
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
            IsShootPressed = _shootAction.IsPressed();

            //weapon slots
            SlotPressed1 = _slotPressed1.WasPressedThisFrame();
            SlotPressed2 = _slotPressed2.WasPressedThisFrame();
            SlotPressed3 = _slotPressed3.WasPressedThisFrame();
        }
        WasEscapePressed = _wasEscapePressed.WasPressedThisFrame();
        WasEscapePauseMenuPressed = _wasEscapePauseMenuPressed.WasPressedThisFrame();
    }
}
