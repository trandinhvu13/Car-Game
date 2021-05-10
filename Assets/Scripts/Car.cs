using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            StartMoving();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            StopMoving();
        }
    }

    private void Start()
    {
        path = new List<Vector2Int>();
        SetUpCar();
        AddPointToPath(carID, new Vector2Int(10, 5));
        AddPointToPath(carID, new Vector2Int(9, 5));
        AddPointToPath(carID, new Vector2Int(8, 5));
        AddPointToPath(carID, new Vector2Int(7, 5));
        AddPointToPath(carID, new Vector2Int(6, 5));
        AddPointToPath(carID, new Vector2Int(6, 4));
        AddPointToPath(carID, new Vector2Int(7, 4));
        AddPointToPath(carID, new Vector2Int(8, 4));
        AddPointToPath(carID, new Vector2Int(9, 4));
        AddPointToPath(carID, new Vector2Int(10, 4));
        AddPointToPath(carID, new Vector2Int(10, 3));
    }

    #endregion

    #region Methods

    private void SetUpCar()
    {
        TilesManager.Instance.SetTileStatus(currentTileID, false);
        currentRotation = transform.rotation.z;
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

    private void StartMoving()
    {
        isMoving = true;
        moveCoroutine = Move();
        StartCoroutine(moveCoroutine);
    }

    private void StopMoving()
    {
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
                StopMoving();
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
                        TilesManager.Instance.SetTileStatus(currentTileID, true);
                        currentTileID = nextTileID;
                        TilesManager.Instance.SetTileStatus(currentTileID, false);
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
        if ((currentDirection == Direction.Left || currentDirection == Direction.Right) &&
            (nextDirection == "Up" || nextDirection == "Down"))
        {
            return true;
        }

        if ((currentDirection == Direction.Up || currentDirection == Direction.Down) &&
            (nextDirection == "Left" || nextDirection == "Right"))
        {
            return true;
        }

        return false;
    }

    private void CarTurn(string direction)
    {
        isTurning = true;
        float turnDelay = 0;

        if (LeanTween.isPaused(turnTweenID))
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
        turnTweenID = LeanTween.rotateZ(gameObject, gameObject.transform.rotation.z - 90,
                EffectData.Instance.carTurnTweenTime)
            .setEase(EffectData.Instance.carMoveTween).setDelay(delay).setOnComplete(() =>
            {
                isTurning = false;
                currentRotation = nextRotation;
                currentDirection = nextDirection;
            }).id;
    }

    #endregion
}