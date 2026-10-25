"use strict";
const crypto=require('crypto');
const catalog=require('./class-catalog.json');
const clone=x=>JSON.parse(JSON.stringify(x));
const categories=['Bug Report','Game Feedback','Account / Cloud','Purchase','Player Report','Other'];
const operations=new Set(['EnsureAccountProfile','GetAccountProfile','EnrollAlphaParticipation','GetEntitlements','SubmitFeedback','RecordOperationalEvent','RequestAccountDeletionPreparation','CommitClassProgress','ImportProfile2','GrantEntitlement','RealMoneyPurchase']);
const safeId=x=>typeof x==='string'&&/^[a-zA-Z0-9_-]{1,96}$/.test(x);
class Rejected extends Error {constructor(code){super(code);this.code=code;}}
const requireThat=(ok,code)=>{if(!ok)throw new Rejected(code);};
const stable=x=>Array.isArray(x)?x.map(stable):x&&typeof x==='object'?Object.fromEntries(Object.keys(x).sort().filter(k=>k!=='integrityToken').map(k=>[k,stable(x[k])])):x;
const operationDigest=(operation,input)=>digest(stable([operation,input]));
const digest=x=>crypto.createHash('sha256').update(typeof x==='string'?x:JSON.stringify(x)).digest('hex');
function supportId(project,player){return 'LF-'+digest(project+':'+player).slice(0,20).toUpperCase().match(/.{4}/g).join('-');}
function localProfile(raw){
 requireThat(raw&&raw.Version===2&&Array.isArray(raw.Classes)&&raw.Classes.length===catalog.Classes.length,'invalid_import');
 requireThat(Object.keys(raw).every(k=>['Version','Selected','Classes'].includes(k)),'invalid_import');
 const result={Version:2,Selected:raw.Selected,Classes:[]};const ids=new Set();
 for(const p of raw.Classes){
  const c=catalog.Classes.find(c=>c.Id===p?.Id);
  requireThat(c&&!ids.has(p.Id)&&typeof p.Owned==='boolean'&&Number.isSafeInteger(p.XP)&&p.XP>=0&&p.XP<=1000000000&&Array.isArray(p.Nodes),'invalid_import');ids.add(p.Id);
  const earned=catalog.Milestones.filter(n=>p.XP>=n).length;
  requireThat(p.Nodes.length<=earned&&new Set(p.Nodes).size===p.Nodes.length&&(p.Owned||p.XP===0&&p.Nodes.length===0),'invalid_import');
  for(const id of p.Nodes){const n=c.Nodes.find(n=>n.Id===id);requireThat(n&&(!n.Prerequisite||p.Nodes.includes(n.Prerequisite)),'invalid_import');}
  result.Classes.push({Id:p.Id,Owned:p.Owned,XP:p.XP,Nodes:[...p.Nodes]});
 }
 requireThat(result.Classes.find(c=>c.Id==='survivor')?.Owned&&result.Classes.find(c=>c.Id===raw.Selected)?.Owned,'invalid_import');
 return result;
}
function newClass(){return {Version:2,Selected:'survivor',Classes:catalog.Classes.map(c=>({Id:c.Id,Owned:c.Price===0&&c.Currency==='Gold',XP:0,Nodes:[]}))};}
function diagnostics(value){
 const allowed=['appVersion','channel','platform','deviceModel','os','locale','saveVersion','profileVersion','protocolVersion','scene','sessionId','lastErrorCode','cloudState'];
 const out={};for(const k of allowed){let v=value?.[k];if(typeof v!=='string'&&typeof v!=='number')continue;v=String(v).slice(0,128);
  // Never forward paths, credentials, email, bearer strings or opaque token-like data.
  if(/[\\/]|@|bearer|token|secret|credential|private.key|[A-Za-z0-9_-]{40,}/i.test(v))continue;out[k]=v;
 }return out;
}
function makeProfile(ctx,now,legacy){
 const imported=legacy?localProfile(legacy):newClass();
 return {Schema:1,PlayerId:ctx.playerId,SupportId:supportId(ctx.projectId,ctx.playerId),CreatedAt:now,ProviderLinked:ctx.linked===true,Revision:0,ClassRevision:0,ClassProfile:imported,
  Import:{Closed:true,Source:legacy?'prelaunch_local_unverified':'new',Receipt:digest(ctx.playerId+':profile2'),At:now},
  Alpha:null,Entitlements:[],Security:{State:'Normal',Reasons:[]},Tickets:[],Audit:[],Operations:[],Daily:{Day:'',Count:0},Deletion:null};
}
function validateProfile(p){
 requireThat(p?.Schema===1&&safeId(p.PlayerId)&&typeof p.SupportId==='string'&&/^LF-(?:[A-F0-9]{4}-){4}[A-F0-9]{4}$/.test(p.SupportId)&&Number.isSafeInteger(p.Revision)&&p.Revision>=0&&Number.isSafeInteger(p.ClassRevision)&&p.ClassRevision>=0,'invalid_cloud_schema');
 localProfile(p.ClassProfile);requireThat(p.Import?.Closed===true&&p.Entitlements.length<=256&&p.Tickets.length<=20&&p.Audit.length<=64&&p.Operations.length<=512,'invalid_cloud_schema');return p;
}
function view(p){return {Schema:p.Schema,PlayerId:p.PlayerId,SupportId:p.SupportId,CreatedAt:p.CreatedAt,ProviderLinked:p.ProviderLinked,Revision:p.Revision,ClassRevision:p.ClassRevision,ClassProfile:clone(p.ClassProfile),AlphaRecorded:!!p.Alpha,Entitlements:clone(p.Entitlements),DeletionPending:!!p.Deletion};}
function audit(p,ctx,type,operationId,result,reason,now){
 p.Audit.push({eventId:crypto.randomUUID(),playerId:ctx.playerId,supportId:p.SupportId,timestamp:now,correlationId:ctx.correlationId,buildVersion:/^[0-9]+(?:\.[0-9]+){0,3}$/.test(ctx.buildVersion)?ctx.buildVersion:'unlisted',buildChannel:ctx.channel,eventType:type,operationId,result,reason});p.Audit=p.Audit.slice(-64);
}
function validOperation(key,now){
 requireThat(typeof key==='string'&&/^\d{4}-\d{2}-\d{2}:[a-f0-9]{32}$/.test(key),'invalid_operation');
 const date=Date.parse(key.slice(0,10)+'T00:00:00Z'),today=Date.parse(now.slice(0,10)+'T00:00:00Z');
 requireThat(Number.isFinite(date)&&date<=today&&today-date<=86400000,'expired_operation');
}
function flag(p,reason){p.Security.State='Suspicious';if(!p.Security.Reasons.includes(reason))p.Security.Reasons.push(reason);p.Security.Reasons=p.Security.Reasons.slice(-16);}
class AccountBackend {
 constructor(store,policy,clock=()=>new Date().toISOString()){this.store=store;this.policy=policy;this.clock=clock;}
 context(ctx){
  requireThat(safeId(ctx.playerId)&&safeId(ctx.projectId)&&safeId(ctx.correlationId)&&typeof ctx.buildVersion==='string'&&/^[0-9A-Za-z._-]{1,32}$/.test(ctx.buildVersion),'authentication_required');
  requireThat(ctx.environmentId===this.policy.environmentId&&ctx.channel===this.policy.channel,'wrong_environment');
  requireThat(!ctx.fixture||ctx.channel==='Development','fixture_forbidden');
 }
 async execute(ctx,operation,input={}){
  this.context(ctx);requireThat(operations.has(operation),"operation_forbidden");const now=this.clock();
  try{
   if(operation==='EnsureAccountProfile'){
    validOperation(input.operationId,now);
    requireThat(!input.legacy||this.policy.allowLegacyImport&&ctx.channel!=='Production','import_closed');
    if(input.legacy)localProfile(input.legacy);
    const created=await this.store.create(ctx.playerId,()=>{const p=makeProfile(ctx,now,input.legacy);audit(p,ctx,'account.created',input.operationId,'ok','',now);audit(p,ctx,input.legacy?'cloud.profile_imported':'cloud.profile_created',input.operationId,'ok','',now);return p;});
    return {ok:true,profile:view((await this.store.read(ctx.playerId)).value),existing:!created.created};
   }
   if(operation==='GetAccountProfile'||operation==='GetEntitlements'){
    const record=await this.store.read(ctx.playerId);requireThat(record,'profile_missing');const p=validateProfile(record.value);
    return operation==='GetEntitlements'?{ok:true,entitlements:clone(p.Entitlements)}:{ok:true,profile:view(p)};
   }

   const result=await this.mutate(ctx,operation,input.operationId,input,p=>this.apply(p,ctx,operation,input,now));return {ok:!result.error,...result};
  }catch(e){if(e instanceof Rejected)return {ok:false,error:e.code};throw e;}
 }
 async mutate(ctx,operation,key,input,edit){
  const now=this.clock();validOperation(key,now);const hash=operationDigest(operation,input);
  for(let attempt=0;attempt<3;attempt++){
   const record=await this.store.read(ctx.playerId);requireThat(record,'profile_missing');const p=validateProfile(clone(record.value));
   const old=p.Operations.find(r=>r.id===key);
   if(old){if(old.hash===hash)return clone(old.result);if(p.Security.Reasons.includes('idempotency_payload_mismatch'))return {error:'operation_conflict'};flag(p,'idempotency_payload_mismatch');audit(p,ctx,'security.suspicious',key,'rejected','idempotency_payload_mismatch',now);p.Revision++;if(await this.store.compareExchange(ctx.playerId,record.version,p))return {error:'operation_conflict'};continue;}
   if(p.Daily.Day!==now.slice(0,10))p.Daily={Day:now.slice(0,10),Count:0};
   if(p.Daily.Count>=200)throw new Rejected('rate_limited');
   let result;try{requireThat(!p.Deletion||operation==='RequestAccountDeletionPreparation','deletion_pending');result=edit(p)||{};}
   catch(e){if(!(e instanceof Rejected))throw e;result={error:e.code};if(['invalid_import','impossible_entitlement','feedback_rate_limited'].includes(e.code))flag(p,e.code);}
   p.Daily.Count++;p.Revision++;audit(p,ctx,operation,key,result.error?'rejected':'ok',result.error||'',now);
   p.Operations=p.Operations.filter(r=>Date.parse(r.id.slice(0,10)+'T00:00:00Z')>=Date.parse(now.slice(0,10)+'T00:00:00Z')-86400000);
   p.Operations.push({id:key,hash,result:clone(result),at:now});requireThat(p.Operations.length<=512,'rate_limited');
   if(await this.store.compareExchange(ctx.playerId,record.version,p))return result;
  }throw new Rejected('busy');
 }
 apply(p,ctx,operation,input,now){
  if(operation==='ImportProfile2'){flag(p,'invalid_migration_attempt');throw new Rejected('import_closed');}
  if(operation==='CommitClassProgress'){
   requireThat(input.baseRevision===p.ClassRevision,'cloud_conflict');const next=localProfile(input.classProfile);
   for(const c of next.Classes){const old=p.ClassProfile.Classes.find(x=>x.Id===c.Id),def=catalog.Classes.find(x=>x.Id===c.Id);requireThat((!old.Owned||c.Owned)&&c.XP>=old.XP,'invalid_class_change');requireThat(def.Currency!=='PremiumGem'||c.Owned===old.Owned,'premium_ownership_forbidden');}
   p.ClassProfile=next;p.ClassRevision++;p.ClassSource='local_host_unverified';return {};
  }
  if(operation==='EnrollAlphaParticipation'){
   requireThat(ctx.channel==='Alpha'&&this.policy.alphaOpen&&ctx.linked&&ctx.trustedAlphaBuild===true&&this.policy.approvedBuilds.includes(ctx.buildVersion),'alpha_unavailable');
   requireThat(Date.parse(now)>=Date.parse(this.policy.alphaStart)&&Date.parse(now)<Date.parse(this.policy.alphaEnd),'alpha_unavailable');
   if(!p.Alpha)p.Alpha={Program:'alpha_2026',FirstSeenAt:now,LastSeenAt:now,Build:ctx.buildVersion,ProviderLinked:true,SourceEnvironment:ctx.environmentId};else p.Alpha.LastSeenAt=now;
   // Alpha participation is the source for a later privileged grant, not a production reward now.
   return {alphaRecorded:true};
  }
  if(operation==='SubmitFeedback'){
   requireThat(categories.includes(input.category)&&typeof input.text==='string'&&input.text.trim().length>=5&&input.text.length<=1500,'invalid_feedback');
   const recent=p.Tickets.filter(t=>t.At.slice(0,10)===now.slice(0,10));
   requireThat(recent.length<5&&(!p.Tickets.length||Date.parse(now)-Date.parse(p.Tickets[p.Tickets.length-1].At)>=60000),'feedback_rate_limited');
   const id='T-'+crypto.randomUUID();p.Tickets=p.Tickets.filter(t=>Date.parse(t.At)>Date.parse(now)-30*86400000);
   p.Tickets.push({Id:id,At:now,Category:input.category,Text:input.text.trim(),Diagnostic:input.consent===true?diagnostics(input.diagnostic):{},Status:'Open'});p.Tickets=p.Tickets.slice(-20);return {ticketId:id};
  }
  if(operation==='RecordOperationalEvent'){
   if(input.eventType==='account.linked')p.ProviderLinked=ctx.linked===true;
   requireThat(['cloud.conflict','cloud.write_failed','account.linked'].includes(input.eventType),'event_forbidden');
   audit(p,ctx,input.eventType,input.operationId,'reported','client_report_unverified',now);return {};
  }
  if(operation==='GrantEntitlement'||operation==='RealMoneyPurchase')throw new Rejected(operation==='GrantEntitlement'?'impossible_entitlement':'purchase_disabled');
  if(operation==='RequestAccountDeletionPreparation'){
   requireThat(input.confirmation==='DELETE','confirmation_required');
   if(!p.Deletion)p.Deletion={Id:'D-'+crypto.randomUUID(),At:now,Status:'Requested',CompletedEnvironments:[],AuthenticationDeleted:false};return {deletionId:p.Deletion.Id,deletionPending:true};
  }
  throw new Rejected('operation_forbidden');
 }
}
function grantEntitlement(profile,id,receipt,source,now){
 requireThat(id==='alpha_tester_2026'&&safeId(receipt)&&source==='alpha_2026','invalid_grant');
 if(profile.Entitlements.some(e=>e.Id===id))return false;
 profile.Entitlements.push({Id:id,GrantedAt:now,Source:source,ReceiptId:receipt,MetadataVersion:1});return true;
}
module.exports={AccountBackend,Rejected,requireThat,validateProfile,makeProfile,newClass,localProfile,view,supportId,diagnostics,grantEntitlement,digest,operationDigest,operations};
