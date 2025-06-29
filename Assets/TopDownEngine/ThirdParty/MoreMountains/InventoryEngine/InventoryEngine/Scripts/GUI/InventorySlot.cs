using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MoreMountains.Tools;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// This class handles the display of the items in an inventory and will trigger the various things you can do with an item (equip, use, etc.)
    /// 这个类负责处理库存中物品的显示，并将触发你可以对物品执行的各种操作（装备、使用等）。
    /// </summary>
    public class InventorySlot : Button, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!SlotEnabled || InventoryItem.IsNull(CurrentItem)) return;
            Move();
            Debug.Log("拖拽开始 当前的ParentInventoryDisplay 索引：" + InventoryDisplay.CurrentlyBeingMovedItemIndex);

        }
        public void OnDrag(PointerEventData eventData)
        {

        }
        //public void OnEndDrag(PointerEventData eventData)
        //{
        //    Debug.Log("结束拖拽");
        //    // 检查放置位置
        //    GameObject dropTarget = eventData.pointerCurrentRaycast.gameObject;

            
        //    if (dropTarget != null)
        //    {

        //        Debug.Log(dropTarget.gameObject.name);
        //        InventorySlot targetSlot = dropTarget.GetComponent<InventorySlot>();
        //        if (targetSlot != null)
        //        {
        //            if (InventoryDisplay.CurrentlyBeingMovedItemIndex != -1)
        //            {
        //                Debug.Log("SlotClicked");
        //                targetSlot.SlotClicked();
        //            }
        //            else
        //            {
        //                Debug.Log(InventoryDisplay.CurrentlyBeingMovedItemIndex);
        //            }
        //        }
        //        else
        //        {
        //            Debug.Log("targetSlot  " + (targetSlot == null));
        //        }
        //    }
        //    else
        //    {
        //        Debug.Log("dropTarget  "+(dropTarget == null));
        //    }


        //}



        public void OnEndDrag(PointerEventData eventData)
        {
            // 检查放置位置
            GameObject dropTarget = eventData.pointerCurrentRaycast.gameObject;
            if (dropTarget != null)
            {
                InventorySlot targetSlot = dropTarget.GetComponent<InventorySlot>();
                if (targetSlot != null)
                {
                    if (InventoryDisplay.CurrentlyBeingMovedItemIndex != -1)
                    {
                        // 新增：更新选中状态
                        EventSystem.current.SetSelectedGameObject(targetSlot.gameObject);

                        // 执行移动操作
                        targetSlot.SlotClicked();

                        // 新增：强制刷新选中状态视觉效果
                        targetSlot.OnSelect(new BaseEventData(EventSystem.current));

                        Debug.Log(CurrentItem == null);
                        DrawIcon(CurrentItem,Index);
                    }
                }
            }

        //    // 新增：处理拖拽到无效区域的情况
        //    else
        //    {
        //        if (InventoryDisplay.CurrentlyBeingMovedItemIndex != -1)
        //        {
        //            // 取消移动操作
        //            InventoryDisplay
        //.CurrentlyBeingMovedFromInventoryDisplay.InventorySlots[
        //                InventoryDisplay
        //.CurrentlyBeingMovedItemIndex].TargetImage.sprite = _originalSprite;

        //            InventoryDisplay
        //.CurrentlyBeingMovedItemIndex = -1;
        //            InventoryDisplay
        //.CurrentlyBeingMovedFromInventoryDisplay = null;
        //        }
        //    }
        }




        /// the sprite used as a background for the slot while an item is being moved
        [MMLabel("移动操作选中时的sprite")]
        public Sprite MovedSprite;
        /// the inventory display this slot belongs to
        [MMLabel("所属渲染器")]
        public InventoryDisplay ParentInventoryDisplay;

        /// the slot's index (its position in the inventory array)
        [MMLabel("索引")]
        public int Index;

        /// whether or not this slot is currently enabled and can be interacted with
        [MMLabel("激活状态")] public bool SlotEnabled = true;
        [MMLabel("目标图片")] public Image TargetImage;
        [MMLabel("目标渲染组")] public CanvasGroup TargetCanvasGroup;
        [MMLabel("目标RectTransform")] public RectTransform TargetRectTransform;
        [MMLabel("IconRectTransform")] public RectTransform IconRectTransform;
        [MMLabel("IconImage")] public Image IconImage;
        [MMLabel("数量文本")] public TMPro.TMP_Text QuantityText;

        public InventoryItem CurrentItem
        {
            get
            {
                if (ParentInventoryDisplay != null)
                {
                    return ParentInventoryDisplay.TargetInventory.Content[Index];
                }

                return null;
            }
        }

        protected const float _disabledAlpha = 0.5f;
        protected const float _enabledAlpha = 1.0f;

        protected override void Awake()
        {
            base.Awake();
            TargetImage = this.gameObject.GetComponent<Image>();
            TargetCanvasGroup = this.gameObject.GetComponent<CanvasGroup>();
            TargetRectTransform = this.gameObject.GetComponent<RectTransform>();
        }

        /// <summary>
        /// On Start, we start listening to click events on that slot
        /// </summary>
        protected override void Start()
        {
            base.Start();
            this.onClick.AddListener(SlotClicked);
        }

        /// <summary>
        /// If there's an item in this slot, draws its icon inside.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="index">Index.</param>
        public virtual void DrawIcon(InventoryItem item,int i)
        {
            if (ParentInventoryDisplay != null)
            {
                if (!InventoryItem.IsNull(item))
                {
                    SetIcon(item.Icon);
                    SetQuantity(item.Quantity);
                }
                else
                {
                    Debug.Log(1);
                    DisableIconAndQuantity();
                }
            }
            else
            {
                Debug.LogWarning("ParentInventoryDisplay == null");
            }
        }

        public virtual void SetIcon(Sprite newSprite)
        {
            IconImage.gameObject.SetActive(true);
            IconImage.sprite = newSprite;
        }

        public virtual void SetQuantity(int quantity)
        {
            if (quantity > 1)
            {
                QuantityText.gameObject.SetActive(true);
                QuantityText.text = quantity.ToString();
            }
            else
            {
                QuantityText.gameObject.SetActive(false);
            }
        }

        public virtual void DisableIconAndQuantity()
        {
            IconImage.gameObject.SetActive(false);
            QuantityText.gameObject.SetActive(false);
        }

        /// <summary>
        /// When that slot gets selected (via a mouse over or a touch), triggers an event for other classes to act on
        /// </summary>
        /// <param name="eventData">Event data.</param>
        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            if (ParentInventoryDisplay != null)
            {
                InventoryItem item = ParentInventoryDisplay.TargetInventory.Content[Index];
                MMInventoryEvent.Trigger(MMInventoryEventType.Select, this, ParentInventoryDisplay.TargetInventoryName, item, 0, Index, ParentInventoryDisplay.PlayerID);
            }
        }

        /// <summary>
        /// When that slot gets clicked, triggers an event for other classes to act on
        /// </summary>
        public virtual void SlotClicked()
        {
            Debug.Log(1);
            if (ParentInventoryDisplay != null)
            {
                InventoryItem item = ParentInventoryDisplay.TargetInventory.Content[Index];
                if (ParentInventoryDisplay.InEquipSelection)
                {
                    MMInventoryEvent.Trigger(MMInventoryEventType.EquipRequest, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index, ParentInventoryDisplay.PlayerID);
                }
                MMInventoryEvent.Trigger(MMInventoryEventType.Click, this, ParentInventoryDisplay.TargetInventoryName, item, 0, Index, ParentInventoryDisplay.PlayerID);
                // if we're currently moving an object
                if (InventoryDisplay.CurrentlyBeingMovedItemIndex != -1)
                {
                    Move();
                }
            }
        }

        /// <summary>
        /// Selects the item in this slot for a movement, or moves the currently selected one to that slot
        /// This will also swap both objects if possible
        /// </summary>
        public virtual void Move()
        {
            if (!SlotEnabled) { return; }

            // if we're not already moving an object
            if (InventoryDisplay.CurrentlyBeingMovedItemIndex == -1)
            {
                // if the slot we're on is empty, we do nothing
                if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
                {
                    MMInventoryEvent.Trigger(MMInventoryEventType.Error, this, ParentInventoryDisplay.TargetInventoryName, null, 0, Index, ParentInventoryDisplay.PlayerID);
                    return;
                }
                if (ParentInventoryDisplay.TargetInventory.Content[Index].CanMoveObject)
                {
                    // we change the background image
                    TargetImage.sprite = ParentInventoryDisplay.MovedSlotImage;
                    InventoryDisplay.CurrentlyBeingMovedFromInventoryDisplay = ParentInventoryDisplay;
                    InventoryDisplay.CurrentlyBeingMovedItemIndex = Index;
                }
            }
            // if we ARE moving an object
            else
            {
                bool moveSuccessful = false;
                // we move the object to a new slot. 
                if (ParentInventoryDisplay == InventoryDisplay.CurrentlyBeingMovedFromInventoryDisplay)
                {
                    if (!ParentInventoryDisplay.TargetInventory.MoveItem(InventoryDisplay.CurrentlyBeingMovedItemIndex, Index))
                    {
                        // if the move couldn't be made (non empty destination slot for example), we play a sound
                        MMInventoryEvent.Trigger(MMInventoryEventType.Error, this, ParentInventoryDisplay.TargetInventoryName, null, 0, Index, ParentInventoryDisplay.PlayerID);
                        moveSuccessful = false;
                    }
                    else
                    {
                        moveSuccessful = true;
                    }
                }
                else
                {
                    if (!ParentInventoryDisplay.AllowMovingObjectsToThisInventory)
                    {
                        moveSuccessful = false;
                    }
                    else
                    {
                        if (!InventoryDisplay.CurrentlyBeingMovedFromInventoryDisplay.TargetInventory.MoveItemToInventory(InventoryDisplay.CurrentlyBeingMovedItemIndex, ParentInventoryDisplay.TargetInventory, Index))
                        {
                            // if the move couldn't be made (non empty destination slot for example), we play a sound
                            MMInventoryEvent.Trigger(MMInventoryEventType.Error, this, ParentInventoryDisplay.TargetInventoryName, null, 0, Index, ParentInventoryDisplay.PlayerID);
                            moveSuccessful = false;
                        }
                        else
                        {
                            moveSuccessful = true;
                        }
                    }
                }

                if (moveSuccessful)
                {
                    // if the move could be made, we reset our currentlyBeingMoved pointer
                    InventoryDisplay.CurrentlyBeingMovedItemIndex = -1;
                    InventoryDisplay.CurrentlyBeingMovedFromInventoryDisplay = null;
                    MMInventoryEvent.Trigger(MMInventoryEventType.Move, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index, ParentInventoryDisplay.PlayerID);
                }
            }
        }

        /// <summary>
        /// Consume one unity of the item in this slot, triggering a sound and whatever behaviour has been defined for this item being used
        /// </summary>
        public virtual void Use()
        {
            if (!SlotEnabled) { return; }
            MMInventoryEvent.Trigger(MMInventoryEventType.UseRequest, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index, ParentInventoryDisplay.PlayerID);
        }

        /// <summary>
        /// Equip this item if possible.
        /// </summary>
        public virtual void Equip()
        {
            if (!SlotEnabled) { return; }
            MMInventoryEvent.Trigger(MMInventoryEventType.EquipRequest, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index, ParentInventoryDisplay.PlayerID);
        }

        /// <summary>
        /// Unequip this item if possible.
        /// </summary>
        public virtual void UnEquip()
        {
            if (!SlotEnabled) { return; }
            MMInventoryEvent.Trigger(MMInventoryEventType.UnEquipRequest, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index, ParentInventoryDisplay.PlayerID);
        }

        /// <summary>
        /// Drops this item.
        /// </summary>
        public virtual void Drop()
        {
            if (!SlotEnabled) { return; }
            if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.Error, this, ParentInventoryDisplay.TargetInventoryName, null, 0, Index, ParentInventoryDisplay.PlayerID);
                return;
            }
            if (!ParentInventoryDisplay.TargetInventory.Content[Index].Droppable)
            {
                return;
            }
            if (ParentInventoryDisplay.TargetInventory.Content[Index].Drop(ParentInventoryDisplay.PlayerID))
            {
                InventoryDisplay.CurrentlyBeingMovedItemIndex = -1;
                InventoryDisplay.CurrentlyBeingMovedFromInventoryDisplay = null;
                MMInventoryEvent.Trigger(MMInventoryEventType.Drop, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index, ParentInventoryDisplay.PlayerID);
            }
        }

        /// <summary>
        /// Disables the slot.
        /// </summary>
        public virtual void DisableSlot()
        {
            this.interactable = false;
            SlotEnabled = false;
            TargetCanvasGroup.alpha = _disabledAlpha;
        }

        /// <summary>
        /// Enables the slot.
        /// </summary>
        public virtual void EnableSlot()
        {
            this.interactable = true;
            SlotEnabled = true;
            TargetCanvasGroup.alpha = _enabledAlpha;
        }

        /// <summary>
        /// Returns true if the item at this slot can be equipped, false otherwise
        /// </summary>
        public virtual bool Equippable()
        {
            if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
            {
                return false;
            }
            if (!ParentInventoryDisplay.TargetInventory.Content[Index].IsEquippable)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Returns true if the item at this slot can be used, false otherwise
        /// </summary>
        public virtual bool Usable()
        {
            if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
            {
                return false;
            }
            if (!ParentInventoryDisplay.TargetInventory.Content[Index].IsUsable)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Returns true if the item at this slot can be dropped, false otherwise
        /// </summary>
        public virtual bool Movable()
        {
            if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
            {
                return false;
            }
            if (!ParentInventoryDisplay.TargetInventory.Content[Index].CanMoveObject)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Returns true if the item at this slot can be dropped, false otherwise
        /// </summary>
        public virtual bool Droppable()
        {
            if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
            {
                return false;
            }
            if (!ParentInventoryDisplay.TargetInventory.Content[Index].Droppable)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Returns true if the item at this slot can be dropped, false otherwise
        /// </summary>
        public virtual bool Unequippable()
        {
            if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
            {
                return false;
            }
            if (ParentInventoryDisplay.TargetInventory.InventoryType != Inventory.InventoryTypes.Equipment)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public virtual bool EquipUseButtonShouldShow()
        {
            if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index])) { return false; }
            return ParentInventoryDisplay.TargetInventory.Content[Index].DisplayProperties.DisplayEquipUseButton;
        }

        public virtual bool MoveButtonShouldShow()
        {
            if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index])) { return false; }
            return ParentInventoryDisplay.TargetInventory.Content[Index].DisplayProperties.DisplayMoveButton;
        }

        public virtual bool DropButtonShouldShow()
        {
            if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index])) { return false; }
            return ParentInventoryDisplay.TargetInventory.Content[Index].DisplayProperties.DisplayDropButton;
        }

        public virtual bool EquipButtonShouldShow()
        {
            if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index])) { return false; }
            return ParentInventoryDisplay.TargetInventory.Content[Index].DisplayProperties.DisplayEquipButton;
        }

        public virtual bool UseButtonShouldShow()
        {
            if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index])) { return false; }
            return ParentInventoryDisplay.TargetInventory.Content[Index].DisplayProperties.DisplayUseButton;
        }

        public virtual bool UnequipButtonShouldShow()
        {
            if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index])) { return false; }
            return ParentInventoryDisplay.TargetInventory.Content[Index].DisplayProperties.DisplayUnequipButton;
        }

    }
}