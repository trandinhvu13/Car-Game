using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

class Node
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Cost { get; set; }
    public int Distance { get; set; }
    public int CostDistance => Cost + Distance;
    public Node Parent { get; set; }
    
    public void SetDistance(int targetX, int targetY)
    {
        this.Distance = Math.Abs(targetX - X) + Math.Abs(targetY - Y);
    }
}

public class PathPicker : MonoBehaviour
{
    #region Singleton

    private static PathPicker _instance;

    public static PathPicker Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    #endregion

    #region Variables

    [SerializeField] private int currentSelectedCar;
    private Vector2Int currentTileHasArrow = new Vector2Int(-1, -1);
    public List<Vector2Int> selectedCarPath;
    public List<Vector2Int> selectedCarMiddlePath;

    public bool isChangingPath = false;
    [Header("Gates")] [SerializeField] private Vector2Int tileToLeftGate;
    [SerializeField] private Vector2Int tileToRightGate;
    [SerializeField] private Vector2Int tileToUpGate;
    [SerializeField] private Vector2Int tileToDownGate;
    private List<Tile> middleTiles;

    #endregion

    #region Methods

    #region Path Finding

    public List<Vector2Int> FindPath(Vector2Int startTileID, Vector2Int finishTileID)
    {
        List<Vector2Int> finalPath = new List<Vector2Int>();

        Node startNode = new Node();
        Node finishNode = new Node();
        startNode.X = startTileID.x;
        startNode.Y = startTileID.y;
        finishNode.X = finishTileID.x;
        finishNode.Y = finishTileID.y;

        startNode.SetDistance(finishNode.X, finishNode.Y);

        List<Node> activeNodes = new List<Node>();
        activeNodes.Add(startNode);
        List<Node> visitedNodes = new List<Node>();

        while (activeNodes.Any())
        {
            var checkNode = activeNodes.OrderBy(x => x.CostDistance).First();

            if (checkNode.X == finishNode.X && checkNode.Y == finishNode.Y)
            {
                Node node = checkNode;
                int num = checkNode.Cost;
                for (int i = 0; i < num; i++)
                {
                    Vector2Int tileID = new Vector2Int(node.X, node.Y);
                    finalPath.Add(tileID);
                    node = node.Parent;
                }
                finalPath.Reverse();
                return finalPath;
            }
            visitedNodes.Add(checkNode);
            activeNodes.Remove(checkNode);

            List<Node> walkableNodes = GetWalkableNodes(checkNode, finishNode);

            foreach (var walkableNode in walkableNodes)
            {
                if (visitedNodes.Any(x => x.X == walkableNode.X && x.Y == walkableNode.Y))
                    continue;
                if (activeNodes.Any(x => x.X == walkableNode.X && x.Y == walkableNode.Y))
                {
                    var existingTile = activeNodes.First(x => x.X == walkableNode.X && x.Y == walkableNode.Y);
                    if (existingTile.CostDistance > checkNode.CostDistance)
                    {
                        activeNodes.Remove(existingTile);
                        activeNodes.Add(walkableNode);
                    }
                }
                else
                {
                    activeNodes.Add(walkableNode);
                }
            }
        }
        return new List<Vector2Int>();
    }

    private List<Node> GetWalkableNodes(Node currentNode, Node targetNode)
    {
        List<Node> possibleNodes = new List<Node>();

        List<string> availableTiles = new List<string>();
        availableTiles = ShowAvailableDirection(new Vector2Int(currentNode.X, currentNode.Y), false);

        for (int i = 0; i < availableTiles.Count; i++)
        {
            if (availableTiles[i] == "Up")
            {
                possibleNodes.Add(new Node
                    {X = currentNode.X, Y = currentNode.Y + 1, Parent = currentNode, Cost = currentNode.Cost + 1});
                continue;
            }

            if (availableTiles[i] == "Down")
            {
                possibleNodes.Add(new Node
                    {X = currentNode.X, Y = currentNode.Y - 1, Parent = currentNode, Cost = currentNode.Cost + 1});
                continue;
            }

            if (availableTiles[i] == "Left")
            {
                possibleNodes.Add(new Node
                    {X = currentNode.X - 1, Y = currentNode.Y, Parent = currentNode, Cost = currentNode.Cost + 1});
                continue;
            }

            if (availableTiles[i] == "Right")
            {
                possibleNodes.Add(new Node
                    {X = currentNode.X + 1, Y = currentNode.Y, Parent = currentNode, Cost = currentNode.Cost + 1});
                continue;
            }
        }

        foreach (Node node in possibleNodes)
        {
            node.SetDistance(targetNode.X, targetNode.Y);
        }

        return possibleNodes;
    }

    #endregion

    #region Path

    public void UpdateTileStatus()
    {
        isChangingPath = true;

        selectedCarPath = CarManager.Instance.cars[currentSelectedCar].GetCurrentPath();
        selectedCarMiddlePath = CarManager.Instance.cars[currentSelectedCar].GetCurrentMiddlePath();

        for (int x = 0; x < TilesManager.Instance.GetGridXSize(); x++)
        {
            for (int y = 0; y < TilesManager.Instance.GetGridYSize(); y++)
            {
                Vector2Int tileID = new Vector2Int(x, y);
                TilesManager.Instance.SetTileMiddleMiddlePath(tileID, false);
            }
        }

        foreach (Vector2Int tile in selectedCarMiddlePath)
        {
            TilesManager.Instance.SetTileMiddleMiddlePath(tile, true);
        }

        TilesManager.Instance.ResetTilePathStatus();
        ShowAssignedPath();
        if (selectedCarMiddlePath.Count > 0)
        {
            foreach (Vector2Int tileID in selectedCarMiddlePath)
            {
                TilesManager.Instance.MakeTilesRemovableFromPath(tileID);
            }
        }
    }

    public void DoneChangePath()
    {
        isChangingPath = false;
        TilesManager.Instance.ResetTilePathStatus();
        GameEvent.Instance.HideDirectionArrow(currentTileHasArrow);
    }

    public void CloseControllerPanel()
    {
        isChangingPath = false;
        TilesManager.Instance.ResetAllHighlight();
        TilesManager.Instance.ResetTilePathStatus();
        CarManager.Instance.UnHighlightAllCars();
        GameEvent.Instance.HideDirectionArrow(currentTileHasArrow);
    }

    public void OnChangeToPath()
    {
        TilesManager.Instance.ResetTilePathStatus();
        UpdateTileStatus();
    }

    public void ShowAssignedPath()
    {
        TilesManager.Instance.ResetAllHighlight();
        selectedCarPath = CarManager.Instance.cars[currentSelectedCar].GetCurrentPath();
        for (int tileID = 0; tileID < selectedCarPath.Count; tileID++)
        {
            GameEvent.Instance.HighlightAssignedTile(selectedCarPath[tileID]);
        }
        CarManager.Instance.DeleteAllLine();
        CarManager.Instance.cars[currentSelectedCar].DrawLine(true);
    }

    public List<string> ShowAvailableDirection(Vector2Int currentTileHasArrow, bool isShowArrow)
    {
        List<string> arrows = new List<string>();
        Vector2Int[] surroundTile =
        {
            new Vector2Int(currentTileHasArrow.x - 1, currentTileHasArrow.y),
            new Vector2Int(currentTileHasArrow.x + 1, currentTileHasArrow.y),
            new Vector2Int(currentTileHasArrow.x, currentTileHasArrow.y + 1),
            new Vector2Int(currentTileHasArrow.x, currentTileHasArrow.y - 1)
        };
        for (int i = 0; i < surroundTile.Length; i++)
        {
            if (surroundTile[i].x < 0 || surroundTile[i].x > TilesManager.Instance.GetGridXSize() - 1 ||
                surroundTile[i].y < 0 ||
                surroundTile[i].y > TilesManager.Instance.GetGridYSize() - 1) continue;
            if (!TilesManager.Instance.GetTileIsAvailable(surroundTile[i]) && TilesManager.Instance.GetTileIsParkingSlot
                (surroundTile[i])
            ) continue;
            //if (selectedCarPath.Contains(surroundTile[i])) continue;
            if (i == 0 && TilesManager.Instance.GetTileScript(currentTileHasArrow).GetCanMoveLeft())
            {
                arrows.Add("Left");
                continue;
            }

            if (i == 1 && TilesManager.Instance.GetTileScript(currentTileHasArrow).GetCanMoveRight())
            {
                arrows.Add("Right");
                continue;
            }

            if (i == 2 && TilesManager.Instance.GetTileScript(currentTileHasArrow).GetCanMoveUp())
            {
                arrows.Add("Up");
                continue;
            }

            if (i == 3 && TilesManager.Instance.GetTileScript(currentTileHasArrow).GetCanMoveDown())
            {
                arrows.Add("Down");
                continue;
            }
        }
        return arrows;
    }
    
    public List<Vector2Int> MakeFinalPath(List<Vector2Int> middleTiles)
    {
        List<Vector2Int> finalPath = new List<Vector2Int>();
        if (middleTiles.Count <= 1)
        {
            return new List<Vector2Int>();
        }

        for (int i = 0; i < middleTiles.Count - 1; i++)
        {
            Debug.Log($"From {middleTiles[i]} to {middleTiles[i+1]}");
            List<Vector2Int> path = FindPath(middleTiles[i], middleTiles[i + 1]);

            if (path.Count < 1) return new List<Vector2Int>();
            for (int j = 0; j < path.Count; j++)
            {
                finalPath.Add(path[j]);
            }
        }
        return finalPath;
    }

    public void AddToPath(Vector2Int tileID)
    {
        if (!isChangingPath) return;
        CarManager.Instance.cars[currentSelectedCar].AddToPath(tileID);

        OnChangeToPath();
    }

    public void RemoveFromPath(Vector2Int tileID)
    {
        CarManager.Instance.cars[currentSelectedCar].RemoveFromPath(tileID);
        OnChangeToPath();
    }

    #endregion

    #region Car

    public void SetCurrentSelectedCar(int carID)
    {
        currentSelectedCar = carID;
        selectedCarMiddlePath = CarManager.Instance.cars[currentSelectedCar].GetCurrentMiddlePath();
    }

    public int GetCurrentSelectedCar()
    {
        return currentSelectedCar;
    }
    public void StartSelectedCar()
    {
        CarManager.Instance.StartSelectedCar(currentSelectedCar);
    }

    public void StopSelectedCar()
    {
        CarManager.Instance.StopSelectedCar(currentSelectedCar);
    }

    public void IncreaseSpeedSelectedCar()
    {
        CarManager.Instance.IncreaseCarSpeed(currentSelectedCar);
    }
    public void DecreaseSpeedSelectedCar()
    {
        CarManager.Instance.DecreaseCarSpeed(currentSelectedCar);
    }
    #endregion

    #endregion
}