using System.Linq;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerNetworkMovement : NetworkBehaviour
{
    PlayerMovement playerMovement;

    private int _tick = 0;
    private float _tickRate = 1f / 128f;
    private float _tickDeltaTime = 0f;

    private const int BUFFER_SIZE = 1024;
    private InputState[] _inputStates = new InputState[BUFFER_SIZE];
    private TransformState[] _transformStates = new TransformState[BUFFER_SIZE];

    public NetworkVariable<TransformState> ServerTransformState =
        new NetworkVariable<TransformState>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
    public TransformState _previousTransformState;
    public Vector3 mousePosOnGround;

    bool _lastCamRotate; //used to detect when to capture mouse on manual rotate
    float mouseFromPlayerDist; //used to set cursor to position after manual rotate

    [SerializeField]
    float threshold = 0.01f;

    CinemachineBrain brain;

    private void OnEnable()
    {
        ServerTransformState.OnValueChanged += OnServerStateChange;
    }

    public override void OnNetworkSpawn()
    {
        if (playerMovement == null)
            playerMovement = GetComponentInChildren<PlayerMovement>();
        brain = Camera.main.GetComponent<CinemachineBrain>();
        if (IsHost) TeleportTo(GameObject.FindGameObjectWithTag("SpawnPoints").GetComponent<Spawn>().getRandomSpawnPosition());

        base.OnNetworkSpawn();
    }

    private float CalculatePlayerRotation(Vector2 lookInput)
    {
        Camera cam = Camera.main;
        Ray ray = cam.ScreenPointToRay(lookInput);
        if (Physics.Raycast(ray, out var camHit, 500f, LayerMask.GetMask("Ground")))
        {
            mousePosOnGround = camHit.point;
            Vector3 direction = camHit.point - transform.position;
            direction.y = 0f;
            Quaternion rotation = Quaternion.LookRotation(direction);
            float yaw = rotation.eulerAngles.y;
            return yaw;
        }
        return 0;
    }

    GameObject UpdateNearbyEnemies()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            playerMovement.enemyDetectorRadius,
            playerMovement.enemyLayer
        );

        GameObject closestEnemy = null;
        float closestDist = Mathf.Infinity;
        foreach (Collider hit in hits)
        {
            Vector3 dist = hit.gameObject.transform.position - transform.position;
            if (dist.magnitude < closestDist)
            {
                closestDist = dist.magnitude;
                closestEnemy = hit.gameObject;
            }
        }

        return closestEnemy;
    }

    float RotateCamFocus(Vector3 towards)
    {
        if (brain.ActiveVirtualCamera is not CinemachineCamera vcam)
            return 0;

        Vector3 direction = towards - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            float absoluteYaw = targetRotation.eulerAngles.y;

            vcam.gameObject.transform.rotation = Quaternion.Euler(
                vcam.gameObject.transform.eulerAngles.x,
                absoluteYaw,
                0f
            );

            return absoluteYaw;
        }

        return vcam.transform.eulerAngles.y;
    }

    private float RotateCam(Vector2 lookDelta, float tickDelta)
    {
        if (brain.ActiveVirtualCamera is not CinemachineCamera vcam)
            return 0;

        float absoluteYaw = vcam.gameObject.transform.eulerAngles.y;
        absoluteYaw += lookDelta.x * playerMovement.rotationSpeed * tickDelta;
        vcam.gameObject.transform.rotation = Quaternion.Euler(
            vcam.gameObject.transform.eulerAngles.x,
            absoluteYaw,
            0f
        );
        return absoluteYaw;
    }

    void Update() // make player movement based on cam while right-click && make cursor rotate properly
    {
        if (IsClient && IsLocalPlayer)
        {
            Vector2 movementInput = UserInput.MoveInput;
            Vector2 lookInput = UserInput.LookInput;
            Vector2 lookDelta = UserInput.LookDeltaInput;
            bool _camRotate = UserInput.IsCamRotatePressed;

            float currentCamYaw = 0f;
            float rotateAngle = 0f;
            if (brain != null && brain.ActiveVirtualCamera is CinemachineCamera vcam)
            {
                GameObject focusEnemy = UpdateNearbyEnemies();

                if (_camRotate) //rotating cam manually -> in ProcessSimulatedPlayerMovement
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    rotateAngle = RotateCam(lookDelta, _tickRate);
                }
                else if (focusEnemy && UserInput.WasCamFocusPressed) // focussing cam on enemy
                {
                    Cursor.lockState = CursorLockMode.Confined;
                    rotateAngle = RotateCamFocus(focusEnemy.transform.position);
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Confined;
                }
                currentCamYaw = vcam.transform.eulerAngles.y + rotateAngle;
            }
            if (_lastCamRotate != _camRotate)
            {
                HandleManualRotationSwitch(_camRotate, lookInput);
            }
            _lastCamRotate = _camRotate;
            ProcessLocalPlayerMovement(
                movementInput,
                _camRotate ? rotateAngle : CalculatePlayerRotation(lookInput), //player yaw
                currentCamYaw,
                !_camRotate
            );
        }
        else
        {
            ProcessSimulatedPlayerMovement();
        }
    }
    void HandleManualRotationSwitch(bool _camRotate, Vector2 lookInput)
    {
        var localPlayer = NetworkManager.Singleton?.LocalClient?.PlayerObject;
        if (localPlayer)
        {
            EquipWeapon ew = localPlayer.GetComponent<EquipWeapon>();
            if (_camRotate) //capture mouse -> switch to manual rotation
            {
                //dist to player and send to Equip Weapon
                if (localPlayer)
                {
                    //dist to player
                    Ray ray = Camera.main.ScreenPointToRay(lookInput);
                    if (Physics.Raycast(ray, out var camHit, 500f, LayerMask.GetMask("Ground")))
                    {
                        mouseFromPlayerDist = (
                            camHit.point - localPlayer.transform.position
                        ).magnitude;
                        ew.rangeOnManualRotation = mouseFromPlayerDist;
                    }
                }
            }
            else //free mouse
            {
                ew.rangeOnManualRotation = null;
                //mouse to crosshair
                Vector3 screenPos = Camera.main.WorldToScreenPoint(
                    localPlayer.transform.position
                        + localPlayer.transform.forward * mouseFromPlayerDist
                );
                Mouse.current.WarpCursorPosition(new Vector2(screenPos.x, screenPos.y));
            }
        }
    }

    

    private void OnServerStateChange(TransformState previousValue, TransformState serverState)
    {
        if (!IsLocalPlayer || IsHost)
            return;

        _previousTransformState = serverState;

        float thresholdSqr = threshold * threshold;
        int bufferIndex = serverState.Tick % BUFFER_SIZE;
        TransformState calculatedState = _transformStates[bufferIndex];
        if (!calculatedState.HasStartedMoving || calculatedState.Tick != serverState.Tick)
        {
            TeleportPlayer(serverState);
            return;
        }
        if ((calculatedState.Position - serverState.Position).sqrMagnitude > thresholdSqr) //ADD VELOCITY THRESHOLD
        {
            Debug.Log("Correcting client position");
            //Teleport to Server position
            TeleportPlayer(serverState);

            //Replay inputs
            var inputs = _inputStates
                .Where(i =>
                    i.Tick > serverState.Tick && (i.MovementInput != Vector2.zero || i.yaw != 0)
                )
                .OrderBy(i => i.Tick);

            foreach (var inputState in inputs)
            {
                playerMovement.MovePlayer(
                    inputState.MovementInput,
                    inputState.yaw,
                    inputState.CamYaw,
                    _tickRate,
                    inputState.RotateByCam
                );

                TransformState newTransformState = new TransformState()
                {
                    Tick = inputState.Tick,
                    Position = transform.position,
                    Velocity = playerMovement.velocity,
                    HasStartedMoving = true,
                };

                int idx = inputState.Tick % BUFFER_SIZE;
                _transformStates[idx] = newTransformState;
            }
        }
    }

    public void TeleportPlayer(TransformState serverState)
    {
        transform.position = serverState.Position;
        playerMovement.velocity = serverState.Velocity;
        transform.rotation = Quaternion.Euler(0, serverState.Rotation, 0);
        playerMovement.accumulatedRotation = serverState.Rotation;

        int idx = serverState.Tick % BUFFER_SIZE;
        _transformStates[idx] = serverState;
    }

    public void ProcessLocalPlayerMovement(Vector2 movementInput, float yaw, float camYaw, bool rotateByCam)
    {
        _tickDeltaTime += Time.deltaTime;
        while (_tickDeltaTime > _tickRate)
        {
            int bufferIndex = _tick % BUFFER_SIZE;

            MovePlayerServerRpc(_tick, movementInput, yaw, camYaw, rotateByCam);

            InputState inputState = new InputState
            {
                Tick = _tick,
                MovementInput = movementInput,
                yaw = yaw,
                CamYaw = camYaw,
                RotateByCam = rotateByCam
            };

            if (!IsServer)
            {
                playerMovement.MovePlayer(movementInput, yaw, camYaw, _tickRate, rotateByCam);
            }

            TransformState transformState = new TransformState()
            {
                Tick = _tick,
                Position = transform.position,
                Velocity = playerMovement.velocity,
                Rotation = transform.eulerAngles.y,
                HasStartedMoving = true,
            };

            _inputStates[bufferIndex] = inputState;
            _transformStates[bufferIndex] = transformState;

            _tickDeltaTime -= _tickRate;
            _tick++;
        }
    }

    public void ProcessSimulatedPlayerMovement()
    {
        _tickDeltaTime += Time.deltaTime;
        while (_tickDeltaTime > _tickRate)
        {
            if (ServerTransformState.Value.HasStartedMoving)
            {
                transform.position = ServerTransformState.Value.Position;
                transform.rotation = Quaternion.Euler(0, ServerTransformState.Value.Rotation, 0);
            }
            _tickDeltaTime -= _tickRate;
            _tick++;
        }
    }

    [ServerRpc]
    private void MovePlayerServerRpc(int tick, Vector2 movementInput, float yaw, float camYaw, bool rotateByCam)
    {
        //if(_tick != _previousTransformState.Tick + 1)
        //{
        //    Debug.Log("Lost a package!");
        //}
        playerMovement.MovePlayer(movementInput, yaw, camYaw, _tickRate, rotateByCam);
        TransformState state = new TransformState()
        {
            Tick = tick,
            Position = transform.position,
            Rotation = transform.eulerAngles.y,
            HasStartedMoving = true,
        };
        _previousTransformState = ServerTransformState.Value;
        ServerTransformState.Value = state;
    }

    private void OnDisable()
    {
        ServerTransformState.OnValueChanged -= OnServerStateChange;
    }

    public void TeleportTo(Vector3 pos)
    {
        if (IsServer)
        {
            transform.position = pos;

            playerMovement.accumulatedRotation = 0f;
            transform.rotation = Quaternion.identity;

            int newTick = ServerTransformState.Value.HasStartedMoving
                ? ServerTransformState.Value.Tick + 1
                : 0;

            TransformState state = new TransformState()
            {
                Tick = newTick,
                Position = pos,
                Velocity = Vector3.zero,
                Rotation = 0f,
                HasStartedMoving = true,
            };

            _previousTransformState = ServerTransformState.Value;
            ServerTransformState.Value = state;

            int idx = state.Tick % BUFFER_SIZE;
            _transformStates[idx] = state;
        }

        if (IsLocalPlayer)
        {
            for (int i = 0; i < BUFFER_SIZE; i++)
            {
                _inputStates[i] = default;
                _transformStates[i] = default;
            }

            playerMovement.accumulatedRotation = 0f;
            transform.position = pos;
            transform.rotation = Quaternion.identity;

            TransformState local = new TransformState()
            {
                Tick = ServerTransformState.Value.Tick,
                Position = pos,
                Velocity = Vector3.zero,
                Rotation = 0f,
                HasStartedMoving = true,
            };

            int idx = local.Tick % BUFFER_SIZE;
            _transformStates[idx] = local;
        }
    }
}
