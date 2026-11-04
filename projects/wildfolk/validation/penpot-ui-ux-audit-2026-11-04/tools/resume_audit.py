import sys,json,datetime,hashlib
sys.dont_write_bytecode=True
import audit_session as a
a.client.state=a.PRIVATE/'resume-mcp-session.json'
TIMING=a.ROOT/'timing-resume-20261104.json'
def phase(name):
 d=json.loads(TIMING.read_text(encoding='utf8'));now=datetime.datetime.now(datetime.timezone.utc).isoformat()
 d['phases'].append({'phase':name,'startedUtc':d['boundaryUtc'],'endedUtc':now,'seconds':(datetime.datetime.fromisoformat(now)-datetime.datetime.fromisoformat(d['boundaryUtc'])).total_seconds()});d['boundaryUtc']=now;TIMING.write_text(json.dumps(d,ensure_ascii=False,indent=2),encoding='utf8')
def prefs():
 import winreg
 result={}
 for path in [r'Software\DefaultCompany\Lowfire',r'Software\Unity\UnityEditor\DefaultCompany\Lowfire']:
  rows=[]
  try:
   with winreg.OpenKey(winreg.HKEY_CURRENT_USER,path) as k:
    i=0
    while True:
     try:n,v,t=winreg.EnumValue(k,i);i+=1;rows.append((n,str(v),t))
     except OSError:break
   result[path]={'values':len(rows),'sha256':hashlib.sha256(json.dumps(sorted(rows),ensure_ascii=True).encode()).hexdigest()}
  except FileNotFoundError:result[path]=None
 return result
if __name__=='__main__':
 if not TIMING.exists():TIMING.write_text(json.dumps({'startedUtc':'2026-11-04T04:42:41+00:00','boundaryUtc':'2026-11-04T04:42:41+00:00','tokenOrBillingUsage':None,'phases':[]},indent=2),encoding='utf8')
 for name,fn in [('resume-user-before',a.protected_user_files),('resume-native-before',a.native_files),('resume-prefs-before',prefs)]:
  if not (a.PRIVATE/(name+'.json')).exists():a.private_save(name,fn())
 for uri in ['mcpforunity://custom-tools','mcpforunity://editor/state']:
  r=a.client.invoke('resources/read',{'uri':uri});a.private_save('resume-'+uri.split('/')[-1],r)
 print('Resume baseline captured; user files/native files/preferences hashed privately.')
