"""Navigate existing controls in isolated Play. No grants, movement, crafting or save actions."""
import sys,time,json
sys.dont_write_bytecode=True
import resume_audit as r
import audit_session as a
from live_inspect import run,capture
def obj(cls):return 'UnityEngine.Object.FindObjectsByType<Wildfolk.'+cls+'>(UnityEngine.FindObjectsInactive.Include,UnityEngine.FindObjectsSortMode.None).First()'
def step(name,body):
 result=run('if(Wildfolk.TransitionOverlay.Visible)throw new Exception("Transition not settled");'+body+';return new {scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,state=UnityEngine.Object.FindFirstObjectByType<Wildfolk.GameplaySession>().State.ToString()};');time.sleep(.4);shot=capture(name);log.append({'name':name,'navigation':body,'state':result,'capture':shot});a.save('resume-v2-navigation-observed',log);print(name,flush=True)
log=[]
for width,height in [(1920,1080),(1280,720),(844,390)]:
 for lang in ['en','ko']:
  suffix=lang+'-'+str(width)
  setup=obj('GameplaySession')+'.Resume();'+obj('GameplaySession')+'.CloseInventory();'+obj('GameplaySession')+'.CloseCharacter();'+obj('GameplaySession')+'.CloseSkillTree();'+obj('UserPreferences')+'.SetLanguage('+str(0 if lang=='en' else 1)+');Wildfolk.Editor.VisualGameView.Show('+str(width)+','+str(height)+')'
  step('resume-v2-inventory-'+suffix,setup+';'+obj('GameplaySession')+'.OpenInventory()')
  step('resume-v2-hand-crafting-'+suffix,obj('PlayerCrafting')+'.OpenHandCrafting()')
  if width==844:
   step('resume-v2-character-'+suffix,obj('PlayerCrafting')+'.CloseCrafting();'+obj('GameplaySession')+'.OpenCharacter()')
   step('resume-v2-common-tree-'+suffix,obj('CharacterScreen')+'.OpenTree()')
   step('resume-v2-pause-'+suffix,obj('SkillTreeScreen')+'.Back();'+obj('CharacterScreen')+'.Close();'+obj('GameplaySession')+'.OpenPause()')
   step('resume-v2-pause-settings-'+suffix,obj('PauseMenu')+'.OpenSettings()')
   run(obj('PauseMenu')+'.Back();return "settings closed";')
run(obj('GameplaySession')+'.Resume();'+obj('GameplaySession')+'.CloseInventory();'+obj('GameplaySession')+'.OpenPause();return "audit captures complete, game paused";')
r.phase('격리 gameplay 메뉴·한영3viewport 관찰')
