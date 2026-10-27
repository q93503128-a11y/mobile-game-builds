'use strict';
const test=require('node:test'),assert=require('node:assert/strict');
const {EconomyBackend,fresh,transact,validate}=require('../src/economy.cjs');
const L=require('../src/ledger.cjs'),{engineerOwnership}=require('../src/economy-ownership.cjs');
const {Store,ctx,clone}=require('./lifecycle-helper.cjs');
test('Private IDs and receipt keys fit legacy50-character service limits without truncating SHA256',()=>{
 for(const text of ['','ascii','한글 receipt 😀',JSON.stringify(ctx)]){const hex=L.hash(text);assert.equal(L.compactHash(hex),Buffer.from(hex,'hex').toString('base64url'));}
 assert.ok(L.custom('a'.repeat(96),'dev','ledger',999999).length<=50);assert.equal(L.compactHash('f'.repeat(64)).length,43);
});
test('Cloud Save property reordering preserves canonical immutable segment hashes',async()=>{
 const s=await setup(512);await s.backend.maintain(ctx);const segment=await s.store.ledger.archive('player',1);
 const reorder=x=>Array.isArray(x)?x.map(reorder):x&&typeof x==='object'?Object.fromEntries(Object.entries(x).reverse().map(([k,v])=>[k,reorder(v)])):x;
 assert.deepEqual(L.validateSegment(reorder(segment),'player','dev',1).Rows,segment.Rows);
 await s.store.ledger.putArchive('player',reorder(segment));assert.equal(L.hash(reorder(segment)),L.hash(segment));
});
let serial=0;const op=()=> (++serial).toString(16).padStart(32,'0'),time=Date.parse('2026-10-01T00:00:00Z');
async function setup(rows=0){
 const store=new Store(),p=fresh(ctx,time);for(let i=0;i<rows;i++)await transact(p,ctx,1,'admin_adjustment','admin:'+op(),op(),time,{},store);
 await store.create('player',()=>p);const backend=new EconomyBackend(store,{environmentId:'dev',channel:'Development'},undefined,()=>time);return {store,backend,p:()=>store.records.get('player').value};
}
test('Economy1 migration preserves all transaction bytes, balance, claims and Engineer receipt',async()=>{
 const s=await setup(510);let p=s.p();p.Claimed=['hostiles:0'];const tx=await transact(p,ctx,-120,'class_unlock','class:engineer',op(),time,{},s.store);p.Engineer={Id:'class_engineer',TransactionId:tx.TransactionId};
 await transact(p,ctx,1,'admin_adjustment','admin:'+op(),op(),time,{},s.store);const original=clone(p.Ledger),balance=p.Balance;
 await s.backend.maintain(ctx);p=s.p();assert.equal(p.Schema,2);assert.equal(p.Archive.Count,1);assert.equal(p.Ledger.length,256);assert.equal(p.Balance,balance);assert.deepEqual(p.Claimed,['hostiles:0']);assert.equal(engineerOwnership(p,ctx),true);
 const segment=await s.store.ledger.archive('player',1);assert.deepEqual([...segment.Rows,...p.Ledger],original);validate(p,ctx);
});
for(const boundary of ['before_segment','after_segment','after_indexes','after_root','completed'])test('archive crash boundary '+boundary+' retries without loss or duplicate',async()=>{
 const s=await setup(512),original=clone(s.p().Ledger);let failed=false;
 const write=s.store.ledger.putArchive.bind(s.store.ledger),index=s.store.ledger.putIndexes.bind(s.store.ledger);
 s.store.ledger.putArchive=async(...a)=>{if(!failed&&boundary==='before_segment'){failed=true;throw Error('A');}await write(...a);if(!failed&&boundary==='after_segment'){failed=true;throw Error('B');}};
 s.store.ledger.putIndexes=async(...a)=>{await index(...a);if(!failed&&boundary==='after_indexes'){failed=true;throw Error('B-index');}};
 if(boundary==='after_root')s.store.lost=true;
 try{await s.backend.maintain(ctx);}catch(e){assert.ok(['A','B','B-index','unknown root commit'].includes(e.message));}
 await s.backend.maintain(ctx);assert.equal(s.p().Archive.Count,1);assert.equal(s.p().Balance,512);assert.deepEqual([...(await s.store.ledger.archive('player',1)).Rows,...s.p().Ledger],original);
 assert.equal((await L.reconcile(s.store,s.p())).complete,true);assert.equal(s.store.api.data.size,257);
});
test('immutable index retry and competing root CAS never lose committed archived transactions',async()=>{
 const s=await setup(512);s.store.conflicts=1;await s.backend.maintain(ctx);assert.equal(s.p().Archive.Count,1);assert.equal((await L.reconcile(s.store,s.p())).balance,512);
 const source=(await s.store.ledger.archive('player',1)).Rows[0].SourceId;assert.equal((await L.findSource(s.store,s.p(),source)).Sequence,1);
});
test('recent UI history stays hot; admin pages through hot/archive, direct transaction/source lookup and reconciliation',async()=>{
 const s=await setup(512);await s.backend.maintain(ctx);const page=await L.history(s.store,s.p(),258,4);assert.deepEqual(page.transactions.map(t=>t.Sequence),[257,256,255,254]);
 const old=page.transactions[2];assert.deepEqual(await L.transaction(s.store,s.p(),old.TransactionId),old);assert.deepEqual(await L.findSource(s.store,s.p(),old.SourceId),old);
 const ui=await s.backend.execute(ctx,'TransactionHistory',{offset:0,limit:50});assert.equal(ui.transactions.length,50);assert.ok(ui.transactions.every(t=>t.Sequence>256));assert.equal(ui.transactionCount,512);
});
test('archived admin adjustment remains duplicate-safe with reason/operator/case conflict detection',async()=>{
 const s=await setup(512),old=s.p().Ledger[0];old.Operator='operator';old.CaseReference='case';old.AdjustmentReason='Confirmed reimbursement';await s.backend.maintain(ctx);
 const request={operationId:old.SourceId.slice(6),delta:1,reason:old.AdjustmentReason},actor={authorized:true,operator:'operator',caseReference:'case',apply:true};
 const r=await s.backend.adminAdjust(ctx,request,actor);assert.equal(r.duplicate,true);assert.equal(s.p().Balance,512);
 await assert.rejects(s.backend.adminAdjust(ctx,{...request,delta:2},actor),/source_conflict/);
});
test('archived achievement/attendance claim state prevents replay; financial Ads/IAP fixtures retain source proof',async()=>{
 const s=await setup();s.p().Metrics.hostiles=5;
 await s.backend.execute(ctx,'ClaimAll',{operationId:op()});await s.backend.execute(ctx,'ClaimAttendance',{operationId:op(),board:'newcomer'});
 let p=s.p();for(const kind of ['future-ad:sha256-fixture','future-iap:sha256-fixture'])await transact(p,ctx,2,'fixture_verified_provider',kind,op(),time,{},s.store);
 while(p.Ledger.length<512)await transact(p,ctx,1,'admin_adjustment','admin:'+op(),op(),time,{},s.store);
 await s.backend.maintain(ctx);p=s.p();const before=p.Balance;
 assert.equal((await s.backend.execute(ctx,'ClaimAll',{operationId:op()})).granted,0);assert.equal((await s.backend.execute(ctx,'ClaimAttendance',{operationId:op(),board:'newcomer'})).alreadyClaimed,true);
 p=s.p();for(const kind of ['future-ad:sha256-fixture','future-iap:sha256-fixture'])await transact(p,ctx,2,'fixture_verified_provider',kind,op(),time,{},s.store);
 assert.equal(p.Balance,before);assert.equal(L.sequence(p),512);
});
test('resolved revive pruning retains terminal outcome, repeated begin/finish cannot revive/refund twice',async()=>{
 const s=await setup(100);let first;
 for(let i=0;i<36;i++){const source=op();first=first||source;assert.equal((await s.backend.execute(ctx,'BeginRevive',{operationId:op(),source,expectedCost:8})).ok,true);await s.backend.execute(ctx,'FinishRevive',{operationId:op(),source,outcome:'failed'});}
 await s.backend.maintain(ctx);assert.equal(Object.values(s.p().Revives).length,32);assert.equal((await s.store.ledger.resolution('player',first)).State,'Refunded');
 const before=s.p().Balance;assert.equal((await s.backend.execute(ctx,'BeginRevive',{operationId:op(),source:first,expectedCost:8})).revive.State,'Refunded');await s.backend.execute(ctx,'FinishRevive',{operationId:op(),source:first,outcome:'committed'});assert.equal(s.p().Balance,before);
});
test('capacity warnings, index hard cap and corrupt segment fail closed without truncating hot proof',async()=>{
 const s=await setup(512);assert.equal(L.capacity(L.upgrade(s.p())).Status,'Warning');const put=s.store.ledger.putIndexes.bind(s.store.ledger);s.store.ledger.putIndexes=async()=>{throw Error('receipt_capacity');};await assert.rejects(s.backend.maintain(ctx),/receipt_capacity/);assert.equal(s.p().Ledger.length,512);s.store.ledger.putIndexes=put;await s.backend.maintain(ctx);
 const key=L.custom('player','dev','ledger',1)+':segment';s.store.api.data.get(key).value.Rows[0].Delta=900;await assert.rejects(L.reconcile(s.store,s.p()),/archive_hash_mismatch/);
});
test('future financial receipt boundary permanently binds a hashed provider receipt to one player before grant',async()=>{
 const s=await setup(),receipt={Verified:true,Kind:'iap',Provider:'google_play',ReceiptHash:'a'.repeat(64),PlayerId:'player',Placement:'fixture',VerifiedAt:time};
 const first=await s.store.ledger.bindVerifiedFinancialReceipt(receipt,work=>s.store.withCreationGate(work));
 assert.deepEqual(await s.store.ledger.bindVerifiedFinancialReceipt({...receipt,VerifiedAt:time+999},work=>s.store.withCreationGate(work)),first);
 await assert.rejects(s.store.ledger.bindVerifiedFinancialReceipt({...receipt,PlayerId:'other'},work=>s.store.withCreationGate(work)),/financial_receipt_owner_conflict/);
 await assert.rejects(s.store.ledger.bindVerifiedFinancialReceipt({...receipt,Verified:false},work=>s.store.withCreationGate(work)),/unverified_financial_receipt/);
 assert.equal(s.p().Balance,0);assert.equal(s.p().Ledger.length,0);
});
test('index warning/hard key thresholds are checked before root truncation',async()=>{
 const s=await setup(512),p=s.p(),counts={};for(const t of p.Ledger.slice(0,256)){const b=L.sourceHash('player','dev',t.SourceId).slice(0,2);counts[b]=(counts[b]||0)+1;}
 const bucket=Object.keys(counts).find(b=>counts[b]>1);p.ReceiptBuckets={[bucket]:{Keys:1999,Bytes:1000}};
 await assert.rejects(s.backend.maintain(ctx),/receipt_capacity/);assert.equal(s.p().Ledger.length,512);assert.equal(s.p().Archive,undefined);
 const empty=L.upgrade(fresh(ctx,time));empty.ReceiptBuckets={aa:{Keys:1500,Bytes:1000}};assert.equal(L.capacity(empty).Status,'Warning');empty.ReceiptBuckets.aa.Keys=1800;assert.equal(L.capacity(empty).Status,'Critical');
});
test('full legacy8192 ledger migrates incrementally with bounded one-segment maintenance and unchanged balance',async()=>{
 const store=new Store(),p=fresh(ctx,time);p.Ledger=Array.from({length:8192},(_,i)=>({TransactionId:'G-'+(i+1)+'-'+L.hash(['legacy',i]).slice(0,12),Sequence:i+1,PlayerId:'player',Timestamp:new Date(time).toISOString(),Delta:1,BalanceBefore:i,BalanceAfter:i+1,Reason:'legacy_fixture',SourceId:'legacy:'+i,OperationId:i.toString(16).padStart(32,'0'),BuildChannel:'Development',MetadataVersion:1}));p.Balance=8192;validate(p,ctx);await store.create('player',()=>p);
 const b=new EconomyBackend(store,{environmentId:'dev',channel:'Development'});let rounds=0;while(store.records.get('player').value.Ledger.length>=512){await b.maintain(ctx);rounds++;assert.ok(rounds<=32);}
 const current=store.records.get('player').value;assert.equal(rounds,31);assert.equal(current.Ledger.length,256);assert.equal(current.Balance,8192);assert.equal((await L.findSource(store,current,'legacy:0')).TransactionId,p.Ledger[0].TransactionId);
});
