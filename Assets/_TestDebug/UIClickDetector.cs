using UnityEngine;
using UnityEngine.EventSystems;

public class UIClickDetector : MonoBehaviour
{
    void Update()
    {
        // 检测鼠标左键点击
        if (Input.GetMouseButtonDown(0))
        {
            // 检查是否点击到UI元素
            if (EventSystem.current.IsPointerOverGameObject())
            {
                // 创建射线检测所需数据
                PointerEventData pointerData = new PointerEventData(EventSystem.current)
                {
                    position = Input.mousePosition
                };

                // 存储射线检测结果
                System.Collections.Generic.List<RaycastResult> results = new System.Collections.Generic.List<RaycastResult>();

                // 执行射线检测
                EventSystem.current.RaycastAll(pointerData, results);

                // 检查是否有结果
                if (results.Count > 0)
                {
                    // 获取最顶层的UI元素（最后渲染的）
                    GameObject clickedObject = results[0].gameObject;

                    // 打印点击的UI名称
                    Debug.Log("点击的UI名称: " + clickedObject.name);

                    // 如果需要打印所有被点击的UI（从顶层到底层）
                    /*
                    for (int i = 0; i < results.Count; i++)
                    {
                        Debug.Log($"层级 {i}: {results[i].gameObject.name}");
                    }
                    */
                }
            }
            else
            {
                Debug.Log("点击位置没有UI元素");
            }
        }
    }
}