using System;
using System.Collections.Generic;
using UnityEngine;

public class CardQuestions : MonoBehaviour
{
    public static CardQuestions Instance = null;
    public CardPages cardPages;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void nextQuestionPage()
    {
        LogController.Instance?.debug("next question page");
    }

    public void GetAllQuestionAnswers(int numberOfQuestions = 0, GenerateCard cardManager=null)
    {
        if (LoaderConfig.Instance == null || QuestionManager.Instance == null)
            return;

        try
        {
            var questionDataList = QuestionManager.Instance.questionData;
            LogController.Instance?.debug("Loaded questions:" + questionDataList.questions.Count);
            if (questionDataList == null || questionDataList.questions == null || questionDataList.questions.Count == 0)
            {
                return;
            }

            
            this.cardPages.totalPages = questionDataList.questions.Count / numberOfQuestions;
            this.cardPages.pages = new List<Cards>();

            for (int i=0; i< this.cardPages.totalPages; i++)
            {
                var _cards = new Cards();
                _cards.name = "Page_" + i;
                for (int j = 0; j < numberOfQuestions; j++)
                {
                    //int questionCount = questionDataList.questions.Count;
                    bool isLogined = LoaderConfig.Instance.apiManager.IsLogined;

                    if (isLogined)
                    {

                    }
                    else
                    {
                        QuestionList _qa = questionDataList.questions[(i * numberOfQuestions) + j];
                        _cards.qa.Add(_qa);

                        // First card for the question content
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

                        // Second card for the correct answer
                        cardManager.cards[j * 2 + 1].setContent(CardType.Answer, _qa.correctAnswer, null, null, _qa.correctAnswer, _qa.qid, _qa);
                    }
                }
                this.cardPages.pages.Add(_cards);
            }

            cardManager.ShuffleGridElements(); // sort the layout
            cardManager.remainQuestions = numberOfQuestions;
        }
        catch (Exception e)
        {
            LogController.Instance?.debugError(e.Message);
        }
    }

    public void GetNewPageQuestions(int numberOfQuestions = 0, GenerateCard cardManager = null)
    {

        if (this.cardPages.currentPage < this.cardPages.totalPages)
        {
            this.cardPages.currentPage += 1;
        }

        var newPageCards = this.cardPages.pages[this.cardPages.currentPage];

        for (int j = 0; j < numberOfQuestions; j++)
        {
            bool isLogined = LoaderConfig.Instance.apiManager.IsLogined;

            if (isLogined)
            {

            }
            else
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
        }

        cardManager.ShuffleGridElements();
        cardManager.remainQuestions = numberOfQuestions;
        cardManager.ResetAllCards();
    }

    /*
    public void PlayCurrentQuestionAudio()
    {
        this.currentQuestion.playAudio();
    }

    public void PlayCurrentQuestionAudio(AudioSource audio)
    {
        if (audio != null)
        {
            audio.clip = this.currentQuestion.currentAudioClip;
            audio.Play();
        }
    }*/

}

[Serializable]
public class Cards
{
    public string name;
    public List<QuestionList> qa = new List<QuestionList>();
    public List<Card> cards = new List<Card>();
    public bool isStayed = false;
}

[Serializable]
public class CardPages
{
    public int totalPages = 0;
    public int currentPage = 0;
    public List<Cards> pages;
}
