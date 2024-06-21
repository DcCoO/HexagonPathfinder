using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static event Action<int, int> OnGenerateGrid;

    [Header("Grid Info"), SerializeField] private int rows;
    [SerializeField] private int columns;

    [Header("Values"), SerializeField] private string successMessage;
    [SerializeField] private string failureMessage;

    [Header("References"), SerializeField] private Button rowsMinusButton;
    [SerializeField] private Button columnsMinusButton;

    [SerializeField] private Text rowsText;
    [SerializeField] private Text columnsText;
    [SerializeField] private Text messageText;
    [SerializeField] private Text startCellPositionText;
    [SerializeField] private Text endCellPositionText;

    [SerializeField] private GameObject helpPanel;

    private Coroutine messageCoroutine;

    private void OnEnable() 
    {
        Pathfinder.OnPathGenerated += ShowMessage;
        IMap.OnPointsSetEvent += UpdatePointsPositions;
    }

    private void OnDisable()
    {
        Pathfinder.OnPathGenerated -= ShowMessage;
        IMap.OnPointsSetEvent -= UpdatePointsPositions;
    }

    private void Start()
    {
        rowsText.text = rows.ToString();
        columnsText.text = columns.ToString();
        messageText.text = string.Empty;
        UpdatePointsPositions(null, null, null);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) helpPanel.SetActive(!helpPanel.activeSelf);
    }

    public void AddRows(int value)
    {
        rowsMinusButton.interactable = rows + value > 2;
        rows += value;
        rowsText.text = rows.ToString();
    }

    public void AddColumns(int value)
    {
        columnsMinusButton.interactable = columns + value > 2;
        columns += value;
        columnsText.text = columns.ToString();
    }

    public void GenerateGrid()
    {
        UpdatePointsPositions(null, null, null);
        OnGenerateGrid?.Invoke(rows, columns);
    }

    public void UpdatePointsPositions(ICell startCell, ICell endCell, IMap map)
    {
        startCellPositionText.text = startCell == null ? string.Empty : map.GetGridPosition(startCell).ToString();
        endCellPositionText.text = endCell == null ? string.Empty : map.GetGridPosition(endCell).ToString();
    }

    public void ShowMessage(int pathSize)
    {
        bool success = pathSize > 0;
        if (messageCoroutine != null) StopCoroutine(messageCoroutine);
        messageCoroutine = StartCoroutine(ShowMessageRoutine(success ? successMessage.Replace("#", pathSize.ToString()) : failureMessage));
    }

    private IEnumerator ShowMessageRoutine(string message)
    {
        messageText.text = message;
        messageText.color = Color.white;
        yield return new WaitForSeconds(2f);

        for (float t = 0; t < 2; t += Time.deltaTime)
        {
            messageText.color = Color.Lerp(Color.white, Color.clear, t);
            yield return null;
        }
    }

    public void RestartScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);    
}
