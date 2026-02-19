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

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField]
    private float speed;
    [SerializeField]
    private InputActionAsset InputActions;
    private InputAction moveAction;
    private Vector2 movementInput;

    private int _tick = 0;
    private float _tickRate = 1f/128f;
    private float _tickDeltaTime = 0f;

    private const int BUFFER_SIZE = 1024;
    private InputState[] _inputStates = new InputState[BUFFER_SIZE];
    private TransformState[] _transformStates = new TransformState[BUFFER_SIZE];

    public NetworkVariable<TransformState> ServerTransformState = new NetworkVariable<TransformState>();
    public TransformState _previousTransformState;

    //Debug
    [SerializeField]
    private MeshFilter _meshFilter;

    private void OnEnable()
    {
        InputActions.FindActionMap("Player").Enable();
        ServerTransformState.OnValueChanged += OnServerStateChange;
    }

    private void OnServerStateChange(TransformState previousValue, TransformState serverState)
    {
        if (!IsLocalPlayer) return;

        if(_previousTransformState.Position == null)
        {
            _previousTransformState = serverState;
        }

        TransformState calculatedState = _transformStates.First(localState => localState.Tick == serverState.Tick);
        if (calculatedState.Position != serverState.Position)
        {
            Debug.Log("Correcting client position");
            //Teleport to Server position
            TeleportPlayer(serverState);

            //Replay inputs
            IEnumerable<InputState> inputs = _inputStates.Where(input => input.Tick > serverState.Tick);
            inputs = from input in inputs orderby input.Tick select input;

            foreach (var inputState in inputs)
            {
                movePlayer(inputState.MovementInput);

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

    public void ProcessLocalPlayerMovement(Vector2 movementInput)
    {
        _tickDeltaTime += Time.deltaTime;
        if (_tickDeltaTime > _tickRate)
        {
            int bufferIndex = _tick % BUFFER_SIZE;
            MovePlayerServerRpc(_tick, movementInput);
            movePlayer(movementInput);
            InputState inputState = new InputState
            {
                Tick = _tick,
                MovementInput = movementInput
            };

            TransformState transformState = new TransformState()
            {
                Tick = _tick,
                Position = transform.position,
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
        if(_tickDeltaTime > _tickRate)
        {
            if(ServerTransformState.Value.HasStartedMoving)
            {
                transform.position = ServerTransformState.Value.Position;
            }
            _tickDeltaTime -= _tickRate;
            _tick++;
        }
    }


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }
    
    void Update()
    {
        if(IsClient && IsLocalPlayer)
        {
            movementInput = moveAction.ReadValue<Vector2>();
            ProcessLocalPlayerMovement(movementInput);
        } else
        {
            ProcessSimulatedPlayerMovement();
        }
    }

    [ServerRpc]
    private void MovePlayerServerRpc(int tick, Vector2 movementInput)
    {
        //if(_tick != _previousTransformState.Tick + 1)
        //{
        //    Debug.Log("Lost a package!");
        //}
        movePlayer(movementInput);
        TransformState state = new TransformState()
        {
            Tick = tick,
            Position = transform.position,
            HasStartedMoving = true
        };
        _previousTransformState = ServerTransformState.Value;
        ServerTransformState.Value = state;
    }

    private void movePlayer(Vector2 movementInput)
    {
        transform.Translate(movementInput.x * _tickRate * speed, 0, movementInput.y * _tickRate * speed);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawMesh(_meshFilter.mesh, ServerTransformState.Value.Position);
    }

    private void OnDisable()
    {
        InputActions.FindActionMap("Player").Disable();
    }

    private void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Move");
    }
}
