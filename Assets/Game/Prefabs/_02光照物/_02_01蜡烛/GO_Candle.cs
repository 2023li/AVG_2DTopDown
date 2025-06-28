using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using MoreMountains.Tools;


public class GO_Candle : MonoBehaviour, ISceneAddedListener
{
    [SerializeField]
    [MMLabel("是否第一次添加到常见")]

    private bool isFristAddToScenes = true;
    [SerializeField]
    [MMLabel("添加时是否随机缩放")]
    private bool isRandomScaleOnAddScene = false;
    
    [ShowIf("isRandomScaleOnAddScene",false)]
    [SerializeField]
    private float minScale = 2;
    
    [ShowIf("isRandomScaleOnAddScene", false)]
    [SerializeField]
    private float maxScale = 2.5f;
    private void RandomScale()
    {
        //只在第一次添加时才随机
        if (!isRandomScaleOnAddScene) { return; }

        if (isRandomScaleOnAddScene)
        {
            float scale = Random.Range(minScale, maxScale);
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }










    #region

    [SerializeField]
    [FoldoutGroup("编辑器")]
    [MMLabel("添加时是否设置随机状态")]
    private bool isSetRandomState = true;
    public void OnAddedToScene()
    {


        if (!isFristAddToScenes) { return; }
        isFristAddToScenes = false;

        蜡烛火光 = transform.GetChild(0).gameObject;
        熄灭烟雾 = transform.GetChild(1).gameObject;
        蜡烛_点燃时 = transform.GetChild(2).gameObject;
        蜡烛_熄灭时 = transform.GetChild(3).gameObject;
        sr_candle_fire = 蜡烛_点燃时.GetComponent<SpriteRenderer>();
        sr_candle_noFire = 蜡烛_熄灭时.GetComponent<SpriteRenderer>();
        sr_smoke = 熄灭烟雾.GetComponent<SpriteRenderer>();
        sr_fire = 蜡烛火光.GetComponent<SpriteRenderer>();


        

        if (isSetRandomState)
        {
            int i = Random.Range(0, 2);
            if (i > 0)
            {
                SetState(GO_Candle.CandleState.熄灭);
                熄灭烟雾.gameObject.SetActive(false);

            }
            else
            {
                SetState(GO_Candle.CandleState.燃烧);
            }
        }

        RandomScale();

    }

    #endregion





    public enum CandleState
    {
        燃烧,
        正在点燃,
        正在熄灭,
        熄灭
    }

    [Header("快速引燃")]
    public float quickIgnitionTime = 0.2f;
    public AnimationCurve quickIgnitionCurve;

    [Header("自然引燃")]
    public float normalIgnitionTime = 1;
    public AnimationCurve normaIgnitionCurve;

    [Header("快速熄灭")]
    public float quickExtinguishTime = 1;
    public AnimationCurve quickExtinguishCurve;


    [Header("自然熄灭")]
    public float normaExtinguishTime = 1;
    public AnimationCurve normaExtinguishCurve;

    [Header("燃烧状态")]
    public AnimationCurve burningCurve = AnimationCurve.Linear(0, 0.8f, 1, 1.2f);
    public float burningSpeed = 2f;

    [Header("熄灭烟雾")]
    public float smokeTime = 7;
    public AnimationCurve SmokeTransparencyCurve;
    public AnimationCurve SmokeScalingCurve;

    // 组件引用
    private GameObject 蜡烛火光;
    private SpriteRenderer sr_fire;
    private GameObject 熄灭烟雾;
    private SpriteRenderer sr_smoke;
    private GameObject 蜡烛_点燃时;
    private SpriteRenderer sr_candle_fire;
    private GameObject 蜡烛_熄灭时;
    private SpriteRenderer sr_candle_noFire;

    [SerializeField]
    [MMReadOnly]
    private CandleState state = CandleState.燃烧;


    private Tween burningTween;
    private Tween currentTween;





    private void Awake()
    {
        蜡烛火光 = transform.GetChild(0).gameObject;
        熄灭烟雾 = transform.GetChild(1).gameObject;
        蜡烛_点燃时 = transform.GetChild(2).gameObject;
        蜡烛_熄灭时 = transform.GetChild(3).gameObject;
        sr_candle_fire = 蜡烛_点燃时.GetComponent<SpriteRenderer>();
        sr_candle_noFire = 蜡烛_熄灭时.GetComponent<SpriteRenderer>();
        sr_smoke = 熄灭烟雾.GetComponent<SpriteRenderer>();
        sr_fire = 蜡烛火光.GetComponent<SpriteRenderer>();

        InitializeState();
    }

    private void InitializeState()
    {

        SetState(state);
        if (state == CandleState.燃烧)
        {
            SetBurningState(1f);
            Burning();
        }
        else
        {
            SetExtinguishedState(1f);
        }
    }
    [Button("快速点燃")]
    public void QuickIgnition()
    {
        if (state != CandleState.熄灭) return;

        SetState(CandleState.正在点燃);
        KillCurrentTween();

        currentTween = CreateIgnitionTween(quickIgnitionTime, quickIgnitionCurve)
            .OnComplete(() =>
            {
                SetState(CandleState.燃烧);
                Burning();
            });
    }
    [Button("自然点燃")]
    public void NormalIgnition()
    {
        if (state != CandleState.熄灭) return;

        SetState(CandleState.正在点燃);
        KillCurrentTween();

        currentTween = CreateIgnitionTween(normalIgnitionTime, normaIgnitionCurve)
            .OnComplete(() =>
            {
                SetState(CandleState.燃烧);
              
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
    /// 快速熄灭
    /// </summary>
    [Button("快速熄灭")]
    public void QuicklyExtinguish()
    {
   
        if (state != CandleState.燃烧) return;

        SetState(CandleState.正在熄灭);
        KillCurrentTween();

        currentTween = DOTween.To(() => 1f, t =>
        {
            float value = quickExtinguishCurve.Evaluate(1 - t);
            SetBurningState(value);
        }, 0f, quickExtinguishTime)
        .OnComplete(() =>
        {
            PlayExtinguishSmoke();
            //state = CandleState.熄灭;
            SetState(CandleState.熄灭);
        });

    }


    [Button("自然熄灭")]
    public void NormalExtinguishing()
    {
        if (state != CandleState.燃烧) return;

        SetState(CandleState.正在熄灭);
        KillCurrentTween();

        currentTween = DOTween.To(() => 1f, t =>
        {
            float value = normaExtinguishCurve.Evaluate(1 - t);
            SetBurningState(value);
        }, 0f, normaExtinguishTime)
        .OnComplete(() =>
        {
            PlayExtinguishSmoke();
            //state = CandleState.熄灭;
            SetState(CandleState.熄灭);
        });
    }

    private void PlayExtinguishSmoke()
    {
        熄灭烟雾.SetActive(true);
        sr_smoke.color = new Color(1, 1, 1, 0);
        sr_smoke.transform.localScale = Vector3.zero;

        Sequence smokeSeq = DOTween.Sequence();
        smokeSeq.Append(DOTween.To(() => 0f, t =>
        {
            float progress = t / smokeTime;
            sr_smoke.color = new Color(1, 1, 1, SmokeTransparencyCurve.Evaluate(progress));
            sr_smoke.transform.localScale = Vector3.one * SmokeScalingCurve.Evaluate(progress);
        }, smokeTime, smokeTime).SetEase(Ease.Linear));
        smokeSeq.OnComplete(() => 熄灭烟雾.SetActive(false));
    }

    /// <summary>
    /// 燃烧
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


    [Button("Editor_切换状态")]
    private void Editor_ChangeState()
    {
        if (state==CandleState.燃烧)
        {
            SetState(CandleState.熄灭);
        }
        else
        {
            SetState(CandleState.燃烧);
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
            case CandleState.燃烧:
                蜡烛火光.SetActive(true);
                蜡烛_点燃时.SetActive(true);
                蜡烛_熄灭时.SetActive(false);
                熄灭烟雾.SetActive(false);

                break;
            case CandleState.熄灭:
                蜡烛火光.SetActive(false);
                蜡烛_点燃时.SetActive(false);
                蜡烛_熄灭时.SetActive(true);
                熄灭烟雾.SetActive(true);

                break;
            default:
                蜡烛火光.SetActive(true);
                蜡烛_点燃时.SetActive(true);
                蜡烛_熄灭时.SetActive(true);
                熄灭烟雾.SetActive(false);
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