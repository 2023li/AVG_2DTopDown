using System.Collections;
using System.Collections.Generic;
using Assets.PixelFantasy.PixelHeroes.Common.Scripts.CharacterScripts;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

public class ScenesHelp_01 : ScenesHelpBase<ScenesHelp_01>, MMEventListener<TopDownEngineEvent>
{


    // Start is called before the first frame update
    protected override void Start()
    {
       base.Start();
       
    }


    protected override void OnEnable()
    {
      base.OnEnable();
        
    }
    protected override void OnDisable()
    {
        base.OnDisable();
    }



    public override void OnMMEvent(TopDownEngineEvent e)
    {
        base.OnMMEvent(e);
        
        switch (e.EventType)
        {
            case TopDownEngineEventTypes.SpawnCharacterStarts:
                break;
            case TopDownEngineEventTypes.LevelStart:
               
               

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
                var data = PersistenceManager.Instance.GetCharDressDic();
                var eb = e.OriginCharacter.transform.Find("BuildCharacter").GetComponent<CharacterBuilder>();
                CharacterBuilderHelper.DicToCharacterDress(eb, ref data);
               e.OriginCharacter.GetComponent<InventoryCharacterIdentifier>().PlayerID = PersistenceManager.Instance.SaveOnlyID;


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
}
