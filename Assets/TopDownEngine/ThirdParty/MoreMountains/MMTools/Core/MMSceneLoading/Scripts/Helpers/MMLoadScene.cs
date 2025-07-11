﻿using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.Tools
{	
	/// <summary>
	/// Add this component on an object, specify a scene name in its inspector, and call LoadScene() to load the desired scene.
	/// </summary>
	public class MMLoadScene : MonoBehaviour 
	{
        /// the possible modes to load scenes. Either Unity's native API, or MoreMountains' LoadingSceneManager
        /// 加载场景的可能模式。可以是 Unity 的原生 API，也可以是 MoreMountains 的 LoadingSceneManager。
        public enum LoadingSceneModes { UnityNative, MMSceneLoadingManager, MMAdditiveSceneLoadingManager }

		/// the name of the scene that needs to be loaded when LoadScene gets called
		[Tooltip("the name of the scene that needs to be loaded when LoadScene gets called")]
		public string SceneName;
		/// defines whether the scene will be loaded using Unity's native API or MoreMountains' way
		[Tooltip("defines whether the scene will be loaded using Unity's native API or MoreMountains' way")]
		public LoadingSceneModes LoadingSceneMode = LoadingSceneModes.UnityNative;

		/// <summary>
		/// Loads the scene specified in the inspector
		/// </summary>
		public virtual void LoadScene()
		{
			switch (LoadingSceneMode)
			{
				case LoadingSceneModes.UnityNative:
					SceneManager.LoadScene (SceneName);
					break;
				case LoadingSceneModes.MMSceneLoadingManager:
					MMSceneLoadingManager.LoadScene (SceneName);
					break;
				case LoadingSceneModes.MMAdditiveSceneLoadingManager:
					MMAdditiveSceneLoadingManager.LoadScene(SceneName);
					break;
			}
		}
	}
}