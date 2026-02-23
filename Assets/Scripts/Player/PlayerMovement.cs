using System.Collections.Generic;
using System.Globalization;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
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

    [SerializeField]
    private LayerMask mapLayer;

    [SerializeField]
    public float enemyDetectorRadius;
    private GameObject closestEnemy;

    [SerializeField]
    public LayerMask enemyLayer;
    CinemachineBrain brain;

    private bool IsCamAssigned;

    public override void OnNetworkSpawn()
    {
        AudioListener listener = GetComponent<AudioListener>();
        if (IsOwner)
        {
            //Camera.main.transform.SetParent(camSpawn, false);
            //Camera.main.transform.localPosition = Vector3.zero;
            //Camera.main.transform.localRotation = Quaternion.identity;
            brain = Camera.main.GetComponent<CinemachineBrain>();
            if (brain.ActiveVirtualCamera is CinemachineCamera vcam)
            {
                vcam.Follow = transform;
                vcam.LookAt = transform;
                IsCamAssigned = true;
            }
            //Cursor.lockState = CursorLockMode.Locked;
            listener.enabled = true;
        }
        else
        {
            listener.enabled = false;
        }
    }

    //Workaround for host not getting cam assigned
    private void Update()
    {
        if (brain.ActiveVirtualCamera is CinemachineCamera vcam)
        {
            vcam.Follow = transform;
            vcam.LookAt = transform;
            IsCamAssigned = true;
        }
    }

    public void MovePlayer(
        Vector2 movementInput,
        float yaw,
        float camYaw,
        float tickDelta,
        bool rotateByCam
    )
    {
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        if (movementInput.magnitude >= 0.1f)
        {
            Vector3 inputDir = new Vector3(movementInput.x, 0f, movementInput.y);
            Vector3 targetVelocity = rotateByCam
                ? Quaternion.Euler(0, camYaw, 0) * inputDir.normalized * speed
                : Quaternion.Euler(0, yaw, 0) * inputDir.normalized * speed;
            Vector3 curVelocity = velocity;
            curVelocity.y = 0f;

            Vector3 velocityError = targetVelocity - curVelocity;
            Vector3 acceleration = velocityError * accelerationSpeed * tickDelta;
            velocity += acceleration;
        }
        else
        {
            // damping
            velocity *= 1f / (1f + damping * tickDelta);
        }
        if (velocity.sqrMagnitude < 0.001f)
            velocity = Vector3.zero;

        //wall collide and slide
        Vector3 remaining = velocity * tickDelta;
        Vector3 pos = transform.position;
        Vector3 lastNormal = Vector3.zero;

        float radius = 0.5f;
        float height = 2.5f;

        for (int i = 0; i < 3; i++)
        {
            if (remaining.sqrMagnitude < 0.000001f)
                break;
            Vector3 p1 = new Vector3(pos.x, pos.y - height / 2, pos.z) + Vector3.up * radius;
            Vector3 p2 =
                new Vector3(pos.x, pos.y - height / 2, pos.z) + Vector3.up * (height - radius);

            if (
                Physics.CapsuleCast(
                    p1,
                    p2,
                    radius,
                    remaining.normalized,
                    out RaycastHit hit,
                    remaining.magnitude,
                    mapLayer
                )
            )
            {
                Vector3 wallNormal = hit.normal;
                wallNormal.y = 0f;
                wallNormal.Normalize();

                //move to wall
                float moveDist = Mathf.Max(hit.distance - 0.01f, 0f);
                Vector3 move = remaining.normalized * moveDist;
                pos += move;

                //slide along wall
                remaining -= move;
                if (Vector3.Angle(lastNormal, wallNormal) > 1f)
                    remaining = Vector3.ProjectOnPlane(remaining, wallNormal);

                if (Vector3.Dot(remaining, wallNormal) < 1f)
                    remaining -= wallNormal * Vector3.Dot(remaining, wallNormal);

                //clip velocity
                velocity = Vector3.ProjectOnPlane(velocity, wallNormal);
            }
            else
            {
                pos += remaining;
                break;
            }
        }
        transform.position = pos;
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
