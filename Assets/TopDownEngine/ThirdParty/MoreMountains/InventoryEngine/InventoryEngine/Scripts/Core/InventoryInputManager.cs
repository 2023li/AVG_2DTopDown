using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
	using UnityEngine.InputSystem;
#endif

namespace MoreMountains.InventoryEngine
{
	/// <summary>
	/// Example of how you can call an inventory from your game. 
	/// I suggest having your Input and GUI manager classes handle that though.
	/// </summary>
	public class InventoryInputManager : MonoBehaviour, MMEventListener<MMInventoryEvent>
	{

        [Header("目标组件")]
        [MMInformation("绑定背包容器（打开/关闭时显示/隐藏的CanvasGroup）、主背包界面以及打开时显示在背包界面下方的遮罩层。", MMInformationAttribute.InformationType.Info, false)]

        /// The CanvasGroup containing all the elements...
        [MMLabel("背包容器")]
        [Tooltip("包含背包界面所有元素的CanvasGroup（打开/关闭背包时显示/隐藏）")]
        public CanvasGroup TargetInventoryContainer;

        /// The main inventory display
        [MMLabel("背包界面")]
        [Tooltip("主背包显示组件")]
        public InventoryDisplay TargetInventoryDisplay;

        /// The Fader that will be used under it...
        [MMLabel("遮罩层")]
        [Tooltip("背包打开/关闭时显示在界面下方的遮罩CanvasGroup")]
        public CanvasGroup Overlay;

        [Header("遮罩设置")]
        /// the opacity of the overlay when active
        [MMLabel("激活不透明度")]
        [Tooltip("遮罩层激活时的透明度值")]
        public float OverlayActiveOpacity = 0.85f;

        /// the opacity of the overlay when inactive
        [MMLabel("未激活不透明度")]
        [Tooltip("遮罩层未激活时的透明度值")]
        public float OverlayInactiveOpacity = 0f;

        [Header("初始行为")]
        [MMInformation("启用'启动时隐藏'将在游戏开始时自动隐藏背包容器（即使场景中可见），便于设置。", MMInformationAttribute.InformationType.Info, false)]

        /// if this is true, the inventory container...
        [MMLabel("启动时隐藏")]
        [Tooltip("游戏启动时是否自动隐藏背包容器")]
        public bool HideContainerOnStart = true;

        [Header("输入权限")]
        [MMInformation("设置背包系统何时捕获玩家输入：仅在打开时响应或始终响应。", MMInformationAttribute.InformationType.Info, false)]
        /// if this is true, the inventory container...
        [MMLabel("仅开启时输入")]
        [Tooltip("是否仅在背包打开时响应输入操作")]
        public bool InputOnlyWhenOpen = true;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
		
			[Header("Input System Key Mapping")] 

			/// the key used to open/close the inventory
			public InputActionProperty ToggleInventoryKey = new InputActionProperty(
				new InputAction(
					name: "IM_Toggle",
					type: InputActionType.Button, 
					binding: "Keyboard/I", 
					interactions: "Press(behavior=2)"));
			
			/// the key used to open the inventory
			public InputActionProperty OpenInventoryKey = new InputActionProperty(
				new InputAction(
					name: "IM_Open",
					type: InputActionType.Button, 
					interactions: "Press(behavior=2)"));
			
			/// the key used to open the inventory
			public InputActionProperty CloseInventoryKey = new InputActionProperty(
				new InputAction(
					name: "IM_Close",
					type: InputActionType.Button, 
					interactions: "Press(behavior=2)"));

			/// the alt key used to open/close the inventory
			public InputActionProperty CancelKey = new InputActionProperty(
				new InputAction(
					name: "IM_Cancel", 
					type: InputActionType.Button, 
					binding: "Keyboard/escape", 
					interactions: "Press(behavior=2)"));
			
			/// the key used to move an item
			public InputActionProperty MoveKey = new InputActionProperty(
				new InputAction(
					name: "IM_Move", 
					type: InputActionType.Button, 
					binding: "Keyboard/insert", 
					interactions: "Press(behavior=2)"));
			
			/// the key used to equip an item
			public InputActionProperty EquipKey = new InputActionProperty(
				new InputAction(
					name: "IM_Equip", 
					type: InputActionType.Button, 
					binding: "Keyboard/home",
					interactions: "Press(behavior=2)"));
			
			/// the key used to use an item
			public InputActionProperty UseKey = new InputActionProperty(
				new InputAction(
					name: "IM_Use", 
					type: InputActionType.Button, 
					binding: "Keyboard/end",
					interactions: "Press(behavior=2)"));
			
			/// the key used to equip or use an item
			public InputActionProperty EquipOrUseKey = new InputActionProperty(
				new InputAction(
					name: "IM_EquipOrUse", 
					type: InputActionType.Button, 
					binding: "Keyboard/space", 
					interactions: "Press(behavior=2)"));
			
			/// the key used to drop an item
			public InputActionProperty DropKey = new InputActionProperty(
				new InputAction(
					name: "IM_Drop", 
					type: InputActionType.Button,
					binding: "Keyboard/delete",		
					interactions: "Press(behavior=2)"));
			
			/// the key used to go to the next inventory
			public InputActionProperty NextInvKey = new InputActionProperty(
				new InputAction(
					name: "IM_NextInv", 
					type: InputActionType.Button, 
					binding: "Keyboard/pageDown", 
					interactions: "Press(behavior=2)"));
			
			/// the key used to go to the previous inventory
			public InputActionProperty PrevInvKey = new InputActionProperty(
				new InputAction(
					name: "IM_PrevInv", 
					type: InputActionType.Button, 
					binding: "Keyboard/pageUp", 
					interactions: "Press(behavior=2)"));
#else
        [Header("Key Mapping")]
		[MMInformation("Here you need to set the various key bindings you prefer. There are some by default but feel free to change them.", MMInformationAttribute.InformationType.Info, false)]
        /// the key used to open/close the inventory
        [MMLabel("开关背包按键")]
        [Tooltip("打开/关闭背包的主快捷键")]
        public KeyCode ToggleInventoryKey = KeyCode.I;

        /// the alt key used to open/close the inventory
        [MMLabel("开关备选按键")]
        [Tooltip("打开/关闭背包的备选快捷键（手柄按键）")]
        public KeyCode ToggleInventoryAltKey = KeyCode.Joystick1Button6;

        /// the key used to open the inventory
        [MMLabel("打开背包按键")]
        [Tooltip("专门用于打开背包的快捷键")]
        public KeyCode OpenInventoryKey;

        /// the key used to close the inventory
        [MMLabel("关闭背包按键")]
        [Tooltip("专门用于关闭背包的快捷键")]
        public KeyCode CloseInventoryKey;

        /// the alt key used to open/close the inventory
        [MMLabel("取消按键")]
        [Tooltip("取消操作的主快捷键（如关闭背包）")]
        public KeyCode CancelKey = KeyCode.Escape;

        /// the alt key used to open/close the inventory
        [MMLabel("取消备选按键")]
        [Tooltip("取消操作的备选快捷键（手柄按键）")]
        public KeyCode CancelKeyAlt = KeyCode.Joystick1Button7;

        /// the key used to move an item
        [MMLabel("移动物品按键")]
        [Tooltip("移动背包中物品的快捷键")]
        public string MoveKey = "insert";

        /// the alt key used to move an item
        [MMLabel("移动备选按键")]
        [Tooltip("移动物品的备选快捷键（手柄按键）")]
        public string MoveAltKey = "joystick button 2";

        /// the key used to equip an item
        [MMLabel("装备物品按键")]
        [Tooltip("装备背包中物品的快捷键")]
        public string EquipKey = "home";

        /// the alt key used to equip an item
        [MMLabel("装备备选按键")]
        [Tooltip("装备物品的备选快捷键")]
        public string EquipAltKey = "home";

        /// the key used to use an item
        [MMLabel("使用物品按键")]
        [Tooltip("使用背包中物品的快捷键")]
        public string UseKey = "end";

        /// the alt key used to use an item
        [MMLabel("使用备选按键")]
        [Tooltip("使用物品的备选快捷键")]
        public string UseAltKey = "end";

        /// the key used to equip or use an item
        [MMLabel("装备/使用按键")]
        [Tooltip("根据物品类型自动装备或使用的快捷键")]
        public string EquipOrUseKey = "space";

        /// the alt key used to equip or use an item
        [MMLabel("装备/使用备选")]
        [Tooltip("装备/使用物品的备选快捷键（手柄按键）")]
        public string EquipOrUseAltKey = "joystick button 0";

        /// the key used to drop an item
        [MMLabel("丢弃物品按键")]
        [Tooltip("丢弃背包中物品的快捷键")]
        public string DropKey = "delete";

        /// the alt key used to drop an item
        [MMLabel("丢弃备选按键")]
        [Tooltip("丢弃物品的备选快捷键（手柄按键）")]
        public string DropAltKey = "joystick button 1";

        /// the key used to go to the next inventory
        [MMLabel("下一个背包按键")]
        [Tooltip("切换到下一个背包页面的快捷键")]
        public string NextInvKey = "page down";

        /// the alt key used to go to the next inventory
        [MMLabel("下一个备选按键")]
        [Tooltip("切换到下一个背包页面的备选快捷键（手柄按键）")]
        public string NextInvAltKey = "joystick button 4";

        /// the key used to go to the previous inventory
        [MMLabel("上一个背包按键")]
        [Tooltip("切换到上一个背包页面的快捷键")]
        public string PrevInvKey = "page up";

        /// the alt key used to go to the previous inventory
        [MMLabel("上一个备选按键")]
        [Tooltip("切换到上一个背包页面的备选快捷键（手柄按键）")]
        public string PrevInvAltKey = "joystick button 5";
#endif

        [Header("Close Bindings")] 
		/// a list of other inventories that should get force-closed when this one opens
		public List<string> CloseList;

		public enum ManageButtonsModes { Interactable, SetActive }
        
		[Header("Buttons")]
		/// if this is true, the InputManager will change the interactable state of inventory control buttons based on the currently selected slot
		public bool ManageButtons = false;
		/// the selected mode to enable buttons with (interactable will change the button's interactable state, SetActive will enable/disable the button's game object
		[MMCondition("ManageButtons", true)] 
		public ManageButtonsModes ManageButtonsMode = ManageButtonsModes.SetActive;
		/// the button used to equip or use an item
		[MMCondition("ManageButtons", true)]
		public Button EquipUseButton;
		/// the button used to move an item
		[MMCondition("ManageButtons", true)]
		public Button MoveButton;
		/// the button used to drop an item
		[MMCondition("ManageButtons", true)]
		public Button DropButton;
		/// the button used to equip an item
		[MMCondition("ManageButtons", true)]
		public Button EquipButton;
		/// the button used to use an item
		[MMCondition("ManageButtons", true)]
		public Button UseButton;
		/// the button used to unequip an item
		[MMCondition("ManageButtons", true)]
		public Button UnEquipButton;
        
		/// returns the active slot
		public virtual InventorySlot CurrentlySelectedInventorySlot { get; set; }

		[Header("State")] 
		/// if this is true, the associated inventory is open, closed otherwise
		[MMReadOnly]
		public bool InventoryIsOpen;

		protected CanvasGroup _canvasGroup;
		protected GameObject _currentSelection;
		protected InventorySlot _currentInventorySlot;
		protected List<InventoryHotbar> _targetInventoryHotbars;
		protected InventoryDisplay _currentInventoryDisplay;
		private bool _isEquipUseButtonNotNull;
		private bool _isEquipButtonNotNull;
		private bool _isUseButtonNotNull;
		private bool _isUnEquipButtonNotNull;
		private bool _isMoveButtonNotNull;
		private bool _isDropButtonNotNull;
		
		protected bool _toggleInventoryKeyPressed;
		protected bool _openInventoryKeyPressed;
		protected bool _closeInventoryKeyPressed;
		protected bool _cancelKeyPressed;
		protected bool _prevInvKeyPressed;
		protected bool _nextInvKeyPressed;
		protected bool _moveKeyPressed;
		protected bool _equipOrUseKeyPressed;
		protected bool _equipKeyPressed;
		protected bool _useKeyPressed;
		protected bool _dropKeyPressed;
		protected bool _hotbarInputPressed = false;

		/// <summary>
		/// On start, we grab references and prepare our hotbar list
		/// </summary>
		protected virtual void Start()
		{
			_isDropButtonNotNull = DropButton != null;
			_isMoveButtonNotNull = MoveButton != null;
			_isUnEquipButtonNotNull = UnEquipButton != null;
			_isUseButtonNotNull = UseButton != null;
			_isEquipButtonNotNull = EquipButton != null;
			_isEquipUseButtonNotNull = EquipUseButton != null;
			_currentInventoryDisplay = TargetInventoryDisplay;
			InventoryIsOpen = false;
			_targetInventoryHotbars = new List<InventoryHotbar>();
			_canvasGroup = GetComponent<CanvasGroup>();
			foreach (InventoryHotbar go in FindObjectsOfType(typeof(InventoryHotbar)) as InventoryHotbar[])
			{
				_targetInventoryHotbars.Add(go);
			}
			if (HideContainerOnStart)
			{
				if (TargetInventoryContainer != null) { TargetInventoryContainer.alpha = 0; }
				if (Overlay != null) { Overlay.alpha = OverlayInactiveOpacity; }
				EventSystem.current.sendNavigationEvents = false;
				if (_canvasGroup != null)
				{
					_canvasGroup.blocksRaycasts = false;
				}
			}
		}

		/// <summary>
		/// Every frame, we check for input for the inventory, the hotbars and we check the current selection
		/// </summary>
		protected virtual void Update()
		{
			HandleInventoryInput();
			HandleHotbarsInput();
			CheckCurrentlySelectedSlot();
			HandleButtons();
		}

		/// <summary>
		/// Every frame, we check and store what object is currently selected
		/// </summary>
		protected virtual void CheckCurrentlySelectedSlot()
		{
			_currentSelection = EventSystem.current.currentSelectedGameObject;
			if (_currentSelection == null)
			{
				return;
			}
			_currentInventorySlot = _currentSelection.gameObject.MMGetComponentNoAlloc<InventorySlot>();
			if (_currentInventorySlot != null)
			{
				CurrentlySelectedInventorySlot = _currentInventorySlot;
			}
		}

		/// <summary>
		/// Will turn inventory controls interactable or not based on the currently selected slot, if ManageButtons is set to true
		/// </summary>
		protected virtual void HandleButtons()
		{
			if (!ManageButtons)
			{
				return;
			}
            
			if (CurrentlySelectedInventorySlot != null)
			{
				if (_isUseButtonNotNull)
				{
					SetButtonState(UseButton, CurrentlySelectedInventorySlot.Usable() && CurrentlySelectedInventorySlot.UseButtonShouldShow());
				}

				if (_isEquipButtonNotNull)
				{
					SetButtonState(EquipButton, CurrentlySelectedInventorySlot.Equippable() && CurrentlySelectedInventorySlot.EquipButtonShouldShow());
				}

				if (_isEquipUseButtonNotNull)
				{
					SetButtonState(EquipUseButton, (CurrentlySelectedInventorySlot.Usable() ||
					                                CurrentlySelectedInventorySlot.Equippable()) && CurrentlySelectedInventorySlot.EquipUseButtonShouldShow());
				}

				if (_isUnEquipButtonNotNull)
				{
					SetButtonState(UnEquipButton, CurrentlySelectedInventorySlot.Unequippable() && CurrentlySelectedInventorySlot.UnequipButtonShouldShow());
				}

				if (_isMoveButtonNotNull)
				{
					SetButtonState(MoveButton, CurrentlySelectedInventorySlot.Movable() && CurrentlySelectedInventorySlot.MoveButtonShouldShow());
				}

				if (_isDropButtonNotNull)
				{
					SetButtonState(DropButton, CurrentlySelectedInventorySlot.Droppable() && CurrentlySelectedInventorySlot.DropButtonShouldShow());
				}
			}
			else
			{
				SetButtonState(UseButton, false);
				SetButtonState(EquipButton, false);
				SetButtonState(EquipUseButton, false);
				SetButtonState(DropButton, false);
				SetButtonState(MoveButton, false);
				SetButtonState(UnEquipButton, false);
			}
		}

		/// <summary>
		/// An internal method used to turn a button on or off
		/// </summary>
		/// <param name="targetButton"></param>
		/// <param name="state"></param>
		protected virtual void SetButtonState(Button targetButton, bool state)
		{
			if (ManageButtonsMode == ManageButtonsModes.Interactable)
			{
				targetButton.interactable = state;
			}
			else
			{
				targetButton.gameObject.SetActive(state);
			}
		}

		/// <summary>
		/// Opens or closes the inventory panel based on its current status
		/// </summary>
		public virtual void ToggleInventory()
		{
			if (InventoryIsOpen)
			{
				CloseInventory();
			}
			else
			{
				OpenInventory();
			}
		}

		/// <summary>
		/// Opens the inventory panel
		/// </summary>
		public virtual void OpenInventory()
		{
			if (CloseList.Count > 0)
			{
				foreach (string playerID in CloseList)
				{
					MMInventoryEvent.Trigger(MMInventoryEventType.InventoryCloseRequest, null, "", null, 0, 0, playerID);
				}
			}
            
			if (_canvasGroup != null)
			{
				_canvasGroup.blocksRaycasts = true;
			}

			// we open our inventory
			MMInventoryEvent.Trigger(MMInventoryEventType.InventoryOpens, null, TargetInventoryDisplay.TargetInventoryName, TargetInventoryDisplay.TargetInventory.Content[0], 0, 0, TargetInventoryDisplay.PlayerID);
			MMGameEvent.Trigger("inventoryOpens");
			InventoryIsOpen = true;

			StartCoroutine(MMFade.FadeCanvasGroup(TargetInventoryContainer, 0.2f, 1f));
			StartCoroutine(MMFade.FadeCanvasGroup(Overlay, 0.2f, OverlayActiveOpacity));
		}

		/// <summary>
		/// Closes the inventory panel
		/// </summary>
		public virtual void CloseInventory()
		{
			if (_canvasGroup != null)
			{
				_canvasGroup.blocksRaycasts = false;
			}
			// we close our inventory
			MMInventoryEvent.Trigger(MMInventoryEventType.InventoryCloses, null, TargetInventoryDisplay.TargetInventoryName, null, 0, 0, TargetInventoryDisplay.PlayerID);
			MMGameEvent.Trigger("inventoryCloses");
			InventoryIsOpen = false;

			StartCoroutine(MMFade.FadeCanvasGroup(TargetInventoryContainer, 0.2f, 0f));
			StartCoroutine(MMFade.FadeCanvasGroup(Overlay, 0.2f, OverlayInactiveOpacity));
		}

        /// <summary>
        /// Handles the inventory related inputs and acts on them.
        /// 处理与库存相关的输入并据此采取行动。
        /// </summary>
        protected virtual void HandleInventoryInput()
		{
			// if we don't have a current inventory display, we do nothing and exit
			if (_currentInventoryDisplay == null)
			{
				return;
			}
			
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				_toggleInventoryKeyPressed = ToggleInventoryKey.action.WasPressedThisFrame();
				_openInventoryKeyPressed = OpenInventoryKey.action.WasPressedThisFrame();
				_closeInventoryKeyPressed = CloseInventoryKey.action.WasPressedThisFrame();
				_cancelKeyPressed = CancelKey.action.WasPressedThisFrame();
				_prevInvKeyPressed = PrevInvKey.action.WasPressedThisFrame();
				_nextInvKeyPressed = NextInvKey.action.WasPressedThisFrame();
				_moveKeyPressed = MoveKey.action.WasPressedThisFrame();
				_equipOrUseKeyPressed = EquipOrUseKey.action.WasPressedThisFrame();
				_equipKeyPressed = EquipKey.action.WasPressedThisFrame();
				_useKeyPressed = UseKey.action.WasPressedThisFrame();
				_dropKeyPressed = DropKey.action.WasPressedThisFrame();
			#else
				_toggleInventoryKeyPressed = Input.GetKeyDown(ToggleInventoryKey) || Input.GetKeyDown(ToggleInventoryAltKey);
				_openInventoryKeyPressed = Input.GetKeyDown(OpenInventoryKey);
				_closeInventoryKeyPressed = Input.GetKeyDown(CloseInventoryKey);
				_cancelKeyPressed = (Input.GetKeyDown(CancelKey)) || (Input.GetKeyDown(CancelKeyAlt));
				_prevInvKeyPressed = Input.GetKeyDown(PrevInvKey) || Input.GetKeyDown(PrevInvAltKey);
				_nextInvKeyPressed = Input.GetKeyDown(NextInvKey) || Input.GetKeyDown(NextInvAltKey);
				_moveKeyPressed = (Input.GetKeyDown(MoveKey) || Input.GetKeyDown(MoveAltKey));
				_equipOrUseKeyPressed = Input.GetKeyDown(EquipOrUseKey) || Input.GetKeyDown(EquipOrUseAltKey);
				_equipKeyPressed = Input.GetKeyDown(EquipKey) || Input.GetKeyDown(EquipAltKey);
				_useKeyPressed = Input.GetKeyDown(UseKey) || Input.GetKeyDown(UseAltKey);
				_dropKeyPressed = Input.GetKeyDown(DropKey) || Input.GetKeyDown(DropAltKey);
			#endif
			
			// if the user presses the 'toggle inventory' key
			if (_toggleInventoryKeyPressed)
			{
				ToggleInventory();
			}

			if (_openInventoryKeyPressed)
			{
				OpenInventory();
			}

			if (_closeInventoryKeyPressed)
			{
				CloseInventory();
			}

			if (_cancelKeyPressed)
			{
				if (InventoryIsOpen)
				{
					CloseInventory();
				}
			}

			// if we've only authorized input when open, and if the inventory is currently closed, we do nothing and exit
			if (InputOnlyWhenOpen && !InventoryIsOpen)
			{
				return;
			}

			// previous inventory panel
			if (_prevInvKeyPressed)
			{
				if (_currentInventoryDisplay.GoToInventory(-1) != null)
				{
					_currentInventoryDisplay = _currentInventoryDisplay.GoToInventory(-1);
				}
			}

			// next inventory panel
			if (_nextInvKeyPressed)
			{
				if (_currentInventoryDisplay.GoToInventory(1) != null)
				{
					_currentInventoryDisplay = _currentInventoryDisplay.GoToInventory(1);
				}
			}

			// move
			if (_moveKeyPressed)
			{
				if (CurrentlySelectedInventorySlot != null)
				{
					if (CurrentlySelectedInventorySlot.CurrentItem != null && CurrentlySelectedInventorySlot.CurrentItem.DisplayProperties.AllowMoveShortcut)
					{
						CurrentlySelectedInventorySlot.Move();
					}
				}
			}

			// equip or use
			if (_equipOrUseKeyPressed)
			{
				if (CurrentlySelectedInventorySlot.CurrentItem != null && CurrentlySelectedInventorySlot.CurrentItem.DisplayProperties.AllowEquipUseShortcut)
				{
					EquipOrUse();
				}
			}

			// equip
			if (_equipKeyPressed)
			{
				if (CurrentlySelectedInventorySlot != null)
				{
					if (CurrentlySelectedInventorySlot.CurrentItem != null && CurrentlySelectedInventorySlot.CurrentItem.DisplayProperties.AllowEquipShortcut)
					{
						CurrentlySelectedInventorySlot.Equip();
					}
				}
			}

			// use
			if (_useKeyPressed)
			{
				if (CurrentlySelectedInventorySlot != null)
				{
					if (CurrentlySelectedInventorySlot.CurrentItem != null && CurrentlySelectedInventorySlot.CurrentItem.DisplayProperties.AllowUseShortcut)
					{
						CurrentlySelectedInventorySlot.Use();
					}
				}
			}

			// drop
			if (_dropKeyPressed)
			{
				if (CurrentlySelectedInventorySlot != null)
				{
					if (CurrentlySelectedInventorySlot.CurrentItem != null && CurrentlySelectedInventorySlot.CurrentItem.DisplayProperties.AllowDropShortcut)
					{
						CurrentlySelectedInventorySlot.Drop();
					}
				}
			}
		}

		/// <summary>
		/// Checks for hotbar input and acts on it
		/// </summary>
		protected virtual void HandleHotbarsInput()
		{
			if (!InventoryIsOpen)
			{
				foreach (InventoryHotbar hotbar in _targetInventoryHotbars)
				{
					if (hotbar != null)
					{
						#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
						_hotbarInputPressed = hotbar.HotbarInputAction.action.WasPressedThisFrame();
						#else
						_hotbarInputPressed = Input.GetKeyDown(hotbar.HotbarKey) || Input.GetKeyDown(hotbar.HotbarAltKey);
						#endif
						
						if (_hotbarInputPressed)
						{
							hotbar.Action();
						}
					}
				}
			}
		}

		/// <summary>
		/// When pressing the equip/use button, we determine which of the two methods to call
		/// </summary>
		public virtual void EquipOrUse()
		{
			if (CurrentlySelectedInventorySlot.Equippable())
			{
				CurrentlySelectedInventorySlot.Equip();
			}
			if (CurrentlySelectedInventorySlot.Usable())
			{
				CurrentlySelectedInventorySlot.Use();
			}
		}

		public virtual void Equip()
		{
			CurrentlySelectedInventorySlot.Equip();
		}

		public virtual void Use()
		{
			CurrentlySelectedInventorySlot.Use();
		}

		public virtual void UnEquip()
		{
			CurrentlySelectedInventorySlot.UnEquip();
		}

		/// <summary>
		/// Triggers the selected slot's move method
		/// </summary>
		public virtual void Move()
		{
			CurrentlySelectedInventorySlot.Move();
		}

		/// <summary>
		/// Triggers the selected slot's drop method
		/// </summary>
		public virtual void Drop()
		{
			CurrentlySelectedInventorySlot.Drop();
		}

		/// <summary>
		/// Catches MMInventoryEvents and acts on them
		/// </summary>
		/// <param name="inventoryEvent">Inventory event.</param>
		public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			if (inventoryEvent.PlayerID != TargetInventoryDisplay.PlayerID)
			{
				return;
			}
            
			if (inventoryEvent.InventoryEventType == MMInventoryEventType.InventoryCloseRequest)
			{
				CloseInventory();
			}
		}

		/// <summary>
		/// On Enable, we start listening for MMInventoryEvents
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMInventoryEvent>();
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				ToggleInventoryKey.action.Enable();
				OpenInventoryKey.action.Enable();
				CloseInventoryKey.action.Enable();
				CancelKey.action.Enable();
				MoveKey.action.Enable();
				EquipKey.action.Enable();
				UseKey.action.Enable();
				EquipOrUseKey.action.Enable();
				DropKey.action.Enable();
				NextInvKey.action.Enable();
				PrevInvKey.action.Enable();
			#endif
		}

		/// <summary>
		/// On Disable, we stop listening for MMInventoryEvents
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMInventoryEvent>();
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				ToggleInventoryKey.action.Disable();
				OpenInventoryKey.action.Disable();
				CloseInventoryKey.action.Disable();
				CancelKey.action.Disable();
				MoveKey.action.Disable();
				EquipKey.action.Disable();
				UseKey.action.Disable();
				EquipOrUseKey.action.Disable();
				DropKey.action.Disable();
				NextInvKey.action.Disable();
				PrevInvKey.action.Disable();
			#endif
		}
	}
}