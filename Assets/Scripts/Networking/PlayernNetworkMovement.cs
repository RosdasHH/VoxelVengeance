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
    private PlayerInput playerInput;
    private InputAction moveAction;

    PlayerMovement playerMovement;

    // private Vector2 movementInput;

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

    [SerializeField]
    float threshold = 0.01f;

    private float accumulatedRotation;

    private void OnEnable()
    {
        // InputActions.FindActionMap("Player").Enable();
        ServerTransformState.OnValueChanged += OnServerStateChange;
    }

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    public override void OnNetworkSpawn()
    {
        if (playerMovement == null)
            playerMovement = GetComponentInChildren<PlayerMovement>();

        if (IsOwner)
        {
            playerInput.enabled = true;
            playerInput.ActivateInput();

            moveAction = playerInput.actions["Move"];
            moveAction.Enable();
            GetComponent<UserInput>().enabled = true;
        }
        else
        {
            playerInput.enabled = false;
        }

        base.OnNetworkSpawn();
    }

    void Update()
    {
        if (IsClient && IsLocalPlayer)
        {
            // movementInput = moveAction.ReadValue<Vector2>();
            Vector2 movementInput = UserInput.MoveInput;
            Vector2 lookInput = UserInput.LookInput;
            ProcessLocalPlayerMovement(movementInput, lookInput);
        }
        else
        {
            ProcessSimulatedPlayerMovement();
        }
    }

    private void OnServerStateChange(TransformState previousValue, TransformState serverState)
    {
        if (!IsLocalPlayer)
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
                .Where(i => i.Tick > serverState.Tick && (i.MovementInput != Vector2.zero || i.LookInput != Vector2.zero))
                .OrderBy(i => i.Tick);

            foreach (var inputState in inputs)
            {
                playerMovement.MovePlayer(
                    inputState.MovementInput,
                    inputState.LookInput,
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
        accumulatedRotation = serverState.Rotation;

        int idx = serverState.Tick % BUFFER_SIZE;
        _transformStates[idx] = serverState;
    }

    public void ProcessLocalPlayerMovement(Vector2 movementInput, Vector2 lookInput)
    {
        _tickDeltaTime += Time.deltaTime;
        while (_tickDeltaTime > _tickRate)
        {
            int bufferIndex = _tick % BUFFER_SIZE;
            MovePlayerServerRpc(_tick, movementInput, lookInput);
            playerMovement.MovePlayer(movementInput, lookInput, _tickRate);
            InputState inputState = new InputState
            {
                Tick = _tick,
                MovementInput = movementInput,
                LookInput = lookInput,
            };

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
    private void MovePlayerServerRpc(int tick, Vector2 movementInput, Vector2 lookInput)
    {
        //if(_tick != _previousTransformState.Tick + 1)
        //{
        //    Debug.Log("Lost a package!");
        //}
        playerMovement.MovePlayer(movementInput, lookInput, _tickRate);
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

            accumulatedRotation = 0f;
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

            accumulatedRotation = 0f;
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
