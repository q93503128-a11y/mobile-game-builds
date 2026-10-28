'use strict';
const {requireThat,Rejected}=require('./errors.cjs');const L=require('./ledger.cjs');const {EconomyBackend,validate,transact,period}=require('./economy.cjs');
const clone=x=>JSON.parse(JSON.stringify(x)),guid=x=>typeof x==='string'&&/^[a-f0-9]{32}$/.test(x),iso=n=>new Date(n).toISOString();
const DAY=86400000,RETENTION=7*DAY;
function policy(config,channel,settlement=false){requireThat(config?.Schema===1&&config.Provider==='admob'&&Number.isSafeInteger(config.GemPerRewardedAd)&&config.GemPerRewardedAd>=1&&config.GemPerRewardedAd<=3&&Number.isSafeInteger(config.DailyRewardedGemLimit)&&config.DailyRewardedGemLimit>=config.GemPerRewardedAd&&config.DailyRewardedGemLimit<=10&&Number.isSafeInteger(config.CooldownSeconds)&&config.CooldownSeconds>=0&&config.CooldownSeconds<=3600,'ads_setup_required');if(!settlement)requireThat(config.Enabled===true&&config.SsvReady===true&&(channel!=='Production'||config.ProductionLiveEnabled===true),'ad_unavailable');requireThat(config.Channels?.includes(channel)&&/^[0-9]{1,24}$/.test(config.GemAdUnit||''),'ads_setup_required');return config;}
function ads(p){if(!p.Ads)p.Ads={Schema:1,Days:{},Operations:{},Audit:[],Failures:0};requireThat(p.Ads.Schema===1&&p.Ads.Days&&p.Ads.Operations&&Array.isArray(p.Ads.Audit),'invalid_ad_state');return p.Ads;}
function note(p,ctx,type,op,now,reason=''){const a=ads(p);a.Audit.push({At:iso(now),Type:type,OperationId:op||'',Reason:reason});a.Audit=a.Audit.slice(-64);p.Audit.push({eventId:L.hash([ctx.correlationId,p.Revision,type,op]),playerId:p.PlayerId,supportId:ctx.supportId,timestamp:iso(now),correlationId:ctx.correlationId,buildVersion:ctx.buildVersion,buildChannel:ctx.channel,eventType:type,operationId:op||'',result:reason?'rejected':'ok',reason});p.Audit=p.Audit.slice(-64);}
function collect(p,now){const a=ads(p);for(const [id,o]of Object.entries(a.Operations))if(now-Date.parse(o.ExpiresAt)>RETENTION&&(o.State==='Granted'||!o.Receipt))delete a.Operations[id];for(const day of Object.keys(a.Days))if(now-Date.parse(day+'T00:00:00Z')>14*DAY&&!Object.values(a.Operations).some(o=>o.Day===day&&o.State!=='Granted'))delete a.Days[day];requireThat(Object.keys(a.Operations).length<128,'ad_pending_capacity');}
function state(p,ctx,c,now,device){const a=ads(p),day=period(now).day,d=a.Days[day]||{Gem:0,LastRewardAt:0},pending=Object.values(a.Operations).find(o=>o.Day===day&&o.State!=='Granted');const remaining=Math.max(0,Math.floor((c.DailyRewardedGemLimit-d.Gem)/c.GemPerRewardedAd));const cooldown=Math.max(0,(d.LastRewardAt||0)+c.CooldownSeconds*1000-now);return {Schema:1,PlayerId:p.PlayerId,EnvironmentId:p.EnvironmentId,ServerTime:iso(now),NextReset:period(now).next,GemPerRewardedAd:c.GemPerRewardedAd,DailyRewardedGemLimit:c.DailyRewardedGemLimit,Remaining:remaining,CooldownUntil:iso(now+cooldown),Eligible:remaining>0&&cooldown===0&&(!pending||pending.Device===device||now-pending.LeasedAt>=600000),Operation:pending&&pending.Device===device?clone(pending):null,PendingOperations:Object.values(a.Operations).filter(o=>o.Device===device&&o.State!=='Granted').map(clone)};}
class RewardedBackend{
 constructor(store,accounts,config,environment,clock=()=>Date.now()){this.store=store;this.accounts=accounts;this.config=config;this.environment=environment;this.clock=clock;}
 async account(ctx){const a=(await this.accounts.read(ctx.playerId))?.value;requireThat(a?.Schema===1&&!a.Deletion,'account_unavailable');return a;}
 async ready(ctx){policy(this.config,ctx.channel);requireThat(ctx.environmentId===this.environment.environmentId&&ctx.channel===this.environment.channel,'wrong_environment');requireThat(ctx.linked===true,'ad_account_required');await this.account(ctx);}
 async execute(ctx,op,input={}){
  try{
   await this.ready(ctx);requireThat(['GetRewardedState','BeginGemReward','GetGemReward','RetryGemReward','RecordReviveAd','RecordGemAd'].includes(op),'operation_forbidden');requireThat(input&&typeof input==='object'&&JSON.stringify(input).length<=2048,'invalid_request');const now=this.clock();
   await new EconomyBackend(this.store,this.environment).maintain(ctx);
   for(let attempt=0;attempt<3;attempt++){
    const r=await this.store.read(ctx.playerId);requireThat(r,'economy_missing');const p=validate(L.upgrade(r.value),ctx);collect(p,now);const a=ads(p);let result;
    if(op==='GetRewardedState')return {ok:true,state:state(p,ctx,this.config,now,input.device)};
    requireThat(guid(input.operationId)&&guid(input.device),'invalid_request');
    if(op==='BeginGemReward'){
     const day=period(now).day,c=this.config,existing=Object.values(a.Operations).find(o=>o.Day===day&&o.State!=='Granted'),status=state(p,ctx,c,now,input.device);requireThat(status.Remaining>0,'ad_limit_reached');requireThat(Date.parse(status.CooldownUntil)<=now,'ad_cooldown');
     let o=existing;
     if(o){requireThat(o.Device===input.device||now-o.LeasedAt>=600000,'ad_pending');o.Device=input.device;o.LeasedAt=now;}
     else {const used=a.Days[day]?.Gem||0,id=L.hash(['lowfire-ad-slot-v1',p.PlayerId,p.EnvironmentId,day,used]).slice(0,32);o={OperationId:id,RequestId:input.operationId,Nonce:L.hash(['lowfire-ad-nonce',ctx.correlationId,p.PlayerId,id,now]),Day:day,Amount:c.GemPerRewardedAd,Limit:c.DailyRewardedGemLimit,Cooldown:c.CooldownSeconds,AdUnit:c.GemAdUnit,Device:input.device,LeasedAt:now,IssuedAt:iso(now),ExpiresAt:iso(Date.parse(day+'T00:00:00Z')+DAY+600000),State:'Reserved',Receipt:null};a.Operations[id]=o;}
     note(p,ctx,'ads.prepared',o.OperationId,now);result={ok:true,state:state(p,ctx,c,now,input.device),operation:clone(o)};
    }else if(op==='RecordGemAd'){
     requireThat(['earned','cancelled','unavailable','provider_error'].includes(input.result)&&guid(input.adOperationId),'invalid_request');const target=a.Operations[input.adOperationId];requireThat(target&&target.Device===input.device,'ad_operation_missing');
     if(a.Audit.some(x=>x.Type==='ads.gem_client_report'&&x.ReportId===input.operationId))return {ok:true,duplicate:true};const day=period(now).day;if(a.ReportDay!==day){a.ReportDay=day;a.ReportCount=0;}requireThat(a.ReportCount<64,'ad_report_limit_reached');a.ReportCount++;
     note(p,ctx,'ads.gem_client_report',target.OperationId,now);Object.assign(a.Audit[a.Audit.length-1],{ReportId:input.operationId,Placement:'rewarded_gem',Provider:'admob',Outcome:input.result,Trust:'client_reported_not_financial'});result={ok:true};
    }else if(op==='RecordReviveAd'){
     requireThat(['earned','cancelled','unavailable','provider_error','world_saved','world_save_failed'].includes(input.result)&&/^[a-f0-9]{64}$/.test(input.transactionHash||''),'invalid_request');const duplicate=a.Audit.some(x=>x.Type==='ads.revive_client_report'&&x.OperationId===input.operationId);if(duplicate)return {ok:true,duplicate:true};note(p,ctx,'ads.revive_client_report',input.operationId,now,input.result);a.Audit[a.Audit.length-1].Trust='client_reported_not_financial';a.Audit[a.Audit.length-1].TransactionHash=input.transactionHash;result={ok:true};
    }else{
     const o=a.Operations[input.operationId];requireThat(o,'ad_operation_missing');requireThat(o.Device===input.device,'ad_wrong_device');
     if(op==='RetryGemReward'&&o.Receipt)return await this.settle(ctx,o.OperationId);
     return {ok:true,operation:clone(o),state:state(p,ctx,this.config,now,input.device)};
    }
    p.Revision++;validate(p,ctx);if(await this.store.compareExchange(ctx.playerId,r.version,p))return result;
   }throw new Rejected('busy');
  }catch(e){if(e instanceof Rejected)return {ok:false,error:e.code};throw e;}
 }
 async receive(ctx,proof){
  policy(this.config,ctx.channel,true);requireThat(ctx.environmentId===this.environment.environmentId&&ctx.channel===this.environment.channel,'wrong_environment');requireThat(proof.Placement==='rewarded_gem'&&proof.EnvironmentId===ctx.environmentId&&proof.ProviderItem==='lowfire_gem','ad_placement_mismatch');
  const target={...ctx,playerId:proof.PlayerId,linked:true};await this.account(target);await new EconomyBackend(this.store,this.environment).maintain(target);const source='ad:admob:'+proof.ReceiptHash;
  for(let attempt=0;attempt<3;attempt++){
   const r=await this.store.read(target.playerId);requireThat(r,'economy_missing');const p=validate(L.upgrade(r.value),target),prior=await L.findSource(this.store,p,source);if(prior){requireThat(prior.Reason==='rewarded_ad'&&prior.OperationId===proof.OperationId&&prior.Placement===proof.Placement,'ad_receipt_conflict');await this.duplicate(target,proof.OperationId,prior.TransactionId);return {ok:true,duplicate:true,transactionId:prior.TransactionId};}
   const o=ads(p).Operations[proof.OperationId];requireThat(o&&o.Nonce===proof.Nonce,'ad_operation_missing');requireThat(proof.ProviderTimestamp>=Date.parse(o.IssuedAt)-60000&&proof.ProviderTimestamp<=Date.parse(o.ExpiresAt)&&proof.AdUnit===o.AdUnit&&proof.ProviderAmount===o.Amount,'ad_operation_expired');requireThat(o.State!=='Granted'&&(!o.Receipt||o.Receipt.ReceiptHash===proof.ReceiptHash),'ad_operation_reused');
   // Persist the verified receipt intent BEFORE the global receipt bind. Deletion/retry
   // can therefore find a bind even if the final wallet CAS never returned.
   o.Receipt={Provider:'admob',ReceiptHash:proof.ReceiptHash,Placement:proof.Placement,VerifiedAt:proof.ReceivedAt,ProviderTimestamp:proof.ProviderTimestamp,KeyId:proof.keyId};o.State='Verified';p.Revision++;validate(p,target);if(await this.store.compareExchange(target.playerId,r.version,p))return await this.settle(target,o.OperationId);
  }throw new Rejected('busy');
 }
 async settle(ctx,id){
  await this.account(ctx);await new EconomyBackend(this.store,this.environment).maintain(ctx);
  for(let attempt=0;attempt<3;attempt++){
   const r=await this.store.read(ctx.playerId);requireThat(r,'economy_missing');const p=validate(L.upgrade(r.value),ctx),a=ads(p),o=a.Operations[id];requireThat(o?.Receipt,'ad_pending');const receipt=o.Receipt,source='ad:admob:'+receipt.ReceiptHash,old=await L.findSource(this.store,p,source);if(old){requireThat(old.OperationId===id,'ad_receipt_conflict');return {ok:true,transactionId:old.TransactionId,duplicate:true,operation:{...o,State:'Granted',TransactionId:old.TransactionId}};}
   requireThat(o.State==='Verified'&&o.Amount>0&&o.Amount<=3,'ad_invalid_proof');requireThat(this.store.ledger&&this.store.withCreationGate,'archive_storage_required');
   const binding=await this.store.ledger.bindVerifiedFinancialReceipt({Verified:true,Kind:'ad',Provider:'admob',ReceiptHash:receipt.ReceiptHash,PlayerId:p.PlayerId,Placement:'rewarded_gem',VerifiedAt:receipt.VerifiedAt},work=>this.store.withCreationGate(async()=>{
    // A deletion may have started while this request waited for the gate. Return
    // known rejection without a write, so the gate is safely released.
    const account=(await this.accounts.read(ctx.playerId))?.value;if(!account||account.Deletion)return null;return await work();
   },'ad-receipt:'+receipt.ReceiptHash));requireThat(binding,'account_unavailable');
   const d=a.Days[o.Day]||(a.Days[o.Day]={Gem:0,LastRewardAt:0});requireThat(d.Gem+o.Amount<=o.Limit,'ad_limit_reached');const tx=await transact(p,ctx,o.Amount,'rewarded_ad',source,id,this.clock(),{Provider:'admob',Placement:'rewarded_gem',ReceiptHash:receipt.ReceiptHash,RewardDay:o.Day,ProviderTimestamp:receipt.ProviderTimestamp},this.store);d.Gem+=o.Amount;d.LastRewardAt=this.clock();o.State='Granted';o.TransactionId=tx.TransactionId;note(p,ctx,'ads.reward_granted',id,this.clock());p.Revision++;validate(p,ctx);if(await this.store.compareExchange(ctx.playerId,r.version,p))return {ok:true,transactionId:tx.TransactionId,operation:clone(o)};
  }throw new Rejected('busy');
 }
 async duplicate(ctx,id,transactionId){
  for(let n=0;n<3;n++){const r=await this.store.read(ctx.playerId);if(!r)return;const p=validate(clone(r.value),ctx),a=ads(p),day=period(this.clock()).day;if(a.DuplicateDay!==day){a.DuplicateDay=day;a.DuplicateCount=0;}if(a.DuplicateCount>=32)return;a.DuplicateCount++;note(p,ctx,'ads.duplicate',id,this.clock());Object.assign(a.Audit[a.Audit.length-1],{Placement:'rewarded_gem',TransactionId:transactionId});p.Revision++;if(await this.store.compareExchange(ctx.playerId,r.version,p))return;}
 }
 async rejected(ctx,reason){
  // Caller-attributed, bounded suspicious evidence; no ban and no cross-player writes.
  for(let n=0;n<3;n++){const r=await this.store.read(ctx.playerId);if(!r)return;const p=validate(clone(r.value),ctx),a=ads(p),day=period(this.clock()).day;if(a.FailureDay!==day){a.FailureDay=day;a.Failures=0;}if(a.Failures>=32)return;a.Failures++;p.Security.State='Suspicious';p.Security.Reasons=[...new Set([...p.Security.Reasons,reason])].slice(-16);note(p,ctx,'security.suspicious','',this.clock(),reason);p.Revision++;validate(p,ctx);if(await this.store.compareExchange(ctx.playerId,r.version,p))return;}
 }
}
module.exports={RewardedBackend,policy,ads,state,collect};
