using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
using UnityEngine.Serialization;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// A basic melee weapon class, that will activate a "hurt zone" when the weapon is used
	/// </summary>
	[AddComponentMenu("TopDown Engine/Weapons/Melee Weapon")]
	public class MeleeWeapon : Weapon
	{
		/// the possible shapes for the melee weapon's damage area
		public enum MeleeDamageAreaShapes { Rectangle, Circle, Box, Sphere }
		public enum MeleeDamageAreaModes { Generated, Existing }

        [MMInspectorGroup("Damage Area", true, 22)]
        /// the possible modes to handle the damage area...
        [MMLabel("伤害区域模式")]
        [Tooltip("伤害区域处理模式：生成模式自动创建，已有模式需绑定预制伤害区域")]
        public MeleeDamageAreaModes MeleeDamageAreaMode = MeleeDamageAreaModes.Generated;

        /// the shape of the damage area...
        [MMLabel("形状")]
        [Tooltip("伤害区域形状（矩形/圆形）")]
        [MMEnumCondition("MeleeDamageAreaMode", (int)MeleeDamageAreaModes.Generated)]
        public MeleeDamageAreaShapes DamageAreaShape = MeleeDamageAreaShapes.Rectangle;

        /// the offset to apply...
        [MMLabel("偏移")]
        [Tooltip("伤害区域相对于武器附加点的偏移量")]
        [MMEnumCondition("MeleeDamageAreaMode", (int)MeleeDamageAreaModes.Generated)]
        public Vector3 AreaOffset = new Vector3(1, 0);

        /// the size of the damage area...
        [MMLabel("尺寸")]
        [Tooltip("伤害区域的大小范围")]
        [MMEnumCondition("MeleeDamageAreaMode", (int)MeleeDamageAreaModes.Generated)]
        public Vector3 AreaSize = new Vector3(1, 1);

        /// the trigger filters...
        [MMLabel("触发过滤")]
        [Tooltip("伤害触发条件（默认持续造成伤害，可改为仅进入区域时触发）")]
        [MMEnumCondition("MeleeDamageAreaMode", (int)MeleeDamageAreaModes.Generated)]
        public DamageOnTouch.TriggerAndCollisionMask TriggerFilter = DamageOnTouch.AllowedTriggerCallbacks;

        /// the feedback to play when hitting...
        [MMLabel("可伤害反馈")]
        [Tooltip("命中可伤害对象时播放的反馈效果")]
        [MMEnumCondition("MeleeDamageAreaMode", (int)MeleeDamageAreaModes.Generated)]
        public MMFeedbacks HitDamageableFeedback;

        /// the feedback to play when hitting...
        [MMLabel("非伤害反馈")]
        [Tooltip("命中不可伤害对象时播放的反馈效果")]
        [MMEnumCondition("MeleeDamageAreaMode", (int)MeleeDamageAreaModes.Generated)]
        public MMFeedbacks HitNonDamageableFeedback;

        /// an existing damage area...
        [MMLabel("现有伤害区域")]
        [Tooltip("手动绑定的预制伤害区域对象")]
        [MMEnumCondition("MeleeDamageAreaMode", (int)MeleeDamageAreaModes.Existing)]
        public DamageOnTouch ExistingDamageArea;

        [MMInspectorGroup("Damage Area Timing", true, 23)]

        /// the initial delay...
        [MMLabel("伤害区域激活延迟")]
        [Tooltip("伤害区域激活前的初始延迟时间")]
        public float InitialDelay = 0f;

        /// the duration during which...
        [MMLabel("伤害区域激活延迟")]
        [Tooltip("伤害区域保持激活状态的持续时间")]
        public float ActiveDuration = 1f;

        [MMInspectorGroup("Damage Caused", true, 24)]

        /// the layers that will be...
        [MMLabel("目标层级")]
        [Tooltip("会受到伤害的层级遮罩")]
        public LayerMask TargetLayerMask;

        /// The min amount of health...
        [MMLabel("最小伤害")]
        [Tooltip("对目标造成的最小伤害值")]
        public float MinDamageCaused = 10f;

        /// The max amount of health...
        [MMLabel("最大伤害")]
        [Tooltip("对目标造成的最大伤害值")]
        public float MaxDamageCaused = 10f;

        /// the kind of knockback...
        [MMLabel("击退类型")]
        [Tooltip("击退效果的应用方式")]
        public DamageOnTouch.KnockbackStyles Knockback;

        /// The force to apply...
        [MMLabel("击退力度")]
        [Tooltip("施加给被击目标的力度矢量")]
        public Vector3 KnockbackForce = new Vector3(10, 2, 0);

        /// The direction in which...
        [MMLabel("击退方向")]
        [Tooltip("击退方向计算方式（通常基于攻击者位置）")]
        public DamageOnTouch.KnockbackDirections KnockbackDirection = DamageOnTouch.KnockbackDirections.BasedOnOwnerPosition;

        /// The duration of the invincibility...
        [MMLabel("无敌时间")]
        [Tooltip("命中后目标的无敌帧持续时间（秒）")]
        public float InvincibilityDuration = 0.5f;

        /// if this is true...
        [MMLabel("可伤害所有者")]
        [Tooltip("是否允许武器伤害自己的使用者（通常关闭）")]
        public bool CanDamageOwner = false;

        protected Collider _damageAreaCollider;
		protected Collider2D _damageAreaCollider2D;
		protected bool _attackInProgress = false;
		protected Color _gizmosColor;
		protected Vector3 _gizmoSize;
		protected CircleCollider2D _circleCollider2D;
		protected BoxCollider2D _boxCollider2D;
		protected BoxCollider _boxCollider;
		protected SphereCollider _sphereCollider;
		protected Vector3 _gizmoOffset;
		protected DamageOnTouch _damageOnTouch;
		protected GameObject _damageArea;
		protected Coroutine _attackCoroutine;

		/// <summary>
		/// Initialization
		/// </summary>
		public override void Initialization()
		{
			base.Initialization();

			if (_damageArea == null)
			{
				CreateDamageArea();
				DisableDamageArea();
			}
			if (Owner != null)
			{
				_damageOnTouch.Owner = Owner.gameObject;
			}            
		}

		/// <summary>
		/// Creates the damage area.
		/// </summary>
		protected virtual void CreateDamageArea()
		{
			if ((MeleeDamageAreaMode == MeleeDamageAreaModes.Existing) && (ExistingDamageArea != null))
			{
				_damageArea = ExistingDamageArea.gameObject;
				_damageAreaCollider = _damageArea.gameObject.GetComponent<Collider>();
				_damageAreaCollider2D = _damageArea.gameObject.GetComponent<Collider2D>();
				_damageOnTouch = ExistingDamageArea;
				return;
			}
			
			_damageArea = new GameObject();
			_damageArea.name = this.name + "DamageArea";
			_damageArea.transform.position = this.transform.position;
			_damageArea.transform.rotation = this.transform.rotation;
			_damageArea.transform.SetParent(this.transform);
			_damageArea.transform.localScale = Vector3.one;
			_damageArea.layer = this.gameObject.layer;
            
			if (DamageAreaShape == MeleeDamageAreaShapes.Rectangle)
			{
				_boxCollider2D = _damageArea.AddComponent<BoxCollider2D>();
				_boxCollider2D.offset = AreaOffset;
				_boxCollider2D.size = AreaSize;
				_damageAreaCollider2D = _boxCollider2D;
				_damageAreaCollider2D.isTrigger = true;
			}
			if (DamageAreaShape == MeleeDamageAreaShapes.Circle)
			{
				_circleCollider2D = _damageArea.AddComponent<CircleCollider2D>();
				_circleCollider2D.transform.position = this.transform.position;
				_circleCollider2D.offset = AreaOffset;
				_circleCollider2D.radius = AreaSize.x / 2;
				_damageAreaCollider2D = _circleCollider2D;
				_damageAreaCollider2D.isTrigger = true;
			}

			if ((DamageAreaShape == MeleeDamageAreaShapes.Rectangle) || (DamageAreaShape == MeleeDamageAreaShapes.Circle))
			{
				Rigidbody2D rigidBody = _damageArea.AddComponent<Rigidbody2D>();
				rigidBody.isKinematic = true;
				rigidBody.sleepMode = RigidbodySleepMode2D.NeverSleep;
			}            

			if (DamageAreaShape == MeleeDamageAreaShapes.Box)
			{
				_boxCollider = _damageArea.AddComponent<BoxCollider>();
				_boxCollider.center = AreaOffset;
				_boxCollider.size = AreaSize;
				_damageAreaCollider = _boxCollider;
				_damageAreaCollider.isTrigger = true;
			}
			if (DamageAreaShape == MeleeDamageAreaShapes.Sphere)
			{
				_sphereCollider = _damageArea.AddComponent<SphereCollider>();
				_sphereCollider.transform.position = this.transform.position + this.transform.rotation * AreaOffset;
				_sphereCollider.radius = AreaSize.x / 2;
				_damageAreaCollider = _sphereCollider;
				_damageAreaCollider.isTrigger = true;
			}

			if ((DamageAreaShape == MeleeDamageAreaShapes.Box) || (DamageAreaShape == MeleeDamageAreaShapes.Sphere))
			{
				Rigidbody rigidBody = _damageArea.AddComponent<Rigidbody>();
				rigidBody.isKinematic = true;

				rigidBody.gameObject.AddComponent<MMRagdollerIgnore>();
			}

			_damageOnTouch = _damageArea.AddComponent<DamageOnTouch>();
			_damageOnTouch.SetGizmoSize(AreaSize);
			_damageOnTouch.SetGizmoOffset(AreaOffset);
			_damageOnTouch.TargetLayerMask = TargetLayerMask;
			_damageOnTouch.MinDamageCaused = MinDamageCaused;
			_damageOnTouch.MaxDamageCaused = MaxDamageCaused;
			_damageOnTouch.DamageDirectionMode = DamageOnTouch.DamageDirections.BasedOnOwnerPosition;
			_damageOnTouch.DamageCausedKnockbackType = Knockback;
			_damageOnTouch.DamageCausedKnockbackForce = KnockbackForce;
			_damageOnTouch.DamageCausedKnockbackDirection = KnockbackDirection;
			_damageOnTouch.InvincibilityDuration = InvincibilityDuration;
			_damageOnTouch.HitDamageableFeedback = HitDamageableFeedback;
			_damageOnTouch.HitNonDamageableFeedback = HitNonDamageableFeedback;
			_damageOnTouch.TriggerFilter = TriggerFilter;
            
			if (!CanDamageOwner && (Owner != null))
			{
				_damageOnTouch.IgnoreGameObject(Owner.gameObject);    
			}
		}

		/// <summary>
		/// When the weapon is used, we trigger our attack routine
		/// </summary>
		public override void WeaponUse()
		{
			base.WeaponUse();
			_attackCoroutine = StartCoroutine(MeleeWeaponAttack());
		}

		/// <summary>
		/// Triggers an attack, turning the damage area on and then off
		/// </summary>
		/// <returns>The weapon attack.</returns>
		protected virtual IEnumerator MeleeWeaponAttack()
		{
			if (_attackInProgress) { yield break; }

			_attackInProgress = true;
			yield return new WaitForSeconds(InitialDelay);
			EnableDamageArea();
			yield return new WaitForSeconds(ActiveDuration);
			DisableDamageArea();
			_attackInProgress = false;
		}

		/// <summary>
		/// On interrupt, we stop our damage area sequence if needed
		/// </summary>
		public override void Interrupt()
		{
			base.Interrupt();
			if (_attackCoroutine != null)
			{
				StopCoroutine(_attackCoroutine);
			}	
		}

		/// <summary>
		/// Enables the damage area.
		/// </summary>
		protected virtual void EnableDamageArea()
		{
			if (_damageAreaCollider2D != null)
			{
				_damageAreaCollider2D.enabled = true;
			}
			if (_damageAreaCollider != null)
			{
				_damageAreaCollider.enabled = true;
			}
		}


		/// <summary>
		/// Disables the damage area.
		/// </summary>
		protected virtual void DisableDamageArea()
		{
			if (_damageAreaCollider2D != null)
			{
				_damageAreaCollider2D.enabled = false;
			}
			if (_damageAreaCollider != null)
			{
				_damageAreaCollider.enabled = false;
			}
		}

		/// <summary>
		/// When selected, we draw a bunch of gizmos
		/// </summary>
		protected virtual void OnDrawGizmosSelected()
		{
			if (!Application.isPlaying)
			{
				DrawGizmos();
			}            
		}

		/// <summary>
		/// Draws damage area gizmos
		/// </summary>
		protected virtual void DrawGizmos()
		{
			if (MeleeDamageAreaMode == MeleeDamageAreaModes.Existing)
			{
				return;
			}
			
			if (DamageAreaShape == MeleeDamageAreaShapes.Box)
			{
				Gizmos.DrawWireCube(this.transform.position + AreaOffset, AreaSize);
			}

			if (DamageAreaShape == MeleeDamageAreaShapes.Circle)
			{
				Gizmos.DrawWireSphere(this.transform.position + AreaOffset, AreaSize.x / 2);
			}

			if (DamageAreaShape == MeleeDamageAreaShapes.Rectangle)
			{
				MMDebug.DrawGizmoRectangle(this.transform.position + AreaOffset, AreaSize, Color.red);
			}

			if (DamageAreaShape == MeleeDamageAreaShapes.Sphere)
			{
				Gizmos.DrawWireSphere(this.transform.position + AreaOffset, AreaSize.x / 2);
			}
		}

		/// <summary>
		/// On disable we set our flag to false
		/// </summary>
		protected virtual void OnDisable()
		{
			_attackInProgress = false;
		}
	}
}