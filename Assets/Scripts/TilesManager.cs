using System;
using System.Collections;
using System.Collections.Generic;
using Shapes2D;
using UnityEngine;

public class TilesManager : MonoBehaviour
{
    #region Variables

    [Header("Initialize")] [SerializeField]
    private Transform grid;
    [SerializeField] private int gridX;
    [SerializeField] private int gridY;
    
    private Tile[,] tileScripts;
    [SerializeField] private Tile[] gateOut;
    [SerializeField] private Tile[] gateIn;

    #endregion

    #region Singleton

    private static TilesManager _instance;

    public static TilesManager Instance
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

        InitializeGrid();
    }

    #endregion

    #region Mono

    private void Start()
    {
        GameEvent.Instance.SpawnCar();
    }

    #endregion

    #region Methods

    private void InitializeGrid()
    {
        tileScripts = new Tile[gridX, gridY];
        int x = 0;
        int y = 0;
        foreach (Transform Child in grid.transform)
        {
            if (x >= gridX)
            {
                y++;
                x = 0;
            }

            tileScripts[x, y] = Child.GetComponent<Tile>();
            tileScripts[x, y].AssignPos(x, y);
            x++;
        }

        GameEvent.Instance.ResetTilePathStatus();
    }

    public Transform GetTileCurrentTransform(Vector2Int tilePosID)
    {
        return tileScripts[tilePosID.x, tilePosID.y].GetCurrentTransform();
    }

    public bool IsTileAvailable(Vector2Int tileID)
    {
        return tileScripts[tileID.x, tileID.y].GetIsAvailable();
    }

    public void SetTileAvailable(Vector2Int tileID, bool isAvailable)
    {
        tileScripts[tileID.x, tileID.y].SetTileAvailable(isAvailable);
    }

    public void SetTileSelected(Vector2Int tileID, bool isSelected)
    {
        tileScripts[tileID.x, tileID.y].SetTileIsSelected(isSelected);
    }

    public void ResetTilePathStatus()
    {
        GameEvent.Instance.ResetTilePathStatus();
    }

    public void ResetAllHighlight()
    {
        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridY; y++)
            {
                Vector2Int tileID = new Vector2Int(x, y);
                GameEvent.Instance.UnHighlightAssignedTile(tileID);
            }
        }
    }

    public void SetTileCanBeSelected(Vector2Int tileID, bool isEnabled)
    {
        GameEvent.Instance.ChangeColliderEnabled(tileID, isEnabled);
    }

    public void MakeTilesAddableToPath(Vector2Int tileID)
    {
        GameEvent.Instance.ChangeCanBeAddedToPath(tileID, true);
        SetTileCanBeSelected(tileID, true);
    }

    public void MakeTilesRemovableFromPath(Vector2Int tileID)
    {
        GameEvent.Instance.ChangeCanBeRemovedFromPath(tileID, true);
        SetTileCanBeSelected(tileID, true);
    }

    public void UnHighlightTile(Vector2Int tileID)
    {
        GameEvent.Instance.UnHighlightAssignedTile(tileID);
    }

    public bool GetTileIsAvailable(Vector2Int tileID)
    {
        return tileScripts[tileID.x, tileID.y].GetIsAvailable();
    }

    public void MakeGateAddableToPath(int gateNum, string type)
    {
        GameEvent.Instance.ToggleGateSelectable(gateNum, type, true);
    }
    
    public void ResetGate(int gateNum, string type)
    {
        GameEvent.Instance.ToggleGateSelectable(gateNum, type, false);
    }

    public bool CheckIsGateOut(Vector2Int tileID)
    {
        bool isGateOut = false;
        for (int i = 0; i < 4; i++)
        {
            if (tileID == gateOut[i].GetTileID())
            {
                isGateOut = true;
                break;
            }
        }

        return isGateOut;
    }

    public void SetTileMiddleMiddlePath(Vector2Int tileID, bool isMiddlePath)
    {
        tileScripts[tileID.x,tileID.y].SetTileIsMiddlePath(isMiddlePath);
    }
    #endregion

    #region Getter/Setter

    public int GetGridXSize()
    {
        return gridX;
    }
    public int GetGridYSize()
    {
        return gridY;
    }

    public Tile GetTileScript(Vector2Int tileID)
    {
        return tileScripts[tileID.x, tileID.y];
    }

    #endregion
}