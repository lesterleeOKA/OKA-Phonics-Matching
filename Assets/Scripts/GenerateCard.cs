using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[Serializable]
public class GenerateCard
{
    public CardsStatus cardsStatus = CardsStatus.ready;
    public GameObject cellPrefab;
    public int totalCards = 16;
    public GridLayoutGroup gridLayoutGroup;
    public int maxRowCount = 2;
    public int flickedCardNumber = 0;
    public List<Card> flickedCards = new List<Card>();
    public List<Card> cards = new List<Card>();
    public int remainQuestions = 0;

    private void CalculateCellSize()
    {
        float totalWidth = this.gridLayoutGroup.GetComponent<RectTransform>().rect.width;
        float totalHeight = this.gridLayoutGroup.GetComponent<RectTransform>().rect.height;
        int maxColumns = Mathf.CeilToInt((float)totalCards * 2 / maxRowCount);
        float cellWidth = (totalWidth - (this.gridLayoutGroup.spacing.x * (maxColumns - 1))) / maxColumns;
        float cellHeight = (totalHeight - (this.gridLayoutGroup.spacing.y * (maxRowCount - 1))) / maxRowCount;
        this.gridLayoutGroup.cellSize = new Vector2(cellWidth, cellHeight);
    }

    public void CreateCard(int questionsNumber = 0, Sprite cardSprite = null)
    {
        this.totalCards = questionsNumber;
        this.CalculateCellSize();
        this.flickedCards.Clear();
        this.cards.Clear();
        int totalCells = questionsNumber * 2;

        for (int i = 0; i < totalCells; i++)
        {
            GameObject cell = GameObject.Instantiate(cellPrefab, this.gridLayoutGroup.transform);
            cell.name = "Cell_" + i;
            RectTransform rectTransform = cell.GetComponent<RectTransform>();
            rectTransform.sizeDelta = this.gridLayoutGroup.cellSize;
            Card card = cell.GetComponent<Card>();
            card.OnCardClick += FlickedNumberOfCard;
            card.setCardImage(cardSprite);
            this.cards.Add(card);
        }
    }

    void FlickedNumberOfCard(Card clickedCard)
    {
        clickedCard.Flick(true, ()=>
        {
            this.flickedCardNumber += 1;
            LogController.Instance?.debug($"Card with ID {clickedCard.qid} clicked!");
            this.flickedCards.Add(clickedCard);
        });
    }

    public void CheckingCardStatus(Timer gameTimer, PlayerController player=null, CardQuestions questionController=null)
    {

        switch (this.cardsStatus)
        {
            case CardsStatus.ready:
                if (this.flickedCardNumber == 2 && player != null)
                {
                    for (int i = 0; i < this.cards.Count; i++)
                    {
                        switch (this.cards[i].cardStatus)
                        {
                            case CardStatus.hidden:
                                this.cards[i].cardStatus = CardStatus.checking;
                                break;
                        }
                    }

                    if (this.flickedCards.Count == 2 && 
                        !this.flickedCards[0].isAnimated && 
                        !this.flickedCards[1].isAnimated)
                    {
                        int currentTime = Mathf.FloorToInt(((gameTimer.gameDuration - gameTimer.currentTime) / gameTimer.gameDuration) * 100);

                        player.checkAnswer(currentTime,
                                           this.flickedCards[0].question,
                                           this.flickedCards[1].question,
                                           () => 
                                           {
                                               if(questionController != null)
                                               {
                                                   bool updating = questionController.updateProgressiveBar();
                                                   if (updating)
                                                   {
                                                       this.RemovPairedCards();
                                                   }
                                               }
                                           },
                                           () =>
                                           {
                                               this.cardsStatus = CardsStatus.reset;
                                           });
                        this.cardsStatus = CardsStatus.checking;
                    }
                }
                break;
            case CardsStatus.checking:
                LogController.Instance?.debug("Cards are checking the matching");
                break;
            case CardsStatus.reset:
                this.ResetFlickedCards();
                if(player != null) player.playerReset();
                break;
        }  
    }

    public void RemovPairedCards()
    {
        LogController.Instance?.debug("Suceess paired, paired will be remove");

        foreach(var card in this.cards)
        {
            if (this.flickedCards.Contains(card))
            { 
                card.gameObject.SetActive(false);
            }
        }

        if(this.remainQuestions > 0)
        {
            this.remainQuestions -= 1;
        }


        this.cardsStatus = CardsStatus.reset;
    }

    public void ResetFlickedCards()
    {
        LogController.Instance?.debug("Cards are reset");
        for (int i = 0; i < this.flickedCards.Count; i++)
        {
            this.flickedCards[i].ResetFlick();
        }
        this.flickedCardNumber = 0;
        this.flickedCards.Clear();
        for (int i = 0; i < this.cards.Count; i++)
        {
            this.cards[i].cardStatus = CardStatus.hidden;
        }
        this.cardsStatus = CardsStatus.ready;
    }

    public void ResetAllCards()
    {
        LogController.Instance?.debug("All Cards are reset");
        this.flickedCardNumber = 0;
        this.flickedCards.Clear();
        for (int i = 0; i < this.cards.Count; i++)
        {
            this.cards[i].ResetFlick();
        }
        this.cardsStatus = CardsStatus.ready;
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
        this.cardsStatus = CardsStatus.ready;
    }

}


public enum CardsStatus
{
    ready,
    checking,
    reset
}
