using System.Numerics;

namespace MyGameBot;

public class ResourceProbe
{
    public void Farm(IGameState s, IActionAPI a, Context ctx)
    {
        a.MoveForward();
        a.EquipPickaxe();
        _ = ctx;
    }
}
