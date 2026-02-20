using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    private const float TickRate = 1f / 128f;
    private float tickTimer;
    private float bulletSpeed = 40f;

    void FixedUpdate()
    {
        if (!IsServer) return;
        tickTimer += Time.deltaTime;

        while (tickTimer >= TickRate)
        {
            SimulateTick();
            tickTimer -= TickRate;
        }
    }

    void SimulateTick()
    {
        transform.position += transform.forward * TickRate * bulletSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!IsServer) return;
        if (other.gameObject.CompareTag("Player"))
        {
            ulong hitPlayerId = other.GetComponent<NetworkObject>().OwnerClientId;
            if (hitPlayerId == OwnerClientId) return;
            other.gameObject.GetComponent<PlayerHealth>().decreaseHealth(10, OwnerClientId, hitPlayerId);
        }
        NetworkObject.Despawn(true);
    }
}
