using System.Numerics;

namespace MyGameBot;

public class Context
{
    public ITargetable? NearestEnemy  { get; private set; }
    public float        EnemyDistance { get; private set; }
    public float        ThreatLevel   { get; private set; }
    public Vector3      StormCenter   { get; private set; }
    public float        StormDistance { get; private set; }
    public bool         UnderFire     { get; private set; }
    public bool         HasCover      { get; private set; }
    public bool         LowHealth     { get; private set; }
    public bool         LowMats       { get; private set; }
    public Vector3      PlayerPos     { get; private set; }

    public void Refresh(IGameState s)
    {
        PlayerPos = s.PlayerPosition;

        NearestEnemy = null;
        float best = float.MaxValue;
        foreach (var e in s.Enemies)
        {
            if (e == null || !e.IsAlive) continue;
            float d = Vector3.Distance(s.PlayerPosition, e.WorldPosition);
            if (d < best) { best = d; NearestEnemy = e; }
        }
        EnemyDistance = NearestEnemy != null ? best : float.MaxValue;

        StormCenter   = s.StormPath.Count > 0 ? s.StormPath[0] : Vector3.Zero;
        StormDistance = Vector3.Distance(s.PlayerPosition, StormCenter);

        float t = 0f;
        if (NearestEnemy != null)
        {
            t += Math.Clamp(1f - EnemyDistance / 50f, 0f, 1f) * 0.6f;
            t += (1f - s.PlayerHealth / 100f) * 0.4f;
        }
        ThreatLevel = Math.Clamp(t, 0f, 1f);
        UnderFire   = ThreatLevel > 0.5f;
        LowHealth   = s.PlayerHealth < 35f;
        LowMats     = s.Wood < 50 || s.Brick < 50 || s.Metal < 50;
        HasCover    = false;
    }
}
