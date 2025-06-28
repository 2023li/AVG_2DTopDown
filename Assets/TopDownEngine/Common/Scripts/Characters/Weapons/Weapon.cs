using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// This base class, meant to be extended (see ProjectileWeapon.cs for an example of that) handles rate of fire (rate of use actually), and ammo reloading
	/// </summary>
	[SelectionBase]
	public class Weapon : MMMonoBehaviour 
	{
        [MMInspectorGroup("ID", true, 7)]


        /// the name of the weapon, only used for debugging
        [MMLabel("武器名称")]
        [Tooltip("武器的名称只用于调试")]
        public string WeaponName;
        /// the possible use modes for the trigger (semi auto : the Player needs to release the trigger to fire again, auto : the Player can hold the trigger to fire repeatedly
        public enum TriggerModes { SemiAuto, Auto }

        /// the possible states the weapon can be in
        public enum WeaponStates { WeaponIdle, WeaponStart, WeaponDelayBeforeUse, WeaponUse, WeaponDelayBetweenUses, WeaponStop, WeaponReloadNeeded, WeaponReloadStart, WeaponReload, WeaponReloadStop, WeaponInterrupted }

        /// whether or not the weapon is currently active
        [MMReadOnly]
        [MMLabel("当前武器是否激活")]
        [Tooltip("该武器当前是否处于激活状态")]
        public bool WeaponCurrentlyActive = true;

        [MMInspectorGroup("使用", true, 10)]
        /// if this is true, this weapon will be able to read input (usually via the CharacterHandleWeapon ability), otherwise player input will be disabled
        [MMLabel("是否接受输入")]
        [Tooltip("如果是true, 这把武器将接受输入(通常是通过CharacterHandleWeapon ability),否则将禁用玩家输入")]
        public bool InputAuthorized = true;

        /// is this weapon on semi or full auto ?
        [MMLabel("触发模式")]
        [Tooltip("触发是半自动还是全自动")]
        public TriggerModes TriggerMode = TriggerModes.Auto;

        /// the delay before use, that will be applied for every shot
        [MMLabel("使用延迟")]
        [Tooltip("使用前的延迟，每次使用都会应用该延迟")]
        public float DelayBeforeUse = 0f;

        /// whether or not the delay before used can be interrupted by releasing the shoot button (if true, releasing the button will cancel the delayed shot)
        [MMLabel("延迟期内可取消")]
        [Tooltip("如果是true，延迟期未发射之前松开发射键可取消")]
        public bool DelayBeforeUseReleaseInterruption = true;

        /// the time (in seconds) between two shots		
        [MMLabel("使用间隔时间")]
        [Tooltip("两次使用之间的时间（以秒为单位）")]
        public float TimeBetweenUses = 1f;

        /// whether or not the time between uses can be interrupted by releasing the shoot button (if true, releasing the button will cancel the time between uses)
        [MMLabel("间隔期间释放可中断")]
        [Tooltip("如果是true，释放射击按钮将取消使用间隔时间")]
        public bool TimeBetweenUsesReleaseInterruption = true;


        [Header("触发模式")]

        /// if this is true, the weapon will activate repeatedly for every shoot request
        [MMLabel("启用连发模式")]
        [Tooltip("如果是true，武器将在每次射击请求时连续激活")]
        public bool UseBurstMode = false;

        /// the amount of 'shots' in a burst sequence
        [MMLabel("连发次数")]
        [Tooltip("连发序列中的射击次数")]
        public int BurstLength = 3;

        /// the time between shots in a burst sequence (in seconds)
        [MMLabel("连发间隔时间")]
        [Tooltip("连发序列中每次射击之间的时间（以秒为单位）")]
        public float BurstTimeBetweenShots = 0.1f;

        [MMInspectorGroup("杂项", true, 11)]

        /// whether or not the weapon is magazine based. If it's not, it'll just take its ammo inside a global pool
        [MMLabel("基于弹匣")]
        [Tooltip("武器是否基于弹匣。如果不是，将从全局弹药库中获取弹药")]
        public bool MagazineBased = false;

        /// the size of the magazine
        [MMLabel("弹匣容量")]
        [Tooltip("弹匣的容量")]
        public int MagazineSize = 30;

        /// if this is true, pressing the fire button when a reload is needed will reload the weapon. Otherwise you'll need to press the reload button
        [MMLabel("自动装填")]
        [Tooltip("如果是true，当需要装填时按下射击按钮将自动装填武器，否则需要按下装填按钮")]
        public bool AutoReload;

        /// if this is true, reload will automatically happen right after the last bullet is shot, without the need for input
        [MMLabel("无输入装填")]
        [Tooltip("如果是true，射击完最后一发弹药后将自动装填，无需输入")]
        public bool NoInputReload = false;

        /// the time it takes to reload the weapon
        [MMLabel("装填时间")]
        [Tooltip("装填武器所需的时间")]
        public float ReloadTime = 2f;

        /// the amount of ammo consumed everytime the weapon fires
        [MMLabel("每次消耗弹药")]
        [Tooltip("每次射击消耗的弹药量")]
        public int AmmoConsumedPerShot = 1;

        /// if this is set to true, the weapon will auto destroy when there's no ammo left
        [MMLabel("空时自动销毁")]
        [Tooltip("如果是true，弹药耗尽时武器将自动销毁")]
        public bool AutoDestroyWhenEmpty;

        /// the delay (in seconds) before weapon destruction if empty
        [MMLabel("自动销毁延迟")]
        [Tooltip("弹药耗尽后武器自动销毁的延迟时间（以秒为单位）")]
        public float AutoDestroyWhenEmptyDelay = 1f;

        /// if this is true, the weapon won't try and reload if the ammo is empty, when using WeaponAmmo
        [MMLabel("弹药空时防止装填")]
        [Tooltip("如果是true，当弹药库为空时，使用WeaponAmmo时将不会尝试装填")]
        public bool PreventReloadIfAmmoEmpty = false;

        /// the current amount of ammo loaded inside the weapon
        [MMReadOnly]
        [MMLabel("当前弹药")]
        [Tooltip("当前武器中装填的弹药")]
        public int CurrentAmmoLoaded = 0;

        [MMInspectorGroup("Position", true, 12)]

        /// an offset that will be applied to the weapon once attached to the center of the WeaponAttachment transform.
        [MMLabel("附加点偏移")]
        [Tooltip("武器附加到WeaponAttachment变换中心时应用的偏移量")]
        public Vector3 WeaponAttachmentOffset = Vector3.zero;

        /// should that weapon be flipped when the character flips?
        [MMLabel("翻转同步角色")]
        [Tooltip("角色翻转时武器是否同步翻转")]
        public bool FlipWeaponOnCharacterFlip = true;

        /// the FlipValue will be used to multiply the model's transform's localscale on flip. Usually it's -1,1,1, but feel free to change it to suit your model's specs
        [MMLabel("右向翻转值")]
        [Tooltip("模型翻转时用于乘算localScale的值（通常为-1,1,1，可根据模型需求修改）")]
        public Vector3 RightFacingFlipValue = new Vector3(1, 1, 1);

        /// the FlipValue will be used to multiply the model's transform's localscale on flip. Usually it's -1,1,1, but feel free to change it to suit your model's specs
        [MMLabel("左向翻转值")]
        [Tooltip("模型翻转时用于乘算localScale的值（通常为-1,1,1，可根据模型需求修改）")]
        public Vector3 LeftFacingFlipValue = new Vector3(-1, 1, 1);

        /// a transform to use as the spawn point for weapon use (if null, only offset will be considered, otherwise the transform without offset)
        [MMLabel("使用生成点")]
        [Tooltip("武器使用时生成点变换（为空时只使用偏移量）")]
        public Transform WeaponUseTransform;

        /// if this is true, the weapon will flip to match the character's orientation
        [MMLabel("启用方向翻转")]
        [Tooltip("是否根据角色朝向翻转武器模型")]
        public bool WeaponShouldFlip = true;

        [MMInspectorGroup("IK", true, 13)]

        /// the transform to which the character's left hand should be attached to
        [MMLabel("左手握持点")]
        [Tooltip("角色左手应连接的变换")]
        public Transform LeftHandHandle;

        /// the transform to which the character's right hand should be attached to
        [MMLabel("右手握持点")]
        [Tooltip("角色右手应连接的变换")]
        public Transform RightHandHandle;

        [MMInspectorGroup("移动修正", true, 14)]

        /// if this is true, a multiplier will be applied to movement while the weapon is active
        [MMLabel("启用移动修正")]
        [Tooltip("当武器激活时是否应用移动速度乘数")]
        public bool ModifyMovementWhileAttacking = false;

        /// the multiplier to apply to movement while attacking
        [MMLabel("移动乘数")]
        [Tooltip("武器使用期间应用的移动速度乘数值")]
        public float MovementMultiplier = 0f;

        /// if this is true all movement will be prevented (even flip) while the weapon is active
        [MMLabel("禁止所有移动")]
        [Tooltip("武器使用期间是否禁止所有移动（包含翻转）")]
        public bool PreventAllMovementWhileInUse = false;

        /// if this is true all aim will be prevented while the weapon is active
        [MMLabel("禁止瞄准")]
        [Tooltip("武器使用期间是否禁止瞄准操作")]
        public bool PreventAllAimWhileInUse = false;

        [MMInspectorGroup("后座力", true, 15)]

        /// the force to apply to push the character back when shooting - positive values will push the character back, negative values will launch it forward, turning that recoil into a thrust
        [MMLabel("后坐力强度")]
        [Tooltip("射击时施加的后坐力大小（正值使角色后退，负值将产生推进效果）")]
        public float RecoilForce = 0f;

        [MMInspectorGroup("动画", true, 16)]

        /// the other animators (other than the Character's) that you want to update every time this weapon gets used
        [MMLabel("额外动画器")]
        [Tooltip("需要随武器使用同步更新的其他动画器（除角色主动画器外）")]
        public List<Animator> Animators;

        /// If this is true, sanity checks will be performed to make sure animator parameters exist before updating them. Turning this to false will increase performance but will throw errors if you're trying to update non existing parameters. Make sure your animator has the required parameters.
        [MMLabel("动画参数检查")]
        [Tooltip("是否执行动画参数安全检查（关闭可提升性能，但需确保参数存在）")]
        public bool PerformAnimatorSanityChecks = false;

        /// if this is true, the weapon's animator(s) will mirror the animation parameter of the owner character (that way your weapon's animator will be able to "know" if the character is walking, jumping, etc)
        [MMLabel("同步角色参数")]
        [Tooltip("是否使武器动画器镜像角色动画参数（感知移动/跳跃等状态）")]
        public bool MirrorCharacterAnimatorParameters = false;

        [MMInspectorGroup("Animation Parameters Names", true, 17)]

        /// the ID of the weapon to pass to the animator
        [MMLabel("武器动画ID")]
        [Tooltip("传递给动画器的武器ID")]
        public int WeaponAnimationID = 0;

        /// the name of the weapon's idle animation parameter : this will be true all the time except when the weapon is being used
        [MMLabel("空闲参数")]
        [Tooltip("武器空闲动画参数（非使用期间始终为真）")]
        public string IdleAnimationParameter;

        /// the name of the weapon's start animation parameter : true at the frame where the weapon starts being used
        [MMLabel("开始参数")]
        [Tooltip("武器启动动画参数（使用瞬间为真）")]
        public string StartAnimationParameter;

        /// the name of the weapon's delay before use animation parameter : true when the weapon has been activated but hasn't been used yet
        [MMLabel("使用前延迟参数")]
        [Tooltip("武器使用前延迟动画参数（激活后未使用时为真）")]
        public string DelayBeforeUseAnimationParameter;

        /// the name of the weapon's single use animation parameter : true at each frame the weapon activates (shoots)
        [MMLabel("单次使用参数")]
        [Tooltip("武器单次使用动画参数（每次激活/射击时为真）")]
        public string SingleUseAnimationParameter;

        /// the name of the weapon's in use animation parameter : true at each frame the weapon has started firing but hasn't stopped yet
        [MMLabel("使用中参数")]
        [Tooltip("武器使用中动画参数（开始射击后到停止前为真）")]
        public string UseAnimationParameter;

        /// the name of the weapon's delay between each use animation parameter : true when the weapon is in use
        [MMLabel("使用间隔参数")]
        [Tooltip("武器使用间隔动画参数（使用期间为真）")]
        public string DelayBetweenUsesAnimationParameter;

        /// the name of the weapon stop animation parameter : true after a shot and before the next one or the weapon's stop 
        [MMLabel("停止参数")]
        [Tooltip("武器停止动画参数（射击结束后到下次射击前为真）")]
        public string StopAnimationParameter;

        /// the name of the weapon reload start animation parameter
        [MMLabel("装填开始参数")]
        [Tooltip("武器装填开始动画参数")]
        public string ReloadStartAnimationParameter;

        /// the name of the weapon reload animation parameter
        [MMLabel("装填中参数")]
        [Tooltip("武器装填中动画参数")]
        public string ReloadAnimationParameter;

        /// the name of the weapon reload end animation parameter
        [MMLabel("装填结束参数")]
        [Tooltip("武器装填结束动画参数")]
        public string ReloadStopAnimationParameter;

        /// the name of the weapon's angle animation parameter
        [MMLabel("武器角度参数")]
        [Tooltip("武器角度动画参数")]
        public string WeaponAngleAnimationParameter;

        /// the name of the weapon's angle animation parameter, adjusted so it's always relative to the direction the character is currently facing
        [MMLabel("相对角度参数")]
        [Tooltip("武器相对角度动画参数（始终基于角色当前朝向）")]
        public string WeaponAngleRelativeAnimationParameter;

        /// the name of the parameter to send to true as long as this weapon is equipped, used or not. While all the other parameters defined here are updated by the Weapon class itself, and passed to the weapon and character, this one will be updated by CharacterHandleWeapon only."
        [MMLabel("装备状态参数")]
        [Tooltip("武器装备状态参数（装备期间始终为真，由CharacterHandleWeapon更新）")]
        public string EquippedAnimationParameter;

        /// the name of the parameter to send to true when the weapon gets interrupted. While all the other parameters defined here are updated by the Weapon class itself, and passed to the weapon and character, this one will be updated by CharacterHandleWeapon only."
        [MMLabel("中断参数")]
        [Tooltip("武器中断状态参数（中断时触发，由CharacterHandleWeapon更新）")]
        public string InterruptedAnimationParameter;

        [MMInspectorGroup("Feedbacks", true, 18)]

        /// the feedback to play when the weapon starts being used
        [MMLabel("使用开始反馈")]
        [Tooltip("武器开始使用时播放的反馈")]
        public MMFeedbacks WeaponStartMMFeedback;

        /// the feedback to play while the weapon is in use
        [MMLabel("使用中反馈")]
        [Tooltip("武器使用期间持续播放的反馈")]
        public MMFeedbacks WeaponUsedMMFeedback;

        /// if set, this feedback will be used randomly instead of WeaponUsedMMFeedback
        [MMLabel("备用使用反馈")]
        [Tooltip("（如果设置）将随机替代使用中反馈")]
        public MMFeedbacks WeaponUsedMMFeedbackAlt;

        /// the feedback to play when the weapon stops being used
        [MMLabel("使用结束反馈")]
        [Tooltip("武器停止使用时播放的反馈")]
        public MMFeedbacks WeaponStopMMFeedback;

        /// the feedback to play when the weapon gets reloaded
        [MMLabel("装填反馈")]
        [Tooltip("武器装填时播放的反馈")]
        public MMFeedbacks WeaponReloadMMFeedback;

        /// the feedback to play when the weapon gets reloaded
        [MMLabel("需装填反馈")]
        [Tooltip("武器需要装填时播放的反馈")]
        public MMFeedbacks WeaponReloadNeededMMFeedback;

        /// the feedback to play when the weapon can't reload as there's no more ammo available. You'll need PreventReloadIfAmmoEmpty to be true for this to work
        [MMLabel("装填失败反馈")]
        [Tooltip("弹药不足无法装填时播放的反馈（需启用PreventReloadIfAmmoEmpty）")]
        public MMFeedbacks WeaponReloadImpossibleMMFeedback;

        [MMInspectorGroup("Settings", true, 19)]

        /// If this is true, the weapon will initialize itself on start, otherwise it'll have to be init manually, usually by the CharacterHandleWeapon class
        [MMLabel("启动时初始化")]
        [Tooltip("如果是true，武器将在启动时自行初始化，否则需手动初始化（通常由CharacterHandleWeapon处理）")]
        public bool InitializeOnStart = false;

        /// whether or not this weapon can be interrupted 
        [MMLabel("可中断")]
        [Tooltip("该武器是否可被中断")]
        public bool Interruptable = false;

        /// the name of the inventory item corresponding to this weapon. Automatically set (if needed) by InventoryEngineWeapon
        public virtual string WeaponID { get; set; }
		/// the weapon's owner
		public virtual Character Owner { get; protected set; }
		/// the weapon's owner's CharacterHandleWeapon component
		public virtual CharacterHandleWeapon CharacterHandleWeapon { get; set; }
		/// if true, the weapon is flipped
		[MMReadOnly]
		[MMLabel("武器反转")]
		[Tooltip("if true, the weapon is flipped right now")]
		public bool Flipped;
		/// the WeaponAmmo component optionnally associated to this weapon
		public virtual WeaponAmmo WeaponAmmo { get; protected set; }
		/// the weapon's state machine
		public MMStateMachine<WeaponStates> WeaponState;

		protected SpriteRenderer _spriteRenderer;
		protected WeaponAim _weaponAim;
		protected float _movementMultiplierStorage = 1f;

		public float MovementMultiplierStorage
		{
			get => _movementMultiplierStorage;
			set => _movementMultiplierStorage = value;
		}
		
		public bool IsComboWeapon { get; set; }
		public bool IsAutoComboWeapon { get; set; }
		
		protected Animator _ownerAnimator;
		protected WeaponPreventShooting _weaponPreventShooting;
		protected float _delayBeforeUseCounter = 0f;
		protected float _delayBetweenUsesCounter = 0f;
		protected float _reloadingCounter = 0f;
		protected bool _triggerReleased = false;
		protected bool _reloading = false;
		protected ComboWeapon _comboWeapon;
		protected TopDownController _controller;
		protected CharacterMovement _characterMovement;
		protected Vector3 _weaponOffset;
		protected Vector3 _weaponAttachmentOffset;
		protected Transform _weaponAttachment;
		protected List<HashSet<int>> _animatorParameters;
		protected HashSet<int> _ownerAnimatorParameters;
		protected bool _controllerIs3D = false;
        
		protected const string _aliveAnimationParameterName = "Alive";
		protected int _idleAnimationParameter;
		protected int _startAnimationParameter;
		protected int _delayBeforeUseAnimationParameter;
		protected int _singleUseAnimationParameter;
		protected int _useAnimationParameter;
		protected int _delayBetweenUsesAnimationParameter;
		protected int _stopAnimationParameter;
		protected int _reloadStartAnimationParameter;
		protected int _reloadAnimationParameter;
		protected int _reloadStopAnimationParameter;
		protected int _weaponAngleAnimationParameter;
		protected int _weaponAngleRelativeAnimationParameter;
		protected int _aliveAnimationParameter;
		protected int _comboInProgressAnimationParameter;
		protected int _equippedAnimationParameter;
		protected int _interruptedAnimationParameter;
		protected float _lastShootRequestAt = -float.MaxValue;
		protected float _lastTurnWeaponOnAt = -float.MaxValue;
		protected bool _movementSpeedMultiplierSet = false;

		/// <summary>
		/// On start we initialize our weapon
		/// </summary>
		protected virtual void Start()
		{
			if (InitializeOnStart)
			{
				Initialization();
			}
		}

		/// <summary>
		/// Initialize this weapon.
		/// </summary>
		public virtual void Initialization()
		{
			Flipped = false;
			_spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
			_comboWeapon = this.gameObject.GetComponent<ComboWeapon>();
			_weaponPreventShooting = this.gameObject.GetComponent<WeaponPreventShooting>();

			WeaponState = new MMStateMachine<WeaponStates>(gameObject, true);
			WeaponState.ChangeState(WeaponStates.WeaponIdle);
			WeaponAmmo = GetComponent<WeaponAmmo>();
			_animatorParameters = new List<HashSet<int>>();
			_weaponAim = GetComponent<WeaponAim>();
			InitializeAnimatorParameters();
			if (WeaponAmmo == null)
			{
				CurrentAmmoLoaded = MagazineSize;
			}
			InitializeFeedbacks();       
		}

		protected virtual void InitializeFeedbacks()
		{
			WeaponStartMMFeedback?.Initialization(this.gameObject);
			WeaponUsedMMFeedback?.Initialization(this.gameObject);
			WeaponUsedMMFeedbackAlt?.Initialization(this.gameObject);
			WeaponStopMMFeedback?.Initialization(this.gameObject);
			WeaponReloadNeededMMFeedback?.Initialization(this.gameObject);
			WeaponReloadMMFeedback?.Initialization(this.gameObject);
		}

		/// <summary>
		/// Initializes the combo weapon, if it's one
		/// </summary>
		public virtual void InitializeComboWeapons()
		{
			IsComboWeapon = false;
			IsAutoComboWeapon = false;
			if (_comboWeapon != null)
			{
				IsComboWeapon = true;
				IsAutoComboWeapon = (_comboWeapon.InputMode == ComboWeapon.InputModes.Auto);
				_comboWeapon.Initialization();
			}
		}

		/// <summary>
		/// Sets the weapon's owner
		/// </summary>
		/// <param name="newOwner">New owner.</param>
		public virtual void SetOwner(Character newOwner, CharacterHandleWeapon handleWeapon)
		{
			Owner = newOwner;
			if (Owner != null)
			{
				CharacterHandleWeapon = handleWeapon;
				_characterMovement = Owner.GetComponent<Character>()?.FindAbility<CharacterMovement>();
				_controller = Owner.GetComponent<TopDownController>();

				_controllerIs3D = Owner.GetComponent<TopDownController3D>() != null;

				if (CharacterHandleWeapon.AutomaticallyBindAnimator)
				{
					if (CharacterHandleWeapon.CharacterAnimator != null)
					{
						_ownerAnimator = CharacterHandleWeapon.CharacterAnimator;
					}
					if (_ownerAnimator == null)
					{
						_ownerAnimator = CharacterHandleWeapon.gameObject.GetComponentInParent<Character>().CharacterAnimator;
					}
					if (_ownerAnimator == null)
					{
						_ownerAnimator = CharacterHandleWeapon.gameObject.GetComponentInParent<Animator>();
					}
				}
			}
		}

		/// <summary>
		/// Called by input, turns the weapon on
		/// </summary>
		public virtual void WeaponInputStart()
		{
			if (_reloading)
			{
				return;
			}

			if (WeaponState.CurrentState == WeaponStates.WeaponIdle)
			{
				_triggerReleased = false;
				TurnWeaponOn();
			}
		}

		/// <summary>
		/// Describes what happens when the weapon's input gets released
		/// </summary>
		public virtual void WeaponInputReleased()
		{
			
		}

        /// <summary>
        /// Describes what happens when the weapon starts
        /// 描述武器启动时会发生什么
        /// </summary>
        public virtual void TurnWeaponOn()
		{
			if (!InputAuthorized && (Time.time - _lastTurnWeaponOnAt < TimeBetweenUses))
			{
				return;
			}

			_lastTurnWeaponOnAt = Time.time;
			
			TriggerWeaponStartFeedback();
			WeaponState.ChangeState(WeaponStates.WeaponStart);
			if ((_characterMovement != null) && (ModifyMovementWhileAttacking))
			{
				_movementMultiplierStorage = _characterMovement.MovementSpeedMultiplier;
				_characterMovement.MovementSpeedMultiplier = MovementMultiplier;
				_movementSpeedMultiplierSet = true;
			}
			if (_comboWeapon != null)
			{
				_comboWeapon.WeaponStarted(this);
			}
			if (PreventAllMovementWhileInUse && (_characterMovement != null) && (_controller != null))
			{
				_characterMovement.SetMovement(Vector2.zero);
				_characterMovement.MovementForbidden = true;
			}
			if (PreventAllAimWhileInUse && (_weaponAim != null))
			{
				_weaponAim.AimControlActive = false;
			}
		}

		/// <summary>
		/// On Update, we check if the weapon is or should be used
		/// </summary>
		protected virtual void Update()
		{
			FlipWeapon();
			ApplyOffset();       
		}

		/// <summary>
		/// On LateUpdate, processes the weapon state
		/// </summary>
		protected virtual void LateUpdate()
		{     
			ProcessWeaponState();
		}

		/// <summary>
		/// Called every lastUpdate, processes the weapon's state machine
		/// </summary>
		protected virtual void ProcessWeaponState()
		{
			if (WeaponState == null) { return; }
			
			UpdateAnimator();

			switch (WeaponState.CurrentState)
			{
				case WeaponStates.WeaponIdle:
					CaseWeaponIdle();
					break;

				case WeaponStates.WeaponStart:
					CaseWeaponStart();
					break;

				case WeaponStates.WeaponDelayBeforeUse:
					CaseWeaponDelayBeforeUse();
					break;

				case WeaponStates.WeaponUse:
					CaseWeaponUse();
					break;

				case WeaponStates.WeaponDelayBetweenUses:
					CaseWeaponDelayBetweenUses();
					break;

				case WeaponStates.WeaponStop:
					CaseWeaponStop();
					break;

				case WeaponStates.WeaponReloadNeeded:
					CaseWeaponReloadNeeded();
					break;

				case WeaponStates.WeaponReloadStart:
					CaseWeaponReloadStart();
					break;

				case WeaponStates.WeaponReload:
					CaseWeaponReload();
					break;

				case WeaponStates.WeaponReloadStop:
					CaseWeaponReloadStop();
					break;

				case WeaponStates.WeaponInterrupted:
					CaseWeaponInterrupted();
					break;
			}
		}

		/// <summary>
		/// If the weapon is idle, we reset the movement multiplier
		/// </summary>
		public virtual void CaseWeaponIdle()
		{
				ResetMovementMultiplier();	
		}

		/// <summary>
		/// When the weapon starts we switch to a delay or shoot based on our weapon's settings
		/// </summary>
		public virtual void CaseWeaponStart()
		{
			if (DelayBeforeUse > 0)
			{
				_delayBeforeUseCounter = DelayBeforeUse;
				WeaponState.ChangeState(WeaponStates.WeaponDelayBeforeUse);
			}
			else
			{
				StartCoroutine(ShootRequestCo());
			}
		}

		/// <summary>
		/// If we're in delay before use, we wait until our delay is passed and then request a shoot
		/// </summary>
		public virtual void CaseWeaponDelayBeforeUse()
		{
			_delayBeforeUseCounter -= Time.deltaTime;
			if (_delayBeforeUseCounter <= 0)
			{
				StartCoroutine(ShootRequestCo());
			}
		}

		/// <summary>
		/// On weapon use we use our weapon then switch to delay between uses
		/// </summary>
		public virtual void CaseWeaponUse()
		{
			WeaponUse();
			_delayBetweenUsesCounter = TimeBetweenUses;
			WeaponState.ChangeState(WeaponStates.WeaponDelayBetweenUses);
		}

		/// <summary>
		/// When in delay between uses, we either turn our weapon off or make a shoot request
		/// </summary>
		public virtual void CaseWeaponDelayBetweenUses()
		{
			if (_triggerReleased && TimeBetweenUsesReleaseInterruption)
			{
				TurnWeaponOff();
				return;
			}
            
			_delayBetweenUsesCounter -= Time.deltaTime;
			if (_delayBetweenUsesCounter <= 0)
			{
				if ((TriggerMode == TriggerModes.Auto) && !_triggerReleased)
				{
					StartCoroutine(ShootRequestCo());
				}
				else
				{
					TurnWeaponOff();
				}
			}
		}

		/// <summary>
		/// On weapon stop, we switch to idle
		/// </summary>
		public virtual void CaseWeaponStop()
		{
			WeaponState.ChangeState(WeaponStates.WeaponIdle);
		}

		/// <summary>
		/// If a reload is needed, we mention it and switch to idle
		/// </summary>
		public virtual void CaseWeaponReloadNeeded()
		{
			ReloadNeeded();
			ResetMovementMultiplier();
			WeaponState.ChangeState(WeaponStates.WeaponIdle);
		}

		/// <summary>
		/// on reload start, we reload the weapon and switch to reload
		/// </summary>
		public virtual void CaseWeaponReloadStart()
		{
			ReloadWeapon();
			_reloadingCounter = ReloadTime;
			WeaponState.ChangeState(WeaponStates.WeaponReload);
		}

		/// <summary>
		/// on reload, we reset our movement multiplier, and switch to reload stop once our reload delay has passed
		/// </summary>
		public virtual void CaseWeaponReload()
		{
			ResetMovementMultiplier();
			_reloadingCounter -= Time.deltaTime;
			if (_reloadingCounter <= 0)
			{
				WeaponState.ChangeState(WeaponStates.WeaponReloadStop);
			}
		}

		/// <summary>
		/// on reload stop, we swtich to idle and load our ammo
		/// </summary>
		public virtual void CaseWeaponReloadStop()
		{
			_reloading = false;
			WeaponState.ChangeState(WeaponStates.WeaponIdle);
			if (WeaponAmmo == null)
			{
				CurrentAmmoLoaded = MagazineSize;
			}
		}

		/// <summary>
		/// on weapon interrupted, we turn our weapon off and switch back to idle
		/// </summary>
		public virtual void CaseWeaponInterrupted()
		{
			TurnWeaponOff();
			ResetMovementMultiplier();
			if ((WeaponState.CurrentState == WeaponStates.WeaponReload)
			    || (WeaponState.CurrentState == WeaponStates.WeaponReloadStart)
			    || (WeaponState.CurrentState == WeaponStates.WeaponReloadStop))
			{
				return;
			}
			WeaponState.ChangeState(WeaponStates.WeaponIdle);
		}

		/// <summary>
		/// Call this method to interrupt the weapon
		/// </summary>
		public virtual void Interrupt()
		{
			if ((WeaponState.CurrentState == WeaponStates.WeaponReload)
			    || (WeaponState.CurrentState == WeaponStates.WeaponReloadStart)
			    || (WeaponState.CurrentState == WeaponStates.WeaponReloadStop))
			{
				return;
			}
			
			if (Interruptable)
			{
				WeaponState.ChangeState(WeaponStates.WeaponInterrupted);
			}
		}
        
        


		/// <summary>
		/// Determines whether or not the weapon can fire
		/// </summary>
		public virtual IEnumerator ShootRequestCo()
		{
			if (Time.time - _lastShootRequestAt < TimeBetweenUses)
			{
				yield break;
			}
			
			int remainingShots = UseBurstMode ? BurstLength : 1;
			float interval = UseBurstMode ? BurstTimeBetweenShots : 1;

			while (remainingShots > 0)
			{
				ShootRequest();
				_lastShootRequestAt = Time.time;
				remainingShots--;
				yield return MMCoroutine.WaitFor(interval);
			}
		}

		public virtual void ShootRequest()
		{
			// if we have a weapon ammo component, we determine if we have enough ammunition to shoot
			if (_reloading)
			{
				return;
			}

			if (_weaponPreventShooting != null)
			{
				if (!_weaponPreventShooting.ShootingAllowed())
				{
					return;
				}
			}

			if (MagazineBased)
			{
				if (WeaponAmmo != null)
				{
					if (WeaponAmmo.EnoughAmmoToFire())
					{
						WeaponState.ChangeState(WeaponStates.WeaponUse);
					}
					else
					{
						if (AutoReload && MagazineBased)
						{
							InitiateReloadWeapon();
						}
						else
						{
							WeaponState.ChangeState(WeaponStates.WeaponReloadNeeded);
						}
					}
				}
				else
				{
					if (CurrentAmmoLoaded > 0)
					{
						WeaponState.ChangeState(WeaponStates.WeaponUse);
						CurrentAmmoLoaded -= AmmoConsumedPerShot;
					}
					else
					{
						if (AutoReload)
						{
							InitiateReloadWeapon();
						}
						else
						{
							WeaponState.ChangeState(WeaponStates.WeaponReloadNeeded);
						}
					}
				}
			}
			else
			{
				if (WeaponAmmo != null)
				{
					if (WeaponAmmo.EnoughAmmoToFire())
					{
						WeaponState.ChangeState(WeaponStates.WeaponUse);
					}
					else
					{
						WeaponState.ChangeState(WeaponStates.WeaponReloadNeeded);
					}
				}
				else
				{
					WeaponState.ChangeState(WeaponStates.WeaponUse);
				}
			}
		}

		/// <summary>
		/// When the weapon is used, plays the corresponding sound
		/// </summary>
		public virtual void WeaponUse()
		{
			ApplyRecoil();
			TriggerWeaponUsedFeedback();
		}

		/// <summary>
		/// Applies recoil if necessary
		/// </summary>
		protected virtual void ApplyRecoil()
		{
			if ((RecoilForce != 0f) && (_controller != null))
			{
				if (Owner != null)
				{
					if (!_controllerIs3D)
					{
						if (Flipped)
						{
							_controller.Impact(this.transform.right, RecoilForce);
						}
						else
						{
							_controller.Impact(-this.transform.right, RecoilForce);
						}
					}
					else
					{
						_controller.Impact(-this.transform.forward, RecoilForce);
					}
				}                
			}
		}

		/// <summary>
		/// Called by input, turns the weapon off if in auto mode
		/// </summary>
		public virtual void WeaponInputStop()
		{
			if (_reloading)
			{
				return;
			}
			_triggerReleased = true;
			if ((_characterMovement != null) && (ModifyMovementWhileAttacking))
			{
				_characterMovement.MovementSpeedMultiplier = _movementMultiplierStorage;
				_movementMultiplierStorage = 1f;
			}
		}

		/// <summary>
		/// Turns the weapon off.
		/// </summary>
		public virtual void TurnWeaponOff()
		{
			if ((WeaponState.CurrentState == WeaponStates.WeaponIdle || WeaponState.CurrentState == WeaponStates.WeaponStop))
			{
				return;
			}
			_triggerReleased = true;

			TriggerWeaponStopFeedback();
			WeaponState.ChangeState(WeaponStates.WeaponStop);
			ResetMovementMultiplier();
			if (_comboWeapon != null)
			{
				_comboWeapon.WeaponStopped(this);
			}
			if (PreventAllMovementWhileInUse && (_characterMovement != null))
			{
				_characterMovement.MovementForbidden = false;
			}
			if (PreventAllAimWhileInUse && (_weaponAim != null))
			{
				_weaponAim.AimControlActive = true;
			}

			if (NoInputReload)
			{
				bool needToReload = false;
				if (WeaponAmmo != null)
				{
					needToReload = !WeaponAmmo.EnoughAmmoToFire();
				}
				else
				{
					needToReload = (CurrentAmmoLoaded <= 0);
				}
                
				if (needToReload)
				{
					InitiateReloadWeapon();
				}
			}
		}

		protected virtual void ResetMovementMultiplier()
		{
			if ((_characterMovement != null) && (ModifyMovementWhileAttacking) && _movementSpeedMultiplierSet)
			{
				_characterMovement.MovementSpeedMultiplier = _movementMultiplierStorage;
				_movementMultiplierStorage = 1f;
				_movementSpeedMultiplierSet = false;
			}
		}

		/// <summary>
		/// Describes what happens when the weapon needs a reload
		/// </summary>
		public virtual void ReloadNeeded()
		{
			TriggerWeaponReloadNeededFeedback();
		}

		/// <summary>
		/// Initiates a reload
		/// </summary>
		public virtual void InitiateReloadWeapon()
		{
			if (PreventReloadIfAmmoEmpty && WeaponAmmo && WeaponAmmo.CurrentAmmoAvailable == 0)
			{
				WeaponReloadImpossibleMMFeedback?.PlayFeedbacks();
				return;
			}
			
			// if we're already reloading, we do nothing and exit
			if (_reloading || !MagazineBased)
			{
				return;
			}
			if (PreventAllMovementWhileInUse && (_characterMovement != null))
			{
				_characterMovement.MovementForbidden = false;
			}
			if (PreventAllAimWhileInUse && (_weaponAim != null))
			{
				_weaponAim.AimControlActive = true;
			}
			WeaponState.ChangeState(WeaponStates.WeaponReloadStart);
			_reloading = true;
		}

		/// <summary>
		/// Reloads the weapon
		/// </summary>
		/// <param name="ammo">Ammo.</param>
		protected virtual void ReloadWeapon()
		{
			if (MagazineBased)
			{
				TriggerWeaponReloadFeedback();
			}
		}

		/// <summary>
		/// Flips the weapon.
		/// </summary>
		public virtual void FlipWeapon()
		{
			if (!WeaponShouldFlip)
			{
				return;
			}
			
			if (Owner == null)
			{
				return;
			}

			if (Owner.Orientation2D == null)
			{
				return;
			}

			if (FlipWeaponOnCharacterFlip)
			{
				Flipped = !Owner.Orientation2D.IsFacingRight;
				if (_spriteRenderer != null)
				{
					_spriteRenderer.flipX = Flipped;
				}
				else
				{
					transform.localScale = Flipped ? LeftFacingFlipValue : RightFacingFlipValue;
				}
			}

			if (_comboWeapon != null)
			{
				_comboWeapon.FlipUnusedWeapons();
			}
		}            
        
		/// <summary>
		/// Destroys the weapon
		/// </summary>
		/// <returns>The destruction.</returns>
		public virtual IEnumerator WeaponDestruction()
		{
			yield return new WaitForSeconds(AutoDestroyWhenEmptyDelay);
			// if we don't have ammo anymore, and need to destroy our weapon, we do it
			TurnWeaponOff();
			Destroy(this.gameObject);

			if (WeaponID != null)
			{
				// we remove it from the inventory
				List<int> weaponList = Owner.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterInventory>().WeaponInventory.InventoryContains(WeaponID);
				if (weaponList.Count > 0)
				{
					Owner.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterInventory>().WeaponInventory.DestroyItem(weaponList[0]);
				}
			}
		}

		/// <summary>
		/// Applies the offset specified in the inspector
		/// </summary>
		public virtual void ApplyOffset()
		{

			if (!WeaponCurrentlyActive)
			{
				return;
			}
            
			_weaponAttachmentOffset = WeaponAttachmentOffset;

			if (Owner == null)
			{
				return;
			}

			if (Owner.Orientation2D != null)
			{
				if (Flipped)
				{
					_weaponAttachmentOffset.x = -WeaponAttachmentOffset.x;
				}
                
				// we apply the offset
				if (transform.parent != null)
				{
					_weaponOffset = transform.parent.position + _weaponAttachmentOffset;
					transform.position = _weaponOffset;
				}
			}
			else
			{
				if (transform.parent != null)
				{
					_weaponOffset = _weaponAttachmentOffset;
					transform.localPosition = _weaponOffset;
				}
			}           
		}

		/// <summary>
		/// Plays the weapon's start sound
		/// </summary>
		protected virtual void TriggerWeaponStartFeedback()
		{
			WeaponStartMMFeedback?.PlayFeedbacks(this.transform.position);
		}

		/// <summary>
		/// Plays the weapon's used sound
		/// </summary>
		protected virtual void TriggerWeaponUsedFeedback()
		{
			if (WeaponUsedMMFeedbackAlt != null)
			{
				int random = MMMaths.RollADice(2);
				if (random > 1)
				{
					WeaponUsedMMFeedbackAlt?.PlayFeedbacks(this.transform.position);
				}
				else
				{
					WeaponUsedMMFeedback?.PlayFeedbacks(this.transform.position);
				}
			}
			else
			{
				WeaponUsedMMFeedback?.PlayFeedbacks(this.transform.position);    
			}
            
		}

		/// <summary>
		/// Plays the weapon's stop sound
		/// </summary>
		protected virtual void TriggerWeaponStopFeedback()
		{            
			WeaponStopMMFeedback?.PlayFeedbacks(this.transform.position);
		}

		/// <summary>
		/// Plays the weapon's reload needed sound
		/// </summary>
		protected virtual void TriggerWeaponReloadNeededFeedback()
		{
			WeaponReloadNeededMMFeedback?.PlayFeedbacks(this.transform.position);
		}

		/// <summary>
		/// Plays the weapon's reload sound
		/// </summary>
		protected virtual void TriggerWeaponReloadFeedback()
		{
			WeaponReloadMMFeedback?.PlayFeedbacks(this.transform.position);
		}

		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		public virtual void InitializeAnimatorParameters()
		{
			if (Animators.Count > 0)
			{
				for (int i = 0; i < Animators.Count; i++)
				{
					_animatorParameters.Add(new HashSet<int>());
					AddParametersToAnimator(Animators[i], _animatorParameters[i]);
					if (!PerformAnimatorSanityChecks)
					{
						Animators[i].logWarnings = false;
					}

					if (MirrorCharacterAnimatorParameters)
					{
						MMAnimatorMirror mirror = Animators[i].gameObject.AddComponent<MMAnimatorMirror>();
						mirror.SourceAnimator = _ownerAnimator;
						mirror.TargetAnimator = Animators[i];
						mirror.Initialization();
					}
				}                
			}            

			if (_ownerAnimator != null)
			{
				_ownerAnimatorParameters = new HashSet<int>();
				AddParametersToAnimator(_ownerAnimator, _ownerAnimatorParameters);
				if (!PerformAnimatorSanityChecks)
				{
					_ownerAnimator.logWarnings = false;
				}
			}
		}

		protected virtual void AddParametersToAnimator(Animator animator, HashSet<int> list)
		{
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, EquippedAnimationParameter, out _equippedAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, WeaponAngleAnimationParameter, out _weaponAngleAnimationParameter, AnimatorControllerParameterType.Float, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, WeaponAngleRelativeAnimationParameter, out _weaponAngleRelativeAnimationParameter, AnimatorControllerParameterType.Float, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, IdleAnimationParameter, out _idleAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, StartAnimationParameter, out _startAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, DelayBeforeUseAnimationParameter, out _delayBeforeUseAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, DelayBetweenUsesAnimationParameter, out _delayBetweenUsesAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, StopAnimationParameter, out _stopAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, ReloadStartAnimationParameter, out _reloadStartAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, ReloadStopAnimationParameter, out _reloadStopAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, ReloadAnimationParameter, out _reloadAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, SingleUseAnimationParameter, out _singleUseAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, UseAnimationParameter, out _useAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, InterruptedAnimationParameter, out _interruptedAnimationParameter, AnimatorControllerParameterType.Bool, list);

			if (_comboWeapon != null)
			{
				MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, _comboWeapon.ComboInProgressAnimationParameter, out _comboInProgressAnimationParameter, AnimatorControllerParameterType.Bool, list);
			}
		}

		/// <summary>
		/// Override this to send parameters to the character's animator. This is called once per cycle, by the Character 
		/// class, after Early, normal and Late process().
		/// </summary>
		public virtual void UpdateAnimator()
		{
			for (int i = 0; i < Animators.Count; i++)
			{
				UpdateAnimator(Animators[i], _animatorParameters[i]);
			}

			if ((_ownerAnimator != null) && (WeaponState != null) && (_ownerAnimatorParameters != null))
			{
				UpdateAnimator(_ownerAnimator, _ownerAnimatorParameters);
			}
		}

		protected virtual void UpdateAnimator(Animator animator, HashSet<int> list)
		{
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _equippedAnimationParameter, true, list);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _idleAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponIdle), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _startAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponStart), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _delayBeforeUseAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBeforeUse), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _useAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBeforeUse || WeaponState.CurrentState == Weapon.WeaponStates.WeaponUse || WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _singleUseAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponUse), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _delayBetweenUsesAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _stopAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponStop), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _reloadStartAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStart), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _reloadAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponReload), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _reloadStopAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStop), list, PerformAnimatorSanityChecks);

			if (WeaponState.CurrentState == Weapon.WeaponStates.WeaponInterrupted)
			{
				MMAnimatorExtensions.UpdateAnimatorTrigger(animator, _interruptedAnimationParameter, list, PerformAnimatorSanityChecks);
			}
			
			if (Owner != null)
			{
				MMAnimatorExtensions.UpdateAnimatorBool(animator, _aliveAnimationParameter, (Owner.ConditionState.CurrentState != CharacterStates.CharacterConditions.Dead), list, PerformAnimatorSanityChecks);
			}

			if (_weaponAim != null)
			{
				MMAnimatorExtensions.UpdateAnimatorFloat(animator, _weaponAngleAnimationParameter, _weaponAim.CurrentAngle, list, PerformAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorFloat(animator, _weaponAngleRelativeAnimationParameter, _weaponAim.CurrentAngleRelative, list, PerformAnimatorSanityChecks);
			}
			else
			{
				MMAnimatorExtensions.UpdateAnimatorFloat(animator, _weaponAngleAnimationParameter, 0f, list, PerformAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorFloat(animator, _weaponAngleRelativeAnimationParameter, 0f, list, PerformAnimatorSanityChecks);
			}

			if (_comboWeapon != null)
			{
				MMAnimatorExtensions.UpdateAnimatorBool(animator, _comboInProgressAnimationParameter, _comboWeapon.ComboInProgress, list, PerformAnimatorSanityChecks);
			}
		}
	}
}