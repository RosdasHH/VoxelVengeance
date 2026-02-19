using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UIElements;

public class PlayerNetworkMovement : NetworkBehaviour
{
    [SerializeField]
    private float speed;

    [SerializeField]
    private float rotationSpeed;
    private float accumulatedRotation;

    private PlayerInput playerInput;
    private InputAction moveAction;

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

    //Debug
    [SerializeField]
    private MeshFilter _meshFilter;

    private void OnEnable()
    {
        // InputActions.FindActionMap("Player").Enable();
        ServerTransformState.OnValueChanged += OnServerStateChange;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        moveAction.Enable();
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
        TransformState calculatedState = _transformStates.First(localState =>
            localState.Tick == serverState.Tick
        );
        if ((calculatedState.Position - serverState.Position).sqrMagnitude > thresholdSqr)
        {
            Debug.Log("Correcting client position");
            //Teleport to Server position
            TeleportPlayer(serverState);

            //Replay inputs
            IEnumerable<InputState> inputs = _inputStates.Where(input =>
                input.Tick > serverState.Tick
            );
            inputs = from input in inputs orderby input.Tick select input;

            foreach (var inputState in inputs)
            {
                movePlayer(inputState.MovementInput, inputState.LookInput);

                TransformState newTransformState = new TransformState()
                {
                    Tick = inputState.Tick,
                    Position = transform.position,
                    HasStartedMoving = true,
                };

                for (int i = 0; i < _transformStates.Length; i++)
                {
                    if (_transformStates[i].Tick == inputState.Tick)
                    {
                        _transformStates[i] = newTransformState;
                        break;
                    }
                }
            }
        }
    }

    private void TeleportPlayer(TransformState serverState)
    {
        //If we have a Character Controller, we have to deactivate it here before we teleport
        transform.position = serverState.Position;

        for (int i = 0; i < _transformStates.Length; i++)
        {
            if (_transformStates[i].Tick == serverState.Tick)
            {
                _transformStates[i] = serverState;
                break;
            }
        }
    }

    public void ProcessLocalPlayerMovement(Vector2 movementInput, Vector2 lookInput)
    {
        _tickDeltaTime += Time.deltaTime;
        if (_tickDeltaTime > _tickRate)
        {
            int bufferIndex = _tick % BUFFER_SIZE;
            MovePlayerServerRpc(_tick, movementInput, lookInput);
            movePlayer(movementInput, lookInput);
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
        if (_tickDeltaTime > _tickRate)
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
        movePlayer(movementInput, lookInput);
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

    private void movePlayer(Vector2 movementInput, Vector2 lookInput)
    {
        float rotationAmount = lookInput.x * rotationSpeed * _tickRate;

        accumulatedRotation += rotationAmount;
        transform.rotation = Quaternion.Euler(0, accumulatedRotation, 0);
        Debug.Log(accumulatedRotation);

        transform.Translate(
            movementInput.x * _tickRate * speed,
            0,
            movementInput.y * _tickRate * speed
        );
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawMesh(_meshFilter.mesh, ServerTransformState.Value.Position);
    }

    private void OnDisable()
    {
        // InputActions.FindActionMap("Player").Disable();
    }

    private void Awake() { }
}
