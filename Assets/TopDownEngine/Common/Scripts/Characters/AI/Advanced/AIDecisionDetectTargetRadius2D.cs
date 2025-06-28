using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// This decision will return true if an object on its TargetLayer layermask is within its specified radius, false otherwise. It will also set the Brain's Target to that object.
	/// </summary>
	[AddComponentMenu("TopDown Engine/Character/AI/Decisions/AI Decision Detect Target Radius 2D")]
	//[RequireComponent(typeof(CharacterOrientation2D))]
	public class AIDecisionDetectTargetRadius2D : AIDecision
	{
		public enum ObstaclesDetectionModes
		{
			Boxcast,
			Raycast
		}

        [MMLabel("搜索半径")]
        [Tooltip("用于搜索目标的半径范围")]
        public float Radius = 3f;

        [MMLabel("检测原点偏移")]
        [Tooltip("从碰撞体中心位置应用的偏移量")]
        public Vector3 DetectionOriginOffset = new Vector3(0, 0, 0);

        /// the layer(s) to search our target on
        [MMLabel("目标图层")]
        [Tooltip("用于搜索目标的图层")]
        public LayerMask TargetLayer;

		/// whether or not to look for obstacles
		[MMLabel("是否障碍检测")]
		[Tooltip("whether or not to look for obstacles")]
		public bool ObstacleDetection = true;

		/// the layer(s) to look for obstacles on
		[MMLabel("障碍物图层")]
		public LayerMask ObstacleMask = LayerManager.ObstaclesLayerMask;

		/// the method to use to detect obstacles
		[MMLabel("障碍检测模式")]
		public ObstaclesDetectionModes ObstaclesDetectionMode = ObstaclesDetectionModes.Raycast;

        [MMLabel("允许自我 targeting")]
        [Tooltip("是否允许AI将自身或子对象视为目标")]
        public bool CanTargetSelf = false;

        /// the frequency (in seconds) at which to check for obstacles
        [MMLabel("每秒检测频率")]
        [Tooltip("检测障碍物的时间间隔(秒)")]
        public float TargetCheckFrequency = 1f;

        /// the maximum amount of targets the overlap detection can acquire
        [MMLabel("最大重叠目标数")]
        [Tooltip("重叠检测可获取的最大目标数量")]
        public int OverlapMaximum = 10;

		protected Collider2D _collider;
		protected Vector2 _facingDirection;
		protected Vector2 _raycastOrigin;
		protected Character _character;
		protected CharacterOrientation2D _orientation2D;
		protected Color _gizmoColor = Color.yellow;
		protected bool _init = false;
		protected Vector2 _boxcastDirection;
		protected Collider2D[] _results;
		protected List<Transform> _potentialTargets;
		protected float _lastTargetCheckTimestamp = 0f;
		protected bool _lastReturnValue = false;
		protected RaycastHit2D _hit;

		/// <summary>
		/// On init we grab our Character component
		/// </summary>
		public override void Initialization()
		{
			_potentialTargets = new List<Transform>();
			_character = this.gameObject.GetComponentInParent<Character>();
			_orientation2D = _character?.FindAbility<CharacterOrientation2D>();
			_collider = this.gameObject.GetComponentInParent<Collider2D>();
			_gizmoColor.a = 0.25f;
			_init = true;
			_results = new Collider2D[OverlapMaximum];
		}

		/// <summary>
		/// On Decide we check for our target
		/// </summary>
		/// <returns></returns>
		public override bool Decide()
		{
			return DetectTarget();
		}

		/// <summary>
		/// Returns true if a target is found within the circle
		/// </summary>
		/// <returns></returns>
		protected virtual bool DetectTarget()
		{
			// we check if there's a need to detect a new target
			if (Time.time - _lastTargetCheckTimestamp < TargetCheckFrequency)
			{
				return _lastReturnValue;
			}

			_lastTargetCheckTimestamp = Time.time;

			ComputeRaycastOrigin();

			if (!GetPotentialTargets())
			{
				return false;
			}

			// we check if there's a target in the list
			if (_potentialTargets.Count == 0)
			{
				_lastReturnValue = false;
				return false;
			}

			SortTargetsByDistance();

			if (FindUnobscuredTarget())
			{
				return true;
			}

			_lastReturnValue = false;
			return false;
		}

		protected virtual bool FindUnobscuredTarget()
		{
			if (!ObstacleDetection && _potentialTargets[0] != null)
			{
				_brain.Target = _potentialTargets[0].gameObject.transform;
				_lastReturnValue = true;
				return true;
			}

			// we return the first unobscured target
			foreach (Transform t in _potentialTargets)
			{
				_boxcastDirection = (Vector2)(t.gameObject.MMGetComponentNoAlloc<Collider2D>().bounds.center -
				                              _collider.bounds.center);

				if (ObstaclesDetectionMode == ObstaclesDetectionModes.Boxcast)
				{
					_hit = Physics2D.BoxCast(_collider.bounds.center, _collider.bounds.size, 0f,
						_boxcastDirection.normalized, _boxcastDirection.magnitude, ObstacleMask);
				}
				else
				{
					_hit = MMDebug.RayCast(_collider.bounds.center, _boxcastDirection, _boxcastDirection.magnitude,
						ObstacleMask, Color.yellow, true);
				}

				if (!_hit)
				{
					_brain.Target = t;
					_lastReturnValue = true;
					return true;
				}
			}

			return false;
		}

		protected virtual void SortTargetsByDistance()
		{
			_potentialTargets.Sort(delegate(Transform a, Transform b)
			{
				if (a == null || b == null)
				{
					return 0;
				}

				return Vector2.Distance(this.transform.position, a.transform.position)
					.CompareTo(
						Vector2.Distance(this.transform.position, b.transform.position));
			});
		}

		protected virtual void ComputeRaycastOrigin()
		{
			if (_orientation2D != null)
			{
				_facingDirection = _orientation2D.IsFacingRight ? Vector2.right : Vector2.left;
				_raycastOrigin.x = transform.position.x + _facingDirection.x * DetectionOriginOffset.x / 2;
				_raycastOrigin.y = transform.position.y + DetectionOriginOffset.y;
			}
			else
			{
				_raycastOrigin = transform.position + DetectionOriginOffset;
			}
		}

		protected virtual bool GetPotentialTargets()
		{
			int numberOfResults = Physics2D.OverlapCircleNonAlloc(_raycastOrigin, Radius, _results, TargetLayer);
			// if there are no targets around, we exit
			if (numberOfResults == 0)
			{
				_lastReturnValue = false;
				return false;
			}

			// we go through each collider found
			_potentialTargets.Clear();
			int min = Mathf.Min(OverlapMaximum, numberOfResults);
			for (int i = 0; i < min; i++)
			{
				if (ColliderIsAPotentialTarget(_results[i]))
				{
					_potentialTargets.Add(_results[i].gameObject.transform);
				}
			}

			return true;
		}

		protected virtual bool ColliderIsAPotentialTarget(Collider2D collider2D)
		{
			if (collider2D == null)
			{
				return false;
			}

			if (!CanTargetSelf)
			{
				if ((collider2D.gameObject == _brain.Owner) || (collider2D.transform.IsChildOf(this.transform)))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Draws gizmos for the detection circle
		/// </summary>
		protected virtual void OnDrawGizmosSelected()
		{
			_raycastOrigin.x = transform.position.x + _facingDirection.x * DetectionOriginOffset.x / 2;
			_raycastOrigin.y = transform.position.y + DetectionOriginOffset.y;

			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(_raycastOrigin, Radius);
			if (_init)
			{
				Gizmos.color = _gizmoColor;
				Gizmos.DrawSphere(_raycastOrigin, Radius);
			}
		}
	}
}