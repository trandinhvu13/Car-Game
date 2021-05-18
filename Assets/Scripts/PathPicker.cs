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

    //The distance is essentially the estimated distance, ignoring walls to our target. 
    //So how many tiles left and right, up and down, ignoring walls, to get there. 
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            List<Vector2Int> result = FindPath(new Vector2Int(1, 1), new Vector2Int(13, 12));
            foreach (Vector2Int tile in result)
            {
                GameEvent.Instance.HighlightAssignedTile(tile);
            }
        }
    }

    #endregion

    #region Variables

    [SerializeField] private int currentSelectedCar;
    private Vector2Int currentTileHasArrow = new Vector2Int(-1, -1);
    private List<Vector2Int> selectedCarPath;
    private Node[,] map = new Node[24, 12];

    private bool isChangingPath = false;
    [Header("Gates")] [SerializeField] private Vector2Int tileToLeftGate;
    [SerializeField] private Vector2Int tileToRightGate;
    [SerializeField] private Vector2Int tileToUpGate;
    [SerializeField] private Vector2Int tileToDownGate;

    #endregion

    #region Methods

    #region Path Finding

    List<Tile> middleTiles;

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

        //This is where we created the map from our previous step etc. 

        while (activeNodes.Any())
        {
            var checkNode = activeNodes.OrderBy(x => x.CostDistance).First();

            if (checkNode.X == finishNode.X && checkNode.Y == finishNode.Y)
            {
                Debug.Log("We are at the destination!");
                //We can actually loop through the parents of each tile to find our exact path which we will show shortly. 
                //We found the destination and we can be sure (Because the the OrderBy above)
                //That it's the most low cost option. 
                Node node = checkNode;
                int num = checkNode.Cost;
                Debug.Log("cost la " + num);
                for (int i = 0; i < num; i++)
                {
                    Vector2Int tileID = new Vector2Int(node.X, node.Y);
                    finalPath.Add(tileID);
                    Debug.Log(tileID + "start " + startTileID);
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
                //We have already visited this tile so we don't need to do so again!
                if (visitedNodes.Any(x => x.X == walkableNode.X && x.Y == walkableNode.Y))
                    continue;

                //It's already in the active list, but that's OK, maybe this new tile has a better value (e.g. We might zigzag earlier but this is now straighter). 
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
                    //We've never seen this tile before so add it to the list. 
                    activeNodes.Add(walkableNode);
                }
            }
        }

        Debug.Log("No Path Found!");
        return null;
    }

    private List<Node> GetWalkableNodes(Node currentNode, Node targetNode)
    {
        List<Node> possibleNodes = new List<Node>();

        List<string> availableTiles = new List<string>();
        availableTiles = ShowDirectionArrow(new Vector2Int(currentNode.X,currentNode.Y), false);

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
                possibleNodes.Add(new Node {X = currentNode.X, Y = currentNode.Y - 1, Parent = currentNode, Cost = currentNode.Cost + 1});
                continue;
            }

            if (availableTiles[i] == "Left")
            {
                possibleNodes.Add(new Node {X = currentNode.X - 1, Y = currentNode.Y, Parent = currentNode, Cost = currentNode.Cost + 1});
                continue;
            }

            if (availableTiles[i] == "Right")
            {
                possibleNodes.Add( new Node {X = currentNode.X+1, Y = currentNode.Y, Parent = currentNode, Cost = currentNode.Cost + 1});
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
        ShowAssignedPath();
        SetAvailablePath();
        if (selectedCarPath.Count > 0)
        {
            foreach (Vector2Int tileID in selectedCarPath)
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
        GameEvent.Instance.HideDirectionArrow(currentTileHasArrow);
    }

    public void OnChangeToPath()
    {
        TilesManager.Instance.ResetTilePathStatus();
        UpdateTileStatus();
    }

    public void ShowAssignedPath()
    {
        selectedCarPath = CarManager.Instance.cars[currentSelectedCar].GetCurrentPath();
        for (int tileID = 0; tileID < selectedCarPath.Count; tileID++)
        {
            GameEvent.Instance.HighlightAssignedTile(selectedCarPath[tileID]);
        }
    }

    public void SetAvailablePath()
    {
        HideCurrentAvailablePathArrow();
        Car selectedCar = CarManager.Instance.cars[currentSelectedCar];
        //List<Vector2Int> path = selectedCar.GetCurrentPath();
        if (selectedCarPath.Count <= 0)
        {
            currentTileHasArrow = selectedCar.GetCurrentTileID();
            ShowDirectionArrow(currentTileHasArrow, true);
        }
        else
        {
            currentTileHasArrow = selectedCarPath[selectedCarPath.Count - 1];
            ShowDirectionArrow(currentTileHasArrow, true);
        }
    }

    public List<string> ShowDirectionArrow(Vector2Int currentTileHasArrow, bool isShowArrow)
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
            if (!TilesManager.Instance.GetTileIsAvailable(surroundTile[i])) continue;
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

        if (isShowArrow)
        {
            GameEvent.Instance.ShowDirectionArrow(currentTileHasArrow, arrows);
        }

        return arrows;
    }

    public void HideCurrentAvailablePathArrow()
    {
        GameEvent.Instance.HideDirectionArrow(currentTileHasArrow);
    }

    public void AddToPath(Vector2Int tileID)
    {
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
    }

    public void StartSelectedCar()
    {
        CarManager.Instance.StartSelectedCar(currentSelectedCar);
    }

    public void StopSelectedCar()
    {
        CarManager.Instance.StopSelectedCar(currentSelectedCar);
    }

    #endregion

    #endregion
}