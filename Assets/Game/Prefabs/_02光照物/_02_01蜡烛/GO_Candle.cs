using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using MoreMountains.Tools;


public class GO_Candle : MonoBehaviour, ISceneAddedListener
{
    [SerializeField]
    [MMLabel("ÊÇ·ñµÚÒ»´ÎÌí¼Óµ½³¡¾°")]

    private bool isFristAddToScenes = true;
    [SerializeField]
    [MMLabel("Ìí¼ÓÊ±ÊÇ·ñËæ»úËõ·Å")]
    private bool isRandomScaleOnAddScene = false;
    
    [ShowIf("isRandomScaleOnAddScene",false)]
    [SerializeField]
    private float minScale = 2;
    
    [ShowIf("isRandomScaleOnAddScene", false)]
    [SerializeField]
    private float maxScale = 2.5f;
    private void RandomScale()
    {
        //Ö»ÔÚµÚÒ»´ÎÌí¼ÓÊ±²ÅËæ»ú
        if (!isRandomScaleOnAddScene) { return; }

        if (isRandomScaleOnAddScene)
        {
            float scale = Random.Range(minScale, maxScale);
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }










    #region

    [SerializeField]
    [FoldoutGroup("±à¼­Æ÷")]
    [MMLabel("Ìí¼ÓÊ±ÊÇ·ñÉèÖÃËæ»ú×´Ì¬")]
    private bool isSetRandomState = true;
    public void OnAddedToScene()
    {


        if (!isFristAddToScenes) { return; }
        isFristAddToScenes = false;

        À¯Öò»ğ¹â = transform.GetChild(0).gameObject;
        Ï¨ÃğÑÌÎí = transform.GetChild(1).gameObject;
        À¯Öò_µãÈ¼Ê± = transform.GetChild(2).gameObject;
        À¯Öò_Ï¨ÃğÊ± = transform.GetChild(3).gameObject;
        sr_candle_fire = À¯Öò_µãÈ¼Ê±.GetComponent<SpriteRenderer>();
        sr_candle_noFire = À¯Öò_Ï¨ÃğÊ±.GetComponent<SpriteRenderer>();
        sr_smoke = Ï¨ÃğÑÌÎí.GetComponent<SpriteRenderer>();
        sr_fire = À¯Öò»ğ¹â.GetComponent<SpriteRenderer>();


        

        if (isSetRandomState)
        {
            int i = Random.Range(0, 2);
            if (i > 0)
            {
                SetState(GO_Candle.CandleState.Ï¨Ãğ);
                Ï¨ÃğÑÌÎí.gameObject.SetActive(false);

            }
            else
            {
                SetState(GO_Candle.CandleState.È¼ÉÕ);
            }
        }

        RandomScale();

    }

    #endregion





    public enum CandleState
    {
        È¼ÉÕ,
        ÕıÔÚµãÈ¼,
        ÕıÔÚÏ¨Ãğ,
        Ï¨Ãğ
    }

    [Header("¿ìËÙÒıÈ¼")]
    public float quickIgnitionTime = 0.2f;
    public AnimationCurve quickIgnitionCurve;

    [Header("×ÔÈ»ÒıÈ¼")]
    public float normalIgnitionTime = 1;
    public AnimationCurve normaIgnitionCurve;

    [Header("¿ìËÙÏ¨Ãğ")]
    public float quickExtinguishTime = 1;
    public AnimationCurve quickExtinguishCurve;


    [Header("×ÔÈ»Ï¨Ãğ")]
    public float normaExtinguishTime = 1;
    public AnimationCurve normaExtinguishCurve;

    [Header("È¼ÉÕ×´Ì¬")]
    public AnimationCurve burningCurve = AnimationCurve.Linear(0, 0.8f, 1, 1.2f);
    public float burningSpeed = 2f;

    [Header("Ï¨ÃğÑÌÎí")]
    public float smokeTime = 7;
    public AnimationCurve SmokeTransparencyCurve;
    public AnimationCurve SmokeScalingCurve;

    // ×é¼şÒıÓÃ
    [SerializeField] private GameObject À¯Öò»ğ¹â;
    [SerializeField] private SpriteRenderer sr_fire;
    [SerializeField] private GameObject Ï¨ÃğÑÌÎí;
    [SerializeField] private SpriteRenderer sr_smoke;
    [SerializeField] private GameObject À¯Öò_µãÈ¼Ê±;
    [SerializeField] private SpriteRenderer sr_candle_fire;
    [SerializeField] private GameObject À¯Öò_Ï¨ÃğÊ±;
    [SerializeField] private SpriteRenderer sr_candle_noFire;
    [SerializeField] private bool autoReference = true;

    [SerializeField]
    [MMReadOnly]
    private CandleState state = CandleState.È¼ÉÕ;


    private Tween burningTween;
    private Tween currentTween;





    private void Awake()
    {
        if (autoReference)
        {
            À¯Öò»ğ¹â = transform.GetChild(0).gameObject;
            Ï¨ÃğÑÌÎí = transform.GetChild(1).gameObject;
            À¯Öò_µãÈ¼Ê± = transform.GetChild(2).gameObject;
            À¯Öò_Ï¨ÃğÊ± = transform.GetChild(3).gameObject;
            sr_candle_fire = À¯Öò_µãÈ¼Ê±.GetComponent<SpriteRenderer>();
            sr_candle_noFire = À¯Öò_Ï¨ÃğÊ±.GetComponent<SpriteRenderer>();
            sr_smoke = Ï¨ÃğÑÌÎí.GetComponent<SpriteRenderer>();
            sr_fire = À¯Öò»ğ¹â.GetComponent<SpriteRenderer>();
        }

        InitializeState();
    }

    private void InitializeState()
    {

        SetState(state);
        if (state == CandleState.È¼ÉÕ)
        {
            SetBurningState(1f);
            Burning();
        }
        else
        {
            SetExtinguishedState(1f);
        }
    }
    [Button("¿ìËÙµãÈ¼")]
    public void QuickIgnition()
    {
        if (state != CandleState.Ï¨Ãğ) return;

        SetState(CandleState.ÕıÔÚµãÈ¼);
        KillCurrentTween();

        currentTween = CreateIgnitionTween(quickIgnitionTime, quickIgnitionCurve)
            .OnComplete(() =>
            {
                SetState(CandleState.È¼ÉÕ);
                Burning();
            });
    }
    [Button("×ÔÈ»µãÈ¼")]
    public void NormalIgnition()
    {
        if (state != CandleState.Ï¨Ãğ) return;

        SetState(CandleState.ÕıÔÚµãÈ¼);
        KillCurrentTween();

        currentTween = CreateIgnitionTween(normalIgnitionTime, normaIgnitionCurve)
            .OnComplete(() =>
            {
                SetState(CandleState.È¼ÉÕ);
              
                Burning();
            });
    }

    private Tween CreateIgnitionTween(float duration, AnimationCurve curve)
    {
        sr_fire.color = new Color(1, 1, 1, 0);
        sr_fire.transform.localScale = Vector3.zero;

        return DOTween.To(() => 0f, t =>
        {
            float value = curve.Evaluate(t);
            SetBurningState(value);
        }, 1f, duration);
    }


    /// <summary>
    /// ¿ìËÙÏ¨Ãğ
    /// </summary>
    [Button("¿ìËÙÏ¨Ãğ")]
    public void QuicklyExtinguish()
    {
   
        if (state != CandleState.È¼ÉÕ) return;

        SetState(CandleState.ÕıÔÚÏ¨Ãğ);
        KillCurrentTween();

        currentTween = DOTween.To(() => 1f, t =>
        {
            float value = quickExtinguishCurve.Evaluate(1 - t);
            SetBurningState(value);
        }, 0f, quickExtinguishTime)
        .OnComplete(() =>
        {
            PlayExtinguishSmoke();
            //state = CandleState.Ï¨Ãğ;
            SetState(CandleState.Ï¨Ãğ);
        });

    }


    [Button("×ÔÈ»Ï¨Ãğ")]
    public void NormalExtinguishing()
    {
        if (state != CandleState.È¼ÉÕ) return;

        SetState(CandleState.ÕıÔÚÏ¨Ãğ);
        KillCurrentTween();

        currentTween = DOTween.To(() => 1f, t =>
        {
            float value = normaExtinguishCurve.Evaluate(1 - t);
            SetBurningState(value);
        }, 0f, normaExtinguishTime)
        .OnComplete(() =>
        {
            PlayExtinguishSmoke();
            //state = CandleState.Ï¨Ãğ;
            SetState(CandleState.Ï¨Ãğ);
        });
    }

    private void PlayExtinguishSmoke()
    {
        Ï¨ÃğÑÌÎí.SetActive(true);
        sr_smoke.color = new Color(1, 1, 1, 0);
        sr_smoke.transform.localScale = Vector3.zero;

        Sequence smokeSeq = DOTween.Sequence();
        smokeSeq.Append(DOTween.To(() => 0f, t =>
        {
            float progress = t / smokeTime;
            sr_smoke.color = new Color(1, 1, 1, SmokeTransparencyCurve.Evaluate(progress));
            sr_smoke.transform.localScale = Vector3.one * SmokeScalingCurve.Evaluate(progress);
        }, smokeTime, smokeTime).SetEase(Ease.Linear));
        smokeSeq.OnComplete(() => Ï¨ÃğÑÌÎí.SetActive(false));
    }

    /// <summary>
    /// È¼ÉÕ
    /// </summary>
    private void Burning()
    {
        KillBurningTween();

        float duration = 1f / burningSpeed;
        burningTween = DOTween.To(() => 0f, t =>
        {
            t = Mathf.Repeat(t, 1f);
            float value = burningCurve.Evaluate(t);
            sr_fire.transform.localScale = Vector3.one * value;
            sr_fire.color = new Color(1, 1, 1, value);
        }, 1f, duration)
        .SetLoops(-1, LoopType.Restart)
        .SetEase(Ease.Linear);
    }

    private void SetBurningState(float value)
    {
        sr_fire.color = new Color(1, 1, 1, value);
        sr_fire.transform.localScale = Vector3.one * value;
        sr_candle_fire.color = new Color(1, 1, 1, value);
        sr_candle_noFire.color = new Color(1, 1, 1, 1 - value);
    }

    private void SetExtinguishedState(float value)
    {
        sr_candle_fire.color = new Color(1, 1, 1, 0);
        sr_candle_noFire.color = new Color(1, 1, 1, 1);
        sr_fire.color = new Color(1, 1, 1, 0);
        sr_fire.transform.localScale = Vector3.zero;
    }

    private void KillCurrentTween()
    {
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
        }
    }

    private void KillBurningTween()
    {
        if (burningTween != null && burningTween.IsActive())
        {
            burningTween.Kill();
        }
    }


    [Button("Editor_ÇĞ»»×´Ì¬")]
    private void Editor_ChangeState()
    {
        if (state==CandleState.È¼ÉÕ)
        {
            SetState(CandleState.Ï¨Ãğ);
        }
        else
        {
            SetState(CandleState.È¼ÉÕ);
        }

    }
   






    private void SetState(CandleState NewState)
    {



        if (state == NewState)
        {
            return;
        }


        switch (NewState)
        {
            case CandleState.È¼ÉÕ:
                À¯Öò»ğ¹â.SetActive(true);
                À¯Öò_µãÈ¼Ê±.SetActive(true);
                À¯Öò_Ï¨ÃğÊ±.SetActive(false);
                Ï¨ÃğÑÌÎí.SetActive(false);

                break;
            case CandleState.Ï¨Ãğ:
                À¯Öò»ğ¹â.SetActive(false);
                À¯Öò_µãÈ¼Ê±.SetActive(false);
                À¯Öò_Ï¨ÃğÊ±.SetActive(true);
                Ï¨ÃğÑÌÎí.SetActive(true);

                break;
            default:
                À¯Öò»ğ¹â.SetActive(true);
                À¯Öò_µãÈ¼Ê±.SetActive(true);
                À¯Öò_Ï¨ÃğÊ±.SetActive(true);
                Ï¨ÃğÑÌÎí.SetActive(false);
                break;
        }
        this.state = NewState;
    }

    private void OnDestroy()
    {
        KillCurrentTween();
        KillBurningTween();
    }


}