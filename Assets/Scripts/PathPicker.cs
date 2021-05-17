using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    private bool isChangingPath = false;
    [Header("Gates")] 
    [SerializeField] private Vector2Int tileToLeftGate;
    [SerializeField] private Vector2Int tileToRightGate;
    [SerializeField] private Vector2Int tileToUpGate;
    [SerializeField] private Vector2Int tileToDownGate;
    #endregion

    #region Methods

    #region Path

    public void UpdateTileStatus()
    {
        isChangingPath = true;
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

    public void DoneChangePath()
    {
        isChangingPath = false;
        TilesManager.Instance.ResetTilePathStatus();
        GameEvent.Instance.HideDirectionArrow(currentTileHasArrow);
    }

    public void CloseControllerPanel()
    {
        isChangingPath = false;
        TilesManager.Instance.ResetAllHighlight();
        TilesManager.Instance.ResetTilePathStatus();
        GameEvent.Instance.HideDirectionArrow(currentTileHasArrow);
    }

    public void OnChangeToPath()
    {
        TilesManager.Instance.ResetTilePathStatus();
        UpdateTileStatus();
    }

    public void ShowAssignedPath()
    {
        selectedCarPath = CarManager.Instance.cars[currentSelectedCar].GetCurrentPath();
        for (int tileID = 0; tileID < selectedCarPath.Count; tileID++)
        {
            GameEvent.Instance.HighlightAssignedTile(selectedCarPath[tileID]);
        }
    }
    
    public void SetAvailablePath()
    {
        HideCurrentAvailablePathArrow();
        Car selectedCar = CarManager.Instance.cars[currentSelectedCar];
        //List<Vector2Int> path = selectedCar.GetCurrentPath();
        if (selectedCarPath.Count <= 0)
        {
            currentTileHasArrow = selectedCar.GetCurrentTileID();
            ShowDirectionArrow(currentTileHasArrow);
        }
        else
        {
            currentTileHasArrow = selectedCarPath[selectedCarPath.Count - 1];
            ShowDirectionArrow(currentTileHasArrow);
        }
    }

    public void ShowDirectionArrow(Vector2Int currentTileHasArrow)
    {
        List<string> arrows = new List<string>();
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
            if (i == 0) arrows.Add("Left");
            if (i == 1) arrows.Add("Right");
            if (i == 2) arrows.Add("Up");
            if (i == 3) arrows.Add("Down");
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

        OnChangeToPath();
    }

    public void RemoveFromPath(Vector2Int tileID)
    {
        CarManager.Instance.cars[currentSelectedCar].RemoveFromPath(tileID);
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