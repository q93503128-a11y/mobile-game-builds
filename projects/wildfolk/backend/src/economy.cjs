'use strict';
const catalog=require('./economy-catalog.json');
const {requireThat,Rejected}=require('./errors.cjs');
const {sha256}=require('./cryptography.cjs');
const copy=x=>JSON.parse(JSON.stringify(x));
const integer=(n,max=Number.MAX_SAFE_INTEGER)=>Number.isSafeInteger(n)&&n>=0&&n<=max;
const safe=x=>typeof x==='string'&&/^[A-Za-z0-9_-]{1,96}$/.test(x);
const operationId=x=>typeof x==='string'&&/^[a-f0-9]{32}$/.test(x);
const digest=x=>sha256(JSON.stringify(x));
const stable=x=>Array.isArray(x)?x.map(stable):x&&typeof x==='object'?Object.fromEntries(Object.keys(x).sort().filter(k=>k!=='integrityToken').map(k=>[k,stable(x[k])])):x;
const day=now=>new Date(now).toISOString().slice(0,10);
const operations=new Set(['GetEconomy','SubmitProgress','ClaimAchievement','ClaimAll','ClaimAttendance','UnlockEngineer','BeginRevive','FinishRevive','TransactionHistory']);
function period(now){const d=day(now);return {day:d,month:d.slice(0,7),next:new Date(Date.parse(d+'T00:00:00Z')+86400000).toISOString()};}
function fresh(ctx,now){return {Schema:1,PlayerId:ctx.playerId,EnvironmentId:ctx.environmentId,CreatedAt:new Date(now).toISOString(),Revision:0,Balance:0,Ledger:[],Metrics:{},Claimed:[],Devices:{},Critical:[],Newcomer:{Count:0,LastDay:''},Monthly:{Period:'',Count:0,LastDay:''},Events:{},Engineer:null,Revives:{},Audit:[],Operations:[],Security:{State:'Normal',Reasons:[]},Daily:{Day:'',Requests:0,Metrics:{}}};}
function validate(p,ctx){
 requireThat(p?.Schema===1&&p.PlayerId===ctx.playerId&&p.EnvironmentId===ctx.environmentId&&integer(p.Revision)&&integer(p.Balance,catalog.MaximumBalance)&&Array.isArray(p.Ledger),'invalid_economy_schema');
 let balance=0;for(let i=0;i<p.Ledger.length;i++){const t=p.Ledger[i];requireThat(t.Sequence===i+1&&t.PlayerId===p.PlayerId&&Number.isSafeInteger(t.Delta)&&t.BalanceBefore===balance&&t.BalanceAfter===balance+t.Delta&&integer(t.BalanceAfter,catalog.MaximumBalance),'ledger_inconsistent');balance=t.BalanceAfter;}
 requireThat(balance===p.Balance,'ledger_inconsistent');requireThat(p.Ledger.length<=catalog.MaximumLedger&&JSON.stringify(p).length<=catalog.MaximumStateBytes,'economy_capacity');return p;
}
function audit(p,ctx,type,op,now,result='ok',reason=''){
 p.Audit.push({eventId:digest([ctx.correlationId,op,type,p.Revision]),playerId:ctx.playerId,supportId:ctx.supportId,timestamp:new Date(now).toISOString(),correlationId:ctx.correlationId,buildVersion:ctx.buildVersion,buildChannel:ctx.channel,eventType:type,operationId:op,result,reason});p.Audit=p.Audit.slice(-64);
}
function flag(p,ctx,reason,op,now){p.Security.State='Suspicious';if(!p.Security.Reasons.includes(reason))p.Security.Reasons.push(reason);p.Security.Reasons=p.Security.Reasons.slice(-16);audit(p,ctx,'security.suspicious',op,now,'flagged',reason);}
function transact(p,ctx,delta,reason,source,op,now,metadata={}){
 requireThat(Number.isSafeInteger(delta)&&delta!==0&&Math.abs(delta)<=100000&&typeof source==='string'&&source.length<=160,'invalid_transaction');
 const old=p.Ledger.find(t=>t.SourceId===source);if(old){requireThat(old.Delta===delta&&old.Reason===reason,'source_conflict');return old;}
 const next=p.Balance+delta;requireThat(integer(next,catalog.MaximumBalance),'insufficient_gem');requireThat(p.Ledger.length<catalog.MaximumLedger,'economy_capacity');
 const t={TransactionId:'G-'+(p.Ledger.length+1)+'-'+digest([ctx.playerId,source]).slice(0,12),Sequence:p.Ledger.length+1,PlayerId:ctx.playerId,Timestamp:new Date(now).toISOString(),Delta:delta,BalanceBefore:p.Balance,BalanceAfter:next,Reason:reason,SourceId:source,OperationId:op,BuildChannel:ctx.channel,MetadataVersion:1,...metadata};
 p.Ledger.push(t);p.Balance=next;audit(p,ctx,delta>0?'economy.gem_granted':'economy.gem_spent',op,now,'ok',reason);return t;
}
function metricValue(p,definition){const n=p.Metrics[definition.Metric]||0;return definition.Metric==='biomes'?n.toString(2).replace(/0/g,'').length:n;}
function completion(p){let points=0,completed=0;for(const f of catalog.Families)for(let i=0;i<f.Tiers.length;i++)if(metricValue(p,f)>=f.Tiers[i].Threshold){points+=f.Tiers[i].AchievementPoints;completed++;}return {points,completed,total:catalog.Families.reduce((a,f)=>a+f.Tiers.length,0)};}
function eventDefinition(e){
 requireThat(e&&safe(e.EventId)&&Number.isFinite(Date.parse(e.StartAt))&&Number.isFinite(Date.parse(e.EndAt))&&Date.parse(e.EndAt)>Date.parse(e.StartAt)&&integer(e.ClaimCount,28)&&e.ClaimCount>0&&e.Rewards?.length===e.ClaimCount&&e.Rewards.every(n=>integer(n,100))&&Array.isArray(e.Eligibility?.Channels)&&e.Eligibility.Channels.every(c=>['Development','Alpha','Production'].includes(c))&&integer(e.Eligibility.MinimumAccountAgeDays,3650)&&typeof e.NameKo==='string'&&e.NameKo.length<=64&&typeof e.NameEn==='string'&&e.NameEn.length<=64,'invalid_event_config');return e;
}
function boards(p,ctx,now,config){
 const t=period(now);const month=p.Monthly.Period===t.month?p.Monthly:{Count:0,LastDay:''};
 const result=[{Id:'newcomer',Period:'account',Count:p.Newcomer.Count,Rewards:catalog.Newcomer,Available:p.Newcomer.Count<7&&p.Newcomer.LastDay!==t.day,State:p.Newcomer.Count===7?'completed':'active'},
 {Id:'monthly',Period:t.month,Count:month.Count,Rewards:catalog.Monthly,Available:month.Count<28&&month.LastDay!==t.day,State:month.Count===28?'completed':'active'}];
 requireThat(Array.isArray(config.Events)&&config.Events.length<=8,'invalid_event_config');const ids=new Set();
 for(const raw of config.Events){const e=eventDefinition(raw);requireThat(!ids.has(e.EventId),'invalid_event_config');ids.add(e.EventId);const s=p.Events[e.EventId]||{Count:0,LastDay:''};
  const eligible=e.Eligibility.Channels.includes(ctx.channel)&&(!e.Eligibility.RequireLinked||ctx.linked)&&now-Date.parse(p.CreatedAt)>=e.Eligibility.MinimumAccountAgeDays*86400000;
  const active=now>=Date.parse(e.StartAt)&&now<Date.parse(e.EndAt);result.push({Id:e.EventId,Period:e.EventId,NameKo:e.NameKo,NameEn:e.NameEn,Count:s.Count,Rewards:e.Rewards,Available:eligible&&active&&s.Count<e.ClaimCount&&s.LastDay!==t.day,State:!eligible?'ineligible':now<Date.parse(e.StartAt)?'upcoming':!active?'expired':s.Count>=e.ClaimCount?'completed':'active',DefinitionHash:digest(e)});
 }return result;
}
function snapshot(p,ctx,now,config){return {Schema:1,CatalogRevision:catalog.Revision,Revision:p.Revision,PlayerId:p.PlayerId,EnvironmentId:p.EnvironmentId,ServerTime:new Date(now).toISOString(),NextReset:period(now).next,Balance:p.Balance,EngineerOwned:!!p.Engineer,EngineerCost:catalog.EngineerCost,ReviveCost:catalog.ReviveCost,Metrics:Object.entries(p.Metrics).map(([Id,Value])=>({Id,Value})),Claimed:[...p.Claimed],Boards:boards(p,ctx,now,config),PendingRevives:Object.values(p.Revives).filter(r=>r.State==='Reserved'),...completion(p)};}
class EconomyBackend{
 constructor(store,policy,config={Schema:1,Enabled:true,Events:[]},clock=()=>Date.now()){this.store=store;this.policy=policy;this.config=config;this.clock=clock;}
 context(ctx){requireThat(safe(ctx.playerId)&&safe(ctx.projectId)&&safe(ctx.correlationId)&&typeof ctx.buildVersion==='string'&&/^[0-9A-Za-z._-]{1,32}$/.test(ctx.buildVersion),'authentication_required');requireThat(ctx.environmentId===this.policy.environmentId&&ctx.channel===this.policy.channel,'wrong_environment');requireThat(!ctx.fixture||ctx.channel==='Development','fixture_forbidden');requireThat(ctx.channel==='Development'||ctx.linked===true,'account_link_required');requireThat(this.config.Schema===1&&this.config.Enabled===true,'economy_unavailable');}
 async execute(ctx,operation,input={}){
  try{
   this.context(ctx);requireThat(operations.has(operation),'operation_forbidden');requireThat(input&&typeof input==='object'&&!Array.isArray(input)&&JSON.stringify(input).length<=16000,'invalid_request');const now=this.clock();requireThat(Number.isFinite(now),'server_clock_unavailable');
   if(!await this.store.read(ctx.playerId))await this.store.create(ctx.playerId,()=>fresh(ctx,now));
   if(operation==='GetEconomy'||operation==='TransactionHistory'){
    const p=validate((await this.store.read(ctx.playerId)).value,ctx);let extra={};
    if(operation==='TransactionHistory'){const offset=input.offset??0,limit=input.limit??20;requireThat(integer(offset,catalog.MaximumLedger)&&integer(limit,50)&&limit>0,'invalid_page');extra={transactions:p.Ledger.slice().reverse().slice(offset,offset+limit),nextOffset:Math.min(p.Ledger.length,offset+limit),transactionCount:p.Ledger.length};}
    return {ok:true,snapshot:snapshot(p,ctx,now,this.config),...extra};
   }
   requireThat(operationId(input.operationId),'invalid_operation');
   for(let attempt=0;attempt<3;attempt++){
    const record=await this.store.read(ctx.playerId);let p=validate(copy(record.value),ctx);const hash=digest(stable([operation,input]));const previous=p.Operations.find(o=>o.Id===input.operationId);let result;
    if(previous&&previous.Hash===hash)return {...copy(previous.Result),snapshot:snapshot(p,ctx,now,this.config)};
    if(previous){flag(p,ctx,'idempotency_payload_mismatch',input.operationId,now);result={ok:false,error:'operation_conflict'};}
    else{
     if(p.Daily.Day!==day(now))p.Daily={Day:day(now),Requests:0,Metrics:{}};
     requireThat(p.Daily.Requests<500,'rate_limited');const original=copy(p);
     try{result={ok:true,...this.apply(p,ctx,operation,input,now)};}
     catch(e){if(!(e instanceof Rejected))throw e;p=original;result={ok:false,error:e.code};if(['impossible_progress','invalid_progress','source_conflict','operation_forbidden'].includes(e.code))flag(p,ctx,e.code,input.operationId,now);}
     p.Daily.Requests++;audit(p,ctx,({SubmitProgress:'achievement.progressed',ClaimAchievement:'achievement.claimed',ClaimAll:'achievement.claimed',ClaimAttendance:'attendance.claimed',UnlockEngineer:'entitlement.granted',BeginRevive:'economy.revive_reserved',FinishRevive:'economy.revive_resolved'})[operation],input.operationId,now,result.ok?'ok':'rejected',result.error||'');
     p.Operations.push({Id:input.operationId,Hash:hash,Result:result});p.Operations=p.Operations.slice(-256);
    }
    p.Revision++;validate(p,ctx);if(await this.store.compareExchange(ctx.playerId,record.version,p))return {...result,snapshot:snapshot(p,ctx,now,this.config)};
   }throw new Rejected('busy');
  }catch(e){if(e instanceof Rejected)return {ok:false,error:e.code};throw e;}
 }
 apply(p,ctx,operation,input,now){
  const op=input.operationId;
  if(operation==='SubmitProgress'){
   requireThat(ctx.channel==='Development'||ctx.trustedBuild===true,'integrity_required');
   requireThat(operationId(input.device)&&integer(input.first)&&input.first>0&&integer(input.last)&&input.last>=input.first&&input.last-input.first<10000&&Array.isArray(input.metrics)&&input.metrics.length<=catalog.Metrics.length,'invalid_progress');
   const previous=p.Devices[input.device]||0;if(input.last<=previous)return {acknowledged:previous};requireThat(input.first===previous+1,'progress_sequence_conflict');requireThat(Object.keys(p.Devices).length<64||previous>0,'device_capacity');
   const names=new Set();for(let item of input.metrics){const m=catalog.Metrics.find(m=>m.Id===item.Id);requireThat(m&&!names.has(item.Id)&&integer(item.Value,m.Mode==='mask'?15:m.MaxBatch)&&item.Value>0,'impossible_progress');names.add(item.Id);
    if(m.Critical){requireThat(Array.isArray(item.Sources)&&item.Sources.length===item.Value&&item.Sources.every(s=>typeof s==='string'&&/^[a-f0-9]{64}$/.test(s)),'invalid_progress');const unique=new Set(item.Sources);requireThat(unique.size===item.Sources.length,'invalid_progress');let count=0;for(const source of unique){const id=m.Id+':'+source;if(!p.Critical.includes(id)){p.Critical.push(id);count++;}}item={...item,Value:count};}
    const already=p.Metrics[m.Id]||0;
    if(m.Mode==='mask')p.Metrics[m.Id]=already|item.Value;
    else {requireThat((p.Daily.Metrics[m.Id]||0)+item.Value<=m.MaxDaily,'impossible_progress');p.Daily.Metrics[m.Id]=(p.Daily.Metrics[m.Id]||0)+item.Value;p.Metrics[m.Id]=Math.min(m.Maximum,already+item.Value);}
   }requireThat(p.Critical.length<=4096,'economy_capacity');p.Devices[input.device]=input.last;return {acknowledged:input.last};
  }
  if(operation==='ClaimAll'||operation==='ClaimAchievement'){
   let count=0,gems=0;requireThat(operation==='ClaimAll'||catalog.Families.some(f=>f.Id===input.family),'invalid_achievement');
   for(const f of catalog.Families){if(operation==='ClaimAchievement'&&f.Id!==input.family)continue;for(let i=0;i<f.Tiers.length;i++){
    const key=f.Id+':'+i,t=f.Tiers[i];if(metricValue(p,f)<t.Threshold||p.Claimed.includes(key))continue;
    transact(p,ctx,t.GemReward,'achievement_reward','achievement:'+key,op,now);p.Claimed.push(key);count++;gems+=t.GemReward;
   }}return {claimedCount:count,granted:gems};
  }
  if(operation==='ClaimAttendance'){
   const board=boards(p,ctx,now,this.config).find(b=>b.Id===input.board);requireThat(board,'invalid_board');const t=period(now);
   if(board.Id==='monthly'&&p.Monthly.Period!==t.month)p.Monthly={Period:t.month,Count:0,LastDay:''};
   let state=board.Id==='newcomer'?p.Newcomer:board.Id==='monthly'?p.Monthly:p.Events[board.Id];
   if(!state){requireThat(Object.keys(p.Events).length<128,'economy_capacity');state=p.Events[board.Id]={Count:0,LastDay:'',DefinitionHash:board.DefinitionHash};}
   if(board.DefinitionHash)requireThat(state.DefinitionHash===board.DefinitionHash,'event_definition_changed');
   // Same-day/done retries are no-ops even with a new transport operation ID.
   if(state.LastDay===t.day||state.Count>=board.Rewards.length)return {alreadyClaimed:true};requireThat(board.Available,'attendance_unavailable');
   const slot=state.Count+1,reward=board.Rewards[slot-1],reason=board.Id==='newcomer'?'attendance_newcomer':board.Id==='monthly'?'attendance_monthly':'attendance_event';let transaction;
   if(reward>0)transaction=transact(p,ctx,reward,reason,'attendance:'+board.Id+':'+board.Period+':'+slot,op,now);
   state.Count=slot;state.LastDay=t.day;return {claimedDay:slot,granted:reward,transactionId:transaction?.TransactionId};
  }
  if(operation==='UnlockEngineer'){
   if(p.Engineer)return {alreadyOwned:true,transactionId:p.Engineer.TransactionId};requireThat(input.expectedCost===catalog.EngineerCost,'price_changed');
   const tx=transact(p,ctx,-catalog.EngineerCost,'class_unlock','class:engineer',op,now);p.Engineer={Id:'class_engineer',TransactionId:tx.TransactionId,GrantedAt:new Date(now).toISOString()};return {transactionId:tx.TransactionId};
  }
  if(operation==='BeginRevive'){
   requireThat(operationId(input.source)&&input.expectedCost===catalog.ReviveCost,'invalid_revive');const previous=p.Revives[input.source];if(previous)return {revive:previous};
   requireThat(!Object.values(p.Revives).some(r=>r.State==='Reserved'),'revive_pending');const tx=transact(p,ctx,-catalog.ReviveCost,'revive','revive:'+input.source,op,now);
   const revive={Source:input.source,State:'Reserved',Cost:catalog.ReviveCost,TransactionId:tx.TransactionId};p.Revives[input.source]=revive;return {revive};
  }
  if(operation==='FinishRevive'){
   requireThat(operationId(input.source)&&['committed','failed'].includes(input.outcome),'invalid_revive');const r=p.Revives[input.source];requireThat(r,'revive_missing');
   if(r.State==='Reserved'){if(input.outcome==='failed')transact(p,ctx,r.Cost,'revive_refund','revive-refund:'+input.source,op,now);r.State=input.outcome==='committed'?'Committed':'Refunded';}return {revive:r};
  }
  throw new Rejected('operation_forbidden');
 }
 async adminAdjust(ctx,request,{operator,caseReference,authorized=false,apply=false}={}){
  this.context(ctx);requireThat(authorized&&safe(operator)&&safe(caseReference)&&operationId(request.operationId)&&typeof request.reason==='string'&&/^[A-Za-z0-9 _.-]{5,96}$/.test(request.reason)&&Number.isSafeInteger(request.delta)&&request.delta!==0&&Math.abs(request.delta)<=100000,'admin_authorization_required');
  const now=this.clock();for(let attempt=0;attempt<3;attempt++){const record=await this.store.read(ctx.playerId);requireThat(record,'economy_missing');const p=validate(copy(record.value),ctx);const source='admin:'+request.operationId;const existing=p.Ledger.find(t=>t.SourceId===source);
   if(existing){requireThat(existing.Delta===request.delta&&existing.Operator===operator&&existing.CaseReference===caseReference&&existing.AdjustmentReason===request.reason,'source_conflict');return {duplicate:true,transaction:existing};}
   requireThat(integer(p.Balance+request.delta,catalog.MaximumBalance),'insufficient_gem');if(!apply)return {dryRun:true,before:p.Balance,after:p.Balance+request.delta,delta:request.delta};
   const tx=transact(p,ctx,request.delta,'admin_adjustment',source,request.operationId,now,{Operator:operator,CaseReference:caseReference,AdjustmentReason:request.reason});p.Revision++;validate(p,ctx);if(await this.store.compareExchange(ctx.playerId,record.version,p))return {transaction:tx};
  }throw new Rejected('busy');
 }
}
module.exports={EconomyBackend,catalog,fresh,validate,transact,completion,boards,period,snapshot,operations,eventDefinition};
