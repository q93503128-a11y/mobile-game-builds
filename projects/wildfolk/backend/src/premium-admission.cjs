'use strict';
const {requireThat,Rejected}=require('./errors.cjs');
const {sha256}=require('./cryptography.cjs');
const {engineerOwnership}=require('./economy-ownership.cjs');
const {validate}=require('./economy.cjs');
const clone=x=>JSON.parse(JSON.stringify(x));
const guid=x=>typeof x==='string'&&/^[a-f0-9]{32}$/.test(x);
const player=x=>typeof x==='string'&&/^[A-Za-z0-9_-]{1,96}$/.test(x);
const proof=x=>typeof x==='string'&&/^[a-f0-9]{64}$/.test(x);
const TTL=90000;
class PremiumAdmissionBackend{
 constructor(store,accounts,policy,clock=()=>Date.now()){this.store=store;this.accounts=accounts;this.policy=policy;this.clock=clock;}
 async note(ctx,reason,snapshot=null){
  for(let retry=0;retry<3;retry++){
   const record=await this.store.read(ctx.playerId);if(!record)return;
   const p=clone(record.value);if(p.PlayerId!==ctx.playerId||p.EnvironmentId!==ctx.environmentId)return;validate(p,ctx);
   const time=new Date(this.clock()).toISOString();
   const day=time.slice(0,10);if(p.PremiumAudit?.Day!==day)p.PremiumAudit={Day:day,Count:0};if(p.PremiumAudit.Count>=64)return;p.PremiumAudit.Count++;
   p.LastPremiumAdmission={At:time,Result:snapshot?'verified':'rejected',Reason:reason,PlayerId:snapshot?.PlayerId||ctx.playerId,HostPlayerId:snapshot?.HostPlayerId||'',SessionId:snapshot?.SessionId||'',AdmissionId:snapshot?.AdmissionId||'',EconomyRevision:snapshot?.EconomyRevision||p.Revision};
   if(!snapshot){p.Security.State='Suspicious';p.Security.Reasons=[...new Set([...p.Security.Reasons,reason])].slice(-16);}
   p.Audit.push({eventId:sha256(JSON.stringify([ctx.correlationId,p.Revision,reason])),playerId:ctx.playerId,supportId:ctx.supportId,timestamp:time,correlationId:ctx.correlationId,buildVersion:ctx.buildVersion,buildChannel:ctx.channel,eventType:snapshot?'premium.admission_verified':'security.suspicious',operationId:snapshot?.OperationId||ctx.correlationId,result:snapshot?'ok':'flagged',reason});p.Audit=p.Audit.slice(-64);p.Revision++;
   validate(p,ctx);if(await this.store.compareExchange(ctx.playerId,record.version,p))return;
  }throw new Rejected('busy');
 }
 binding(input){requireThat(input&&player(input.HostPlayerId)&&guid(input.SessionId)&&guid(input.AdmissionId)&&guid(input.Profile)&&input.SelectedClassId==='engineer','malformed_admission');}
 async execute(ctx,operation,input={}){
  try{
   requireThat(ctx.environmentId===this.policy.environmentId&&ctx.channel===this.policy.channel&&player(ctx.playerId),'wrong_environment');
   requireThat(ctx.channel==='Development'||ctx.linked===true,'account_link_required');
   requireThat(operation==='IssuePremiumAdmission'||operation==='VerifyPremiumAdmission','operation_forbidden');
   requireThat(JSON.stringify(input).length<=2048,'malformed_admission');this.binding(input);
   if(operation==='IssuePremiumAdmission')return await this.issue(ctx,input);
   return await this.verify(ctx,input);
  }catch(e){if(!(e instanceof Rejected))throw e;await this.note(ctx,e.code);return {ok:false,error:e.code};}
 }
 async issue(ctx,input){
  requireThat(guid(input.OperationId),'malformed_admission');
  const account=(await this.accounts.read(ctx.playerId))?.value;
  requireThat(account?.Schema===1&&!account.Deletion,'account_unavailable');
  for(let retry=0;retry<3;retry++){
   const record=await this.store.read(ctx.playerId);requireThat(record,'premium_unowned');const p=clone(record.value);validate(p,ctx);
   requireThat(engineerOwnership(p,ctx),'premium_unowned');
   const now=this.clock(),id=sha256(JSON.stringify([ctx.playerId,input.HostPlayerId,input.SessionId,input.AdmissionId,input.Profile,input.SelectedClassId,input.OperationId]));
   p.PremiumAdmissions=p.PremiumAdmissions||{};
   const old=p.PremiumAdmissions[id];if(old){requireThat(Date.parse(old.ExpiresAt)>now,'premium_expired');return {ok:true,admission:old};}
   p.PremiumAdmissions=Object.fromEntries(Object.entries(p.PremiumAdmissions).filter(([,s])=>Date.parse(s.ExpiresAt)>now));
   requireThat(Object.keys(p.PremiumAdmissions).length<16,'admission_rate_limited');
   const day=new Date(now).toISOString().slice(0,10);if(p.Daily.Day!==day)p.Daily={Day:day,Requests:0,Metrics:{}};requireThat(p.Daily.Requests<500,'rate_limited');p.Daily.Requests++;
   const snapshot={ProofId:id,PlayerId:ctx.playerId,HostPlayerId:input.HostPlayerId,SessionId:input.SessionId,AdmissionId:input.AdmissionId,Profile:input.Profile,SelectedClassId:'engineer',AllowedClassIds:['engineer'],AccountRevision:account.Revision,EconomyRevision:p.Revision+1,IssuedAt:new Date(now).toISOString(),ExpiresAt:new Date(now+TTL).toISOString(),OperationId:input.OperationId};
   p.PremiumAdmissions[id]=snapshot;p.Revision++;validate(p,ctx);
   if(await this.store.compareExchange(ctx.playerId,record.version,p))return {ok:true,admission:snapshot};
  }throw new Rejected('busy');
 }
 async verify(ctx,input){
  requireThat(player(input.PlayerId)&&proof(input.ProofId),'malformed_admission');
  requireThat(input.HostPlayerId===ctx.playerId,'premium_host_mismatch');
  const account=(await this.accounts.read(input.PlayerId))?.value;
  requireThat(account?.Schema===1&&!account.Deletion,'account_unavailable');
  const record=await this.store.read(input.PlayerId),p=record?.value;
  requireThat(p,'premium_unowned');validate(p,{...ctx,playerId:input.PlayerId});
  const snapshot=p.PremiumAdmissions?.[input.ProofId];requireThat(snapshot,'premium_player_mismatch');
  requireThat(snapshot.PlayerId===input.PlayerId&&snapshot.HostPlayerId===ctx.playerId,'premium_player_mismatch');
  requireThat(snapshot.SessionId===input.SessionId&&snapshot.AdmissionId===input.AdmissionId&&snapshot.Profile===input.Profile&&snapshot.SelectedClassId===input.SelectedClassId&&snapshot.OperationId===input.OperationId,'premium_session_mismatch');
  const now=this.clock();requireThat(Date.parse(snapshot.IssuedAt)<=now&&Date.parse(snapshot.ExpiresAt)>now&&Date.parse(snapshot.ExpiresAt)-Date.parse(snapshot.IssuedAt)===TTL,'premium_expired');
  requireThat(Number.isSafeInteger(snapshot.AccountRevision)&&snapshot.AccountRevision>=0&&snapshot.AccountRevision<=account.Revision&&Number.isSafeInteger(snapshot.EconomyRevision)&&snapshot.EconomyRevision>=0&&snapshot.EconomyRevision<=p.Revision,'premium_revision_invalid');
  requireThat(engineerOwnership(p,{...ctx,playerId:input.PlayerId}),'premium_unowned');
  await this.note({...ctx,playerId:input.PlayerId,supportId:account.SupportId},'verified',snapshot);
  return {ok:true,admission:snapshot};
 }
}
module.exports={PremiumAdmissionBackend,TTL};
