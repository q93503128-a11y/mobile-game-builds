using System;
using System.IO;
using UnityEditor;
namespace Wildfolk.Editor
{
    public static class OverviewSeedBatch
    {
        public static string Status {get;private set;}="No seed batch running.";
        public static bool Running {get;private set;}
        private static WorldGenerationDefinition definition;private static int seed,index,count;private static string root;
        public static void Begin(WorldGenerationDefinition source,int firstSeed,int seedCount,string directory=null)
        {
            if(Running)throw new InvalidOperationException("A seed batch is already running.");
            if(source==null||seedCount<1||seedCount>20)throw new ArgumentException("Choose 1–20 seeds and a world definition.");
            definition=source;seed=firstSeed;count=seedCount;index=0;root=Path.GetFullPath(directory??"Captures/world-overview");Directory.CreateDirectory(root);Running=true;Status="Generating seed overviews 0/"+count;EditorApplication.update+=Step;
        }
        private static void Step()
        {
            if(EditorApplication.isCompiling)return;
            try {using(var overview=new WorldOverview(new WorldMap(definition,unchecked(seed+index),WorldGenerationVersion.Current),512)){overview.Export(root);if(overview.Issues.Count>0)File.WriteAllLines(Path.Combine(root,"issues-"+overview.Map.Seed+".txt"),overview.Issues);}index++;Status="Exported "+index+"/"+count+" actual seeds → "+root;if(index>=count)Stop();}
            catch(Exception e){Status="Seed batch failed at "+unchecked(seed+index)+": "+e.Message;Stop();}
        }
        private static void Stop(){Running=false;definition=null;EditorApplication.update-=Step;}
    }
}
