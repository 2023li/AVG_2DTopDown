using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleMaskController : MonoBehaviour
{
    public float radius = 2f;      // ���ְ뾶
    public float feather = 0.5f;   // ��Ե�𻯱���

    void Update()
    {
        // ����ɫλ�úͲ������ݸ�Shader
        Shader.SetGlobalVector("Center", transform.position);
        Shader.SetGlobalFloat("Radius", radius);
        Shader.SetGlobalFloat("Feather", feather);
        Debug.Log("2");
    }
}
