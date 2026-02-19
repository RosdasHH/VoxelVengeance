using System;
using Unity.Netcode;
using UnityEngine;

public struct TransformState : INetworkSerializable, IEquatable<TransformState>
{
    public int Tick;
    public Vector3 Position;
    public float Rotation;
    public bool HasStartedMoving;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref Tick);
        serializer.SerializeValue(ref Position);
        serializer.SerializeValue(ref Rotation);
        serializer.SerializeValue(ref HasStartedMoving);
    }

    public bool Equals(TransformState other)
    {
        return Tick == other.Tick
            && Position.Equals(other.Position)
            && Rotation.Equals(other.Rotation)
            && HasStartedMoving == other.HasStartedMoving;
    }

    public override bool Equals(object obj) => obj is TransformState other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(Tick, Position, Rotation, HasStartedMoving);
}
