using System;
using System.Collections.Generic;
using UnityEngine;

public class CardQuestions : MonoBehaviour
{
    public static CardQuestions Instance = null;
    public int totalPages = 0;
    public List<Cards> cardpages;
    public GenerateCard cardManager;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void nextQuestionPage()
    {
        LogController.Instance?.debug("next question page");
    }

    public void GetAllQuestionAnswers(int numberOfQuestions = 0, Sprite cardImage=null)
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

            this.totalPages = questionDataList.questions.Count / numberOfQuestions;
            this.cardManager.CreateCard(numberOfQuestions, cardImage);
            this.cardpages = new List<Cards>();

            for (int i=0; i< this.totalPages; i++)
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
                        QuestionList _qa = questionDataList.questions[j];
                        _cards.qa.Add(_qa);

                        // First card for the question content
                        switch (_qa.questionType)
                        {
                            case "picture":
                                this.cardManager.cards[j * 2].setContent(CardType.Image, _qa.correctAnswer, _qa.texture, null, _qa.correctAnswer, _qa.qid);
                                break;
                            case "audio":
                                this.cardManager.cards[j * 2].setContent(CardType.Audio, _qa.correctAnswer, null, _qa.audioClip, _qa.correctAnswer, _qa.qid);
                                break;
                            case "text":
                                this.cardManager.cards[j * 2].setContent(CardType.Text, _qa.correctAnswer, null, null, _qa.correctAnswer, _qa.qid);
                                break;
                            case "fillInBlank":
                                // You can add specific logic for fill-in-the-blank if needed
                                break;
                        }

                        // Second card for the correct answer
                        this.cardManager.cards[j * 2 + 1].setContent(CardType.Answer, _qa.correctAnswer, null, null, _qa.correctAnswer, _qa.qid);
                    }
                }
                this.cardpages.Add(_cards);
            }

            this.cardManager.ShuffleGridElements(); // sort the layout
        }
        catch (Exception e)
        {
            LogController.Instance?.debugError(e.Message);
        }
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
