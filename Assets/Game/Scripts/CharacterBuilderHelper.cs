using System.Collections;
using System.Collections.Generic;
using Assets.PixelFantasy.PixelHeroes.Common.Scripts.CharacterScripts;
using UnityEngine;

public enum CharacterBuilderPartEnum
{
    None = 0 ,
    //ͷ ���� ���� Ӧ�úϲ�Ϊһ��
    Head = 1, //ͷ �Լ�������
    Eyes = 2, //�۾�
    Ears = 3,
    Body = 4, //���� �Լ������� Ӧ�ú�ͷ�ϲ�
    Hair = 5, //ͷ��
    Armor = 6,
    Helmet = 7,
    Weapon = 8,
    Firearm = 9,
    Shield = 10,
    Cape = 11,
    Back = 12,
    Mask = 13,
    Horns = 14,
}

public class CharacterBuilderHelper 
{
   
    public static void RebuildCharacter(CharacterBuilder targetChar, CharacterBuilderPartEnum partEnum,string partName,bool isReBuild = true)
    {
        switch (partEnum)
        {
            case CharacterBuilderPartEnum.Head:
                targetChar.Head = partName;
                break;
            case CharacterBuilderPartEnum.Eyes:
                targetChar.Eyes = partName;
                break;
            case CharacterBuilderPartEnum.Ears:
                targetChar.Ears = partName;
                break;
            case CharacterBuilderPartEnum.Body:
                targetChar.Body = partName;
                break;
            case CharacterBuilderPartEnum.Hair:
                targetChar.Hair = partName;
                break;
            case CharacterBuilderPartEnum.Armor:
                targetChar.Armor = partName;
                break;
            case CharacterBuilderPartEnum.Helmet:
                targetChar.Helmet = partName;
                break;
            case CharacterBuilderPartEnum.Weapon:
                Debug.LogWarning("��Ӧ����������");
                targetChar.Weapon = partName;
                break;
            case CharacterBuilderPartEnum.Firearm:
                Debug.LogWarning("��Ӧ�����û���");
                targetChar.Firearm = partName;
                break;
            case CharacterBuilderPartEnum.Shield:
                targetChar.Shield = partName;
                break;
            case CharacterBuilderPartEnum.Cape:
                targetChar.Cape = partName;
                break;
            case CharacterBuilderPartEnum.Back:
                targetChar.Back = partName;
                break;
            case CharacterBuilderPartEnum.Mask:
                targetChar.Mask = partName;
                break;
            case CharacterBuilderPartEnum.Horns:
                targetChar.Horns = partName;
                break;
        }
        if (isReBuild)
        {
            targetChar.Rebuild();
        }
    }


    public static void CharacterDressToDic(CharacterBuilder targetChar,ref Dictionary<CharacterBuilderPartEnum, string> dic_CharDress)
    {

        if (dic_CharDress == null) { dic_CharDress = new Dictionary<CharacterBuilderPartEnum, string>(); }
        dic_CharDress.Clear();
        dic_CharDress.Add(CharacterBuilderPartEnum.Head, targetChar.Head);
        dic_CharDress.Add(CharacterBuilderPartEnum.Eyes, targetChar.Eyes);
        dic_CharDress.Add(CharacterBuilderPartEnum.Ears, targetChar.Ears);
        dic_CharDress.Add(CharacterBuilderPartEnum.Body, targetChar.Body);
        dic_CharDress.Add(CharacterBuilderPartEnum.Hair, targetChar.Hair);
        dic_CharDress.Add(CharacterBuilderPartEnum.Armor, targetChar.Armor);
        dic_CharDress.Add(CharacterBuilderPartEnum.Helmet, targetChar.Helmet);
        dic_CharDress.Add(CharacterBuilderPartEnum.Weapon, targetChar.Weapon);
        dic_CharDress.Add(CharacterBuilderPartEnum.Firearm, targetChar.Firearm);
        dic_CharDress.Add(CharacterBuilderPartEnum.Shield, targetChar.Shield);
        dic_CharDress.Add(CharacterBuilderPartEnum.Cape, targetChar.Cape);
        dic_CharDress.Add(CharacterBuilderPartEnum.Back, targetChar.Back);
        dic_CharDress.Add(CharacterBuilderPartEnum.Mask, targetChar.Mask);
        dic_CharDress.Add(CharacterBuilderPartEnum.Horns, targetChar.Horns);
    }

    public static void DicToCharacterDress(CharacterBuilder targetChar, ref Dictionary<CharacterBuilderPartEnum, string> dic_CharDress)
    {
        if (dic_CharDress == null) 
        {
            
            Debug.Log("װ������Ϊ��");
            return;
        }
        if (targetChar==null)
        {
            Debug.Log("Ŀ���ɫΪ��");
            return;
        }

        foreach (var parts in dic_CharDress.Keys)
        {
            RebuildCharacter(targetChar, parts, dic_CharDress[parts],false);
        }
        targetChar.Rebuild();
    }


}
