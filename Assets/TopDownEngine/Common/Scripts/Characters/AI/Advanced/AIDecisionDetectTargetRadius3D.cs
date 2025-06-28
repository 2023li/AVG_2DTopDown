using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// This decision will return true if an object on its TargetLayer layermask is within its specified radius, false otherwise. It will also set the Brain's Target to that object.
	/// </summary>
	[AddComponentMenu("TopDown Engine/Character/AI/Decisions/AI Decision Detect Target Radius 3D")]
	//[RequireComponent(typeof(Character))]
	public class AIDecisionDetectTargetRadius3D : AIDecision
	{
        /// the radius to search our target in
        [MMLabel("搜索半径")]
        [Tooltip("用于搜索目标的半径范围")]
        public float Radius = 3f;
        /// the offset to apply (from the collider's center)
        [MMLabel("检测原点偏移")]
        [Tooltip("从碰撞体中心位置应用的偏移量")]
        public Vector3 DetectionOriginOffset = new Vector3(0, 0, 0);

        /// the layer(s) to search our target on
        [MMLabel("目标图层")]
        [Tooltip("用于搜索目标的图层")]
        public LayerMask TargetLayerMask;

        /// the layer(s) to block the sight
        [MMLabel("视线遮挡图层")]
        [Tooltip("阻挡视线的图层")]
        public LayerMask ObstacleMask = LayerManager.ObstaclesLayerMask;

        /// the frequency (in seconds) at which to check for obstacles
        [MMLabel("每秒检测频率")]
        [Tooltip("检测障碍物的时间间隔(秒)")]
        public float TargetCheckFrequency = 1f;

        /// if this is true, this AI will be able to consider itself (or its children) a target
        [MMLabel("允许自我 targeting")]
        [Tooltip("是否允许AI将自身或子对象视为目标")]
        public bool CanTargetSelf = false;


        /// the maximum amount of targets the overlap detection can acquire
        [MMLabel("最大重叠目标数")]
        [Tooltip("重叠检测可获取的最大目标数量")]
        public int OverlapMaximum = 10;

        protected Collider _collider;
		protected Vector3 _raycastOrigin;
		protected Character _character;
		protected Color _gizmoColor = Color.yellow;
		protected bool _init = false;
		protected Vector3 _raycastDirection;
		protected Collider[] _hits;
		protected float _lastTargetCheckTimestamp = 0f;
		protected bool _lastReturnValue = false;
		protected List<Transform> _potentialTargets;

		/// <summary>
		/// On init we grab our Character component
		/// </summary>
		public override void Initialization()
		{
			_lastTargetCheckTimestamp = 0f;
			_potentialTargets = new List<Transform>();
			_character = this.gameObject.GetComponentInParent<Character>();
			_collider = this.gameObject.GetComponentInParent<Collider>();
			_gizmoColor.a = 0.25f;
			_init = true;
			_lastReturnValue = false;
			_hits = new Collider[OverlapMaximum];
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
			_potentialTargets.Clear();

			_lastTargetCheckTimestamp = Time.time;
			_raycastOrigin = _collider.bounds.center + DetectionOriginOffset / 2;
			int numberOfCollidersFound = Physics.OverlapSphereNonAlloc(_raycastOrigin, Radius, _hits, TargetLayerMask);

			// if there are no targets around, we exit
			if (numberOfCollidersFound == 0)
			{
				_lastReturnValue = false;
				return false;
			}
            
			// we go through each collider found
			int min = Mathf.Min(OverlapMaximum, numberOfCollidersFound);
			for (int i = 0; i < min; i++)
			{
				if (_hits[i] == null)
				{
					continue;
				}
                
				if (!CanTargetSelf)
				{
					if ((_hits[i].gameObject == _brain.Owner) || (_hits[i].transform.IsChildOf(this.transform)))
					{
						continue;
					}    
				}
                
				_potentialTargets.Add(_hits[i].gameObject.transform);
			}
            
			// we sort our targets by distance
			_potentialTargets.Sort(delegate(Transform a, Transform b)
			{return Vector3.Distance(this.transform.position,a.transform.position)
				.CompareTo(
					Vector3.Distance(this.transform.position,b.transform.position) );
			});
            
			// we return the first unobscured target
			foreach (Transform t in _potentialTargets)
			{
				_raycastDirection = t.position - _raycastOrigin;
				RaycastHit hit = MMDebug.Raycast3D(_raycastOrigin, _raycastDirection, _raycastDirection.magnitude, ObstacleMask.value, Color.yellow, true);
				if (hit.collider == null)
				{
					_brain.Target = t;
					_lastReturnValue = true;
					return true;
				}
			}

			_lastReturnValue = false;
			return false;
		}

		/// <summary>
		/// Draws gizmos for the detection circle
		/// </summary>
		protected virtual void OnDrawGizmosSelected()
		{
			_raycastOrigin = transform.position + DetectionOriginOffset / 2;

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