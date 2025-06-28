using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// An event triggered every time health values change, for other classes to listen to
	/// </summary>
	public struct HealthChangeEvent
	{
		public Health AffectedHealth;
		public float NewHealth;
		
		public HealthChangeEvent(Health affectedHealth, float newHealth)
		{
			AffectedHealth = affectedHealth;
			NewHealth = newHealth;
		}

		static HealthChangeEvent e;
		public static void Trigger(Health affectedHealth, float newHealth)
		{
			e.AffectedHealth = affectedHealth;
			e.NewHealth = newHealth;
			MMEventManager.TriggerEvent(e);
		}
	}
	
	/// <summary>
	/// This class manages the health of an object, pilots its potential health bar, handles what happens when it takes damage,
	/// and what happens when it dies.
	/// </summary>
	[AddComponentMenu("TopDown Engine/Character/Core/Health")] 
	public class Health : TopDownMonoBehaviour
	{
        [MMInspectorGroup("Bindings", true, 3)]

        /// the model to disable (if set so)
        [MMLabel("禁用模型")]
        [Tooltip("当设置为禁用时，需要关闭的模型对象")]
        public GameObject Model;

        [MMInspectorGroup("Status", true, 29)]

        /// the current health of the character
        [MMReadOnly]
        [MMLabel("当前生命值")]
        [Tooltip("角色当前的生命值数值")]
        public float CurrentHealth;

        /// If this is true, this object can't take damage at this time

        [MMReadOnly]
        [MMLabel("无敌状态")]
        [Tooltip("如果是true, 该物体此时无法受到伤害。")]
        public bool Invulnerable = false;

        [MMInspectorGroup("Health", true, 5)]

        [MMInformation("将此组件添加到一个对象上，该对象就会拥有生命值，能够受到伤害并有可能死亡。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]

        /// the initial amount of health of the object
        [MMLabel("初始生命值")]
        [Tooltip("the initial amount of health of the object")]
        public float InitialHealth = 10;

        /// the maximum amount of health of the object
        [MMLabel("最大生命值")]
        [Tooltip("the maximum amount of health of the object")]
        public float MaximumHealth = 10;

        /// if this is true, health values will be reset everytime this character is enabled (usually at the start of a scene)
        [MMLabel("启用时重置生命")]
        [Tooltip("if this is true, health values will be reset everytime this character is enabled (usually at the start of a scene)")]
        public bool ResetHealthOnEnable = true;

        [MMInspectorGroup("Damage", true, 6)]
        [MMInformation("在这里，你可以指定对象受到伤害时要实例化的特效和音效，以及对象被击中时应闪烁多长时间（仅适用于精灵）。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]

        /// whether or not this Health object can be damaged 
        [MMLabel("免疫伤害")]
        [Tooltip("是否完全免疫所有类型的伤害")]
        public bool ImmuneToDamage = false;

        /// the feedback to play when getting damage
        [MMLabel("受伤反馈系统")]
        [Tooltip("受到伤害时触发的反馈效果")]
        public MMFeedbacks DamageMMFeedbacks;

        /// if this is true, the damage value will be passed to the MMFeedbacks as its Intensity parameter, letting you trigger more intense feedbacks as damage increases
        [MMLabel("反馈强度关联伤害")]
        [Tooltip("将伤害值作为强度参数传递给反馈系统")]
        public bool FeedbackIsProportionalToDamage = false;

        /// if you set this to true, other objects damaging this one won't take any self damage
        [MMLabel("防止自伤机制")]
        [Tooltip("启用后其他对象伤害此对象时不会受到反伤")]
        public bool PreventTakeSelfDamage = false;

        [MMInspectorGroup("击退", true, 63)]

        /// whether or not this object is immune to damage knockback
        [MMLabel("免疫击退效果")]
        [Tooltip("是否完全免疫所有击退效果")]
        public bool ImmuneToKnockback = false;

        /// whether or not this object is immune to damage knockback if the damage received is zero
        [MMLabel("零伤害时免疫击退")]
        [Tooltip("当受到的伤害为零时免疫击退")]
        public bool ImmuneToKnockbackIfZeroDamage = false;

        /// a multiplier applied to the incoming knockback forces. 0 will cancel all knockback, 0.5 will cut it in half, 1 will have no effect, 2 will double the knockback force, etc
        [MMLabel("击退力系数")]
        [Tooltip("击退力乘数：0取消击退，0.5减半，1无影响，2加倍等")]
        public float KnockbackForceMultiplier = 1f;

        [MMInspectorGroup("Death", true, 53)]

        [MMInformation("在这里，你可以设置物体死亡时要实例化的效果、要施加给它的力（需要自上而下的控制器）、要给游戏得分增加的点数，以及角色应该在哪里重生（仅适用于非玩家角色）。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]

        /// whether or not this object should get destroyed on death
        [MMLabel("死亡后销毁")]
        [Tooltip("生命归零后是否销毁对象")]
        public bool DestroyOnDeath = true;

        /// the time (in seconds) before the character is destroyed or disabled
        [MMLabel("销毁延迟时间")]
        [Tooltip("死亡后执行销毁/禁用的延迟时间（秒）")]
        public float DelayBeforeDestruction = 0f;

        /// the points the player gets when the object's health reaches zero
        [MMLabel("死亡得分")]
        [Tooltip("对象被摧毁时玩家获得的分数")]
        public int PointsWhenDestroyed;

        /// if this is set to false, the character will respawn at the location of its death, otherwise it'll be moved to its initial position (when the scene started)
        [MMLabel("初始位置复活")]
        [Tooltip("true=在场景初始位置复活，false=在死亡位置复活")]
        public bool RespawnAtInitialLocation = false;

        /// if this is true, the controller will be disabled on death
        [MMLabel("禁用控制器")]
        [Tooltip("死亡后是否禁用角色控制器")]
        public bool DisableControllerOnDeath = true;

        /// if this is true, the model will be disabled instantly on death (if a model has been set)
        [MMLabel("立即隐藏模型")]
        [Tooltip("死亡后立即隐藏模型（需设置模型引用）")]
        public bool DisableModelOnDeath = true;

        [MMLabel("关闭碰撞体")]
        [Tooltip("死亡后关闭主碰撞体")]
        public bool DisableCollisionsOnDeath = true;

        /// if this is true, collisions will also be turned off on child colliders when the character dies
        [MMLabel("关闭子碰撞体")]
        [Tooltip("死亡后同时关闭所有子对象的碰撞体")]
        public bool DisableChildCollisionsOnDeath = false;

        /// whether or not this object should change layer on death
        [MMLabel("切换层级")]
        [Tooltip("死亡后是否改变对象层级")]
        public bool ChangeLayerOnDeath = false;

        /// whether or not this object should change layer on death
        [MMLabel("递归切换层级")]
        [Tooltip("死亡后是否递归改变所有子对象层级")]
        public bool ChangeLayersRecursivelyOnDeath = false;

        /// the layer we should move this character to on death
        [MMLabel("死亡层级")]
        [Tooltip("死亡后对象将被移至此层级")]
        public MMLayer LayerOnDeath;

        /// the feedback to play when dying
        [MMLabel("死亡反馈系统")]
        [Tooltip("死亡时触发的反馈效果")]
        public MMFeedbacks DeathMMFeedbacks;

        /// if this is true, color will be reset on revive
        [MMLabel("复活重置颜色")]
        [Tooltip("复活时是否恢复原始颜色")]
        public bool ResetColorOnRevive = true;

        /// the name of the property on your renderer's shader that defines its color 
        [MMLabel("颜色属性名")]
        [Tooltip("the name of the property on your renderer's shader that defines its color")]
        [MMCondition("ResetColorOnRevive", true)]
        public string ColorMaterialPropertyName = "_Color";

        /// if this is true, this component will use material property blocks instead of working on an instance of the material.
        [MMLabel("使用材质属性块")]
        [Tooltip("if this is true, this component will use material property blocks instead of working on an instance of the material.")]
        public bool UseMaterialPropertyBlocks = false;

        [MMInspectorGroup("共享生命与抗性", true, 12)]

        /// another Health component (usually on another character) towards which all health will be redirected
        [MMLabel("需要传导的主生命系统")]
        [Tooltip("将生命操作重定向到另一个Health组件")]
        public Health MasterHealth;

        /// a DamageResistanceProcessor this Health will use to process damage when it's received
        [MMLabel("伤害抗性处理器")]
        [Tooltip("用于处理伤害的抗性计算组件")]
        public DamageResistanceProcessor TargetDamageResistanceProcessor;

        [MMInspectorGroup("Animator", true, 14)]

        /// the target animator to pass a Death animation parameter to. The Health component will try to auto bind this if left empty
        [MMLabel("目标动画控制器")]
        [Tooltip("自动绑定的目标Animator（用于播放死亡动画）")]
        public Animator TargetAnimator;

        /// if this is true, animator logs for the associated animator will be turned off to avoid potential spam
        [MMLabel("禁用动画日志")]
        [Tooltip("禁用关联Animator的日志输出以避免冗余信息")]
        public bool DisableAnimatorLogs = true;

        public virtual float LastDamage { get; set; }
		public virtual Vector3 LastDamageDirection { get; set; }
		public virtual bool Initialized => _initialized;

		// hit delegate
		public delegate void OnHitDelegate();
		public OnHitDelegate OnHit;

		// respawn delegate
		public delegate void OnReviveDelegate();
		public OnReviveDelegate OnRevive;

		// death delegate
		public delegate void OnDeathDelegate();
		public OnDeathDelegate OnDeath;

		protected Vector3 _initialPosition;
		protected Renderer _renderer;
		protected Character _character;
		protected CharacterMovement _characterMovement;
		protected TopDownController _controller;
		
		protected MMHealthBar _healthBar;
		protected Collider2D _collider2D;
		protected Collider _collider3D;
		protected CharacterController _characterController;
		protected bool _initialized = false;
		protected Color _initialColor;
		protected AutoRespawn _autoRespawn;
		protected int _initialLayer;
		protected MaterialPropertyBlock _propertyBlock;
		protected bool _hasColorProperty = false;

		protected const string _deathAnimatorParameterName = "Death";
		protected const string _healthAnimatorParameterName = "Health";
		protected const string _healthAsIntAnimatorParameterName = "HealthAsInt";
		protected int _deathAnimatorParameter;
		protected int _healthAnimatorParameter;
		protected int _healthAsIntAnimatorParameter;

		protected class InterruptiblesDamageOverTimeCoroutine
		{
			public Coroutine DamageOverTimeCoroutine;
			public DamageType DamageOverTimeType;
		}
		
		protected List<InterruptiblesDamageOverTimeCoroutine> _interruptiblesDamageOverTimeCoroutines;
		protected List<InterruptiblesDamageOverTimeCoroutine> _damageOverTimeCoroutines;

		#region Initialization
		
		/// <summary>
		/// On Awake, we initialize our health
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
			InitializeCurrentHealth();
		}

		/// <summary>
		/// On Start we grab our animator
		/// </summary>
		protected virtual void Start()
		{
			GrabAnimator();
		}
		
		/// <summary>
		/// Grabs useful components, enables damage and gets the inital color
		/// </summary>
		public virtual void Initialization()
		{
			_character = this.gameObject.GetComponentInParent<Character>(); 

			if (Model != null)
			{
				Model.SetActive(true);
			}        
            
			if (gameObject.GetComponentInParent<Renderer>() != null)
			{
				_renderer = GetComponentInParent<Renderer>();				
			}
			if (_character != null)
			{
				_characterMovement = _character.FindAbility<CharacterMovement>();
				if (_character.CharacterModel != null)
				{
					if (_character.CharacterModel.GetComponentInChildren<Renderer> ()!= null)
					{
						_renderer = _character.CharacterModel.GetComponentInChildren<Renderer> ();	
					}
				}	
			}
			if (_renderer != null)
			{
				if (UseMaterialPropertyBlocks && (_propertyBlock == null))
				{
					_propertyBlock = new MaterialPropertyBlock();
				}
	            
				if (ResetColorOnRevive)
				{
					if (UseMaterialPropertyBlocks)
					{
						if (_renderer.sharedMaterial.HasProperty(ColorMaterialPropertyName))
						{
							_hasColorProperty = true; 
							_initialColor = _renderer.sharedMaterial.GetColor(ColorMaterialPropertyName);
						}
					}
					else
					{
						if (_renderer.material.HasProperty(ColorMaterialPropertyName))
						{
							_hasColorProperty = true;
							_initialColor = _renderer.material.GetColor(ColorMaterialPropertyName);
						} 
					}
				}
			}

			_interruptiblesDamageOverTimeCoroutines = new List<InterruptiblesDamageOverTimeCoroutine>();
			_damageOverTimeCoroutines = new List<InterruptiblesDamageOverTimeCoroutine>();
			_initialLayer = gameObject.layer;
			
			_deathAnimatorParameter = Animator.StringToHash(_deathAnimatorParameterName);
			_healthAnimatorParameter = Animator.StringToHash(_healthAnimatorParameterName);
			_healthAsIntAnimatorParameter = Animator.StringToHash(_healthAsIntAnimatorParameterName);

			_autoRespawn = this.gameObject.GetComponentInParent<AutoRespawn>();
			_healthBar = this.gameObject.GetComponentInParent<MMHealthBar>();
			_controller = this.gameObject.GetComponentInParent<TopDownController>();
			_characterController = this.gameObject.GetComponentInParent<CharacterController>();
			_collider2D = this.gameObject.GetComponentInParent<Collider2D>();
			_collider3D = this.gameObject.GetComponentInParent<Collider>();

			DamageMMFeedbacks?.Initialization(this.gameObject);
			DeathMMFeedbacks?.Initialization(this.gameObject);

			StoreInitialPosition();
			_initialized = true;
			
			DamageEnabled();
		}
		
		/// <summary>
		/// Grabs the target animator
		/// </summary>
		protected virtual void GrabAnimator()
		{
			if (TargetAnimator == null)
			{
				BindAnimator();
			}

			if ((TargetAnimator != null) && DisableAnimatorLogs)
			{
				TargetAnimator.logWarnings = false;
			}
			UpdateHealthAnimationParameters();
		}

		/// <summary>
		/// Finds and binds an animator if possible
		/// </summary>
		protected virtual void BindAnimator()
		{
			if (_character != null)
			{
				if (_character.CharacterAnimator != null)
				{
					TargetAnimator = _character.CharacterAnimator;
				}
				else
				{
					TargetAnimator = GetComponent<Animator>();
				}
			}
			else
			{
				TargetAnimator = GetComponent<Animator>();
			}    
		}

		/// <summary>
		/// Stores the initial position for further use
		/// </summary>
		public virtual void StoreInitialPosition()
		{
			_initialPosition = this.transform.position;
		}
		
		/// <summary>
		/// Initializes health to either initial or current values
		/// </summary>
		public virtual void InitializeCurrentHealth()
		{
			if (MasterHealth == null)
			{
				SetHealth(InitialHealth);	
			}
			else
			{
				if (MasterHealth.Initialized)
				{
					SetHealth(MasterHealth.CurrentHealth);
				}
				else
				{
					SetHealth(MasterHealth.InitialHealth);
				}
			}
		}

		/// <summary>
		/// When the object is enabled (on respawn for example), we restore its initial health levels
		/// </summary>
		protected virtual void OnEnable()
		{
			if (ResetHealthOnEnable)
			{
				InitializeCurrentHealth();
			}
			if (Model != null)
			{
				Model.SetActive(true);
			}            
			DamageEnabled();
		}
		
		/// <summary>
		/// On Disable, we prevent any delayed destruction from running
		/// </summary>
		protected virtual void OnDisable()
		{
			CancelInvoke();
		}

		#endregion

		/// <summary>
		/// Returns true if this Health component can be damaged this frame, and false otherwise
		/// </summary>
		/// <returns></returns>
		public virtual bool CanTakeDamageThisFrame()
		{
			// if the object is invulnerable, we do nothing and exit
			if (Invulnerable || ImmuneToDamage)
			{
				return false;
			}

			if (!this.enabled)
			{
				return false;
			}
			
			// if we're already below zero, we do nothing and exit
			if ((CurrentHealth <= 0) && (InitialHealth != 0))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Called when the object takes damage
		/// </summary>
		/// <param name="damage">The amount of health points that will get lost.</param>
		/// <param name="instigator">The object that caused the damage.</param>
		/// <param name="flickerDuration">The time (in seconds) the object should flicker after taking the damage - not used anymore, kept to not break retrocompatibility</param>
		/// <param name="invincibilityDuration">The duration of the short invincibility following the hit.</param>
		public virtual void Damage(float damage, GameObject instigator, float flickerDuration, float invincibilityDuration, Vector3 damageDirection, List<TypedDamage> typedDamages = null)
		{
			if (!CanTakeDamageThisFrame())
			{
				return;
			}

			damage = ComputeDamageOutput(damage, typedDamages, true);
			
			// we decrease the character's health by the damage
			float previousHealth = CurrentHealth;
			if (MasterHealth != null)
			{
				previousHealth = MasterHealth.CurrentHealth;
				MasterHealth.SetHealth(MasterHealth.CurrentHealth - damage);
			}
			else
			{
				SetHealth(CurrentHealth - damage);	
			}

			LastDamage = damage;
			LastDamageDirection = damageDirection;
			if (OnHit != null)
			{
				OnHit();
			}

			// we prevent the character from colliding with Projectiles, Player and Enemies
			if (invincibilityDuration > 0)
			{
				DamageDisabled();
				StartCoroutine(DamageEnabled(invincibilityDuration));	
			}
            
			// we trigger a damage taken event
			MMDamageTakenEvent.Trigger(this, instigator, CurrentHealth, damage, previousHealth, typedDamages);

			// we update our animator
			if (TargetAnimator != null)
			{
				TargetAnimator.SetTrigger("Damage");
			}

			// we play our feedback
			if (FeedbackIsProportionalToDamage)
			{
				DamageMMFeedbacks?.PlayFeedbacks(this.transform.position, damage);    
			}
			else
			{
				DamageMMFeedbacks?.PlayFeedbacks(this.transform.position);
			}
            
			// we update the health bar
			UpdateHealthBar(true);
			
			// we process any condition state change
			ComputeCharacterConditionStateChanges(typedDamages);
			ComputeCharacterMovementMultipliers(typedDamages);

			// if health has reached zero we set its health to zero (useful for the healthbar)
			if (MasterHealth != null)
			{
				if (MasterHealth.CurrentHealth <= 0)
				{
					MasterHealth.CurrentHealth = 0;
					MasterHealth.Kill();
				}
			}
			else
			{
				if (CurrentHealth <= 0)
				{
					CurrentHealth = 0;
					Kill();
				}
					
			}
		}

        /// <summary>
        /// Interrupts all damage over time, regardless of type
        /// 中断所有持续性伤害，无论其类型如何
        /// </summary>
        public virtual void InterruptAllDamageOverTime()
		{
			foreach (InterruptiblesDamageOverTimeCoroutine coroutine in _interruptiblesDamageOverTimeCoroutines)
			{
				StopCoroutine(coroutine.DamageOverTimeCoroutine);
			}
			_interruptiblesDamageOverTimeCoroutines.Clear();
		}

		/// <summary>
		/// Interrupts all damage over time, even the non interruptible ones (usually on death)
		/// </summary>
		public virtual void StopAllDamageOverTime()
		{
			foreach (InterruptiblesDamageOverTimeCoroutine coroutine in _damageOverTimeCoroutines)
			{
				StopCoroutine(coroutine.DamageOverTimeCoroutine);
			}
			_damageOverTimeCoroutines.Clear();
		}

		/// <summary>
		/// Interrupts all damage over time of the specified type
		/// </summary>
		/// <param name="damageType"></param>
		public virtual void InterruptAllDamageOverTimeOfType(DamageType damageType)
		{
			foreach (InterruptiblesDamageOverTimeCoroutine coroutine in _interruptiblesDamageOverTimeCoroutines)
			{
				if (coroutine.DamageOverTimeType == damageType)
				{
					StopCoroutine(coroutine.DamageOverTimeCoroutine);	
				}
			}
			TargetDamageResistanceProcessor?.InterruptDamageOverTime(damageType);
		}

		/// <summary>
		/// Applies damage over time, for the specified amount of repeats (which includes the first application of damage, makes it easier to do quick maths in the inspector, and at the specified interval).
		/// Optionally you can decide that your damage is interruptible, in which case, calling InterruptAllDamageOverTime() will stop these from being applied, useful to cure poison for example.
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="instigator"></param>
		/// <param name="flickerDuration"></param>
		/// <param name="invincibilityDuration"></param>
		/// <param name="damageDirection"></param>
		/// <param name="typedDamages"></param>
		/// <param name="amountOfRepeats"></param>
		/// <param name="durationBetweenRepeats"></param>
		/// <param name="interruptible"></param>
		public virtual void DamageOverTime(float damage, GameObject instigator, float flickerDuration,
			float invincibilityDuration, Vector3 damageDirection, List<TypedDamage> typedDamages = null,
			int amountOfRepeats = 0, float durationBetweenRepeats = 1f, bool interruptible = true, DamageType damageType = null)
		{
			if (ComputeDamageOutput(damage, typedDamages, false) == 0)
			{
				return;
			}
			
			InterruptiblesDamageOverTimeCoroutine damageOverTime = new InterruptiblesDamageOverTimeCoroutine();
			damageOverTime.DamageOverTimeType = damageType;
			damageOverTime.DamageOverTimeCoroutine = StartCoroutine(DamageOverTimeCo(damage, instigator, flickerDuration,
				invincibilityDuration, damageDirection, typedDamages, amountOfRepeats, durationBetweenRepeats,
				interruptible));
			_damageOverTimeCoroutines.Add(damageOverTime);
			if (interruptible)
			{
				_interruptiblesDamageOverTimeCoroutines.Add(damageOverTime);
			}
		}

		/// <summary>
		/// A coroutine used to apply damage over time
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="instigator"></param>
		/// <param name="flickerDuration"></param>
		/// <param name="invincibilityDuration"></param>
		/// <param name="damageDirection"></param>
		/// <param name="typedDamages"></param>
		/// <param name="amountOfRepeats"></param>
		/// <param name="durationBetweenRepeats"></param>
		/// <param name="interruptible"></param>
		/// <param name="damageType"></param>
		/// <returns></returns>
		protected virtual IEnumerator DamageOverTimeCo(float damage, GameObject instigator, float flickerDuration,
			float invincibilityDuration, Vector3 damageDirection, List<TypedDamage> typedDamages = null,
			int amountOfRepeats = 0, float durationBetweenRepeats = 1f, bool interruptible = true, DamageType damageType = null)
		{
			for (int i = 0; i < amountOfRepeats; i++)
			{
				Damage(damage, instigator, flickerDuration, invincibilityDuration, damageDirection, typedDamages);
				yield return MMCoroutine.WaitFor(durationBetweenRepeats);
			}
		}

		/// <summary>
		/// Returns the damage this health should take after processing potential resistances
		/// </summary>
		/// <param name="damage"></param>
		/// <returns></returns>
		public virtual float ComputeDamageOutput(float damage, List<TypedDamage> typedDamages = null, bool damageApplied = false)
		{
			if (Invulnerable || ImmuneToDamage)
			{
				return 0;
			}
			
			float totalDamage = 0f;
			// we process our damage through our potential resistances
			if (TargetDamageResistanceProcessor != null)
			{
				if (TargetDamageResistanceProcessor.isActiveAndEnabled)
				{
					totalDamage = TargetDamageResistanceProcessor.ProcessDamage(damage, typedDamages, damageApplied);	
				}
			}
			else
			{
				totalDamage = damage;
				if (typedDamages != null)
				{
					foreach (TypedDamage typedDamage in typedDamages)
					{
						totalDamage += typedDamage.DamageCaused;
					}
				}
			}
			return totalDamage;
		}

		/// <summary>
		/// Goes through resistances and applies condition state changes if needed
		/// </summary>
		/// <param name="typedDamages"></param>
		protected virtual void ComputeCharacterConditionStateChanges(List<TypedDamage> typedDamages)
		{
			if ((typedDamages == null) || (_character == null))
			{
				return;
			}
			
			foreach (TypedDamage typedDamage in typedDamages)
			{
				if (typedDamage.ForceCharacterCondition)
				{
					if (TargetDamageResistanceProcessor != null)
					{
						if (TargetDamageResistanceProcessor.isActiveAndEnabled)
						{
							bool checkResistance =
								TargetDamageResistanceProcessor.CheckPreventCharacterConditionChange(typedDamage.AssociatedDamageType);
							if (checkResistance)
							{
								continue;		
							}
						}
					}
					_character.ChangeCharacterConditionTemporarily(typedDamage.ForcedCondition, typedDamage.ForcedConditionDuration, typedDamage.ResetControllerForces, typedDamage.DisableGravity);	
				}
			}
			
		}

		/// <summary>
		/// Goes through the resistance list and applies movement multipliers if needed
		/// </summary>
		/// <param name="typedDamages"></param>
		protected virtual void ComputeCharacterMovementMultipliers(List<TypedDamage> typedDamages)
		{
			if ((typedDamages == null) || (_character == null))
			{
				return;
			}
			
			foreach (TypedDamage typedDamage in typedDamages)
			{
				if (typedDamage.ApplyMovementMultiplier)
				{
					if (TargetDamageResistanceProcessor != null)
					{
						if (TargetDamageResistanceProcessor.isActiveAndEnabled)
						{
							bool checkResistance =
								TargetDamageResistanceProcessor.CheckPreventMovementModifier(typedDamage.AssociatedDamageType);
							if (checkResistance)
							{
								continue;		
							}
						}
					}
					_characterMovement?.ApplyMovementMultiplier(typedDamage.MovementMultiplier,
						typedDamage.MovementMultiplierDuration);
				}
			}
		}
		
		/// <summary>
		/// Determines a new knockback force by processing it through resistances
		/// </summary>
		/// <param name="knockbackForce"></param>
		/// <param name="typedDamages"></param>
		/// <returns></returns>
		public virtual Vector3 ComputeKnockbackForce(Vector3 knockbackForce, List<TypedDamage> typedDamages = null)
		{
			return (TargetDamageResistanceProcessor == null) ? knockbackForce : TargetDamageResistanceProcessor.ProcessKnockbackForce(knockbackForce, typedDamages);;

		}

		/// <summary>
		/// Returns true if this Health can get knockbacked, false otherwise
		/// </summary>
		/// <param name="typedDamages"></param>
		/// <returns></returns>
		public virtual bool CanGetKnockback(List<TypedDamage> typedDamages) 
		{
			if (ImmuneToKnockback)
			{
				return false;
			}
			if (TargetDamageResistanceProcessor != null)
			{
				if (TargetDamageResistanceProcessor.isActiveAndEnabled)
				{
					bool checkResistance = TargetDamageResistanceProcessor.CheckPreventKnockback(typedDamages);
					if (checkResistance)
					{
						return false;
					}
				}
			}
			return true;
		}

		/// <summary>
		/// Kills the character, instantiates death effects, handles points, etc
		/// </summary>
		public virtual void Kill()
		{
			if (ImmuneToDamage)
			{
				return;
			}
	        
			if (_character != null)
			{
				// we set its dead state to true
				_character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Dead);
				_character.Reset();

				if (_character.CharacterType == Character.CharacterTypes.Player)
				{
					TopDownEngineEvent.Trigger(TopDownEngineEventTypes.PlayerDeath, _character);
				}
			}
			SetHealth(0);

			// we prevent further damage
			StopAllDamageOverTime();
			DamageDisabled();

			DeathMMFeedbacks?.PlayFeedbacks(this.transform.position);
            
			// Adds points if needed.
			if(PointsWhenDestroyed != 0)
			{
				// we send a new points event for the GameManager to catch (and other classes that may listen to it too)
				TopDownEnginePointEvent.Trigger(PointsMethods.Add, PointsWhenDestroyed);
			}

			if (TargetAnimator != null)
			{
				TargetAnimator.SetTrigger(_deathAnimatorParameter);
			}
			// we make it ignore the collisions from now on
			if (DisableCollisionsOnDeath)
			{
				if (_collider2D != null)
				{
					_collider2D.enabled = false;
				}
				if (_collider3D != null)
				{
					_collider3D.enabled = false;
				}

				// if we have a controller, removes collisions, restores parameters for a potential respawn, and applies a death force
				if (_controller != null)
				{				
					_controller.CollisionsOff();						
				}

				if (DisableChildCollisionsOnDeath)
				{
					foreach (Collider2D collider in this.gameObject.GetComponentsInChildren<Collider2D>())
					{
						collider.enabled = false;
					}
					foreach (Collider collider in this.gameObject.GetComponentsInChildren<Collider>())
					{
						collider.enabled = false;
					}
				}
			}

			if (ChangeLayerOnDeath)
			{
				gameObject.layer = LayerOnDeath.LayerIndex;
				if (ChangeLayersRecursivelyOnDeath)
				{
					this.transform.ChangeLayersRecursively(LayerOnDeath.LayerIndex);
				}
			}
            
			OnDeath?.Invoke();
			MMLifeCycleEvent.Trigger(this, MMLifeCycleEventTypes.Death);

			if (DisableControllerOnDeath && (_controller != null))
			{
				_controller.enabled = false;
			}

			if (DisableControllerOnDeath && (_characterController != null))
			{
				_characterController.enabled = false;
			}

			if (DisableModelOnDeath && (Model != null))
			{
				Model.SetActive(false);
			}

			if (DelayBeforeDestruction > 0f)
			{
				Invoke ("DestroyObject", DelayBeforeDestruction);
			}
			else
			{
				// finally we destroy the object
				DestroyObject();	
			}
		}

		/// <summary>
		/// Revive this object.s
		/// 复活
		/// </summary>
		public virtual void Revive()
		{
			if (!_initialized)
			{
				return;
			}

			if (_collider2D != null)
			{
				_collider2D.enabled = true;
			}
			if (_collider3D != null)
			{
				_collider3D.enabled = true;
			}
			if (DisableChildCollisionsOnDeath)
			{
				foreach (Collider2D collider in this.gameObject.GetComponentsInChildren<Collider2D>())
				{
					collider.enabled = true;
				}
				foreach (Collider collider in this.gameObject.GetComponentsInChildren<Collider>())
				{
					collider.enabled = true;
				}
			}
			if (ChangeLayerOnDeath)
			{
				gameObject.layer = _initialLayer;
				if (ChangeLayersRecursivelyOnDeath)
				{
					this.transform.ChangeLayersRecursively(_initialLayer);
				}
			}
			if (_characterController != null)
			{
				_characterController.enabled = true;
			}
			if (_controller != null)
			{
				_controller.enabled = true;
				_controller.CollisionsOn();
				_controller.Reset();
			}
			if (_character != null)
			{
				_character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
			}
			if (ResetColorOnRevive && (_renderer != null))
			{
				if (UseMaterialPropertyBlocks)
				{
					_renderer.GetPropertyBlock(_propertyBlock);
					_propertyBlock.SetColor(ColorMaterialPropertyName, _initialColor);
					_renderer.SetPropertyBlock(_propertyBlock);    
				}
				else
				{
					_renderer.material.SetColor(ColorMaterialPropertyName, _initialColor);
				}
			}            

			if (RespawnAtInitialLocation)
			{
				transform.position = _initialPosition;
			}
			if (_healthBar != null)
			{
				_healthBar.Initialization();
			}

			Initialization();
			InitializeCurrentHealth();
			OnRevive?.Invoke();
			MMLifeCycleEvent.Trigger(this, MMLifeCycleEventTypes.Revive);
		}

		/// <summary>
		/// Destroys the object, or tries to, depending on the character's settings
		/// </summary>
		protected virtual void DestroyObject()
		{
			if (_autoRespawn == null)
			{
				if (DestroyOnDeath)
				{
					if (_character != null)
					{
						_character.gameObject.SetActive(false);
					}
					else
					{
						gameObject.SetActive(false);	
					}
				}                
			}
			else
			{
				_autoRespawn.Kill();
			}
		}

		#region HealthManipulationAPIs
		

		/// <summary>
		/// Sets the current health to the specified new value, and updates the health bar
		/// </summary>
		/// <param name="newValue"></param>
		public virtual void SetHealth(float newValue)
		{
			CurrentHealth = newValue;
			UpdateHealthBar(false);
			HealthChangeEvent.Trigger(this, newValue);
		}
		
		/// <summary>
		/// Called when the character gets health (from a stimpack for example)
		/// </summary>
		/// <param name="health">The health the character gets.</param>
		/// <param name="instigator">The thing that gives the character health.</param>
		public virtual void ReceiveHealth(float health,GameObject instigator)
		{
			// this function adds health to the character's Health and prevents it to go above MaxHealth.
			if (MasterHealth != null)
			{
				MasterHealth.SetHealth(Mathf.Min (CurrentHealth + health,MaximumHealth));	
			}
			else
			{
				SetHealth(Mathf.Min (CurrentHealth + health,MaximumHealth));	
			}
			UpdateHealthBar(true);
		}
		
		/// <summary>
		/// Resets the character's health to its max value
		/// </summary>
		public virtual void ResetHealthToMaxHealth()
		{
			SetHealth(MaximumHealth);
		}
		
		/// <summary>
		/// Forces a refresh of the character's health bar
		/// </summary>
		public virtual void UpdateHealthBar(bool show)
		{
			UpdateHealthAnimationParameters();
			
			if (_healthBar != null)
			{
				_healthBar.UpdateBar(CurrentHealth, 0f, MaximumHealth, show);
			}

			if (MasterHealth == null)
			{
				if (_character != null)
				{
					if (_character.CharacterType == Character.CharacterTypes.Player)
					{
						// We update the health bar
						if (GUIManager.HasInstance)
						{
							GUIManager.Instance.UpdateHealthBar(CurrentHealth, 0f, MaximumHealth, _character.PlayerID);
						}
					}
				}    
			}
		}

		protected virtual void UpdateHealthAnimationParameters()
		{
			if (TargetAnimator != null)
			{
				TargetAnimator.SetFloat(_healthAnimatorParameter, CurrentHealth);
				TargetAnimator.SetInteger(_healthAsIntAnimatorParameter, (int)CurrentHealth);
			}
		}

		#endregion
		
		#region DamageDisablingAPIs

		/// <summary>
		/// Prevents the character from taking any damage
		/// </summary>
		public virtual void DamageDisabled()
		{
			Invulnerable = true;
		}

		/// <summary>
		/// Allows the character to take damage
		/// </summary>
		public virtual void DamageEnabled()
		{
			Invulnerable = false;
		}

		/// <summary>
		/// makes the character able to take damage again after the specified delay
		/// </summary>
		/// <returns>The layer collision.</returns>
		public virtual IEnumerator DamageEnabled(float delay)
		{
			yield return new WaitForSeconds (delay);
			Invulnerable = false;
		}

		#endregion
	}
}