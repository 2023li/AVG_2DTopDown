using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using MoreMountains.Tools;


#if UNITY_EDITOR
using UnityEditor;
#endif


[CreateAssetMenu(fileName = "SO�����_", menuName = "LuminaOdyssey/SO�����", order = 1)]
public class SO_RandomSpritePool : ScriptableObject
{
    //�����
    
    [SerializeField]
    private Sprite[] randomPool;


    public Sprite GetRandomSprite()
    {
        return randomPool[Random.Range(0, randomPool.Length)];
    }


    #region �༭���ű�

#if UNITY_EDITOR


    public DefaultAsset targetFolder;


    [Button("�Զ����")]
    private void AutoFill()
    {
        Undo.RecordObject(this, "Auto Fill Vase Sprites");

        FillSpritesFromFolder(targetFolder, ref randomPool);
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

    #endregion
}
