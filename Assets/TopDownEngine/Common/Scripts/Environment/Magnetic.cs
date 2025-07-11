﻿using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
	public class Magnetic : TopDownMonoBehaviour
	{
		/// the possible update modes
		public enum UpdateModes { Update, FixedUpdate, LateUpdate }

        [Header("磁力")]

        /// the layermask this magnetic element is attracted to
        [MMLabel("目标层级")]
        [Tooltip("磁力元素会吸引的目标层级遮罩")]
        public LayerMask TargetLayerMask = LayerManager.PlayerLayerMask;

        /// whether or not to start moving when something on the target layer mask enters this magnetic element's trigger
        [MMLabel("进入时启动")]
        [Tooltip("当目标层级对象进入磁力触发器时是否开始移动")]
        public bool StartMagnetOnEnter = true;

        /// whether or not to stop moving when something on the target layer mask exits this magnetic element's trigger
        [MMLabel("退出时停止")]
        [Tooltip("当目标层级对象离开磁力触发器时是否停止移动")]
        public bool StopMagnetOnExit = false;

        /// a unique ID for this type of magnetic objects. This can then be used by a MagneticEnabler to target only that specific ID. An ID of 0 will be picked by all MagneticEnablers automatically.
        [MMLabel("磁力类型ID")]
        [Tooltip("磁力对象唯一标识（ID为0时会被所有磁力启动器选中）")]
        public int MagneticTypeID = 0;

        [Header("Target")]

        /// the offset to apply to the followed target
        [MMLabel("目标偏移")]
        [Tooltip("应用到跟随目标的偏移量")]
        public Vector3 Offset;

        [Header("Position Interpolation")]

        /// whether or not we need to interpolate the movement
        [MMLabel("启用位置插值")]
        [Tooltip("是否对移动进行插值平滑处理")]
        public bool InterpolatePosition = true;

        /// the speed at which to interpolate the follower's movement
        [MMCondition("InterpolatePosition", true)]
        [MMLabel("跟随速度")]
        [Tooltip("跟随目标移动的插值速度")]
        public float FollowPositionSpeed = 5f;

        /// the acceleration to apply to the object once it starts following
        [MMCondition("InterpolatePosition", true)]
        [MMLabel("跟随加速度")]
        [Tooltip("开始跟随时应用的加速度值")]
        public float FollowAcceleration = 0.75f;

        [Header("Mode")]

        /// the update at which the movement happens
        [MMLabel("更新模式")]
        [Tooltip("移动逻辑的更新阶段（Update/FixedUpdate/LateUpdate）")]
        public UpdateModes UpdateMode = UpdateModes.Update;

        [Header("State")]

        /// an object this magnetic object should copy the active state on
        [MMLabel("状态复制对象")]
        [Tooltip("需要同步激活状态的关联对象")]
        public GameObject CopyState;

        [Header("Debug")]
        /// the target to follow, read only, for debug only
        [MMLabel("跟随目标")]
        [Tooltip("当前跟随目标（调试只读）")]
        [MMReadOnly]
        public Transform Target;

        /// whether or not the object is currently following its target's position
        [MMLabel("跟随位置中")]
        [Tooltip("是否正在跟随目标位置")]
        [MMReadOnly]
        public bool FollowPosition = true;

        protected Collider2D _collider2D;
		protected Collider _collider;
		protected Vector3 _velocity = Vector3.zero;
		protected Vector3 _newTargetPosition;
		protected Vector3 _lastTargetPosition;
		protected Vector3 _direction;
		protected Vector3 _newPosition;
		protected float _speed;
		protected Vector3 _initialPosition;

		/// <summary>
		/// On Awake we initialize our magnet
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}
		protected virtual void OnEnable()
		{
			Reset();
		}

		/// <summary>
		/// Grabs the collider and ensures it's set as trigger, initializes the speed
		/// </summary>
		protected virtual void Initialization()
		{
			_initialPosition = this.transform.position;
			
			_collider2D = this.gameObject.GetComponent<Collider2D>();
			if (_collider2D != null) { _collider2D.isTrigger = true; }
			
			_collider = this.gameObject.GetComponent<Collider>();
			if (_collider != null) { _collider.isTrigger = true; }
			
			Reset();
		}
		
		/// <summary>
		/// Call this to reset the target and speed of this magnetic object
		/// </summary>
		public virtual void Reset()
		{
			Target = null;
			_speed = 0f;
		}

		/// <summary>
		/// Call this to reset the position of the magnetic to its initial position
		/// </summary>
		public virtual void ResetPosition()
		{
			this.transform.position = _initialPosition;
			Reset();
		}

		/// <summary>
		/// When something enters our trigger, if it's a proper target, we start following it
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void OnTriggerEnter2D(Collider2D colliding)
		{
			OnTriggerEnterInternal(colliding.gameObject);
		}

		/// <summary>
		/// When something exits our trigger, we stop following it
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void OnTriggerExit2D(Collider2D colliding)
		{
			OnTriggerExitInternal(colliding.gameObject);
		}

		/// <summary>
		/// When something enters our trigger, if it's a proper target, we start following it
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void OnTriggerEnter(Collider colliding)
		{
			OnTriggerEnterInternal(colliding.gameObject);
		}

		/// <summary>
		/// When something exits our trigger, we stop following it
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void OnTriggerExit(Collider colliding)
		{
			OnTriggerExitInternal(colliding.gameObject);
		}

		/// <summary>
		/// Starts following an object we trigger with if conditions are met
		/// </summary>
		/// <param name="colliding"></param>
		protected virtual void OnTriggerEnterInternal(GameObject colliding)
		{
			if (!StartMagnetOnEnter)
			{
				return;
			}

			if (!TargetLayerMask.MMContains(colliding.layer))
			{
				return;
			}

			Target = colliding.transform;
			StartFollowing();
		}

		/// <summary>
		/// Stops following an object we trigger with if conditions are met
		/// </summary>
		/// <param name="colliding"></param>
		protected virtual void OnTriggerExitInternal(GameObject colliding)
		{
			if (!StopMagnetOnExit)
			{
				return;
			}

			if (!TargetLayerMask.MMContains(colliding.layer))
			{
				return;
			}

			StopFollowing();
		}
        
		/// <summary>
		/// At update we follow our target 
		/// </summary>
		protected virtual void Update()
		{
			if (CopyState != null)
			{
				this.gameObject.SetActive(CopyState.activeInHierarchy);
			}            

			if (Target == null)
			{
				return;
			}
			if (UpdateMode == UpdateModes.Update)
			{
				FollowTargetPosition();
			}
		}

		/// <summary>
		/// At fixed update we follow our target 
		/// </summary>
		protected virtual void FixedUpdate()
		{
			if (UpdateMode == UpdateModes.FixedUpdate)
			{
				FollowTargetPosition();
			}
		}

		/// <summary>
		/// At late update we follow our target 
		/// </summary>
		protected virtual void LateUpdate()
		{
			if (UpdateMode == UpdateModes.LateUpdate)
			{
				FollowTargetPosition();
			}
		}
        
		/// <summary>
		/// Follows the target, lerping the position or not based on what's been defined in the inspector
		/// </summary>
		protected virtual void FollowTargetPosition()
		{
			if (Target == null)
			{
				return;
			}

			if (!FollowPosition)
			{
				return;
			}

			_newTargetPosition = Target.position + Offset;

			float trueDistance = 0f;
			_direction = (_newTargetPosition - this.transform.position).normalized;
			trueDistance = Vector3.Distance(this.transform.position, _newTargetPosition);

			_speed = (_speed < FollowPositionSpeed) ? _speed + FollowAcceleration * Time.deltaTime : FollowPositionSpeed;

			float interpolatedDistance = trueDistance;
			if (InterpolatePosition)
			{
				interpolatedDistance = MMMaths.Lerp(0f, trueDistance, _speed, Time.deltaTime);
				this.transform.Translate(_direction * interpolatedDistance, Space.World);
			}
			else
			{
				this.transform.Translate(_direction * interpolatedDistance, Space.World);
			}
		}

		/// <summary>
		/// Prevents the object from following the target anymore
		/// </summary>
		public virtual void StopFollowing()
		{
			FollowPosition = false;
		}

		/// <summary>
		/// Makes the object follow the target
		/// </summary>
		public virtual void StartFollowing()
		{
			FollowPosition = true;
		}

		/// <summary>
		/// Sets a new target for this object to magnet towards
		/// </summary>
		/// <param name="newTarget"></param>
		public virtual void SetTarget(Transform newTarget)
		{
			Target = newTarget;
		}
	}
}