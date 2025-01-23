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
    public EventTrigger eventTrigger;
    public UnityAction<Card> OnCardClick;
    public EventTrigger.Entry entry;

    private CanvasGroup textCg = null;
    private CanvasGroup imageCg = null;
    private CanvasGroup audioCg = null;


    public string CardId
    {
        set { this.qid = value; }
        get { return this.qid; } 
    }

    public void setCardImage(Sprite cardSprite = null)
    {
        // Get or add the EventTrigger component
        this.textCg = this.qaText?.GetComponent<CanvasGroup>();
        this.imageCg = this.qaImage?.GetComponent<CanvasGroup>();
        this.audioCg = this.qaAudio?.GetComponent<CanvasGroup>();

        this.setElements(false);
        if (this.cardImage != null && cardSprite != null)
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
        if(this.textCg == null || this.imageCg == null || this.audioCg == null) return;
        this.cardStatus = status ? CardStatus.flicked : CardStatus.hidden;
        SetUI.Set(this.particleEffect, status, 1f);
        SetUI.Set(this.textCg, false, 0f, delay/2);
        SetUI.Set(this.imageCg, false, 0f, delay/2);
        SetUI.Set(this.audioCg, false, 0f, delay/2, () =>
        {
            if (this.audioCg != null)
            {
                this.audioCg.interactable = false;
                this.audioCg.blocksRaycasts = false;
            }
        });

        if (status)
        {
            switch (this.type)
            {
                case CardType.Text:
                case CardType.Answer:
                    SetUI.Set(this.textCg, true, 0f, delay/2);
                    break;
                case CardType.Image:
                    SetUI.Set(this.imageCg, true, 0f, delay/2);
                    break;
                case CardType.Audio:
                    SetUI.Set(this.audioCg, true, 0f, delay/2, () =>
                    {
                        this.audioCg.interactable = true;
                        this.audioCg.blocksRaycasts = true;
                    });
                    break;
            }
        }
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
                this.cardImage.DOScale(1.08f, 0.5f).OnComplete(() =>
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
        if (this.eventTrigger == null)
        {
            LogController.Instance.debugError("EventTrigger is not initialized.");
            return;
        }

        // Ensure action is not null
        if (action != null)
        {
            this.entry.callback.AddListener(action);
            this.eventTrigger.triggers.Add(this.entry);
        }
        else
        {
            LogController.Instance.debugError("Action cannot be null.");
        }
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