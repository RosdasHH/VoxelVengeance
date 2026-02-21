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
    private GameObject WallCrosshair;

    private LayerMask _ground;
    private float range;

    private void Awake()
    {
        WeaponCanvas = GameObject.FindGameObjectWithTag("WeaponCanvas");
        if (!WeaponCanvas)
            Debug.LogError("weapon canvas not found");
        Crosshair = WeaponCanvas.transform.Find("Crosshair").gameObject;
        if (!Crosshair)
            Debug.LogError("crosshair not found");
        WallCrosshair = GameObject.FindGameObjectWithTag("WallCrosshair");
        if (!WallCrosshair)
            Debug.LogError("wall crosshair not found");
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

        if (Crosshair && activeWeaponInstance)
        {
            PlaceCrosshair();
        }
    }

    void PlaceCrosshair()
    {
        //cast ray forwards from weapon
        Vector3 origin = activeWeaponInstance.transform.position;
        bool hitInfoStraight = Physics.Linecast(
            origin,
            activeWeaponInstance.transform.forward * range,
            out RaycastHit hit
        );
        if (hitInfoStraight)
        {
            Vector3 hitPos = hit.point;
            SwitchCrosshair(WallCrosshair);
        }
    }

    void SwitchCrosshair(GameObject newCrosshair)
    {
        if(WallCrosshair && newCrosshair == WallCrosshair)
        {
            WallCrosshair.SetActive(true);
        }
        else if(Crosshair)
        {
            Crosshair.SetActive(true);
        }
    }

    public void tryEquip(int weaponId)
    {
        if (!IsOwner)
            return;
        if (Weapons[weaponId] == null)
            return;

        RequestEquipServerRpc(weaponId);
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
        Image crosshairImg = Crosshair.GetComponent<Image>();
        crosshairImg.sprite = activeWeaponInstance.GetComponent<WeaponData>().crosshair;
    }
}
