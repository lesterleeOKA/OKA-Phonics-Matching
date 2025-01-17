using UnityEngine;
using DG.Tweening;
using TMPro;


public enum CardType
{
    None,
    Text,
    Image,
    Audio
}
public class Card : MonoBehaviour
{
    public CardType type = CardType.Text;
    public Transform cardImage;
    public TextMeshProUGUI text;
    public bool selected = false;
    public float flickDuration = 0.5f; // 翻轉動畫的持續時間
    public float flickAngle = 180f; // 翻轉的角度

    private void Start()
    {
        this.setElements(false);
    }

    void setElements(bool status, float delay = 0f)
    {
        this.selected = status;
        switch (this.type)
        {
            case CardType.Text:
                if (this.text != null)
                {
                    this.text.GetComponent<CanvasGroup>().DOFade(status? 1f : 0f, 0f).SetDelay(delay / 2);
                }
                break;
            case CardType.Image:
                break;
            case CardType.Audio:
                break;
        }
    }

    // 方法來處理翻轉動畫
    public void Flick()
    {
        if(this.cardImage  != null)
        {
            // 確保當前的旋轉動畫被終止
            this.cardImage.DOKill();
            this.setElements(true, flickDuration / 2);
            // 創建翻轉動畫
            this.cardImage.DOBlendableRotateBy(new Vector3(0, -flickAngle, 0), flickDuration / 2);
        }
    }
}