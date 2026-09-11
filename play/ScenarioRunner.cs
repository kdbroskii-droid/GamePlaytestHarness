using System.Numerics;

namespace MyGameBot;

public class ScenarioRunner
{
    private readonly IGameState _s;
    private readonly IActionAPI _a;
    private readonly AimTelemetry _aim;
    private readonly Context _ctx = new();

    public bool Enabled     { get; private set; }
    public bool RushEnabled { get; set; } = true;

    public float TickRate = 1f / 60f;
    private float _accum;

    public ScenarioRunner(IGameState s, IActionAPI a, AimTelemetry aim)
    {
        _s = s; _a = a; _aim = aim;
    }

    public void Toggle()
    {
        Enabled = !Enabled;
        if (Enabled) { if (!_aim.Enabled) _aim.Toggle(); }
        else         { _a.Stop(); }
    }

    public void Tick(float dt)
    {
        if (!Enabled) return;
        _accum += dt;
        while (_accum >= TickRate) { _accum -= TickRate; InnerTick(TickRate); }
    }

    private void InnerTick(float dt)
    {
        _ctx.Refresh(_s);

        if (_s.PlayerHealth < 30f) { _a.SelectSlot(3); _a.Fire(); return; }

        if (_ctx.StormDistance < 20f) { RotateToStorm(); return; }

        if (RushEnabled && _ctx.NearestEnemy != null && _ctx.EnemyDistance < 30f && _s.PlayerHealth > 60f)
        {
            Rush(_ctx, dt);
            return;
        }

        if (_ctx.NearestEnemy != null && _ctx.EnemyDistance < 8f) { QuadEdit(); return; }

        if (_ctx.UnderFire && !_ctx.HasCover) { TakeCover(_ctx); return; }

        if (_ctx.NearestEnemy != null) { Engage(_ctx); return; }

        _a.MoveLeft();
    }

    private void RotateToStorm()
    {
        _a.Sprint(true);
        _a.MoveForward();
    }

    private void Rush(Context ctx, float dt)
    {
        var e = ctx.NearestEnemy!;
        float d = ctx.EnemyDistance;

        if (d > 7f)
        {
            _a.SelectStairs();
            _a.MoveForward();
            _a.Sprint(true);
            _a.Jump();
            _a.PlaceBuild();
            _a.ConfirmBuild();

            _a.SelectWall();
            _a.RotatePiece();
            _a.PlaceBuild();
            _a.ConfirmBuild();
        }
        else
        {
            _a.SelectSlot(0);
            _a.Sprint(false);
            _a.ADS(false);

            float angle = AngleTo(e.AimPoint);
            if (angle < 6f) _a.Fire();

            if (((int)(_clock * 2f)) % 2 == 0) _a.MoveLeft();
            else _a.MoveRight();
        }
    }

    private void Engage(Context ctx)
    {
        var e = ctx.NearestEnemy!;
        float d = ctx.EnemyDistance;

        if (d < 7f)       _a.SelectSlot(0);
        else if (d < 45f) _a.SelectSlot(1);
        else              _a.SelectSlot(2);

        _a.ADS(d >= 7f);

        if (_s.IsReloading) return;
        if (_s.AmmoInMag <= 0) { _a.Reload(); return; }

        float angle = AngleTo(e.AimPoint);
        float threshold = d < 7f ? 8f : 4f;
        if (angle < threshold) _a.Fire();
    }

    private void QuadEdit()
    {
        _a.EnterEdit();
        _a.SelectEditTile(new Vector2Int(1, 0));
        _a.SelectEditTile(new Vector2Int(2, 0));
        _a.SelectEditTile(new Vector2Int(1, 1));
        _a.SelectEditTile(new Vector2Int(2, 1));
        _a.ConfirmEdit();
        _a.ExitEdit();
    }

    private void TakeCover(Context ctx)
    {
        var e = ctx.NearestEnemy;
        if (e == null) return;

        Vector3 dir = Vector3.Normalize(e.WorldPosition - _s.PlayerPosition);

        _a.SelectWall();
        var wallCell = _s.BuildGrid.SnapToGrid(_s.PlayerPosition + dir * 2f, PieceType.Wall);
        if (_s.BuildGrid.CanPlace(wallCell, PieceType.Wall))
        {
            _a.PlaceBuild();
            _a.ConfirmBuild();
        }

        _a.SelectStairs();
        var rampCell = _s.BuildGrid.SnapToGrid(_s.PlayerPosition + dir * 1f, PieceType.Ramp);
        if (_s.BuildGrid.CanPlace(rampCell, PieceType.Ramp))
        {
            _a.PlaceBuild();
            _a.ConfirmBuild();
        }
    }

    private float _clock;
    private float AngleTo(Vector3 target)
    {
        Vector3 dir = Vector3.Normalize(target - _s.PlayerPosition);
        float dot = Math.Clamp(Vector3.Dot(_s.CameraForward, dir), -1f, 1f);
        return MathF.Acos(dot) * 180f / MathF.PI;
    }
}
