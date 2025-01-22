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
    public Ease easeType = Ease.Linear;
    public Transform cardImage;
    public TextMeshProUGUI qaText;
    public RawImage qaImage;
    public AudioSource qaAudio;
    public float flickDuration = 0.5f;
    public float flickAngle = 180f;
    public bool isAnimated = false;
    public CanvasGroup particleEffect;
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
        SetUI.Set(this.particleEffect, status, 1f);
        CanvasGroup textCg = qaText?.GetComponent<CanvasGroup>();
        CanvasGroup imageCg = qaImage?.GetComponent<CanvasGroup>();
        CanvasGroup audioCg = qaAudio?.GetComponent<CanvasGroup>();

        this.FadeOut(textCg, delay);
        this.FadeOut(imageCg, delay);
        this.FadeOut(audioCg, delay, () =>
        {
            if (audioCg != null)
            {
                audioCg.interactable = false;
                audioCg.blocksRaycasts = false;
            }
        });

        if (status)
        {
            switch (this.type)
            {
                case CardType.Text:
                case CardType.Answer:
                    this.FadeIn(textCg, delay);
                    break;
                case CardType.Image:
                    this.FadeIn(imageCg, delay);
                    break;
                case CardType.Audio:
                    this.FadeIn(audioCg, delay, () =>
                    {
                        audioCg.interactable = true;
                        audioCg.blocksRaycasts = true;
                    });
                    break;
            }
        }
    }

    void FadeOut(CanvasGroup cg, float delay, Action onComplete = null)
    {
        cg?.DOFade(0f, 0f).SetDelay(delay / 2).OnComplete(()=> onComplete());
    }

    void FadeIn(CanvasGroup cg, float delay, Action onComplete = null)
    {
        cg?.DOFade(1f, 0f).SetDelay(delay / 2).OnComplete(() => onComplete());
    }

    public void Flick(bool status, float delay=0f, Action OnCompleted = null)
    {
        if(this.cardImage  != null && 
           this.cardStatus != CardStatus.flicked &&
           this.cardStatus != CardStatus.checking &&
           this.cardImage.gameObject.activeInHierarchy &&
           !this.isAnimated)
        {
            this.isAnimated = true;
            this.cardImage.DOKill();
            this.setElements(status, this.flickDuration + 0.5f + delay);

            if (status)
            {
                this.cardImage.DOScale(1.1f, 0.5f).OnComplete(() =>
                {
                    this.cardImage.DOBlendableRotateBy(new Vector3(0, -this.flickAngle, 0), this.flickDuration).SetEase(this.easeType).SetDelay(delay).OnPlay(() =>
                    {
                        AudioController.Instance?.PlayAudio(12);
                        if (this.type == CardType.Audio)
                        {
                            this.PlayCurrentQuestionAudio();
                        }
                    }).OnComplete(() =>
                    {
                        this.isAnimated = false;
                    });
                });
            }
            else
            {
                this.cardImage.DOBlendableRotateBy(new Vector3(0, -this.flickAngle, 0), this.flickDuration).SetEase(this.easeType).SetDelay(delay).OnPlay(()=>
                {
                    AudioController.Instance?.PlayAudio(12);
                }).OnComplete(() =>
                {
                    this.cardImage.DOScale(1f, 0.5f).OnComplete(() =>
                    {
                        this.isAnimated = false;
                    });
                });
            }
            
            OnCompleted?.Invoke();
        }
    }

    public void ResetFlick(float delay=0f)
    {
        this.cardStatus = CardStatus.reset;
        this.Flick(false, delay);
    }

    public void dissolveCard()
    {
        this.gameObject.SetActive(false);
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

    public void PlayCurrentQuestionAudio()
    {
        if (this.qaAudio != null && this.qaAudio.clip != null)
        {
            this.qaAudio.Play();
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