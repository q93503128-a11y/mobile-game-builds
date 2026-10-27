'use strict';
// Deterministic lifecycle/storage model. No network latency, real provider rewards or revenue implied.
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict');
const {fresh,transact,validate}=require('../src/economy.cjs'),L=require('../src/ledger.cjs');
const {Store,ctx}=require('../tests/lifecycle-helper.cjs');
async function main(){
 const out=path.resolve(process.argv[2]||path.join(__dirname,'../../validation/economy-premium-mp-ledger'));fs.mkdirSync(out,{recursive:true});
 const results=[];const now=Date.parse('2026-10-27T00:00:00Z');
 for(const total of [10000,50000,100000]){
  const start=performance.now(),store=new Store(),p=L.upgrade(fresh(ctx,now));await store.create('player',()=>p);
  for(let i=1;i<=total;i++){
   if(p.Ledger.length>=L.SOFT)await L.compact(store,p);
   await transact(p,ctx,1,'provider_receipt_model','fixture:'+i.toString(16).padStart(64,'0'),i.toString(16).padStart(32,'0'),now,{},store);
  }
  validate(p,ctx);assert.equal(p.Balance,total);
  for(const seq of [1,Math.floor(total/2),total]){const row=await L.findSource(store,p,'fixture:'+seq.toString(16).padStart(64,'0'));assert.equal(row.Sequence,seq);assert.equal(row.BalanceAfter,seq);}
  const before=p.Balance;await transact(p,ctx,1,'provider_receipt_model','fixture:'+('1'.padStart(64,'0')),'f'.repeat(32),now,{},store);assert.equal(p.Balance,before);
  let cursor,complete=false;while(!complete){const check=await L.reconcile(store,p,cursor,8);cursor=check.cursor;complete=check.complete;}
  const archiveBytes=[...store.api.data.values()].filter(x=>x.key==='segment').reduce((n,s)=>n+L.bytes(s.value),0);
  const indexBytes=[...store.api.data.values()].filter(x=>x.key.startsWith('s_')).reduce((n,s)=>n+L.bytes(s.value)+s.key.length,0);
  const rows=p.Ledger.map(L.bytes),sample={minimum:Math.min(...rows),maximum:Math.max(...rows),mean:rows.reduce((n,s)=>n+s,0)/rows.length};
  results.push({transactions:total,...L.capacity(p),archiveBytes,indexBytes,estimatedCloudSaveBytes:L.bytes(p)+archiveBytes+indexBytes,privateKeys:store.api.data.size,privateCustomItems:new Set([...store.api.data.keys()].map(k=>k.split(':')[0])).size,largestIndexBucketKeys:Math.max(...Object.values(p.ReceiptBuckets).map(x=>x.Keys)),rowSerializedBytes:sample,fixtureApiCalls:store.api.calls,modelWallSeconds:(performance.now()-start)/1000,sourceLookupAndDuplicateChecks:true,fullChainReconciled:true});
  process.stdout.write('Model '+total+' passed\n');
 }
 const projections=[];
 for(const days of [30,90,365,1095])for(const ads of [0,3,5,10])for(const iap of [0,1,3]){
  const months=Math.ceil(days/30),revives=Math.floor(days/7),rows=58+7+1+months*4+ads*days+iap*months+revives+Math.ceil(revives*.05);
  const archived=Math.max(0,Math.floor((rows-L.SEGMENT)/L.SEGMENT)),hot=rows-archived*L.SEGMENT;
  const average=results.at(-1).rowSerializedBytes.mean;
  projections.push({days,adsPerDayAssumed:ads,iapPerMonthAssumed:iap,revivesPerWeekAssumed:1,refundRatioAssumed:.05,rows,archiveSegments:archived,hotRows:hot,ledgerBytesApprox:Math.ceil(rows*average),receiptIndexBytesApprox:archived*L.SEGMENT*300,legacy8192RowsExceeded:rows>8192});
 }
 const document={classification:'DETERMINISTIC MODEL / NO REAL REQUEST LATENCY',limits:{segmentRows:L.SEGMENT,archiveTrigger:L.SOFT,hotHard:L.HARD,rootHardBytes:L.ROOT_BYTES,indexShards:256,indexKeysHardPerShard:2000,indexBytesHardPerShard:L.INDEX_BYTES},scale:results,projections,assumptions:'One-time58 achievement+7 newcomer+1 Engineer, monthly4 Gem transactions; hypothetical ads and IAP are receipt counts only, no ad policy/package/value/revenue decision. One revive/week and5% refunds. Source index bytes estimated300/key for projections; exact fixture sizes above. Private Custom Item has2000keys/5MiB per access class and no item-count cap per official docs. Paid quota/storage/egress and deletion work grow with history.'};
 fs.writeFileSync(path.join(out,'ledger-model.json'),JSON.stringify(document,null,2));
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});
