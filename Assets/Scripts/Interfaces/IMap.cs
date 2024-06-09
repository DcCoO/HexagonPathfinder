using System;
using System.Collections.Generic;
using UnityEngine;

public interface IMap
{
    public void SetStartPoint(ICell cell);

    public void SetEndPoint(ICell cell);

    public IEnumerable<ICell> GetCells();

    public Vector2Int GetGridPosition(ICell cell);

    public Vector2Int GetGridSize();

    public NeighbourData GetNeighbourData();

    public static event Action<ICell, ICell, IMap> OnPointsSetEvent;
    public static void InvokeOnPointsSet(ICell cellStart, ICell cellEnd, IMap map) => OnPointsSetEvent?.Invoke(cellStart, cellEnd, map);
}