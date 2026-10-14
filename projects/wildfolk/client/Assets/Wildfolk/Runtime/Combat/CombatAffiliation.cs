using UnityEngine;
namespace Wildfolk
{
    public enum EntityRelation { PlayerTeam, WildAnimal, NeutralGoblin, Hostile, TeamStructure }
    public sealed class CombatAffiliation : MonoBehaviour
    {
        public EntityRelation Relation;
        public int TeamId;
        public static bool CanDamage(Health attacker,Health target)
        {
            if(attacker==null || target==null || target==attacker || !target.IsAlive)return false;
            var other=target.GetComponent<CombatAffiliation>();
            var own=attacker.GetComponent<CombatAffiliation>();
            if(other!=null)
            {
                var friendly=other.Relation==EntityRelation.PlayerTeam || other.Relation==EntityRelation.TeamStructure;
                var teamActor=own==null ? attacker.GetComponent<PlayerInventory>()!=null : own.Relation==EntityRelation.PlayerTeam || own.Relation==EntityRelation.TeamStructure;
                return !friendly || !teamActor || other.TeamId!=(own==null ? 0 : own.TeamId);
            }
            return target.GetComponent<PlayerInventory>()==null && target.GetComponent<WorldPlaceable>()==null;
        }
    }
}
