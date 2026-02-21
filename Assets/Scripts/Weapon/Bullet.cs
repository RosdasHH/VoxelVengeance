using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    private float bulletSpeed = 40f;
    [SerializeField] Vector3 minBounds;
    [SerializeField] Vector3 maxBounds;

    void FixedUpdate()
    {
        if(IsServer) transform.position += transform.forward * bulletSpeed * Time.fixedDeltaTime;
            Vector3 pos = transform.position;

    if (pos.x < minBounds.x || pos.x > maxBounds.x ||
        pos.z < minBounds.z || pos.z > maxBounds.z)
    {
        NetworkObject.Despawn(true);
    }
    }

    private void Update()
    {
        if(IsClient && !IsHost) transform.position += transform.forward * bulletSpeed * Time.deltaTime;
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
        if(NetworkObject.IsSpawned) NetworkObject.Despawn(true);
    }
}
