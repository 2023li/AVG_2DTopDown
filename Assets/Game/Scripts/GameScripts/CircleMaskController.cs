using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleMaskController : MonoBehaviour
{
    public float radius = 2f;      // 遮罩半径
    public float feather = 0.5f;   // 边缘羽化比例

    void Update()
    {
        // 将角色位置和参数传递给Shader
        Shader.SetGlobalVector("Center", transform.position);
        Shader.SetGlobalFloat("Radius", radius);
        Shader.SetGlobalFloat("Feather", feather);
        Debug.Log("2");
    }
}
