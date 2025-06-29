using System.Collections;
using System.Collections.Generic;
using MoreMountains.InventoryEngine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LOInventorySlot : InventorySlot, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // 新增字段
    private Canvas _dragCanvas;
    private GameObject _dragIcon;
    private RectTransform _dragIconTransform;
    protected static bool _isDragging = false;
    InventoryInputManager inventoryInputManager;

    protected override void Start()
    {
        base.Start();
        inventoryInputManager = GameObject.FindWithTag(GameConstant.Tag_仓库画布).GetComponent<InventoryInputManager>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!SlotEnabled || InventoryItem.IsNull(CurrentItem)) return;

        _isDragging = true;


        inventoryInputManager.Move();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
    }

    private void SetDraggedPosition(PointerEventData eventData)
    {
        if (_dragIconTransform == null) return;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            _dragIconTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 worldPoint);

        _dragIconTransform.position = worldPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        _isDragging = false;

       

        // 检查放置位置
        GameObject dropTarget = eventData.pointerCurrentRaycast.gameObject;
        if (dropTarget != null)
        {
            InventorySlot targetSlot = dropTarget.GetComponent<InventorySlot>();
            if (targetSlot != null)
            {
                if (InventoryDisplay.CurrentlyBeingMovedItemIndex != -1)
                {
                    Move();
                }
            }
        }

      
    }
   

}
