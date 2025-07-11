﻿using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
using System.Collections.Generic;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// Extend this class to activate something when a button is pressed in a certain zone
	/// </summary>
	[AddComponentMenu("TopDown Engine/Environment/Button Activated")]
	public class ButtonActivated : TopDownMonoBehaviour 
	{
		public enum ButtonActivatedRequirements { Character, ButtonActivator, Either, None }
		public enum InputTypes { Default, Button, Key }

        [MMInspectorGroup("Requirements", true, 10)]
        [MMInformation("定义区域交互的基本需求：是否需要按钮激活能力？是否仅限玩家交互？", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]

        /// if this is true, objects with a ButtonActivator...
        [MMLabel("按钮激活需求")]
        [Tooltip("是否允许带ButtonActivator组件的对象交互")]
        public ButtonActivatedRequirements ButtonActivatedRequirement = ButtonActivatedRequirements.Either;

        /// if this is true, this can only...
        [MMLabel("仅限玩家")]
        [Tooltip("是否仅限玩家角色进行交互")]
        public bool RequiresPlayerType = true;

        /// if this is true, this zone can...
        [MMLabel("需要激活能力")]
        [Tooltip("是否要求角色具备按钮激活能力")]
        public bool RequiresButtonActivationAbility = true;

        [MMInspectorGroup("Activation Conditions", true, 11)]
        [MMInformation("配置激活条件：自动激活、需地面状态或完全禁用激活", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]

        /// if this is false, the zone...
        [MMLabel("可激活")]
        [Tooltip("如果关闭，该区域将完全不可激活")]
        public bool Activable = true;

        /// if true, the zone will...
        [MMLabel("自动激活")]
        [Tooltip("是否无需按钮操作自动激活")]
        public bool AutoActivation = false;

        /// the delay, in seconds...
        [MMLabel("自动激活延迟")]
        [Tooltip("自动激活所需持续停留时间（秒）")]
        [MMCondition("AutoActivation", true)]
        public float AutoActivationDelay = 0f;

        /// if this is true, exiting...
        [MMLabel("退出重置延迟")]
        [Tooltip("离开区域时是否重置自动激活计时")]
        [MMCondition("AutoActivation", true)]
        public bool AutoActivationDelayResetsOnExit = true;

        /// if this is set to false...
        [MMLabel("需地面状态")]
        [Tooltip("是否只能在落地时激活")]
        public bool CanOnlyActivateIfGrounded = false;

        /// Set this to true...
        [MMLabel("更新角色状态")]
        [Tooltip("是否通知CharacterBehaviorState玩家进入")]
        public bool ShouldUpdateState = true;

        /// if this is true, enter...
        [MMLabel("单一激活")]
        [Tooltip("是否同时只允许一个对象激活")]
        public bool OnlyOneActivationAtOnce = true;

        /// a layermask with all...
        [MMLabel("交互层级")]
        [Tooltip("可交互对象的层级遮罩")]
        public LayerMask TargetLayerMask = ~0;

        [MMInspectorGroup("Number of Activations", true, 12)]
        [MMInformation("配置激活次数限制：无限次或有限次数，可设置使用间隔时间", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]

        /// if this is set to false...
        [MMLabel("无限激活")]
        [Tooltip("开启后激活次数不受限制")]
        public bool UnlimitedActivations = true;

        /// the number of times...
        [MMLabel("最大激活次数")]
        [Tooltip("允许的最大交互次数")]
        public int MaxNumberOfActivations = 0;

        /// the amount of remaining...
        [MMLabel("剩余激活次数")]
        [Tooltip("当前剩余的可用激活次数")]
        [MMReadOnly]
        public int NumberOfActivationsLeft;

        /// the delay (in seconds)...
        [MMLabel("使用间隔")]
        [Tooltip("两次激活间的最小间隔时间（秒）")]
        public float DelayBetweenUses = 0f;

        /// if this is true, the zone...
        [MMLabel("用后禁用")]
        [Tooltip("最终使用后是否自动禁用区域")]
        public bool DisableAfterUse = false;

        [MMInspectorGroup("Input", true, 13)]

        /// the selected input type...
        [MMLabel("输入类型")]
        [Tooltip("选择输入方式（默认/按钮/按键）")]
        public InputTypes InputType = InputTypes.Default;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
    /// the input action...
    [MMLabel("输入系统动作")]
    [Tooltip("使用输入系统定义的操作")]
    public InputActionProperty InputSystemAction = new InputActionProperty(
        new InputAction(
            name: "ButtonActivatedAction",
            type: InputActionType.Button, 
            binding: "Keyboard/space", 
            interactions: "Press(behavior=2)"));
#else
        /// the selected button...
        [MMLabel("按钮名称")]
        [Tooltip("使用的按钮名称（旧输入系统）")]
        [MMEnumCondition("InputType", (int)InputTypes.Button)]
        public string InputButton = "Interact";

        /// the key used...
        [MMLabel("按键绑定")]
        [Tooltip("使用的键盘按键（旧输入系统）")]
        [MMEnumCondition("InputType", (int)InputTypes.Key)]
        public KeyCode InputKey = KeyCode.Space;
#endif

        [MMInspectorGroup("Animation", true, 14)]

        /// an (absolutely optional)...
        [MMLabel("动画触发器")]
        [Tooltip("激活时触发的动画参数名称")]
        public string AnimationTriggerParameterName;

        [MMInspectorGroup("Visual Prompt", true, 15)]
        [MMInformation("配置视觉提示：显示交互按钮提示及其样式", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]

        /// if this is true, a...
        [MMLabel("启用提示")]
        [Tooltip("是否显示交互按钮提示")]
        public bool UseVisualPrompt = true;

        /// the gameobject to...
        [MMLabel("提示预制体")]
        [Tooltip("按钮提示的预制体对象")]
        [MMCondition("UseVisualPrompt", true)]
        public ButtonPrompt ButtonPromptPrefab;

        /// the text to display...
        [MMLabel("提示文本")]
        [Tooltip("按钮上显示的文字")]
        [MMCondition("UseVisualPrompt", true)]
        public string ButtonPromptText = "A";

        /// the text to display...
        [MMLabel("提示颜色")]
        [Tooltip("按钮背景颜色")]
        [MMCondition("UseVisualPrompt", true)]
        public Color ButtonPromptColor = MMColors.LawnGreen;

        /// the color for...
        [MMLabel("文字颜色")]
        [Tooltip("按钮文本颜色")]
        [MMCondition("UseVisualPrompt", true)]
        public Color ButtonPromptTextColor = MMColors.White;

        /// If true, the "buttonA"...
        [MMLabel("常显提示")]
        [Tooltip("是否始终显示按钮提示")]
        [MMCondition("UseVisualPrompt", true)]
        public bool AlwaysShowPrompt = true;

        /// If true, the "buttonA"...
        [MMLabel("碰撞时显示")]
        [Tooltip("玩家进入区域时显示提示")]
        [MMCondition("UseVisualPrompt", true)]
        public bool ShowPromptWhenColliding = true;

        /// If true, the prompt...
        [MMLabel("用后隐藏")]
        [Tooltip("激活后自动隐藏提示")]
        [MMCondition("UseVisualPrompt", true)]
        public bool HidePromptAfterUse = false;

        /// the position of...
        [MMLabel("提示相对位置")]
        [Tooltip("提示相对于对象中心的位置偏移")]
        [MMCondition("UseVisualPrompt", true)]
        public Vector3 PromptRelativePosition = Vector3.zero;

        /// the rotation of...
        [MMLabel("提示旋转")]
        [Tooltip("提示对象的旋转角度")]
        [MMCondition("UseVisualPrompt", true)]
        public Vector3 PromptRotation = Vector3.zero;

        [MMInspectorGroup("Feedbacks", true, 16)]

        /// a feedback to play...
        [MMLabel("激活反馈")]
        [Tooltip("成功激活时播放的反馈效果")]
        public MMFeedbacks ActivationFeedback;

        /// a feedback to play...
        [MMLabel("拒绝反馈")]
        [Tooltip("激活被拒绝时播放的反馈")]
        public MMFeedbacks DeniedFeedback;

        /// a feedback to play...
        [MMLabel("进入反馈")]
        [Tooltip("玩家进入区域时播放的反馈")]
        public MMFeedbacks EnterFeedback;

        /// a feedback to play...
        [MMLabel("退出反馈")]
        [Tooltip("玩家离开区域时播放的反馈")]
        public MMFeedbacks ExitFeedback;

        [MMInspectorGroup("Actions", true, 17)]

        /// a UnityEvent to...
        [Tooltip("区域被激活时触发的事件")]
        public UnityEvent OnActivation;

        /// a UnityEvent to...
        [Tooltip("玩家离开区域时触发的事件")]
        public UnityEvent OnExit;

        /// a UnityEvent to...
        [Tooltip("玩家在区域内持续触发的事件")]
        public UnityEvent OnStay;

		[Space(10)]

        protected Animator _buttonPromptAnimator;
		protected ButtonPrompt _buttonPrompt;
		protected Collider _collider;
		protected Collider2D _collider2D;
		protected bool _promptHiddenForever = false;
		protected CharacterButtonActivation _characterButtonActivation;
		protected float _lastActivationTimestamp;
		protected List<GameObject> _collidingObjects;
		protected Character _currentCharacter;
		protected bool _staying = false;
		protected Coroutine _autoActivationCoroutine;
        
		public virtual bool AutoActivationInProgress { get; set; }
		public virtual float AutoActivationStartedAt { get; set; }
		public bool InputActionPerformed
		{
			get
			{
				#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
					return InputSystemAction.action.WasPressedThisFrame();
				#else
					return false;
				#endif
			}
		}

		/// <summary>
		/// On Enable, we initialize our ButtonActivated zone
		/// </summary>
		protected virtual void OnEnable()
		{
			Initialization ();
		}

		/// <summary>
		/// Grabs components and shows prompt if needed
		/// </summary>
		public virtual void Initialization()
		{
			_collider = this.gameObject.GetComponent<Collider>();
			_collider2D = this.gameObject.GetComponent<Collider2D>();
			NumberOfActivationsLeft = MaxNumberOfActivations;
			_collidingObjects = new List<GameObject>();

			ActivationFeedback?.Initialization(this.gameObject);
			DeniedFeedback?.Initialization(this.gameObject);
			EnterFeedback?.Initialization(this.gameObject);
			ExitFeedback?.Initialization(this.gameObject);

			if (AlwaysShowPrompt)
			{
				ShowPrompt();
			}
			
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				InputSystemAction.action.Enable();
			#endif
		}
		
		/// <summary>
		/// On disable we disable our input action if needed
		/// </summary>
		protected virtual void OnDisable()
		{
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				InputSystemAction.action.Disable();
			#endif
		}

		protected virtual IEnumerator TriggerButtonActionCo()
		{
			if (AutoActivationDelay <= 0f)
			{
				TriggerButtonAction();
				yield break;
			}
			else
			{
				AutoActivationInProgress = true;
				AutoActivationStartedAt = Time.time;
				yield return MMCoroutine.WaitFor(AutoActivationDelay);
				AutoActivationInProgress = false;
				TriggerButtonAction();
				yield break;
			}
		}

		/// <summary>
		/// When the input button is pressed, we check whether or not the zone can be activated, and if yes, trigger ZoneActivated
		/// </summary>
		public virtual void TriggerButtonAction()
		{
			if (!CheckNumberOfUses())
			{
				PromptError();
				return;
			}

			_staying = true;
			ActivateZone();
		}

		public virtual void TriggerExitAction(GameObject collider)
		{
			_staying = false;
			if (OnExit != null)
			{
				OnExit.Invoke();
			}
		}

		/// <summary>
		/// Makes the zone activable
		/// </summary>
		public virtual void MakeActivable()
		{
			Activable = true;
		}

		/// <summary>
		/// Makes the zone unactivable
		/// </summary>
		public virtual void MakeUnactivable()
		{
			Activable = false;
		}

		/// <summary>
		/// Makes the zone activable if it wasn't, unactivable if it was activable.
		/// </summary>
		public virtual void ToggleActivable()
		{
			Activable = !Activable;
		}

		protected virtual void Update()
		{
			if (_staying && (OnStay != null))
			{
				OnStay.Invoke();
			}
		}

		/// <summary>
		/// Activates the zone
		/// </summary>
		protected virtual void ActivateZone()
		{
			if (OnActivation != null)
			{
				OnActivation.Invoke();
			}

			_lastActivationTimestamp = Time.time;

			ActivationFeedback?.PlayFeedbacks(this.transform.position);

			if (HidePromptAfterUse)
			{
				_promptHiddenForever = true;
				HidePrompt();	
			}	
			NumberOfActivationsLeft--;

			if (DisableAfterUse && (NumberOfActivationsLeft <= 0))
			{
				DisableZone();
			}
		}

		/// <summary>
		/// Triggers an error 
		/// </summary>
		public virtual void PromptError()
		{
			if (_buttonPromptAnimator != null)
			{
				_buttonPromptAnimator.SetTrigger("Error");
			}
			DeniedFeedback?.PlayFeedbacks(this.transform.position);
		}

		/// <summary>
		/// Shows the button A prompt.
		/// </summary>
		public virtual void ShowPrompt()
		{
			if (!UseVisualPrompt || _promptHiddenForever || (ButtonPromptPrefab == null))
			{
				return;
			}
            
			// we add a blinking A prompt to the top of the zone
			if (_buttonPrompt == null)
			{
				_buttonPrompt = (ButtonPrompt)Instantiate(ButtonPromptPrefab);
				_buttonPrompt.Initialization();
				_buttonPromptAnimator = _buttonPrompt.gameObject.MMGetComponentNoAlloc<Animator>();
			}
			
			if (_collider != null)
			{
				_buttonPrompt.transform.position = _collider.bounds.center + PromptRelativePosition;
			}
			if (_collider2D != null)
			{
				_buttonPrompt.transform.position = _collider2D.bounds.center + PromptRelativePosition;
			}

			if (_buttonPrompt != null)
			{
				_buttonPrompt.transform.parent = transform;
				_buttonPrompt.transform.localEulerAngles = PromptRotation;
				_buttonPrompt.SetText(ButtonPromptText);
				_buttonPrompt.SetBackgroundColor(ButtonPromptColor);
				_buttonPrompt.SetTextColor(ButtonPromptTextColor);
				_buttonPrompt.Show();
			}
		}

		/// <summary>
		/// Hides the button A prompt.
		/// </summary>
		public virtual void HidePrompt()
		{
			if (_buttonPrompt != null)
			{
				_buttonPrompt.Hide();
			}
		}

		/// <summary>
		/// Disables the button activated zone
		/// </summary>
		public virtual void DisableZone()
		{
			Activable = false;
            
			if (_collider != null)
			{
				_collider.enabled = false;
			}

			if (_collider2D != null)
			{
				_collider2D.enabled = false;
			}
	            
			if (ShouldUpdateState && (_characterButtonActivation != null))
			{
				_characterButtonActivation.InButtonActivatedZone = false;
				_characterButtonActivation.ButtonActivatedZone = null;
			}
		}

		/// <summary>
		/// Enables the button activated zone
		/// </summary>
		public virtual void EnableZone()
		{
			Activable = true;
            
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
		/// Handles enter collision with 2D triggers
		/// </summary>
		/// <param name="collidingObject">Colliding object.</param>
		protected virtual void OnTriggerEnter2D (Collider2D collidingObject)
		{
			TriggerEnter (collidingObject.gameObject);
		}
		/// <summary>
		/// Handles enter collision with 2D triggers
		/// </summary>
		/// <param name="collidingObject">Colliding object.</param>
		protected virtual void OnTriggerExit2D (Collider2D collidingObject)
		{
			TriggerExit (collidingObject.gameObject);
		}
		/// <summary>
		/// Handles enter collision with 2D triggers
		/// </summary>
		/// <param name="collidingObject">Colliding object.</param>
		protected virtual void OnTriggerEnter (Collider collidingObject)
		{
			TriggerEnter (collidingObject.gameObject);
		}
		/// <summary>
		/// Handles enter collision with 2D triggers
		/// </summary>
		/// <param name="collidingObject">Colliding object.</param>
		protected virtual void OnTriggerExit (Collider collidingObject)
		{
			TriggerExit (collidingObject.gameObject);
		}
        
		/// <summary>
		/// Triggered when something collides with the button activated zone
		/// </summary>
		/// <param name="collider">Something colliding with the water.</param>
		protected virtual void TriggerEnter(GameObject collider)
		{            
			if (!CheckConditions(collider))
			{
				return;
			}

			// if we can only activate this zone when grounded, we check if we have a controller and if it's not grounded,
			// we do nothing and exit
			if (CanOnlyActivateIfGrounded)
			{
				if (collider != null)
				{
					TopDownController controller = collider.gameObject.MMGetComponentNoAlloc<TopDownController>();
					if (controller != null)
					{
						if (!controller.Grounded)
						{
							return;
						}
					}
				}
			}

			// at this point the object is colliding and authorized, we add it to our list
			_collidingObjects.Add(collider.gameObject);
			if (!TestForLastObject(collider))
			{
				return;
			}
            
			EnterFeedback?.PlayFeedbacks(this.transform.position);

			if (ShouldUpdateState)
			{
				_characterButtonActivation = collider.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterButtonActivation>();
				if (_characterButtonActivation != null)
				{
					_characterButtonActivation.InButtonActivatedZone = true;
					_characterButtonActivation.ButtonActivatedZone = this;
					_characterButtonActivation.InButtonAutoActivatedZone = AutoActivation;
				}
			}

			if (AutoActivation)
			{
				_autoActivationCoroutine = StartCoroutine(TriggerButtonActionCo());
			}	

			// if we're not already showing the prompt and if the zone can be activated, we show it
			if (ShowPromptWhenColliding)
			{
				ShowPrompt();	
			}
		}

		/// <summary>
		/// Triggered when something exits the water
		/// </summary>
		/// <param name="collider">Something colliding with the dialogue zone.</param>
		protected virtual void TriggerExit(GameObject collider)
		{
			if (!CheckConditions(collider))
			{
				return;
			}

			_collidingObjects.Remove(collider.gameObject);
			if (!TestForLastObject(collider))
			{
				return;
			}
            
			AutoActivationInProgress = false;
			if (_autoActivationCoroutine != null)
			{
				StopCoroutine(_autoActivationCoroutine);
			}

			if (ShouldUpdateState)
			{
				_characterButtonActivation = collider.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterButtonActivation>();
				if (_characterButtonActivation != null)
				{
					_characterButtonActivation.InButtonActivatedZone=false;
					_characterButtonActivation.ButtonActivatedZone=null;		
				}
			}

			ExitFeedback?.PlayFeedbacks(this.transform.position);

			if ((_buttonPrompt!=null) && !AlwaysShowPrompt)
			{
				HidePrompt();	
			}

			TriggerExitAction(collider);
		}

		/// <summary>
		/// Tests if the object exiting our zone is the last remaining one
		/// </summary>
		/// <param name="collider"></param>
		/// <returns></returns>
		protected virtual bool TestForLastObject(GameObject collider)
		{
			if (OnlyOneActivationAtOnce)
			{
				if (_collidingObjects.Count > 0)
				{
					bool lastObject = true;
					foreach (GameObject obj in _collidingObjects)
					{
						if ((obj != null) && (obj != collider))
						{
							lastObject = false;
						}
					}
					return lastObject;
				}                    
			}
			return true;            
		}

		/// <summary>
		/// Checks the remaining number of uses and eventual delay between uses and returns true if the zone can be activated.
		/// </summary>
		/// <returns><c>true</c>, if number of uses was checked, <c>false</c> otherwise.</returns>
		public virtual bool CheckNumberOfUses()
		{
			if (!Activable)
			{
				return false;
			}

			if (Time.time - _lastActivationTimestamp < DelayBetweenUses)
			{
				return false;
			}

			if (UnlimitedActivations)
			{
				return true;
			}

			if (NumberOfActivationsLeft == 0)
			{
				return false;
			}

			if (NumberOfActivationsLeft > 0)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Determines whether or not this zone should be activated
		/// </summary>
		/// <returns><c>true</c>, if conditions was checked, <c>false</c> otherwise.</returns>
		/// <param name="character">Character.</param>
		/// <param name="characterButtonActivation">Character button activation.</param>
		protected virtual bool CheckConditions(GameObject collider)
		{
			if (!MMLayers.LayerInLayerMask(collider.layer, TargetLayerMask))
			{
				return false;
			}
			
			Character character = collider.gameObject.MMGetComponentNoAlloc<Character>();

			switch (ButtonActivatedRequirement)
			{
				case ButtonActivatedRequirements.Character:
					if (character == null)
					{
						return false;
					}
					break;

				case ButtonActivatedRequirements.ButtonActivator:
					if (collider.gameObject.MMGetComponentNoAlloc<ButtonActivator>() == null)
					{
						return false;
					}
					break;

				case ButtonActivatedRequirements.Either:
					if ((character == null) && (collider.gameObject.MMGetComponentNoAlloc<ButtonActivator>() == null))
					{
						return false;
					}
					break;
			}

			if (RequiresPlayerType)
			{
				if (character == null)
				{
					return false;
				}
				if (character.CharacterType != Character.CharacterTypes.Player)
				{
					return false;
				}
			}

			if (RequiresButtonActivationAbility)
			{
				CharacterButtonActivation characterButtonActivation = collider.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterButtonActivation>();
				// we check that the object colliding with the water is actually a TopDown controller and a character
				if (characterButtonActivation == null)
				{
					return false;	
				}
				else
				{
					if (!characterButtonActivation.AbilityAuthorized)
					{
						return false;
					}
				}
			}

			return true;
		}
	}
}