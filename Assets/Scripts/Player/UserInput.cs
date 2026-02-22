using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class UserInput : NetworkBehaviour
{
    public static int selectedSlot = 0;

    //Movement
    public static Vector2 MoveInput;
    public static Vector2 LookInput;
    public static bool WasEscapePressed;
    public static bool WasEscapePauseMenuPressed;
    public static bool WasShootPressed;
    public static bool IsShootPressed;
    public static bool IsAimPressed;
    public enum WeaponSideType
    {
        Left,
        Right
    }
    public static bool WasWeaponSidePressed;
    public static WeaponSideType weaponSide = WeaponSideType.Right;

    public static bool SlotChange;

    public static PlayerInput playerInput;
    
    private InputAction _moveAction;
    private InputAction _wasEscapePressed;
    private InputAction _lookAction;
    private InputAction _wasEscapePauseMenuPressed;
    private InputAction _shootAction;
    private InputAction _weaponSide;
    private InputAction _Aim;

    private InputAction _slotChange;

    private bool inputEnabled = true;

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            playerInput = GetComponent<PlayerInput>();
            _moveAction = playerInput.actions["Move"];
            _lookAction = playerInput.actions["Look"];
            _wasEscapePressed = playerInput.actions["Escape"];
            _wasEscapePauseMenuPressed = playerInput.actions["EscapePauseMenu"];
            _shootAction = playerInput.actions["Attack"];
            _weaponSide = playerInput.actions["WeaponSide"];
            _slotChange = playerInput.actions["ChangeSlot"];
            _Aim = playerInput.actions["Aim"];
            playerInput.ActivateInput();
        }
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
        if (!IsOwner) return;
        if (inputEnabled)
        {
            MoveInput = _moveAction.ReadValue<Vector2>();
            LookInput = _lookAction.ReadValue<Vector2>();
            WasShootPressed = _shootAction.WasPressedThisFrame();
            IsShootPressed = _shootAction.IsPressed();
            WasWeaponSidePressed = _weaponSide.WasPressedThisFrame();
            IsAimPressed = _Aim.IsPressed();

            if(WasWeaponSidePressed)
            {
                if(weaponSide == WeaponSideType.Right)
                {
                    weaponSide = WeaponSideType.Left;
                }
                else
                {
                    weaponSide= WeaponSideType.Right;
                }
            }
            GetComponent<EquipWeapon>().changeWeaponSideNetworkServerRpc(weaponSide);

            //weapon slots
            SlotChange = _slotChange.WasPressedThisFrame();
            if (SlotChange)
            {
                if (selectedSlot == 0)
                {
                    selectedSlot = 1;
                }
                else
                {
                    selectedSlot = 0;
                }
            }
        }
        WasEscapePressed = _wasEscapePressed.WasPressedThisFrame();
        WasEscapePauseMenuPressed = _wasEscapePauseMenuPressed.WasPressedThisFrame();
    }
}
