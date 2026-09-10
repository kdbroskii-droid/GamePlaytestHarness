using System.Numerics;

namespace MyGameBot;

public interface ITargetable
{
    Vector3 AimPoint      { get; }   // head bone world position
    Vector3 WorldPosition { get; }   // body / root
    bool    IsAlive       { get; }
}
