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

    [SerializeField]
    private Transform WeaponSpawnerFront;

    [SerializeField]
    public GameObject[] Weapons = new GameObject[3];

    public GameObject activeWeaponInstance;

    private GameObject WeaponCanvas;
    private GameObject Crosshair;

    private void Awake()
    {
        WeaponCanvas = GameObject.FindGameObjectWithTag("WeaponCanvas");
        Crosshair = WeaponCanvas.transform.Find("Crosshair").gameObject;
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
        if (UserInput.SlotPressed1)
            tryEquip(0);
        else if (UserInput.SlotPressed2)
            tryEquip(1);
        else if (UserInput.SlotPressed3)
            tryEquip(2);
    }

    public void tryEquip(int weaponId)
    {
        if (!IsOwner)
            return;
        if (Weapons[weaponId] == null)
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

        activeWeaponInstance = Instantiate(Weapons[weaponId], WeaponSpawnerFront);
    }
}
