using Unity.Netcode;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private const float TickRate = 1f / 128f;
    private float tickTimer;
    private float bulletSpeed = 40f;

    void Update()
    {
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
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Taking damage");
        }
        Destroy(gameObject);
    }
}
