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
    private float speed;

    [System.NonSerialized]
    public Vector3 velocity = Vector3.zero;

    [SerializeField]
    private float accelerationSpeed;

    [SerializeField]
    private float damping;

    [SerializeField]
    public float rotationSpeed;
    public float accumulatedRotation;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        AudioListener listener = GetComponent<AudioListener>();
        if (IsOwner)
        {
            Camera.main.transform.SetParent(camSpawn, false);
            Camera.main.transform.localPosition = Vector3.zero;
            Camera.main.transform.localRotation = Quaternion.identity;
            Cursor.lockState = CursorLockMode.Locked;
            listener.enabled = true;
        } else
        {
            listener.enabled = false;
        }
    }

    public void MovePlayer(Vector2 movementInput, Vector2 lookInput, float tickDelta)
    {
        float rotationAmount = lookInput.x * rotationSpeed * tickDelta;

        accumulatedRotation += rotationAmount;
        transform.rotation = Quaternion.Euler(0, accumulatedRotation, 0);

        if (movementInput.magnitude >= 0.1f)
        {
            Vector3 inputDir = new Vector3(movementInput.x, 0f, movementInput.y);
            Vector3 targetVelocity = transform.TransformDirection(inputDir.normalized) * speed;
            Vector3 curVelocity = velocity;
            curVelocity.y = 0f;

            Vector3 velocityError = targetVelocity - curVelocity;
            Vector3 acceleration = velocityError * accelerationSpeed * tickDelta;
            velocity += acceleration;
        }
        else
        {
            // damping
            velocity = Vector3.Lerp(velocity, Vector3.zero, damping * tickDelta);
        }
        if(velocity.sqrMagnitude < 0.001f) velocity = Vector3.zero;
        transform.position += velocity * tickDelta;
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
