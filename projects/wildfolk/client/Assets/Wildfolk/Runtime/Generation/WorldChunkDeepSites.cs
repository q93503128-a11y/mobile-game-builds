using UnityEngine;
namespace Wildfolk
{
    public sealed partial class WorldChunkRuntime
    {
        private GameObject DeepLandmark(Transform parent,DeepSite site)
        {var go=new GameObject(L.Text("poi."+site.Kind));go.transform.SetParent(parent,false);go.transform.position=site.Position;var view=go.AddComponent<SpriteRenderer>();view.sprite=World.DeepPoiSprites[(int)site.Kind];view.sharedMaterial=World.SpriteMaterial;view.sortingOrder=WorldSorting.GroundDecoration+1;go.transform.localScale=Vector3.one*2;return go;}
        private void MaterializeDeepSites(WorldChunk chunk,Transform parent)
        {
            foreach(var site in DeepCavernContent.Sites(World.Map.Continental))
            {
                if(!WorldChunk.At(site.Position,World.Definition.ChunkSize).Equals(chunk))continue;DeepLandmark(parent,site);
                if(!SimulationAuthority.Allows(World) || site.Kind!=DeepSiteKind.DeepMine && site.Kind!=DeepSiteKind.AbandonedWorkshop || features.ContainsKey(site.Id))continue;
                features[site.Id]=new WorldFeatureSnapshot{Id=site.Id,Visited=true};var catalog=World.Definition.ProgressionCatalog;var item=catalog.Item("industrial_container");
                var p=site.Position+Vector2.right*3;if(!World.Map.IsWalkable(p,.8f))p=site.Position;
                var placed=NetworkSpawnFactory.Spawn(item.Placement.Prefab,p,Quaternion.identity);placed.SaveId=site.Id+":cargo";placed.CreatorId="";
                var chest=placed.GetComponent<StorageChest>();chest.BindVisitor(World.Player);var roll=(uint)WorldSeeds.Derive(World.Seed,SeedDomain.POI,WorldSeeds.StableText(site.Id));
                chest.Model.Add(catalog.Item("crossbow_bolt"),6+(int)(roll%7));if(roll%3==0)chest.Model.Add(catalog.Item("mechanical_part"),1+(int)((roll/3)%3));
                if(site.Kind==DeepSiteKind.AbandonedWorkshop){chest.Model.Add(catalog.Item("ballista_bolt"),2);if(roll%2==0)chest.Model.Add(catalog.Item("bronze"),1);}
            }
        }
    }
}
