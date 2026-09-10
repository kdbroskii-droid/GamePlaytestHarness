using System;
using System.Numerics;

namespace MyGameBot;

public class AimTelemetry
{
    private readonly IGameState _s;
    private readonly IActionAPI _a;

    public bool Enabled     { get; private set; }
    public bool FireEnabled { get; private set; }

    // Circle
    public float LockRadiusPx    = 120f;
    public float PulseSpeed      = 6f;
    public float PulseAmplitudePx = 6f;
    public float GlowWidthPx     = 4f;
    public float GlowAlphaMax    = 0.9f;

    // Targeting
    public float MaxRange = 150f;
    public float MinDot   = 0.85f;

    // Distance-based smoothing
    public bool  HardLock     = true;
    public float SmoothNear   = 30f;
    public float SmoothFar    = 6f;
    public float NearDistance = 10f;
    public float FarDistance  = 120f;

    // Fire
    public float FireCooldown = 0.08f;
    public float FireDelay    = 0f;
    public float FireRadiusPx = 8f;

    public float PulsePhase { get; private set; }
    public ITargetable? LockedTarget { get; private set; }

    private float _currentSmoothing = 15f;
    private float _clock;
    private float _lastFire = -999f;
    private float _fireStart = -1f;

    public AimTelemetry(IGameState s, IActionAPI a) { _s = s; _a = a; }

    public void Toggle()      { Enabled = !Enabled; if (!Enabled) LockedTarget = null; }
    public void ToggleFire()  { FireEnabled = !FireEnabled; }

    public void Tick(float dt)
    {
        _clock += dt;
        PulsePhase += dt * PulseSpeed;
        if (PulsePhase > MathF.PI * 2f) PulsePhase -= MathF.PI * 2f;

        if (!Enabled) { LockedTarget = null; }
        else          { UpdateLock(); }

        if (FireEnabled) UpdateFire();
    }

    private void UpdateLock()
    {
        ITargetable? best = null;
        float bestScore = float.MaxValue;
        float bestDist  = 0f;

        foreach (var t in _s.Enemies)
        {
            if (t == null || !t.IsAlive) continue;
            Vector3 toT = t.AimPoint - _s.PlayerPosition;
            float dist  = toT.Length();
            if (dist > MaxRange) continue;

            Vector3 dir = Vector3.Normalize(toT);
            if (Vector3.Dot(_s.CameraForward, dir) < MinDot) continue;

            float score = dist; // TODO: swap to screen-space distance once ProjectToScreen works
            if (score < bestScore) { bestScore = score; best = t; bestDist = dist; }
        }

        LockedTarget = best;
        if (best == null) return;

        if (HardLock)
        {
            // TODO: rotate camera instantly to best.AimPoint via your controller
        }
        else
        {
            float targetSmoothing = GetSmoothing(bestDist);
            _currentSmoothing += (targetSmoothing - _currentSmoothing) * (1f - MathF.Exp(-8f * 0.016f));
            // TODO: apply smoothed rotation via your controller
        }
    }

    private void UpdateFire()
    {
        ITargetable? onTarget = null;
        foreach (var t in _s.Enemies)
        {
            if (t == null || !t.IsAlive) continue;
            Vector3 toT = t.AimPoint - _s.PlayerPosition;
            Vector3 dir = Vector3.Normalize(toT);
            if (Vector3.Dot(_s.CameraForward, dir) < 0.98f) continue;
            onTarget = t;
            break;
        }

        if (onTarget == null) { _fireStart = -1f; return; }
        if (_fireStart < 0f) _fireStart = _clock;
        if (_clock - _fireStart < FireDelay) return;
        if (_clock - _lastFire  < FireCooldown) return;

        _lastFire = _clock;
        _a.Fire();
    }

    public float GetSmoothing(float d)
    {
        float t = Math.Clamp((d - NearDistance) / MathF.Max(0.0001f, FarDistance - NearDistance), 0f, 1f);
        t = t * t * (3f - 2f * t);
        return SmoothNear + (SmoothFar - SmoothNear) * t;
    }
}
