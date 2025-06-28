using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
	[System.Serializable]
	public struct AutoPickItem
	{
		public InventoryItem Item;
		public int Quantity;
	}

	/// <summary>
	/// Add this component to a character and it'll be able to control an inventory
	/// Animator parameters : none
	/// </summary>
	[MMHiddenProperties("AbilityStopFeedbacks")]
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Inventory")] 
	public class CharacterInventory : CharacterAbility, MMEventListener<MMInventoryEvent>
	{
		public enum WeaponRotationModes { Normal, AddEmptySlot, AddInitialWeapon }

        [Header("仓库设置")]

        /// 玩家在库存系统中的唯一ID（需与所有库存组件匹配）
        [MMLabel("玩家ID")]
        [Tooltip("玩家在库存系统中的唯一标识（单机游戏可保留Player1）")]
        public string PlayerID = "Player1";

        /// 角色主仓库名称
        [MMLabel("主仓库")]
        [Tooltip("角色主要物品仓库的名称")]
        public string MainInventoryName;


        /// 武器仓库名称
        [MMLabel("武器仓库")]
        [Tooltip("存储角色武器的仓库名称")]
        public string WeaponInventoryName;

        /// 快捷栏仓库名称
        [MMLabel("快捷栏")]
        [Tooltip("角色快捷物品栏的仓库名称")]
        public string HotbarInventoryName;

        /// 仓库参考变换（用于物品掉落定位）
        [MMLabel("参考变换")]
        [Tooltip("仓库参考变换（为空时使用角色自身变换）")]
        public Transform InventoryTransform;

        [Header("武器轮换")]

        /// 武器循环模式
        [MMLabel("循环模式")]
        [Tooltip("普通：循环所有武器 | 空手：包含空手状态 | 初始武器：循环回初始武器")]
        public WeaponRotationModes WeaponRotationMode = WeaponRotationModes.Normal;

        [Header("初始物品")]

        /// 游戏开始时自动添加的物品列表
        [MMLabel("初始物品")]
        [Tooltip("游戏开始时自动添加到角色仓库的物品列表")]
        public AutoPickItem[] AutoPickItems;

        /// 是否仅在主仓库为空时添加初始物品
        [MMLabel("空仓添加")]
        [Tooltip("仅在主仓库为空时添加初始物品")]
        public bool AutoPickOnlyIfMainInventoryIsEmpty;

        [Header("初始装备")]

        /// 游戏开始时自动装备的武器
        [MMLabel("初始武器")]
        [Tooltip("游戏开始时自动装备的武器")]
        public InventoryWeapon AutoEquipWeaponOnStart;

        /// 是否仅在主仓库为空时自动装备
        [MMLabel("主仓空时装备")]
        [Tooltip("仅在主仓库为空时自动装备武器")]
        public bool AutoEquipOnlyIfMainInventoryIsEmpty;

        /// 是否仅在装备仓库为空时自动装备
        [MMLabel("装备仓空时装备")]
        [Tooltip("仅在武器仓库为空时自动装备武器")]
        public bool AutoEquipOnlyIfEquipmentInventoryIsEmpty;

        /// 是否在重生时自动装备
        [MMLabel("重生时装备")]
        [Tooltip("角色重生时自动装备初始武器")]
        public bool AutoEquipOnRespawn = true;

        /// 武器操作能力组件
        [MMLabel("武器操作组件")]
        [Tooltip("控制武器使用的组件（为空时自动查找）")]
        public CharacterHandleWeapon CharacterHandleWeapon;

        public virtual Inventory MainInventory { get; set; }
		public virtual Inventory WeaponInventory { get; set; }
		public virtual Inventory HotbarInventory { get; set; }
		public virtual List<string> AvailableWeaponsIDs => _availableWeaponsIDs;

		protected List<int> _availableWeapons;
		protected List<string> _availableWeaponsIDs;
		protected string _nextWeaponID;
		protected bool _nextFrameWeapon = false;
		protected string _nextFrameWeaponName;
		protected const string _emptySlotWeaponName = "_EmptySlotWeaponName";
		protected const string _initialSlotWeaponName = "_InitialSlotWeaponName";
		protected bool _initialized = false;
		protected int _initializedFrame = -1;

		/// <summary>
		/// On init we setup our ability
		/// </summary>
		protected override void Initialization () 
		{
			base.Initialization();
			Setup ();
		}

		/// <summary>
		/// Grabs all inventories, and fills weapon lists
		/// </summary>
		protected virtual void Setup()
		{
			if (InventoryTransform == null)
			{
				InventoryTransform = this.transform;
			}
			GrabInventories ();
			if (CharacterHandleWeapon == null)
			{
				CharacterHandleWeapon = _character?.FindAbility<CharacterHandleWeapon> ();	
			}
			FillAvailableWeaponsLists ();

			if (_initialized)
			{
				return;
			}

			bool mainInventoryEmpty = true;
			if (MainInventory != null)
			{
				mainInventoryEmpty = MainInventory.NumberOfFilledSlots == 0;
			}
			bool canAutoPick = !(AutoPickOnlyIfMainInventoryIsEmpty && !mainInventoryEmpty);
			bool canAutoEquip = !(AutoEquipOnlyIfMainInventoryIsEmpty && !mainInventoryEmpty);

			if (AutoEquipOnlyIfEquipmentInventoryIsEmpty && (WeaponInventory.NumberOfFilledSlots > 0))
			{
				canAutoEquip = false;
			}
			
			// we auto pick items if needed
			if ((AutoPickItems.Length > 0) && !_initialized && canAutoPick)
			{
				foreach (AutoPickItem item in AutoPickItems)
				{
					MMInventoryEvent.Trigger(MMInventoryEventType.Pick, null, item.Item.TargetInventoryName, item.Item, item.Quantity, 0, PlayerID);
				}
			}
			
			// we auto equip a weapon if needed
			if ((AutoEquipWeaponOnStart != null) && !_initialized && canAutoEquip)
			{
				AutoEquipWeapon();
			}

			_initialized = true;
			_initializedFrame = Time.frameCount;
		}

		protected virtual void AutoEquipWeapon()
		{
			MMInventoryEvent.Trigger(MMInventoryEventType.Pick, null, AutoEquipWeaponOnStart.TargetInventoryName, AutoEquipWeaponOnStart, 1, 0, PlayerID);
			EquipWeapon(AutoEquipWeaponOnStart.ItemID);
		}

		public override void ProcessAbility()
		{
			base.ProcessAbility();
            
			if (_nextFrameWeapon)
			{
				EquipWeapon(_nextFrameWeaponName);
				_nextFrameWeapon = false;
			}
		}

        /// <summary>
        /// Grabs any inventory it can find that matches the names set in the inspector
        /// 抓取它能找到的与检查器中设置的名称匹配的任何库存
        /// </summary>
        protected virtual void GrabInventories()
		{
			Inventory[] inventories = FindObjectsOfType<Inventory>();
			foreach (Inventory inventory in inventories)
			{
				if (inventory.PlayerID != PlayerID)
				{
					continue;
				}
				if ((MainInventory == null) && (inventory.name == MainInventoryName))
				{
					MainInventory = inventory;
				}
				if ((WeaponInventory == null) && (inventory.name == WeaponInventoryName))
				{
					WeaponInventory = inventory;
				}
				if ((HotbarInventory == null) && (inventory.name == HotbarInventoryName))
				{
					HotbarInventory = inventory;
				}
			}
			if (MainInventory != null) { MainInventory.SetOwner (this.gameObject); MainInventory.TargetTransform = InventoryTransform;}
			if (WeaponInventory != null) { WeaponInventory.SetOwner (this.gameObject); WeaponInventory.TargetTransform = InventoryTransform;}
			if (HotbarInventory != null) { HotbarInventory.SetOwner (this.gameObject); HotbarInventory.TargetTransform = InventoryTransform;}
		}

		/// <summary>
		/// On handle input, we watch for the switch weapon button, and switch weapon if needed
		/// </summary>
		protected override void HandleInput()
		{
			if (!AbilityAuthorized
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
			{
				return;
			}
			if (_inputManager.SwitchWeaponButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				SwitchWeapon ();
			}
		}

		/// <summary>
		/// Fills the weapon list. The weapon list will be used to determine what weapon we can switch to
		/// </summary>
		protected virtual void FillAvailableWeaponsLists()
		{
			_availableWeaponsIDs = new List<string> ();
			if ((CharacterHandleWeapon == null) || (WeaponInventory == null))
			{
				return;
			}
			_availableWeapons = MainInventory.InventoryContains (ItemClasses.Weapon);
			foreach (int index in _availableWeapons)
			{
				_availableWeaponsIDs.Add (MainInventory.Content [index].ItemID);
			}
			if (!InventoryItem.IsNull(WeaponInventory.Content[0]))
			{
				if ((MainInventory.InventoryContains(WeaponInventory.Content[0].ItemID).Count <= 0) ||
				    WeaponInventory.Content[0].MoveWhenEquipped)
				{
					_availableWeaponsIDs.Add(WeaponInventory.Content[0].ItemID);
				}
			}

			_availableWeaponsIDs.Sort ();
		}

		/// <summary>
		/// Determines the name of the next weapon in line
		/// </summary>
		protected virtual void DetermineNextWeaponName ()
		{
			if (InventoryItem.IsNull(WeaponInventory.Content[0]))
			{
				_nextWeaponID = _availableWeaponsIDs [0];
				return;
			}

			if ((_nextWeaponID == _emptySlotWeaponName) || (_nextWeaponID == _initialSlotWeaponName))
			{
				_nextWeaponID = _availableWeaponsIDs[0];
				return;
			}

			for (int i = 0; i < _availableWeaponsIDs.Count; i++)
			{
				if (_availableWeaponsIDs[i] == WeaponInventory.Content[0].ItemID)
				{
					if (i == _availableWeaponsIDs.Count - 1)
					{
						switch (WeaponRotationMode)
						{
							case WeaponRotationModes.AddEmptySlot:
								_nextWeaponID = _emptySlotWeaponName;
								return;
							case WeaponRotationModes.AddInitialWeapon:
								_nextWeaponID = _initialSlotWeaponName;
								return;
						}

						_nextWeaponID = _availableWeaponsIDs [0];
					}
					else
					{
						_nextWeaponID = _availableWeaponsIDs [i+1];
					}
				}
			}
		}

		/// <summary>
		/// Equips the weapon with the name passed in parameters
		/// </summary>
		/// <param name="weaponID"></param>
		public virtual void EquipWeapon(string weaponID)
		{
			if ((weaponID == _emptySlotWeaponName) && (CharacterHandleWeapon != null))
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.UnEquipRequest, null, WeaponInventoryName, WeaponInventory.Content[0], 0, 0, PlayerID);
				CharacterHandleWeapon.ChangeWeapon(null, _emptySlotWeaponName, false);
				MMInventoryEvent.Trigger(MMInventoryEventType.Redraw, null, WeaponInventory.name, null, 0, 0, PlayerID);
				return;
			}

			if ((weaponID == _initialSlotWeaponName) && (CharacterHandleWeapon != null))
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.UnEquipRequest, null, WeaponInventoryName, WeaponInventory.Content[0], 0, 0, PlayerID);
				CharacterHandleWeapon.ChangeWeapon(CharacterHandleWeapon.InitialWeapon, _initialSlotWeaponName, false);
				MMInventoryEvent.Trigger(MMInventoryEventType.Redraw, null, WeaponInventory.name, null, 0, 0, PlayerID);
				return;
			}

			Debug.Log(MainInventory == null);

			for (int i = 0; i < MainInventory.Content.Length ; i++)
			{
				if (InventoryItem.IsNull(MainInventory.Content[i]))
				{
					continue;
				}
				if (MainInventory.Content[i].ItemID == weaponID)
				{
					MMInventoryEvent.Trigger(MMInventoryEventType.EquipRequest, null, MainInventory.name, MainInventory.Content[i], 0, i, PlayerID);
					break;
				}
			}
		}

		/// <summary>
		/// Switches to the next weapon in line
		/// </summary>
		protected virtual void SwitchWeapon()
		{
			// if there's no character handle weapon component, we can't switch weapon, we do nothing and exit
			if ((CharacterHandleWeapon == null) || (WeaponInventory == null))
			{
				return;
			}

			FillAvailableWeaponsLists ();

			// if we only have 0 or 1 weapon, there's nothing to switch, we do nothing and exit
			if (_availableWeaponsIDs.Count <= 0)
			{
				return;
			}

			DetermineNextWeaponName ();
			EquipWeapon (_nextWeaponID);
			PlayAbilityStartFeedbacks();
			PlayAbilityStartSfx();
		}

		/// <summary>
		/// Watches for InventoryLoaded events
		/// When an inventory gets loaded, if it's our WeaponInventory, we check if there's already a weapon equipped, and if yes, we equip it
		/// </summary>
		/// <param name="inventoryEvent">Inventory event.</param>
		public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			if (inventoryEvent.InventoryEventType == MMInventoryEventType.InventoryLoaded)
			{
				if (inventoryEvent.TargetInventoryName == WeaponInventoryName)
				{
					this.Setup ();
					if (WeaponInventory != null)
					{
						if (!InventoryItem.IsNull (WeaponInventory.Content [0]))
						{
							CharacterHandleWeapon.Setup ();
							WeaponInventory.Content [0].Equip (PlayerID);
						}
					}
				}
			}
			if (inventoryEvent.InventoryEventType == MMInventoryEventType.Pick)
			{
				bool isSubclass = (inventoryEvent.EventItem.GetType().IsSubclassOf(typeof(InventoryWeapon)));
				bool isClass = (inventoryEvent.EventItem.GetType() == typeof(InventoryWeapon));
				if (isClass || isSubclass)
				{
					InventoryWeapon inventoryWeapon = (InventoryWeapon)inventoryEvent.EventItem;
					switch (inventoryWeapon.AutoEquipMode)
					{
						case InventoryWeapon.AutoEquipModes.NoAutoEquip:
							// we do nothing
							break;

						case InventoryWeapon.AutoEquipModes.AutoEquip:
							_nextFrameWeapon = true;
							_nextFrameWeaponName = inventoryEvent.EventItem.ItemID;
							break;

						case InventoryWeapon.AutoEquipModes.AutoEquipIfEmptyHanded:
							if (CharacterHandleWeapon.CurrentWeapon == null)
							{
								_nextFrameWeapon = true;
								_nextFrameWeaponName = inventoryEvent.EventItem.ItemID;
							}
							break;
					}
				}
			}
		}
		
		protected override void OnRespawn()
		{
			if (_initializedFrame == Time.frameCount)
			{
				return;
			}
			
			if ((AutoEquipWeaponOnStart == null) || !AutoEquipOnRespawn || (MainInventory == null) || (WeaponInventory == null))
			{
				return;
			}
			
			MMInventoryEvent.Trigger(MMInventoryEventType.Destroy, null, MainInventoryName, AutoEquipWeaponOnStart, 1, 0, PlayerID);
			AutoEquipWeapon();
		}

		protected override void OnDeath()
		{
			base.OnDeath();
			if (WeaponInventory != null)
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.UnEquipRequest, null, WeaponInventoryName, WeaponInventory.Content[0], 0, 0, PlayerID);
			}            
		}

		/// <summary>
		/// On enable, we start listening for MMGameEvents. You may want to extend that to listen to other types of events.
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();
			this.MMEventStartListening<MMInventoryEvent>();
		}

		/// <summary>
		/// On disable, we stop listening for MMGameEvents. You may want to extend that to stop listening to other types of events.
		/// </summary>
		protected override void OnDisable()
		{
			base.OnDisable ();
			this.MMEventStopListening<MMInventoryEvent>();
		}
	}
}