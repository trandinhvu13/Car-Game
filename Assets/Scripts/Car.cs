using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Pool;
using UnityEngine;

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

    [Header("Info")] [SerializeField] private int carID;
    [SerializeField] private Direction currentDirection;
    [SerializeField] private Vector2Int currentTileID;
    [SerializeField] private Vector2Int nextTileID;


    [Header("Move")] [SerializeField] private bool isMoving = false;
    private Coroutine move;
    private List<Vector2Int> path;
    [SerializeField] private float carSpeed;
    private int moveTweenID;

    private IEnumerator moveCoroutine;
    [Header("Turn")] [SerializeField] private bool isTurning = false;
    private int turnTweenID;
    [SerializeField] private float currentRotation;
    [SerializeField] private float nextRotation;

    #endregion

    #region Mono

    public void OnSpawn()
    {
        GameEvent.Instance.OnStartMoving += StartMoving;
        GameEvent.Instance.OnStopMoving += StopMoving;
        
   
       
    }

    public void OnDespawn()
    {
        GameEvent.Instance.OnStartMoving -= StartMoving;
        GameEvent.Instance.OnStopMoving -= StopMoving;
        
        //reset car stats
    }

    private void Awake()
    {
        CarManager.Instance.ChangeCarAmount(1);
        carID = CarManager.Instance.GetCarAmount();
        CarManager.Instance.cars[carID] = this;
        
        path = new List<Vector2Int>();
    }

    private void Update()
    {
    }

    #endregion

    #region Methods

    public void SetUpCar()
    {
        TilesManager.Instance.SetTileAvailable(currentTileID, false);
        TilesManager.Instance.SetTileSelected(currentTileID, true);
    }

    private void AddPointToPath(int carID, Vector2Int tilePos)
    {
        if (carID == this.carID)
        {
            path.Add(tilePos);
        }
    }

    private void DeletePointInPath(int carID, int pointIndex)
    {
        if (carID == this.carID)
        {
            path.RemoveRange(pointIndex, path.Count - pointIndex);
        }
    }

    private void StartMoving(int carID)
    {
        if (carID != this.carID) return;
        isMoving = true;
        moveCoroutine = Move();
        StartCoroutine(moveCoroutine);
    }

    private void StopMoving(int carID)
    {
        if (carID != this.carID) return;
        isMoving = false;
        isTurning = false;
        LeanTween.pause(moveTweenID);
        StopCoroutine(moveCoroutine);
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
            moveTweenID = LeanTween.move(gameObject, TilesManager.Instance.GetTileCurrentTransform(path[0]).position,
                    carSpeed)
                .setEase
                    (EffectData.Instance.carMoveTween).setSpeed(carSpeed)
                .setOnComplete(
                    () =>
                    {
                        canContinue = true;
                        TilesManager.Instance.SetTileSelected(currentTileID, false);
                        TilesManager.Instance.SetTileAvailable(currentTileID, true);
                        currentTileID = nextTileID;
                        TilesManager.Instance.SetTileSelected(currentTileID, true);
                        TilesManager.Instance.SetTileAvailable(currentTileID, false);
                        GameEvent.Instance.UnHighlightAssignedTile(path[0]);
                        path.RemoveAt(0);
                    }).id;
            yield return new WaitUntil(() => canContinue);
        }

        isMoving = false;
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
        isTurning = true;
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
                isTurning = false;
                currentRotation = nextRotation;
                currentDirection = nextDirection;
            }).id;
    }

    private void TurnRight(float delay, Direction nextDirection)
    {
        //currentRotation = transform.rotation.z;
        nextRotation = currentRotation - 90;
        turnTweenID = LeanTween.rotateZ(gameObject, nextRotation, EffectData.Instance.carTurnTweenTime)
            .setEase(EffectData.Instance.carMoveTween).setDelay(delay).setOnComplete(() =>
            {
                isTurning = false;
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
                isTurning = false;
                currentRotation = nextRotation;
                currentDirection = nextDirection;
            }).id;
    }

    public void AddToPath(Vector2Int tileID)
    {
        path.Add(tileID);
    }

    public void RemoveFromPath(Vector2Int tileID)
    {
        int startIndex = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            if (path[i] == tileID)
            {
                startIndex = i+1;
            }
        }

        int timeHaveToRemove = path.Count - startIndex;
        for (int i = 0; i < timeHaveToRemove; i++)
        {
            TilesManager.Instance.UnHighlightTile(path[path.Count - 1]);
            path.RemoveAt(path.Count - 1);
        }
    }

    public void SetCurrentSelectedCar()
    {
        PathPicker.Instance.SetCurrentSelectedCar(carID);
    }

    public void ShowControllerPanel()
    {
        UIManager.Instance.ShowControllerPanel();
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
    #endregion
}