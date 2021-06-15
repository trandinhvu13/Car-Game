using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Pool;
using UnityEngine;
using Random = UnityEngine.Random;

public class Car : MonoBehaviour, IPoolable
{
    #region Variables

    enum Direction
    {
        Left,
        Right,
        Up,
        Down
    };

    [Header("Line")] [SerializeField] private float lineWidth;
    [SerializeField] private LineRenderer lineRenderer;
    [Header("Car Model")] [SerializeField] private List<GameObject> carModels;
    [Header("Info")] [SerializeField] private int carID;
    [SerializeField] private Direction currentDirection;
    [SerializeField] private Vector2Int currentTileID;
    [SerializeField] private Vector2Int nextTileID;
    [SerializeField] private Outline[] outlines;
    [SerializeField] private int carModelID;

    [Header("Move")] [SerializeField] private bool isMoving = false;
    private Coroutine move;
    [SerializeField] private List<Vector2Int> path;
    [SerializeField] private List<Vector2Int> middleTiles;
    [SerializeField] private float carSpeed;
    private int moveTweenID = -1;

    private IEnumerator moveCoroutine;
    [Header("Turn")] private int turnTweenID;
    [SerializeField] private float currentRotation;
    [SerializeField] private float nextRotation;

    #endregion

    #region Mono

    public void OnSpawn()
    {
        GameEvent.Instance.OnStartMoving += StartMoving;
        GameEvent.Instance.OnStopMoving += StopMoving;
        GameEvent.Instance.OnCarExitGate += ExitGate;
        GameEvent.Instance.OnHighlightCar += HighlightCar;
    }

    public void OnDespawn()
    {
        GameEvent.Instance.OnStartMoving -= StartMoving;
        GameEvent.Instance.OnStopMoving -= StopMoving;
        GameEvent.Instance.OnCarExitGate -= ExitGate;
        GameEvent.Instance.OnHighlightCar -= HighlightCar;

        //reset car stats
        ResetCar();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Car"))
        {
            Car otherCarScript = other.gameObject.GetComponent<Car>();
            Vector2Int otherCarCurrentTileID = otherCarScript.GetCurrentTileID();

            if (currentDirection == Direction.Left && otherCarCurrentTileID.x < currentTileID.x)
            {
                StopMoving(carID);
                return;
            }

            if (currentDirection == Direction.Right && otherCarCurrentTileID.x > currentTileID.x)
            {
                StopMoving(carID);
                return;
            }

            if (currentDirection == Direction.Up && otherCarCurrentTileID.y > currentTileID.y)
            {
                StopMoving(carID);
                return;
            }

            if (currentDirection == Direction.Down && otherCarCurrentTileID.y < currentTileID.y)
            {
                StopMoving(carID);
                return;
            }

            StopMoving(carID);
        }
    }

    private void Awake()
    {
        CarManager.Instance.ChangeCarAmount(1);
        carID = CarManager.Instance.GetCarAmount();
        CarManager.Instance.cars[carID] = this;

        path = new List<Vector2Int>();
        middleTiles = new List<Vector2Int>();
        RandomCarModel();
        HighlightCar(carID, false);
    }

    private void Update()
    {
    }

    #endregion


    #region Methods

    public void RandomCarModel()
    {
        carModelID = Random.Range(0, carModels.Count);
        carModels[carModelID].SetActive(true);
    }

    public void SetUpCar()
    {
        TilesManager.Instance.SetTileAvailable(currentTileID, false);
        TilesManager.Instance.SetTileSelected(currentTileID, true);
    }

    public void ResetCar()
    {
        isMoving = false;
        path = new List<Vector2Int>();
        middleTiles = new List<Vector2Int>();
        carSpeed = 2;
    }

    public void AddPointToPath(Vector2Int tilePos)
    {
        path.Add(tilePos);
    }

    public void DrawLine(bool isDrawn)
    {
        return;
    }


    public void DeletePointInPath(int carID, int pointIndex)
    {
        if (carID == this.carID)
        {
            middleTiles.RemoveRange(pointIndex, middleTiles.Count - pointIndex);
        }
    }

    public void AddMiddlePoint(Vector2Int tile)
    {
        middleTiles.Add(tile);
    }

    private void StartMoving(int carID)
    {
        if (carID != this.carID) return;
        if (path.Count <= 0) return;
        if (isMoving) return;
        if (path.Count <= 0) return;
        isMoving = true;
        moveCoroutine = Move();
        StartCoroutine(moveCoroutine);
    }

    private void StopMoving(int carID)
    {
        if (carID != this.carID) return;
        if (!isMoving) return;
        if (moveTweenID == -1) return;
        isMoving = false;

        if (middleTiles.Count > 0)
        {
            middleTiles.Insert(0, currentTileID);
        }

        LeanTween.pause(moveTweenID);
        StopCoroutine(moveCoroutine);
    }

    private void HighlightCar(int id, bool isHighlighted)
    {
        if (id != carID) return;
        outlines[carModelID].enabled = isHighlighted;
    }

    IEnumerator Move()
    {
        int numOfPoints = path.Count;
        bool canContinue = true;
        for (int i = 0; i < numOfPoints; i++)
        {
            nextTileID = path[0];
            if (!TilesManager.Instance.IsTileAvailable(nextTileID))
            {
                StopMoving(carID);
                yield break;
            }

            if (HaveToTurn(CheckNextDirection()))
            {
                CarTurn(CheckNextDirection());
                yield return new WaitForSeconds(EffectData.Instance.carTurnTweenTime);
            }

            canContinue = false;
            moveTweenID = LeanTween.move(gameObject,
                    TilesManager.Instance.GetTileCurrentTransform(nextTileID).position,
                    carSpeed)
                .setEase
                    (EffectData.Instance.carMoveTween).setSpeed(carSpeed)
                .setOnComplete(
                    () =>
                    {
                        canContinue = true;
                        if (middleTiles[0] == currentTileID) middleTiles.RemoveAt(0);
                        TilesManager.Instance.SetTileSelected(currentTileID, false);
                        TilesManager.Instance.SetTileAvailable(currentTileID, true);
                        currentTileID = nextTileID;

                        TilesManager.Instance.SetTileSelected(currentTileID, true);
                        TilesManager.Instance.SetTileAvailable(currentTileID, false);

                        int count = 0;
                        for (int i = 0; i < path.Count; i++)
                        {
                            if (path[i] == nextTileID)
                            {
                                count++;
                            }
                        }

                        if (count < 2)
                        {
                            GameEvent.Instance.UnHighlightAssignedTile(nextTileID);
                        }

                        if (TilesManager.Instance.CheckIsGateOut(nextTileID))
                        {
                            CarManager.Instance.CarExit(carID);
                            TilesManager.Instance.SetTileSelected(nextTileID, false);
                            TilesManager.Instance.SetTileAvailable(nextTileID, true);
                        }

                        if (path.Count >= 1)
                        {
                            path.RemoveAt(0);
                        }
                    }).id;
            yield return new WaitUntil(() => canContinue);
        }
        isMoving = false;
        SpecificCarUI.Instance.Init();
        yield return null;
    }

    private string CheckNextDirection()
    {
        if (currentTileID.x - nextTileID.x < 0)
        {
            return "Right";
        }

        if (currentTileID.x - nextTileID.x > 0)
        {
            return "Left";
        }

        if (currentTileID.y - nextTileID.y < 0)
        {
            return "Up";
        }

        if (currentTileID.y - nextTileID.y > 0)
        {
            return "Down";
        }

        return null;
    }

    private bool HaveToTurn(string nextDirection)
    {
        if (currentDirection.ToString() == nextDirection) return false;
        return true;
    }

    private void CarTurn(string direction)
    {
        float turnDelay = 0;

        if (turnTweenID != 0 && LeanTween.isPaused(turnTweenID))
        {
            turnDelay = EffectData.Instance.carTurnTweenTime;
            turnTweenID = LeanTween.rotateZ(gameObject, currentRotation, EffectData.Instance.carTurnTweenTime)
                .setEase(EffectData.Instance.carTurnTween).id;
        }

        if (currentDirection == Direction.Left)
        {
            if (direction == "Up")
            {
                TurnRight(turnDelay, Direction.Up);
            }

            if (direction == "Down")
            {
                TurnLeft(turnDelay, Direction.Down);
            }

            if (direction == "Right")
            {
                TurnAround(turnDelay, Direction.Right);
            }
        }

        if (currentDirection == Direction.Right)
        {
            if (direction == "Up")
            {
                TurnLeft(turnDelay, Direction.Up);
            }

            if (direction == "Down")
            {
                TurnRight(turnDelay, Direction.Down);
            }

            if (direction == "Left")
            {
                TurnAround(turnDelay, Direction.Left);
            }
        }

        if (currentDirection == Direction.Up)
        {
            if (direction == "Left")
            {
                TurnLeft(turnDelay, Direction.Left);
            }

            if (direction == "Right")
            {
                TurnRight(turnDelay, Direction.Right);
            }

            if (direction == "Down")
            {
                TurnAround(turnDelay, Direction.Down);
            }
        }

        if (currentDirection == Direction.Down)
        {
            if (direction == "Left")
            {
                TurnRight(turnDelay, Direction.Left);
            }

            if (direction == "Right")
            {
                TurnLeft(turnDelay, Direction.Right);
            }

            if (direction == "Up")
            {
                TurnAround(turnDelay, Direction.Up);
            }
        }
    }

    private void TurnLeft(float delay, Direction nextDirection)
    {
        //currentRotation = transform.rotation.z;
        nextRotation = currentRotation + 90;
        turnTweenID = LeanTween.rotateZ(gameObject, nextRotation, EffectData.Instance.carTurnTweenTime)
            .setEase(EffectData.Instance.carTurnTween).setDelay(delay).setOnComplete(() =>
            {
                currentRotation = nextRotation;
                currentDirection = nextDirection;
            }).id;
    }

    private void TurnRight(float delay, Direction nextDirection)
    {
        nextRotation = currentRotation - 90;
        turnTweenID = LeanTween.rotateZ(gameObject, nextRotation, EffectData.Instance.carTurnTweenTime)
            .setEase(EffectData.Instance.carMoveTween).setDelay(delay).setOnComplete(() =>
            {
                currentRotation = nextRotation;
                currentDirection = nextDirection;
            }).id;
    }

    private void TurnAround(float delay, Direction nextDirection)
    {
        //currentRotation = transform.rotation.z;
        nextRotation = currentRotation - 180;
        turnTweenID = LeanTween.rotateZ(gameObject, nextRotation, EffectData.Instance.carTurnTweenTime)
            .setEase(EffectData.Instance.carMoveTween).setDelay(delay).setOnComplete(() =>
            {
                currentRotation = nextRotation;
                currentDirection = nextDirection;
            }).id;
    }

    public void AddToPath(Vector2Int tileID)
    {
        if (middleTiles.Count > 0)
        {
            middleTiles[0] = currentTileID;
        }
        else
        {
            middleTiles.Add(currentTileID);
        }

        middleTiles.Add(tileID);
        TilesManager.Instance.SetTileMiddleMiddlePath(tileID, true);
        path = new List<Vector2Int>(PathPicker.Instance.MakeFinalPath(middleTiles));
    }

    public void RemoveFromPath(Vector2Int tileID)
    {
        int startIndex = middleTiles.Count;
        for (int i = 0; i < middleTiles.Count; i++)
        {
            if (middleTiles[i] == tileID)
            {
                startIndex = i;
            }
        }

        int timeHaveToRemove = middleTiles.Count - startIndex;
        for (int i = 0; i < timeHaveToRemove; i++)
        {
            TilesManager.Instance.UnHighlightTile(middleTiles[middleTiles.Count - 1]);
            middleTiles.RemoveAt(middleTiles.Count - 1);
        }

        path = new List<Vector2Int>(PathPicker.Instance.MakeFinalPath(middleTiles));
    }

    public void ExitGate(int carID)
    {
        if (carID != this.carID) return;
        if (carID == PathPicker.Instance.GetCurrentSelectedCar())
        {
            SpecificCarUI.Instance.ResetToNone();
        }
        LeanPool.Despawn(gameObject);
    }

    public void SetCurrentSelectedCar()
    {
        if (PathPicker.Instance.isChangingPath) return;
        PathPicker.Instance.SetCurrentSelectedCar(carID);
        TilesManager.Instance.ResetAllHighlight();
        CarManager.Instance.UnHighlightAllCars();
        HighlightCar(carID, true);
        PathPicker.Instance.ShowAssignedPath();
    }

    public void ShowControllerPanel()
    {
        if (PathPicker.Instance.isChangingPath) return;
        SpecificCarUI.Instance.Init();
    }

    #endregion

    #region Getter/Setter

    public Vector2Int GetCurrentTileID()
    {
        return currentTileID;
    }

    public void SetCurrentTileID(Vector2Int currentTileID)
    {
        this.currentTileID = currentTileID;
    }

    public List<Vector2Int> GetCurrentPath()
    {
        return path;
    }

    public List<Vector2Int> GetCurrentMiddlePath()
    {
        return middleTiles;
    }

    public void SetCurrentDirection(string dir)
    {
        if (dir == "Left") currentDirection = Direction.Left;
        if (dir == "Right") currentDirection = Direction.Right;
        if (dir == "Down") currentDirection = Direction.Down;
        if (dir == "Up") currentDirection = Direction.Up;
    }

    public void SetCurrentRotation(float rotation)
    {
        currentRotation = rotation;
    }

    public List<Vector2Int> GetCurrentMiddleTiles()
    {
        return middleTiles;
    }

    public void IncreaseCarSpeed()
    {
        carSpeed++;
    }

    public void DecreaseCarSpeed()
    {
        carSpeed--;
    }

    public float GetCarSpeed()
    {
        return carSpeed;
    }

    public bool GetCarIsMoving()
    {
        return isMoving;
    }

    #endregion
}