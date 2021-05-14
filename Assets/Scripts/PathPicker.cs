using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathPicker : MonoBehaviour
{
    #region Singleton

    private static PathPicker _instance;

    public static PathPicker Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    #endregion

    #region Variables

    [SerializeField] private int currentSelectedCar;
    private Vector2Int currentTileHasArrow = new Vector2Int(-1, -1);
    private List<Vector2Int> selectedCarPath;

    #endregion

    #region Methods

    #region Path

    public void UpdateTileStatus()
    {
        selectedCarPath = CarManager.Instance.cars[currentSelectedCar].GetCurrentPath();
        ShowAssignedPath();
        SetAvailablePath();
        if (selectedCarPath.Count > 0)
        {
            foreach (Vector2Int tileID in selectedCarPath)
            {
                TilesManager.Instance.MakeTilesRemovableFromPath(tileID);
            }
        }
    }

    public void OnChangeToPath()
    {
        TilesManager.Instance.ResetTilePathStatus();
        UpdateTileStatus();
    }

    public void ShowAssignedPath()
    {
        for (int tileID = 0; tileID < selectedCarPath.Count; tileID++)
        {
            GameEvent.Instance.HighlightAssignedTile(selectedCarPath[tileID]);
        }
    }

    public void ResetAllHighlight()
    {
        for (int x = 0; x < 23; x++)
        {
            for (int y = 0; y < 11; y++)
            {
                Vector2Int tileID = new Vector2Int(x, y);
                GameEvent.Instance.UnHighlightAssignedTile(tileID);
            }
        }
    }

    public void SetAvailablePath()
    {
        HideCurrentAvailablePathArrow();
        Car selectedCar = CarManager.Instance.cars[currentSelectedCar];
        List<Vector2Int> path = selectedCar.GetCurrentPath();
        if (path.Count <= 0)
        {
            currentTileHasArrow = selectedCar.GetCurrentTileID();
            ShowDirectionArrow(currentTileHasArrow);
        }
        else
        {
            currentTileHasArrow = path[path.Count - 1];
            ShowDirectionArrow(currentTileHasArrow);
        }
    }

    public void ShowDirectionArrow(Vector2Int currentTileHasArrow)
    {
        string[] arrows = new string[4];
        Vector2Int[] surroundTile =
        {
            new Vector2Int(currentTileHasArrow.x - 1, currentTileHasArrow.y),
            new Vector2Int(currentTileHasArrow.x + 1, currentTileHasArrow.y),
            new Vector2Int(currentTileHasArrow.x, currentTileHasArrow.y + 1),
            new Vector2Int(currentTileHasArrow.x, currentTileHasArrow.y - 1)
        };
        for (int i = 0; i < surroundTile.Length; i++)
        {
            if(surroundTile[i].x<0 || surroundTile[i].x>22 || surroundTile[i].y<0 || surroundTile[i].y>10) continue;
            if (!TilesManager.Instance.GetTileIsAvailable(surroundTile[i])) continue;
            if(selectedCarPath.Contains(surroundTile[i])) continue;
            if (i == 0) arrows[i] = "Left";
            if (i == 1) arrows[i] = "Right";
            if (i == 2) arrows[i] = "Up";
            if (i == 3) arrows[i] = "Down";
        }
        
        GameEvent.Instance.ShowDirectionArrow(currentTileHasArrow,arrows);
    }

    public void HideCurrentAvailablePathArrow()
    {
        GameEvent.Instance.HideDirectionArrow(currentTileHasArrow);
    }

    public void AddToPath(Vector2Int tileID)
    {
        CarManager.Instance.cars[currentSelectedCar].AddToPath(tileID);
        TilesManager.Instance.SetTileCanBeSelected(tileID, false);
        OnChangeToPath();
    }

    public void RemoveFromPath(Vector2Int tileID)
    {
        CarManager.Instance.cars[currentSelectedCar].RemoveFromPath(tileID);
        TilesManager.Instance.SetTileCanBeSelected(tileID, true);
        OnChangeToPath();
    }

    #endregion

    #region Car

    public void SetCurrentSelectedCar(int carID)
    {
        currentSelectedCar = carID;
    }

    public void StartSelectedCar()
    {
        CarManager.Instance.StartSelectedCar(currentSelectedCar);
    }

    public void StopSelectedCar()
    {
        CarManager.Instance.StopSelectedCar(currentSelectedCar);
    }

    #endregion

    #endregion
}