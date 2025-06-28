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
    [FoldoutGroup("�༭��")]
    [MMLabel("���ʱ�Ƿ��������״̬")]
    private bool isSetRandomState = true;
    public void OnAddedToScene()
    {
      
        if (isSetRandomState)
        {
            int i = UnityEngine.Random.Range(0, 2);
            if (i > 0)
            {
                SetState(LightState.Ϩ��);

            }
            else
            {
                SetState(LightState.����);
            }
        }
    }
    [FoldoutGroup("�༭��")]
    [Button("�Զ�����")]
    protected virtual void AutoQuote()
    {
        go_������� = transform.GetChild(0).gameObject;
        //go_��ȼʱ = transform.GetChild(1).gameObject;
        //go_Ϩ��ʱ = transform.GetChild(2).gameObject;
        sr_���� = go_�������.GetComponent<SpriteRenderer>();
        //sr_��ȼʱmodel = go_��ȼʱ.GetComponent<SpriteRenderer>();
        //sr_Ϩ��ʱModle = go_Ϩ��ʱ.GetComponent <SpriteRenderer>();
    }

#endif
    #endregion





    public enum LightState
    {
        ����,
        ��ʼ����,
        ��ʼϨ��,
        Ϩ��
    }

    [Header("���ٵ���")]
    public float quickIgnitionTime = 0.2f;
    public AnimationCurve quickIgnitionCurve;

    [Header("��Ȼ����")]
    public float normalIgnitionTime = 1;
    public AnimationCurve normaIgnitionCurve;

    [Header("����Ϩ��")]
    public float quickExtinguishTime = 1;
    public AnimationCurve quickExtinguishCurve;


    [Header("��ȻϨ��")]
    public float normaExtinguishTime = 1;
    public AnimationCurve normaExtinguishCurve;

    [Header("��������ʱ")]
    public AnimationCurve burningCurve = AnimationCurve.Linear(0, 0.8f, 1, 1.2f);
    public float burningSpeed = 2f;

    // �������
    [SerializeField] private GameObject go_�������;
    [SerializeField] private SpriteRenderer sr_����;
    //private GameObject go_��ȼʱ;
    //private SpriteRenderer sr_��ȼʱmodel;
    //private GameObject go_Ϩ��ʱ;
    //private SpriteRenderer sr_Ϩ��ʱModle;

    [SerializeField]
    [MMReadOnly]
    private LightState state = LightState.����;


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
        if (state == LightState.����)
        {
            SetBurningState(1f);
            Burning();
        }
        else
        {
            SetExtinguishedState(1f);
        }
    }
    [Button("���ٵ�ȼ")]
    public void QuickIgnition()
    {
        if (state != LightState.Ϩ��) return;

        SetState(LightState.��ʼ����);
        KillCurrentTween();

        currentTween = CreateIgnitionTween(quickIgnitionTime, quickIgnitionCurve)
            .OnComplete(() =>
            {
                FullIgnition();
            });
    }
    [Button("��Ȼ��ȼ")]
    public void NormalIgnition()
    {
        if (state != LightState.Ϩ��) return;

        SetState(LightState.��ʼ����);
        KillCurrentTween();

        currentTween = CreateIgnitionTween(normalIgnitionTime, normaIgnitionCurve)
            .OnComplete(() =>
            {
                FullIgnition();
            });
    }

   
    protected virtual void FullIgnition()
    {
        SetState(LightState.����);
        Burning();
        OnFullIgnition?.Invoke();
    }

    private Tween CreateIgnitionTween(float duration, AnimationCurve curve)
    {
        sr_����.color = new Color(1, 1, 1, 0);
        sr_����.transform.localScale = Vector3.zero;

        return DOTween.To(() => 0f, t =>
        {
            float value = curve.Evaluate(t);
            SetBurningState(value);
        }, 1f, duration);
    }


    /// <summary>
    /// ����Ϩ��
    /// </summary>
    [Button("����Ϩ��")]
    public void QuicklyExtinguish()
    {

        if (state != LightState.����) return;

        SetState(LightState.��ʼϨ��);
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


    [Button("��ȻϨ��")]
    public void NormalExtinguishing()
    {
        if (state != LightState.����) return;

        SetState(LightState.��ʼϨ��);
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
        SetState(LightState.Ϩ��);
    }


    /// <summary>
    /// ȼ��
    /// </summary>
    private void Burning()
    {
        KillBurningTween();

        float duration = 1f / burningSpeed;
        burningTween = DOTween.To(() => 0f, t =>
        {
            t = Mathf.Repeat(t, 1f);
            float value = burningCurve.Evaluate(t);
            sr_����.transform.localScale = Vector3.one * value;
            sr_����.color = new Color(1, 1, 1, value);
        }, 1f, duration)
        .SetLoops(-1, LoopType.Restart)
        .SetEase(Ease.Linear);
    }

    private void SetBurningState(float value)
    {
        sr_����.color = new Color(1, 1, 1, value);
        sr_����.transform.localScale = Vector3.one * value;
        //sr_��ȼʱmodel.color = new Color(1, 1, 1, value);
        //sr_Ϩ��ʱModle.color = new Color(1, 1, 1, 1 - value);
    }

    private void SetExtinguishedState(float value)
    {
        //sr_��ȼʱmodel.color = new Color(1, 1, 1, 0);
        //sr_Ϩ��ʱModle.color = new Color(1, 1, 1, 1);
        sr_����.color = new Color(1, 1, 1, 0);
        sr_����.transform.localScale = Vector3.zero;
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
            case LightState.����:
                SetState_����();
                break;
            case LightState.��ʼ����:
                SetState_��ʼ����();
                break;
            case LightState.Ϩ��:
                SetState_Ϩ��();
                break;
            case LightState.��ʼϨ��:
                SetState_��ʼϨ��();
                break;
        }
        this.state = NewState;
    }

    protected virtual void SetState_����() 
    {
        go_�������.SetActive(true);
        //go_��ȼʱ.SetActive(true);
        //go_Ϩ��ʱ.SetActive(false);
    }
    protected virtual void SetState_��ʼ����() 
    {
        go_�������.SetActive(true);
        //go_��ȼʱ.SetActive(true);
        //go_Ϩ��ʱ.SetActive(true);
    }
    protected virtual void SetState_Ϩ��() 
    {
        go_�������.SetActive(false);
        //go_��ȼʱ.SetActive(false);
        //go_Ϩ��ʱ.SetActive(true);
    }
    protected virtual void SetState_��ʼϨ��() 
    {
        go_�������.SetActive(true);
        //go_��ȼʱ.SetActive(true);
        //go_Ϩ��ʱ.SetActive(true);
    }


    private void OnDestroy()
    {
        KillCurrentTween();
        KillBurningTween();
    }
}