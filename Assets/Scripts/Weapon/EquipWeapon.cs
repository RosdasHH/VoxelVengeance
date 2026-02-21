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

    private GameObject Crosshair;
    private GameObject WallCrosshair;
    private GameObject curCrosshair;

    [SerializeField]
    private int _bulletLayerInt;
    private int chCheckLayers;

    private float range;

    private void Awake()
    {
        chCheckLayers = ~(1 << _bulletLayerInt);
            Debug.Log($"Ignoring bullet layer {_bulletLayerInt}, mask {chCheckLayers}");

    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Crosshair = GameObject.FindGameObjectWithTag("GroundCrosshair");
            if (!Crosshair)
                Debug.LogError("crosshair not found");
            WallCrosshair = GameObject.FindGameObjectWithTag("WallCrosshair");
            if (!WallCrosshair)
                Debug.LogError("wall crosshair not found");
            WallCrosshair.SetActive(false);
        }
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

        if (Crosshair && WallCrosshair && activeWeaponInstance)
        {
            PlaceCrosshair();
        }
    }

    void PlaceCrosshair()
    {
        //cast ray forwards from weapon
        Vector3 origin = activeWeaponInstance.GetComponent<WeaponData>().bulletSpawn.position;
        Vector3 forward = activeWeaponInstance.GetComponent<WeaponData>().bulletSpawn.forward;
        bool hitInfoStraight = Physics.Linecast(
            origin,
            origin + forward * range,
            out RaycastHit hit,
            chCheckLayers
        );
        if (hitInfoStraight)
        {
            if (curCrosshair != WallCrosshair)
                SwitchCrosshair(WallCrosshair);
            Vector3 hitPos = hit.point;
            curCrosshair.transform.position = hitPos;
            curCrosshair.transform.rotation = Quaternion.LookRotation(hit.normal, Vector3.up);
        }
        else
        {
            if (curCrosshair != Crosshair)
                SwitchCrosshair(Crosshair);
            //project line down, until hits ground
            bool hitInfoDown = Physics.Raycast(
                origin + forward * range,
                Vector3.down,
                out RaycastHit hitDown,
                100,
                chCheckLayers
            );
            curCrosshair.transform.rotation = gameObject.transform.rotation;
            if (hitInfoDown)
            {
                curCrosshair.transform.position = hitDown.point + Vector3.up * 0.02f;
            }
            else
            {
                curCrosshair.transform.position = origin + forward * range + Vector3.down * 1.5f;
            }
        }
    }

    void SwitchCrosshair(GameObject newCh)
    {
        Crosshair.SetActive(newCh == Crosshair);
        WallCrosshair.SetActive(newCh == WallCrosshair);
        curCrosshair = newCh;
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
        range = activeWeaponInstance.GetComponent<WeaponData>().crosshairRange;
    }
}
