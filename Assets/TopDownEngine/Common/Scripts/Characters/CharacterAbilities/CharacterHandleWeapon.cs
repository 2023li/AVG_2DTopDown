using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// Add this class to a character so it can use weapons
	/// Note that this component will trigger animations (if their parameter is present in the Animator), based on 
	/// the current weapon's Animations
	/// Animator parameters : defined from the Weapon's inspector
	/// </summary>
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Handle Weapon")]
	public class CharacterHandleWeapon : CharacterAbility
	{
		/// This method is only used to display a helpbox text at the beginning of the ability's inspector
		public override string HelpBoxText() { return "该组件将允许你的角色拾取并使用武器。武器的功能在武器类中定义。这里描述的只是握持武器的 “手” 的行为，而非武器本身。在此，你可以为角色设置初始武器，允许拾取武器，并指定武器挂载点（角色内部的一个变换，可以只是一个空的子游戏对象，或者是模型的一个子部件）。"; }

		[Header("Weapon")]

        /// the initial weapon owned by the character
        [MMLabel("初始武器")]
        [Tooltip("角色初始持有的武器")]
        public Weapon InitialWeapon;

        /// if this is set to true, the character can pick up PickableWeapons
        [MMLabel("可拾取武器")]
        [Tooltip("角色是否可以从场景中拾取武器")]
        public bool CanPickupWeapons = true;

        [Header("Feedbacks")]

        /// a feedback that gets triggered at the character level everytime the weapon is used
        [MMLabel("武器使用反馈")]
        [Tooltip("每次使用武器时触发的角色级反馈效果")]
        public MMFeedbacks WeaponUseFeedback;

        [Header("绑定设置")]

        /// the position the weapon will be attached to. If left blank, will be this.transform.
        [MMLabel("武器挂载点")]
        [Tooltip("武器附加的变换位置（为空时使用角色自身变换）")]
        public Transform WeaponAttachment;

        /// the position from which projectiles will be spawned (can be safely left empty)
        [MMLabel("飞行道具生成点")]
        [Tooltip("抛射物生成的起始位置（可空）")]
        public Transform ProjectileSpawn;

        /// if this is true this animator will be automatically bound to the weapon
        [MMLabel("自动绑定动画器")]
        [Tooltip("是否自动将角色动画器绑定到武器")]
        public bool AutomaticallyBindAnimator = true;

        /// the ID of the AmmoDisplay this ability should update
        [MMLabel("弹药显示ID")]
        [Tooltip("要更新的弹药显示组件ID")]
        public int AmmoDisplayID = 0;

        /// if this is true, IK will be automatically setup if possible
        [MMLabel("自动IK设置")]
        [Tooltip("是否自动设置反向动力学（如适用）")]
        public bool AutoIK = true;

        [Header("输入控制")]


        /// if this is true you won't have to release your fire button to auto reload
        [MMLabel("连续按压模式")]
        [Tooltip("无需释放开火按钮即可自动装填")]
        public bool ContinuousPress = false;

        /// whether or not this character getting hit should interrupt its attack (will only work if the weapon is marked as interruptable)
        [MMLabel("受击中断攻击")]
        [Tooltip("角色受击时是否中断当前攻击（需武器支持）")]
        public bool GettingHitInterruptsAttack = false;

        /// whether or not pushing the secondary axis above its threshold should cause the weapon to shoot
        [MMLabel("副轴触发射击")]
        [Tooltip("副轴输入超过阈值时是否触发射击")]
        public bool UseSecondaryAxisThresholdToShoot = false;

        /// if this is true, the ForcedWeaponAimControl mode will be applied to all weapons equipped by this character
        [MMLabel("强制瞄准模式")]
        [Tooltip("为所有装备的武器强制统一瞄准控制模式")]
        public bool ForceWeaponAimControl = false;

        /// if ForceWeaponAimControl is true, the AimControls mode to apply to all weapons equipped by this character
        [MMLabel("强制瞄准类型")]
        [Tooltip("统一应用的武器瞄准控制模式")]
        [MMCondition("ForceWeaponAimControl", true)]
        public WeaponAim.AimControls ForcedWeaponAimControl = WeaponAim.AimControls.PrimaryMovement;

        /// if this is true, the character will continuously fire its weapon
        [MMLabel("强制持续射击")]
        [Tooltip("角色是否持续自动射击")]
        public bool ForceAlwaysShoot = false;

        [Header("Buffering")]

        /// whether or not attack input should be buffered, letting you prepare an attack while another is being performed, making it easier to chain them
        [MMLabel("输入缓冲")]
        [Tooltip("允许在攻击过程中预输入下一指令（实现连招）")]
        public bool BufferInput;

        /// if this is true, every new input will prolong the buffer
        [MMLabel("新输入延长缓冲")]
        [MMCondition("BufferInput", true)]
        [Tooltip("新输入是否延长缓冲时间窗口")]
        public bool NewInputExtendsBuffer;

        /// the maximum duration for the buffer, in seconds
        [MMLabel("最大缓冲时长")]
        [MMCondition("BufferInput", true)]
        [Tooltip("输入缓冲的最大持续时间（秒）")]
        public float MaximumBufferDuration = 0.25f;

        /// if this is true, and if this character is using GridMovement, then input will only be triggered when on a perfect tile
        [MMLabel("需完美格子")]
        [MMCondition("BufferInput", true)]
        [Tooltip("使用网格移动时是否仅在完美格子上触发")]
        public bool RequiresPerfectTile = false;

        [Header("Debug")]

		/// the weapon currently equipped by the Character
		[MMReadOnly]
        [MMLabel("当前武器")]
        [Tooltip("角色当前装备的武器")]
        public Weapon CurrentWeapon;

		/// the ID / index of this CharacterHandleWeapon. This will be used to determine what handle weapon ability should equip a weapon.
		/// If you create more Handle Weapon abilities, make sure to override and increment this  
		public virtual int HandleWeaponID { get { return 1; } }

		/// an animator to update when the weapon is used
		public virtual Animator CharacterAnimator { get; set; }
		/// the weapon's weapon aim component, if it has one
		public virtual WeaponAim WeaponAimComponent { get { return _weaponAim; } }

		public delegate void OnWeaponChangeDelegate();
		/// a delegate you can hook to, to be notified of weapon changes
		public OnWeaponChangeDelegate OnWeaponChange;

		protected float _fireTimer = 0f;
		protected float _secondaryHorizontalMovement;
		protected float _secondaryVerticalMovement;
		protected WeaponAim _weaponAim;
		protected ProjectileWeapon _projectileWeapon;
		protected WeaponIK _weaponIK;
		protected Transform _leftHandTarget = null;
		protected Transform _rightHandTarget = null;
		protected float _bufferEndsAt = 0f;
		protected bool _buffering = false;
		protected const string _weaponEquippedAnimationParameterName = "WeaponEquipped";
		protected const string _weaponEquippedIDAnimationParameterName = "WeaponEquippedID";
		protected int _weaponEquippedAnimationParameter;
		protected int _weaponEquippedIDAnimationParameter;
		protected CharacterGridMovement _characterGridMovement;
		protected List<WeaponModel> _weaponModels;

		/// <summary>
		/// Sets the weapon attachment
		/// </summary>
		protected override void PreInitialization()
		{
			base.PreInitialization();
			// filler if the WeaponAttachment has not been set
			if (WeaponAttachment == null)
			{
				WeaponAttachment = transform;
			}
		}

		// Initialization
		protected override void Initialization()
		{
			base.Initialization();
			Setup();
		}

		/// <summary>
		/// Grabs various components and inits stuff
		/// </summary>
		public virtual void Setup()
		{
			_character = this.gameObject.GetComponentInParent<Character>();
			_characterGridMovement = _character?.FindAbility<CharacterGridMovement>();
			_weaponModels = new List<WeaponModel>();
			foreach (WeaponModel model in _character.gameObject.GetComponentsInChildren<WeaponModel>())
			{
				_weaponModels.Add(model);
			}
			CharacterAnimator = _animator;
			// filler if the WeaponAttachment has not been set
			if (WeaponAttachment == null)
			{
				WeaponAttachment = transform;
			}
			if ((_animator != null) && (AutoIK))
			{
				_weaponIK = _animator.GetComponent<WeaponIK>();
			}
			// we set the initial weapon
			if (InitialWeapon != null)
			{
				if (CurrentWeapon != null)
				{
					if (CurrentWeapon.name != InitialWeapon.name)
					{
						ChangeWeapon(InitialWeapon, InitialWeapon.WeaponName, false);    
					}
				}
				else
				{
					ChangeWeapon(InitialWeapon, InitialWeapon.WeaponName, false);    
				}
			}
		}

		/// <summary>
		/// Every frame we check if it's needed to update the ammo display
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();
			HandleCharacterState();
			HandleFeedbacks();
			UpdateAmmoDisplay();
			HandleBuffer();
		}

		/// <summary>
		/// Checks character state and stops shooting if not in normal state
		/// </summary>
		protected virtual void HandleCharacterState()
		{
			if (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)
			{
				ShootStop();
			}
		}

		/// <summary>
		/// Triggers the weapon used feedback if needed
		/// </summary>
		protected virtual void HandleFeedbacks()
		{
			if (CurrentWeapon != null)
			{
				if (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponUse)
				{
					WeaponUseFeedback?.PlayFeedbacks();
				}
			}
		}

		/// <summary>
		/// Gets input and triggers methods based on what's been pressed
		/// </summary>
		protected override void HandleInput()
		{
			if (!AbilityAuthorized
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)
			    || (CurrentWeapon == null))
			{
				return;
			}

			bool inputAuthorized = true;
			if (CurrentWeapon != null)
			{
				inputAuthorized = CurrentWeapon.InputAuthorized;
			}

			if (ForceAlwaysShoot)
			{
				ShootStart();
			}
			
			if (inputAuthorized && ((_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonDown) || (_inputManager.ShootAxis == MMInput.ButtonStates.ButtonDown)))
			{
				ShootStart();
			}

			bool buttonPressed =
				(_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonPressed) ||
				(_inputManager.ShootAxis == MMInput.ButtonStates.ButtonPressed); 
                
			if (inputAuthorized && ContinuousPress && (CurrentWeapon.TriggerMode == Weapon.TriggerModes.Auto) && buttonPressed)
			{
				ShootStart();
			}
			
			if (inputAuthorized && ContinuousPress && (CurrentWeapon.IsAutoComboWeapon) && buttonPressed)
			{
				ShootStart();
			}
            
			if (_inputManager.ReloadButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				Reload();
			}

			if (inputAuthorized && ((_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonUp) || (_inputManager.ShootAxis == MMInput.ButtonStates.ButtonUp)))
			{
				ShootStop();
				CurrentWeapon.WeaponInputReleased();
			}

			if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses)
			    && ((_inputManager.ShootAxis == MMInput.ButtonStates.Off) && (_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.Off))
			    && !(UseSecondaryAxisThresholdToShoot && (_inputManager.SecondaryMovement.magnitude > _inputManager.Threshold.magnitude)))
			{
				CurrentWeapon.WeaponInputStop();
			}

			if (inputAuthorized && UseSecondaryAxisThresholdToShoot && (_inputManager.SecondaryMovement.magnitude > _inputManager.Threshold.magnitude))
			{
				ShootStart();
			}
		}

		/// <summary>
		/// Triggers an attack if the weapon is idle and an input has been buffered
		/// </summary>
		protected virtual void HandleBuffer()
		{
			if (CurrentWeapon == null)
			{
				return;
			}
            
			// if we are currently buffering an input and if the weapon is now idle
			if (_buffering && (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponIdle))
			{
				// and if our buffer is still valid, we trigger an attack
				if (Time.time < _bufferEndsAt)
				{
					ShootStart();
				}
				else
				{
					_buffering = false;
				}                
			}
		}

		/// <summary>
		/// Causes the character to start shooting
		/// </summary>
		public virtual void ShootStart()
		{
			// if the Shoot action is enabled in the permissions, we continue, if not we do nothing.  If the player is dead we do nothing.
			if (!AbilityAuthorized
			    || (CurrentWeapon == null)
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
			{
				return;
			}

			//  if we've decided to buffer input, and if the weapon is in use right now
			if (BufferInput && (CurrentWeapon.WeaponState.CurrentState != Weapon.WeaponStates.WeaponIdle))
			{
				// if we're not already buffering, or if each new input extends the buffer, we turn our buffering state to true
				ExtendBuffer();
			}

			if (BufferInput && RequiresPerfectTile && (_characterGridMovement != null))            
			{
				if (!_characterGridMovement.PerfectTile)
				{
					ExtendBuffer();
					return;
				}
				else
				{
					_buffering = false;
				}
			}
			PlayAbilityStartFeedbacks();
			CurrentWeapon.WeaponInputStart();
		}

		/// <summary>
		/// Extends the duration of the buffer if needed
		/// </summary>
		protected virtual void ExtendBuffer()
		{
			if (!_buffering || NewInputExtendsBuffer)
			{
				_buffering = true;
				_bufferEndsAt = Time.time + MaximumBufferDuration;
			}
		}

		/// <summary>
		/// Causes the character to stop shooting
		/// </summary>
		public virtual void ShootStop()
		{
			// if the Shoot action is enabled in the permissions, we continue, if not we do nothing
			if (!AbilityAuthorized
			    || (CurrentWeapon == null))
			{
				return;
			}

			if (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponIdle)
			{
				return;
			}

			if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReload)
			    || (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStart)
			    || (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStop))
			{
				return;
			}

			if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBeforeUse) && (!CurrentWeapon.DelayBeforeUseReleaseInterruption))
			{
				return;
			}

			if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses) && (!CurrentWeapon.TimeBetweenUsesReleaseInterruption))
			{
				return;
			}

			if (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponUse) 
			{
				return;
			}

			ForceStop();
		}

		/// <summary>
		/// Forces the weapon to stop 
		/// </summary>
		public virtual void ForceStop()
		{
			StopStartFeedbacks();
			PlayAbilityStopFeedbacks();
			if (CurrentWeapon != null)
			{
				CurrentWeapon.TurnWeaponOff();    
			}
		}

		/// <summary>
		/// Reloads the weapon
		/// </summary>
		public virtual void Reload()
		{
			if (CurrentWeapon != null)
			{
				CurrentWeapon.InitiateReloadWeapon();
			}
		}

		/// <summary>
		/// Changes the character's current weapon to the one passed as a parameter
		/// </summary>
		/// <param name="newWeapon">The new weapon.</param>
		public virtual void ChangeWeapon(Weapon newWeapon, string weaponID, bool combo = false)
		{
			// if the character already has a weapon, we make it stop shooting
			if (CurrentWeapon != null)
			{
				CurrentWeapon.TurnWeaponOff();
				if (!combo)
				{
					ShootStop();

					if (_weaponAim != null) { _weaponAim.RemoveReticle(); }
					if (_character._animator != null)
					{
						AnimatorControllerParameter[] parameters = _character._animator.parameters;
						foreach(AnimatorControllerParameter parameter in parameters)
						{
							if (parameter.name == CurrentWeapon.EquippedAnimationParameter)
							{
								MMAnimatorExtensions.UpdateAnimatorBool(_animator, CurrentWeapon.EquippedAnimationParameter, false);
							}
						}
					}
					Destroy(CurrentWeapon.gameObject);
				}
			}

			if (newWeapon != null)
			{
				InstantiateWeapon(newWeapon, weaponID, combo);
			}
			else
			{
				CurrentWeapon = null;
				HandleWeaponModel(null, null);
			}

			if (OnWeaponChange != null)
			{
				OnWeaponChange();
			}
		}

		/// <summary>
		/// Instantiates the specified weapon
		/// </summary>
		/// <param name="newWeapon"></param>
		/// <param name="weaponID"></param>
		/// <param name="combo"></param>
		protected virtual void InstantiateWeapon(Weapon newWeapon, string weaponID, bool combo = false)
		{
			if (!combo)
			{
				CurrentWeapon = (Weapon)Instantiate(newWeapon, WeaponAttachment.transform.position + newWeapon.WeaponAttachmentOffset, WeaponAttachment.transform.rotation);
			}

			CurrentWeapon.name = newWeapon.name;
			CurrentWeapon.transform.parent = WeaponAttachment.transform;
			CurrentWeapon.transform.localPosition = newWeapon.WeaponAttachmentOffset;
			CurrentWeapon.SetOwner(_character, this);
			CurrentWeapon.WeaponID = weaponID;
			CurrentWeapon.FlipWeapon();
			_weaponAim = CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();

			HandleWeaponAim();

			// we handle (optional) inverse kinematics (IK) 
			HandleWeaponIK();

			// we handle the weapon model
			HandleWeaponModel(newWeapon, weaponID, combo, CurrentWeapon);

			// we turn off the gun's emitters.
			CurrentWeapon.Initialization();
			CurrentWeapon.InitializeComboWeapons();
			CurrentWeapon.InitializeAnimatorParameters();
			InitializeAnimatorParameters();
		}

		/// <summary>
		/// Applies aim if possible
		/// </summary>
		protected virtual void HandleWeaponAim()
		{
			if ((_weaponAim != null) && (_weaponAim.enabled))
			{
				if (ForceWeaponAimControl)
				{
					_weaponAim.AimControl = ForcedWeaponAimControl;
				}
				_weaponAim.ApplyAim();
			}
		}

		/// <summary>
		/// Sets IK handles if needed
		/// </summary>
		protected virtual void HandleWeaponIK()
		{
			if (_weaponIK != null)
			{
				_weaponIK.SetHandles(CurrentWeapon.LeftHandHandle, CurrentWeapon.RightHandHandle);
			}
			_projectileWeapon = CurrentWeapon.gameObject.MMFGetComponentNoAlloc<ProjectileWeapon>();
			if (_projectileWeapon != null)
			{
				_projectileWeapon.SetProjectileSpawnTransform(ProjectileSpawn);
			}
		}

		protected virtual void HandleWeaponModel(Weapon newWeapon, string weaponID, bool combo = false, Weapon weapon = null)
		{
			if (_weaponModels == null)
			{
				return;
			}

			bool handlesSet = false;
			
			foreach (WeaponModel model in _weaponModels)
			{
				if (model.Owner == this)
				{
					model.Hide();	
					if (model.UseIK && !handlesSet)
					{
						_weaponIK.SetHandles(null, null);
					}
				}
				
				if (model.WeaponID == weaponID)
				{
					model.Show(this);
					if (model.UseIK)
					{
						_weaponIK.SetHandles(model.LeftHandHandle, model.RightHandHandle);
						handlesSet = true;
					}
					if (weapon != null)
					{
						if (model.BindFeedbacks)
						{
							weapon.WeaponStartMMFeedback = model.WeaponStartMMFeedback;
							weapon.WeaponUsedMMFeedback = model.WeaponUsedMMFeedback;
							weapon.WeaponStopMMFeedback = model.WeaponStopMMFeedback;
							weapon.WeaponReloadMMFeedback = model.WeaponReloadMMFeedback;
							weapon.WeaponReloadNeededMMFeedback = model.WeaponReloadNeededMMFeedback;
						}
						if (model.AddAnimator)
						{
							weapon.Animators.Add(model.TargetAnimator);
						}
						if (model.OverrideWeaponUseTransform)
						{
							weapon.WeaponUseTransform = model.WeaponUseTransform;
						}
					}
				}
			}
		}

		/// <summary>
		/// Flips the current weapon if needed
		/// </summary>
		public override void Flip()
		{
		}

		/// <summary>
		/// Updates the ammo display bar and text.
		/// </summary>
		public virtual void UpdateAmmoDisplay()
		{
			if ((GUIManager.HasInstance) && (_character.CharacterType == Character.CharacterTypes.Player))
			{
				if (CurrentWeapon == null)
				{
					GUIManager.Instance.SetAmmoDisplays(false, _character.PlayerID, AmmoDisplayID);
					return;
				}

				if (!CurrentWeapon.MagazineBased && (CurrentWeapon.WeaponAmmo == null))
				{
					GUIManager.Instance.SetAmmoDisplays(false, _character.PlayerID, AmmoDisplayID);
					return;
				}

				if (CurrentWeapon.WeaponAmmo == null)
				{
					GUIManager.Instance.SetAmmoDisplays(true, _character.PlayerID, AmmoDisplayID);
					GUIManager.Instance.UpdateAmmoDisplays(CurrentWeapon.MagazineBased, 0, 0, CurrentWeapon.CurrentAmmoLoaded, CurrentWeapon.MagazineSize, _character.PlayerID, AmmoDisplayID, false);
					return;
				}
				else
				{
					GUIManager.Instance.SetAmmoDisplays(true, _character.PlayerID, AmmoDisplayID); 
					GUIManager.Instance.UpdateAmmoDisplays(CurrentWeapon.MagazineBased, CurrentWeapon.WeaponAmmo.CurrentAmmoAvailable + CurrentWeapon.CurrentAmmoLoaded, CurrentWeapon.WeaponAmmo.MaxAmmo, CurrentWeapon.CurrentAmmoLoaded, CurrentWeapon.MagazineSize, _character.PlayerID, AmmoDisplayID, true);
					return;
				}
			}
		}

		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			if (CurrentWeapon == null)
			{ return; }

			RegisterAnimatorParameter(_weaponEquippedAnimationParameterName, AnimatorControllerParameterType.Bool, out _weaponEquippedAnimationParameter);
			RegisterAnimatorParameter(_weaponEquippedIDAnimationParameterName, AnimatorControllerParameterType.Int, out _weaponEquippedIDAnimationParameter);
		}

		/// <summary>
		/// Override this to send parameters to the character's animator. This is called once per cycle, by the Character
		/// class, after Early, normal and Late process().
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _weaponEquippedAnimationParameter, (CurrentWeapon != null), _character._animatorParameters, _character.RunAnimatorSanityChecks);
			if (CurrentWeapon == null)
			{
				MMAnimatorExtensions.UpdateAnimatorInteger(_animator, _weaponEquippedIDAnimationParameter, -1, _character._animatorParameters, _character.RunAnimatorSanityChecks);
				return;
			}
			else
			{
				MMAnimatorExtensions.UpdateAnimatorInteger(_animator, _weaponEquippedIDAnimationParameter, CurrentWeapon.WeaponAnimationID, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			}
		}

		protected override void OnHit()
		{
			base.OnHit();
			if (GettingHitInterruptsAttack && (CurrentWeapon != null))
			{
				CurrentWeapon.Interrupt();
			}
		}

		protected override void OnDeath()
		{
			base.OnDeath();
			ShootStop();
			if (CurrentWeapon != null)
			{
				ChangeWeapon(null, "");
			}
		}

		protected override void OnRespawn()
		{
			base.OnRespawn();
			Setup();
		}
	}
}