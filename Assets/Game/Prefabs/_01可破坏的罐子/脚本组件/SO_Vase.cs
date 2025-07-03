using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using MoreMountains.Tools;



#if UNITY_EDITOR
using UnityEditor;

#endif

[CreateAssetMenu(fileName = "SO_��ƿ", menuName = "LuminaOdyssey/SO_��ƿ", order = 1)]
public class SO_Vase : ScriptableObject
{
    [System.Flags]
    public enum VaseColor
    {
        _��ɫ = 1 << 0,
        _����ɫ = 1 << 1,
        _��ɫ = 1 << 2,
        _����ɫ = 1 << 3,
        _��ɫ = 1 << 4,
        _����ɫ = 1 << 5
    }

    /// <summary>
    /// ��ȡ�����ƿ��ɫö��ֵ
    /// </summary>
    /// <returns>�����VaseColorö��ֵ</returns>
    public VaseColor GetRandomVaseColor()
    {
        // ��ȡ���п��ܵ���ɫֵ
        VaseColor[] allColors = (VaseColor[])System.Enum.GetValues(typeof(VaseColor));
        List<VaseColor> permittedColorsList = new List<VaseColor>();

        // ɸѡ�����б��������ɫ
        foreach (var color in allColors)
        {
            if ((PermittedColors & color) != 0)
            {
                permittedColorsList.Add(color);
            }
        }

        // ���û���������ɫ������Ĭ��ֵ
        if (permittedColorsList.Count == 0)
        {
            Debug.LogWarning("û�������κ��������ɫ�����ص�һ����ɫ��");
            return allColors.Length > 0 ? allColors[0] : 0;
        }

        // ���������ɫ�����ѡ��
        return permittedColorsList[Random.Range(0, permittedColorsList.Count)];
    }



    [SerializeField]
    [EnumToggleButtons]
    private VaseColor PermittedColors = VaseColor._��ɫ | VaseColor._����ɫ | VaseColor._��ɫ |
                                      VaseColor._����ɫ | VaseColor._��ɫ | VaseColor._����ɫ;

    [Header("����״̬")]
    public Sprite[] Vases_��ɫ;
    public Sprite[] Vases_����ɫ;
    public Sprite[] Vases_��ɫ;
    public Sprite[] Vases_����ɫ;
    public Sprite[] Vases_��ɫ;
    public Sprite[] Vases_����ɫ;

    [Header("����״̬")]
    public Sprite[] Vases_��ɫ_����;
    public Sprite[] Vases_����ɫ_����;
    public Sprite[] Vases_��ɫ_����;
    public Sprite[] Vases_����ɫ_����;
    public Sprite[] Vases_��ɫ_����;
    public Sprite[] Vases_����ɫ_����;


    /// <summary>
    /// ��ȡ���������ƿ
    /// </summary>
    /// <param name="color">��ƿ��ɫ</param>
    /// <returns>�����ƿ���飬����б�Ϊ���򷵻�null</returns>
    public Sprite GetRandomVase(VaseColor color)
    {
        Sprite[] vaseArray = GetVaseArrayByColor(color);
        return GetRandomSpriteFromArray(vaseArray);
    }

    /// <summary>
    /// ��ȡ������黨ƿ
    /// </summary>
    /// <param name="color">��ƿ��ɫ</param>
    /// <returns>������黨ƿ���飬����б�Ϊ���򷵻�null</returns>
    public Sprite GetRandomBrokenVase(VaseColor color)
    {
        Sprite[] vaseArray = GetBrokenVaseArrayByColor(color);
        return GetRandomSpriteFromArray(vaseArray);
    }


    // ������ɫ��ȡ��Ӧ��������ƿ����
    private Sprite[] GetVaseArrayByColor(VaseColor color)
    {
        return color switch
        {
            VaseColor._��ɫ => Vases_��ɫ,
            VaseColor._����ɫ => Vases_����ɫ,
            VaseColor._��ɫ => Vases_��ɫ,
            VaseColor._����ɫ => Vases_����ɫ,
            VaseColor._��ɫ => Vases_��ɫ,
            VaseColor._����ɫ => Vases_����ɫ,
            _ => new Sprite[0],
        };
    }

    // ������ɫ��ȡ��Ӧ�����黨ƿ����
    
    private Sprite[] GetBrokenVaseArrayByColor(VaseColor color)
    {
        return color switch
        {
            VaseColor._��ɫ => Vases_��ɫ_����,
            VaseColor._����ɫ => Vases_����ɫ_����,
            VaseColor._��ɫ => Vases_��ɫ_����,
            VaseColor._����ɫ => Vases_����ɫ_����,
            VaseColor._��ɫ => Vases_��ɫ_����,
            VaseColor._����ɫ => Vases_����ɫ_����,
            _ => new Sprite[0],
        };
    }

    // �������������ȡһ������
    private Sprite GetRandomSpriteFromArray(Sprite[] spriteArray)
    {
        if (spriteArray == null || spriteArray.Length == 0)
        {
            Debug.LogWarning($"���Դӿ������л�ȡ����");
            return null;
        }

        return spriteArray[Random.Range(0, spriteArray.Length)];
    }


#if UNITY_EDITOR



    
    [Header("��Դ·��_����״̬")]
    public DefaultAsset ArtFolder_Vases_��ɫ;
    public DefaultAsset ArtFolder_����ɫ;
    public DefaultAsset ArtFolder_Vases_��ɫ;
    public DefaultAsset ArtFolder_Vases_����ɫ;
    public DefaultAsset ArtFolder_Vases_��ɫ;
    public DefaultAsset ArtFolder_Vases_����ɫ;

    [Header("��Դ·��_����״̬")]
    public DefaultAsset ArtFolder_Vases_��ɫ_����;
    public DefaultAsset ArtFolder_Vases_����ɫ_����;
    public DefaultAsset ArtFolder_Vases_��ɫ_����;
    public DefaultAsset ArtFolder_Vases_����ɫ_����;
    public DefaultAsset ArtFolder_Vases_��ɫ_����;
    public DefaultAsset ArtFolder_Vases_����ɫ_����;

    [Button("AutoFill")]
    private void AutoFill()
    {
        Undo.RecordObject(this, "Auto Fill Vase Sprites");

        // ����״̬���
        FillSpritesFromFolder(ArtFolder_Vases_��ɫ, ref Vases_��ɫ);
        FillSpritesFromFolder(ArtFolder_����ɫ, ref Vases_����ɫ);
        FillSpritesFromFolder(ArtFolder_Vases_��ɫ, ref Vases_��ɫ);
        FillSpritesFromFolder(ArtFolder_Vases_����ɫ, ref Vases_����ɫ);
        FillSpritesFromFolder(ArtFolder_Vases_��ɫ, ref Vases_��ɫ);
        FillSpritesFromFolder(ArtFolder_Vases_����ɫ, ref Vases_����ɫ);

        // ����״̬���
        FillSpritesFromFolder(ArtFolder_Vases_��ɫ_����, ref Vases_��ɫ_����);
        FillSpritesFromFolder(ArtFolder_Vases_����ɫ_����, ref Vases_����ɫ_����);
        FillSpritesFromFolder(ArtFolder_Vases_��ɫ_����, ref Vases_��ɫ_����);
        FillSpritesFromFolder(ArtFolder_Vases_����ɫ_����, ref Vases_����ɫ_����);
        FillSpritesFromFolder(ArtFolder_Vases_��ɫ_����, ref Vases_��ɫ_����);
        FillSpritesFromFolder(ArtFolder_Vases_����ɫ_����, ref Vases_����ɫ_����);

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

    }

    private void FillSpritesFromFolder(DefaultAsset folder, ref Sprite[] targetArray)
    {
        if (folder == null)
        {
            Debug.LogWarning($"δָ���ļ��У��������");
            return;
        }

        string folderPath = AssetDatabase.GetAssetPath(folder);

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogError($"��Ч�ļ���·��: {folderPath}");
            return;
        }

        List<Sprite> sprites = new List<Sprite>();
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
            {
                sprites.Add(sprite);
            }
        }

        if (sprites.Count == 0)
        {
            Debug.LogWarning($"�ļ��� {folderPath} ��û���ҵ��κ�Sprite");
            return;
        }

        targetArray = sprites.ToArray();
        Debug.Log($"�ɹ��� {folderPath} ���� {sprites.Count} ��Sprite");
    }



#endif





}