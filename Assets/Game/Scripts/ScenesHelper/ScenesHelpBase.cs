using FantasyFin.Unity;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;



public abstract class ScenesHelpBase<T> :ffSingletonMonoBehaviour<T>, MMEventListener<TopDownEngineEvent>, MMEventListener<CheckPointEvent>, MMEventListener<LOGameEvent>where T: ScenesHelpBase<T>
{

    [Button("自动引用仓库组件")]
    public void AutoReferenceInventories()
    {
        allInventoryDisplays = GameObject.FindObjectsOfType<InventoryDisplay>();
        allInventorys = GameObject.FindObjectsOfType<Inventory>();
    }

    protected Transform Inventories;


    [SerializeField]
    private Inventory[] allInventorys;
    [SerializeField]
    private InventoryDisplay[] allInventoryDisplays;
    private void Reset()
    {
        allInventoryDisplays = GameObject.FindObjectsOfType<InventoryDisplay>();
        allInventorys = GameObject.FindObjectsOfType<Inventory>();
    }

    protected override void Awake()
    {
        
    }

    protected virtual void OnEnable()
    {
        this.MMEventStartListening<TopDownEngineEvent>();
        this.MMEventStartListening<CheckPointEvent>();
        this.MMEventStartListening<LOGameEvent>();
    }
    protected virtual void Start()
    {


    }

    protected virtual void OnDisable()
    {
        this.MMEventStopListening<TopDownEngineEvent>();
        this.MMEventStopListening<CheckPointEvent>();
        this.MMEventStopListening<LOGameEvent>();
    }

    public virtual void OnMMEvent(TopDownEngineEvent e)
    {
        switch (e.EventType)
        {
            case TopDownEngineEventTypes.SpawnCharacterStarts:
                break;
            case TopDownEngineEventTypes.LevelStart:
                string sceneName = SceneManager.GetActiveScene().name;
                //记录最后的场景位置
                PersistenceManager.Instance.SetLastScene(sceneName);
                PersistenceManager persistenceManager = PersistenceManager.Instance;
                if (persistenceManager != null)
                {
                    for (int i = 0; i < allInventorys.Length; i++)
                    {
                        allInventorys[i].PlayerID = persistenceManager.SaveOnlyID;
                    }
                    for (int i = 0;i < allInventoryDisplays.Length; i++)
                    {
                        allInventoryDisplays[i].PlayerID = persistenceManager.SaveOnlyID;
                    }

                }
             
                break;
            case TopDownEngineEventTypes.LevelComplete:
                break;
            case TopDownEngineEventTypes.LevelEnd:
                break;
            case TopDownEngineEventTypes.Pause:
                break;
            case TopDownEngineEventTypes.UnPause:
                break;
            case TopDownEngineEventTypes.PlayerDeath:
                break;
            case TopDownEngineEventTypes.SpawnComplete:

                var charIn  = e.OriginCharacter.GetComponent<CharacterInventory>();
                charIn.PlayerID = PersistenceManager.Instance.SaveOnlyID;

                break;
            case TopDownEngineEventTypes.RespawnStarted:
                break;
            case TopDownEngineEventTypes.RespawnComplete:
                break;
            case TopDownEngineEventTypes.StarPicked:
                break;
            case TopDownEngineEventTypes.GameOver:
                break;
            case TopDownEngineEventTypes.CharacterSwap:
                break;
            case TopDownEngineEventTypes.CharacterSwitch:
                break;
            case TopDownEngineEventTypes.Repaint:
                break;
            case TopDownEngineEventTypes.TogglePause:
                break;
            case TopDownEngineEventTypes.LoadNextScene:
                break;
            case TopDownEngineEventTypes.PauseNoMenu:
                break;
            default:
                break;
        }
    }

   

    public void OnMMEvent(CheckPointEvent e)
    {
        Debug.Log("监听到检查点事件");
        PersistenceManager.Instance.SetLastPoint(e.Order);
    }

    public virtual void OnMMEvent(LOGameEvent eventType)
    {
        
    }
}

