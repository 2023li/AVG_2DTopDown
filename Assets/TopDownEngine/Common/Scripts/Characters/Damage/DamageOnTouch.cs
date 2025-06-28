using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using System;
using MoreMountains.Feedbacks;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// Add this component to an object and it will cause damage to objects that collide with it. 
	/// </summary>
	[AddComponentMenu("TopDown Engine/Character/Damage/Damage On Touch")]
	public class DamageOnTouch : MMMonoBehaviour
	{
		[Flags]
		public enum TriggerAndCollisionMask
		{
			IgnoreAll = 0,
			OnTriggerEnter = 1 << 0,
			OnTriggerStay = 1 << 1,
			OnTriggerEnter2D = 1 << 6,
			OnTriggerStay2D = 1 << 7,

			All_3D = OnTriggerEnter | OnTriggerStay,
			All_2D = OnTriggerEnter2D | OnTriggerStay2D,
			All = All_3D | All_2D
		}

		/// the possible ways to add knockback : noKnockback, which won't do nothing, set force, or add force
		public enum KnockbackStyles
		{
			NoKnockback,
			AddForce
		}

		/// the possible knockback directions
		public enum KnockbackDirections
		{
			BasedOnOwnerPosition,
			BasedOnSpeed,
			BasedOnDirection,
			BasedOnScriptDirection
		}

		/// the possible ways to determine damage directions
		public enum DamageDirections
		{
			BasedOnOwnerPosition,
			BasedOnVelocity,
			BasedOnScriptDirection
		}

		public const TriggerAndCollisionMask AllowedTriggerCallbacks = TriggerAndCollisionMask.OnTriggerEnter
		                                                                  | TriggerAndCollisionMask.OnTriggerStay
		                                                                  | TriggerAndCollisionMask.OnTriggerEnter2D
		                                                                  | TriggerAndCollisionMask.OnTriggerStay2D;

        [MMInspectorGroup("Targets", true, 3)]
        [MMInformation("该组件将使对象对碰撞物造成伤害。可定义影响层级、伤害值、击退力及无敌时间等参数。", MMInformationAttribute.InformationType.Info, false)]

        /// the layers that will be damaged by this object
        [MMLabel("目标层级")]
        [Tooltip("会受到伤害的层级遮罩")]
        public LayerMask TargetLayerMask;

        /// the owner of the DamageOnTouch zone
        [MMReadOnly]
        [MMLabel("所有者")]
        [Tooltip("伤害区域的持有者对象")]
        public GameObject Owner;

        /// Defines on what triggers the damage should be applied...
        [MMLabel("触发过滤")]
        [Tooltip("伤害触发条件（默认进入与停留触发，可排除特定触发类型）")]
        public TriggerAndCollisionMask TriggerFilter = AllowedTriggerCallbacks;

        [MMInspectorGroup("Damage Caused", true, 8)]

        /// The min amount of health...
        [MMLabel("最小伤害")]
        [Tooltip("对目标造成的最小基础伤害值")]
        public float MinDamageCaused = 10f;

        /// The max amount of health...
        [MMLabel("最大伤害")]
        [Tooltip("对目标造成的最大基础伤害值")]
        public float MaxDamageCaused = 10f;

        /// a list of typed damage definitions...
        [MMLabel("类型伤害")]
        [Tooltip("在基础伤害上附加的特定类型伤害列表")]
        public List<TypedDamage> TypedDamages;

        /// how to determine the damage direction...
        [MMLabel("伤害方向模式")]
        [Tooltip("伤害方向计算方式（移动物体用速度方向，近战用持有者位置）")]
        public DamageDirections DamageDirectionMode = DamageDirections.BasedOnVelocity;

        [Header("Knockback")]

        /// the type of knockback...
        [MMLabel("击退类型")]
        [Tooltip("造成伤害时应用的击退方式")]
        public KnockbackStyles DamageCausedKnockbackType = KnockbackStyles.AddForce;

        /// The direction to apply the knockback 
        [MMLabel("击退方向")]
        [Tooltip("击退方向计算基准（通常基于持有者位置）")]
        public KnockbackDirections DamageCausedKnockbackDirection = KnockbackDirections.BasedOnOwnerPosition;

        /// The force to apply...
        [MMLabel("击退力度")]
        [Tooltip("击退力矢量（会根据方向模式自动旋转方向）")]
        public Vector3 DamageCausedKnockbackForce = new Vector3(10, 10, 10);

        [Header("无敌")]

        /// The duration of the invincibility...
        [MMLabel("无敌时间")]
        [Tooltip("命中后目标的无敌帧持续时间（秒）")]
        public float InvincibilityDuration = 0.5f;

        [Header("Damage over time")]

        /// Whether or not...
        [MMLabel("持续伤害")]
        [Tooltip("是否启用持续伤害模式")]
        public bool RepeatDamageOverTime = false;

        /// if in damage over time mode...
        [MMLabel("重复次数")]
        [Tooltip("持续伤害模式下的伤害触发次数")]
        [MMCondition("RepeatDamageOverTime", true)]
        public int AmountOfRepeats = 3;

        /// if in damage over time mode...
        [MMLabel("间隔时间")]
        [Tooltip("持续伤害的触发间隔时间（秒）")]
        [MMCondition("RepeatDamageOverTime", true)]
        public float DurationBetweenRepeats = 1f;

        /// if in damage over time mode...
        [MMLabel("可中断")]
        [Tooltip("持续伤害是否可通过Health组件中断")]
        [MMCondition("RepeatDamageOverTime", true)]
        public bool DamageOverTimeInterruptible = true;

        /// if in damage over time mode...
        [MMLabel("伤害类型")]
        [Tooltip("持续伤害的特定类型")]
        [MMCondition("RepeatDamageOverTime", true)]
        public DamageType RepeatedDamageType;

        [MMInspectorGroup("Damage Taken", true, 69)]
        [MMInformation("碰撞后可对自身造成伤害（需Health组件）。例如子弹命中后自毁。可定义不同情况下的自损伤害值。", MMInformationAttribute.InformationType.Info, false)]

        /// The Health component...
        [MMLabel("自损目标")]
        [Tooltip("用于接收自损的Health组件（空时自动获取）")]
        public Health DamageTakenHealth;

        /// The amount of damage taken...
        [MMLabel("每次伤害自损")]
        [Tooltip("每次碰撞时无论对象类型都会承受的伤害")]
        public float DamageTakenEveryTime = 0;

        /// The amount of damage taken...
        [MMLabel("可伤害对象自损")]
        [Tooltip("碰撞可伤害对象时自身承受的伤害")]
        public float DamageTakenDamageable = 0;

        /// The amount of damage taken...
        [MMLabel("非伤害对象自损")]
        [Tooltip("碰撞非伤害对象时自身承受的伤害")]
        public float DamageTakenNonDamageable = 0;

        /// the type of knockback...
        [MMLabel("自损击退类型")]
        [Tooltip("自身承受伤害时的击退方式")]
        public KnockbackStyles DamageTakenKnockbackType = KnockbackStyles.NoKnockback;

        /// The force to apply...
        [MMLabel("自损击退力度")]
        [Tooltip("自身承受伤害时的击退力矢量")]
        public Vector3 DamageTakenKnockbackForce = Vector3.zero;

        /// The duration of the invincibility...
        [MMLabel("自损无敌时间")]
        [Tooltip("自身受伤后的无敌帧持续时间（秒）")]
        public float DamageTakenInvincibilityDuration = 0.5f;

        [MMInspectorGroup("Feedbacks", true, 18)]

        /// the feedback to play...
        [MMLabel("可伤害反馈")]
        [Tooltip("命中可伤害对象时播放的反馈")]
        public MMFeedbacks HitDamageableFeedback;

        /// the feedback to play...
        [MMLabel("非伤害反馈")]
        [Tooltip("命中不可伤害对象时播放的反馈")]
        public MMFeedbacks HitNonDamageableFeedback;

        /// the feedback to play...
        [MMLabel("通用命中反馈")]
        [Tooltip("命中任意对象时播放的反馈")]
        public MMFeedbacks HitAnythingFeedback;

        /// an event to trigger when hitting a Damageable
        public UnityEvent<Health> HitDamageableEvent;
		/// an event to trigger when hitting a non Damageable
		public UnityEvent<GameObject> HitNonDamageableEvent;
		/// an event to trigger when hitting anything
		public UnityEvent<GameObject> HitAnythingEvent;

		// storage		
		protected Vector3 _lastPosition, _lastDamagePosition, _velocity, _knockbackForce, _damageDirection;
		protected float _startTime = 0f;
		protected Health _colliderHealth;
		protected TopDownController _topDownController;
		protected TopDownController _colliderTopDownController;
		protected List<GameObject> _ignoredGameObjects;
		protected Vector3 _knockbackForceApplied;
		protected CircleCollider2D _circleCollider2D;
		protected BoxCollider2D _boxCollider2D;
		protected SphereCollider _sphereCollider;
		protected BoxCollider _boxCollider;
		protected Color _gizmosColor;
		protected Vector3 _gizmoSize;
		protected Vector3 _gizmoOffset;
		protected Transform _gizmoTransform;
		protected bool _twoD = false;
		protected bool _initializedFeedbacks = false;
		protected Vector3 _positionLastFrame;
		protected Vector3 _knockbackScriptDirection;
		protected Vector3 _relativePosition;
		protected Vector3 _damageScriptDirection;
		protected Health _collidingHealth;

		#region Initialization
		
		/// <summary>
		/// On Awake we initialize our damage on touch area
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}

		/// <summary>
		/// OnEnable we set the start time to the current timestamp
		/// </summary>
		protected virtual void OnEnable()
		{
			_startTime = Time.time;
			_lastPosition = transform.position;
			_lastDamagePosition = transform.position;
		}

		/// <summary>
		/// Initializes ignore list, feedbacks, colliders and grabs components
		/// </summary>
		public virtual void Initialization()
		{
			InitializeIgnoreList();
			GrabComponents();
			InitalizeGizmos();
			InitializeColliders();
			InitializeFeedbacks();
		}

		/// <summary>
		/// Stores components
		/// </summary>
		protected virtual void GrabComponents()
		{
			if (DamageTakenHealth == null)
			{
				DamageTakenHealth = GetComponent<Health>();	
			}
			_topDownController = GetComponent<TopDownController>();
			_boxCollider = GetComponent<BoxCollider>();
			_sphereCollider = GetComponent<SphereCollider>();
			_boxCollider2D = GetComponent<BoxCollider2D>();
			_circleCollider2D = GetComponent<CircleCollider2D>();
			_lastDamagePosition = transform.position;
		}

		/// <summary>
		/// Initializes colliders, setting them as trigger if needed
		/// </summary>
		protected virtual void InitializeColliders()
		{
			_twoD = _boxCollider2D != null || _circleCollider2D != null;
			if (_boxCollider2D != null)
			{
				SetGizmoOffset(_boxCollider2D.offset);
				_boxCollider2D.isTrigger = true;
			}

			if (_boxCollider != null)
			{
				SetGizmoOffset(_boxCollider.center);
				_boxCollider.isTrigger = true;
			}

			if (_sphereCollider != null)
			{
				SetGizmoOffset(_sphereCollider.center);
				_sphereCollider.isTrigger = true;
			}

			if (_circleCollider2D != null)
			{
				SetGizmoOffset(_circleCollider2D.offset);
				_circleCollider2D.isTrigger = true;
			}
		}

		/// <summary>
		/// Initializes the _ignoredGameObjects list if needed
		/// </summary>
		protected virtual void InitializeIgnoreList()
		{
			if (_ignoredGameObjects == null) _ignoredGameObjects = new List<GameObject>();
		}

		/// <summary>
		/// Initializes feedbacks
		/// </summary>
		public virtual void InitializeFeedbacks()
		{
			if (_initializedFeedbacks) return;

			HitDamageableFeedback?.Initialization(this.gameObject);
			HitNonDamageableFeedback?.Initialization(this.gameObject);
			HitAnythingFeedback?.Initialization(this.gameObject);
			_initializedFeedbacks = true;
		}

		/// <summary>
		/// On disable we clear our ignore list
		/// </summary>
		protected virtual void OnDisable()
		{
			ClearIgnoreList();
		}

		/// <summary>
		/// On validate we ensure our inspector is in sync
		/// </summary>
		protected virtual void OnValidate()
		{
			TriggerFilter &= AllowedTriggerCallbacks;
		}
		
		#endregion

		#region Gizmos

		/// <summary>
		/// Initializes gizmo colors & settings
		/// </summary>
		protected virtual void InitalizeGizmos()
		{
			_gizmosColor = Color.red;
			_gizmosColor.a = 0.25f;
		}
		
		/// <summary>
		/// A public method letting you (re)define gizmo size
		/// </summary>
		/// <param name="newGizmoSize"></param>
		public virtual void SetGizmoSize(Vector3 newGizmoSize)
		{
			_boxCollider2D = GetComponent<BoxCollider2D>();
			_boxCollider = GetComponent<BoxCollider>();
			_sphereCollider = GetComponent<SphereCollider>();
			_circleCollider2D = GetComponent<CircleCollider2D>();
			_gizmoSize = newGizmoSize;
		}

		/// <summary>
		/// A public method letting you specify a gizmo offset
		/// </summary>
		/// <param name="newOffset"></param>
		public virtual void SetGizmoOffset(Vector3 newOffset)
		{
			_gizmoOffset = newOffset;
		}
		
		/// <summary>
		/// draws a cube or sphere around the damage area
		/// </summary>
		protected virtual void OnDrawGizmos()
		{
			Gizmos.color = _gizmosColor;

			if (_boxCollider2D != null)
			{
				if (_boxCollider2D.enabled)
				{
					MMDebug.DrawGizmoCube(transform, _gizmoOffset, _boxCollider2D.size, false);
				}
				else
				{
					MMDebug.DrawGizmoCube(transform, _gizmoOffset, _boxCollider2D.size, true);
				}
			}

			if (_circleCollider2D != null)
			{
				Matrix4x4 rotationMatrix = transform.localToWorldMatrix;
				Gizmos.matrix = rotationMatrix;
				if (_circleCollider2D.enabled)
				{
					Gizmos.DrawSphere( (Vector2)_gizmoOffset, _circleCollider2D.radius);
				}
				else
				{
					Gizmos.DrawWireSphere((Vector2)_gizmoOffset, _circleCollider2D.radius);
				}
			}

			if (_boxCollider != null)
			{
				if (_boxCollider.enabled)
					MMDebug.DrawGizmoCube(transform,
						_gizmoOffset,
						_boxCollider.size,
						false);
				else
					MMDebug.DrawGizmoCube(transform,
						_gizmoOffset,
						_boxCollider.size,
						true);
			}

			if (_sphereCollider != null)
			{
				if (_sphereCollider.enabled)
					Gizmos.DrawSphere(transform.position, _sphereCollider.radius);
				else
					Gizmos.DrawWireSphere(transform.position, _sphereCollider.radius);
			}
		}

		#endregion

		#region PublicAPIs

		/// <summary>
		/// When knockback is in script direction mode, lets you specify the direction of the knockback
		/// </summary>
		/// <param name="newDirection"></param>
		public virtual void SetKnockbackScriptDirection(Vector3 newDirection)
		{
			_knockbackScriptDirection = newDirection;
		}

		/// <summary>
		/// When damage direction is in script mode, lets you specify the direction of damage
		/// </summary>
		/// <param name="newDirection"></param>
		public virtual void SetDamageScriptDirection(Vector3 newDirection)
		{
			_damageDirection = newDirection;
		}

		/// <summary>
		/// Adds the gameobject set in parameters to the ignore list
		/// </summary>
		/// <param name="newIgnoredGameObject">New ignored game object.</param>
		public virtual void IgnoreGameObject(GameObject newIgnoredGameObject)
		{
			InitializeIgnoreList();
			_ignoredGameObjects.Add(newIgnoredGameObject);
		}
		
		/// <summary>
		/// Removes the object set in parameters from the ignore list
		/// </summary>
		/// <param name="ignoredGameObject">Ignored game object.</param>
		public virtual void StopIgnoringObject(GameObject ignoredGameObject)
		{
			if (_ignoredGameObjects != null) _ignoredGameObjects.Remove(ignoredGameObject);
		}

		/// <summary>
		/// Clears the ignore list.
		/// </summary>
		public virtual void ClearIgnoreList()
		{
			InitializeIgnoreList();
			_ignoredGameObjects.Clear();
		}

		#endregion

		#region Loop

		/// <summary>
		/// During last update, we store the position and velocity of the object
		/// </summary>
		protected virtual void Update()
		{
			ComputeVelocity();
		}

		/// <summary>
		/// On Late Update we store our position
		/// </summary>
		protected void LateUpdate()
		{
			_positionLastFrame = transform.position;
		}

		/// <summary>
		/// Computes the velocity based on the object's last position
		/// </summary>
		protected virtual void ComputeVelocity()
		{
			if (Time.deltaTime != 0f)
			{
				_velocity = (_lastPosition - (Vector3)transform.position) / Time.deltaTime;

				if (Vector3.Distance(_lastDamagePosition, transform.position) > 0.5f)
				{
					_lastDamagePosition = transform.position;
				}

				_lastPosition = transform.position;
			}
		}

		/// <summary>
		/// Determine the damage direction to pass to the Health Damage method
		/// </summary>
		protected virtual void DetermineDamageDirection()
		{
			switch (DamageDirectionMode)
			{
				case DamageDirections.BasedOnOwnerPosition:
					if (Owner == null)
					{
						Owner = gameObject;
					}
					if (_twoD)
					{
						_damageDirection = _collidingHealth.transform.position - Owner.transform.position;
						_damageDirection.z = 0;
					}
					else
					{
						_damageDirection = _collidingHealth.transform.position - Owner.transform.position;
					}
					break;
				case DamageDirections.BasedOnVelocity:
					_damageDirection = transform.position - _lastDamagePosition;
					break;
				case DamageDirections.BasedOnScriptDirection:
					_damageDirection = _damageScriptDirection;
					break;
			}

			_damageDirection = _damageDirection.normalized;
		}

		#endregion

		#region CollisionDetection

		/// <summary>
		/// When a collision with the player is triggered, we give damage to the player and knock it back
		/// </summary>
		/// <param name="collider">what's colliding with the object.</param>
		public virtual void OnTriggerStay2D(Collider2D collider)
		{
			if (0 == (TriggerFilter & TriggerAndCollisionMask.OnTriggerStay2D)) return;
			Colliding(collider.gameObject);
		}

		/// <summary>
		/// On trigger enter 2D, we call our colliding endpoint
		/// </summary>
		/// <param name="collider"></param>S
		public virtual void OnTriggerEnter2D(Collider2D collider)
		{
			if (0 == (TriggerFilter & TriggerAndCollisionMask.OnTriggerEnter2D)) return;
			Colliding(collider.gameObject);
		}

		/// <summary>
		/// On trigger stay, we call our colliding endpoint
		/// </summary>
		/// <param name="collider"></param>
		public virtual void OnTriggerStay(Collider collider)
		{
			if (0 == (TriggerFilter & TriggerAndCollisionMask.OnTriggerStay)) return;
			Colliding(collider.gameObject);
		}

		/// <summary>
		/// On trigger enter, we call our colliding endpoint
		/// </summary>
		/// <param name="collider"></param>
		public virtual void OnTriggerEnter(Collider collider)
		{
			if (0 == (TriggerFilter & TriggerAndCollisionMask.OnTriggerEnter)) return;
			Colliding(collider.gameObject);
		}

		#endregion
		
		/// <summary>
		/// When colliding, we apply the appropriate damage
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void Colliding(GameObject collider)
		{
			if (!EvaluateAvailability(collider))
			{
				return;
			}

			// cache reset 
			_colliderTopDownController = null;
			_colliderHealth = collider.gameObject.MMGetComponentNoAlloc<Health>();
			
			// if what we're colliding with is damageable
			if (_colliderHealth != null)
			{
				if (_colliderHealth.CurrentHealth > 0)
				{
					OnCollideWithDamageable(_colliderHealth);
				}
			}
			else // if what we're colliding with can't be damaged
			{
				OnCollideWithNonDamageable();
				HitNonDamageableEvent?.Invoke(collider);
			}

			OnAnyCollision(collider);
			HitAnythingEvent?.Invoke(collider);
			HitAnythingFeedback?.PlayFeedbacks(transform.position);
		}

		/// <summary>
		/// Checks whether or not damage should be applied this frame
		/// </summary>
		/// <param name="collider"></param>
		/// <returns></returns>
		protected virtual bool EvaluateAvailability(GameObject collider)
		{
			// if we're inactive, we do nothing
			if (!isActiveAndEnabled) { return false; }

			// if the object we're colliding with is part of our ignore list, we do nothing and exit
			if (_ignoredGameObjects.Contains(collider)) { return false; }

			// if what we're colliding with isn't part of the target layers, we do nothing and exit
			if (!MMLayers.LayerInLayerMask(collider.layer, TargetLayerMask)) { return false; }

			// if we're on our first frame, we don't apply damage
			if (Time.time == 0f) { return false; }

			return true;
		}

		/// <summary>
		/// Describes what happens when colliding with a damageable object
		/// </summary>
		/// <param name="health">Health.</param>
		protected virtual void OnCollideWithDamageable(Health health)
		{
			_collidingHealth = health;

			if (health.CanTakeDamageThisFrame())
			{
				// if what we're colliding with is a TopDownController, we apply a knockback force
				_colliderTopDownController = health.gameObject.MMGetComponentNoAlloc<TopDownController>();
				if (_colliderTopDownController == null)
				{
					_colliderTopDownController = health.gameObject.GetComponentInParent<TopDownController>();
				}

				HitDamageableFeedback?.PlayFeedbacks(this.transform.position);
				HitDamageableEvent?.Invoke(_colliderHealth);

				// we apply the damage to the thing we've collided with
				float randomDamage =
					UnityEngine.Random.Range(MinDamageCaused, Mathf.Max(MaxDamageCaused, MinDamageCaused));

				ApplyKnockback(randomDamage, TypedDamages);

				DetermineDamageDirection();

				if (RepeatDamageOverTime)
				{
					_colliderHealth.DamageOverTime(randomDamage, gameObject, InvincibilityDuration,
						InvincibilityDuration, _damageDirection, TypedDamages, AmountOfRepeats, DurationBetweenRepeats,
						DamageOverTimeInterruptible, RepeatedDamageType);
				}
				else
				{
					_colliderHealth.Damage(randomDamage, gameObject, InvincibilityDuration, InvincibilityDuration,
						_damageDirection, TypedDamages);
				}
			}

			// we apply self damage
			if (DamageTakenEveryTime + DamageTakenDamageable > 0 && !_colliderHealth.PreventTakeSelfDamage)
			{
				SelfDamage(DamageTakenEveryTime + DamageTakenDamageable);
			}
		}

		#region Knockback

		/// <summary>
		/// Applies knockback if needed
		/// </summary>
		protected virtual void ApplyKnockback(float damage, List<TypedDamage> typedDamages)
		{
			if (ShouldApplyKnockback(damage, typedDamages))
			{
				_knockbackForce = DamageCausedKnockbackForce * _colliderHealth.KnockbackForceMultiplier;
				_knockbackForce = _colliderHealth.ComputeKnockbackForce(_knockbackForce, typedDamages);

				if (_twoD) // if we're in 2D
				{
					ApplyKnockback2D();
				}
				else // if we're in 3D
				{
					ApplyKnockback3D();
				}
				
				if (DamageCausedKnockbackType == KnockbackStyles.AddForce)
				{
					_colliderTopDownController.Impact(_knockbackForce.normalized, _knockbackForce.magnitude);
				}
			}
		}

		/// <summary>
		/// Determines whether or not knockback should be applied
		/// </summary>
		/// <returns></returns>
		protected virtual bool ShouldApplyKnockback(float damage, List<TypedDamage> typedDamages)
		{
			if (_colliderHealth.ImmuneToKnockbackIfZeroDamage)
			{
				if (_colliderHealth.ComputeDamageOutput(damage, typedDamages, false) == 0)
				{
					return false;
				}
			}
			
			return (_colliderTopDownController != null)
			       && (DamageCausedKnockbackForce != Vector3.zero)
			       && !_colliderHealth.Invulnerable
			       && _colliderHealth.CanGetKnockback(typedDamages);
		}

		/// <summary>
		/// Applies knockback if we're in a 2D context
		/// </summary>
		protected virtual void ApplyKnockback2D()
		{
			switch (DamageCausedKnockbackDirection)
			{
				case KnockbackDirections.BasedOnSpeed:
					var totalVelocity = _colliderTopDownController.Speed + _velocity;
					_knockbackForce = Vector3.RotateTowards(_knockbackForce,
						totalVelocity.normalized, 10f, 0f);
					break;
				case KnockbackDirections.BasedOnOwnerPosition:
					if (Owner == null)
					{
						Owner = gameObject;
					}
					_relativePosition = _colliderTopDownController.transform.position - Owner.transform.position;
					_knockbackForce = Vector3.RotateTowards(_knockbackForce, _relativePosition.normalized, 10f, 0f);
					break;
				case KnockbackDirections.BasedOnDirection:
					var direction = transform.position - _positionLastFrame;
					_knockbackForce = direction * _knockbackForce.magnitude;
					break;
				case KnockbackDirections.BasedOnScriptDirection:
					_knockbackForce = _knockbackScriptDirection * _knockbackForce.magnitude;
					break;
			}
		}

		/// <summary>
		/// Applies knockback if we're in a 3D context
		/// </summary>
		protected virtual void ApplyKnockback3D()
		{
			switch (DamageCausedKnockbackDirection)
			{
				case KnockbackDirections.BasedOnSpeed:
					var totalVelocity = _colliderTopDownController.Speed + _velocity;
					_knockbackForce = _knockbackForce * totalVelocity.magnitude;
					break;
				case KnockbackDirections.BasedOnOwnerPosition:
					if (Owner == null)
					{
						Owner = gameObject;
					}
					_relativePosition = _colliderTopDownController.transform.position - Owner.transform.position;
					_knockbackForce = Quaternion.LookRotation(_relativePosition) * _knockbackForce;
					break;
				case KnockbackDirections.BasedOnDirection:
					var direction = transform.position - _positionLastFrame;
					_knockbackForce = direction * _knockbackForce.magnitude;
					break;
				case KnockbackDirections.BasedOnScriptDirection:
					_knockbackForce = _knockbackScriptDirection * _knockbackForce.magnitude;
					break;
			}
		}

		#endregion
		

		/// <summary>
		/// Describes what happens when colliding with a non damageable object
		/// </summary>
		protected virtual void OnCollideWithNonDamageable()
		{
			float selfDamage = DamageTakenEveryTime + DamageTakenNonDamageable; 
			if (selfDamage > 0)
			{
				SelfDamage(selfDamage);
			}
			HitNonDamageableFeedback?.PlayFeedbacks(transform.position);
		}

		/// <summary>
		/// Describes what could happens when colliding with anything
		/// </summary>
		protected virtual void OnAnyCollision(GameObject other)
		{
		}

		/// <summary>
		/// Applies damage to itself
		/// </summary>
		/// <param name="damage">Damage.</param>
		protected virtual void SelfDamage(float damage)
		{
			if (DamageTakenHealth != null)
			{
				_damageDirection = Vector3.up;
				DamageTakenHealth.Damage(damage, gameObject, 0f, DamageTakenInvincibilityDuration, _damageDirection);
			}

			// if what we're colliding with is a TopDownController, we apply a knockback force
			if ((_topDownController != null) && (_colliderTopDownController != null))
			{
				Vector3 totalVelocity = _colliderTopDownController.Speed + _velocity;
				Vector3 knockbackForce =
					Vector3.RotateTowards(DamageTakenKnockbackForce, totalVelocity.normalized, 10f, 0f);

				if (DamageTakenKnockbackType == KnockbackStyles.AddForce)
				{
					_topDownController.AddForce(knockbackForce);
				}
			}
		}
	}
}