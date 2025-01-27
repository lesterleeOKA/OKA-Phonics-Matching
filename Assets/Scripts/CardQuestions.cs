using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardQuestions : MonoBehaviour
{
    public CardPages cardPages;
    public int totalQuestions = 0;
    public int numberQuestion = 0;
    public int answeredQuestion = 0;
    public CanvasGroup progressiveBar;
    public Image progressFillImage;

    public void nextQuestionPage()
    {
        LogController.Instance?.debug("next question page");
    }

    public void GetAllQuestionAnswers(int numberOfQuestions = 0, GenerateCard cardManager = null)
    {
        if (LoaderConfig.Instance == null || QuestionManager.Instance == null)
            return;

        try
        {
            var questionDataList = QuestionManager.Instance.questionData;
            this.totalQuestions = questionDataList.questions.Count;
            LogController.Instance?.debug("Loaded questions:" + this.totalQuestions);
            if (questionDataList == null || questionDataList.questions == null || this.totalQuestions == 0)
            {
                return;
            }

            // Adjusted line to calculate total pages including any remainder
            this.cardPages.totalPages = (questionDataList.questions.Count + numberOfQuestions - 1) / numberOfQuestions;
            this.cardPages.pages = new List<Cards>();

            for (int i = 0; i < this.cardPages.totalPages; i++)
            {
                var _cards = new Cards();
                _cards.name = "Page_" + i;
                for (int j = 0; j < numberOfQuestions; j++)
                {
                    // Ensure we do not exceed the questions available
                    if ((i * numberOfQuestions) + j >= this.totalQuestions)
                        break;

                    QuestionList _qa = questionDataList.questions[(i * numberOfQuestions) + j];
                    _cards.qa.Add(_qa);                  
                }
                this.cardPages.pages.Add(_cards);
            }


            var firstPageCards = this.cardPages.pages[this.cardPages.currentPage];
            int actualQuestionNumber = firstPageCards.qa.Count;
            for (int j = 0; j < actualQuestionNumber; j++)
            {
                QuestionList _qa = firstPageCards.qa[j];
                switch (_qa.questionType)
                {
                    case "picture":
                        cardManager.cards[j * 2].setContent(CardType.Image, _qa.correctAnswer, _qa.texture, null, _qa.correctAnswer, _qa.qid, _qa);
                        break;
                    case "audio":
                        cardManager.cards[j * 2].setContent(CardType.Audio, _qa.correctAnswer, null, _qa.audioClip, _qa.correctAnswer, _qa.qid, _qa);
                        break;
                    case "text":
                        cardManager.cards[j * 2].setContent(CardType.Text, _qa.correctAnswer, null, null, _qa.correctAnswer, _qa.qid, _qa);
                        break;
                    case "fillInBlank":
                        // You can add specific logic for fill-in-the-blank if needed
                        break;
                }
                cardManager.cards[j * 2 + 1].setContent(CardType.Answer, _qa.correctAnswer, null, null, _qa.correctAnswer, _qa.qid, _qa);
            }

            cardManager.ShuffleGridElements(numberOfQuestions);
            this.setProgressiveBar(true);
        }
        catch (Exception e)
        {
            LogController.Instance?.debugError(e.Message);
        }
    }

    public void GetNewPageQuestions(int numberOfQuestions=0, GenerateCard cardManager = null, float delayResetCards=1f)
    {
        if (this.cardPages.currentPage < this.cardPages.totalPages-1)
        {
            this.cardPages.currentPage += 1;
        }
        else
        {
            return;
        }

        var newPageCards = this.cardPages.pages[this.cardPages.currentPage];
        int actualQuestionNumber = newPageCards.qa.Count;

        if(actualQuestionNumber < numberOfQuestions)
        {
            int neededQuestions = numberOfQuestions - actualQuestionNumber;
            List<QuestionList> availableQuestions = new List<QuestionList>();

            for (int i = 0; i < this.cardPages.currentPage; i++)
            {
                availableQuestions.AddRange(this.cardPages.pages[i].qa);
            }

            HashSet<QuestionList> selectedQuestions = new HashSet<QuestionList>();
            System.Random rand = new System.Random();
            while (selectedQuestions.Count < neededQuestions && availableQuestions.Count > 0)
            {
                int index = rand.Next(availableQuestions.Count);
                QuestionList selectedQuestion = availableQuestions[index];
                if (selectedQuestions.Add(selectedQuestion))
                {
                    newPageCards.qa.Add(selectedQuestion);
                }
                availableQuestions.RemoveAt(index);
            }
            actualQuestionNumber = newPageCards.qa.Count;
        }
        else
        {
            actualQuestionNumber = numberOfQuestions;
        }

        for (int j = 0; j < actualQuestionNumber; j++)
        {
            QuestionList _qa = newPageCards.qa[j];
            switch (_qa.questionType)
            {
                case "picture":
                    cardManager.cards[j * 2].setContent(CardType.Image, _qa.correctAnswer, _qa.texture, null, _qa.correctAnswer, _qa.qid, _qa);
                    break;
                case "audio":
                    cardManager.cards[j * 2].setContent(CardType.Audio, _qa.correctAnswer, null, _qa.audioClip, _qa.correctAnswer, _qa.qid, _qa);
                    break;
                case "text":
                    cardManager.cards[j * 2].setContent(CardType.Text, _qa.correctAnswer, null, null, _qa.correctAnswer, _qa.qid, _qa);
                    break;
                case "fillInBlank":
                    // You can add specific logic for fill-in-the-blank if needed
                    break;
            }
            cardManager.cards[j * 2 + 1].setContent(CardType.Answer, _qa.correctAnswer, null, null, _qa.correctAnswer, _qa.qid, _qa);
        }

        cardManager.ShuffleGridElements(numberOfQuestions);

    }

    public void setProgressiveBar(bool status)
    {
        if (this.progressiveBar != null)
        {
            this.progressiveBar.DOFade(status ? 1f : 0f, 0f);
            this.progressiveBar.GetComponentInChildren<NumberCounter>().Init(this.numberQuestion.ToString(), "/" + this.totalQuestions);
        }
    }

    public bool updateProgressiveBar(Action onQuestionCompleted = null)
    {
        bool updating = true;
        float progress = 0f;
        if (this.numberQuestion < this.totalQuestions)
        {
            this.answeredQuestion += 1;
            progress = (float)this.answeredQuestion / this.totalQuestions;
            updating = true;
        }
        else
        {
            progress = 1f;
            updating = false;
        }

        progress = Mathf.Clamp(progress, 0f, 1f);
        if (this.progressFillImage != null && this.progressiveBar != null)
        {
            this.progressFillImage.DOFillAmount(progress, 0.5f).OnComplete(() =>
            {
                if (progress >= 1f) GameController.Instance.endGame();
            });

            this.progressiveBar.GetComponentInChildren<NumberCounter>().Unit = "/" + this.totalQuestions;
            this.progressiveBar.GetComponentInChildren<NumberCounter>().Value = this.answeredQuestion;
        }
        return updating;
    }

}

[Serializable]
public class Cards
{
    public string name;
    public List<QuestionList> qa = new List<QuestionList>();
}

[Serializable]
public class CardPages
{
    public int totalPages = 0;
    public int currentPage = 0;
    public List<Cards> pages;

    public List<QuestionList> currentPageQuestions
    {
        get
        {
            return this.pages[this.currentPage].qa;
        }
    }
}
