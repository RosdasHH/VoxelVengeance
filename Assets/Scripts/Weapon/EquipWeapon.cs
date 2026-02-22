using Unity.Netcode;
using UnityEngine;

public class EquipWeapon : NetworkBehaviour
{

    public NetworkVariable<int> activeSlot = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> slot0WeaponId = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner
    );
    public NetworkVariable<int> slot1WeaponId = new NetworkVariable<int>(
        1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner
    );

    public NetworkVariable<UserInput.WeaponSideType> weaponSideNetwork =
        new NetworkVariable<UserInput.WeaponSideType>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    [SerializeField] private Transform WeaponSpawnLeft;
    [SerializeField] private Transform WeaponSpawnRight;

    public GameObject activeWeaponInstance;

    private GameObject Crosshair;
    private GameObject WallCrosshair;
    private GameObject curCrosshair;

    [SerializeField] private int _bulletLayerInt;
    private int chCheckLayers;

    private float range;

    private PauseMenuManager pmm;

    private void Awake()
    {
        pmm = GameObject.FindWithTag("PauseMenuManager")?.GetComponent<PauseMenuManager>();
        chCheckLayers = ~(1 << _bulletLayerInt);
        Debug.Log($"Ignoring bullet layer {_bulletLayerInt}, mask {chCheckLayers}");
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Crosshair = GameObject.FindGameObjectWithTag("GroundCrosshair");
            if (!Crosshair) Debug.LogError("GroundCrosshair not found");

            WallCrosshair = GameObject.FindGameObjectWithTag("WallCrosshair");
            if (!WallCrosshair) Debug.LogError("WallCrosshair not found");

            if (WallCrosshair) WallCrosshair.SetActive(false);
        }

        activeSlot.OnValueChanged += (_, __) => SpawnWeaponLocal();
        slot0WeaponId.OnValueChanged += (_, __) => SpawnWeaponLocal();
        slot1WeaponId.OnValueChanged += (_, __) => SpawnWeaponLocal();
        weaponSideNetwork.OnValueChanged += (_, __) => SpawnWeaponLocal();

        SpawnWeaponLocal();
    }

    private void Update()
    {
        if (!IsSpawned || !IsClient || !IsOwner)
            return;

        if (UserInput.SlotChange)
        {
            int next = (activeSlot.Value == 0) ? 1 : 0;
            TryEquipSlot(next);
        }

        if (Crosshair && WallCrosshair && activeWeaponInstance)
        {
            PlaceCrosshair();
        }
    }

    public void SetLoadout(int weaponIdSlot0, int weaponIdSlot1)
    {
        if (!IsOwner || !IsSpawned) return;

        slot0WeaponId.Value = weaponIdSlot0;
        slot1WeaponId.Value = weaponIdSlot1;

        SpawnWeaponLocal();
    }

    public void reloadWeaponLocalOnly()
    {
        if (!IsClient) return;
        SpawnWeaponLocal();
    }

    public void TryEquipSlot(int slotIndex)
    {
        if (!IsOwner) return;
        slotIndex = Mathf.Clamp(slotIndex, 0, 1);
        RequestEquipSlotServerRpc(slotIndex);
    }

    [ServerRpc]
    private void RequestEquipSlotServerRpc(int slotIndex)
    {
        slotIndex = Mathf.Clamp(slotIndex, 0, 1);
        if (activeSlot.Value == slotIndex) return;
        activeSlot.Value = slotIndex;
    }

    [ServerRpc]
    public void changeWeaponSideNetworkServerRpc(UserInput.WeaponSideType weaponSide)
    {
        weaponSideNetwork.Value = weaponSide;
    }

    private int GetActiveWeaponId()
    {
        return activeSlot.Value == 0 ? slot0WeaponId.Value : slot1WeaponId.Value;
    }

    private void SpawnWeaponLocal()
    {
        if (!IsClient) return;
        if (pmm == null || pmm.Weapons == null || pmm.Weapons.Length == 0) return;

        int weaponId = GetActiveWeaponId();
        if (weaponId < 0 || weaponId >= pmm.Weapons.Length) return;
        if (pmm.Weapons[weaponId] == null || pmm.Weapons[weaponId].Prefab == null) return;

        if (activeWeaponInstance != null)
        {
            Destroy(activeWeaponInstance);
            activeWeaponInstance = null;
        }

        Transform spawnpoint = (weaponSideNetwork.Value == UserInput.WeaponSideType.Right)
            ? WeaponSpawnRight
            : WeaponSpawnLeft;

        activeWeaponInstance = Instantiate(pmm.Weapons[weaponId].Prefab, spawnpoint);

        var weaponData = activeWeaponInstance.GetComponent<WeaponData>();
        if (weaponData != null) range = weaponData.crosshairRange;
    }

    void PlaceCrosshair()
    {
        var wd = activeWeaponInstance.GetComponent<WeaponData>();
        if (wd == null || wd.bulletSpawn == null) return;

        Vector3 origin = wd.bulletSpawn.position;
        Vector3 forward = wd.bulletSpawn.forward;

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

            bool hitInfoDown = Physics.Raycast(
                origin + forward * range,
                Vector3.down,
                out RaycastHit hitDown,
                100,
                chCheckLayers
            );

            curCrosshair.transform.rotation = transform.rotation;

            if (hitInfoDown)
                curCrosshair.transform.position = hitDown.point + Vector3.up * 0.02f;
            else
                curCrosshair.transform.position = origin + forward * range + Vector3.down * 1.5f;
        }
    }

    void SwitchCrosshair(GameObject newCh)
    {
        if (Crosshair) Crosshair.SetActive(newCh == Crosshair);
        if (WallCrosshair) WallCrosshair.SetActive(newCh == WallCrosshair);
        curCrosshair = newCh;
    }

    public WeaponData GetSelectedWeaponData()
    {
        if (pmm == null || pmm.Weapons == null || pmm.Weapons.Length == 0)
            return null;

        int weaponId = GetActiveWeaponId();
        if (weaponId < 0 || weaponId >= pmm.Weapons.Length)
            return null;

        if (pmm.Weapons[weaponId] == null || pmm.Weapons[weaponId].Prefab == null)
            return null;

        return pmm.Weapons[weaponId].Prefab.GetComponent<WeaponData>();
    }
}