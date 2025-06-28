using System.Collections;
using System.Collections.Generic;
using Assets.PixelFantasy.PixelHeroes.Common.Scripts.CharacterScripts;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIItem_PlayerOption : MonoBehaviour
{
    [MMLabel("目标角色")]
    public CharacterBuilder targetChar;
    [MMLabel("目标组件类型")]
    public CharacterBuilderPartEnum partType;
    [MMLabel("是否重绘角色")]
    public bool isRebuildCharacterOnValueChange = true;

    //左边按钮
    [SerializeField]
    private Button btn_L;
    //右边按钮
    [SerializeField]
    private Button btn_R;
    //显示值的文本
    [SerializeField]
    private TMP_Text value;

    // 可在Inspector中配置的选项列表
    [SerializeField]
    public List<string> AllowedValues = new List<string>();

    // 当前选中的索引
    private int currentIndex = 0;

    private void Awake()
    {
        btn_L.onClick.AddListener(PreviousValue);
        btn_R.onClick.AddListener(NextValue);
    }

    void Start()
    {
        // 初始化显示第一个值
        UpdateDisplay();
    }

    // 显示下一个值
    private void NextValue()
    {
        currentIndex = (currentIndex + 1) % AllowedValues.Count;
        UpdateDisplay();
    }

    // 显示上一个值
    private void PreviousValue()
    {
        currentIndex = (currentIndex - 1 + AllowedValues.Count) % AllowedValues.Count;
        UpdateDisplay();
    }

    // 更新文本显示
    private void UpdateDisplay()
    {
        if (AllowedValues.Count > 0 && value != null)
        {
            value.text = AllowedValues[currentIndex];
        }
        RebuildCharacter(AllowedValues[currentIndex]);
    }

    private void RebuildCharacter(string partName)
    {
        if (partType==CharacterBuilderPartEnum.None)
        {
            return;
        }

        if (isRebuildCharacterOnValueChange)
        {
            CharacterBuilderHelper.RebuildCharacter(targetChar, partType, partName);
            
        }
    }

}
