using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

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
    public int answeredQuestionsCount = 0;
    private Vector2 originalGridLayoutSpacing = Vector2.zero;

    private void CalculateCellSize(int questionsNumber)
    {
        if(this.gridLayoutGroup != null)
        {
            float totalWidth = this.gridLayoutGroup.GetComponent<RectTransform>().rect.width;
            float totalHeight = this.gridLayoutGroup.GetComponent<RectTransform>().rect.height;
            float cellWidth = (totalWidth - (this.gridLayoutGroup.spacing.x * (questionsNumber - 1))) / questionsNumber;
            float cellHeight = (totalHeight - (this.gridLayoutGroup.spacing.y * (maxRowCount - 1))) / maxRowCount;

            if (questionsNumber > 4)
            {
                float spacing = Mathf.Clamp(questionsNumber * 20, 0f, 110f);
                //this.OriginalGridLayoutSpacing = new Vector2(spacing, this.gridLayoutGroup.spacing.y);
                this.gridLayoutGroup.cellSize = new Vector2(cellWidth, cellHeight);
            }
            else
            {
                //this.OriginalGridLayoutSpacing = new Vector2(100f, this.gridLayoutGroup.spacing.y);
                this.gridLayoutGroup.cellSize = new Vector2(400f, 538f);
            }
           // this.OriginalGridLayoutSpacing = this.gridLayoutGroup.spacing;
           // this.gridLayoutGroup.spacing = this.OriginalGridLayoutSpacing;
        }
    }

    Vector2 OriginalGridLayoutSpacing
    {
        set { 
            this.originalGridLayoutSpacing = value;
        }
        get
        {
            return this.originalGridLayoutSpacing;
        }
    }

    public void CreateCard(int questionsNumber = 0, Texture _cardFrontTex = null, Texture _cardBackTex = null)
    {
        this.totalCards = questionsNumber * 2;
        this.CalculateCellSize(questionsNumber);
        this.flickedCards.Clear();
        this.cards.Clear();
        int totalCells = questionsNumber * 2;

        for (int i = 0; i < totalCells; i++)
        {
            GameObject cell = GameObject.Instantiate(this.cellPrefab, this.gridLayoutGroup.transform);
            cell.name = "Cell_" + i;
            RectTransform rectTransform = cell.GetComponent<RectTransform>();
            rectTransform.sizeDelta = this.gridLayoutGroup.cellSize;
            Card card = cell.GetComponent<Card>();
            if (card != null)
            {
                card.OnCardClick += FlickedNumberOfCard;
                card.setCardImage(_cardFrontTex, _cardBackTex);
                this.cards.Add(card);
            }
        }
    }

    void FlickedNumberOfCard(Card clickedCard)
    {
        clickedCard.Flick(true, 0f, ()=>
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
                                               this.RemovPairedCards(questionController);
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
            case CardsStatus.paired:
                this.disablePairedCards();
                if (player != null) player.playerReset();
                break;
            case CardsStatus.reset:
                this.ResetFlickedCards();
                if(player != null) player.playerReset();
                break;
        }  
    }

    public void RemovPairedCards(CardQuestions questionController = null)
    {
        if (!this.flickedCards[0].question.isDone && 
            !this.flickedCards[1].question.isDone)
        {
            if (questionController != null)
            {
                bool updating = questionController.updateProgressiveBar();
                if (updating)
                {
                    LogController.Instance?.debug("Suceess paired, paired will be remove");
                    var qaList = questionController.cardPages.currentPageQuestions;
                    for (int i = 0; i < qaList.Count; i++)
                    {
                        if (this.flickedCards[0].qid == qaList[i].qid)
                        {
                            qaList[i].isDone = true;
                        }
                    }
                }
            }
        }

        foreach(var card in this.cards)
        {
            if (this.flickedCards.Contains(card))
            { 
                card.dissolveCard();
            }
        }

        if(this.remainQuestions > 0)
        {
            this.answeredQuestionsCount += 1;
            this.remainQuestions -= 1;
        }


        this.cardsStatus = CardsStatus.paired;
    }

    public void disablePairedCards()
    {
        LogController.Instance?.debug("Disable Paired Cards");
        for (int i = 0; i < this.cards.Count; i++)
        {
            if (this.cards[i] != null && !this.cards[i].isDone)
            {
                this.cards[i].cardStatus = CardStatus.hidden;
            }
        }
        this.flickedCardNumber = 0;
        this.flickedCards.Clear();
        this.cardsStatus = CardsStatus.ready;
    }

    public void ResetFlickedCards()
    {
        LogController.Instance?.debug("Cards are reset");
        for (int i = 0; i < this.flickedCards.Count; i++)
        {
            this.flickedCards[i].ResetFlick();
        }
        this.disablePairedCards();
    }

    public void ResetAllCards()
    {
        //this.gridLayoutGroup.spacing = this.OriginalGridLayoutSpacing;
        LogController.Instance?.debug("All Cards are reset");
        this.flickedCardNumber = 0;
        this.flickedCards.Clear();
        for (int i = 0; i < this.cards.Count; i++)
        {
            this.cards[i].ResetFlick();
        }
        this.cardsStatus = CardsStatus.ready;
    }

    public void ShuffleGridElements(int numberOfQuestions)
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
        this.remainQuestions = numberOfQuestions;
        this.answeredQuestionsCount = 0;
    }

}


public enum CardsStatus
{
    ready,
    checking,
    paired,
    reset
}
