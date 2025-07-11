﻿using System;
using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{	
	/// <summary>
	/// Projectile class to be used along with projectile weapons
	/// </summary>
	[AddComponentMenu("TopDown Engine/Weapons/Projectile")]
	public class Projectile : MMPoolableObject  
	{
		public enum MovementVectors { Forward, Right, Up}
		
		[Header("Movement")]

        /// if true, the projectile will rotate at initialization towards its rotation
        [MMLabel("面向初始方向")]
        [Tooltip("如果启用，抛射物在初始化时将转向其指定的旋转方向")]
        public bool FaceDirection = true;

        /// if true, the projectile will rotate towards movement
        [MMLabel("面向移动方向")]
        [Tooltip("如果启用，抛射物将在移动过程中转向其移动方向")]
        public bool FaceMovement = false;

        /// if FaceMovement is true, the projectile's vector specified below will be aligned to the movement vector, usually you'll want to go with Forward in 3D, Right in 2D
        [MMLabel("移动对齐轴")]
        [Tooltip("当面向移动方向启用时，选择与移动方向对齐的轴：3D通常选Forward，2D通常选Right")]
        [MMCondition("FaceMovement", true)]
        public MovementVectors MovementVector = MovementVectors.Forward;

        /// the speed of the object (relative to the level's speed)
        [MMLabel("移动速度")]
        [Tooltip("抛射物的移动速度（相对于关卡速度的倍率）")]
        public float Speed = 0;

        /// the acceleration of the object over time. Starts accelerating on enable.
        [MMLabel("加速度")]
        [Tooltip("抛射物的加速度，从启用时开始生效")]
        public float Acceleration = 0;

        /// the current direction of the object
        [MMLabel("初始方向")]
        [Tooltip("抛射物的初始移动方向")]
        public Vector3 Direction = Vector3.left;

        /// if set to true, the spawner can change the direction of the object. If not the one set in its inspector will be used.
        [MMLabel("允许发射器改变方向")]
        [Tooltip("是否允许发射器覆盖此抛射物的初始方向")]
        public bool DirectionCanBeChangedBySpawner = true;

        /// the flip factor to apply if and when the projectile is mirrored
        [MMLabel("镜像翻转因子")]
        [Tooltip("当抛射物需要镜像时应用的缩放因子")]
        public Vector3 FlipValue = new Vector3(-1, 1, 1);

        /// set this to true if your projectile's model (or sprite) is facing right, false otherwise
        [MMLabel("模型默认朝右")]
        [Tooltip("设置抛射物模型/精灵图是否默认朝右")]
        public bool ProjectileIsFacingRight = true;

        [Header("Spawn")]
        [MMInformation("Here you can define an initial delay (in seconds) during which this object won't take or cause damage. This delay starts when the object gets enabled. You can also define whether the projectiles should damage their owner (think rockets and the likes) or not", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]

        /// the initial delay during which the projectile can't be destroyed
        [MMLabel("初始无敌时间")]
        [Tooltip("抛射物启用后保持无敌的持续时间（秒）")]
        public float InitialInvulnerabilityDuration = 0f;

        /// should the projectile damage its owner?
        [MMLabel("伤害所有者")]
        [Tooltip("是否允许抛射物伤害其发射者")]
        public bool DamageOwner = false;

        /// Returns the associated damage on touch zone
        public virtual DamageOnTouch TargetDamageOnTouch { get { return _damageOnTouch; } }
		public virtual Weapon SourceWeapon { get { return _weapon; } }

		protected Weapon _weapon;
		protected GameObject _owner;
		protected Vector3 _movement;
		protected float _initialSpeed;
		protected SpriteRenderer _spriteRenderer;
		protected DamageOnTouch _damageOnTouch;
		protected WaitForSeconds _initialInvulnerabilityDurationWFS;
		protected Collider _collider;
		protected Collider2D _collider2D;
		protected Rigidbody _rigidBody;
		protected Rigidbody2D _rigidBody2D;
		protected bool _facingRightInitially;
		protected bool _initialFlipX;
		protected Vector3 _initialLocalScale;
		protected bool _shouldMove = true;
		protected Health _health;
		protected bool _spawnerIsFacingRight;

		/// <summary>
		/// On awake, we store the initial speed of the object 
		/// </summary>
		protected virtual void Awake ()
		{
			_facingRightInitially = ProjectileIsFacingRight;
			_initialSpeed = Speed;
			_health = GetComponent<Health> ();
			_collider = GetComponent<Collider> ();
			_collider2D = GetComponent<Collider2D>();
			_spriteRenderer = GetComponent<SpriteRenderer> ();
			_damageOnTouch = GetComponent<DamageOnTouch>();
			_rigidBody = GetComponent<Rigidbody> ();
			_rigidBody2D = GetComponent<Rigidbody2D> ();
			_initialInvulnerabilityDurationWFS = new WaitForSeconds (InitialInvulnerabilityDuration);
			if (_spriteRenderer != null) {	_initialFlipX = _spriteRenderer.flipX ;		}
			_initialLocalScale = transform.localScale;
		}

		/// <summary>
		/// Handles the projectile's initial invincibility
		/// </summary>
		/// <returns>The invulnerability.</returns>
		protected virtual IEnumerator InitialInvulnerability()
		{
			if (_damageOnTouch == null) { yield break; }
			if (_weapon == null) { yield break; }

			_damageOnTouch.ClearIgnoreList();
			if (_weapon.Owner != null)
			{
				_damageOnTouch.IgnoreGameObject(_weapon.Owner.gameObject);	
			}
			yield return _initialInvulnerabilityDurationWFS;
			if (DamageOwner)
			{
				_damageOnTouch.StopIgnoringObject(_weapon.Owner.gameObject);
			}
		}

		/// <summary>
		/// Initializes the projectile
		/// </summary>
		protected virtual void Initialization()
		{
			Speed = _initialSpeed;
			ProjectileIsFacingRight = _facingRightInitially;
			if (_spriteRenderer != null) {	_spriteRenderer.flipX = _initialFlipX;	}
			transform.localScale = _initialLocalScale;	
			_shouldMove = true;
			_damageOnTouch?.InitializeFeedbacks();

			if (_collider != null)
			{
				_collider.enabled = true;
			}
			if (_collider2D != null)
			{
				_collider2D.enabled = true;
			}
		}

		/// <summary>
		/// On update(), we move the object based on the level's speed and the object's speed, and apply acceleration
		/// </summary>
		protected virtual void FixedUpdate ()
		{
			base.Update ();
			if (_shouldMove)
			{
				Movement();
			}
		}

		/// <summary>
		/// Handles the projectile's movement, every frame
		/// </summary>
		public virtual void Movement()
		{
			_movement = Direction * (Speed / 10) * Time.deltaTime;
			//transform.Translate(_movement,Space.World);
			if (_rigidBody != null)
			{
				_rigidBody.MovePosition (this.transform.position + _movement);
			}
			if (_rigidBody2D != null)
			{
				_rigidBody2D.MovePosition(this.transform.position + _movement);
			}
			// We apply the acceleration to increase the speed
			Speed += Acceleration * Time.deltaTime;
		}

		/// <summary>
		/// Sets the projectile's direction.
		/// </summary>
		/// <param name="newDirection">New direction.</param>
		/// <param name="newRotation">New rotation.</param>
		/// <param name="spawnerIsFacingRight">If set to <c>true</c> spawner is facing right.</param>
		public virtual void SetDirection(Vector3 newDirection, Quaternion newRotation, bool spawnerIsFacingRight = true)
		{
			_spawnerIsFacingRight = spawnerIsFacingRight;

			if (DirectionCanBeChangedBySpawner)
			{
				Direction = newDirection;
			}
			if (ProjectileIsFacingRight != spawnerIsFacingRight)
			{
				Flip ();
			}
			if (FaceDirection)
			{
				transform.rotation = newRotation;
			}

			if (_damageOnTouch != null)
			{
				_damageOnTouch.SetKnockbackScriptDirection(newDirection);
			}

			if (FaceMovement)
			{
				switch (MovementVector)
				{
					case MovementVectors.Forward:
						transform.forward = newDirection;
						break;
					case MovementVectors.Right:
						transform.right = newDirection;
						break;
					case MovementVectors.Up:
						transform.up = newDirection;
						break;
				}
			}
		}

		/// <summary>
		/// Flip the projectile
		/// </summary>
		protected virtual void Flip()
		{
			if (_spriteRenderer != null)
			{
				_spriteRenderer.flipX = !_spriteRenderer.flipX;
			}	
			else
			{
				this.transform.localScale = Vector3.Scale(this.transform.localScale,FlipValue) ;
			}
		}
        
		/// <summary>
		/// Flip the projectile
		/// </summary>
		protected virtual void Flip(bool state)
		{
			if (_spriteRenderer != null)
			{
				_spriteRenderer.flipX = state;
			}
			else
			{
				this.transform.localScale = Vector3.Scale(this.transform.localScale, FlipValue);
			}
		}

		/// <summary>
		/// Sets the projectile's parent weapon.
		/// </summary>
		/// <param name="newWeapon">New weapon.</param>
		public virtual void SetWeapon(Weapon newWeapon)
		{
			_weapon = newWeapon;
		}

		/// <summary>
		/// Sets the damage caused by the projectile's DamageOnTouch to the specified value
		/// </summary>
		/// <param name="newDamage"></param>
		public virtual void SetDamage(float minDamage, float maxDamage)
		{
			if (_damageOnTouch != null)
			{
				_damageOnTouch.MinDamageCaused = minDamage;
				_damageOnTouch.MaxDamageCaused = maxDamage;
			}
		}
        
		/// <summary>
		/// Sets the projectile's owner.
		/// </summary>
		/// <param name="newOwner">New owner.</param>
		public virtual void SetOwner(GameObject newOwner)
		{
			_owner = newOwner;
			DamageOnTouch damageOnTouch = this.gameObject.MMGetComponentNoAlloc<DamageOnTouch>();
			if (damageOnTouch != null)
			{
				damageOnTouch.Owner = newOwner;
				if (!DamageOwner)
				{
					damageOnTouch.ClearIgnoreList();
					damageOnTouch.IgnoreGameObject(newOwner);
				}
			}
		}

		/// <summary>
		/// Returns the current Owner of the projectile
		/// </summary>
		/// <returns></returns>
		public virtual GameObject GetOwner()
		{
			return _owner;
		}

		/// <summary>
		/// On death, disables colliders and prevents movement
		/// </summary>
		public virtual void StopAt()
		{
			if (_collider != null)
			{
				_collider.enabled = false;
			}
			if (_collider2D != null)
			{
				_collider2D.enabled = false;
			}
			
			_shouldMove = false;
		}

		/// <summary>
		/// On death, we stop our projectile
		/// </summary>
		protected virtual void OnDeath()
		{
			StopAt ();
		}

		/// <summary>
		/// On enable, we trigger a short invulnerability
		/// </summary>
		protected override void OnEnable ()
		{
			base.OnEnable ();

			Initialization();
			if (InitialInvulnerabilityDuration>0)
			{
				StartCoroutine(InitialInvulnerability());
			}

			if (_health != null)
			{
				_health.OnDeath += OnDeath;
			}
		}

		/// <summary>
		/// On disable, we plug our OnDeath method to the health component
		/// </summary>
		protected override void OnDisable()
		{
			base.OnDisable ();
			if (_health != null)
			{
				_health.OnDeath -= OnDeath;
			}			
		}
	}	
}