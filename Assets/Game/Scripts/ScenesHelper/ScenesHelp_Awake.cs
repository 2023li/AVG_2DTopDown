using Assets.PixelFantasy.PixelHeroes.Common.Scripts.CharacterScripts;
using MoreMountains.TopDownEngine;


public class ScenesHelp_Awake : ScenesHelpBase<ScenesHelp_Awake>
{
    public GoToLevelEntryPoint point;
    public CharacterBuilder character;
    protected override void Awake()
    {
       base.Awake();
    }
    protected override void Start()
    {
        base.Start();
    }


    public void EnterNewGame(string playerName)
    {
        PersistenceManager.Instance.CreateNewGameSave(playerName);
        PersistenceManager.Instance.SaveCharDress(character);

        point.GoToNextLevel();

    }


 

    public override void OnMMEvent(LOGameEvent eventType)
    {
        base.OnMMEvent(eventType);

        switch (eventType.eventTpye)
        {
            case LOGameEventType.AppInitComplete:
                break;
            case LOGameEventType.SetLanguage:
                break;

            case LOGameEventType.LoadSaveComplete:
                //监听到存档数据加载完成
                point.LevelName = PersistenceManager.Instance.LastScene;
                point.UseEntryPoints = true;
                point.PointOfEntryIndex = PersistenceManager.Instance.LastPointIndex;
                point.GoToNextLevel();
                break;
            default:
                break;
        }

    }


}
