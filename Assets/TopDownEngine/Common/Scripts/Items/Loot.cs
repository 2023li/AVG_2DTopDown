using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using Random = UnityEngine.Random;



namespace  MoreMountains.TopDownEngine
{
    /// <summary>
    /// A class meant to spawn objects (usually item pickers, but not necessarily)
    /// The spawn can be triggered by any script, at any time, and comes with automatic hooks
    /// to trigger loot on damage or death
	/// 
    /// 一个用于生成对象的类（通常是物品拾取器，但不一定）
    /// 生成操作可以由任何脚本在任何时间触发，并且自带自动挂钩
    /// 以便在受到伤害或死亡时触发掉落物
    /// </summary>
    public class Loot : TopDownMonoBehaviour
	{
		/// the possible modes by which loot can be defined 
		public enum LootModes { Unique, LootTable, LootTableScriptableObject }

        [Header("战利品模式")]
        /// the selected loot mode :
        /// - unique : 单一对象
        /// - loot table : 该对象专用的战利品表
        /// - loot definition : 通过右键创建的ScriptableObject战利品表（可在不同Loot对象间复用）
        /// This loot definition can then be reused in other Loot objects.
        [MMLabel("LootModes")]
        [Tooltip("战利品模式：- 唯一：单一对象 - 独立战利品表：该对象专用的战利品表 - 可复用战利品定义：通过右键创建的ScriptableObject战利品表（可在不同Loot对象间复用）")]
        public LootModes LootMode = LootModes.Unique;

        /// the object to loot, when in LootMode
        [MMLabel("目标战利品对象")]
        [Tooltip("LootMode模式下要掠夺的目标对象")]
        //[MMEnumCondition("LootMode", (int) LootModes.Unique)]
        [MMEnumCondition("LootMode", (int)LootModes.Unique)]
        public GameObject GameObjectToLoot;

        /// a loot table defining what objects to spawn
        [MMLabel("独立战利品表")]
        [Tooltip("定义生成对象的独立战利品表")]
        [MMEnumCondition("LootMode", (int)LootModes.LootTable)]
        //[ShowIf("LootMode", LootModes.LootTable)]
        public MMLootTableGameObject LootTable;

        /// a loot table scriptable object defining what objects to spawn
        [MMLabel("可复用战利品表")]
        [Tooltip("定义生成对象的可复用ScriptableObject战利品表")]
        [MMEnumCondition("LootMode", (int)LootModes.LootTableScriptableObject)]
        //[ShowIf("LootMode", LootModes.LootTableScriptableObject)]
        public MMLootTableGameObjectSO LootTableSO;

        [Header("生成条件")]

        /// if this is true, loot will happen when this object dies
        [MMLabel("死亡时生成")]
        [Tooltip("如果启用，该对象死亡时会生成战利品")]
        public bool SpawnLootOnDeath = true;

        /// if this is true, loot will happen when this object takes damage
        [MMLabel("受伤时生成")]
        [Tooltip("如果启用，该对象受伤时会生成战利品")]
        public bool SpawnLootOnDamage = false;

        [Header("Pooling")]

        /// if this is true, lootables will be pooled
        [MMLabel("启用对象池")]
        [Tooltip("如果启用，战利品将使用对象池管理")]
        public bool PoolLoot = false;

        /// determines the size of the pool for each object in the loot table
        [MMLabel("对象池大小")]
        [Tooltip("战利品表中每个对象的对象池容量")]
        [MMCondition("PoolLoot", true)]
        public int PoolSize = 20;

        /// a unique name for this pool, has to be common between all Loot objects sharing the same loot table if you want to mutualize their pools
        [MMLabel("共享池名称")]
        [Tooltip("共享对象池的唯一名称（需在相同战利品表的Loot对象间保持一致）")]
        [MMCondition("PoolLoot", true)]
        //[ShowIf("LootMode",true)]
        public string MutualizedPoolName = "";

        [Header("Spawn")]

        /// if this is false, spawn won't happen
        [MMLabel("允许生成")]
        [Tooltip("如果禁用，将不会生成任何战利品")]
        public bool CanSpawn = true;

        /// a delay (in seconds) to wait for before spawning loot
        [MMLabel("生成延迟")]
        [Tooltip("生成战利品前的等待时间（秒）")]
        public float Delay = 0f;

        /// the minimum and maximum quantity of objects to spawn 
        [MMLabel("生成数量范围")]
        [Tooltip("生成对象的最小和最大数量范围")]
        [MMVector("Min", "Max")]
        public Vector2 Quantity = Vector2.one;


		/// the position, rotation and scale objects should spawn at
		//[MMLabel("生成属性配置")]
		[Header("详细生成属性")]
        [Tooltip("生成对象的位置、旋转和缩放属性")]
        public MMSpawnAroundProperties SpawnProperties;

        /// if this is true, loot will be limited to MaximumQuantity, any new loot attempt beyond that will have no outcome. If this is false, loot is unlimited and can happen forever.
        [MMLabel("限制战利品数量")]
        [Tooltip("如果启用，战利品生成将被限制在最大数量内，超出后不再生成")]
        public bool LimitedLootQuantity = true;

        /// The maximum quantity of objects that can be looted from this Loot object
        [MMLabel("最大生成数量")]
        [Tooltip("可从该对象生成战利品的最大数量")]
        [MMCondition("LimitedLootQuantity", true)]
        public int MaximumQuantity = 100;

        /// The remaining quantity of objects that can be looted from this Loot object, displayed for debug purposes 
        [MMLabel("剩余可生成数量")]
        [Tooltip("当前剩余可生成的战利品数量（调试用）")]
        [MMReadOnly]
        public int RemainingQuantity = 100;

        [Header("Collisions")]

        /// Whether or not spawned objects should try and avoid obstacles 
        [MMLabel("避障功能")]
        [Tooltip("生成对象是否尝试避开障碍物")]
        public bool AvoidObstacles = false;
        
        /// the possible modes collision detection can operate on
        public enum DimensionModes { TwoD, ThreeD }

        /// whether collision detection should happen in 2D or 3D
        [MMLabel("碰撞维度模式")]
        [Tooltip("碰撞检测使用2D还是3D模式")]
        [MMCondition("AvoidObstacles", true)]
        public DimensionModes DimensionMode = DimensionModes.TwoD;

        /// the layer mask containing layers the spawned objects shouldn't collide with 
        [MMLabel("避障层遮罩")]
        [Tooltip("需要避开的障碍物层级")]
        [MMCondition("AvoidObstacles", true)]
        public LayerMask AvoidObstaclesLayerMask = LayerManager.ObstaclesLayerMask;

        /// the radius around the object within which no obstacle should be found
        [MMLabel("避障检测半径")]
        [Tooltip("障碍物检测的周围半径范围")]
        [MMCondition("AvoidObstacles", true)]
        public float AvoidRadius = 0.25f;

        /// the amount of times the script should try finding another position for the loot if the last one was within an obstacle. More attempts : better results, higher cost
        [MMLabel("最大避障尝试次数")]
        [Tooltip("当位置有障碍物时尝试重新寻找位置的次数（次数越多效果越好，性能消耗越大）")]
        [MMCondition("AvoidObstacles", true)]
        public int MaxAvoidAttempts = 5;

        [Header("生成反馈")]

        /// A MMFeedbacks to play when spawning loot. Only one feedback will play. If you want one per item, it's best to place it on the item itself, and have it play when the object gets instantiated. 
        
        [Tooltip("生成战利品时播放的全局反馈（每个生成批次只播放一次）")]
        public MMFeedbacks LootFeedback;

        [Header("Debug")]

        /// if this is true, gizmos will be drawn to show the shape within which loot will spawn
        [MMLabel("显示生成区域")]
        [Tooltip("是否绘制战利品生成区域的调试图示")]
        public bool DrawGizmos = false;

        /// the amount of gizmos to draw
        [MMLabel("调试点数量")]
        [Tooltip("要绘制的调试点数量")]
        public int GizmosQuantity = 1000;

        /// the color the gizmos should be drawn with
        [MMLabel("调试颜色")]
        [Tooltip("调试图示的显示颜色")]
        public Color GizmosColor = MMColors.LightGray;

        /// the size at which to draw the gizmos
        [MMLabel("调试点尺寸")]
        [Tooltip("调试点的绘制尺寸")]
        public float GimosSize = 1f;

        /// a debug button used to trigger a loot
        [MMLabel("测试生成按钮")]
        [Tooltip("调试用战利品生成按钮")]
        [MMInspectorButton("SpawnLootDebug")]
        public bool SpawnLootButton;

        public static List<MMSimpleObjectPooler> SimplePoolers = new List<MMSimpleObjectPooler>();
		public static List<MMMultipleObjectPooler> MultiplePoolers = new List<MMMultipleObjectPooler>();
		
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		protected static void InitializeStatics()
		{
			SimplePoolers = new List<MMSimpleObjectPooler>();
			MultiplePoolers = new List<MMMultipleObjectPooler>();
		}

		protected Health _health;
		protected GameObject _objectToSpawn;
		protected GameObject _spawnedObject;
		protected Vector3 _raycastOrigin;
		protected RaycastHit2D _raycastHit2D;
		protected Collider[] _overlapBox;
		protected MMSimpleObjectPooler _simplePooler;
		protected MMMultipleObjectPooler _multipleObjectPooler;
        
		/// <summary>
		/// On Awake we grab the health component if there's one, and initialize our loot table
		/// </summary>
		protected virtual void Awake()
		{
			_health = this.gameObject.GetComponentInParent<Health>();
			if (_health == null)
			{
				_health = this.gameObject.GetComponentInChildren<Health>();
			}
			InitializeLootTable();
			InitializePools();
			ResetRemainingQuantity();
		}

		/// <summary>
		/// Resets the remaining quantity to the maximum quantity
		/// </summary>
		public virtual void ResetRemainingQuantity()
		{
			RemainingQuantity = MaximumQuantity;
		}

		/// <summary>
		/// Computes the associated loot table's weights
		/// </summary>
		public virtual void InitializeLootTable()
		{
			switch (LootMode)
			{
				case LootModes.LootTableScriptableObject:
					if (LootTableSO != null)
					{
						LootTableSO.ComputeWeights();
					}
					break;
				case LootModes.LootTable:
					LootTable.ComputeWeights();
					break;
			}
		}

		protected virtual void InitializePools()
		{
			if (!PoolLoot)
			{
				return;
			}

			switch (LootMode)
			{
				case LootModes.Unique:
					_simplePooler = FindSimplePooler();
					break;
				case LootModes.LootTable:
					_multipleObjectPooler = FindMultiplePooler();
					break;
				case LootModes.LootTableScriptableObject:
					_multipleObjectPooler = FindMultiplePooler();
					break;
			}
		}

		protected virtual MMSimpleObjectPooler FindSimplePooler()
		{
			foreach (MMSimpleObjectPooler simplePooler in SimplePoolers)
			{
				if (simplePooler.GameObjectToPool == GameObjectToLoot)
				{
					return simplePooler;
				}
			}
			// if we haven't found one, we create one
			GameObject newObject = new GameObject("[MMSimpleObjectPooler] "+GameObjectToLoot.name);
			MMSimpleObjectPooler pooler = newObject.AddComponent<MMSimpleObjectPooler>();
			pooler.GameObjectToPool = GameObjectToLoot;
			pooler.PoolSize = PoolSize;
			pooler.NestUnderThis = true;
			pooler.FillObjectPool();            
			pooler.Owner = SimplePoolers;
			SimplePoolers.Add(pooler);
			return pooler;
		}
        
		protected virtual MMMultipleObjectPooler FindMultiplePooler()
		{
			foreach (MMMultipleObjectPooler multiplePooler in MultiplePoolers)
			{
				if ((multiplePooler != null) && (multiplePooler.MutualizedPoolName == MutualizedPoolName)) 
				{
					return multiplePooler;
				}
			}
			// if we haven't found one, we create one
			GameObject newObject = new GameObject("[MMMultipleObjectPooler] "+MutualizedPoolName);
			MMMultipleObjectPooler pooler = newObject.AddComponent<MMMultipleObjectPooler>();
			pooler.MutualizeWaitingPools = true;
			pooler.MutualizedPoolName = MutualizedPoolName;
			pooler.NestUnderThis = true;
			pooler.Pool = new List<MMMultipleObjectPoolerObject>();
			if (LootMode == LootModes.LootTable)
			{
				foreach (MMLootGameObject loot in LootTable.ObjectsToLoot)
				{
					MMMultipleObjectPoolerObject objectToPool = new MMMultipleObjectPoolerObject();
					objectToPool.PoolSize = PoolSize * (int)loot.Weight;
					objectToPool.GameObjectToPool = loot.Loot;
					pooler.Pool.Add(objectToPool);
				}
			}
			else if (LootMode == LootModes.LootTableScriptableObject)
			{
				foreach (MMLootGameObject loot in LootTableSO.LootTable.ObjectsToLoot)
				{
					MMMultipleObjectPoolerObject objectToPool = new MMMultipleObjectPoolerObject
					{
						PoolSize = PoolSize * (int)loot.Weight,
						GameObjectToPool = loot.Loot
					};
					pooler.Pool.Add(objectToPool);
				}
			}
			pooler.FillObjectPool();
			pooler.Owner = MultiplePoolers;
			MultiplePoolers.Add(pooler);
			return pooler;
		}

		/// <summary>
		/// This method spawns the specified loot after applying a delay (if there's one)
		/// </summary>
		public virtual void SpawnLoot()
		{
			if (!CanSpawn)
			{
				return;
			}
			StartCoroutine(SpawnLootCo());
		}

		/// <summary>
		/// A debug method called by the inspector button
		/// </summary>
		protected virtual void SpawnLootDebug()
		{
			if (!Application.isPlaying)
			{
				Debug.LogWarning("This debug button is only meant to be used while in Play Mode.");
				return;
			}

			SpawnLoot();
		}

		/// <summary>
		/// A coroutine used to spawn loot after a delay
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator SpawnLootCo()
		{
			yield return MMCoroutine.WaitFor(Delay);
			int randomQuantity = Random.Range((int)Quantity.x, (int)Quantity.y + 1);
			for (int i = 0; i < randomQuantity; i++)
			{
				SpawnOneLoot();
			}
			LootFeedback?.PlayFeedbacks();
		}

		protected virtual void Spawn(GameObject gameObjectToSpawn)
		{
			if (PoolLoot)
			{
				switch (LootMode)
				{
					case LootModes.Unique:
						_spawnedObject = _simplePooler.GetPooledGameObject();
						break;
					case LootModes.LootTable: case LootModes.LootTableScriptableObject:
						_spawnedObject = _multipleObjectPooler.GetPooledGameObject();
						break;
				}
			}
			else
			{
				_spawnedObject = Instantiate(gameObjectToSpawn);    
			}
		}

		/// <summary>
		/// Spawns a single loot object, without delay, and regardless of the defined quantities 
		/// </summary>
		public virtual void SpawnOneLoot()
		{
			_objectToSpawn = GetObject();

			if (_objectToSpawn == null)
			{
				return;
			}

			if (LimitedLootQuantity && (RemainingQuantity <= 0))
			{
				return;
			}

			Spawn(_objectToSpawn);

			if (AvoidObstacles)
			{
				bool placementOK = false;
				int amountOfAttempts = 0;
				while (!placementOK && (amountOfAttempts < MaxAvoidAttempts))
				{
					MMSpawnAround.ApplySpawnAroundProperties(_spawnedObject, SpawnProperties, this.transform.position);
                    
					if (DimensionMode == DimensionModes.TwoD)
					{
						_raycastOrigin = _spawnedObject.transform.position;
						_raycastHit2D = Physics2D.BoxCast(_raycastOrigin + Vector3.right * AvoidRadius, AvoidRadius * Vector2.one, 0f, Vector2.left, AvoidRadius, AvoidObstaclesLayerMask);
						if (_raycastHit2D.collider == null)
						{
							placementOK = true;
						}
						else
						{
							amountOfAttempts++;
						}
					}
					else
					{
						_raycastOrigin = _spawnedObject.transform.position;
						_overlapBox = Physics.OverlapBox(_raycastOrigin, Vector3.one * AvoidRadius, Quaternion.identity, AvoidObstaclesLayerMask);
                        
						if (_overlapBox.Length == 0)
						{
							placementOK = true;
						}
						else
						{
							amountOfAttempts++;
						}
					}
				}
			}
			else
			{
				MMSpawnAround.ApplySpawnAroundProperties(_spawnedObject, SpawnProperties, this.transform.position);    
			}
			if (_spawnedObject != null)
			{
				_spawnedObject.gameObject.SetActive(true);
			}
			_spawnedObject.SendMessage("OnInstantiate", SendMessageOptions.DontRequireReceiver);

			if (LimitedLootQuantity)
			{
				RemainingQuantity--;	
			}
		}

		/// <summary>
		/// Gets the object that should be spawned
		/// </summary>
		/// <returns></returns>
		protected virtual GameObject GetObject()
		{
			_objectToSpawn = null;
			switch (LootMode)
			{
				case LootModes.Unique:
					_objectToSpawn = GameObjectToLoot;
					break;
				case LootModes.LootTableScriptableObject:
					if (LootTableSO == null)
					{
						_objectToSpawn = null;
						break;
					}
					_objectToSpawn = LootTableSO.GetLoot();
					break;
				case LootModes.LootTable:
					_objectToSpawn = LootTable.GetLoot()?.Loot;
					break;
			}

			return _objectToSpawn;
		}

		/// <summary>
		/// On hit, we spawn loot if needed
		/// </summary>
		protected virtual void OnHit()
		{
			if (!SpawnLootOnDamage)
			{
				return;
			}

			SpawnLoot();
		}
        
		/// <summary>
		/// On death, we spawn loot if needed
		/// </summary>
		protected virtual void OnDeath()
		{
			if (!SpawnLootOnDeath)
			{
				return;
			}

			SpawnLoot();
		}
        
		/// <summary>
		/// OnEnable we start listening for death and hit if needed
		/// </summary>
		protected virtual void OnEnable()
		{
			if (_health != null)
			{
				_health.OnDeath += OnDeath;
				_health.OnHit += OnHit;
			}
		}

		/// <summary>
		/// OnDisable we stop listening for death and hit if needed
		/// </summary>
		protected virtual void OnDisable()
		{
			if (_health != null)
			{
				_health.OnDeath -= OnDeath;
				_health.OnHit -= OnHit;
			}
		}
        
		/// <summary>
		/// OnDrawGizmos, we display the shape at which objects will spawn when looted
		/// </summary>
		protected virtual void OnDrawGizmos()
		{
			if (DrawGizmos)
			{
				MMSpawnAround.DrawGizmos(SpawnProperties, this.transform.position, GizmosQuantity, GimosSize, GizmosColor);    
			}
		}

	}
}