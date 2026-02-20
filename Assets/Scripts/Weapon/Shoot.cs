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
        ShootServerRpc();
    }

    [ServerRpc]
    public void ShootServerRpc()
    {
        GameObject instance = Instantiate(Bullet, BulletSpawnPoint.transform.position, BulletSpawnPoint.transform.rotation);
        NetworkObject net = instance.GetComponent<NetworkObject>();
        net.SpawnWithOwnership(OwnerClientId);
    }
}
