using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SpecificCarUI : MonoBehaviour
{
    #region Singleton

    private static SpecificCarUI _instance;

    public static SpecificCarUI Instance
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
    [Header("Current Car Stats")]
    public int minSpeed;
    public int maxSpeed;
    public int currentSpeed;
    public int currentSelectedCarID;
    public bool isMoving;
    public bool isChanging;

    [Header("Components")]
    public TextMeshProUGUI carTitle;
    public TextMeshProUGUI carSpeedText;
    public Button startButton;
    public Text startButtonText;
    public Button changeButton;
    public Text changeButtonText;
    public Button increaseSpeedButton;
    public Button decreaseSpeedButton;
    public GameObject specificGO;
    #endregion

    #region Mono

    private void Start()
    {
        Init();
    }

    #endregion

    #region Methods

    public void Init()
    {
        //tween
        currentSelectedCarID = PathPicker.Instance.GetCurrentSelectedCar();
        if (currentSelectedCarID == -1)
        {
            startButton.interactable = false;
            changeButton.interactable = false;
            decreaseSpeedButton.interactable = false;
            increaseSpeedButton.interactable = false;
            return;
        }
        
        startButton.interactable = true;
        changeButton.interactable = true;
        decreaseSpeedButton.interactable = true;
        increaseSpeedButton.interactable = true;
        
        currentSpeed = (int) CarManager.Instance.GetCarSpeed(currentSelectedCarID);
        isMoving = CarManager.Instance.GetCarIsMoving(currentSelectedCarID);
        isChanging = PathPicker.Instance.isChangingPath;
        
        LeanTween.scale(specificGO, EffectData.Instance.panelExpandScale, EffectData.Instance.panelExpandTime).setEase
            (EffectData.Instance.panelExpandTween)
            .setOnComplete(() =>
        {
            carTitle.text = $"Car No.{currentSelectedCarID.ToString()}";
            carSpeedText.text = $"Speed: {currentSpeed.ToString()}";
            if (currentSpeed >= maxSpeed)
            {
                increaseSpeedButton.interactable = false;
            }

            if (currentSpeed <= minSpeed)
            {
                decreaseSpeedButton.interactable = false;
            }

            if (isMoving)
            {
                startButtonText.text = "Stop";
                changeButton.interactable = false;
            }
            else
            {
                startButtonText.text = "Start";
                changeButton.interactable = true;
            }

            if (!isChanging)
            {
                changeButtonText.text = "Change";
            }
            else
            {
                changeButtonText.text = "Done";
                startButton.interactable = false;
            }
        });
        LeanTween.scale(specificGO, new Vector3(1, 1, 1), EffectData.Instance.panelExpandTime).setEase
            (EffectData.Instance.panelExpandTween).setDelay(EffectData.Instance.panelExpandTime);
    }


    public void StartButton()
    {
        if (isMoving)
        {
            PathPicker.Instance.StopSelectedCar();
            startButtonText.text = "Start";
            isMoving = false;
            changeButton.interactable = true;
        }
        else
        {
            PathPicker.Instance.StartSelectedCar();
            startButtonText.text = "Stop";
            isMoving = true;
            changeButton.interactable = false;
        }
    }

    public void ChangeButton()
    {
        if (isChanging)
        {
            PathPicker.Instance.DoneChangePath();
            changeButtonText.text = "Change";
            isChanging = false;
            startButton.interactable = true;
        }
        else
        {
            PathPicker.Instance.UpdateTileStatus();
            changeButtonText.text = "Done";
            isChanging = true;
            startButton.interactable = false;
        }
    }

    public void IncreaseSpeedButton()
    {
        currentSpeed++;
        PathPicker.Instance.IncreaseSpeedSelectedCar();
        carSpeedText.text = $"Speed: {currentSpeed.ToString()}";
        if (currentSpeed >= maxSpeed)
        {
            increaseSpeedButton.interactable = false;
        }
    }
    public void DecreaseSpeedButton()
    {
        currentSpeed--;
        PathPicker.Instance.DecreaseSpeedSelectedCar();
        carSpeedText.text = $"Speed: {currentSpeed.ToString()}";
        if (currentSpeed <=minSpeed)
        {
            decreaseSpeedButton.interactable = false;
        }
    }

    public void StartAllButton()
    {
        CarManager.Instance.StartAllCar();
        changeButton.interactable = false;
    }

    public void StopAllButton()
    {
        CarManager.Instance.StopAllCar();
        changeButton.interactable = true;
    }

    public void Reset()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    #endregion
}