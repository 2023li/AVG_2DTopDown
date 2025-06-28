using System.Collections;
using UnityEngine;
using DG.Tweening;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using System;


public class GO_GeneralLight : MonoBehaviour
{
    #region
#if UNITY_EDITOR
    [SerializeField]
    [FoldoutGroup("编辑器")]
    [MMLabel("添加时是否设置随机状态")]
    private bool isSetRandomState = true;
    public void OnAddedToScene()
    {
      
        if (isSetRandomState)
        {
            int i = UnityEngine.Random.Range(0, 2);
            if (i > 0)
            {
                SetState(LightState.熄灭);

            }
            else
            {
                SetState(LightState.点亮);
            }
        }
    }
    [FoldoutGroup("编辑器")]
    [Button("自动引用")]
    protected virtual void AutoQuote()
    {
        go_光球对象 = transform.GetChild(0).gameObject;
        //go_点燃时 = transform.GetChild(1).gameObject;
        //go_熄灭时 = transform.GetChild(2).gameObject;
        sr_光球 = go_光球对象.GetComponent<SpriteRenderer>();
        //sr_点燃时model = go_点燃时.GetComponent<SpriteRenderer>();
        //sr_熄灭时Modle = go_熄灭时.GetComponent <SpriteRenderer>();
    }

#endif
    #endregion





    public enum LightState
    {
        点亮,
        开始点亮,
        开始熄灭,
        熄灭
    }

    [Header("快速点亮")]
    public float quickIgnitionTime = 0.2f;
    public AnimationCurve quickIgnitionCurve;

    [Header("自然点亮")]
    public float normalIgnitionTime = 1;
    public AnimationCurve normaIgnitionCurve;

    [Header("快速熄灭")]
    public float quickExtinguishTime = 1;
    public AnimationCurve quickExtinguishCurve;


    [Header("自然熄灭")]
    public float normaExtinguishTime = 1;
    public AnimationCurve normaExtinguishCurve;

    [Header("持续发光时")]
    public AnimationCurve burningCurve = AnimationCurve.Linear(0, 0.8f, 1, 1.2f);
    public float burningSpeed = 2f;

    // 组件引用
    [SerializeField] private GameObject go_光球对象;
    [SerializeField] private SpriteRenderer sr_光球;
    //private GameObject go_点燃时;
    //private SpriteRenderer sr_点燃时model;
    //private GameObject go_熄灭时;
    //private SpriteRenderer sr_熄灭时Modle;

    [SerializeField]
    [MMReadOnly]
    private LightState state = LightState.点亮;


    private Tween burningTween;
    private Tween currentTween;

    public event Action OnFullIgnition;
    public event Action OnFullExtinguish;

    





    private void Awake()
    {


        InitializeState();
    }

    private void InitializeState()
    {

        SetState(state);
        if (state == LightState.点亮)
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
        if (state != LightState.熄灭) return;

        SetState(LightState.开始点亮);
        KillCurrentTween();

        currentTween = CreateIgnitionTween(quickIgnitionTime, quickIgnitionCurve)
            .OnComplete(() =>
            {
                FullIgnition();
            });
    }
    [Button("自然点燃")]
    public void NormalIgnition()
    {
        if (state != LightState.熄灭) return;

        SetState(LightState.开始点亮);
        KillCurrentTween();

        currentTween = CreateIgnitionTween(normalIgnitionTime, normaIgnitionCurve)
            .OnComplete(() =>
            {
                FullIgnition();
            });
    }

   
    protected virtual void FullIgnition()
    {
        SetState(LightState.点亮);
        Burning();
        OnFullIgnition?.Invoke();
    }

    private Tween CreateIgnitionTween(float duration, AnimationCurve curve)
    {
        sr_光球.color = new Color(1, 1, 1, 0);
        sr_光球.transform.localScale = Vector3.zero;

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

        if (state != LightState.点亮) return;

        SetState(LightState.开始熄灭);
        KillCurrentTween();

        currentTween = DOTween.To(() => 1f, t =>
        {
            float value = quickExtinguishCurve.Evaluate(1 - t);
            SetBurningState(value);
        }, 0f, quickExtinguishTime)
        .OnComplete(() =>
        {

            FullExtinguish();
        });

    }


    [Button("自然熄灭")]
    public void NormalExtinguishing()
    {
        if (state != LightState.点亮) return;

        SetState(LightState.开始熄灭);
        KillCurrentTween();

        currentTween = DOTween.To(() => 1f, t =>
        {
            float value = normaExtinguishCurve.Evaluate(1 - t);
            SetBurningState(value);
        }, 0f, normaExtinguishTime)
        .OnComplete(() =>
        {
            FullExtinguish();
        });
    }

 
    protected virtual void FullExtinguish()
    {
        OnFullExtinguish?.Invoke();
        SetState(LightState.熄灭);
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
            sr_光球.transform.localScale = Vector3.one * value;
            sr_光球.color = new Color(1, 1, 1, value);
        }, 1f, duration)
        .SetLoops(-1, LoopType.Restart)
        .SetEase(Ease.Linear);
    }

    private void SetBurningState(float value)
    {
        sr_光球.color = new Color(1, 1, 1, value);
        sr_光球.transform.localScale = Vector3.one * value;
        //sr_点燃时model.color = new Color(1, 1, 1, value);
        //sr_熄灭时Modle.color = new Color(1, 1, 1, 1 - value);
    }

    private void SetExtinguishedState(float value)
    {
        //sr_点燃时model.color = new Color(1, 1, 1, 0);
        //sr_熄灭时Modle.color = new Color(1, 1, 1, 1);
        sr_光球.color = new Color(1, 1, 1, 0);
        sr_光球.transform.localScale = Vector3.zero;
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

    protected virtual void SetState(LightState NewState)
    {
        if (state == NewState)
        {
            return;
        }
        switch (NewState)
        {
            case LightState.点亮:
                SetState_点亮();
                break;
            case LightState.开始点亮:
                SetState_开始点亮();
                break;
            case LightState.熄灭:
                SetState_熄灭();
                break;
            case LightState.开始熄灭:
                SetState_开始熄灭();
                break;
        }
        this.state = NewState;
    }

    protected virtual void SetState_点亮() 
    {
        go_光球对象.SetActive(true);
        //go_点燃时.SetActive(true);
        //go_熄灭时.SetActive(false);
    }
    protected virtual void SetState_开始点亮() 
    {
        go_光球对象.SetActive(true);
        //go_点燃时.SetActive(true);
        //go_熄灭时.SetActive(true);
    }
    protected virtual void SetState_熄灭() 
    {
        go_光球对象.SetActive(false);
        //go_点燃时.SetActive(false);
        //go_熄灭时.SetActive(true);
    }
    protected virtual void SetState_开始熄灭() 
    {
        go_光球对象.SetActive(true);
        //go_点燃时.SetActive(true);
        //go_熄灭时.SetActive(true);
    }


    private void OnDestroy()
    {
        KillCurrentTween();
        KillBurningTween();
    }
}