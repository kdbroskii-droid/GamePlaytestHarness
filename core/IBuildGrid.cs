using System.Numerics;

namespace MyGameBot;

public interface IBuildGrid
{
    Vector3 SnapToGrid(Vector3 world, PieceType kind);
    bool    CanPlace(Vector3 cell, PieceType kind);
}
