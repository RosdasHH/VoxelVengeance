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
        if(equipWeapon.getSelectedWeaponData().autofire) shootInput= UserInput.IsShootPressed;
        if (shootInput && _timer >= equipWeapon.getSelectedWeaponData().cooldown)
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
            GameObject bullet = weapondata.bullet;
            Transform spawnPoint = weapondata.bulletSpawn;
            GameObject instance = Instantiate(bullet, spawnPoint.transform.position, bloom(spawnPoint.transform.rotation, weapondata.bloom));
            instance.GetComponent<Bullet>().bulletDamage = equipWeapon.getSelectedWeaponData().damage;
            NetworkObject net = instance.GetComponent<NetworkObject>();
            net.SpawnWithOwnership(OwnerClientId);
            weaponAnimationClientRpc();
        }
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
