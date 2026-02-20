using System.Globalization;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField]
    private CinemachineCamera cam;

    [SerializeField]
    private AudioListener listener;

    Rigidbody rb;

    [SerializeField]
    private float speed;

    [SerializeField]
    public float rotationSpeed;
    private float accumulatedRotation;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            listener.enabled = true;
            cam.Priority = 1;
            rb = GetComponent<Rigidbody>();
        }
        else
        {
            cam.Priority = 0;
        }
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void movePlayer(Vector2 movementInput, Vector2 lookInput, float _tickRate)
    {
        float rotationAmount = lookInput.x * rotationSpeed * _tickRate;

        accumulatedRotation += rotationAmount;
        transform.rotation = Quaternion.Euler(0, accumulatedRotation, 0);

        transform.Translate(
            movementInput.x * _tickRate * speed,
            0,
            movementInput.y * _tickRate * speed
        );
    }
}
