using System.Runtime.CompilerServices;
using Unity.Netcode;
using Unity.VisualScripting;
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
    public NetworkVariable<UserInput.WeaponSideType> weaponSideNetwork = new NetworkVariable<UserInput.WeaponSideType>(
    default,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
);

    [SerializeField]
    private Transform WeaponSpawnLeft;
    [SerializeField]
    private Transform WeaponSpawnRight;

    public GameObject activeWeaponInstance;

    private GameObject Crosshair;
    private GameObject WallCrosshair;
    private GameObject curCrosshair;

    [SerializeField]
    private int _bulletLayerInt;
    private int chCheckLayers;

    private float range;

    private PauseMenuManager pmm;

    private void Awake()
    {
        pmm = GameObject.FindWithTag("PauseMenuManager").GetComponent<PauseMenuManager>();
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
        weaponSideNetwork.OnValueChanged += (pre, post) => SpawnWeaponLocal(activeWeaponId.Value);
        reloadWeapon();
    }
    [ServerRpc]
    public void changeWeaponSideNetworkServerRpc(UserInput.WeaponSideType weaponSide)
    {
        weaponSideNetwork.Value = weaponSide;
    }

    public void reloadWeapon()
    {
        SpawnWeaponLocal(activeWeaponId.Value);
    }

    private void Update()
    {
        if (!IsOwner || !IsClient)
            return;
        if (UserInput.SlotChange)
            tryEquip(UserInput.selectedSlot);

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
        if (pmm.Weapons[weaponId] == null)
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
        WeaponData weaponData = pmm.selectedWeapons[activeWeaponId.Value].Prefab.GetComponent<WeaponData>();
        Transform spawnpoint;

        if (weaponSideNetwork.Value == UserInput.WeaponSideType.Right)
        {
            spawnpoint = WeaponSpawnRight;
        } else
        {
            spawnpoint = WeaponSpawnLeft;
        }
            activeWeaponInstance = Instantiate(pmm.selectedWeapons[activeWeaponId.Value].Prefab, spawnpoint);
        range = weaponData.crosshairRange;
    }
    public WeaponData getSelectedWeaponData()
    {
        return (pmm.selectedWeapons[activeWeaponId.Value].Prefab.GetComponent<WeaponData>());
    }
}
