using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

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
        if (UserInput.WasShootPressed && _timer >= 0.3)
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
            GameObject instance = Instantiate(bullet, spawnPoint.transform.position, spawnPoint.transform.rotation);
            NetworkObject net = instance.GetComponent<NetworkObject>();
            net.SpawnWithOwnership(OwnerClientId);
            weaponAnimationClientRpc();
        }
    }

    [ClientRpc]
    public void weaponAnimationClientRpc()
    {
        Animator anim = equipWeapon.GetComponentInChildren<Animator>();
        anim.SetTrigger("shoot");
    }
}
