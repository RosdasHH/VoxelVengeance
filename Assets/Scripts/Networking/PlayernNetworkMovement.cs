using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UIElements;

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

    [SerializeField]
    float threshold = 0.01f;

    private void OnEnable()
    {
        ServerTransformState.OnValueChanged += OnServerStateChange;
    }

    public override void OnNetworkSpawn()
    {
        if (playerMovement == null)
            playerMovement = GetComponentInChildren<PlayerMovement>();
        base.OnNetworkSpawn();
    }

    private float CalculateRotatation(Vector2 lookInput)
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

    void Update()
    {
        if (IsClient && IsLocalPlayer)
        {
            Vector2 movementInput = UserInput.MoveInput;
            Vector2 lookInput = UserInput.LookInput;
            ProcessLocalPlayerMovement(movementInput, CalculateRotatation(lookInput));
        }
        else
        {
            ProcessSimulatedPlayerMovement();
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
                .Where(i => i.Tick > serverState.Tick && (i.MovementInput != Vector2.zero || i.yaw != 0))
                .OrderBy(i => i.Tick);

            foreach (var inputState in inputs)
            {
                playerMovement.MovePlayer(
                    inputState.MovementInput,
                    inputState.yaw,
                    _tickRate
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

    public void ProcessLocalPlayerMovement(Vector2 movementInput, float yaw)
    {
        _tickDeltaTime += Time.deltaTime;
        while (_tickDeltaTime > _tickRate)
        {
            int bufferIndex = _tick % BUFFER_SIZE;
            MovePlayerServerRpc(_tick, movementInput, yaw);
            InputState inputState = new InputState
            {
                Tick = _tick,
                MovementInput = movementInput,
                yaw = yaw,
            };

            if(!IsServer)
            {
                playerMovement.MovePlayer(movementInput, yaw, _tickRate);
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
    private void MovePlayerServerRpc(int tick, Vector2 movementInput, float yaw)
    {
        //if(_tick != _previousTransformState.Tick + 1)
        //{
        //    Debug.Log("Lost a package!");
        //}
        playerMovement.MovePlayer(movementInput, yaw, _tickRate);
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
