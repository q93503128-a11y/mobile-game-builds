using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed class ResourcePopulation
    {
        public const int Columns=4,Rows=3;
        private readonly WorldMap map;
        private readonly int[,] counts,targets;
        public readonly List<RecoveryTicket> Pending=new List<RecoveryTicket>();
        public readonly List<HarvestExclusion> Harvests=new List<HarvestExclusion>();
        public int Serial,CandidateCursor;
        public ResourcePopulation(WorldMap value)
        {
            map=value;counts=new int[map.Definition.Resources.Length,Columns*Rows];targets=new int[map.Definition.Resources.Length,Columns*Rows];
            foreach(var p in map.Resources)counts[p.Rule,Region(p.Rule,p.Position)]++;
            for(var r=0;r<counts.GetLength(0);r++)for(var region=0;region<Columns*Rows;region++)targets[r,region]=Mathf.CeilToInt(counts[r,region]*map.Definition.Resources[r].TargetDensityRatio);
        }
        public int Region(int rule,Vector2 p)
        {if(map.Definition.Resources[rule].WorldRecoveryScope)return 0;var x=Mathf.Clamp((int)((p.x/map.Definition.Size.x+.5f)*Columns),0,Columns-1);var y=Mathf.Clamp((int)((p.y/map.Definition.Size.y+.5f)*Rows),0,Rows-1);return y*Columns+x;}
        public int Count(int rule,int region)=>counts[rule,region];
        public int Target(int rule,int region)=>targets[rule,region];
        public void Remove(int rule,Vector2 p,double now,bool harvested)
        {
            var region=Region(rule,p);counts[rule,region]=Mathf.Max(0,counts[rule,region]-1);var data=map.Definition.Resources[rule];
            if(harvested)Harvests.Add(new HarvestExclusion{Rule=rule,Position=p,Until=now+data.HarvestExclusionSeconds});Schedule(rule,region,now);
        }
        public void Add(int rule,Vector2 p)=>counts[rule,Region(rule,p)]++;
        public void Schedule(int rule,int region,double now)
        {if(!map.Renewable(rule) || counts[rule,region]>=targets[rule,region] || Pending.Exists(t=>t.Rule==rule && t.Region==region))return;Pending.Add(new RecoveryTicket{Rule=rule,Region=region,Due=now+map.Definition.Resources[rule].RespawnSeconds});}
        public void Expire(double now)=>Harvests.RemoveAll(h=>h.Until<=now);
        public bool Excluded(int rule,Vector2 p,double now)
        {var data=map.Definition.Resources[rule];foreach(var h in Harvests)if(h.Until>now && map.Definition.Resources[h.Rule].Family==data.Family && Vector2.Distance(h.Position,p)<Mathf.Max(data.HarvestExclusionRadius,map.Definition.Resources[h.Rule].HarvestExclusionRadius))return true;return false;}
        public Vector2 Candidate(int rule,int region)
        {
            var random=new System.Random(unchecked(map.Seed*397+CandidateCursor++*7919));var u=(float)random.NextDouble();var v=(float)random.NextDouble();var size=map.Definition.Size;
            if(!map.Definition.Resources[rule].WorldRecoveryScope){u=(region%Columns+u)/Columns;v=(region/Columns+v)/Rows;}
            return new Vector2(Mathf.Clamp((u-.5f)*size.x,-size.x*.5f+2,size.x*.5f-2),Mathf.Clamp((v-.5f)*size.y,-size.y*.5f+2,size.y*.5f-2));
        }
        public ResourceRecoverySnapshot Capture()=>new ResourceRecoverySnapshot{Serial=Serial,CandidateCursor=CandidateCursor,Pending=Pending.ToArray(),Harvests=Harvests.ToArray()};
        public void Restore(ResourceRecoverySnapshot state)
        {if(state==null)return;Serial=state.Serial;CandidateCursor=state.CandidateCursor;Pending.Clear();Harvests.Clear();Pending.AddRange(state.Pending);Harvests.AddRange(state.Harvests);}
    }
}
