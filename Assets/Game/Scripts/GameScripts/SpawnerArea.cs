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


    [Header("��������")]
    [Min(0)] public int spawnCount = 10;
    [Tooltip("���ɳ��Ե�������")]
    [Min(1)] public int maxSpawnAttempts = 1000;

    [Header("��Ʒ������")]
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
        _collider.isTrigger = true; // ȷ����ײ��Ϊ������
    }

    void ValidateSettings()
    {
        if (spawnPool.Count == 0)
        {
            Debug.LogError("���ɳز���Ϊ�գ����������һ�������ɶ���");
            enabled = false;
            return;
        }

        if (CalculateTotalWeight() <= 0)
        {
            Debug.LogError("��Ȩ�ر�������㣡");
            enabled = false;
        }
    }

    [Button("����")]
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
                Debug.LogWarning("�ﵽ����Դ������޷�����ײ�����ҵ���Чλ��");
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
        // ʹ��OverlapPoint����������ײ��⣬֧������2D��ײ������
        return _collider.OverlapPoint(point);
    }
}