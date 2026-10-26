"""Deterministic projections from canonical data. Assumptions, not measured play."""
import json,datetime,csv,pathlib,sys
root=pathlib.Path(__file__).resolve().parents[1]
catalog=json.loads((root/'client/Assets/Wildfolk/Resources/EconomyCatalog.json').read_text(encoding='utf8'))
columns=['hostiles','undead','goblins','wildlife','harvests','crafts','fish','builds','nights','rescues','bosses','gold_classes','class_nodes','workbench_ii','workbench_iii','diamond','cavern','deep','biomes','crown','summoning','camp']
rows=[
 ('30m',.5,1,[8,3,0,1,35,12,4,4,1,0,0,0,0,0,0,0,0,0,1,0,0,0]),
 ('1h',1,1,[25,12,5,3,80,25,8,8,4,0,0,1,1,1,0,0,0,0,2,0,0,1]),
 ('3h',3,2,[80,40,20,15,220,70,22,22,10,1,0,2,3,1,0,0,1,0,3,0,0,1]),
 ('5h',5,4,[150,80,40,30,380,110,40,40,20,2,0,3,4,1,1,1,1,1,4,0,1,1]),
 ('10h',10,7,[300,200,90,70,650,180,80,65,30,4,1,4,6,1,1,1,1,1,4,0,1,1]),
 ('20h',20,14,[360,240,180,120,900,240,120,100,30,12,3,4,6,1,1,1,1,1,4,0,1,1]),
 ('30d-month-aligned',30,30,[360,240,180,120,900,240,120,100,30,12,3,4,6,1,1,1,1,1,4,0,1,1]),
 ('90d-month-aligned',90,90,[360,240,180,120,900,240,120,100,30,30,3,4,6,1,1,1,1,1,4,0,1,1]),
]
def attendance(start,days,claims=None):
 counts={};total=0
 for i in range(days):
  if claims is not None and i%30>=claims:continue
  date=start+datetime.timedelta(days=i);month=date.strftime('%Y-%m');slot=counts.get(month,0)
  if slot<28:total+=catalog['Monthly'][slot];counts[month]=slot+1
 return total
def project(label,hours,days,values,start):
 metrics=dict(zip(columns,values));achievement=sum(t['GemReward'] for f in catalog['Families'] for t in f['Tiers'] if metrics[f['Metric']]>=t['Threshold'])
 newcomer=sum(catalog['Newcomer'][:min(7,days)]);monthly=attendance(start,days);total=achievement+newcomer+monthly
 return {'scenario':label,'playHoursAssumed':hours,'calendarDays':days,'achievement':achievement,'newcomer':newcomer,'monthly':monthly,'total':total,'revivesIfNoUnlock':total//catalog['ReviveCost'],'engineerProgress':round(min(1,total/catalog['EngineerCost']),3),'afterEngineer':max(0,total-catalog['EngineerCost']),'revivesAfterEngineer':max(0,total-catalog['EngineerCost'])//catalog['ReviveCost']}
def main():
 out=pathlib.Path(sys.argv[1]) if len(sys.argv)>1 else root/'validation/launch-economy-foundation'
 out.mkdir(parents=True,exist_ok=True);result=[project(*row,datetime.date(2026,11,1)) for row in rows]
 for days in [30,90]:result.append(project(str(days)+'d-start-Oct26',days,days,rows[-2 if days==30 else -1][3],datetime.date(2026,10,26)))
 totals={'families':len(catalog['Families']),'tiers':sum(len(f['Tiers']) for f in catalog['Families']),'special':sum(len(f['Tiers'])==1 for f in catalog['Families']),'achievementGem':sum(t['GemReward'] for f in catalog['Families'] for t in f['Tiers']),'achievementPoints':sum(t['AchievementPoints'] for f in catalog['Families'] for t in f['Tiers']),'newcomerGem':sum(catalog['Newcomer']),'monthlyGem':sum(catalog['Monthly']),'engineerCost':catalog['EngineerCost'],'reviveCost':catalog['ReviveCost']}
 assert totals['tiers']==58 and totals['newcomerGem']==45 and totals['monthlyGem']==40
 assert result[0]['total']>=catalog['ReviveCost'] and result[4]['total']>=catalog['EngineerCost']
 with (out/'economy-scenarios.csv').open('w',encoding='utf8',newline='') as f:w=csv.DictWriter(f,fieldnames=result[0].keys());w.writeheader();w.writerows(result)
 document={'method':'Explicit low-frequency play assumptions, not observed play/retention/earnings. Crown RNG is excluded; co-op milestones require co-op. Hour scenarios use spread-out days as stated; 30/90-day values do not imply those hours are required.','totals':totals,'scenarios':result,'conservativeAttendance20Claims':{str(n):attendance(datetime.date(2026,11,1),n,20) for n in [30,90]},'adsAndIapNow':0,'futureExtension':'Trusted verified provider receipt -> internal server grant -> same CAS ledger; no client grant, pricing, ad SDK or billing implemented. Each extra trusted Gem grant adds to the same supply model; no assumed ad revenue or purchase conversion.','requestEstimate':{'batchSeconds':300,'30mProgressCalls':6,'1hProgressCalls':12,'menuReadAndClaimsAssumed':6,'oneEconomyCallTypical':'1 CloudCode + 1 Auth GET + 1 Private policy/config batch GET + 1 account GET + 2 economy GET + optional 1 CAS write. First creation adds guarded creation reads/writes. Alpha/Production progress adds actual Integrity verification. CAS/client retry bounded3 each','notIncluded':'Real-world idle/menu patterns, latency, multi-device concurrency and operator access are unmeasured.'}}
 (out/'economy-model.json').write_text(json.dumps(document,ensure_ascii=False,indent=2)+'\n',encoding='utf8');print(json.dumps({'totals':totals,'scenarios':result},ensure_ascii=False))
if __name__=='__main__':main()
