using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using static Wildfolk.Editor.WorldArt;
namespace Wildfolk.Editor
{
    public static class CreatureLootAssets
    {
        private static ContentCatalog catalog;
        private static List<ItemDefinition> items;private static List<RecipeDefinition> recipes;
        private static readonly List<GameObject> sources=new List<GameObject>();
        public static readonly string[] Species={"Bat","StoneCrawler","BogLeech","Wolf","MountainGoat","Crow","GiantBoar","CrystalCrawler"};
        private static T D<T>(string name)where T:UnityEngine.Object=>AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+name+".asset");
        private static T Make<T>(string name,Action<T> initialize)where T:ScriptableObject
        {var d=GameplayAssets.Asset<T>(name,initialize);initialize(d);EditorUtility.SetDirty(d);return d;}
        private static ItemDefinition I(string id)=>items.Find(x=>x.Id==id)??throw new InvalidOperationException(id);
        private static ItemAmount Cost(string id,int count)=>new ItemAmount{Item=I(id),Count=count};
        private static ItemDefinition Item(string name,string id,ItemCategory kind,int stack)
        {var i=Make<ItemDefinition>(name,d=>{d.Id=id;d.DisplayName=name;d.Category=kind;d.MaxStack=stack;d.Icon=VisualArtImporter.Sprite(id);});if(!items.Contains(i))items.Add(i);return i;}
        private static void Recipe(string name,string id,int count,int tier,StationKind station,RecipeCategory category,float seconds,params ItemAmount[] cost)
        {var r=Make<RecipeDefinition>(name+"Recipe",d=>{d.Id=id;d.Result=I(id);d.ResultCount=count;d.RequiredWorkbenchTier=tier;d.RequiredStation=station;d.Category=category;d.ProcessingSeconds=seconds;d.Processing=seconds>0 ? ProcessingKind.Cooking : ProcessingKind.None;d.Ingredients=cost;d.SortOrder=650+(int)category*10;});if(!recipes.Contains(r))recipes.Add(r);}
        private static LootEntry Drop(string id,int min,int max,float chance=1)=>new LootEntry{Item=I(id),Minimum=min,Maximum=max,Chance=chance};
        private static LootTable Loot(string name,int xp,int gold,params LootEntry[] entries)=>Make<LootTable>(name,d=>{d.Items=entries;d.Experience=xp;d.Gold=gold;});
        private static void Food(string name,string id,float hunger,float thirst,int health,float stamina)
        {var i=Item(name,id,ItemCategory.Consumable,20);i.Consumable=Make<ConsumableDefinition>(name+"Food",d=>{d.HungerRestore=hunger;d.ThirstRestore=thirst;d.HealthRestore=health;d.StaminaRestore=stamina;d.UseCooldownSeconds=4;});EditorUtility.SetDirty(i);}
        public static void Build()
        {
            sources.Clear();VisualArtImporter.Import();SpriteMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Wildfolk/Art/WorldSprite.mat");catalog=D<ContentCatalog>("ContentCatalog");items=new List<ItemDefinition>(catalog.Items);recipes=new List<RecipeDefinition>(catalog.Recipes);
            Item("Bone","bone",ItemCategory.Material,50);Item("Chitin","chitin",ItemCategory.Material,50);Item("MedicinalGland","medicinal_gland",ItemCategory.Material,50);
            var arrow=Item("BoneArrow","bone_arrow",ItemCategory.Material,100);arrow.Ammo=AmmoKind.Arrow;arrow.AmmoDamage=3;
            var bolt=Item("BarbedBolt","barbed_bolt",ItemCategory.Material,100);bolt.Ammo=AmmoKind.Bolt;bolt.AmmoDamage=6;
            var vest=Item("CarapaceVest","carapace_vest",ItemCategory.Armor,1);vest.Armor=Make<ArmorDefinition>("CarapaceVestArmor",d=>{d.Slot=ArmorSlot.Body;d.Defense=3;d.StaminaRecoveryBonus=.15f;d.Tier=D<EquipmentTier>("IronTier");d.WornSprite=VisualArtImporter.Sprite("carapace_vest_worn");});
            Food("HealingSalve","healing_salve",0,0,25,0);Food("BoneBroth","bone_broth",24,12,18,0);Food("RestorativeBroth","restorative_broth",22,10,0,35);Food("TrailRation","trail_ration",25,0,0,20);
            Recipe("BoneArrow","bone_arrow",12,1,StationKind.Workbench,RecipeCategory.Weapons,0,Cost("bone",2),Cost("wood",1),Cost("feather",1));
            Recipe("BarbedBolt","barbed_bolt",8,2,StationKind.Workbench,RecipeCategory.Weapons,0,Cost("chitin",1),Cost("wood",2),Cost("feather",1));
            Recipe("CarapaceVest","carapace_vest",1,2,StationKind.Workbench,RecipeCategory.Armor,0,Cost("chitin",4),Cost("leather",1),Cost("plant_fiber",2));
            Recipe("HealingSalve","healing_salve",1,1,StationKind.None,RecipeCategory.Survival,0,Cost("medicinal_gland",1),Cost("plant_fiber",2));
            Recipe("BoneBroth","bone_broth",1,1,StationKind.CookingPot,RecipeCategory.Survival,8,Cost("bone",2),Cost("mushroom",1),Cost("raw_meat",1));
            Recipe("RestorativeBroth","restorative_broth",1,1,StationKind.CookingPot,RecipeCategory.Survival,8,Cost("medicinal_gland",1),Cost("mushroom",1),Cost("berry",2));
            Recipe("TrailRation","trail_ration",1,1,StationKind.Campfire,RecipeCategory.Survival,7,Cost("raw_meat",1),Cost("mushroom",1),Cost("berry",2));
            BoneTrap();
            recipes.Find(r=>r.Id=="reinforced_frame").Ingredients=new[]{Cost("steel",2),Cost("leather",1),Cost("chitin",2)};
            var mixed=I("mixed_stew").Consumable;mixed.HealthRestore=8;mixed.UseCooldownSeconds=4;EditorUtility.SetDirty(mixed);
            Loot("RabbitLoot",0,0,Drop("raw_meat",1,1),Drop("bone",1,1,.5f));Loot("SheepLoot",0,0,Drop("raw_meat",2,2));
            Loot("DeerLoot",0,0,Drop("raw_meat",2,3),Drop("leather",1,1,.75f),Drop("bone",1,1));
            Loot("ZombieLoot",5,1,Drop("bone",1,1,.5f));
            Loot("SkeletonLoot",7,2,Drop("bone",1,2));var sword=Loot("SkeletonSwordLoot",8,2,Drop("bone",1,2));sword.RareItems=new[]{Drop("rusted_sword",1,1,.12f)};
            Loot("GoblinLoot",8,3,Drop("leather",1,1,.25f));
            var sling=Loot("SlingshotGoblinLoot",8,3,Drop("leather",1,1,.25f));
            var slingRoot=PrefabUtility.LoadPrefabContents("Assets/Wildfolk/Prefabs/SlingshotGoblin.prefab");slingRoot.GetComponent<LootOnDeath>().Table=sling;PrefabUtility.SaveAsPrefabAsset(slingRoot,"Assets/Wildfolk/Prefabs/SlingshotGoblin.prefab");PrefabUtility.UnloadPrefabContents(slingRoot);
            NewSpecies();UpdateCamp();
            catalog.Items=items.ToArray();catalog.Recipes=recipes.ToArray();D<RecipeCatalog>("Recipes").Recipes=recipes.ToArray();EditorUtility.SetDirty(catalog);EditorUtility.SetDirty(D<RecipeCatalog>("Recipes"));foreach(var i in items)if(i.Id=="carapace_vest" || i.Id=="barbed_bolt" || i.Id=="bone_arrow")EditorUtility.SetDirty(i);foreach(var r in recipes)if(r.Id=="reinforced_frame")EditorUtility.SetDirty(r);
            RegisterNetwork();TierThreeAssets.RefreshPresentation();AssetDatabase.SaveAssets();
        }
        private static void BoneTrap()
        {
            var i=Item("BoneTrap","bone_trap",ItemCategory.Placeable,10);var source=I("spike_trap").Placement;
            var place=Make<PlaceableDefinition>("BoneTrapPlacement",d=>{d.Item=i;d.MaximumDistance=source.MaximumDistance;d.Footprint=source.Footprint;d.Vitality=Make<VitalityDefinition>("BoneTrapVitality",v=>v.MaximumHealth=40);});i.Placement=place;EditorUtility.SetDirty(i);
            var path="Assets/Wildfolk/Prefabs/BoneTrap.prefab";var root=UnityEngine.Object.Instantiate(source.Prefab.gameObject);root.name="BoneTrap";var p=root.GetComponent<WorldPlaceable>();p.Definition=place;p.Health.Definition=place.Vitality;p.Visual.sprite=i.Icon;root.GetComponent<SpikeTrap>().Damage=10;place.Prefab=PrefabUtility.SaveAsPrefabAsset(root,path).GetComponent<WorldPlaceable>();UnityEngine.Object.DestroyImmediate(root);EditorUtility.SetDirty(place);sources.Add(place.Prefab.gameObject);
            Recipe("BoneTrap","bone_trap",1,1,StationKind.Workbench,RecipeCategory.Building,0,Cost("bone",2),Cost("wood",3));
        }
        private static void NewSpecies()
        {
            var population=D<AnimalPopulationDefinition>("AnimalPopulation");var rules=new List<AnimalPopulationRule>(population.Rules);
            var hp=new[]{24,80,28,60,45,14,130,95};var xp=new[]{3,7,5,8,0,0,15,12};
            var habitats=new[]{CreatureHabitat.Caverns,CreatureHabitat.Rocky|CreatureHabitat.DeepCaverns,CreatureHabitat.Shore,CreatureHabitat.Forest,CreatureHabitat.Rocky,CreatureHabitat.Grassland|CreatureHabitat.ForestEdge,CreatureHabitat.ForestEdge|CreatureHabitat.Rocky,CreatureHabitat.CrystalPocket};
            var attacks=new[]{WildlifeAttack.Swoop,WildlifeAttack.Bite,WildlifeAttack.Ambush,WildlifeAttack.Bite,WildlifeAttack.None,WildlifeAttack.None,WildlifeAttack.Charge,WildlifeAttack.Pulse};
            var ids=new[]{"bat","stone_crawler","bog_leech","wolf","mountain_goat","crow","giant_boar","crystal_crawler"};
            var damage=new[]{5,10,6,9,0,0,18,12};var walk=new[]{1.2f,.55f,.4f,1.2f,.8f,.7f,.8f,.65f};var chase=new[]{4.2f,1.7f,1.2f,3.3f,4.8f,4.6f,2.2f,1.4f};var action=new[]{8f,0,6,0,0,0,11,0};
            var reach=new[]{1.1f,1.5f,1.0f,1.5f,0,0,1.3f,2.4f};var detection=new[]{6f,4,3,6,5,5.5f,7,5};var engage=new[]{4.5f,1.7f,2.8f,1.8f,0,0,6,2.6f};
            var warnings=new[]{.35f,1f,.45f,.4f,0,0,.7f,.65f};var windups=new[]{.35f,.65f,.45f,.45f,0,0,.85f,1f};var actions=new[]{.4f,.2f,.25f,.2f,0,0,.65f,.25f};var recoveries=new[]{1.2f,1.5f,1.4f,.95f,0,0,2f,1.8f};
            var counts=new[]{10,8,6,8,6,6,3,5};var delay=new[]{180,300,180,360,300,240,600,450};var targets=new[]{3,2,2,3,2,2,1,1};var chance=new[]{.75f,.45f,.5f,.4f,.5f,.5f,.2f,.55f};
            for(var n=0;n<Species.Length;n++)
            {
                var index=n;var name=Species[n];var id=ids[n];
                var behaviour=Make<WildlifeActionDefinition>(name+"Action",d=>{d.Attack=attacks[index];d.Movement=index==0 || index==5 ? WildlifeMovement.Flight : index==4 ? WildlifeMovement.Hop : WildlifeMovement.Walk;d.Camouflage=index==1;d.Defensive=index==1 || index==6;d.Detection=detection[index];d.Leash=index==2 ? 5 : 12;d.WalkSpeed=walk[index];d.ChaseSpeed=chase[index];d.ActionSpeed=action[index];d.FleeSpeed=chase[index];d.Reach=reach[index];d.EngageRange=engage[index];d.Damage=damage[index];d.WarningSeconds=warnings[index];d.WindupSeconds=windups[index];d.ActionSeconds=actions[index];d.RecoverySeconds=recoveries[index];});
                var creature=Make<CreatureDefinition>(name+"Creature",d=>{d.Id=id;d.Family=index==1 || index==2 || index==7 ? CreatureFamily.Invertebrate : CreatureFamily.Animal;d.Faction=CreatureFaction.Wildlife;d.Actions=behaviour;d.Habitat=habitats[index];d.Traits=(behaviour.Movement==WildlifeMovement.Flight ? CreatureTrait.Flying : CreatureTrait.None)|(index==1 ? CreatureTrait.Camouflage : CreatureTrait.None)|(index==3 ? CreatureTrait.PackAggro : index==4 ? CreatureTrait.Herding|CreatureTrait.FleeTogether : CreatureTrait.None);d.MinimumGroup=index==3 ? 2 : 1;d.MaximumGroup=targets[index];d.SeparationRadius=index==6 ? .8f : index==5 || index==0 ? .28f : .45f;d.PreferredSpacing=index==3 ? 2.4f : 1.8f;d.RoamRadius=index==2 ? 2.5f : 7;});
                var loot=Loot(name+"Loot",xp[n],0,n==0 ? new[]{Drop("medicinal_gland",1,1,.75f),Drop("bone",1,1,.4f)} : n==1 ? new[]{Drop("chitin",1,1),Drop("stone",1,2,.7f)} : n==2 ? new[]{Drop("medicinal_gland",1,1)} : n==3 ? new[]{Drop("bone",1,2),Drop("raw_meat",1,1),Drop("leather",1,1,.25f)} : n==4 ? new[]{Drop("bone",1,2),Drop("raw_meat",1,1),Drop("leather",1,1,.5f)} : n==5 ? new[]{Drop("feather",1,1,.75f),Drop("raw_meat",1,1,.25f)} : n==6 ? new[]{Drop("raw_meat",4,4),Drop("leather",2,2),Drop("bone",2,2)} : new[]{Drop("chitin",2,2),Drop("crystal",1,1,.15f)});
                var vitality=Make<VitalityDefinition>(name+"Vitality",v=>v.MaximumHealth=hp[index]);
                var prefab=BuildCreature(name,id,creature,vitality,loot);var at=rules.FindIndex(r=>r.Prefab.Member.Definition.Id==id);var rule=new AnimalPopulationRule{Prefab=prefab,Population=counts[n],RespawnSeconds=delay[n],RegionalTarget=targets[n],ChunkChance=chance[n]};if(at<0)rules.Add(rule);else rules[at]=rule;
            }
            population.Rules=rules.ToArray();EditorUtility.SetDirty(population);
        }
        private static NeutralAnimal BuildCreature(string name,string id,CreatureDefinition creature,VitalityDefinition vitality,LootTable loot)
        {
            var root=Group(name,null);Shadow(root,creature.SeparationRadius*3.4f,.45f);var visual=Group("Visual",root);var group=visual.gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>();root.gameObject.AddComponent<DepthSort>().Visual=group;var pose=Group("Body motion",visual);
            var body=Sprite("Body",id,pose,Vector2.zero,id=="giant_boar" ? 1.05f : .85f,0);
            var rigid=root.gameObject.AddComponent<Rigidbody2D>();rigid.bodyType=RigidbodyType2D.Kinematic;rigid.gravityScale=0;rigid.constraints=RigidbodyConstraints2D.FreezeRotation;rigid.interpolation=RigidbodyInterpolation2D.Interpolate;var collider=root.gameObject.AddComponent<CircleCollider2D>();collider.radius=creature.SeparationRadius;collider.isTrigger=true;
            var health=root.gameObject.AddComponent<Health>();health.Definition=vitality;root.gameObject.AddComponent<CombatAffiliation>().Relation=EntityRelation.WildAnimal;
            var animal=root.gameObject.AddComponent<NeutralAnimal>();animal.Definition=D<AnimalMovementDefinition>("AnimalMovement");animal.Visual=visual;animal.Health=health;animal.Member=root.gameObject.AddComponent<CreatureMember>();animal.Member.Definition=creature;animal.Separation=root.gameObject.AddComponent<SoftSeparation>();animal.Separation.Member=animal.Member;
            var brain=root.gameObject.AddComponent<WildlifeActionBrain>();brain.Animal=animal;brain.Pose=pose;brain.Body=body;brain.ActiveSprite=body.sprite;brain.RestSprite=creature.Actions.Camouflage ? VisualArtImporter.Sprite("stone_crawler_rest") : body.sprite;
            var appendage=creature.Actions.Movement==WildlifeMovement.Flight ? id=="bat" ? "bat_wing" : "crow_wing" : creature.Family==CreatureFamily.Invertebrate && creature.Actions.Attack!=WildlifeAttack.Ambush ? "crawler_legs" : null;
            if(appendage!=null){brain.LeftAppendage=Sprite("Left appendage",appendage,pose,new Vector2(-.65f,0),.72f,-1).transform;var right=Sprite("Right appendage",appendage,pose,new Vector2(.65f,0),.72f,-1);right.flipX=true;brain.RightAppendage=right.transform;}
            brain.Tell=Sprite("Warning", "wildlife_tell",root,Vector2.zero,1,WorldSorting.GroundDecoration+4);brain.Tell.enabled=false;
            var drop=root.gameObject.AddComponent<LootOnDeath>();drop.Health=health;drop.Table=loot;drop.Settings=D<GameplaySettings>("GameplaySettings");drop.Pickup=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/WorldPickup.prefab").GetComponent<WorldPickup>();Feedback(health,visual,HitReaction.Creature);HealthBar(health,1.5f,false);
            var prefab=PrefabUtility.SaveAsPrefabAsset(root.gameObject,"Assets/Wildfolk/Prefabs/"+name+".prefab").GetComponent<NeutralAnimal>();UnityEngine.Object.DestroyImmediate(root.gameObject);sources.Add(prefab.gameObject);return prefab;
        }
        private static void UpdateCamp()
        {var path="Assets/Wildfolk/Prefabs/GoblinCamp.prefab";var root=PrefabUtility.LoadPrefabContents(path);var camp=root.GetComponent<GoblinCamp>();camp.Supplies=root.transform.Find("Camp Supplies");camp.SupplyIcon=VisualArtImporter.Sprite("camp_supplies");camp.OpenedSprite=VisualArtImporter.Sprite("camp_supplies_open");camp.SupplyVisual=camp.Supplies.Find("Box").GetComponent<SpriteRenderer>();camp.SupplyVisual.sprite=camp.SupplyIcon;PrefabUtility.SaveAsPrefabAsset(root,path);PrefabUtility.UnloadPrefabContents(root);sources.Add(AssetDatabase.LoadAssetAtPath<GameObject>(path));sources.Add(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/SlingshotGoblin.prefab"));}
        private static void RegisterNetwork()
        {
            var ecology=PrefabUtility.LoadPrefabContents(NetworkEcologyBuilder.PrefabPath);var world=ecology.GetComponent<NetworkCreatureWorld>();var list=new List<NetworkSpawnEntry>(world.Prefabs);
            foreach(var source in sources)
            {
                var root=UnityEngine.Object.Instantiate(source);root.name="Network"+source.name;root.AddComponent<NetworkObject>();
                var animal=root.GetComponent<NeutralAnimal>();
                if(root.GetComponent<CreatureMember>()!=null){var nt=root.AddComponent<NetworkTransform>();nt.SyncPositionZ=false;nt.SyncRotAngleX=nt.SyncRotAngleY=nt.SyncRotAngleZ=false;nt.SyncScaleX=nt.SyncScaleY=nt.SyncScaleZ=false;nt.PositionThreshold=.04f;nt.Interpolate=true;nt.UseUnreliableDeltas=true;root.AddComponent<NetworkRigidbody2D>();root.AddComponent<NetworkCreature>().Visual=animal!=null ? animal.Visual : root.GetComponent<GoblinController>().Pose;}
                if(root.GetComponent<GoblinCamp>()!=null)root.AddComponent<NetworkCamp>();
                if(root.GetComponent<WorldPlaceable>()!=null){root.AddComponent<NetworkBuilding>();if(root.GetComponent<SpikeTrap>()!=null)root.AddComponent<NetworkTrap>();}
                var assetName=System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(source));var result=PrefabUtility.SaveAsPrefabAsset(root,"Assets/Wildfolk/Prefabs/Network"+assetName+".prefab");UnityEngine.Object.DestroyImmediate(root);list.RemoveAll(e=>e.Source==source);list.Add(new NetworkSpawnEntry{Source=source,NetworkPrefab=result});
            }
            world.Prefabs=list.ToArray();PrefabUtility.SaveAsPrefabAsset(ecology,NetworkEcologyBuilder.PrefabPath);PrefabUtility.UnloadPrefabContents(ecology);
            var session=PrefabUtility.LoadPrefabContents(MultiplayerSceneBuilder.SessionPrefabPath);session.GetComponent<NetworkManager>().NetworkConfig.ProtocolVersion=NetworkProtocol.Current;PrefabUtility.SaveAsPrefabAsset(session,MultiplayerSceneBuilder.SessionPrefabPath);PrefabUtility.UnloadPrefabContents(session);
        }
    }
}
