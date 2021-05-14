using System;
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
    public Car[] cars = new Car[96];

    #endregion

    #region Mono

 
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

    #region Getter/Setter

    public int GetCarAmount()
    {
        return carAmount;
    }

    public void ChangeCarAmount(int amount)
    {
        carAmount += amount;
    }

    #endregion
}