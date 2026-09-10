using System;
using System.Collections.Generic;
using System.Numerics;

namespace MyGameBot;

public class GameAdapter : IGameState, IActionAPI
{
    // ---- IGameState ----
    public Vector3 PlayerPosition { get; private set; }
    public Vector3 PlayerVelocity { get; private set; }
    public float   PlayerHealth   { get; set; } = 100f;
    public float   PlayerShield   { get; set; }
    public int     Wood           { get; set; } = 500;
    public int     Brick          { get; set; } = 500;
    public int     Metal          { get; set; } = 500;

    public IReadOnlyList<ITargetable> Enemies   { get; private set; } = Array.Empty<ITargetable>();
    public IReadOnlyList<Vector3>     StormPath { get; private set; } = Array.Empty<Vector3>();

    public bool  IsReloading { get; set; }
    public bool  IsEditing   { get; set; }
    public bool  IsBuilding  { get; set; }
    public int   AmmoInMag   { get; set; } = 30;
    public float WeaponRange { get; set; } = 50f;
    public Vector3 CameraForward { get; set; } = new Vector3(0, 0, -1);

    public IBuildGrid BuildGrid { get; } = new SimpleBuildGrid();

    public bool HasBuildPieceAt(Vector3 pos) => false;
    public PieceType? GetBuildPieceAt(Vector3 pos) => null;

    // ---- IActionAPI — replace each body with a call into your game ----
    public void MoveForward()           => Log("W");
    public void MoveBack()              => Log("S");
    public void MoveLeft()              => Log("A");
    public void MoveRight()             => Log("D");
    public void Jump()                  => Log("Space");
    public void Sprint(bool on)         => Log($"Sprint={on}");
    public void CrouchSlide(bool hold)  => Log($"Crouch={hold}");
    public void AutoRun(bool on)        => Log($"AutoRun={on}");
    public void Stop()                  => Log("Stop");

    public void Fire()                  => Log("LMB Fire");
    public void ADS(bool on)            => Log($"ADS={on}");
    public void Reload()                => Log("R Reload");
    public void Interact()              => Log("E");
    public void EquipPickaxe()          => Log("F Pickaxe");
    public void SelectSlot(int slot)    => Log($"Slot {slot + 1}");

    public void SelectWall()            => Log("Z Wall");
    public void SelectFloor()           => Log("X Floor");
    public void SelectStairs()          => Log("C Stairs");
    public void SelectCone()            => Log("V Cone");
    public void SelectTrap()            => Log("T Trap");
    public void RotatePiece()           => Log("R Rotate");
    public void ChangeMaterial()        => Log("RMB Material");
    public void PlaceBuild()            => Log("LMB Place");
    public void ConfirmBuild()          => Log("Confirm Build");
    public void ResetBuild()            => Log("Reset Build");

    public void EnterEdit()             => Log("G Edit");
    public void SelectEditTile(Vector2Int c) => Log($"Select ({c.X},{c.Y})");
    public void ConfirmEdit()           => Log("Confirm Edit");
    public void ResetEdit()             => Log("RMB Reset Edit");
    public void ExitEdit()              => Log("G Exit Edit");

    private void Log(string msg)
        => System.Diagnostics.Debug.WriteLine($"[Adapter] {msg}");

    public void Update(float dt)
    {
        // TODO: pull real values from your game.
        // PlayerPosition = MyGame.Player.Position;
        // Enemies        = MyGame.EnemyRegistry.Active;
    }

    public (float x, float y) ProjectToScreen(Vector3 world)
    {
        // TODO: use your camera's view-projection to project world -> screen pixels.
        return (0, 0);
    }
}

public class SimpleBuildGrid : IBuildGrid
{
    public Vector3 SnapToGrid(Vector3 world, PieceType kind)
    {
        const float size = 2f;
        return new Vector3(
            MathF.Round(world.X / size) * size,
            MathF.Round(world.Y / size) * size,
            MathF.Round(world.Z / size) * size);
    }
    public bool CanPlace(Vector3 cell, PieceType kind) => true;
}
