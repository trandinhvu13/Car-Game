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

    [Header("Move")] 
    private bool isMoving = false;
    private Coroutine move;
    private List<Vector2Int> path;
    [SerializeField] private float carSpeed;
    private int moveTweenID;
    private IEnumerator moveCoroutine;
    private float tweenTimeRemaining = 0;
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
        AddPointToPath(carID,new Vector2Int(10,5));
        AddPointToPath(carID,new Vector2Int(9,5));
        AddPointToPath(carID,new Vector2Int(8,5));
        AddPointToPath(carID,new Vector2Int(7,5));
        AddPointToPath(carID,new Vector2Int(6,5));
    }

    #endregion

    #region Methods

    private void SetUpCar()
    {
        TilesManager.Instance.SetTileStatus(currentTileID, false);
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
            path.RemoveRange(pointIndex,path.Count-pointIndex);
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
                isMoving = false;
                yield break;
            }
            canContinue = false;
            moveTweenID = LeanTween.move(gameObject, TilesManager.Instance.GetTileCurrentTransform(path[0]).position, carSpeed)
            .setEase
            (EffectData.Instance.carMoveTween).setSpeed(carSpeed)
            .setOnComplete(
                () =>
                {
                    canContinue = true;
                    TilesManager.Instance.SetTileStatus(currentTileID,true);
                    currentTileID = nextTileID;
                    TilesManager.Instance.SetTileStatus(currentTileID,false);
                    path.RemoveAt(0);
                }).id;
            yield return new WaitUntil(() => canContinue);
        }
        isMoving = false;
        yield return null;
    }

    private string CheckDirection()
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
        if (currentTileID.x - nextTileID.x > 0)
        {
            return "Down";
        }

        return null;
    }

    private void CarTurn(string direction)
    {
        if (direction == "Up")
        {
            
        }
        if (direction == "Down")
        {
            
        }
        if (direction == "Left")
        {
            
        }
        if (direction == "Right")
        {
            
        }
        if (direction == null)
        {
            
        }
    }
   

    #endregion
}