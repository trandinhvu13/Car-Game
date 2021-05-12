using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarManager : MonoBehaviour
{
    #region Singleton

    private static CarManager _instance;

    public static CarManager Instance
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

    #region Variables

    [SerializeField] private int carAmount = 0;
    private List<Car> cars;

    #endregion

    #region Method

    public void StartSelectedCar(int carID)
    {
        GameEvent.Instance.StartMoving(carID);
    }

    public void StopSelectedCar(int carID)
    {
        GameEvent.Instance.StopMoving(carID);
    }

    #endregion
}