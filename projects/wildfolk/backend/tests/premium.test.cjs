'use strict';
const test=require('node:test'),assert=require('node:assert/strict');
const {EconomyBackend}=require('../src/economy.cjs');
const {PremiumAdmissionBackend,TTL}=require('../src/premium-admission.cjs');
const {Store,ctx}=require('./lifecycle-helper.cjs');
let serial=0;const op=()=> (++serial).toString(16).padStart(32,'0');
async function setup(owned=true){
 const store=new Store(),accounts=new Store();let time=Date.parse('2026-10-27T00:00:00Z');const policy={environmentId:'dev',channel:'Development'},economy=new EconomyBackend(store,policy,undefined,()=>time);
 for(const playerId of ['player','host','other']){
  const context={...ctx,playerId};await accounts.create(playerId,()=>({Schema:1,PlayerId:playerId,Revision:1,SupportId:'LF-'+playerId}));await economy.execute(context,'GetEconomy');
 }
 if(owned){await economy.adminAdjust(ctx,{operationId:op(),delta:120,reason:'QA isolated preparation'},{authorized:true,operator:'qa',caseReference:'case',apply:true});await economy.execute(ctx,'UnlockEngineer',{operationId:op(),expectedCost:120});}
 const backend=new PremiumAdmissionBackend(store,accounts,policy,()=>time),binding={HostPlayerId:'host',SessionId:op(),AdmissionId:op(),Profile:op(),SelectedClassId:'engineer',OperationId:op()};
 return {store,accounts,economy,backend,binding,expire:()=>time+=TTL+1,issue:input=>backend.execute(ctx,'IssuePremiumAdmission',input||binding),verify:input=>backend.execute({...ctx,playerId:'host'},'VerifyPremiumAdmission',input)};
}
test('owned remote receives server snapshot; authenticated host verifies scope and source',async()=>{
 const s=await setup(),issued=await s.issue();assert.equal(issued.ok,true);const r=await s.verify(issued.admission);assert.equal(r.ok,true);assert.equal(r.admission.SelectedClassId,'engineer');assert.deepEqual(r.admission.AllowedClassIds,['engineer']);assert.equal(r.admission.PlayerId,'player');assert.equal(s.store.records.get('player').value.LastPremiumAdmission.Result,'verified');assert.equal(s.store.records.get('player').value.Balance,0);
});
test('unowned Engineer, local bool/receipt spoof and invalid selected class cannot issue',async()=>{
 const s=await setup(false);assert.equal((await s.issue({...s.binding,EngineerOwned:true,TransactionId:'fake'})).error,'premium_unowned');assert.equal((await s.issue({...s.binding,SelectedClassId:'made-up'})).error,'malformed_admission');assert.equal(s.store.records.get('player').value.Security.State,'Suspicious');
});
test('malformed proof is rejected and audited against caller, not innocent target',async()=>{
 const s=await setup();assert.equal((await s.verify({...s.binding,PlayerId:'player',ProofId:'fake'})).error,'malformed_admission');assert.equal(s.store.records.get('host').value.Security.State,'Suspicious');assert.equal(s.store.records.get('player').value.Security.State,'Normal');
});
for(const [field,value,error] of [['PlayerId','other','premium_player_mismatch'],['HostPlayerId','other','premium_host_mismatch'],['SessionId','f'.repeat(32),'premium_session_mismatch'],['AdmissionId','e'.repeat(32),'premium_session_mismatch'],['Profile','d'.repeat(32),'premium_session_mismatch'],['OperationId','c'.repeat(32),'premium_session_mismatch']])test('replay binding '+field,async()=>{
 const s=await setup(),issued=await s.issue();assert.equal((await s.verify({...issued.admission,[field]:value})).error,error);
});
test('expired proof cannot be renewed by replay; fresh reconnect nonce revalidates',async()=>{
 const s=await setup(),issued=await s.issue();s.expire();assert.equal((await s.verify(issued.admission)).error,'premium_expired');assert.equal((await s.issue()).error,'premium_expired');s.binding.AdmissionId=op();s.binding.OperationId=op();const fresh=await s.issue();assert.equal((await s.verify(fresh.admission)).ok,true);
});
test('duplicate admission is idempotent, new session requires a fresh record, revisions remain bounded',async()=>{
 const s=await setup(),first=await s.issue(),again=await s.issue();assert.deepEqual(first.admission,again.admission);assert.equal((await s.verify(first.admission)).ok,true);s.binding.SessionId=op();s.binding.AdmissionId=op();const next=await s.issue();assert.notEqual(first.admission.ProofId,next.admission.ProofId);assert.equal((await s.verify(next.admission)).ok,true);
});
test('impossible revision, account deletion and lost backend entitlement fail closed',async()=>{
 const s=await setup(),issued=await s.issue(),p=s.store.records.get('player').value;p.PremiumAdmissions[issued.admission.ProofId].AccountRevision=9999;assert.equal((await s.verify(issued.admission)).error,'premium_revision_invalid');p.PremiumAdmissions[issued.admission.ProofId].AccountRevision=1;p.Engineer=null;assert.equal((await s.verify(issued.admission)).error,'premium_unowned');s.accounts.records.get('player').value.Deletion={Id:'pending'};assert.equal((await s.verify(issued.admission)).error,'account_unavailable');
});
test('Host uses the same issuer/verifier flow and offline claims cannot substitute for protected data',async()=>{
 const s=await setup();s.binding.HostPlayerId='player';const issued=await s.issue();assert.equal((await s.backend.execute(ctx,'VerifyPremiumAdmission',issued.admission)).ok,true);const original=s.store.read;s.store.read=async()=>{throw Error('unavailable');};await assert.rejects(s.issue(),/unavailable/);s.store.read=original;
});
test('more than16 active admission tickets cannot grow protected state without bound',async()=>{
 const s=await setup();for(let i=0;i<16;i++){s.binding.AdmissionId=op();s.binding.OperationId=op();assert.equal((await s.issue()).ok,true);}s.binding.AdmissionId=op();assert.equal((await s.issue()).error,'admission_rate_limited');s.expire();assert.equal((await s.issue()).ok,true);assert.equal(Object.keys(s.store.records.get('player').value.PremiumAdmissions).length,1);
});
