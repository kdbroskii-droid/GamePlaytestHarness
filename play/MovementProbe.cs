
using System.Numerics;

namespace MyGameBot;

public class MovementProbe
{
    public float StormSafetyMargin = 15f;

    public void RotateToStorm(IGameState s, IActionAPI a, Context ctx)
    {
        Vector3 dir  = Vector3.Normalize(ctx.StormCenter - s.PlayerPosition);
        Vector3 dest = ctx.StormCenter - dir * StormSafetyMargin;

        a.Sprint(true);
        a.MoveForward();
        a.SelectStairs();
        _ = dest; // TODO: steer toward dest via your pathfinder
    }

    public void Strafe(IActionAPI a, bool left)
    {
        if (left) a.MoveLeft(); else a.MoveRight();
    }

    public void HighGroundRetake(IGameState s, IActionAPI a, ITargetable enemy)
    {
        Vector3 dir = Vector3.Normalize(enemy.WorldPosition - s.PlayerPosition);
        a.SelectStairs();
        a.MoveForward();
        a.Sprint(true);
        a.Jump();
        a.PlaceBuild();
        a.ConfirmBuild();
    }
}
