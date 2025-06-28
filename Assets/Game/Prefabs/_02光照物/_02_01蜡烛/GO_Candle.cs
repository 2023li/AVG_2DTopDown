using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using MoreMountains.Tools;


public class GO_Candle : MonoBehaviour, ISceneAddedListener
{
    [SerializeField]
    [MMLabel("�Ƿ��һ����ӵ�����")]

    private bool isFristAddToScenes = true;
    [SerializeField]
    [MMLabel("���ʱ�Ƿ��������")]
    private bool isRandomScaleOnAddScene = false;
    
    [ShowIf("isRandomScaleOnAddScene",false)]
    [SerializeField]
    private float minScale = 2;
    
    [ShowIf("isRandomScaleOnAddScene", false)]
    [SerializeField]
    private float maxScale = 2.5f;
    private void RandomScale()
    {
        //ֻ�ڵ�һ�����ʱ�����
        if (!isRandomScaleOnAddScene) { return; }

        if (isRandomScaleOnAddScene)
        {
            float scale = Random.Range(minScale, maxScale);
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }










    #region

    [SerializeField]
    [FoldoutGroup("�༭��")]
    [MMLabel("���ʱ�Ƿ��������״̬")]
    private bool isSetRandomState = true;
    public void OnAddedToScene()
    {


        if (!isFristAddToScenes) { return; }
        isFristAddToScenes = false;

        ������ = transform.GetChild(0).gameObject;
        Ϩ������ = transform.GetChild(1).gameObject;
        ����_��ȼʱ = transform.GetChild(2).gameObject;
        ����_Ϩ��ʱ = transform.GetChild(3).gameObject;
        sr_candle_fire = ����_��ȼʱ.GetComponent<SpriteRenderer>();
        sr_candle_noFire = ����_Ϩ��ʱ.GetComponent<SpriteRenderer>();
        sr_smoke = Ϩ������.GetComponent<SpriteRenderer>();
        sr_fire = ������.GetComponent<SpriteRenderer>();


        

        if (isSetRandomState)
        {
            int i = Random.Range(0, 2);
            if (i > 0)
            {
                SetState(GO_Candle.CandleState.Ϩ��);
                Ϩ������.gameObject.SetActive(false);

            }
            else
            {
                SetState(GO_Candle.CandleState.ȼ��);
            }
        }

        RandomScale();

    }

    #endregion





    public enum CandleState
    {
        ȼ��,
        ���ڵ�ȼ,
        ����Ϩ��,
        Ϩ��
    }

    [Header("������ȼ")]
    public float quickIgnitionTime = 0.2f;
    public AnimationCurve quickIgnitionCurve;

    [Header("��Ȼ��ȼ")]
    public float normalIgnitionTime = 1;
    public AnimationCurve normaIgnitionCurve;

    [Header("����Ϩ��")]
    public float quickExtinguishTime = 1;
    public AnimationCurve quickExtinguishCurve;


    [Header("��ȻϨ��")]
    public float normaExtinguishTime = 1;
    public AnimationCurve normaExtinguishCurve;

    [Header("ȼ��״̬")]
    public AnimationCurve burningCurve = AnimationCurve.Linear(0, 0.8f, 1, 1.2f);
    public float burningSpeed = 2f;

    [Header("Ϩ������")]
    public float smokeTime = 7;
    public AnimationCurve SmokeTransparencyCurve;
    public AnimationCurve SmokeScalingCurve;

    // �������
    private GameObject ������;
    private SpriteRenderer sr_fire;
    private GameObject Ϩ������;
    private SpriteRenderer sr_smoke;
    private GameObject ����_��ȼʱ;
    private SpriteRenderer sr_candle_fire;
    private GameObject ����_Ϩ��ʱ;
    private SpriteRenderer sr_candle_noFire;

    [SerializeField]
    [MMReadOnly]
    private CandleState state = CandleState.ȼ��;


    private Tween burningTween;
    private Tween currentTween;





    private void Awake()
    {
        ������ = transform.GetChild(0).gameObject;
        Ϩ������ = transform.GetChild(1).gameObject;
        ����_��ȼʱ = transform.GetChild(2).gameObject;
        ����_Ϩ��ʱ = transform.GetChild(3).gameObject;
        sr_candle_fire = ����_��ȼʱ.GetComponent<SpriteRenderer>();
        sr_candle_noFire = ����_Ϩ��ʱ.GetComponent<SpriteRenderer>();
        sr_smoke = Ϩ������.GetComponent<SpriteRenderer>();
        sr_fire = ������.GetComponent<SpriteRenderer>();

        InitializeState();
    }

    private void InitializeState()
    {

        SetState(state);
        if (state == CandleState.ȼ��)
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
        if (state != CandleState.Ϩ��) return;

        SetState(CandleState.���ڵ�ȼ);
        KillCurrentTween();

        currentTween = CreateIgnitionTween(quickIgnitionTime, quickIgnitionCurve)
            .OnComplete(() =>
            {
                SetState(CandleState.ȼ��);
                Burning();
            });
    }
    [Button("��Ȼ��ȼ")]
    public void NormalIgnition()
    {
        if (state != CandleState.Ϩ��) return;

        SetState(CandleState.���ڵ�ȼ);
        KillCurrentTween();

        currentTween = CreateIgnitionTween(normalIgnitionTime, normaIgnitionCurve)
            .OnComplete(() =>
            {
                SetState(CandleState.ȼ��);
              
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
    /// ����Ϩ��
    /// </summary>
    [Button("����Ϩ��")]
    public void QuicklyExtinguish()
    {
   
        if (state != CandleState.ȼ��) return;

        SetState(CandleState.����Ϩ��);
        KillCurrentTween();

        currentTween = DOTween.To(() => 1f, t =>
        {
            float value = quickExtinguishCurve.Evaluate(1 - t);
            SetBurningState(value);
        }, 0f, quickExtinguishTime)
        .OnComplete(() =>
        {
            PlayExtinguishSmoke();
            //state = CandleState.Ϩ��;
            SetState(CandleState.Ϩ��);
        });

    }


    [Button("��ȻϨ��")]
    public void NormalExtinguishing()
    {
        if (state != CandleState.ȼ��) return;

        SetState(CandleState.����Ϩ��);
        KillCurrentTween();

        currentTween = DOTween.To(() => 1f, t =>
        {
            float value = normaExtinguishCurve.Evaluate(1 - t);
            SetBurningState(value);
        }, 0f, normaExtinguishTime)
        .OnComplete(() =>
        {
            PlayExtinguishSmoke();
            //state = CandleState.Ϩ��;
            SetState(CandleState.Ϩ��);
        });
    }

    private void PlayExtinguishSmoke()
    {
        Ϩ������.SetActive(true);
        sr_smoke.color = new Color(1, 1, 1, 0);
        sr_smoke.transform.localScale = Vector3.zero;

        Sequence smokeSeq = DOTween.Sequence();
        smokeSeq.Append(DOTween.To(() => 0f, t =>
        {
            float progress = t / smokeTime;
            sr_smoke.color = new Color(1, 1, 1, SmokeTransparencyCurve.Evaluate(progress));
            sr_smoke.transform.localScale = Vector3.one * SmokeScalingCurve.Evaluate(progress);
        }, smokeTime, smokeTime).SetEase(Ease.Linear));
        smokeSeq.OnComplete(() => Ϩ������.SetActive(false));
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


    [Button("Editor_�л�״̬")]
    private void Editor_ChangeState()
    {
        if (state==CandleState.ȼ��)
        {
            SetState(CandleState.Ϩ��);
        }
        else
        {
            SetState(CandleState.ȼ��);
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
            case CandleState.ȼ��:
                ������.SetActive(true);
                ����_��ȼʱ.SetActive(true);
                ����_Ϩ��ʱ.SetActive(false);
                Ϩ������.SetActive(false);

                break;
            case CandleState.Ϩ��:
                ������.SetActive(false);
                ����_��ȼʱ.SetActive(false);
                ����_Ϩ��ʱ.SetActive(true);
                Ϩ������.SetActive(true);

                break;
            default:
                ������.SetActive(true);
                ����_��ȼʱ.SetActive(true);
                ����_Ϩ��ʱ.SetActive(true);
                Ϩ������.SetActive(false);
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