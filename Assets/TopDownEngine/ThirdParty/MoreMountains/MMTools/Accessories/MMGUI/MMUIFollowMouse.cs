using Sirenix.OdinInspector;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace MoreMountains.Tools
{
    /// <summary>
    /// This component will let you have a UI object follow the mouse position
    /// </summary>
    public class MMUIFollowMouse : MonoBehaviour
    {

      
        public virtual Canvas TargetCanvas { get; set; }
        protected Vector2 _newPosition;
        protected Vector2 _mousePosition;

       
        protected virtual void LateUpdate()
        {

            //Debug.Log(TargetCanvas.gameObject.name);
#if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
            _mousePosition = Input.mousePosition;
#else
			_mousePosition = Mouse.current.position.ReadValue();
#endif
            // 修改坐标转换代码（兼容 Overlay 模式）
            if (TargetCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(TargetCanvas.transform as RectTransform, _mousePosition, null, out _newPosition);
            }
            else
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(TargetCanvas.transform as RectTransform, _mousePosition, TargetCanvas.worldCamera, out _newPosition);
            }

            transform.position = TargetCanvas.transform.TransformPoint(_newPosition);




        }
    }
}