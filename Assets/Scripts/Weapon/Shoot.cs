using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Shoot : NetworkBehaviour
{
    private EquipWeapon equipWeapon;
    private float _timer;

    //ammo
    private TMP_Text ammoCountText;
    private int ammo;
    private int maxAmmo;
    private WeaponData curWD;

    private void Awake()
    {
        equipWeapon = GetComponent<EquipWeapon>();
        if(equipWeapon != null)
        {
            equipWeapon.OnEquipWeapon.AddListener(OnEquipWeapon);
        }
    } 
    void OnEquipWeapon()
    {
        curWD = equipWeapon.GetSelectedWeaponData();
    }

    private void Update()
    {
        if (!IsOwner || !IsClient || !IsSpawned || equipWeapon == null)
            return;

        var localWd = equipWeapon.GetSelectedWeaponData();
        if (localWd == null) return;

        _timer += Time.deltaTime;

        bool shootInput = UserInput.WasShootPressed;
        if (localWd.autofire) shootInput = UserInput.IsShootPressed;

        maxAmmo = localWd.magazineSize;

        if (shootInput && _timer >= localWd.cooldown && ammo > 0)
        {
            ShootServerRpc();
            _timer = 0;
            ammo--;
        }
    }

    [ServerRpc]
    private void ShootServerRpc()
    {
        if (equipWeapon == null) return;

        WeaponData wd = equipWeapon.GetSelectedWeaponDataPrefab();
        if (wd == null) return;

        Transform weaponRoot = equipWeapon.GetWeaponRootServer();
        if (weaponRoot == null) return;

        Vector3 origin = weaponRoot.position;
        Vector3 forward = weaponRoot.forward;

        ShootBulletsServer(wd, origin, forward);

        weaponAnimationClientRpc();
    }

    private void ShootBulletsServer(WeaponData wd, Vector3 origin, Vector3 forward)
    {
        if (wd.bullet == null) return;

        int count = wd.bulletCount;

        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                Quaternion spreadRot = GetRandomSpreadRotationFromForward(forward, wd.bulletSpread);
                Quaternion finalRot = ApplyBloom(spreadRot, wd.bloom);

                SpawnBulletServer(wd, origin, finalRot);
            }
        }
        else
        {
            Quaternion baseRot = Quaternion.LookRotation(forward, Vector3.up);
            Quaternion finalRot = ApplyBloom(baseRot, wd.bloom);
            SpawnBulletServer(wd, origin, finalRot);
        }
    }

    private void SpawnBulletServer(WeaponData wd, Vector3 pos, Quaternion rot)
    {
        GameObject instance = Instantiate(wd.bullet, pos, rot);

        var b = instance.GetComponent<Bullet>();
        if (b != null) b.bulletDamage = wd.damage;

        var net = instance.GetComponent<NetworkObject>();
        if (net != null) net.SpawnWithOwnership(OwnerClientId);
    }

    private static Quaternion GetRandomSpreadRotationFromForward(Vector3 forward, float maxAngleDeg)
    {
        Vector2 p = Random.insideUnitCircle;
        float yaw = p.x * maxAngleDeg;

        Quaternion baseRot = Quaternion.LookRotation(forward, Vector3.up);
        return baseRot * Quaternion.Euler(0f, yaw, 0f);
    }

    private static Quaternion ApplyBloom(Quaternion baseRot, float bloomDeg)
    {
        if (bloomDeg <= 0f) return baseRot;

        Vector2 p = Random.insideUnitCircle * bloomDeg;
        return baseRot * Quaternion.Euler(0f, p.x, 0f);
    }

    [ClientRpc]
    private void weaponAnimationClientRpc()
    {
        if (equipWeapon == null) return;

        try
        {
            if (equipWeapon.activeWeaponInstance != null)
            {
                var weapondata = equipWeapon.activeWeaponInstance.GetComponent<WeaponData>();
                if (weapondata != null && weapondata.bulletSpawn != null && weapondata.MuzzleFlash != null)
                {
                    ParticleSystem inst = Instantiate(
                        weapondata.MuzzleFlash,
                        weapondata.bulletSpawn.position,
                        weapondata.bulletSpawn.rotation
                    );
                    Destroy(inst.gameObject, 0.3f);
                }

                var ap = equipWeapon.activeWeaponInstance.GetComponent<AudioPlayer>();
                if (ap != null) ap.Play("shoot");
            }
        }
        catch { }

        Animator anim = equipWeapon.GetComponentInChildren<Animator>();
        if (anim != null) anim.SetTrigger("shoot");
    }
}