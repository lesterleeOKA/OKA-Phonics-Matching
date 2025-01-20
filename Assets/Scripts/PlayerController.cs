using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : UserData
{
    //public BloodController bloodController;
    public Scoring scoring;
    public string answer = string.Empty;
    public bool IsCorrect = false;
    public bool IsTriggerToNextQuestion = false;
    public bool IsCheckedAnswer = false;
    public Image answerBoxFrame;
    public float speed;
    private CharacterAnimation characterAnimation = null;
    private TextMeshProUGUI answerBox = null;
    public List<Card> flickedCard = new List<Card>();
    public float countGetAnswerAtStartPoints = 2f;
    private float countAtStartPoints = 0f;



    public void Init(CharacterSet characterSet = null)
    {
        this.countAtStartPoints = this.countGetAnswerAtStartPoints;
        this.updateRetryTimes(false);
        this.characterAnimation = this.GetComponent<CharacterAnimation>();
        this.characterAnimation.characterSet = characterSet;

        /*if (this.bloodController == null)
        {
            this.bloodController = GameObject.FindGameObjectWithTag("P" + this.RealUserId + "_Blood").GetComponent<BloodController>();
        }
        */
        if (this.PlayerIcons[0] == null)
        {
            this.PlayerIcons[0] = GameObject.FindGameObjectWithTag("P" + this.RealUserId + "_Icon").GetComponent<PlayerIcon>();
        }

        if (this.scoring.scoreTxt == null)
        {
            this.scoring.scoreTxt = GameObject.FindGameObjectWithTag("P" + this.RealUserId + "_Score").GetComponent<TextMeshProUGUI>();
        }

        /*if (this.scoring.answeredEffectTxt == null)
        {
            this.scoring.answeredEffectTxt = GameObject.FindGameObjectWithTag("P" + this.RealUserId + "_AnswerScore").GetComponent<TextMeshProUGUI>();
        }*/

        if (this.scoring.resultScoreTxt == null)
        {
            this.scoring.resultScoreTxt = GameObject.FindGameObjectWithTag("P" + this.RealUserId + "_ResultScore").GetComponent<TextMeshProUGUI>();
        }

        this.scoring.init();
    }

    void updateRetryTimes(bool deduct = false)
    {
        if (deduct)
        {
            if (this.Retry > 0)
            {
                this.Retry--;
            }

            /*if (this.bloodController != null)
            {
                this.bloodController.setBloods(false);
            }*/
        }
        else
        {
            this.NumberOfRetry = LoaderConfig.Instance.gameSetup.retry_times;
            this.Retry = this.NumberOfRetry;
        }
    }

    public void updatePlayerIcon(bool _status = false, string _playerName = "", Sprite _icon = null)
    {
        for (int i = 0; i < this.PlayerIcons.Length; i++)
        {
            if (this.PlayerIcons[i] != null)
            {
                this.PlayerColor = this.characterAnimation.characterSet.playerColor;
                this.PlayerIcons[i].playerColor = this.characterAnimation.characterSet.playerColor;
                this.PlayerIcons[i].SetStatus(_status, _playerName, _icon);
            }
        }

    }


    string CapitalizeFirstLetter(string str)
    {
        if (string.IsNullOrEmpty(str)) return str; // Return if the string is empty or null
        return char.ToUpper(str[0]) + str.Substring(1).ToLower();
    }

    public void checkAnswer(int currentTime, Action onCompleted = null)
    {
        var currentQuestion = QuestionController.Instance?.currentQuestion;
        if(currentQuestion.answersChoics == null || currentQuestion.answersChoics.Length == 0) return;

        var getChoice = this.answerBox.text;
        var lowerQIDAns = currentQuestion.correctAnswer.ToLower();
        switch (getChoice)
        {
            case "A":
                this.answer = currentQuestion.answersChoics[0].ToLower();
                break;
            case "B":
                this.answer = currentQuestion.answersChoics[1].ToLower();
                break;
            case "C":
                this.answer = currentQuestion.answersChoics[2].ToLower();
                break;
            case "D":
                this.answer = currentQuestion.answersChoics[3].ToLower();
                break;
        }
        if(this.answer != lowerQIDAns) 
            return;

        if (!this.IsCheckedAnswer)
        {
            this.IsCheckedAnswer = true;
            var loader = LoaderConfig.Instance;
            int eachQAScore = currentQuestion.qa.score.full == 0 ? 10 : currentQuestion.qa.score.full;
            int currentScore = this.Score;

            int resultScore = this.scoring.score(this.answer, currentScore, lowerQIDAns, eachQAScore);
            this.Score = resultScore;
            this.IsCorrect = this.scoring.correct;
            StartCoroutine(this.showAnswerResult(this.scoring.correct,()=>
            {
                if (this.UserId == 0 && loader != null && loader.apiManager.IsLogined) // For first player
                {
                    float currentQAPercent = 0f;
                    int correctId = 0;
                    float score = 0f;
                    float answeredPercentage;
                    int progress = (int)((float)currentQuestion.answeredQuestion / QuestionManager.Instance.totalItems * 100);

                    if (this.answer == lowerQIDAns)
                    {
                        if (this.CorrectedAnswerNumber < QuestionManager.Instance.totalItems)
                            this.CorrectedAnswerNumber += 1;

                        correctId = 2;
                        score = eachQAScore; // load from question settings score of each question

                        LogController.Instance?.debug("Each QA Score!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" + eachQAScore + "______answer" + this.answer);
                        currentQAPercent = 100f;
                    }
                    else
                    {
                        if (this.CorrectedAnswerNumber > 0)
                        {
                            this.CorrectedAnswerNumber -= 1;
                        }
                    }

                    if (this.CorrectedAnswerNumber < QuestionManager.Instance.totalItems)
                    {
                        answeredPercentage = this.AnsweredPercentage(QuestionManager.Instance.totalItems);
                    }
                    else
                    {
                        answeredPercentage = 100f;
                    }

                    loader.SubmitAnswer(
                               currentTime,
                               this.Score,
                               answeredPercentage,
                               progress,
                               correctId,
                               currentTime,
                               currentQuestion.qa.qid,
                               currentQuestion.correctAnswerId,
                               this.CapitalizeFirstLetter(this.answer),
                               currentQuestion.correctAnswer,
                               score,
                               currentQAPercent,
                               onCompleted
                               );
                }
                else
                {
                   onCompleted?.Invoke();
                }
            }));
        }
    }

    public void resetRetryTime()
    {
        this.scoring.resetText();
        this.updateRetryTimes(false);
       // this.bloodController.setBloods(true);
        this.IsTriggerToNextQuestion = false;
    }

    public IEnumerator showAnswerResult(bool correct, Action onCompleted = null)
    {
        float delay = 2f;
        if (correct)
        {
            GameController.Instance?.PrepareNextQuestion();
            LogController.Instance?.debug("Add marks" + this.Score);
            GameController.Instance?.setGetScorePopup(true);
            AudioController.Instance?.PlayAudio(1);
            yield return new WaitForSeconds(delay);
            GameController.Instance?.setGetScorePopup(false);
            GameController.Instance?.UpdateNextQuestion();
        }
        else
        {
            GameController.Instance?.setWrongPopup(true);
            AudioController.Instance?.PlayAudio(2);
            this.updateRetryTimes(true);
            yield return new WaitForSeconds(delay);
            GameController.Instance?.setWrongPopup(false);
            if (this.Retry <= 0)
            {
                this.IsTriggerToNextQuestion = true;
            }
        }
        this.scoring.correct = false;

        onCompleted?.Invoke();
    }

   
    public void playerReset()
    {     
        this.deductAnswer();
        this.setAnswer("");
        this.IsCheckedAnswer = false;
        this.IsCorrect = false;
    }

    public void setAnswer(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            this.answer = "";
        }
        else
        {
            this.answer = content;
        }
    }

    public void autoDeductAnswer()
    {
        if(this.flickedCard.Count > 0) {
            if (this.countAtStartPoints > 0f)
            {
                this.countAtStartPoints -= Time.deltaTime;
            }
            else
            {
                this.deductAnswer();
                this.countAtStartPoints = this.countGetAnswerAtStartPoints;
            }
        }
        else
        {
            this.countAtStartPoints = this.countGetAnswerAtStartPoints;
        }
    }

    public void deductAnswer()
    {
        if (this.answer.Length > 0)
        {
            this.setAnswer("");
        }
    }

}
