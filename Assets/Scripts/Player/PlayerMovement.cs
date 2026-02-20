using System.Globalization;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    Rigidbody rb;

    [SerializeField]
    private Transform camSpawn;

    [SerializeField]
    private Camera camPrefab;

    [SerializeField]
    private float speed;

    [SerializeField]
    public float rotationSpeed;
    private float accumulatedRotation;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            rb = GetComponent<Rigidbody>();
            Instantiate(camPrefab, camSpawn);
        }
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void MovePlayer(Vector2 movementInput, Vector2 lookInput)
    {
        float deltaTime = Time.fixedDeltaTime;
        float rotationAmount = lookInput.x * rotationSpeed * deltaTime;

        accumulatedRotation += rotationAmount;
        transform.rotation = Quaternion.Euler(0, accumulatedRotation, 0);

        transform.Translate(
            movementInput.x * speed * deltaTime,
            0,
            movementInput.y * speed * deltaTime
        );
    }
}
