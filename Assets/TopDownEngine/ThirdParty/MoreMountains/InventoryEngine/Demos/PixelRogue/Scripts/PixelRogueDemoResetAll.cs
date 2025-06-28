using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// A very small class used to reset inventories and persistence data in the PixelRogue demos
    /// 一个非常小的类，用于重置 PixelRogue 演示中的库存和持久化数据
    /// </summary>
    public class PixelRogueDemoResetAll : MonoBehaviour
	{
		const string _inventorySaveFolderName = "InventoryEngine"; 
		
		public virtual void ResetAll()
		{
			// we delete the save folder for inventories
			MMSaveLoadManager.DeleteSaveFolder (_inventorySaveFolderName);
			// we delete our persistence data
			MMPersistenceManager.Instance.ResetPersistence();
			// we reload the scene
			SceneManager.LoadScene("PixelRogueRoom1");
		}
	}	
}

