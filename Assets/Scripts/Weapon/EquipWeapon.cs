using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class EquipWeapon : NetworkBehaviour
{
    public NetworkVariable<int> activeWeaponId = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private InputAction firstSlotAction;

    [SerializeField]
    private GameObject pistol;
    [SerializeField]
    private Transform WeaponSpawnerFront;

    public GameObject activeWeaponInstance;

    private void Awake()
    {
        firstSlotAction = InputSystem.actions.FindAction("FirstSlot");
    }

    private void Update()
    {
        if (!IsOwner || !IsClient) return;
        if(firstSlotAction.WasPressedThisFrame())
        {
            tryEquip(1);
        }
    }

    public override void OnNetworkSpawn()
    {
        activeWeaponId.OnValueChanged += OnActiveWeaponChanged;
    }

    public void tryEquip(int weaponId)
    {
        if (!IsOwner) return;
        RequestEquipServerRpc(weaponId);
    }

    [ServerRpc]
    public void RequestEquipServerRpc(int weaponId)
    {
        //Check if the weapon is owned.
        Debug.Log("Validated. Weapon is allowed.");
        activeWeaponId.Value = weaponId;
    }

    public void OnActiveWeaponChanged(int previous, int next)
    {
        //Instantiate
        Debug.Log("Successfully equiped " + next);
        Destroy(activeWeaponInstance);
        if (next == 1)
        {
            activeWeaponInstance = Instantiate(pistol, WeaponSpawnerFront);
        }
    }
}
