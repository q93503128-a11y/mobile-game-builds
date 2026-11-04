import sys,json,time
sys.dont_write_bytecode=True
import audit_session as a
from live_inspect import run,capture
def find(t):return 'UnityEngine.Object.FindObjectsByType<Wildfolk.'+t+'>(UnityEngine.FindObjectsInactive.Include,UnityEngine.FindObjectsSortMode.None).First()'
ops=[]
def step(name,body):
    r=run(body+';return "navigation-only";');time.sleep(.25);result=capture(name);ops.append({'name':name,'code':body,'result':r,'capture':result});a.save('live-frontend-operations',ops);print(name,flush=True)
h=find('HomeScreen');e=find('EconomyScreen');s=find('StoreScreen');p=find('AccountProfileScreen');c=find('ClassLobbyScreen');m=find('MultiplayerMenu')
for w,height in [(1920,1080),(1280,720),(844,390)]:
 for lang in ['en','ko']:
    prefix=lang+'-'+str(w)
    step('home-v2-'+prefix,e+'.Back();'+e+'.CloseHub();'+h+'.ShowHome();'+h+'.Preferences.SetLanguage('+str(0 if lang=='en' else 1)+');Wildfolk.Editor.VisualGameView.Show('+str(w)+','+str(height)+')')
    step('settings-v2-'+prefix,h+'.OpenSettings()')
    step('achievements-v2-'+prefix,h+'.ShowHome();'+e+'.OpenAchievements()')
    step('attendance-v2-'+prefix,e+'.Back();'+e+'.OpenAttendance();'+e+'.Board(0)')
    step('attendance-monthly-v2-'+prefix,e+'.Board(1)')
step('store-ko-844',e+'.Back();'+h+'.ShowHome();'+s+'.Open()')
step('store-help-ko-844',s+'.Contact()')
step('store-help-back-ko-844',p+'.CloseHelp()')
step('profile-ko-844',p+'.Back()')
step('account-ko-844',e+'.OpenAccount()')
step('delete-unconfirmed-ko-844',p+'.Delete()')
step('class-overview-ko-844',p+'.CancelDelete();'+p+'.Back();'+e+'.CloseHub();'+c+'.Open()')
step('class-tree-ko-844',c+'.OpenTree()')
step('multiplayer-internet-ko-844',c+'.Back();'+c+'.Back();'+h+'.OpenMultiplayer()')
step('multiplayer-invalid-ko-844',m+'.Destination.text="!";'+m+'.Join()')
step('multiplayer-lan-ko-844',m+'.ToggleRoute()')
step('home-restored-ko-844',m+'.Back()')
a.phase('Unity 격리·전면 메뉴 3해상도 한영 관찰 및 소스 대조')
