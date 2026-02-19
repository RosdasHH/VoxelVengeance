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

    private InputAction shootAction;
    private void Awake()
    {
        shootAction = InputSystem.actions.FindAction("Attack");
        equipWeapon = GetComponent<EquipWeapon>();
    }
    private void Update()
    {
        if (!equipWeapon.IsOwner) return;
        if(shootAction.WasPressedThisFrame())
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
    }

    private void instantiation()
    {
        Instantiate(Bullet, BulletSpawnPoint.transform.position, BulletSpawnPoint.transform.rotation);
    }
}
