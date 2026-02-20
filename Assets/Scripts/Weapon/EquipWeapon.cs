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

    public void tryEquip(int weaponId)
    {
        if (!IsOwner) return;
        RequestEquipServerRpc(weaponId);
    }

    [ServerRpc]
    public void RequestEquipServerRpc(int weaponId)
    {
        activeWeaponId.Value = weaponId;
        if(activeWeaponInstance != null) activeWeaponInstance.GetComponent<NetworkObject>().Despawn();
        activeWeaponInstance = Instantiate(pistol, WeaponSpawnerFront.position, WeaponSpawnerFront.rotation);
        activeWeaponInstance.GetComponent<NetworkObject>().Spawn();
        activeWeaponInstance.GetComponent<NetworkObject>().TrySetParent(NetworkObject);
    }
}
