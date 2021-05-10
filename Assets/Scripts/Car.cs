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

    [Header("Move")] private bool isMoving = false;
    private Coroutine move;
    private List<Vector2Int> path;
    [SerializeField] private float carSpeed;
    private int moveTweenID;
    private IEnumerator moveCoroutine;

    #endregion

    #region Mono

    private void Start()
    {
        SetUpCar();
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
    private void PauseMoving()
    {
        isMoving = false;
        LeanTween.pause(moveTweenID);
        StopCoroutine(moveCoroutine);
    }
    IEnumerator Move()
    {
        int numOfPoints = path.Count;
        for (int i = 0; i < numOfPoints; i++)
        {
            nextTileID = path[0];
            if (!TilesManager.Instance.IsTileAvailable(nextTileID))
            {
                isMoving = false;
                yield break;
            }
            LeanTween.move(gameObject, TilesManager.Instance.GetTileCurrentTransform(path[0]).position, carSpeed).setEase
            (EffectData.Instance.carMoveTween)
            .setOnComplete(
                () =>
                {
                    TilesManager.Instance.SetTileStatus(currentTileID,true);
                    currentTileID = nextTileID;
                    TilesManager.Instance.SetTileStatus(currentTileID,false);
                    path.RemoveAt(0);
                });
            yield return new WaitForSeconds(carSpeed);
        }
        isMoving = false;
        yield return null;
    }
    
   

    #endregion
}