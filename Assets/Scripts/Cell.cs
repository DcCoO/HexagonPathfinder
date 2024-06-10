using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour, ICell
{
    [SerializeField] private SpriteRenderer background;
    [SerializeField] private SpriteRenderer foreground;
    [SerializeField] private SpriteRenderer overlay;
    [SerializeField] private SpriteRenderer pointIndicator;

    private ECellType cellType;
    private Vector2Int position;
    private List<ICell> neighbours = new List<ICell>();
    private PathFinderData pathFinderData = new PathFinderData();

    public bool IsWalkable() => cellType != ECellType.NON_WALKABLE && gameObject.activeSelf;

    public bool IsPoint() => cellType == ECellType.SOURCE || cellType == ECellType.TARGET;

    public ECellType GetCellType() => cellType;

    public void SetCellType(ECellType cellType) => this.cellType = cellType;

    public IEnumerable<ICell> GetNeighboursList() => neighbours;

    public void Setup(Vector2Int position, bool isWalkable, SpriteGroup spriteGroup)
    {
        transform.position = new Vector2(position.x, position.y);
        cellType = isWalkable ? ECellType.WALKABLE : ECellType.NON_WALKABLE;
        this.position = position;
        SetSprites(spriteGroup);
    }

    private void SetSprites(SpriteGroup spriteGroup)
    {
        foreground.sprite = spriteGroup.foreground;
        background.sprite = spriteGroup.background;
        overlay.sprite = spriteGroup.overlay;
        pointIndicator.sprite = spriteGroup.pointIndicator;
    }

    public void AddNeighbour(ICell cell) 
    {
        if (neighbours.Contains(cell)) return;
        neighbours.Add(cell);
    }
    
    public GameObject GetGameObject() => gameObject;

    public Vector2Int GetPosition() => position;

    public void CleanPoints()
    {        
        cellType = ECellType.WALKABLE;
        pointIndicator.sprite = null;        
    }

    public void SetStartPoint(SpriteGroup spriteGroup)
    {
        if (cellType == ECellType.TARGET) return;
        cellType = ECellType.SOURCE;
        pointIndicator.sprite = spriteGroup.pointIndicator;
    }

    public void SetEndPoint(SpriteGroup spriteGroup)
    {
        if (cellType == ECellType.SOURCE) return;
        cellType = ECellType.TARGET;
        pointIndicator.sprite = spriteGroup.pointIndicator;
    }

    public int CompareTo(ICell other)
    {
        var otherPosition = other.GetPosition();
        return (position.x == otherPosition.x && position.y == otherPosition.y) ? 0 : 1;
    }

    public PathFinderData GetPathfinderData() => pathFinderData;
}
