using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    #region Singleton

    private static UIManager _instance;

    public static UIManager Instance
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

    [SerializeField] private GameObject controllerPanel;
    

    #endregion

    #region Methods

    #region Path Picker
    public void ShowControllerPanel()
    {
        controllerPanel.SetActive(true);
    }
    
    

    #endregion
   

    
    #endregion
}
