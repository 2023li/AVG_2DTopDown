﻿using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// The Fader class can be put on an Image, and it'll intercept MMFadeEvents and turn itself on or off accordingly.
    /// 淡入淡出圆环
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
	[AddComponentMenu("More Mountains/Tools/GUI/MM Fader Round")]
	public class MMFaderRound : MMMonoBehaviour, MMEventListener<MMFadeEvent>, MMEventListener<MMFadeInEvent>, MMEventListener<MMFadeOutEvent>, MMEventListener<MMFadeStopEvent>
	{
		public enum CameraModes { Main, Override }

		[MMInspectorGroup("Bindings", true, 121)] 
		public CameraModes CameraMode = CameraModes.Main;
		[MMEnumCondition("CameraMode",(int)CameraModes.Override)]

		/// the camera to pick the position from (usually the "regular" game camera)
		[MMLabel("目标相机")]
		public Camera TargetCamera;

		/// the background to fade 
		[MMLabel("淡出背景")]
		public RectTransform FaderBackground;

		/// the mask used to draw a hole in the background that will get faded / scaled
		[MMLabel("淡出遮罩")]
		public RectTransform FaderMask;

		[MMInspectorGroup("Identification", true, 122)]

		/// the ID for this fader (0 is default), set more IDs if you need more than one fader
		[MMLabel("渐变器ID")]
		public int ID;
		
		[MMInspectorGroup("Mask", true, 127)]
		[MMVector("min", "max")]

		/// the mask's scale at minimum and maximum opening
		[MMLabel("遮罩缩放")]
		public Vector2 MaskScale;
		
		[MMInspectorGroup("Timing", true, 124)]

		/// the default duration of the fade in/out
		[MMLabel("淡入淡出默认持续时间")]
		public float DefaultDuration = 0.2f;

        /// the default curve to use for this fader
        [MMLabel("淡入淡出默认曲线")]
        public MMTweenType DefaultTween = new MMTweenType(MMTween.MMTweenCurve.LinearTween);

		/// whether or not the fade should happen in unscaled time 
		[MMLabel("忽略时间缩放")]
		public bool IgnoreTimescale = true;
		
		[MMInspectorGroup("Interaction", true, 125)]

		/// whether or not the fader should block raycasts when visible
		[MMLabel("淡入淡出过程是否阻挡射线")]
		public bool ShouldBlockRaycasts = false;
		
		[MMInspectorGroup("Debug", true, 126)]
		public Transform DebugWorldPositionTarget;
		[MMInspectorButtonBar(new string[] { "FadeIn1Second", "FadeOut1Second", "DefaultFade", "ResetFader" }, 
			new string[] { "FadeIn1Second", "FadeOut1Second", "DefaultFade", "ResetFader" }, 
			new bool[] { true, true, true, true },
			new string[] { "main-call-to-action", "", "", "" })]
		public bool DebugToolbar;
		protected CanvasGroup _canvasGroup;

		protected float _initialScale;
		protected float _currentTargetScale;

		protected float _currentDuration;
		protected MMTweenType _currentCurve;

		protected bool _fading = false;
		protected float _fadeStartedAt;

		/// <summary>
		/// Test method triggered by an inspector button
		/// </summary>
		protected virtual void ResetFader()
		{
			FaderMask.transform.localScale = MaskScale.x * Vector3.one;
		}

		/// <summary>
		/// Test method triggered by an inspector button
		/// </summary>
		protected virtual void DefaultFade()
		{
			MMFadeEvent.Trigger(DefaultDuration, MaskScale.y, DefaultTween, ID, IgnoreTimescale, DebugWorldPositionTarget.transform.position);
		}

		/// <summary>
		/// Test method triggered by an inspector button
		/// </summary>
		protected virtual void FadeIn1Second()
		{
			MMFadeInEvent.Trigger(1f, DefaultTween, ID, IgnoreTimescale, DebugWorldPositionTarget.transform.position);
		}

		/// <summary>
		/// Test method triggered by an inspector button
		/// </summary>
		protected virtual void FadeOut1Second()
		{
			MMFadeOutEvent.Trigger(1f, DefaultTween, ID, IgnoreTimescale, DebugWorldPositionTarget.transform.position);
		}

		/// <summary>
		/// On Start, we initialize our fader
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}

		/// <summary>
		/// On init, we grab our components, and disable/hide everything
		/// </summary>
		protected virtual void Initialization()
		{
			if (CameraMode == CameraModes.Main)
			{
				TargetCamera = Camera.main;
			}
			_canvasGroup = GetComponent<CanvasGroup>();
			FaderMask.transform.localScale = MaskScale.x * Vector3.one;
		}

		/// <summary>
		/// On Update, we update our alpha 
		/// </summary>
		protected virtual void Update()
		{
			if (_canvasGroup == null) { return; }

			if (_fading)
			{
				Fade();
			}
		}

		/// <summary>
		/// Fades the canvasgroup towards its target alpha
		/// </summary>
		protected virtual void Fade()
		{
			float currentTime = IgnoreTimescale ? Time.unscaledTime : Time.time;
			float endTime = _fadeStartedAt + _currentDuration;
			if (currentTime - _fadeStartedAt < _currentDuration)
			{
				float newScale = MMTween.Tween(currentTime, _fadeStartedAt, endTime, _initialScale, _currentTargetScale, _currentCurve);
				FaderMask.transform.localScale = newScale * Vector3.one;
			}
			else
			{
				StopFading();
			}
		}

		/// <summary>
		/// Stops the fading.
		/// </summary>
		protected virtual void StopFading()
		{
			FaderMask.transform.localScale = _currentTargetScale * Vector3.one;
			_fading = false;
			if (FaderMask.transform.localScale == MaskScale.y * Vector3.one)
			{
				DisableFader();
			}
		}

		/// <summary>
		/// Disables the fader.
		/// </summary>
		protected virtual void DisableFader()
		{
			if (ShouldBlockRaycasts)
			{
				_canvasGroup.blocksRaycasts = false;
			}
			_canvasGroup.alpha = 0;
		}

		/// <summary>
		/// Enables the fader.
		/// </summary>
		protected virtual void EnableFader()
		{
			if (ShouldBlockRaycasts)
			{
				_canvasGroup.blocksRaycasts = true;
			}
			_canvasGroup.alpha = 1;
		}

		protected virtual void StartFading(float initialAlpha, float endAlpha, float duration, MMTweenType curve, int id, 
			bool ignoreTimeScale, Vector3 worldPosition)
		{
			if (id != ID)
			{
				return;
			}

			if (TargetCamera == null)
			{
				Debug.LogWarning(this.name + " : You're using a fader round but its TargetCamera hasn't been setup in its inspector. It can't fade.");
				return;
			}

			FaderMask.anchoredPosition = Vector3.zero;

			Vector3 viewportPosition = TargetCamera.WorldToViewportPoint(worldPosition);
			viewportPosition.x = Mathf.Clamp01(viewportPosition.x);
			viewportPosition.y = Mathf.Clamp01(viewportPosition.y);
			viewportPosition.z = Mathf.Clamp01(viewportPosition.z);
            
			FaderMask.anchorMin = viewportPosition;
			FaderMask.anchorMax = viewportPosition;

			IgnoreTimescale = ignoreTimeScale;
			EnableFader();
			_fading = true;
			_initialScale = initialAlpha;
			_currentTargetScale = endAlpha;
			_fadeStartedAt = IgnoreTimescale ? Time.unscaledTime : Time.time;
			_currentCurve = curve;
			_currentDuration = duration;

			float newScale = MMTween.Tween(0f, 0f, duration, _initialScale, _currentTargetScale, _currentCurve);
			FaderMask.transform.localScale = newScale * Vector3.one;
		}

		/// <summary>
		/// When catching a fade event, we fade our image in or out
		/// </summary>
		/// <param name="fadeEvent">Fade event.</param>
		public virtual void OnMMEvent(MMFadeEvent fadeEvent)
		{
			_currentTargetScale = (fadeEvent.TargetAlpha == -1) ? MaskScale.y : fadeEvent.TargetAlpha;
			StartFading(FaderMask.transform.localScale.x, _currentTargetScale, fadeEvent.Duration, fadeEvent.Curve, fadeEvent.ID, 
				fadeEvent.IgnoreTimeScale, fadeEvent.WorldPosition);
		}

		/// <summary>
		/// When catching an MMFadeInEvent, we fade our image in
		/// </summary>
		/// <param name="fadeEvent">Fade event.</param>
		public virtual void OnMMEvent(MMFadeInEvent fadeEvent)
		{
			if (fadeEvent.Duration > 0)
			{
				StartFading(MaskScale.y, MaskScale.x, fadeEvent.Duration, fadeEvent.Curve, fadeEvent.ID, 
					fadeEvent.IgnoreTimeScale, fadeEvent.WorldPosition);	
			}
			else
			{
				FaderMask.transform.localScale = MaskScale.x * Vector3.one;
			}
		}

		/// <summary>
		/// When catching an MMFadeOutEvent, we fade our image out
		/// </summary>
		/// <param name="fadeEvent">Fade event.</param>
		public virtual void OnMMEvent(MMFadeOutEvent fadeEvent)
		{
			if (fadeEvent.Duration > 0)
			{
				StartFading(MaskScale.x, MaskScale.y, fadeEvent.Duration, fadeEvent.Curve, fadeEvent.ID, 
					fadeEvent.IgnoreTimeScale, fadeEvent.WorldPosition);	
			}
			else
			{
				FaderMask.transform.localScale = MaskScale.y * Vector3.one;
			}
		}

		/// <summary>
		/// When catching an MMFadeStopEvent, we stop our fade
		/// </summary>
		/// <param name="fadeEvent">Fade event.</param>
		public virtual void OnMMEvent(MMFadeStopEvent fadeStopEvent)
		{
			if (fadeStopEvent.ID == ID)
			{
				_fading = false;
				if (fadeStopEvent.Restore)
				{
					FaderMask.transform.localScale = _initialScale * Vector3.one;
				}
			}
		}

		/// <summary>
		/// On enable, we start listening to events
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMFadeEvent>();
			this.MMEventStartListening<MMFadeStopEvent>();
			this.MMEventStartListening<MMFadeInEvent>();
			this.MMEventStartListening<MMFadeOutEvent>();
		}

		/// <summary>
		/// On disable, we stop listening to events
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMFadeEvent>();
			this.MMEventStopListening<MMFadeStopEvent>();
			this.MMEventStopListening<MMFadeInEvent>();
			this.MMEventStopListening<MMFadeOutEvent>();
		}
	}
}