using UnityEngine;
using UnityEngine.UI;

public class AutoGridManager : MonoBehaviour
{
    public GameObject cellPrefab; // 用於生成每個格子的預製件
    public int totalQuestions = 16; // 問題的總數
    public GridLayoutGroup gridLayoutGroup; // 引用 GridLayoutGroup
    public int maxRowCount = 2; // 最大行數

    private void Start()
    {
        CalculateCellSize();
        GenerateGrid();
    }

    private void CalculateCellSize()
    {
        float totalWidth = gridLayoutGroup.GetComponent<RectTransform>().rect.width;
        float totalHeight = gridLayoutGroup.GetComponent<RectTransform>().rect.height;
        int maxColumns = Mathf.CeilToInt((float)totalQuestions * 2 / maxRowCount);
        float cellWidth = (totalWidth - (gridLayoutGroup.spacing.x * (maxColumns - 1))) / maxColumns;
        float cellHeight = (totalHeight - (gridLayoutGroup.spacing.y * (maxRowCount - 1))) / maxRowCount;
        gridLayoutGroup.cellSize = new Vector2(cellWidth, cellHeight);
    }

    private void GenerateGrid()
    {
        int totalCells = totalQuestions * 2;

        for (int i = 0; i < totalCells; i++)
        {
            GameObject cell = Instantiate(cellPrefab, gridLayoutGroup.transform);
            RectTransform rectTransform = cell.GetComponent<RectTransform>();
            rectTransform.sizeDelta = gridLayoutGroup.cellSize;
        }
    }
}
