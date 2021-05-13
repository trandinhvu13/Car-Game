using System;
using UnityEngine;

public class GameEvent : MonoBehaviour
{
    #region Singleton

    private static GameEvent _instance;

    public static GameEvent Instance
    {
        get { return _instance; }
    }
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    #endregion

    #region Tile

    public event Action<Vector2Int> OnHighlightAssignedTile;

    public void HighlightAssignedTile(Vector2Int tileID)
    {
        OnHighlightAssignedTile?.Invoke(tileID);
    }
    
    public event Action<Vector2Int> OnUnHighlightAssignedTile;

    public void UnHighlightAssignedTile(Vector2Int tileID)
    {
        OnUnHighlightAssignedTile?.Invoke(tileID);
    }
    public event Action<Vector2Int> OnShowDirectionArrow;

    public void ShowDirectionArrow(Vector2Int tileID)
    {
        OnShowDirectionArrow?.Invoke(tileID);
    }
    public event Action<Vector2Int> OnHideDirectionArrow;

    public void HideDirectionArrow(Vector2Int tileID)
    {
        OnHideDirectionArrow?.Invoke(tileID);
    }
    
    public event Action<Vector2Int,bool> OnChangeCanBeAddedToPath;

    public void ChangeCanBeAddedToPath(Vector2Int tileID,bool canBeAddedToPath)
    {
        OnChangeCanBeAddedToPath?.Invoke(tileID,canBeAddedToPath);
    }

    #endregion

    #region Car

    public event Action<int> OnStartMoving;

    public void StartMoving(int carID)
    {
        OnStartMoving?.Invoke(carID);
    }
    
    public event Action<int> OnStopMoving;

    public void StopMoving(int carID)
    {
        OnStopMoving?.Invoke(carID);
    }

    #endregion
}