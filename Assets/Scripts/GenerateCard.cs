using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[Serializable]
public class GenerateCard
{
    public GameObject cellPrefab;
    public int totalQuestions = 16;
    public GridLayoutGroup gridLayoutGroup;
    public int maxRowCount = 2;
    public List<Card> cards = new List<Card>();

    private void CalculateCellSize()
    {
        float totalWidth = gridLayoutGroup.GetComponent<RectTransform>().rect.width;
        float totalHeight = gridLayoutGroup.GetComponent<RectTransform>().rect.height;
        int maxColumns = Mathf.CeilToInt((float)totalQuestions * 2 / maxRowCount);
        float cellWidth = (totalWidth - (gridLayoutGroup.spacing.x * (maxColumns - 1))) / maxColumns;
        float cellHeight = (totalHeight - (gridLayoutGroup.spacing.y * (maxRowCount - 1))) / maxRowCount;
        gridLayoutGroup.cellSize = new Vector2(cellWidth, cellHeight);
    }

    public void CreateCard(int totalQuestions = 0, Sprite cardSprite = null)
    {
        this.totalQuestions = totalQuestions;
        this.CalculateCellSize();
        this.cards.Clear();
        int totalCells = totalQuestions * 2;

        for (int i = 0; i < totalCells; i++)
        {
            GameObject cell = GameObject.Instantiate(cellPrefab, gridLayoutGroup.transform);
            cell.name = "Cell_" + i;
            RectTransform rectTransform = cell.GetComponent<RectTransform>();
            rectTransform.sizeDelta = gridLayoutGroup.cellSize;
            Card card = cell.GetComponent<Card>();
            card.setCardImage(cardSprite);
            this.cards.Add(card);
        }
    }

    public void ShuffleGridElements()
    {
        int childCount = this.gridLayoutGroup.transform.childCount;
        Transform[] children = new Transform[childCount];

        for (int i = 0; i < childCount; i++)
        {
            children[i] = this.gridLayoutGroup.transform.GetChild(i);
        }
        for (int i = childCount - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            Transform temp = children[i];
            children[i] = children[j];
            children[j] = temp;
        }

        foreach (Transform child in children)
        {
            child.SetAsLastSibling();
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.gridLayoutGroup.GetComponent<RectTransform>());
    }

}

