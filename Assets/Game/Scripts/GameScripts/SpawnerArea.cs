using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Collider2D))]
public class SpawnerArea : MonoBehaviour
{
    [System.Serializable]
    public class SpawnableObject
    {
        public GameObject prefab;
        [Min(0)] public float weight = 1;
    }


    [Header("生成设置")]
    [Min(0)] public int spawnCount = 10;
    [Tooltip("生成尝试的最大次数")]
    [Min(1)] public int maxSpawnAttempts = 1000;

    [Header("物品池设置")]
    [SerializeField] private List<SpawnableObject> spawnPool = new List<SpawnableObject>();

    private Collider2D _collider;


    private void Reset()
    {
        _collider = GetComponent<Collider2D>();
    }
    void Start()
    {
        InitializeComponents();
        ValidateSettings();
        SpawnObjects();
    }

    void InitializeComponents()
    {
        _collider = GetComponent<Collider2D>();
        _collider.isTrigger = true; // 确保碰撞体为触发器
    }

    void ValidateSettings()
    {
        if (spawnPool.Count == 0)
        {
            Debug.LogError("生成池不能为空！请添加至少一个可生成对象");
            enabled = false;
            return;
        }

        if (CalculateTotalWeight() <= 0)
        {
            Debug.LogError("总权重必须大于零！");
            enabled = false;
        }
    }

    [Button("生成")]
    public virtual List<GameObject> SpawnObjects()
    {
        List<GameObject> list = new List<GameObject>();
        for (int i = 0; i < spawnCount; i++)
        {
            var selected = GetRandomSpawnable();
            if (selected.prefab != null && TryGetRandomPositionInCollider(out Vector2 position))
            {
               list.Add(Instantiate(selected.prefab, position, Quaternion.identity));
            }
        }
        return list;
    }




    SpawnableObject GetRandomSpawnable()
    {
        float totalWeight = CalculateTotalWeight();
        float randomValue = Random.Range(0, totalWeight);
        float accumulated = 0;

        foreach (var item in spawnPool)
        {
            accumulated += item.weight;
            if (randomValue <= accumulated)
            {
                return item;
            }
        }

        return spawnPool[0];
    }

    float CalculateTotalWeight()
    {
        float total = 0;
        foreach (var item in spawnPool)
        {
            total += item.weight;
        }
        return total;
    }

    bool TryGetRandomPositionInCollider(out Vector2 position)
    {
        position = Vector2.zero;
        int attempts = 0;

        do
        {
            position = GetRandomPointInBounds();
            attempts++;

            if (attempts > maxSpawnAttempts)
            {
                Debug.LogWarning("达到最大尝试次数，无法在碰撞体内找到有效位置");
                return false;
            }

        } while (!IsPointInCollider(position));

        return true;
    }

    Vector2 GetRandomPointInBounds()
    {
        Bounds bounds = _collider.bounds;
        return new Vector2(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y)
        );
    }

    bool IsPointInCollider(Vector2 point)
    {
        // 使用OverlapPoint方法进行碰撞检测，支持所有2D碰撞体类型
        return _collider.OverlapPoint(point);
    }
}