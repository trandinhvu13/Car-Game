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

    #endregion

    #region Methods

    #region Path

    public void ShowAssignedPath(List<Vector2Int> path)
    {
        StartCoroutine(GraduallyShowPath());

        IEnumerator GraduallyShowPath()
        {
            for (int tileID = 0; tileID < path.Count; tileID++)
            {
                GameEvent.Instance.HighlightAssignedTile(path[tileID]);
                yield return new WaitForSeconds(EffectData.Instance.tileHighlightGradualSpeed);
            }
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