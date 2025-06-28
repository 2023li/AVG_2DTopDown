
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[InitializeOnLoad]
public class EditorScenesHelper
{
    private static HashSet<GameObject> previousObjects = new HashSet<GameObject>();

    static EditorScenesHelper()
    {
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
        UpdatePreviousObjects();
    }

    static void UpdatePreviousObjects()
    {
        previousObjects.Clear();
        foreach (GameObject go in Object.FindObjectsOfType<GameObject>(true))
        {
            previousObjects.Add(go);
        }
    }

    static void OnHierarchyChanged()
    {
        var currentObjects = new HashSet<GameObject>(Object.FindObjectsOfType<GameObject>(true));
        var addedObjects = currentObjects.Except(previousObjects);

        foreach (var go in addedObjects)
        {
            // 如果是新增的根预制体实例
            GameObject root = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
            if (root != null && !previousObjects.Contains(root))
            {
                // 检查该预制体根对象或其子物体是否实现了接口
                var listeners = root.GetComponentsInChildren<ISceneAddedListener>(true);
                foreach (var listener in listeners)
                {
                    listener.OnAddedToScene();
                }
            }
        }

        UpdatePreviousObjects();
    }





}