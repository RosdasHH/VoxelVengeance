using Unity.Netcode;
using UnityEngine;

public class TeamAssign : NetworkBehaviour
{
    [SerializeField]
    private LayerMask enemyLayer;

    public override void OnNetworkSpawn()
    {
        enemyLayer = LayerMask.NameToLayer("Enemy");
        if (IsOwner) { }
        else
        { // assign to enemy team
            gameObject.layer = enemyLayer;
        }
    }
}
