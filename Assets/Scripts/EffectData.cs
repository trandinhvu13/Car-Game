using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectData : MonoBehaviour
{
    #region Singleton

    private static EffectData _instance;

    public static EffectData Instance
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

    #region Car

    [Header("Car")] 
    public LeanTweenType carMoveTween;
    public float carTurnTweenTime;
    public LeanTweenType carTurnTween;

    #endregion

    #region Tile

    [Header("Tile")] 
    public Color32 tileHighlightColor;
    public Color32 tileMiddlePathColor;
    public Color32 tileNormalColor;
    public Color32 tileParkingSlotColor;

    [Header("Path Picker")] public float tileHighlightGradualSpeed;
    [Header("UI")] public Vector3 panelExpandScale;
    public float panelExpandTime;
    public LeanTweenType panelExpandTween;
    

    #endregion
}

