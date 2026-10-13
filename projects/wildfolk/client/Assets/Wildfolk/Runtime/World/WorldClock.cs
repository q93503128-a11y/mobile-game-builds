using UnityEngine;
namespace Wildfolk
{
    public sealed class WorldClock : MonoBehaviour
    {
        public DayNightDefinition Definition;
        public GameplaySession Session;
        public WorldTimeModel Model { get; private set; }
        private void Awake() => Model=new WorldTimeModel(Definition);
        private void Update() {if(Session.WorldIsRunning)Model.Advance(Time.deltaTime);}
    }
}
