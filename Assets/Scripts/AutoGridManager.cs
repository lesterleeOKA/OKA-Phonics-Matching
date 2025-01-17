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
        // 獲取 GridLayoutGroup 的總寬度和高度
        float totalWidth = gridLayoutGroup.GetComponent<RectTransform>().rect.width;
        float totalHeight = gridLayoutGroup.GetComponent<RectTransform>().rect.height;

        // 計算每行的最大格子數
        int maxColumns = Mathf.CeilToInt((float)totalQuestions * 2 / maxRowCount);

        // 計算每個格子的大小
        float cellWidth = (totalWidth - (gridLayoutGroup.spacing.x * (maxColumns - 1))) / maxColumns;
        float cellHeight = (totalHeight - (gridLayoutGroup.spacing.y * (maxRowCount - 1))) / maxRowCount;

        // 設置 GridLayoutGroup 的 cellSize
        gridLayoutGroup.cellSize = new Vector2(cellWidth, cellHeight);
    }

    private void GenerateGrid()
    {
        // 每個問題需要兩個格子
        int totalCells = totalQuestions * 2;

        for (int i = 0; i < totalCells; i++)
        {
            // 在指定位置生成格子
            GameObject cell = Instantiate(cellPrefab, gridLayoutGroup.transform);

            // 設置格子的大小（根據計算的 cellSize）
            RectTransform rectTransform = cell.GetComponent<RectTransform>();
            rectTransform.sizeDelta = gridLayoutGroup.cellSize;
        }
    }
}
