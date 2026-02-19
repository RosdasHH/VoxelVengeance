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
    }

    void FixedUpdate()
    {
        Vector2 moveValue = UserInput.MoveInput;
        Vector3 direction = new Vector3(moveValue.x, 0f, moveValue.y).normalized;
        Vector3 acceleration = Vector3.zero;

        if (direction.magnitude >= .1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        }
    }
}
