using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
public class GameController : GameBaseController
{
    public static GameController Instance = null;
    public CharacterSet[] characterSets;
    public GridManager gridManager;
    public Cell[,] grid;
    public GameObject playerPrefab;
    public Transform parent;
    public List<PlayerController> playerControllers = new List<PlayerController>();
    public bool showCells = false;
    public CanvasGroup[] audioTypeButtons, fillInBlankTypeButtons;
    public TextMeshProUGUI choiceText;

    protected override void Awake()
    {
        if (Instance == null) Instance = this;
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        this.CreateGrids();
    }

    void CreateGrids()
    {
        Sprite gridTexture = LoaderConfig.Instance.gameSetup.gridTexture != null ?
                            SetUI.ConvertTextureToSprite(LoaderConfig.Instance.gameSetup.gridTexture as Texture2D) : null;
        //this.grid = gridManager.CreateGrid(gridTexture);
    }

    
    private IEnumerator InitialQuestion()
    {
        var questionController = CardQuestions.Instance;
        if (questionController == null) yield break;
        questionController.GetAllQuestionAnswers(4);
        yield return new WaitForEndOfFrame();
        this.createPlayer();
    }

    void createPlayer()
    {

        for (int i = 0; i < this.maxPlayers; i++)
        {
            if (i < this.playerNumber)
            {
                var playerController = GameObject.Instantiate(this.playerPrefab, this.parent).GetComponent<PlayerController>();
                playerController.gameObject.name = "Player_" + i;
                playerController.UserId = i;
                this.playerControllers.Add(playerController);
                this.playerControllers[i].Init(this.characterSets[i]);

                if (i == 0 && LoaderConfig.Instance != null && LoaderConfig.Instance.apiManager.peopleIcon != null)
                {
                    var _playerName = LoaderConfig.Instance?.apiManager.loginName;
                    var icon = SetUI.ConvertTextureToSprite(LoaderConfig.Instance.apiManager.peopleIcon as Texture2D);
                    this.playerControllers[i].UserName = _playerName;
                    this.playerControllers[i].updatePlayerIcon(true, _playerName, icon);
                }
                else
                {
                    var icon = SetUI.ConvertTextureToSprite(this.characterSets[i].defaultIcon as Texture2D);
                    this.playerControllers[i].updatePlayerIcon(true, null, icon);
                }
            }
            else
            {
                int notUsedId = i + 1;
                var notUsedPlayerIcon = GameObject.FindGameObjectWithTag("P" + notUsedId + "_Icon");
                if (notUsedPlayerIcon != null)
                {
                    var notUsedIcon = notUsedPlayerIcon.GetComponent<PlayerIcon>();

                    if (notUsedIcon != null)
                    {
                        notUsedIcon.HiddenIcon();
                    }
                    //notUsedPlayerIcon.SetActive(false);
                }

                var notUsedPlayerController = GameObject.FindGameObjectWithTag("P" + notUsedId + "-controller");
                if (notUsedPlayerController != null)
                {
                    var notUsedMoveController = notUsedPlayerController.GetComponent<CharacterMoveController>();
                    notUsedMoveController.TriggerActive(false);
                }
                // notUsedPlayerController.SetActive(false);
            }
        }
    }


    public override void enterGame()
    {
        base.enterGame();
        StartCoroutine(this.InitialQuestion());
    }

    public override void endGame()
    {
        bool showSuccess = false;
        for (int i = 0; i < this.playerControllers.Count; i++)
        {
            if(i < this.playerNumber)
            {
                var playerController = this.playerControllers[i];
                if (playerController != null)
                {
                    if (playerController.Score >= 30)
                    {
                        showSuccess = true;
                    }
                    this.endGamePage.updateFinalScore(i, playerController.Score);
                }
            }
        }
        this.endGamePage.setStatus(true, showSuccess);
        base.endGame();
    }  

    public void PrepareNextQuestion()
    {
        LogController.Instance?.debug("Prepare Next Question");
    }

    public void UpdateNextQuestion()
    {
        LogController.Instance?.debug("Next Question");
        var questionController = QuestionController.Instance;

        if (questionController != null) {
            questionController.nextQuestion();

            if (questionController.currentQuestion.answersChoics != null &&
                questionController.currentQuestion.answersChoics.Length > 0)
            {
                string[] answers = questionController.currentQuestion.answersChoics;
                this.gridManager.UpdateGridWithWord(answers, null);
            }
            else
            {
                string word = questionController.currentQuestion.correctAnswer;
                this.gridManager.UpdateGridWithWord(null, word);
            }

            this.playersReset();
        }       
    }

    void playersReset()
    {
        for (int i = 0; i < this.playerNumber; i++)
        {
            if (this.playerControllers[i] != null)
            {
                this.playerControllers[i].resetRetryTime();
                this.playerControllers[i].flickedCard.Clear();
                this.playerControllers[i].playerReset();
            }
        }
    }
   
    
    private void Update()
    {
        if(!this.playing) return;

        if(Input.GetKeyDown(KeyCode.F1))
        {
            this.showCells = !this.showCells;
            this.gridManager.setAllCellsStatus(this.showCells);
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            this.playersReset();
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            this.UpdateNextQuestion();
        }

       

    } 
}


public enum CardStatus
{
    hidden,
    flicked,
    checking,
    reset
}