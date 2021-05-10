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


    #endregion
}
