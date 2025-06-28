using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Add this component to an object in your scene to have it act like a chest. You'll need a key operated zone to open it, and item picker(s) on it to fill its contents
    /// 将此组件添加到场景中的对象上，使其表现得像一个箱子。你需要一个钥匙操作区域来打开它，并在其上设置物品拾取器来填充其内容。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Items/Inventory Engine Chest")]
	public class InventoryEngineChest : TopDownMonoBehaviour 
	{
		protected Animator _animator;
		protected ItemPicker[] _itemPickerList;

		/// <summary>
		/// On start we grab our animator and list of item pickers
		/// </summary>
		protected virtual void Start()
		{
			_animator = GetComponent<Animator> ();
			_itemPickerList = GetComponents<ItemPicker> ();
		}

		/// <summary>
		/// A public method to open the chest, usually called by the associated key operated zone
		/// </summary>
		public virtual void OpenChest()
		{
			TriggerOpeningAnimation ();
			PickChestContents ();
		}

		/// <summary>
		/// Triggers the opening animation.
		/// </summary>
		protected virtual void TriggerOpeningAnimation()
		{
			if (_animator == null)
			{
				return;
			}
			_animator.SetTrigger ("OpenChest");
		}

		/// <summary>
		/// Puts all the items in the associated pickers into the player's inventories
		/// </summary>
		protected virtual void PickChestContents()
		{
			if (_itemPickerList.Length == 0)
			{
				return;
			}
			foreach (ItemPicker picker in _itemPickerList)
			{
				picker.Pick ();
			}
		}
			
	}
}