using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Validation;
using UnityEngine;

public class CanPenetratePlayer : MonoBehaviour
{
    [Header("Settings")]
    public float minAlpha = 0.4f;
    public float radius = 2f;
    public float feather = 0.3f;
    public LayerMask obstacleLayer;
    private List<SpriteRenderer> playerAllSpriteRenderers;


    private HashSet<Renderer> dic_Colliders;
    MaterialPropertyBlock mpb;
    private void Awake()
    {
        playerAllSpriteRenderers = new List<SpriteRenderer>();
        dic_Colliders = new HashSet<Renderer>();
        mpb = new MaterialPropertyBlock();

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsInObstacleLayer(collision))
        {

            CanPenetrateItem item = collision.GetComponent<CanPenetrateItem>();

            if (item == null)
            {
                Debug.LogWarning($"{collision.gameObject.name} 在 可透视层，但是并没有挂载CanPenetrateItem组件，不做处理");
                return;
            }

            Renderer renderer = item.GetMainRenderer();
            if (!dic_Colliders.Contains(renderer))
            {
                renderer.GetPropertyBlock(mpb);

                mpb.SetVector("_Center", transform.position);
                mpb.SetFloat("_Radius", radius);
                mpb.SetFloat("_Feather", feather);
                mpb.SetFloat("_MinAlpha", minAlpha);

                dic_Colliders.Add(renderer);
            }


        }

    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (IsInObstacleLayer(collision))
        {
            foreach (Renderer renderer in dic_Colliders)
            {
                renderer.GetPropertyBlock(mpb);

                mpb.SetVector("_Center", transform.position);
                mpb.SetFloat("_Radius", radius);
                mpb.SetFloat("_Feather", feather);
                mpb.SetFloat("_MinAlpha", minAlpha);
                renderer.SetPropertyBlock(mpb);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // 检查该层是否在obstacleLayer中
        if (IsInObstacleLayer(collision))
        {
            CanPenetrateItem item = collision.GetComponent<CanPenetrateItem>();

            if (item == null)
            {
                Debug.LogWarning($"{collision.gameObject.name} 在 可透视层，但是并没有挂载CanPenetrateItem组件，不做处理");
                return;
            }
            Renderer renderer = item.GetMainRenderer();

            if (dic_Colliders.Contains(renderer))
            {
                renderer.GetPropertyBlock(mpb);
                mpb.SetVector("_Center", new Vector3(99, 99, 99));
                mpb.SetFloat("_Radius", radius);
                mpb.SetFloat("_Feather", feather);
                mpb.SetFloat("_MinAlpha", 1);
                renderer.SetPropertyBlock(mpb);

                dic_Colliders.Remove(renderer);
            }

        }

    }

    private  void SetShaderValue()
    {

    }


    //void UpdateTransparency()
    //{
    //    // 获取所有障碍物
    //    Collider2D[] obstacles = Physics2D.OverlapCircleAll(transform.position, radius, obstacleLayer);

    //    foreach (var obstacle in obstacles)
    //    {
    //        Renderer renderer = obstacle.GetComponent<Renderer>();
    //        if (renderer != null)
    //        {
    //            // 使用MaterialPropertyBlock优化性能
    //            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
    //            renderer.GetPropertyBlock(mpb);

    //            mpb.SetVector("_Center", transform.position);
    //            mpb.SetFloat("_Radius", radius);
    //            mpb.SetFloat("_Feather", feather);

    //            renderer.SetPropertyBlock(mpb);
    //        }
    //    }
    //}



    private void SetPlayerTransparency()
    {

    }

    public bool IsInObstacleLayer(Collider2D collision)
    {
        // 将碰撞对象的层转换为LayerMask值
        int collisionLayer = 1 << collision.gameObject.layer;

        // 检查该层是否在obstacleLayer中
        return (obstacleLayer.value & collisionLayer) != 0;

    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}