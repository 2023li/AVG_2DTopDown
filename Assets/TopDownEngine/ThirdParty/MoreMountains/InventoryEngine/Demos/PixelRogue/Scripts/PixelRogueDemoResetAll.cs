using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// A very small class used to reset inventories and persistence data in the PixelRogue demos
    /// һ���ǳ�С���࣬�������� PixelRogue ��ʾ�еĿ��ͳ־û�����
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

