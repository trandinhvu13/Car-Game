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

    public event Action<Vector2, int, bool> OnSpawnNewBlock;

    public void SpawnNewBlock(Vector2 pos, int blockType, bool isRainbow)
    {
        OnSpawnNewBlock?.Invoke(pos, blockType, isRainbow);
    }

    #endregion
}