using UnityEngine;
using UnityEngine.SceneManagement;
namespace Wildfolk
{
    public static class SceneFlow
    {
        public const string HomeScene="MainMenu",GameplayScene="Wildfolk",MultiplayerScene="Multiplayer";
        private static SavedGame pending;
        private static SaveRepository repository;
        public static bool IsTransitioning {get;private set;}
        public static SaveRepository Repository {get{return repository ?? (repository=SaveRepository.Default());}set{repository=value;}}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset(){pending=null;repository=null;IsTransitioning=false;}
        public static void NewGame()
        {if(IsTransitioning)return;pending=null;WorldGenerator.SelectedSeed=WorldGenerator.RandomSeed();Go(GameplayScene);}
        public static void Continue(SavedGame game,SaveRepository source)
        {if(IsTransitioning)return;pending=game;Repository=source;WorldGenerator.SelectedSeed=game.Seed;Go(GameplayScene);}
        public static void Home(SaveRepository source)
        {if(IsTransitioning)return;pending=null;Repository=source;Go(HomeScene);}
        private static void Go(string scene){IsTransitioning=true;Time.timeScale=1;SceneManager.LoadScene(scene);}
        public static SavedGame TakePending(){var value=pending;pending=null;IsTransitioning=false;return value;}
        public static void HomeReady(){pending=null;IsTransitioning=false;}
        public static void MultiplayerReady(){pending=null;IsTransitioning=false;Time.timeScale=1;}
    }
}
