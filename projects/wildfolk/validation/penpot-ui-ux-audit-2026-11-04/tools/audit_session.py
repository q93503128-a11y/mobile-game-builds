from pathlib import Path
import sys, json, hashlib, datetime, importlib.util, subprocess
sys.dont_write_bytecode = True
ROOT = Path(__file__).resolve().parents[1]
PROJECT = ROOT.parents[1]
CAN = PROJECT.parents[1]
WORK = CAN.parent
ASSETS = PROJECT/'client/Assets/Wildfolk'
NATIVE = Path('C:/UnityProjects/Wildfolk')
PRIVATE = ROOT/'private'
PRIVATE.mkdir(exist_ok=True)
spec = importlib.util.spec_from_file_location('audit_mcp_client', WORK/'lowfire_mcp.py')
client = importlib.util.module_from_spec(spec); spec.loader.exec_module(client)
client.state = PRIVATE/'mcp-session.json'
def save(name, value):
    (ROOT/(name+'.json')).write_text(json.dumps(value,ensure_ascii=False,indent=2),encoding='utf-8')
def private_save(name, value):
    (PRIVATE/(name+'.json')).write_text(json.dumps(value,ensure_ascii=False,indent=2),encoding='utf-8')
def call(name, args):
    return client.invoke('tools/call',{'name':name,'arguments':args})
def unpack(result):
    d=result.get('result',{}); value=d.get('structuredContent')
    if value is None:value=json.loads(next(c['text'] for c in d.get('content',[]) if c['type']=='text'))
    return value.get('result',value)
def code(body):
    result=unpack(call('execute_code',{'action':'execute','code':body}))
    if not result.get('success'):raise RuntimeError(json.dumps(result,ensure_ascii=True))
    return result.get('data',{}).get('result')
def hash_file(path):
    return {'bytes':path.stat().st_size,'mtime_ns':path.stat().st_mtime_ns,'sha256':hashlib.sha256(path.read_bytes()).hexdigest()}
def protected_user_files():
    result={}
    for root in [NATIVE/'UserData', PROJECT/'client/UserData',Path('C:/Users/User/AppData/LocalLow/DefaultCompany/Wildfolk'),Path('C:/Users/User/AppData/LocalLow/DefaultCompany/Lowfire')]:
        if root.exists():
            for path in root.rglob('*'):
                if path.is_file() and 'Unity' not in path.relative_to(root).parts and path.name!='TestResults.xml':result[str(path)]=hash_file(path)
    return result
def native_files():
    return {str(path.relative_to(NATIVE)):hash_file(path) for folder in ['Assets/Wildfolk','ProjectSettings','Packages'] for path in (NATIVE/folder).rglob('*') if path.is_file()}
def phase(name):
    path=ROOT/'timing.json'; value=json.loads(path.read_text(encoding='utf-8')); now=datetime.datetime.now(datetime.timezone.utc).isoformat()
    value['phases'].append({'phase':name,'startedUtc':value['boundaryUtc'],'endedUtc':now,'seconds':(datetime.datetime.fromisoformat(now)-datetime.datetime.fromisoformat(value['boundaryUtc'])).total_seconds()})
    value['boundaryUtc']=now;save('timing',value)
if __name__=='__main__':
    assert PROJECT.name=='wildfolk' and CAN.name=='canonical', (PROJECT,CAN)
    if not (ROOT/'timing.json').exists():save('timing',{'startedUtc':'2026-11-04T01:23:13+00:00','boundaryUtc':'2026-11-04T01:23:13+00:00','initialPreparation':'First prompt/fetch before measurement excluded','tokenOrBillingUsage':None,'phases':[]})
    for name,fn in [('user-before',protected_user_files),('native-before',native_files)]:
        if not (PRIVATE/(name+'.json')).exists():private_save(name,fn())
    result=client.invoke('resources/read',{'uri':'mcpforunity://editor/state'});private_save('editor-before',result)
    print(json.dumps(result,ensure_ascii=True))
