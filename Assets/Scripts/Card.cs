using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;


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
    public CardType type = CardType.Text;
    public Transform cardImage;
    public TextMeshProUGUI qaText;
    public RawImage qaImage;
    public AudioSource qaAudio;
    public bool selected = false;
    public float flickDuration = 0.5f;
    public float flickAngle = 180f;


    private void Start()
    {
        this.setElements(false);
    }

    public void setContent(CardType _type,
        string _content = "", 
        Texture _picture = null, 
        AudioClip _audio = null,
        string _correctAnswer = "")
    {
        this.type = _type;

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
        this.selected = status;
        switch (this.type)
        {
            case CardType.Text:
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

    public void Flick()
    {
        if(this.cardImage  != null)
        {
            this.cardImage.DOKill();
            this.setElements(this.selected, this.flickDuration / 2);
            this.cardImage.DOBlendableRotateBy(new Vector3(0, -this.flickAngle, 0), this.flickDuration / 2);
        }
    }
}