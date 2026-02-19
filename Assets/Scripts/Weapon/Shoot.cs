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
        if(shootAction.WasPressedThisFrame())
        {
            Debug.Log("Shoot"); //Ownership problems
        }
    }
}
