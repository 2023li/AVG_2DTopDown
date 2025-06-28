using System.Collections;
using System.Collections.Generic;
using Assets.PixelFantasy.PixelHeroes.Common.Scripts.CharacterScripts;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIItem_PlayerOption : MonoBehaviour
{
    [MMLabel("Ŀ���ɫ")]
    public CharacterBuilder targetChar;
    [MMLabel("Ŀ���������")]
    public CharacterBuilderPartEnum partType;
    [MMLabel("�Ƿ��ػ��ɫ")]
    public bool isRebuildCharacterOnValueChange = true;

    //��߰�ť
    [SerializeField]
    private Button btn_L;
    //�ұ߰�ť
    [SerializeField]
    private Button btn_R;
    //��ʾֵ���ı�
    [SerializeField]
    private TMP_Text value;

    // ����Inspector�����õ�ѡ���б�
    [SerializeField]
    public List<string> AllowedValues = new List<string>();

    // ��ǰѡ�е�����
    private int currentIndex = 0;

    private void Awake()
    {
        btn_L.onClick.AddListener(PreviousValue);
        btn_R.onClick.AddListener(NextValue);
    }

    void Start()
    {
        // ��ʼ����ʾ��һ��ֵ
        UpdateDisplay();
    }

    // ��ʾ��һ��ֵ
    private void NextValue()
    {
        currentIndex = (currentIndex + 1) % AllowedValues.Count;
        UpdateDisplay();
    }

    // ��ʾ��һ��ֵ
    private void PreviousValue()
    {
        currentIndex = (currentIndex - 1 + AllowedValues.Count) % AllowedValues.Count;
        UpdateDisplay();
    }

    // �����ı���ʾ
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
