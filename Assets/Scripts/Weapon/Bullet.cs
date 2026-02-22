using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField] float bulletSpeed;
    [System.NonSerialized] public int bulletDamage;
    [SerializeField] Vector3 minBounds;
    [SerializeField] Vector3 maxBounds;
    [SerializeField] ParticleSystem bulletCollisionParticles;
    [SerializeField] ParticleSystem bulletHitParticles;

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

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            ulong hitPlayerId = collision.gameObject.GetComponent<NetworkObject>().OwnerClientId;
            if (hitPlayerId == OwnerClientId) return;
            collision.gameObject.GetComponent<PlayerHealth>().decreaseHealth(bulletDamage, OwnerClientId, hitPlayerId);
            spawnCollisionParticlesClientRpc(collision.GetContact(0).point, Quaternion.LookRotation(collision.GetContact(0).normal), 'h');
        } else spawnCollisionParticlesClientRpc(collision.GetContact(0).point, Quaternion.LookRotation(collision.GetContact(0).normal), 'c');
        if (NetworkObject.IsSpawned) NetworkObject.Despawn(true);
    }

    [ClientRpc]
    private void spawnCollisionParticlesClientRpc(Vector3 pos, Quaternion rot, char type)
    {
        if (type == 'c')
        {
            ParticleSystem inst = Instantiate(bulletCollisionParticles, pos, rot);
            Destroy(inst.gameObject, 0.3f);
        }
        if (type == 'h')
        {
            ParticleSystem inst = Instantiate(bulletHitParticles, pos, rot);
            Destroy(inst.gameObject, 0.8f);
        }
    }
}