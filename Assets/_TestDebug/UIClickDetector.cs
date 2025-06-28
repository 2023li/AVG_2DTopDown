using UnityEngine;
using UnityEngine.EventSystems;

public class UIClickDetector : MonoBehaviour
{
    void Update()
    {
        // ������������
        if (Input.GetMouseButtonDown(0))
        {
            // ����Ƿ�����UIԪ��
            if (EventSystem.current.IsPointerOverGameObject())
            {
                // �������߼����������
                PointerEventData pointerData = new PointerEventData(EventSystem.current)
                {
                    position = Input.mousePosition
                };

                // �洢���߼����
                System.Collections.Generic.List<RaycastResult> results = new System.Collections.Generic.List<RaycastResult>();

                // ִ�����߼��
                EventSystem.current.RaycastAll(pointerData, results);

                // ����Ƿ��н��
                if (results.Count > 0)
                {
                    // ��ȡ����UIԪ�أ������Ⱦ�ģ�
                    GameObject clickedObject = results[0].gameObject;

                    // ��ӡ�����UI����
                    Debug.Log("�����UI����: " + clickedObject.name);

                    // �����Ҫ��ӡ���б������UI���Ӷ��㵽�ײ㣩
                    /*
                    for (int i = 0; i < results.Count; i++)
                    {
                        Debug.Log($"�㼶 {i}: {results[i].gameObject.name}");
                    }
                    */
                }
            }
            else
            {
                Debug.Log("���λ��û��UIԪ��");
            }
        }
    }
}