using System;
using System.Collections.Generic;
using UnityEngine;

public interface ICell : IComparable<ICell>
{
    public void Setup(Vector2Int position, bool isWalkable, SpriteGroup spriteGroup);

    public ECellType GetCellType();

    public void SetCellType(ECellType cellType);

    public void AddNeighbour(ICell cell);

    public bool IsWalkable();

    public bool IsPoint();  // Is it the source point or target point?

    public IEnumerable<ICell> GetNeighboursList();

    public GameObject GetGameObject();

    public Vector2Int GetPosition();

    public void CleanPoints();

    public void SetStartPoint(SpriteGroup spriteGroup);

    public void SetEndPoint(SpriteGroup spriteGroup);

    public PathFinderData GetPathfinderData();
}