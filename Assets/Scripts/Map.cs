using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Map : MonoBehaviour, IMap
{
    public static event Action<IMap> OnGridGenerated;

    [SerializeField] private NeighbourData neighbourData;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private SpriteProvider spriteProvider;
    [SerializeField, Range(0f, 1f)] private float walkableChance;

    private Camera mainCamera;
    private Dictionary<(int, int), ICell> cells = new Dictionary<(int, int), ICell>();
    private List<Vector2Int> offsets;

    private int rows;
    private int columns;

    private ICell startCell;
    private ICell endCell;

    private void OnEnable() 
    {
        UIController.OnGenerateGrid += GenerateGrid;  
        mainCamera = Camera.main;
    }

    private void OnDisable() => UIController.OnGenerateGrid -= GenerateGrid;

    private void Start()
    {
        var diagonalOffset = neighbourData.diagonalOffset;
        var verticalOffset = neighbourData.verticalOffset;

        offsets = new List<Vector2Int> {
            new Vector2Int(0, verticalOffset),
            new Vector2Int(0, -verticalOffset),
            new Vector2Int(diagonalOffset.x, diagonalOffset.y),
            new Vector2Int(diagonalOffset.x, -diagonalOffset.y),
            new Vector2Int(-diagonalOffset.x, diagonalOffset.y),
            new Vector2Int(-diagonalOffset.x, -diagonalOffset.y)
        };
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            Vector2 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            SetPointOnMouse(mouseWorldPosition);            
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector2 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            SetPointOnMouse(mouseWorldPosition, false);
        }
    }

    private void SetPointOnMouse(Vector2 mouseWorldPosition, bool startPoint = true)
    {
        var columnSize = neighbourData.diagonalOffset.x;
        var columnPosition = GetRoundedWorldPosition(Mathf.RoundToInt(mouseWorldPosition.x), columnSize);

        int currentVerticalOffset = (columnPosition / columnSize) % 2 == 0 ? 0 : neighbourData.diagonalOffset.y;

        mouseWorldPosition.y -= currentVerticalOffset;
        var rowSize = neighbourData.verticalOffset;
        var rowPosition = GetRoundedWorldPosition(Mathf.RoundToInt(mouseWorldPosition.y), rowSize) + currentVerticalOffset;

        if (cells.TryGetValue((columnPosition, rowPosition), out ICell cell))
        {
            if (startPoint) SetStartPoint(cell);
            else SetEndPoint(cell);
        }
    }

    public Vector2Int GetGridSize() => new Vector2Int(rows, columns);

    /// <summary>Get world position of row or column, given its size and the mouse position.</summary>
    private int GetRoundedWorldPosition(int position, int size)
    {
        position += size / 2;
        return position - (position % size);
    }  

    public void GenerateGrid(int rows, int columns)
    {
        if (startCell != null) startCell.CleanPoints();
        if (endCell != null) endCell.CleanPoints();

        startCell = null;
        endCell = null;

        var diagonalOffset = neighbourData.diagonalOffset;
        int verticalOffset = neighbourData.verticalOffset;

        for (int i = 0, maxColumns = Mathf.Max(columns, this.columns); i < maxColumns; i++) 
        {
            int currentVerticalOffset = i % 2 == 0 ? 0 : diagonalOffset.y;
            int x = i * diagonalOffset.x;

            for (int j = 0, maxRows = Mathf.Max(rows, this.rows); j < maxRows; j++) 
            {
                Vector2Int position = new Vector2Int(x, currentVerticalOffset + j * verticalOffset);

                if (i >= columns || j >= rows) 
                {
                    cells[position.ToTuple()].GetGameObject().SetActive(false);
                    continue;
                }

                bool isWalkable = Random.value < walkableChance;

                if (cells.TryGetValue(position.ToTuple(), out ICell cell))
                {
                    cell.Setup(position, isWalkable, spriteProvider.GetSpriteGroup(isWalkable ? ECellType.WALKABLE : ECellType.NON_WALKABLE));
                    cell.GetGameObject().SetActive(true);
                    continue;
                }

                GameObject cellGameObject = Instantiate(cellPrefab, transform);
                var newCell = cellGameObject.GetComponent<ICell>();
                newCell.Setup(position, isWalkable, spriteProvider.GetSpriteGroup(isWalkable ? ECellType.WALKABLE : ECellType.NON_WALKABLE));
                cells[position.ToTuple()] = newCell;
                FillNeighbours(newCell);
            }
        }

        this.rows = rows;
        this.columns = columns;

        OnGridGenerated?.Invoke(this);
    }

    private void FillNeighbours(ICell cell)
    {
        foreach (var offset in offsets)
        {
            Vector2Int neighbourPosition = cell.GetPosition() + offset;
            if (cells.TryGetValue(neighbourPosition.ToTuple(), out ICell neighbourCell))
            {
                cell.AddNeighbour(neighbourCell);
                neighbourCell.AddNeighbour(cell);
            }
        }
    }

    public void SetStartPoint(ICell cell)
    {
        if (cell.IsPoint()) return;
        if (!cell.IsWalkable()) return;

        if (startCell != null) startCell.CleanPoints();
        startCell = cell;
        cell.SetStartPoint(spriteProvider.GetSpriteGroup(ECellType.SOURCE));

        IMap.InvokeOnPointsSet(startCell, endCell, this);
    }

    public void SetEndPoint(ICell cell)
    {
        if (cell.IsPoint()) return;
        if (!cell.IsWalkable()) return;

        if (endCell != null) endCell.CleanPoints();
        endCell = cell;
        cell.SetEndPoint(spriteProvider.GetSpriteGroup(ECellType.TARGET));

        IMap.InvokeOnPointsSet(startCell, endCell, this);
    }

    public NeighbourData GetNeighbourData() => neighbourData;

    public IEnumerable<ICell> GetCells() => cells.Values;

    public Vector2Int GetGridPosition(ICell cell)
    {
        var cellWorldPosition = cell.GetPosition();

        var columnSize = neighbourData.diagonalOffset.x;
        int columnIndex = cellWorldPosition.x / columnSize;

        int currentVerticalOffset = columnIndex % 2 == 0 ? 0 : neighbourData.diagonalOffset.y;
        int rowIndex = (cellWorldPosition.y - currentVerticalOffset) / neighbourData.verticalOffset;

        return new Vector2Int(rowIndex, columnIndex);
    }
}