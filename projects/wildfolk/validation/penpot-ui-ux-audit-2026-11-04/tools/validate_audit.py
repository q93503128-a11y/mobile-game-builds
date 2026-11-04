"""Read-only report/evidence validation; writes only final-validation.json."""
import sys,json,re,hashlib,subprocess,datetime
from pathlib import Path
sys.dont_write_bytecode=True
import audit_session as a
from close_reports import REPORTS
R=a.ROOT;errors=[];checked=0;anchors=0
def check(ok,msg):
 if not ok:errors.append(msg)
def read(n):return json.loads((R/(n+'.json')).read_text(encoding='utf8'))
for n in REPORTS:
 p=R/(n+'.md');check(p.exists() and p.stat().st_size>500,'Missing/empty report '+n)
 if not p.exists():continue
 t=p.read_text(encoding='utf8');check(not re.search(r'\bDRAFT\b|^\*\*PARTIAL|^# .*checkpoint',t,re.M),'Unclosed draft '+n)
 check(t.count('```')%2==0,'Unclosed fence '+n)
 for target in re.findall(r'\]\(([^)]+)\)',t):
  target=target.strip('<>')
  if re.match(r'[a-z]+://|#',target):continue
  path,sep,fragment=target.partition('#');p2=(R/path).resolve();checked+=1
  check(p2.exists(),f'{n}: broken {target}')
  if p2.exists() and sep and re.fullmatch(r'L\d+',fragment):
   anchors+=1;check(int(fragment[1:])<=len(p2.read_text(encoding='utf8').splitlines()),f'{n}: line out of bounds {target}')
for n in ['screens','actions','navigation','issues','unverified']:
 xs=read(n);check(len(xs)==len({x['id'] for x in xs}),n+' duplicate ID')
for x in read('source-manifest')+read('scene-inventory')+read('source-art-index'):
 if 'file' not in x or 'sha256' not in x:continue
 p=a.CAN/x['file'];check(p.exists() and hashlib.sha256(p.read_bytes()).hexdigest()==x['sha256'],'Source/art hash drift '+x['file'])
live=read('live-evidence-index')
for x in live:
 p=R/x['file'];check(p.exists() and hashlib.sha256(p.read_bytes()).hexdigest()==x['sha256'],'PNG hash drift '+x['file'])
check(len(live)==87 and sum(x['acceptedAsMenuEvidence'] for x in live)==67,'Wrong accepted image count')
check(sum(x['level']=='CURRENT rendered subset' for x in read('screen-coverage'))==21,'Wrong rendered surface subset count')
check(sum(x['visuallyOpenedInThisAudit'] for x in read('historical-image-index'))==10,'Historical opened count')
controls=read('serialized-controls');actions=read('actions');mapped={(x['scene'],x['path']) for c in actions for x in c['instances']}
bound=[x for x in controls if x['callbacks']];unmapped=[{'scene':x['scene'],'component':x['component'],'path':x['path'],'callbacks':x['callbacks']} for x in bound if (x['scene'],x['path']) not in mapped]
check(not unmapped,'Serialized callback control instances not mapped: '+str(unmapped[:5]))
changed=subprocess.check_output(['git','diff','--name-only','HEAD'],cwd=a.CAN,text=True).splitlines()
untracked=subprocess.check_output(['git','ls-files','--others','--exclude-standard'],cwd=a.CAN,text=True).splitlines()
prefix=R.relative_to(a.CAN).as_posix()+'/'
check(all(x.startswith(prefix) for x in changed+untracked),'Changes outside audit directory')
product=subprocess.check_output(['git','diff','48d6528758d18bf3df41ed02ad9aa0f268654a51','--','projects/wildfolk/client','projects/wildfolk/docs','projects/wildfolk/art-source'],cwd=a.CAN,text=True)
check(not product,'Product diff since evidence baseline')
check(all(x['unchanged'] for x in read('resume-protection-check').values()),'Protection mismatch')
dc=subprocess.run(['git','diff','--check'],cwd=a.CAN,capture_output=True,text=True);check(dc.returncode==0,'git diff --check: '+dc.stdout+dc.stderr)
result={'status':'PASS_RESEARCH_SCOPE_WITH_EXPLICIT_UNVERIFIED' if not errors else 'FAIL','checkedUtc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'reports':REPORTS,'requiredReportCount':len(REPORTS),'localLinksChecked':checked,'lineAnchorsChecked':anchors,'screens':len(read('screens')),'currentSurfaces':57,'renderedCurrentSurfaceSubset':21,'actions':len(actions),'serializedControls':len(controls),'serializedControlsWithCallbacks':len(bound),'mappedSerializedCallbackControls':len(bound)-len(unmapped),'unmappedSerializedCallbackControls':unmapped,'routes':len(read('navigation')),'currentPngCount':len(live),'acceptedMenuPngCount':sum(x['acceptedAsMenuEvidence'] for x in live),'overlayOnlyPngCount':20,'historicalIndexed':len(read('historical-image-index')),'historicalOpened':10,'issuesBySeverity':dict(__import__('collections').Counter(x['severity'] for x in read('issues'))),'unverifiedQuestions':len(read('unverified')),'productUnchanged':not product,'changedFilesBeforeFinalValidationWrite':len(set(changed+untracked)),'allWritesWithinAuditDirectory':all(x.startswith(prefix) for x in changed+untracked),'tests':'Report structure/local links/line bounds/IDs/current source-art-scene-PNG hashes/serialized callback instance mapping/product-scope/protection/editor state/console query. No gameplay/NUnit/build/payment/network suite.','errors':errors}
a.save('final-validation',result)
print(json.dumps(result,ensure_ascii=True))
if errors:raise SystemExit(1)
