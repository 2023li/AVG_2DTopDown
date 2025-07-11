﻿using UnityEngine;
using MoreMountains.Tools;
using System.Collections.Generic;
using System;

namespace MoreMountains.InventoryEngine
{
	[Serializable]
    /// <summary>
    /// Base inventory class. 
    /// Will handle storing items, saving and loading its content, adding items to it, removing items, equipping them, etc.
    /// 
    /// 基础物品清单类。
    /// 将处理物品存储、保存和加载其内容、向其中添加物品、移除物品、装备物品等操作。
    /// </summary>
    public class Inventory : MonoBehaviour, MMEventListener<MMInventoryEvent>, MMEventListener<MMGameEvent>
	{
		public static List<Inventory> RegisteredInventories;
		
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		protected static void InitializeStatics()
		{
			RegisteredInventories = null;
		}

        /// The different possible inventory types, main are regular, equipment will have special behaviours (use them for slots where you put the equipped weapon/armor/etc).
        // 可能的不同库存类型中，主要的是常规类型，装备类型会有特殊行为（将其用于放置已装备武器 / 盔甲等的栏位）。
		public enum InventoryTypes { Main, Equipment }

		[Header("Player ID")]
        /// a unique ID used to identify the owner of this inventory
        /// 用于识别此库存所有者的唯一 ID
        [Tooltip("用于识别此库存所有者的唯一 ID")]
		public string PlayerID = "Player1";

		/// the complete list of inventory items in this inventory
		
		[Tooltip("这是您库存内容的实时视图。请勿通过检查器修改此列表，它仅为便于控制而显示。")]
		[MMReadOnly]
		public InventoryItem[] Content;

		[Header("Inventory Type")]
		/// whether this inventory is a main inventory or equipment one
		[Tooltip("Here you can define your inventory's type. Main are 'regular' inventories. Equipment inventories will be bound to a certain item class and have dedicated options.")]
		public InventoryTypes InventoryType = InventoryTypes.Main;

		[Header("丢弃掉落点")]
		[Tooltip("目标变换是你场景中的任何变换，从物品栏中掉落的对象将在该变换位置生成。")]
		/// the transform at which objects will be spawned when dropped
		public Transform TargetTransform;

		[Header("持久化")]
		[Tooltip("在这里，你可以定义此物品清单是否应响应 “加载” 和 “保存” 事件。如果你不希望将物品清单保存到磁盘，请将此设置为 “假”。你还可以让它在启动时重置，以确保在本关卡开始时它始终为空。")]
		/// whether this inventory will be saved and loaded
		[MMLabel("持久化")]		
		public bool Persistent = true;
		/// whether or not this inventory should be reset on start
		[MMLabel("重启时清空")]
		public bool ResetThisInventorySaveOnStart = false;
        
		[Header("Debug")]
		/// If true, will draw the contents of the inventory in its inspector
		[Tooltip("库存组件类似于库存的数据库和控制部分。它不会在屏幕上显示任何内容，为此你还需要一个库存显示组件。在这里，你可以决定是否要在检查器中输出调试内容（这对调试很有用）。")]
		public bool DrawContentInInspector = false;

        /// the owner of the inventory (for games where you have multiple characters)
        /// 库存所有者（适用于拥有多个角色的游戏）
        public virtual GameObject Owner { get; set; }

		/// The number of free slots in this inventory
		public virtual int NumberOfFreeSlots => Content.Length - NumberOfFilledSlots;

		/// whether or not the inventory is full (doesn't have any remaining free slots)
		public virtual bool IsFull => NumberOfFreeSlots <= 0;

		/// The number of filled slots 
		public int NumberOfFilledSlots
		{
			get
			{
				int numberOfFilledSlots = 0;
				for (int i = 0; i < Content.Length; i++)
				{
					if (!InventoryItem.IsNull(Content[i]))
					{
						numberOfFilledSlots++;
					}
				}
				return numberOfFilledSlots;
			}
		}

		public int NumberOfStackableSlots(string searchedItemID, int maxStackSize)
		{
			int numberOfStackableSlots = 0;
			int i = 0;

			while (i < Content.Length)
			{
				if (InventoryItem.IsNull(Content[i]))
				{
					numberOfStackableSlots += maxStackSize;
				}
				else
				{
					if (Content[i].ItemID == searchedItemID)
					{
						numberOfStackableSlots += maxStackSize - Content[i].Quantity;
					}
				}
				i++;
			}

			return numberOfStackableSlots;
		}
		

        /// <summary>
        /// Returns (if found) an inventory matching the searched name and playerID
        /// 返回（如果找到）与搜索到的名称和玩家 ID 匹配的物品清单
        /// </summary>
        /// <param name="inventoryName"></param>
        /// <param name="playerID"></param>
        /// <returns></returns>
        public static Inventory FindInventory(string inventoryName, string playerID)
		{
			if (inventoryName == null)
			{
				return null;
			}
            
			foreach (Inventory inventory in RegisteredInventories)
			{
				if ((inventory.name == inventoryName) && (inventory.PlayerID == playerID))
				{
					return inventory;
				}
			}
			return null;
		}

		/// <summary>
		/// On Awake we register this inventory
		/// </summary>
		protected virtual void Awake()
		{
			RegisterInventory();
		}

		/// <summary>
		/// Registers this inventory so other scripts can access it later on
		/// </summary>
		protected virtual void RegisterInventory()
		{
			if (RegisteredInventories == null)
			{
				RegisteredInventories = new List<Inventory>();
			}
			if (RegisteredInventories.Count > 0)
			{
				for (int i = RegisteredInventories.Count - 1; i >= 0; i--)
				{
					if (RegisteredInventories[i] == null)
					{
						RegisteredInventories.RemoveAt(i);
					}
				}    
			}
			RegisteredInventories.Add(this);
		}

		/// <summary>
		/// Sets the owner of this inventory, useful to apply the effect of an item for example.
		/// </summary>
		/// <param name="newOwner">New owner.</param>
		public virtual void SetOwner(GameObject newOwner)
		{
			Owner = newOwner;
		}

        /// <summary>
		/// 尝试添加指定类型的一项。请注意，这是基于名称的。
        /// </summary>
        /// <returns><c>true</c>, if item was added, <c>false</c> if it couldn't be added (item null, inventory full).</returns>
        /// <param name="itemToAdd">Item to add.</param>
        public virtual bool AddItem(InventoryItem itemToAdd, int quantity)
		{
			// if the item to add is null, we do nothing and exit
			if (itemToAdd == null)
			{
				Debug.LogWarning(this.name + " : The item you want to add to the inventory is null");
				return false;
			}

			List<int> list = InventoryContains(itemToAdd.ItemID);
;
			quantity = CapMaxQuantity(itemToAdd, quantity);
			
			// if there's at least one item like this already in the inventory and it's stackable
			if (list.Count > 0 && itemToAdd.MaximumStack > 1)
			{
				// we store items that match the one we want to add
				for (int i = 0; i < list.Count; i++)
				{
					// if there's still room in one of these items of this kind in the inventory, we add to it
					if (Content[list[i]].Quantity < itemToAdd.MaximumStack)
					{
						// we increase the quantity of our item
						Content[list[i]].Quantity += quantity;
						// if this exceeds the maximum stack
						if (Content[list[i]].Quantity > Content[list[i]].MaximumStack)
						{
							InventoryItem restToAdd = itemToAdd;
							int restToAddQuantity = Content[list[i]].Quantity - Content[list[i]].MaximumStack;
							// we clamp the quantity and add the rest as a new item
							Content[list[i]].Quantity = Content[list[i]].MaximumStack;
							AddItem(restToAdd, restToAddQuantity);
						}
						MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
						return true;
					}
				}
			}
			// if we've reached the max size of our inventory, we don't add the item
			if (NumberOfFilledSlots >= Content.Length)
			{
				return false;
			}
			while (quantity > 0)
			{
				if (quantity > itemToAdd.MaximumStack)
				{
					AddItem(itemToAdd, itemToAdd.MaximumStack);
					quantity -= itemToAdd.MaximumStack;
				}
				else
				{
					AddItemToArray(itemToAdd, quantity);
					quantity = 0;
				}
			}
			// if we're still here, we add the item in the first available slot
			MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
			return true;
		}

        /// <summary>
		/// 将指定数量的指定物品添加到库存中，添加到选定的目标索引位置。
        /// Adds the specified quantity of the specified item to the inventory, at the destination index of choice
        /// </summary>
        /// <param name="itemToAdd"></param>
        /// <param name="quantity"></param>
        /// <param name="destinationIndex"></param>
        /// <returns></returns>
        public virtual bool AddItemAt(InventoryItem itemToAdd, int quantity, int destinationIndex)
		{
			int tempQuantity = quantity;

			tempQuantity = CapMaxQuantity(itemToAdd, quantity);
			
			if (!InventoryItem.IsNull(Content[destinationIndex]))
			{
				if ((Content[destinationIndex].ItemID != itemToAdd.ItemID) || (Content[destinationIndex].MaximumStack <= 1))
				{
					return false;
				}
				else
				{
					tempQuantity += Content[destinationIndex].Quantity;
				}
			}
			
			if (tempQuantity > itemToAdd.MaximumStack)
			{
				tempQuantity = itemToAdd.MaximumStack;
			}
            
			Content[destinationIndex] = itemToAdd.Copy();
			Content[destinationIndex].Quantity = tempQuantity;
            
			// if we're still here, we add the item in the first available slot
			MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
			return true;
		}

		/// <summary>
		/// Tries to move the item at the first parameter slot to the second slot
		/// </summary>
		/// <returns><c>true</c>, if item was moved, <c>false</c> otherwise.</returns>
		/// <param name="startIndex">Start index.</param>
		/// <param name="endIndex">End index.</param>
		public virtual bool MoveItem(int startIndex, int endIndex)
		{
			bool swap = false;
			// if what we're trying to move is null, this means we're trying to move an empty slot
			if (InventoryItem.IsNull(Content[startIndex]))
			{
				Debug.LogWarning("InventoryEngine : you're trying to move an empty slot.");
				return false;
			}
			// if both objects are swappable, we'll swap them
			if (Content[startIndex].CanSwapObject)
			{
				if (!InventoryItem.IsNull(Content[endIndex]))
				{
					if (Content[endIndex].CanSwapObject)
					{
						swap = true;
					}
				}
			}
			// if the target slot is empty
			if (InventoryItem.IsNull(Content[endIndex]))
			{
				// we create a copy of our item to the destination
				Content[endIndex] = Content[startIndex].Copy();
				// we remove the original
				RemoveItemFromArray(startIndex);
				// we mention that the content has changed and the inventory probably needs a redraw if there's a GUI attached to it
				MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
				return true;
			}
			else
			{
				// if we can swap objects, we'll try and do it, otherwise we return false as the slot we target is not null
				if (swap)
				{
					// we swap our items
					InventoryItem tempItem = Content[endIndex].Copy();
					Content[endIndex] = Content[startIndex].Copy();
					Content[startIndex] = tempItem;
					MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		/// <summary>
		/// This method lets you move the item at startIndex to the chosen targetInventory, at an optional endIndex there
		/// </summary>
		/// <param name="startIndex"></param>
		/// <param name="targetInventory"></param>
		/// <param name="endIndex"></param>
		/// <returns></returns>
		public virtual bool MoveItemToInventory(int startIndex, Inventory targetInventory, int endIndex = -1)
		{
			// if what we're trying to move is null, this means we're trying to move an empty slot
			if (InventoryItem.IsNull(Content[startIndex]))
			{
				Debug.LogWarning("InventoryEngine : you're trying to move an empty slot.");
				return false;
			}
            
			// if our destination isn't empty, we exit too
			if ( (endIndex >=0) && (!InventoryItem.IsNull(targetInventory.Content[endIndex])) )
			{
				Debug.LogWarning("InventoryEngine : the destination slot isn't empty, can't move.");
				return false;
			}

			InventoryItem itemToMove = Content[startIndex].Copy();
            
			// if we've specified a destination index, we use it, otherwise we add normally
			if (endIndex >= 0)
			{
				targetInventory.AddItemAt(itemToMove, itemToMove.Quantity, endIndex);    
			}
			else
			{
				targetInventory.AddItem(itemToMove, itemToMove.Quantity);
			}
            
			// we then remove from the original inventory
			RemoveItem(startIndex, itemToMove.Quantity);

			return true;
		}

		/// <summary>
		/// Removes the specified item from the inventory.
		/// </summary>
		/// <returns><c>true</c>, if item was removed, <c>false</c> otherwise.</returns>
		/// <param name="itemToRemove">Item to remove.</param>
		public virtual bool RemoveItem(int i, int quantity)
		{
			if (i < 0 || i >= Content.Length)
			{
				Debug.LogWarning("InventoryEngine : you're trying to remove an item from an invalid index.");
				return false;
			}
			if (InventoryItem.IsNull(Content[i]))
			{
				Debug.LogWarning("InventoryEngine : you're trying to remove from an empty slot.");
				return false;
			}

			quantity = Mathf.Max(0, quantity);
            
			Content[i].Quantity -= quantity;
			if (Content[i].Quantity <= 0)
			{
				bool suppressionSuccessful = RemoveItemFromArray(i);
				MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
				return suppressionSuccessful;
			}
			else
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
				return true;
			}
		}
        
		/// <summary>
		/// Removes the specified quantity of the item matching the specified itemID
		/// </summary>
		/// <param name="itemID"></param>
		/// <param name="quantity"></param>
		/// <returns></returns>
		public virtual bool RemoveItemByID(string itemID, int quantity)
		{
			if (quantity < 1)
			{
				Debug.LogWarning("InventoryEngine : you're trying to remove an incorrect quantity ("+quantity+") from your inventory.");
				return false;
			}
            
			if (itemID == null || itemID == "")
			{
				Debug.LogWarning("InventoryEngine : you're trying to remove an item but itemID hasn't been specified.");
				return false;
			}

			int quantityLeftToRemove = quantity;
			
            
			List<int> list = InventoryContains(itemID);
			foreach (int index in list)
			{
				int quantityAtIndex = Content[index].Quantity;
				RemoveItem(index, quantityLeftToRemove);
				quantityLeftToRemove -= quantityAtIndex;
				if (quantityLeftToRemove <= 0)
				{
					return true;
				}
			}
			
			return false;
		}

		/// <summary>
		/// Destroys the item stored at index i
		/// </summary>
		/// <returns><c>true</c>, if item was destroyed, <c>false</c> otherwise.</returns>
		/// <param name="i">The index.</param>
		public virtual bool DestroyItem(int i)
		{
			Content[i] = null;

			MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
			return true;
		}

        /// <summary>
        /// 清空库存的当前状态。
        /// Empties the current state of the inventory.
        /// </summary>
        public virtual void EmptyInventory()
		{
			Content = new InventoryItem[Content.Length];

			MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
		}

		/// <summary>
		/// Returns the max value of a specific item that can be added to this inventory  without exceeding the max quantity defined on the item
		/// </summary>
		/// <param name="itemToAdd"></param>
		/// <param name="newQuantity"></param>
		/// <returns></returns>
		public virtual int CapMaxQuantity(InventoryItem itemToAdd, int newQuantity)
		{
			return Mathf.Min(newQuantity, itemToAdd.MaximumQuantity - GetQuantity(itemToAdd.ItemID));
		}

		/// <summary>
		/// Adds the item to content array.
		/// </summary>
		/// <returns><c>true</c>, if item to array was added, <c>false</c> otherwise.</returns>
		/// <param name="itemToAdd">Item to add.</param>
		/// <param name="quantity">Quantity.</param>
		protected virtual bool AddItemToArray(InventoryItem itemToAdd, int quantity)
		{
			if (NumberOfFreeSlots == 0)
			{
				return false;
			}
			int i = 0;
			while (i < Content.Length)
			{
				if (InventoryItem.IsNull(Content[i]))
				{
					Content[i] = itemToAdd.Copy();
					Content[i].Quantity = quantity;
					return true;
				}
				i++;
			}
			return false;
		}

		/// <summary>
		/// Removes the item at index i from the array.
		/// </summary>
		/// <returns><c>true</c>, if item from array was removed, <c>false</c> otherwise.</returns>
		/// <param name="i">The index.</param>
		protected virtual bool RemoveItemFromArray(int i)
		{
			if (i < Content.Length)
			{
				//Content[i].ItemID = null;
				Content[i] = null;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Resizes the array to the specified new size
		/// </summary>
		/// <param name="newSize">New size.</param>
		public virtual void ResizeArray(int newSize)
		{
			InventoryItem[] temp = new InventoryItem[newSize];
			for (int i = 0; i < Mathf.Min(newSize, Content.Length); i++)
			{
				temp[i] = Content[i];
			}
			Content = temp;
		}

		/// <summary>
		/// Returns the total quantity of items matching the specified name
		/// </summary>
		/// <returns>The quantity.</returns>
		/// <param name="searchedItem">Searched item.</param>
		public virtual int GetQuantity(string searchedItemID)
		{
			List<int> list = InventoryContains(searchedItemID);
			int total = 0;
			foreach (int i in list)
			{
				total += Content[i].Quantity;
			}
			return total;
		}

        /// <summary>
        /// 返回库存中所有与指定名称匹配的物品列表
        /// Returns a list of all the items in the inventory that match the specified name
        /// </summary>
        /// <returns>A list of item matching the search criteria.</returns>
        /// <param name="searchedType">The searched type.</param>
        public virtual List<int> InventoryContains(string searchedItemID)
		{
			List<int> list = new List<int>();

			for (int i = 0; i < Content.Length; i++)
			{
				if (!InventoryItem.IsNull(Content[i]))
				{
					if (Content[i].ItemID == searchedItemID)
					{
						list.Add(i);
					}
				}
			}
			return list;
		}

		/// <summary>
		/// Returns a list of all the items in the inventory that match the specified class
		/// </summary>
		/// <returns>A list of item matching the search criteria.</returns>
		/// <param name="searchedType">The searched type.</param>
		public virtual List<int> InventoryContains(MoreMountains.InventoryEngine.ItemClasses searchedClass)
		{
			List<int> list = new List<int>();

			for (int i = 0; i < Content.Length; i++)
			{
				if (InventoryItem.IsNull(Content[i]))
				{
					continue;
				}
				if (Content[i].ItemClass == searchedClass)
				{
					list.Add(i);
				}
			}
			return list;
		}

		/// <summary>
		/// Saves the inventory to a file
		/// </summary>
		public virtual void SaveInventory()
		{
			SerializedInventory serializedInventory = new SerializedInventory();
			FillSerializedInventory(serializedInventory);
			MMSaveLoadManager.Save(serializedInventory, DetermineSaveName(), InventorySystemConstant._saveFolderName);
		}

		/// <summary>
		/// Tries to load the inventory if a file is present
		/// </summary>
		public virtual void LoadSavedInventory()
		{
			//从指定路径加载指定类型文件
			SerializedInventory serializedInventory = (SerializedInventory)MMSaveLoadManager.Load(typeof(SerializedInventory), DetermineSaveName(), InventorySystemConstant._saveFolderName);
			ExtractSerializedInventory(serializedInventory);
			MMInventoryEvent.Trigger(MMInventoryEventType.InventoryLoaded, null, this.name, null, 0, 0, PlayerID);
		}

        /// <summary>
        /// 填充序列化库存以进行存储
        /// </summary>
        /// <param name="serializedInventory">Serialized inventory.</param>
        protected virtual void FillSerializedInventory(SerializedInventory serializedInventory)
		{
			serializedInventory.InventoryType = InventoryType;
			serializedInventory.DrawContentInInspector = DrawContentInInspector;
			serializedInventory.ContentType = new string[Content.Length];
			serializedInventory.ContentQuantity = new int[Content.Length];
			for (int i = 0; i < Content.Length; i++)
			{
				if (!InventoryItem.IsNull(Content[i]))
				{
					serializedInventory.ContentType[i] = Content[i].ItemID;
					serializedInventory.ContentQuantity[i] = Content[i].Quantity;
				}
				else
				{
					serializedInventory.ContentType[i] = null;
					serializedInventory.ContentQuantity[i] = 0;
				}
			}
		}

		protected InventoryItem _loadedInventoryItem;

		/// <summary>
		/// Extracts the serialized inventory from a file content
		/// </summary>
		/// <param name="serializedInventory">Serialized inventory.</param>
		protected virtual void ExtractSerializedInventory(SerializedInventory serializedInventory)
		{
			if (serializedInventory == null)
			{
				return;
			}

			InventoryType = serializedInventory.InventoryType;
			DrawContentInInspector = serializedInventory.DrawContentInInspector;
			Content = new InventoryItem[serializedInventory.ContentType.Length];
			for (int i = 0; i < serializedInventory.ContentType.Length; i++)
			{
				if ((serializedInventory.ContentType[i] != null) && (serializedInventory.ContentType[i] != ""))
				{
					_loadedInventoryItem = Resources.Load<InventoryItem>(InventorySystemConstant._resourceItemPath + serializedInventory.ContentType[i]);
					if (_loadedInventoryItem == null)
					{
						Debug.LogError("InventoryEngine : Couldn't find any inventory item to load at Resources/"+ InventorySystemConstant._resourceItemPath
                            + " named "+serializedInventory.ContentType[i] + ". Make sure all your items definitions names (the name of the InventoryItem scriptable " +
							"objects) are exactly the same as their ItemID string in their inspector. Make sure they are in a  Resources/"+InventorySystemConstant._resourceItemPath +" folder. " +
							"Once that's done, also make sure you reset all saved inventories as the mismatched names and IDs may have " +
							"corrupted them.");
					}
					else
					{
						Content[i] = _loadedInventoryItem.Copy();
						Content[i].Quantity = serializedInventory.ContentQuantity[i];
					}
				}
				else
				{
					Content[i] = null;
				}
			}
		}

		//确定保存名称
		protected virtual string DetermineSaveName()
		{
            // 仓库名称 + _ ＋ 玩家id + 加上后缀
            // Main仓库_Player1.inventory
            return gameObject.name + "_" + PlayerID + InventorySystemConstant._saveFileExtension;
		}

		/// <summary>
		/// Destroys any save file 
		/// </summary>
		public virtual void ResetSavedInventory()
		{
			MMSaveLoadManager.DeleteSave(DetermineSaveName(), InventorySystemConstant._saveFolderName);
			Debug.LogFormat("Inventory save file deleted");
		}

		/// <summary>
		/// Triggers the use and potential consumption of the item passed in parameter. You can also specify the item's slot (optional) and index.
		/// </summary>
		/// <param name="item">Item.</param>
		/// <param name="slot">Slot.</param>
		/// <param name="index">Index.</param>
		public virtual bool UseItem(InventoryItem item, int index, InventorySlot slot = null)
		{
			if (InventoryItem.IsNull(item))
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index, PlayerID);
				return false;
			}
			if (!item.IsUsable)
			{
				return false;
			}
			if (item.Use(PlayerID))
			{
				// remove 1 from quantity
				if (item.Consumable)
				{
					RemoveItem(index, item.ConsumeQuantity);    
				}
				MMInventoryEvent.Trigger(MMInventoryEventType.ItemUsed, slot, this.name, item.Copy(), 0, index, PlayerID);
			}
			return true;
		}

		/// <summary>
		/// Triggers the use of an item, as specified by its name. Prefer this signature over the previous one if you don't particularly care what slot the item will be taken from in case of duplicates.
		/// </summary>
		/// <param name="itemName"></param>
		/// <returns></returns>
		public virtual bool UseItem(string itemName)
		{
			List<int> list = InventoryContains(itemName);
			if (list.Count > 0)
			{
				UseItem(Content[list[list.Count - 1]], list[list.Count - 1], null);
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Equips the item at the specified slot 
		/// </summary>
		/// <param name="item">Item.</param>
		/// <param name="index">Index.</param>
		/// <param name="slot">Slot.</param>
		public virtual void EquipItem(InventoryItem item, int index, InventorySlot slot = null)
		{
			if (InventoryType == Inventory.InventoryTypes.Main)
			{
				InventoryItem oldItem = null;
				if (InventoryItem.IsNull(item))
				{
					MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index, PlayerID);
					return;
				}
				// if the object is not equipable, we do nothing and exit
				if (!item.IsEquippable)
				{
					return;
				}
				// if a target equipment inventory is not set, we do nothing and exit
				if (item.TargetEquipmentInventory(PlayerID) == null)
				{
					Debug.LogWarning("InventoryEngine Warning : " + Content[index].ItemName + "'s target equipment inventory couldn't be found.");
					return;
				}
				// if the object can't be moved, we play an error sound and exit
				if (!item.CanMoveObject)
				{
					MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index, PlayerID);
					return;
				}
				// if the object can't be equipped if the inventory is full, and if it indeed is, we do nothing and exit
				if (!item.EquippableIfInventoryIsFull)
				{
					if (item.TargetEquipmentInventory(PlayerID).IsFull)
					{
						return;
					}
				}
				// if this is a mono slot inventory, we prepare to swap
				if (item.TargetEquipmentInventory(PlayerID).Content.Length == 1)
				{
					if (!InventoryItem.IsNull(item.TargetEquipmentInventory(PlayerID).Content[0]))
					{
						if (
							(item.CanSwapObject)
							&& (item.TargetEquipmentInventory(PlayerID).Content[0].CanMoveObject)
							&& (item.TargetEquipmentInventory(PlayerID).Content[0].CanSwapObject)
						)
						{
							// we store the item in the equipment inventory
							oldItem = item.TargetEquipmentInventory(PlayerID).Content[0].Copy();
							oldItem.UnEquip(PlayerID);
							MMInventoryEvent.Trigger(MMInventoryEventType.ItemUnEquipped, slot, this.name, oldItem, oldItem.Quantity, index, PlayerID);
							item.TargetEquipmentInventory(PlayerID).EmptyInventory();
						}
					}
				}
				// we add one to the target equipment inventory
				item.TargetEquipmentInventory(PlayerID).AddItem(item.Copy(), item.Quantity);
				// remove 1 from quantity
				if (item.MoveWhenEquipped)
				{
					RemoveItem(index, item.Quantity);    
				}
				if (oldItem != null)
				{
					oldItem.Swap(PlayerID);
					if (oldItem.MoveWhenEquipped)
					{
						if (oldItem.ForceSlotIndex)
						{
							AddItemAt(oldItem, oldItem.Quantity, oldItem.TargetIndex);    
						}
						else
						{
							AddItem(oldItem, oldItem.Quantity);    
						}	
					}
				}
				// call the equip method of the item
				if (!item.Equip(PlayerID))
				{
					return;
				}
				MMInventoryEvent.Trigger(MMInventoryEventType.ItemEquipped, slot, this.name, item, item.Quantity, index, PlayerID);
			}
		}

		/// <summary>
		/// Drops the item, removing it from the inventory and potentially spawning an item on the ground near the character
		/// </summary>
		/// <param name="item">Item.</param>
		/// <param name="index">Index.</param>
		/// <param name="slot">Slot.</param>
		public virtual void DropItem(InventoryItem item, int index, InventorySlot slot = null)
		{
			if (InventoryItem.IsNull(item))
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index, PlayerID);
				return;
			}
			item.SpawnPrefab(PlayerID);
            
			if (this.name == item.TargetEquipmentInventoryName)
			{
				if (item.UnEquip(PlayerID))
				{
					DestroyItem(index);
				}
			} else
			{
				DestroyItem(index);
			}

		}

		public virtual void DestroyItem(InventoryItem item, int index, InventorySlot slot = null)
		{
			if (InventoryItem.IsNull(item))
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index, PlayerID);
				return;
			}
			DestroyItem(index);
		}

		public virtual void UnEquipItem(InventoryItem item, int index, InventorySlot slot = null)
		{
			// if there's no item at this slot, we trigger an error
			if (InventoryItem.IsNull(item))
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index, PlayerID);
				return;
			}
			// if we're not in an equipment inventory, we trigger an error
			if (InventoryType != InventoryTypes.Equipment)
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index, PlayerID);
				return;
			}
			// we trigger the unequip effect of the item
			if (!item.UnEquip(PlayerID))
			{
				return;
			}
			MMInventoryEvent.Trigger(MMInventoryEventType.ItemUnEquipped, slot, this.name, item, item.Quantity, index, PlayerID);

			// if there's a target inventory, we'll try to add the item back to it
			if (item.TargetInventory(PlayerID) != null)
			{
				bool itemAdded = false;
				if (item.ForceSlotIndex)
				{
					itemAdded = item.TargetInventory(PlayerID).AddItemAt(item, item.Quantity, item.TargetIndex);
					if (!itemAdded)
					{
						itemAdded = item.TargetInventory(PlayerID).AddItem(item, item.Quantity);    	
					}
				}
				else
				{
					itemAdded = item.TargetInventory(PlayerID).AddItem(item, item.Quantity);    
				}
				
				// if we managed to add the item
				if (itemAdded)
				{
					DestroyItem(index);
				}
				else
				{
					// if we couldn't (inventory full for example), we drop it to the ground
					MMInventoryEvent.Trigger(MMInventoryEventType.Drop, slot, this.name, item, item.Quantity, index, PlayerID);
				}
			}
		}

		/// <summary>
		/// Catches inventory events and acts on them
		/// </summary>
		/// <param name="inventoryEvent">Inventory event.</param>
		public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			// if this event doesn't concern our inventory display, we do nothing and exit
			if (inventoryEvent.TargetInventoryName != this.name)
			{
				return;
			}
			if (inventoryEvent.PlayerID != PlayerID)
			{
				return;
			}
			switch (inventoryEvent.InventoryEventType)
			{
				case MMInventoryEventType.Pick:
					if (inventoryEvent.EventItem.ForceSlotIndex)
					{
						AddItemAt(inventoryEvent.EventItem, inventoryEvent.Quantity, inventoryEvent.EventItem.TargetIndex);    
					}
					else
					{
						AddItem(inventoryEvent.EventItem, inventoryEvent.Quantity);    
					}
					break;

				case MMInventoryEventType.UseRequest:
					UseItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
					break;

				case MMInventoryEventType.EquipRequest:
					EquipItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
					break;

				case MMInventoryEventType.UnEquipRequest:
					UnEquipItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
					break;

				case MMInventoryEventType.Destroy:
					DestroyItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
					break;

				case MMInventoryEventType.Drop:
					DropItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
					break;
			}
		}

		/// <summary>
		/// When we catch an MMGameEvent, we do stuff based on its name
		/// </summary>
		/// <param name="gameEvent">Game event.</param>
		public virtual void OnMMEvent(MMGameEvent gameEvent)
		{
			if ((gameEvent.EventName == "Save") && Persistent)
			{
				SaveInventory();
			}
			if ((gameEvent.EventName == "Load") && Persistent)
			{
				if (ResetThisInventorySaveOnStart)
				{
					ResetSavedInventory();
				}
				LoadSavedInventory();
			}
		}

		/// <summary>
		/// On enable, we start listening for MMGameEvents. You may want to extend that to listen to other types of events.
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMGameEvent>();
			this.MMEventStartListening<MMInventoryEvent>();
		}

		/// <summary>
		/// On disable, we stop listening for MMGameEvents. You may want to extend that to stop listening to other types of events.
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMGameEvent>();
			this.MMEventStopListening<MMInventoryEvent>();
		}
	}
}