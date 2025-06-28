using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using MoreMountains.Tools;

namespace MoreMountains.InventoryEngine
{	
	/// <summary>
	/// A class used to display an item's details in GUI
	/// </summary>
	public class InventoryDetails : MonoBehaviour, MMEventListener<MMInventoryEvent>
	{
        /// 参考仓库（详情来源）
        [MMInformation("指定要在此详情面板中显示物品详情的仓库名称。启用'全局'后将显示所有物品详情（无视所属仓库）。", MMInformationAttribute.InformationType.Info, false)]
        [MMLabel("目标仓库")]
        public string TargetInventoryName;
        [MMLabel("玩家ID")]
        public string PlayerID = "Player1";
        /// 是否全局显示所有仓库物品
        [MMLabel("全局模式")]
        [Tooltip("启用后显示所有物品详情（无视目标仓库）")]
        public bool Global = false;

        /// 详情面板当前是否隐藏
        public virtual bool Hidden { get; protected set; }

        [Header("默认值")]
        [MMInformation("启用'空槽隐藏'后，选择空插槽时将不显示详情面板。", MMInformationAttribute.InformationType.Info, false)]

        /// 选择空插槽时是否隐藏面板
        [MMLabel("空槽隐藏")]
        [Tooltip("选择空插槽时自动隐藏详情面板")]
        public bool HideOnEmptySlot = true;

        [MMInformation("设置无物品选中时详情面板的默认显示值（未启用空槽隐藏时生效）。", MMInformationAttribute.InformationType.Info, false)]

        /// 默认标题
        [MMLabel("默认标题")]
        [Tooltip("无物品选中时显示的标题文本")]
        public string DefaultTitle;

        /// 默认简短描述
        [MMLabel("默认简描")]
        [Tooltip("无物品选中时显示的简短描述")]
        public string DefaultShortDescription;

        /// 默认描述
        [MMLabel("默认描述")]
        [Tooltip("无物品选中时显示的完整描述")]
        public string DefaultDescription;

        /// 默认数量
        [MMLabel("默认数量")]
        [Tooltip("无物品选中时显示的数量文本")]
        public string DefaultQuantity;

        /// 默认图标
        [MMLabel("默认图标")]
        [Tooltip("无物品选中时显示的图标")]
        public Sprite DefaultIcon;

        [Header("行为设置")]
        [MMInformation("设置详情面板在游戏启动时的默认可见状态。", MMInformationAttribute.InformationType.Info, false)]

        /// 启动时是否隐藏面板
        [MMLabel("启动隐藏")]
        [Tooltip("游戏启动时自动隐藏详情面板")]
        public bool HideOnStart = true;

        [Header("组件绑定")]
        [MMInformation("绑定详情面板的UI组件。", MMInformationAttribute.InformationType.Info, false)]

        /// 图标容器
        [MMLabel("图标组件")]
        [Tooltip("显示物品图标的Image组件")]
        public Image Icon;

        /// the title container object
        [MMLabel("标题组件")]
        [Tooltip("显示物品标题的TMP Text组件")]
        public TMPro.TMP_Text Title;

        /// the short description container object
        [MMLabel("简描组件")]
        [Tooltip("显示物品简短描述的TMP Text组件")]
        public TMPro.TMP_Text ShortDescription;

        /// the description container object
        [MMLabel("描述组件")]
        [Tooltip("显示物品完整描述的TMPText组件")]
        public TMPro.TMP_Text Description;

        /// the quantity container object
        [MMLabel("数量组件")]
        [Tooltip("显示物品数量的TMPText组件")]
        public TMPro.TMP_Text Quantity;

		protected float _fadeDelay=0.2f;
		protected CanvasGroup _canvasGroup;

		/// <summary>
		/// On Start, we grab and store the canvas group and determine our current Hidden status
		/// </summary>
		protected virtual void Start()
		{
			_canvasGroup = GetComponent<CanvasGroup>();

			if (HideOnStart)
			{
				_canvasGroup.alpha = 0;
			}

			if (_canvasGroup.alpha == 0)
			{
				Hidden = true;
			}
			else
			{
				Hidden = false;
			}
		}

        /// <summary>
        /// Starts the display coroutine or the panel's fade depending on whether or not the current slot is empty
        /// 根据当前插槽是否为空，启动显示协程或面板的淡入淡出效果
        /// </summary>
        /// <param name="item">Item.</param>
        public virtual void DisplayDetails(InventoryItem item)
		{
		

			if (InventoryItem.IsNull(item))
			{
				if (HideOnEmptySlot && !Hidden)
				{
					StartCoroutine(MMFade.FadeCanvasGroup(_canvasGroup,_fadeDelay,0f));
					Hidden=true;
				}
				if (!HideOnEmptySlot)
				{
					StartCoroutine(FillDetailFieldsWithDefaults(0));
				}
			}
			else
			{
				StartCoroutine(FillDetailFields(item,0f));

				if (HideOnEmptySlot && Hidden)
				{
					StartCoroutine(MMFade.FadeCanvasGroup(_canvasGroup,_fadeDelay,1f));
					Hidden=false;
				}
			}

		}

		/// <summary>
		/// Fills the various detail fields with the item's metadata
		/// </summary>
		/// <returns>The detail fields.</returns>
		/// <param name="item">Item.</param>
		/// <param name="initialDelay">Initial delay.</param>
		protected virtual IEnumerator FillDetailFields(InventoryItem item, float initialDelay)
		{
			yield return new WaitForSeconds(initialDelay);
			if (Title!=null) { Title.text = item.ItemName ; }
			if (ShortDescription!=null) { ShortDescription.text = item.ShortDescription;}
			if (Description!=null) { Description.text = item.Description;}
			if (Quantity!=null) { Quantity.text = item.Quantity.ToString();}
			if (Icon!=null) { Icon.sprite = item.Icon;}
			
			if (HideOnEmptySlot && !Hidden && (item.Quantity == 0))
			{
				StartCoroutine(MMFade.FadeCanvasGroup(_canvasGroup,_fadeDelay,0f));
				Hidden=true;
			}
		}

		/// <summary>
		/// Fills the detail fields with default values.
		/// </summary>
		/// <returns>The detail fields with defaults.</returns>
		/// <param name="initialDelay">Initial delay.</param>
		protected virtual IEnumerator FillDetailFieldsWithDefaults(float initialDelay)
		{
			yield return new WaitForSeconds(initialDelay);
			if (Title!=null) { Title.text = DefaultTitle ;}
			if (ShortDescription!=null) { ShortDescription.text = DefaultShortDescription;}
			if (Description!=null) { Description.text = DefaultDescription;}
			if (Quantity!=null) { Quantity.text = DefaultQuantity;}
			if (Icon!=null) { Icon.sprite = DefaultIcon;}
		}

		/// <summary>
		/// Catches MMInventoryEvents and displays details if needed
		/// </summary>
		/// <param name="inventoryEvent">Inventory event.</param>
		public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			// if this event doesn't concern our inventory display, we do nothing and exit
			if (!Global && (inventoryEvent.TargetInventoryName != this.TargetInventoryName))
			{
				return;
			}

			if (inventoryEvent.PlayerID != PlayerID)
			{
				return;
			}

			switch (inventoryEvent.InventoryEventType)
			{
				case MMInventoryEventType.Select:
			
					DisplayDetails (inventoryEvent.EventItem);
					break;
				case MMInventoryEventType.UseRequest:
					DisplayDetails (inventoryEvent.EventItem);
					break;
				case MMInventoryEventType.InventoryOpens:
					DisplayDetails (inventoryEvent.EventItem);
					break;
				case MMInventoryEventType.Drop:
					DisplayDetails (null);
					break;
				case MMInventoryEventType.EquipRequest:
					DisplayDetails (null);
					break;
			}
		}

		/// <summary>
		/// On Enable, we start listening for MMInventoryEvents
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMInventoryEvent>();
		}

		/// <summary>
		/// On Disable, we stop listening for MMInventoryEvents
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMInventoryEvent>();
		}
	}
}