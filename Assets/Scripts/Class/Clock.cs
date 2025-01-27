using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Clock : MonoBehaviour
{
    private Tween timerAlertTween = null;
    [Header("摇晃设置")]
    public float shakeDuration = 1.0f;
    public float shakeStrength = 10f;
    public int shakeVibrato = 10;
    public float shakeRandomness = 90f; 

    [Header("缩放设置")]
    public float scaleDuration = 0.2f;
    public float scaleStrength = 1.2f;
    public RectTransform rectTransform;
    public Vector3 originalPosition; 
    private Vector3 originalScale;

    public TextMeshProUGUI timer = null;
    public UnityEvent finishedEvent;
    public CanvasGroup canvasGroup;
    public float duration = 15f;
    public float currentTime = 0f;
    private bool isRunning = false;
    public bool IsRunning
    {
        set { this.isRunning = value; }
        get { return this.isRunning; }
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        LogController.Instance?.debug("Open Cards OnEnable!!!!!!!!!!!!!!!!!!!!!");
        this.Init();
        SetUI.Set(this.canvasGroup, true, 0.5f, 0f, () =>
        {
            this.IsRunning = true;
        });
    }

    void OnDisable()
    {
        LogController.Instance?.debug("Open Cards Disable !!!!!!!!!!!!!!!!!!!!!");
        SetUI.Set(this.canvasGroup, false, 0.5f, 0f, ()=>
        {
            this.Init();
            this.IsRunning = false;
            this.StopAlarmAnimation();
        });
    }

    // Update is called once per frame
    void Update()
    {
        if(this.timer != null && this.isRunning && this.enabled)
        {
            if (this.currentTime > 1f)
            {

                if (this.currentTime < 5f)
                {
                    this.PlayAlarmAnimation();
                }
                this.currentTime -= Time.deltaTime;
                this.UpdateTimerText();
            }
            else
            {
                this.currentTime = 0f;
                this.UpdateTimerText();
                if (this.finishedEvent != null) this.finishedEvent.Invoke();
            }
        }
    }

    public void PlayAlarmAnimation()
    {
        if (this.timerAlertTween == null)
        {
            // 重置位置和缩放
            this.rectTransform.anchoredPosition = originalPosition;
            this.rectTransform.localScale = Vector3.one;

            // 使用 DOTween 创建摇晃动画
            this.timerAlertTween = this.rectTransform.DOShakeAnchorPos(shakeDuration, shakeStrength, shakeVibrato, shakeRandomness).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo); // 无限循环

            // 添加缩放动画
            this.rectTransform.DOScale(Vector3.one * scaleStrength, scaleDuration)
                         .SetEase(Ease.InOutQuad)
                         .SetLoops(-1, LoopType.Yoyo); // 无限循环   
        }
    }

    public void Init()
    {
        if (LoaderConfig.Instance != null && LoaderConfig.Instance.gameSetup.timeToViewCards > 0f)
            this.currentTime = LoaderConfig.Instance.gameSetup.timeToViewCards + 1;
        else
            this.currentTime = this.duration + 1;
        this.UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        if(this.timer != null) 
            this.timer.text = Mathf.FloorToInt(this.currentTime).ToString();
    }

    public void StopAlarmAnimation()
    {
        // 停止所有动画
        this.timerAlertTween.Kill();
        this.rectTransform.DOKill();

        // 重置位置和缩放
        this.rectTransform.anchoredPosition = originalPosition;
        this.rectTransform.localScale = Vector3.one;
        this.timerAlertTween = null;
    }

    private void OnDestroy()
    {
        this.StopAlarmAnimation();
    }
}
