using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class Shoot : NetworkBehaviour
{
    private EquipWeapon equipWeapon;
    private float _timer;

    private void Awake()
    {
        equipWeapon = GetComponent<EquipWeapon>();
    }

    private void Update()
    {
        if (!IsOwner || !IsClient || equipWeapon == null)
            return;
        _timer += Time.deltaTime;
        bool shootInput = UserInput.WasShootPressed;
        if(equipWeapon.GetSelectedWeaponData().autofire) shootInput= UserInput.IsShootPressed;
        if (shootInput && _timer >= equipWeapon.GetSelectedWeaponData().cooldown)
        {
            shoot();
            _timer = 0;
        }
    }

    private void shoot()
    {
        ShootServerRpc();
    }

    [ServerRpc]
    public void ShootServerRpc()
    {
        WeaponData weapondata = null;
        try
        {
            weapondata = equipWeapon.activeWeaponInstance.GetComponent<WeaponData>();
        } catch { }
        if (weapondata != null)
        {
            shootBullets(weapondata);

            weaponAnimationClientRpc();
        }
    }
    public static Quaternion GetRandomSpreadRotation(
        Transform origin,
        float maxAngle)
    {
        Vector2 randomPoint = Random.insideUnitCircle;

        float yaw = randomPoint.x * maxAngle;
        float pitch = randomPoint.y * maxAngle;

        return origin.rotation * Quaternion.Euler(pitch, yaw, 0f);
    }

    private void shootBullets(WeaponData weapondata)
    {
        GameObject bullet = weapondata.bullet;
        Transform spawnPoint = weapondata.bulletSpawn;
        if(weapondata.bulletCount > 0)
        {
            for (int i = 0; i < weapondata.bulletCount; i++)
            {
                shootSingleBullet(bullet, spawnPoint.transform.position, GetRandomSpreadRotation(spawnPoint.transform, weapondata.bulletSpread), weapondata.bloom);
            }
        } else
        {
            shootSingleBullet(bullet, spawnPoint.transform.position, spawnPoint.transform.rotation, weapondata.bloom);
        }
    }

    private void shootSingleBullet(GameObject bullet, Vector3 position, Quaternion rotation, float bloomIntensity)
    {
        GameObject instance = Instantiate(bullet, position, bloom(rotation, bloomIntensity));
        instance.GetComponent<Bullet>().bulletDamage = equipWeapon.GetSelectedWeaponData().damage;
        NetworkObject net = instance.GetComponent<NetworkObject>();
        net.SpawnWithOwnership(OwnerClientId);
    }

    private Quaternion bloom(Quaternion rot, float bloomAmount)
    {
        float x = rot.eulerAngles.x;
        float y = rot.eulerAngles.y;
        float z = rot.eulerAngles.z;

        float randomY = Random.Range(y-bloomAmount, y+bloomAmount);

        Quaternion bloomRot = Quaternion.Euler(0, randomY, 0);
        return bloomRot;
    }

    [ClientRpc]
    public void weaponAnimationClientRpc()
    {
        WeaponData weapondata = null;
        try
        {
            weapondata = equipWeapon.activeWeaponInstance.GetComponent<WeaponData>();
            ParticleSystem inst = Instantiate(weapondata.MuzzleFlash, weapondata.bulletSpawn.transform.position, weapondata.bulletSpawn.transform.rotation);
            Destroy(inst.gameObject, 0.3f);
            equipWeapon.activeWeaponInstance.GetComponent<AudioPlayer>().Play("shoot");
        }
        catch { }
        Animator anim = equipWeapon.GetComponentInChildren<Animator>();
        anim.SetTrigger("shoot");
    }
}
