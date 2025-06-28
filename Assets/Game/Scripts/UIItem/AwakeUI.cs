using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AwakeUI : MonoBehaviour, MMEventListener<LOGameEvent>
{
    
    void OnEnable()
    {
        this.MMEventStartListening<LOGameEvent>();

    }
    void OnDisable()
    {
        this.MMEventStopListening<LOGameEvent>();
    }
    public void OnMMEvent(LOGameEvent eventType)
    {
        if (eventType.eventTpye == LOGameEventType.AppInitComplete)
        {
            Init();
        }
    }

    private void Init()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        transform.Find("背景装饰").gameObject.SetActive(true);


        Init_存档选择面板();
        Init_开始界面();
        Init_创建界面();
        Init_语言选择();

     


    }


    #region 开始界面


    [FoldoutGroup("开始界面")]
    public RectTransform rt_开始界面;

    [FoldoutGroup("开始界面")]
    public RectTransform rt_按钮组;
    private Vector2 rt_按钮组_originalPos;
    private bool busy_开始界面 = false;

    [FoldoutGroup("开始界面")]
    public Button btn_新游戏;
    [FoldoutGroup("开始界面")]
    public Button btn_继续;
    [FoldoutGroup("开始界面")]
    public Button btn_设置;

    private void Init_开始界面()
    {
        rt_按钮组_originalPos = rt_按钮组.anchoredPosition;

        btn_新游戏.onClick.AddListener(() =>
        {
            Show_创建界面();
        });

        btn_继续.interactable = rt_存档按钮父物体.childCount != 0;

        btn_继续.onClick.AddListener(() =>
        {
            Hide_开始界面();
            Show_存档选择面板();
        });

        btn_设置.onClick.AddListener(() =>
        {
            Debug.Log("未实现");
        });
    }

    [FoldoutGroup("开始界面")]
    [Button("Show_开始界面")]
    public void Show_开始界面()
    {
        if (busy_开始界面)
        {
            return;
        }

        rt_开始界面.gameObject.SetActive(true);

        // 设置初始位置（屏幕底部下方）
        rt_按钮组.anchoredPosition = new Vector2(rt_按钮组_originalPos.x,-Screen.height);

        // 使用DOTween动画移动到原始位置
        rt_按钮组.DOAnchorPos(rt_按钮组_originalPos, 0.8f) // 0.8秒动画时长
            .SetEase(Ease.OutBack)               // 弹性效果
            .OnStart(() =>
            {
                // 可选：启用组件（如果需要）
                rt_按钮组.gameObject.SetActive(true);
                busy_开始界面 = true;
            }).OnComplete(() =>
            {
                busy_开始界面 =false;
            });
            


    }

    [FoldoutGroup("开始界面")]
    [Button("Hide_开始界面")]
    public void Hide_开始界面()
    {
        if (busy_开始界面)
        {
            return;
        }


        Vector2 hidePos = new Vector2(rt_按钮组_originalPos.x,-Screen.height);
        rt_按钮组.DOAnchorPos(hidePos, 0.8f) // 0.8秒动画时长
          .SetEase(Ease.OutBack).OnStart(() =>
          {
              busy_开始界面=true;
          })
          .OnComplete(() =>
          {
              busy_开始界面=false;
              rt_按钮组.gameObject.SetActive(false);
          });
         
    }





    #endregion

    #region 语言选择弹窗
    [FoldoutGroup("语言选择")]
    public RectTransform rt_语言选择面板;

    private Vector2 rt_语言选择面板_原始大小;

    [FoldoutGroup("语言选择")]
    public Button btn_确认语言选择;

    [FoldoutGroup("语言选择")]
    public TMP_Dropdown dropdown_语言选择下拉菜单;

    public void Init_语言选择()
    {

        rt_语言选择面板_原始大小 = rt_语言选择面板.anchoredPosition;

        btn_确认语言选择.onClick.AddListener(() =>
        {
            Hide_语言选择面板();
            Show_开始界面();
            PersistenceManager.Instance.SetFirstLaunch();
            PersistenceManager.Instance.SetLanguage(GameHelper.Int2LOGameLanguage(dropdown_语言选择下拉菜单.value));
        });

        if (PersistenceManager.Instance.FirstLaunch)
        {
            Show_语言选择();
        }

    }
    [FoldoutGroup("语言选择")]
    [Button("Show_语言选择")]
    public void Show_语言选择()
    {
        rt_语言选择面板.DOScale(Vector2.one, 0.2f).From(Vector2.zero);

        rt_语言选择面板.gameObject.SetActive(true);
    }
    [FoldoutGroup("语言选择")]
    [Button("Hide_语言选择面板")]
    private void Hide_语言选择面板()
    {
        rt_语言选择面板.DOScale(Vector2.zero, 0.2f).OnComplete(() =>
        {
            rt_语言选择面板.gameObject.SetActive(false);
        });


        
    }



    #endregion

    #region 存档选择面板
    [FoldoutGroup("存档选择面板")]
    public SaveSelectButon pr_存档选择按钮;

    private bool isBusy_存档选择面板;
    private Vector2 rt_存档按钮父物体_原始位置;

    [FoldoutGroup("存档选择面板")]
    public RectTransform rt_存档选择面板;
    [FoldoutGroup("存档选择面板")]
    public RectTransform rt_存档按钮父物体;


    [FoldoutGroup("存档选择面板")]
    public Button btn_存档选择返回主页面;

    public void Init_存档选择面板()
    {
        rt_存档按钮父物体_原始位置 = rt_存档按钮父物体.anchoredPosition;


        for (int i = rt_存档按钮父物体.childCount - 1; i >= 0; i--)
        {
            Destroy(rt_存档按钮父物体.GetChild(i).gameObject);
        }

        var list = PersistenceManager.Instance.GetAllSaveGames();
        for (int i = 0; i < list.Count; i++) 
        {
            SaveSelectButon sbtn =  Instantiate(pr_存档选择按钮.gameObject).GetComponent<SaveSelectButon>();
            sbtn.Init(list[i].PlayerName, list[i].SaveTime, list[i].SavePath);
            sbtn.transform.SetParent(rt_存档按钮父物体);
        }


        btn_存档选择返回主页面.onClick.AddListener(() =>
        {
            Hide_存档界面();
            Show_开始界面();
        });


    }

    [FoldoutGroup("存档选择面板")]
    [Button("Show_存档选择面板")]

    public void Show_存档选择面板()
    {
        if (isBusy_存档选择面板)
        {
            return;
        }

        rt_存档选择面板.gameObject.SetActive (true);
        rt_存档选择面板.anchoredPosition = new Vector2(rt_存档按钮父物体_原始位置.x, Screen.height*2);

        // 使用DOTween动画移动到原始位置
        rt_存档选择面板.DOAnchorPos(rt_存档按钮父物体_原始位置, 0.8f) // 0.8秒动画时长
            .SetEase(Ease.OutBack)               // 弹性效果
            .OnStart(() =>
            {
                // 可选：启用组件（如果需要）
                rt_存档选择面板.gameObject.SetActive(true);
                isBusy_存档选择面板 = true;
            }).OnComplete(() =>
            {
                isBusy_存档选择面板 = false;
            });





    }
    [FoldoutGroup("存档选择面板")]
    [Button("Hide_存档界面")]
    public void Hide_存档界面()
    {
        if (isBusy_存档选择面板)
        {
            return;
        }


        Vector2 hidePos = new Vector2(rt_存档按钮父物体_原始位置.x, Screen.height*2);
        rt_存档选择面板.DOAnchorPos(hidePos, 0.8f) // 0.8秒动画时长
          .SetEase(Ease.OutBack).OnStart(() =>
          {
              isBusy_存档选择面板 = true;
          })
          .OnComplete(() =>
          {
              isBusy_存档选择面板 = false;
              rt_存档选择面板.gameObject.SetActive(false);
          });




    }

    #endregion


    #region 创建界面
    [FoldoutGroup("人物创建界面")]
    public GameObject go_创建人物面板;
    [FoldoutGroup("人物创建界面")]
    public Button btn_创建完成后进入游戏;
    [FoldoutGroup("人物创建界面")]
    public Button btn_关闭面板返回主页面;
    [FoldoutGroup("人物创建界面")]
    public TMP_InputField inputField_玩家名称;

    

    private void Init_创建界面()
    {

        btn_关闭面板返回主页面.onClick.AddListener(() =>
        {
            Hide_创建界面();
        });

        btn_创建完成后进入游戏.onClick.AddListener(() =>
        {
            ScenesHelp_Awake.Instance.EnterNewGame(inputField_玩家名称.text);
          
        });
    }
    [FoldoutGroup("人物创建界面")]
    [Button("Show_创建界面")]
    private void Show_创建界面()
    {
        go_创建人物面板.gameObject.SetActive(true);

        inputField_玩家名称.text = GetRandomName();



    }
    [FoldoutGroup("人物创建界面")]
    [Button("Hide_创建界面")]
    private void Hide_创建界面()
    {

     
    }

    private string GetRandomName()
    {
        return "r11";
    }


    #endregion



}
