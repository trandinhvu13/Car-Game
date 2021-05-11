using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilesManager : MonoBehaviour
{
    #region Variables
    [Header("Initialize")]
    [SerializeField] private Transform grid;
    private Tile[,] tileScripts;
    
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



    #endregion

    #region Methods

    private void InitializeGrid()
    {
        tileScripts = new Tile[23, 11];
        int x = 0;
        int y = 0;
        foreach (Transform Child in grid.transform)
        {
            if (x >= 23)
            {
                y++;
                x = 0;
            }
            tileScripts[x, y] = Child.GetComponent<Tile>();
            tileScripts[x, y].AssignPos(x,y);
            x++;
        }
        
    }

    public Transform GetTileCurrentTransform(Vector2Int tilePosID)
    {
        return tileScripts[tilePosID.x, tilePosID.y].GetCurrentTransform();
    }

    public bool IsTileAvailable(Vector2Int tileID)
    {
        return tileScripts[tileID.x, tileID.y].IsAvailable();
    }
    public void SetTileAvailable(Vector2Int tileID, bool isAvailable)
    {
        tileScripts[tileID.x, tileID.y].SetTileAvailable(isAvailable);
    }

    public void SetTileSelected(Vector2Int tileID, bool isSelected)
    {
        tileScripts[tileID.x, tileID.y].SetTileIsSelected(isSelected);
    }

    #endregion
}
