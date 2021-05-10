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

    [Header("Car")] public LeanTweenType carMoveTween;


    #endregion
}

