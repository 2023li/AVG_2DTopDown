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
    [MMInformation("����ű�Ӧ�ù�����һ������������", MMInformationAttribute.InformationType.Info, false)]


    [SerializeField]
    [MMLabel("��͸�������ָ��Shader")]
    private Material material;
 
    [SerializeField]
    private Renderer mainRenderer;

    private void Awake()
    {
        if (mainRenderer == null) 
        {
            Debug.LogWarning("��͸�Ӷ���δ��������Ⱦ���������Զ���ȡ��������Ⱦ��");
            mainRenderer = transform.parent.GetComponent<Renderer>();
            if (mainRenderer == null) 
            {
                Debug.LogWarning("��͸�Ӷ���δ��������Ⱦ���������Զ���ȡʧ��");
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
        //�������е��Ӷ�����һ��
        for (int i = 0; i < transform.childCount; i++) 
        {
            //25�ǿ�͸�Ӳ㳣��
            transform.GetChild(i).gameObject.layer = GameConstant.CanPenetrateLayer;
        }  
    }


    public Renderer GetMainRenderer()
    {
        if (mainRenderer == null) Debug.LogWarning("��Ⱦ��δ����");
        return mainRenderer;
    }



   


}
