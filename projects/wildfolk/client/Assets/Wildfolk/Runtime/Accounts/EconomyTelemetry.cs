using System;
using UnityEngine;
namespace Wildfolk
{
    public static class EconomyTelemetry
    {
        public static void Record(Component actor,string metric,int amount=1,string source=null)
        {
            if(!Application.isPlaying||actor==null||actor.GetComponent<PlayerInventory>()==null)return;
            try{var n=actor.GetComponent<NetworkCommonGrowth>();if(n!=null){if(n.IsSpawned&&n.IsServer)n.EmitEconomy(metric,amount,source??Guid.NewGuid().ToString("N"));return;}
                EconomyService.Get().Record(metric,amount,source??Guid.NewGuid().ToString("N"));}catch(Exception){/* Economic local I/O failure must not roll back a committed gameplay action. Service reports its unavailable state. */}
        }
        public static void Killed(Component actor,GameObject creature,string source)
        {
            var d=creature.GetComponent<CreatureMember>()?.Definition;if(d==null)return;
            if(d.Faction==CreatureFaction.Undead){Record(actor,"undead",1,source);Record(actor,"hostiles",1,source);}
            else if(d.Faction==CreatureFaction.Goblin){Record(actor,"goblins",1,source);Record(actor,"hostiles",1,source);}
            else if(d.Faction==CreatureFaction.Wildlife){Record(actor,"wildlife",1,source);if(d.Actions!=null&&d.Actions.Attack!=WildlifeAttack.None&&!d.Actions.Defensive||creature.GetComponent<EnemyController>()!=null)Record(actor,"hostiles",1,source);}
        }
        public static void Crafted(Component actor,ItemDefinition item){Record(actor,"crafts");if(item!=null&&Array.IndexOf(new[]{"diamond_axe","diamond_body","diamond_head","diamond_pickaxe","diamond_spear","diamond_sword"},item.Id)>=0)Record(actor,"diamond");}
        public static string WorldSource(WorldGenerator world,string source)=>world.Seed+":"+world.Map.GenerationVersion+":"+source;
    }
    public sealed class AchievementObserver:MonoBehaviour
    {
        private WorldGenerator world;private WorldClock clock;private PlayerDeath death;private NetworkPlayer net;private double next;private DayPhase previous;private int night;private bool initialized,fullNight;
        private void Start(){world=FindFirstObjectByType<WorldGenerator>();clock=world?.GetComponent<WorldClock>();death=GetComponent<PlayerDeath>();net=GetComponent<NetworkPlayer>();}
        private async void Update()
        {
            if(Time.unscaledTimeAsDouble<next)return;next=Time.unscaledTimeAsDouble+.5;
            var session=GetComponent<PlayerInputCommands>().Session;if(session==null||session.Mode==SessionMode.Multiplayer&&(net==null||!net.IsSpawned||!net.IsOwner)||world?.Map==null||clock==null)return;
            // Owner-local exploration and survival observe replicated world/position. Other actors only record server-issued events.
            var e=EconomyService.Get();var alive=death!=null&&death.State==PlayerLife.Alive;
            if(alive&&world.Map.Streamed){var p=(Vector2)transform.position;var c=world.Map.Continental;if(c.Underground(p)){e.Record("cavern",1,"cavern");if(c.CaveAt(p).Nearest(p).Depth>=.6f)e.Record("deep",1,"deep");}else e.Record("biomes",1<<(int)c.Environment(p).Biome,"biome:"+(int)c.Environment(p).Biome);}
            var phase=clock.Model.Phase;if(initialized){if(previous!=DayPhase.Night&&phase==DayPhase.Night){night=clock.Model.Day;fullNight=alive;}if(!alive)fullNight=false;if(previous==DayPhase.Night&&phase==DayPhase.Dawn&&fullNight&&night==clock.Model.Day){e.Record("nights",1,EconomyTelemetry.WorldSource(world,"night:"+night));fullNight=false;}}else initialized=true;previous=phase;
            await e.Pulse();if(ReviveServices.Wallet is IAsyncPremiumCurrencyWallet wallet)await wallet.Reconcile();
        }
    }
}
