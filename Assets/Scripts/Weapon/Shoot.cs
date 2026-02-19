using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Shoot : NetworkBehaviour
{
    [SerializeField]
    private GameObject Bullet;

    [SerializeField]
    Transform BulletSpawnPoint;

    private EquipWeapon equipWeapon;

    private void Awake()
    {
        equipWeapon = GetComponent<EquipWeapon>();
    }

    private void Update()
    {
        if (!equipWeapon.IsOwner)
            return;
        if (UserInput.WasShootPressed)
        {
            shoot();
        }
    }

    private void shoot()
    {
        instantiation();
        ShootServerRpc();
    }

    [ServerRpc]
    public void ShootServerRpc()
    {
        instantiation();
        SpawnBulletClientRpc();
    }

    [ClientRpc]
    public void SpawnBulletClientRpc()
    {
        if (IsOwner)
            return;
        instantiation();
    }

    private void instantiation()
    {
        Instantiate(
            Bullet,
            BulletSpawnPoint.transform.position,
            BulletSpawnPoint.transform.rotation
        );
    }
}
