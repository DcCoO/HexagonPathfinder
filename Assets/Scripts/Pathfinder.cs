using System;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour, IPathFinder
{
    public static event Action<int> OnPathGenerated;

    [SerializeField] private GameObject pathMarker;
    [SerializeField] private Gradient pathColor;

    private List<GameObject> pathMarkerPool = new List<GameObject>();
    private LineRenderer lineRenderer;

    private void OnEnable()
    {
        IMap.OnPointsSetEvent += ShowPath;
        UIController.OnGenerateGrid += Clear;
    }

    private void OnDisable()
    {
        IMap.OnPointsSetEvent -= ShowPath;
        UIController.OnGenerateGrid -= Clear;
    }

    private void Start() => lineRenderer = GetComponent<LineRenderer>();

    private void Clear(int _, int __)
    {
        foreach (var marker in pathMarkerPool) marker.SetActive(false);
        lineRenderer.positionCount = 0;
    }

    private void ShowPath(ICell cellStart, ICell cellEnd, IMap map)
    {
        if (cellStart == null || cellEnd == null) return;

        var path = FindPathOnMap(cellStart, cellEnd, map);
        int markerIndex = 0;
        int pathSize = path.Count;

        for(int i = 1; i < path.Count - 1; i++)
        {
            var position = path[i].GetPosition();

            if (markerIndex >= pathMarkerPool.Count)
            {
                var marker = Instantiate(pathMarker, new Vector3(position.x, position.y), Quaternion.identity, transform);
                pathMarkerPool.Add(marker);
            }
            else
            {
                var marker = pathMarkerPool[markerIndex];
                marker.transform.position = new Vector3(position.x, position.y);
                marker.SetActive(true);
            }

            pathMarkerPool[markerIndex++].GetComponent<SpriteRenderer>().color = pathColor.Evaluate((float)i / pathSize);
        }

        for (int i = markerIndex; i < pathMarkerPool.Count; i++) pathMarkerPool[i].SetActive(false);

        lineRenderer.positionCount = path.Count;
        lineRenderer.SetPositions(path.ConvertAll(cell => new Vector3(cell.GetPosition().x, cell.GetPosition().y, -1)).ToArray());

        OnPathGenerated?.Invoke(path.Count - 1);    // -1 to exclude the start cell from the path length
    }

    // A* algorithm to find the shortest path between two cells
    public List<ICell> FindPathOnMap(ICell source, ICell target, IMap map)
    {
        ClearPathFinderData(map);

        // Grabbing distance data from the map
        NeighbourData neighboursData = map.GetNeighbourData();
        int verticalOffset = neighboursData.verticalOffset;
        Vector2Int diagonalOffset = neighboursData.diagonalOffset;

        // Initializing the start cell
        source.GetPathfinderData().Setup(0, H(source.GetPosition(), target.GetPosition(), verticalOffset, diagonalOffset), null);

        List<ICell> openList = new List<ICell> { source };
        List<ICell> closedList = new List<ICell>();

        while (openList.Count > 0)
        {
            ICell lowestFCell = GetLowestFCell(openList);
            if (lowestFCell == target) return ReconstructPath(lowestFCell);

            openList.Remove(lowestFCell);
            closedList.Add(lowestFCell);

            foreach (var neighbour in lowestFCell.GetNeighboursList())
            {
                if (!neighbour.IsWalkable()) continue;
                if (closedList.Contains(neighbour)) continue;

                int g = lowestFCell.GetPathfinderData().g + 1;
                if (g < neighbour.GetPathfinderData().g)
                {
                    neighbour.GetPathfinderData().Setup(g, H(neighbour.GetPosition(), target.GetPosition(), verticalOffset, diagonalOffset), lowestFCell);
                    if (!openList.Contains(neighbour)) openList.Add(neighbour);
                }
            }
        }
        return new();
    }

    public void ClearPathFinderData(IMap map)
    {
        foreach (var cell in map.GetCells()) cell.GetPathfinderData().Reset();
    }

    public List<ICell> ReconstructPath(ICell cell)
    {
        List<ICell> path = new List<ICell>();
        while (cell != null)
        {
            path.Add(cell);
            cell = cell.GetPathfinderData().parent;
        }
        path.Reverse();
        return path;
    }

    private ICell GetLowestFCell(List<ICell> openList)
    {
        ICell lowestFCell = openList[0];
        foreach (var cell in openList)
        {
            if (cell.GetPathfinderData().f < lowestFCell.GetPathfinderData().f) lowestFCell = cell;
        }
        return lowestFCell;
    }

    // Evaluate shortest path between start and end ignoring obstacles in O(1)
    private int H(Vector2Int start, Vector2Int end, int verticalDistance, Vector2Int diagonalDistance)
    {
        int diagonalSteps = Mathf.Abs(start.x - end.x) / diagonalDistance.x;
        int deltaY = Mathf.Abs(start.y - end.y);
        int maxMovementY = diagonalSteps * diagonalDistance.y;
        if (deltaY < maxMovementY) return diagonalSteps;
        deltaY -= maxMovementY;
        return diagonalSteps + deltaY / verticalDistance;
    }
}
