using System.Collections.Generic;
using System.Numerics;

namespace MyGameBot;

public interface IGameState
{
    Vector3 PlayerPosition { get; }
    Vector3 PlayerVelocity { get; }
    float   PlayerHealth   { get; }
    float   PlayerShield   { get; }
    int     Wood           { get; }
    int     Brick          { get; }
    int     Metal          { get; }

    IReadOnlyList<ITargetable> Enemies   { get; }
    IReadOnlyList<Vector3>     StormPath { get; }

    bool  IsReloading { get; }
    bool  IsEditing   { get; }
    bool  IsBuilding  { get; }
    int   AmmoInMag   { get; }
    float WeaponRange { get; }

    Vector3 CameraForward { get; }

    IBuildGrid BuildGrid { get; }
    bool HasBuildPieceAt(Vector3 pos);
    PieceType? GetBuildPieceAt(Vector3 pos);
}
