using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Add this component to a collider 2D and you'll be able to have it perform an action when 
    /// a character equipped with the specified key enters it.
    /// 
    /// 将此组件添加到 2D 碰撞体上，当配备指定钥匙的角色进入时，你就能让它执行一个动作。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Environment/Key Operated Zone")]
	public class KeyOperatedZone : ButtonActivated 
	{
		[MMInspectorGroup("Key", true, 18)]

        /// whether this zone actually requires a key
        [MMLabel("需要钥匙")]
        [Tooltip("该区域是否真的需要钥匙才能使用")]
        public bool RequiresKey = true;

        /// the key ID, that will be checked against the existence (or not) of a key of the same name in the player's inventory
        [MMLabel("钥匙ID")]
        [Tooltip("钥匙唯一标识ID（需与玩家背包中的钥匙名称完全匹配）")]
        public string KeyID;

        /// the method that should be triggered when the key is used
        [MMLabel("钥匙使用事件")]
        [Tooltip("当使用正确钥匙时触发的事件响应")]
        public UnityEvent KeyAction;

        protected GameObject _collidingObject;
		protected List<int> _keyList;

		/// <summary>
		/// On Start we initialize our object
		/// </summary>
		protected virtual void Start()
		{
			_keyList = new List<int> ();
		}

		/// <summary>
		/// On enter we store our colliding object
		/// </summary>
		/// <param name="collider">Something colliding with the water.</param>
		protected override void OnTriggerEnter2D(Collider2D collider)
		{
			_collidingObject = collider.gameObject;
			base.OnTriggerEnter2D (collider);
		}

		protected override void OnTriggerEnter(Collider collider)
		{
			_collidingObject = collider.gameObject;
			base.OnTriggerEnter(collider);
		}

		/// <summary>
		/// When the button is pressed, we check if we have a key in our inventory
		/// </summary>
		public override void TriggerButtonAction()
		{
			if (!CheckNumberOfUses())
			{
				PromptError();
				return;
			}

			if (_collidingObject == null) { return; }

			if (RequiresKey)
			{
				CharacterInventory characterInventory = _collidingObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterInventory> ();
				if (characterInventory == null)
				{
					PromptError();
					return;
				}	

				_keyList.Clear ();
				_keyList = characterInventory.MainInventory.InventoryContains (KeyID);
				if (_keyList.Count == 0)
				{
					PromptError();
					return;
				}
				else
				{
					base.TriggerButtonAction ();
					characterInventory.MainInventory.UseItem(KeyID);
				}
			}

			TriggerKeyAction ();
			ActivateZone ();
		}

		/// <summary>
		/// Calls the method associated to the key action
		/// </summary>
		protected virtual void TriggerKeyAction()
		{
			if (KeyAction != null)
			{
				KeyAction.Invoke ();
			}
		}
	}
}