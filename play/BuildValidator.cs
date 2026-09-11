using System.Numerics;

namespace MyGameBot;

public class BuildValidator
{
    public bool InProgress { get; private set; }
    public float EditCooldown = 0.35f;
    private float _editCd;

    public void Tick(float dt) { if (_editCd > 0f) _editCd -= dt; }

    public void QuadEdit(IGameState s, IActionAPI a)
    {
        if (InProgress || _editCd > 0f) return;
        InProgress = true;

        a.EnterEdit();
        a.SelectEditTile(new Vector2Int(1, 0));
        a.SelectEditTile(new Vector2Int(2, 0));
        a.SelectEditTile(new Vector2Int(1, 1));
        a.SelectEditTile(new Vector2Int(2, 1));
        a.ConfirmEdit();
        a.ExitEdit();

        _editCd    = EditCooldown;
        InProgress = false;
    }

    public void PlaceWall(IGameState s, IActionAPI a, Vector3 world)
    {
        a.SelectWall();
        var cell = s.BuildGrid.SnapToGrid(world, PieceType.Wall);
        if (s.BuildGrid.CanPlace(cell, PieceType.Wall))
        {
            a.PlaceBuild();
            a.ConfirmBuild();
        }
    }

    public void PlaceRamp(IGameState s, IActionAPI a, Vector3 world)
    {
        a.SelectStairs();
        var cell = s.BuildGrid.SnapToGrid(world, PieceType.Ramp);
        if (s.BuildGrid.CanPlace(cell, PieceType.Ramp))
        {
            a.PlaceBuild();
            a.ConfirmBuild();
        }
    }

    public void ResetEdit(IActionAPI a)
    {
        a.EnterEdit();
        a.ResetEdit();
        a.ConfirmEdit();
        a.ExitEdit();
    }
}
