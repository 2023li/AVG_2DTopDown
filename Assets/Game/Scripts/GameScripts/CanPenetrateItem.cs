using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Collider2D))]
public class CanPenetrateItem : MonoBehaviour
{
    [MMInformation("这个脚本应该挂载在一个单独物体上", MMInformationAttribute.InformationType.Info, false)]


    [SerializeField]
    [MMLabel("可透视物体的指定Shader")]
    private Material material;
 
    [SerializeField]
    private Renderer mainRenderer;

    private void Awake()
    {
        if (mainRenderer == null) 
        {
            Debug.LogWarning("可透视对象未设置主渲染器，尝试自动获取父对象渲染器");
            mainRenderer = transform.parent.GetComponent<Renderer>();
            if (mainRenderer == null) 
            {
                Debug.LogWarning("可透视对象未设置主渲染器，尝试自动获取失败");
            }
        }
        else
        {
            mainRenderer.material = material;
        }
        var cols = GetComponents<Collider2D>();
        foreach (Collider2D col in cols) 
        {
            col.isTrigger = true;
        }

        gameObject.layer = GameConstant.CanPenetrateLayer;
        //设置所有的子对象到这一层
        for (int i = 0; i < transform.childCount; i++) 
        {
            //25是可透视层常量
            transform.GetChild(i).gameObject.layer = GameConstant.CanPenetrateLayer;
        }  
    }


    public Renderer GetMainRenderer()
    {
        if (mainRenderer == null) Debug.LogWarning("渲染器未设置");
        return mainRenderer;
    }



   


}
