using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EquipWeapon : NetworkBehaviour
{
    public NetworkVariable<int> activeWeaponId = new NetworkVariable<int>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    private InputAction firstSlotAction;

    [SerializeField]
    private GameObject pistol;

    [SerializeField]
    private Transform WeaponSpawnerFront;

    public GameObject activeWeaponInstance;

    private GameObject WeaponCanvas;
    private GameObject Crosshair;

    private void Awake()
    {
        WeaponCanvas = GameObject.FindGameObjectWithTag("WeaponCanvas");
        Crosshair = WeaponCanvas.transform.Find("Crosshair").gameObject;
        firstSlotAction = InputSystem.actions.FindAction("FirstSlot");
    }

    public override void OnNetworkSpawn()
    {
        activeWeaponId.OnValueChanged += (pre, post) => SpawnWeaponLocal(post);
        SpawnWeaponLocal(activeWeaponId.Value);
    }

    private void Update()
    {
        if (!IsOwner || !IsClient)
            return;
        if (firstSlotAction.WasPressedThisFrame())
        {
            tryEquip(1);
        }
    }

    public void tryEquip(int weaponId)
    {
        if (!IsOwner)
            return;
        RequestEquipServerRpc(weaponId);
        //equipped weapon??
        // Image crosshairImg = Crosshair.GetComponent<Image>();
        // crosshairImg.sprite = activeWeaponInstance.GetComponent<WeaponData>().crosshair;
    }

    [ServerRpc]
    public void RequestEquipServerRpc(int weaponId)
    {
        activeWeaponId.Value = weaponId;
    }

    private void SpawnWeaponLocal(int weaponId)
    {
        if (activeWeaponInstance != null)
        {
            Destroy(activeWeaponInstance);
            activeWeaponInstance = null;
        }

        if (weaponId == 1)
            activeWeaponInstance = Instantiate(pistol, WeaponSpawnerFront);
    }
}
