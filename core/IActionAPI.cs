namespace MyGameBot;

public interface IActionAPI
{
    // Movement
    void MoveForward();
    void MoveBack();
    void MoveLeft();
    void MoveRight();
    void Jump();
    void Sprint(bool on);
    void CrouchSlide(bool hold);
    void AutoRun(bool on);
    void Stop();

    // Combat
    void Fire();
    void ADS(bool on);
    void Reload();
    void Interact();
    void EquipPickaxe();
    void SelectSlot(int slot);

    // Building
    void SelectWall();
    void SelectFloor();
    void SelectStairs();
    void SelectCone();
    void SelectTrap();
    void RotatePiece();
    void ChangeMaterial();
    void PlaceBuild();
    void ConfirmBuild();
    void ResetBuild();

    // Editing
    void EnterEdit();
    void SelectEditTile(Vector2Int cell);
    void ConfirmEdit();
    void ResetEdit();
    void ExitEdit();
}
