using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// A class used to go from one level to the next while specifying an entry point in the target level. 
    /// Entry points are defined in each level's LevelManager component. They're simply Transforms in a list. 
    /// The index in the list is the identifier for the entry point. 
    /// 
    /// 一个用于从一个关卡进入下一个关卡，同时在目标关卡中指定一个入口点的类。
    /// 入口点在每个关卡的 LevelManager 组件中定义。它们只是列表中的 Transform。
    /// 列表中的索引就是入口点的标识符。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Spawn/Go To Level Entry Point")]
	public class GoToLevelEntryPoint : FinishLevel 
	{
		[Space(10)]
		[Header("Points of Entry")]

		/// Whether or not to use entry points. If you don't, you'll simply move on to the next level
		[MMLabel("是否使用入口")]
		[Tooltip("是否使用入口点。如果你不使用，你将直接进入下一个级别。")]
		public bool UseEntryPoints = false;
        /// The index of the point of entry to move to in the next level
        [MMLabel("在下一级中要移动到的入口点的索引")]
        [Tooltip("The index of the point of entry to move to in the next level")]
		public int PointOfEntryIndex;
        /// The direction to face when moving to the next level
        [MMLabel("移动到下一层时要面对的方向")]
        [Tooltip("The direction to face when moving to the next level")]
		public Character.FacingDirections FacingDirection;

		/// <summary>
		/// Loads the next level and stores the target entry point index in the game manager
		/// </summary>
		public override void GoToNextLevel()
		{
			if (UseEntryPoints)
			{
				GameManager.Instance.StorePointsOfEntry(LevelName, PointOfEntryIndex, FacingDirection);
			}
			
			base.GoToNextLevel ();
		}
	}
}