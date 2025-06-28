using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using MoreMountains.Tools;


#if UNITY_EDITOR
using UnityEditor;
#endif


[CreateAssetMenu(fileName = "SO随机池_", menuName = "LuminaOdyssey/SO随机池", order = 1)]
public class SO_RandomSpritePool : ScriptableObject
{
    //随机池
    
    [SerializeField]
    private Sprite[] randomPool;


    public Sprite GetRandomSprite()
    {
        return randomPool[Random.Range(0, randomPool.Length)];
    }


    #region 编辑器脚本

#if UNITY_EDITOR


    public DefaultAsset targetFolder;


    [Button("自动填充")]
    private void AutoFill()
    {
        Undo.RecordObject(this, "Auto Fill Vase Sprites");

        FillSpritesFromFolder(targetFolder, ref randomPool);
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

    #endregion
}
