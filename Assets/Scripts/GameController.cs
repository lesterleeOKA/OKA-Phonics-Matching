using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
public class GameController : GameBaseController
{
    public static GameController Instance = null;
    public CharacterSet[] characterSets;
    public CardQuestions questionController;
    public GenerateCard cardManager;
    public Cell[,] grid;
    public GameObject playerPrefab;
    public Transform parent;
    public List<PlayerController> playerControllers = new List<PlayerController>();
    public bool showCells = false;
    public CanvasGroup[] audioTypeButtons, fillInBlankTypeButtons;
    public TextMeshProUGUI choiceText;
    public int numberOfQuestions = 4;
    public GameStatus gameStatus = GameStatus.readCards;
    public bool updateNextQA = false;
    public float durationOfRememberCards = 3f;
    public Clock timerOfOpenCard;

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
        Texture card_image_front = LoaderConfig.Instance.gameSetup.card_image_front != null ?
                            LoaderConfig.Instance.gameSetup.card_image_front : null;

        Texture card_image_bottom = LoaderConfig.Instance.gameSetup.card_image_bottom != null ?
                    LoaderConfig.Instance.gameSetup.card_image_bottom : null;

        this.numberOfQuestions = LoaderConfig.Instance.gameSetup.pairOfEachPage;
        this.durationOfRememberCards = LoaderConfig.Instance.gameSetup.timeToViewCards;
        this.cardManager.CreateCard(this.numberOfQuestions, card_image_front, card_image_bottom);
    }


    private IEnumerator InitialQuestion()
    {
        this.gameTimer.IsRunning = false;
        this.questionController.GetAllQuestionAnswers(this.numberOfQuestions, this.cardManager);
        yield return new WaitForEndOfFrame();
        this.createPlayer();
        this.gameStatus = GameStatus.readCards;
        if (this.timerOfOpenCard != null)
        {
            this.timerOfOpenCard.enabled = true;
        }
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

                if (i == 0 && 
                    LoaderConfig.Instance != null && 
                    LoaderConfig.Instance.apiManager.peopleIcon != null)
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
        this.gameStatus = GameStatus.endgame;
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
        if (!this.updateNextQA)
        {
            LogController.Instance?.debug("Prepare Next Question");
            this.questionController.GetNewPageQuestions(this.numberOfQuestions, this.cardManager, this.durationOfRememberCards);
            this.playersReset();
            this.gameTimer.IsRunning = false;
            this.gameStatus = GameStatus.readCards;
            if (this.timerOfOpenCard != null)
            {
                this.timerOfOpenCard.enabled = true;
            }
            this.updateNextQA = true;
        }
    }

    public void ResetAllCards()
    {
        this.gameTimer.IsRunning = true;
        this.cardManager.ResetAllCards();
        this.gameStatus = GameStatus.ready;
    }

    void playersReset()
    {
        for (int i = 0; i < this.playerNumber; i++)
        {
            if (this.playerControllers[i] != null)
            {
                this.playerControllers[i].resetRetryTime();
                this.playerControllers[i].playerReset();
            }
        }
    }
   
    
    private void Update()
    {
        if(!this.playing) return;

        if (Input.GetKeyDown(KeyCode.F2))
        {
            this.playersReset();
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            this.PrepareNextQuestion();
        }

       //LogController.Instance?.debug("selected cards:" + this.cardManager.flickedCardNumber);

        if(this.playerControllers.Count > 0 && this.gameStatus == GameStatus.ready)
        {
            bool nextQuestionPage = this.cardManager.remainQuestions == 0 ? true : false;       
            if (nextQuestionPage)
            {
                this.PrepareNextQuestion();
            }
            else
            {
                this.cardManager.CheckingCardStatus(this.gameTimer, this.playerControllers[0], this.questionController);
                this.updateNextQA = false;
            }

        }
        

    } 
}

public enum GameStatus
{
    readCards,
    ready,
    changePage,
    endgame,
}
