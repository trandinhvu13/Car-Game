using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Lean.Pool;
using Shapes2D;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Tile : MonoBehaviour
{
    #region Variables

    [Header("Pos")] [SerializeField] private Vector2Int id;
    [Header("Gate")] [SerializeField] private bool isGate;
    [SerializeField] private bool isWall;
    [SerializeField] private string gateType;
    [Header("Stats")] [SerializeField] private bool isAvailable;

    [SerializeField] private bool isSelected;
    [SerializeField] private bool isParkingSlot;
    [SerializeField] private bool isMiddlePath =false;
    [SerializeField] private bool canMoveLeft;
    [SerializeField] private bool canMoveRight;
    [SerializeField] private bool canMoveUp;
    [SerializeField] private bool canMoveDown;

    [SerializeField] private bool canBeAddedToPath;
    [SerializeField] private bool canBeRemovedFromPath;

    [Header("Visual")] [SerializeField] private Shape shape;

    [SerializeField] private GameObject upArrow;
    [SerializeField] private GameObject downArrow;
    [SerializeField] private GameObject leftArrow;
    [SerializeField] private GameObject rightArrow;

    [Header("Car")] [SerializeField] private GameObject car;
    [SerializeField] private string carSpawnDir;
    [SerializeField] private Transform carParent;

    [Header("Other Components")] [SerializeField]
    private BoxCollider2D col;

    #endregion

   
    #region Mono

    private void Awake()
    {
        col.enabled = false;
    }

    private void OnEnable()
    {
        GameEvent.Instance.OnHighlightAssignedTile += HightlightTile;
        GameEvent.Instance.OnUnHighlightAssignedTile += UnHighlightTile;
        GameEvent.Instance.OnShowDirectionArrow += ShowAvailablePathArrow;
        GameEvent.Instance.OnChangeCanBeAddedToPath += ChangeCanBeAddedToPath;
        GameEvent.Instance.OnHideDirectionArrow += HideAvailablePathArrow;
        GameEvent.Instance.OnResetTilePathStatus += ResetTilePathStatus;
        GameEvent.Instance.OnChangeCanBeRemovedFromPath += ChangeCanBeRemovedFromPath;
        GameEvent.Instance.OnSpawnCar += SpawnCar;
        GameEvent.Instance.OnChangeColliderEnabled += SetCollider;
    }

    private void OnDisable()
    {
        GameEvent.Instance.OnHighlightAssignedTile -= HightlightTile;
        GameEvent.Instance.OnUnHighlightAssignedTile -= UnHighlightTile;
        GameEvent.Instance.OnShowDirectionArrow -= ShowAvailablePathArrow;
        GameEvent.Instance.OnChangeCanBeAddedToPath -= ChangeCanBeAddedToPath;
        GameEvent.Instance.OnHideDirectionArrow -= HideAvailablePathArrow;
        GameEvent.Instance.OnResetTilePathStatus -= ResetTilePathStatus;
        GameEvent.Instance.OnChangeCanBeRemovedFromPath -= ChangeCanBeRemovedFromPath;
        GameEvent.Instance.OnSpawnCar -= SpawnCar;
        GameEvent.Instance.OnChangeColliderEnabled -= SetCollider;
    }

    #endregion

    #region Methods

    public void AssignPos(int x, int y)
    {
        id.x = x;
        id.y = y;
    }

    public void SpawnCar()
    {
        if (!isParkingSlot) return;
        Quaternion rotation = new Quaternion();
        float currentRotation = 0;

        if (carSpawnDir == "Up") currentRotation = -90;
        if (carSpawnDir == "Down") currentRotation = 90;
        if (carSpawnDir == "Left") currentRotation = 0;
        if (carSpawnDir == "Right") currentRotation = 180;

        rotation = Quaternion.Euler(0, 0, currentRotation);
        GameObject spawnedCar = LeanPool.Spawn(car, transform.position, rotation, carParent);
        Car carScript = spawnedCar.GetComponent<Car>();
        carScript.SetCurrentTileID(id);
        carScript.SetCurrentDirection(carSpawnDir);
        carScript.SetCurrentRotation(currentRotation);
        carScript.SetUpCar();
        SetTileAvailable(false);
    }

    private void HightlightTile(Vector2Int tileID)
    {
        if (tileID != id) return;
        if (isWall) return;
        if (isGate) return;
        if (isMiddlePath)
        {
            ChangeColor(EffectData.Instance.tileMiddlePathColor);
        }
        else
        {
            ChangeColor(EffectData.Instance.tileHighlightColor);
        }
        
    }

    private void UnHighlightTile(Vector2Int tileID)
    {
        if (tileID != id) return;
        if (isWall) return;
        if (isGate)
        {
            return;
        }

        if (isParkingSlot)
        {
            ChangeColor(EffectData.Instance.tileParkingSlotColor);
        }
        else
        {
            ChangeColor(EffectData.Instance.tileNormalColor);
        }
    }

    private void ChangeColor(Color32 color)
    {
        shape.settings.fillColor = color;
    }

    private void ShowAvailablePathArrow(Vector2Int id, List<string> arrows)
    {
        if (id != this.id) return;
        if (isWall) return;
        //HideAvailablePathArrow(this.id);
       
        for (int i = 0; i < arrows.Count; i++)
        {
            if (arrows[i] == "Left")
            {
                leftArrow.SetActive(true);
                TilesManager.Instance.MakeTilesAddableToPath(new Vector2Int(id.x - 1, id.y));
                continue;
            }

            if (arrows[i] == "Right")
            {
                rightArrow.SetActive(true);
                TilesManager.Instance.MakeTilesAddableToPath(new Vector2Int(id.x + 1, id.y));
                continue;
            }

            if (arrows[i] == "Up")
            {
                upArrow.SetActive(true);
                TilesManager.Instance.MakeTilesAddableToPath(new Vector2Int(id.x, id.y + 1));
                continue;
            }

            if (arrows[i] == "Down")
            {
                downArrow.SetActive(true);
                TilesManager.Instance.MakeTilesAddableToPath(new Vector2Int(id.x, id.y - 1));
                continue;
            }
        }
    }

    private void HideAvailablePathArrow(Vector2Int id)
    {
        if (id != this.id) return;
        if (isWall) return;
        upArrow.SetActive(false);
        downArrow.SetActive(false);
        leftArrow.SetActive(false);
        rightArrow.SetActive(false);
    }

    private void ChangeCanBeAddedToPath(Vector2Int id, bool canBeAddedToPath)
    {
        if (id != this.id) return;
        if (!isAvailable) return;
        this.canBeAddedToPath = canBeAddedToPath;
    }

    private void ChangeCanBeRemovedFromPath(Vector2Int id, bool canBeRemovedFromPath)
    {
        if (id != this.id) return;
        this.canBeRemovedFromPath = canBeRemovedFromPath;
    }

    public void OnSelectedPathPicker()
    {
        // if (!canBeSelected) return;
        if (isWall) return;
        /*if (!isAvailable) return;
       
        if (canBeAddedToPath)
        {
            PathPicker.Instance.AddToPath(id);
            return;
        }*/
        if (canBeAddedToPath)
        {
            canBeAddedToPath = false;
            PathPicker.Instance.AddToPath(id);
            return;
        }

        if (canBeRemovedFromPath)
        {
            canBeRemovedFromPath = false;
            PathPicker.Instance.RemoveFromPath(id);
        }
    }

    private void ResetTilePathStatus()
    {
        if (isWall) return;
        canBeAddedToPath = false;
        
        if (isMiddlePath)
        {
            canBeAddedToPath = false;
            canBeRemovedFromPath = true;
        }
        else
        {
            canBeAddedToPath = true;
            canBeRemovedFromPath = false;
        }
        
        if (PathPicker.Instance.isChangingPath)
        {
            col.enabled = true;
        }
        else
        {
            col.enabled = false;
        }
    }

    #endregion

    #region Getter/Setter

    public void SetTileIsMiddlePath(bool isMiddlePath)
    {
        this.isMiddlePath = isMiddlePath;
    }
    public bool GetTileIsMiddlePath()
    {
        return isMiddlePath;
    }
    public Transform GetCurrentTransform()
    {
        return gameObject.transform;
    }

    public void SetTileAvailable(bool isAvailable)
    {
        this.isAvailable = isAvailable;
    }

    public void SetCollider(Vector2Int id, bool isEnabled)
    {
        if (id != this.id) return;
        if (isWall) return;
        col.enabled = isEnabled;
    }

    public void SetTileIsSelected(bool isSelected)
    {
        this.isSelected = isSelected;
    }

    public bool GetIsAvailable()
    {
        return isAvailable;
    }

    public bool GetCanMoveLeft()
    {
        return canMoveLeft;
    }

    public bool GetCanMoveRight()
    {
        return canMoveRight;
    }

    public bool GetCanMoveUp()
    {
        return canMoveUp;
    }

    public bool GetCanMoveDown()
    {
        return canMoveDown;
    }

    public bool GetIsParkingSpot()
    {
        return isParkingSlot;
    }

    public bool GetIsGate()
    {
        return isGate;
    }
    public Vector2Int GetTileID()
    {
        return id;
    }

    public bool GetTileIsParkingSlot()
    {
        return isParkingSlot;
    }
  

    #endregion
}