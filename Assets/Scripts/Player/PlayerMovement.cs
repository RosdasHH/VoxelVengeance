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
    private float accelerationSpeed;

    [SerializeField]
    public float rotationSpeed;
    private float accumulatedRotation;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        if (IsOwner)
        {
            Instantiate(camPrefab, camSpawn);
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void MovePlayer(Vector2 movementInput, Vector2 lookInput)
    {
        float deltaTime = Time.fixedDeltaTime;
        float rotationAmount = lookInput.x * rotationSpeed * deltaTime;

        accumulatedRotation += rotationAmount;
        transform.rotation = Quaternion.Euler(0, accumulatedRotation, 0);

        if (movementInput.magnitude >= 0.1f)
        {
            Vector3 targetVelocity =
                new Vector3(movementInput.x, 0f, movementInput.y).normalized * speed;
            Vector3 curVelocity = rb.linearVelocity;
            curVelocity.y = 0f;

            Vector3 velocityError = targetVelocity - curVelocity;
            Vector3 acceleration = velocityError * accelerationSpeed * deltaTime;
            rb.AddForce(acceleration, ForceMode.Acceleration);
        }

        // transform.Translate(
        //     movementInput.x * speed * deltaTime,
        //     0,
        //     movementInput.y * speed * deltaTime
        // );
    }
    public void setSensitivity(float value)
    {
        rotationSpeed = value;
        setSensitivityServerRpc(value);
    }
    [ServerRpc]
    public void setSensitivityServerRpc(float value)
    {
        rotationSpeed = value;
    }
}
