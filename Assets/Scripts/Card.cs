using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;


public enum CardType
{
    None,
    Text,
    Image,
    Audio,
    Answer
}
public class Card : MonoBehaviour
{
    public string qid = "";
    public QuestionList question;
    public CardType type = CardType.Text;
    public CardStatus cardStatus = CardStatus.hidden;
    public Transform cardImage;
    public TextMeshProUGUI qaText;
    public RawImage qaImage;
    public AudioSource qaAudio;
    public float flickDuration = 0.5f;
    public float flickAngle = 180f;
    public bool isAnimated = false;

    private EventTrigger eventTrigger;
    public UnityAction<Card> OnCardClick;

    public string CardId
    {
        set { this.qid = value; }
        get { return this.qid; } 
    }

    void Awake()
    {
        // Get or add the EventTrigger component
        this.eventTrigger = this.GetComponent<EventTrigger>();
        if (this.eventTrigger == null)
        {
            this.eventTrigger = this.gameObject.AddComponent<EventTrigger>();
        }
    }

    private void Start()
    {
        this.setElements(false);
    }

    public void setCardImage(Sprite cardSprite = null)
    {
        if(this.cardImage != null && cardSprite != null)
        {
            var cardImg = this.cardImage.GetComponent<Image>();

            if (cardImg != null)
            {
                cardImg.sprite = cardSprite;
            }
        }
        this.AddPointerClickEvent(OnCardClicked);
    }

    public void setContent(CardType _type,
        string _content = "", 
        Texture _picture = null, 
        AudioClip _audio = null,
        string _correctAnswer = "",
        string _cardId = "",
        QuestionList _question = null)
    {
        if(!this.gameObject.activeInHierarchy) this.gameObject.SetActive(true);

        this.cardStatus = CardStatus.hidden;
        this.type = _type;
        this.CardId = _cardId;
        this.question = _question;
        this.setElements(true);

        switch (this.type)
        {
            case CardType.Text:
            case CardType.Answer:
                if (!string.IsNullOrEmpty(_content) && this.qaText != null)
                {
                    this.qaText.text = _content;
                }
                break;
            case CardType.Image:
                if (_picture != null && this.qaImage != null)
                {
                    var aspecRatioFitter = this.qaImage.GetComponent<AspectRatioFitter>();
                    var width = this.qaImage.GetComponent<RectTransform>().sizeDelta.x;
                    var height = this.qaImage.GetComponent<RectTransform>().sizeDelta.y;
                    if (_picture.width >= _picture.height)
                    {
                        aspecRatioFitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
                        this.qaImage.GetComponent<RectTransform>().sizeDelta = new Vector2(335f, height);
                        aspecRatioFitter.aspectRatio = (float)_picture.width / (float)_picture.height;
                    }
                    else
                    {
                        aspecRatioFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
                        this.qaImage.GetComponent<RectTransform>().sizeDelta = new Vector2(width, 450f);
                        aspecRatioFitter.aspectRatio = (float)_picture.width / (float)_picture.height;
                    }
                    this.qaImage.texture = _picture;
                }
                break;
            case CardType.Audio:
                if (_audio != null && this.qaAudio != null)
                {
                    this.qaAudio.clip = _audio;
                }
                break;
        }
    }

    void setElements(bool status, float delay = 0f)
    {
        this.cardStatus = status ? CardStatus.flicked : CardStatus.hidden;
        switch (this.type)
        {
            case CardType.Text:
            case CardType.Answer:
                if (this.qaText != null && this.qaText.GetComponent<CanvasGroup>() != null)
                {
                    var text = this.qaText.GetComponent<CanvasGroup>();
                    if(text != null)
                    {
                        text.DOFade(status ? 1f : 0f, 0f).SetDelay(delay / 2);
                    }
                }
                break;
            case CardType.Image:
                if (this.qaImage != null)
                {
                    var image = this.qaImage.GetComponent<CanvasGroup>();
                    if(image != null)
                    {
                        image.DOFade(status ? 1f : 0f, 0f).SetDelay(delay / 2);
                    }
                }
                break;
            case CardType.Audio:
                if (this.qaAudio != null)
                {
                    var audioBtn = this.qaAudio.GetComponent<CanvasGroup>();
                    if (audioBtn != null)
                    {
                        audioBtn.DOFade(status ? 1f : 0f, 0f).SetDelay(delay / 2).OnComplete(()=>
                        {
                            audioBtn.interactable = status;
                            audioBtn.blocksRaycasts = status;
                        });
                    }
                }
                break;
        }
    }

    public void Flick(bool status, Action OnCompleted = null)
    {
        if(this.cardImage  != null && 
           this.cardStatus != CardStatus.flicked &&
           this.cardStatus != CardStatus.checking &&
           this.cardImage.gameObject.activeInHierarchy &&
           !this.isAnimated)
        {
            this.isAnimated = true;
            this.cardImage.DOKill();
            this.setElements(status, this.flickDuration / 2);
            this.cardImage.DOBlendableRotateBy(new Vector3(0, -this.flickAngle, 0), this.flickDuration).OnComplete(()=>
            {
                this.isAnimated = false;
            });
            OnCompleted?.Invoke();
        }
    }

    public void ResetFlick()
    {
        this.cardStatus = CardStatus.reset;
        this.Flick(false);
    }

    public void AddPointerClickEvent(UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick
        };
        entry.callback.AddListener(action);
        this.eventTrigger.triggers.Add(entry);
    }

    private void OnCardClicked(BaseEventData eventData)
    {
        this.OnCardClick?.Invoke(this);
    }
}

public enum CardStatus
{
    hidden,
    flicked,
    checking, // add checking effect
    reset
}