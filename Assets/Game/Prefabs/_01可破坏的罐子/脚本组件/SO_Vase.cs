using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using MoreMountains.Tools;



#if UNITY_EDITOR
using UnityEditor;

#endif

[CreateAssetMenu(fileName = "SO_花瓶", menuName = "LuminaOdyssey/SO_花瓶", order = 1)]
public class SO_Vase : ScriptableObject
{
    [System.Flags]
    public enum VaseColor
    {
        _棕色 = 1 << 0,
        _深绿色 = 1 << 1,
        _紫色 = 1 << 2,
        _红棕色 = 1 << 3,
        _绿色 = 1 << 4,
        _蓝绿色 = 1 << 5
    }

    /// <summary>
    /// 获取随机花瓶颜色枚举值
    /// </summary>
    /// <returns>随机的VaseColor枚举值</returns>
    public VaseColor GetRandomVaseColor()
    {
        // 获取所有可能的颜色值
        VaseColor[] allColors = (VaseColor[])System.Enum.GetValues(typeof(VaseColor));
        List<VaseColor> permittedColorsList = new List<VaseColor>();

        // 筛选出所有被允许的颜色
        foreach (var color in allColors)
        {
            if ((PermittedColors & color) != 0)
            {
                permittedColorsList.Add(color);
            }
        }

        // 如果没有允许的颜色，返回默认值
        if (permittedColorsList.Count == 0)
        {
            Debug.LogWarning("没有设置任何允许的颜色！返回第一个颜色。");
            return allColors.Length > 0 ? allColors[0] : 0;
        }

        // 从允许的颜色中随机选择
        return permittedColorsList[Random.Range(0, permittedColorsList.Count)];
    }



    [SerializeField]
    [EnumToggleButtons]
    private VaseColor PermittedColors = VaseColor._棕色 | VaseColor._深绿色 | VaseColor._紫色 |
                                      VaseColor._红棕色 | VaseColor._绿色 | VaseColor._蓝绿色;

    [Header("完整状态")]
    public Sprite[] Vases_棕色;
    public Sprite[] Vases_深绿色;
    public Sprite[] Vases_紫色;
    public Sprite[] Vases_红棕色;
    public Sprite[] Vases_绿色;
    public Sprite[] Vases_蓝绿色;

    [Header("破碎状态")]
    public Sprite[] Vases_棕色_破碎;
    public Sprite[] Vases_深绿色_破碎;
    public Sprite[] Vases_紫色_破碎;
    public Sprite[] Vases_红棕色_破碎;
    public Sprite[] Vases_绿色_破碎;
    public Sprite[] Vases_蓝绿色_破碎;


    /// <summary>
    /// 获取随机完整花瓶
    /// </summary>
    /// <param name="color">花瓶颜色</param>
    /// <returns>随机花瓶精灵，如果列表为空则返回null</returns>
    public Sprite GetRandomVase(VaseColor color)
    {
        Sprite[] vaseArray = GetVaseArrayByColor(color);
        return GetRandomSpriteFromArray(vaseArray);
    }

    /// <summary>
    /// 获取随机破碎花瓶
    /// </summary>
    /// <param name="color">花瓶颜色</param>
    /// <returns>随机破碎花瓶精灵，如果列表为空则返回null</returns>
    public Sprite GetRandomBrokenVase(VaseColor color)
    {
        Sprite[] vaseArray = GetBrokenVaseArrayByColor(color);
        return GetRandomSpriteFromArray(vaseArray);
    }


    // 根据颜色获取对应的完整花瓶数组
    private Sprite[] GetVaseArrayByColor(VaseColor color)
    {
        return color switch
        {
            VaseColor._棕色 => Vases_棕色,
            VaseColor._深绿色 => Vases_深绿色,
            VaseColor._紫色 => Vases_紫色,
            VaseColor._红棕色 => Vases_红棕色,
            VaseColor._绿色 => Vases_绿色,
            VaseColor._蓝绿色 => Vases_蓝绿色,
            _ => new Sprite[0],
        };
    }

    // 根据颜色获取对应的破碎花瓶数组
    
    private Sprite[] GetBrokenVaseArrayByColor(VaseColor color)
    {
        return color switch
        {
            VaseColor._棕色 => Vases_棕色_破碎,
            VaseColor._深绿色 => Vases_深绿色_破碎,
            VaseColor._紫色 => Vases_紫色_破碎,
            VaseColor._红棕色 => Vases_红棕色_破碎,
            VaseColor._绿色 => Vases_绿色_破碎,
            VaseColor._蓝绿色 => Vases_蓝绿色_破碎,
            _ => new Sprite[0],
        };
    }

    // 从数组中随机获取一个精灵
    private Sprite GetRandomSpriteFromArray(Sprite[] spriteArray)
    {
        if (spriteArray == null || spriteArray.Length == 0)
        {
            Debug.LogWarning($"尝试从空数组中获取精灵");
            return null;
        }

        return spriteArray[Random.Range(0, spriteArray.Length)];
    }


#if UNITY_EDITOR



    
    [Header("资源路径_完整状态")]
    public DefaultAsset ArtFolder_Vases_棕色;
    public DefaultAsset ArtFolder_深绿色;
    public DefaultAsset ArtFolder_Vases_紫色;
    public DefaultAsset ArtFolder_Vases_红棕色;
    public DefaultAsset ArtFolder_Vases_绿色;
    public DefaultAsset ArtFolder_Vases_蓝绿色;

    [Header("资源路径_破碎状态")]
    public DefaultAsset ArtFolder_Vases_棕色_破碎;
    public DefaultAsset ArtFolder_Vases_深绿色_破碎;
    public DefaultAsset ArtFolder_Vases_紫色_破碎;
    public DefaultAsset ArtFolder_Vases_红棕色_破碎;
    public DefaultAsset ArtFolder_Vases_绿色_破碎;
    public DefaultAsset ArtFolder_Vases_蓝绿色_破碎;

    [Button("AutoFill")]
    private void AutoFill()
    {
        Undo.RecordObject(this, "Auto Fill Vase Sprites");

        // 完整状态填充
        FillSpritesFromFolder(ArtFolder_Vases_棕色, ref Vases_棕色);
        FillSpritesFromFolder(ArtFolder_深绿色, ref Vases_深绿色);
        FillSpritesFromFolder(ArtFolder_Vases_紫色, ref Vases_紫色);
        FillSpritesFromFolder(ArtFolder_Vases_红棕色, ref Vases_红棕色);
        FillSpritesFromFolder(ArtFolder_Vases_绿色, ref Vases_绿色);
        FillSpritesFromFolder(ArtFolder_Vases_蓝绿色, ref Vases_蓝绿色);

        // 破碎状态填充
        FillSpritesFromFolder(ArtFolder_Vases_棕色_破碎, ref Vases_棕色_破碎);
        FillSpritesFromFolder(ArtFolder_Vases_深绿色_破碎, ref Vases_深绿色_破碎);
        FillSpritesFromFolder(ArtFolder_Vases_紫色_破碎, ref Vases_紫色_破碎);
        FillSpritesFromFolder(ArtFolder_Vases_红棕色_破碎, ref Vases_红棕色_破碎);
        FillSpritesFromFolder(ArtFolder_Vases_绿色_破碎, ref Vases_绿色_破碎);
        FillSpritesFromFolder(ArtFolder_Vases_蓝绿色_破碎, ref Vases_蓝绿色_破碎);

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

    }

    private void FillSpritesFromFolder(DefaultAsset folder, ref Sprite[] targetArray)
    {
        if (folder == null)
        {
            Debug.LogWarning($"未指定文件夹，跳过填充");
            return;
        }

        string folderPath = AssetDatabase.GetAssetPath(folder);

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogError($"无效文件夹路径: {folderPath}");
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
            Debug.LogWarning($"文件夹 {folderPath} 中没有找到任何Sprite");
            return;
        }

        targetArray = sprites.ToArray();
        Debug.Log($"成功从 {folderPath} 加载 {sprites.Count} 个Sprite");
    }



#endif





}