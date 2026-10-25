"use strict";
const {validateProfile,grantEntitlement,requireThat,digest}=require('./account.cjs');
async function migrateAlpha(source,target,players,{apply=false,sourceEnvironment,targetEnvironment,now=new Date().toISOString()}={}){
 requireThat(sourceEnvironment&&targetEnvironment&&sourceEnvironment!==targetEnvironment,'wrong_environment');
 const results=[];
 for(const player of [...new Set(players)]){
  const from=await source.read(player);if(!from){results.push({player,result:'not_found'});continue;}
  const p=validateProfile(from.value);if(p.Deletion||p.Alpha?.Program!=='alpha_2026'||!p.Alpha.ProviderLinked||p.Alpha.SourceEnvironment!==sourceEnvironment){results.push({player,result:'not_qualified'});continue;}
  if(!await target.read(player)){results.push({player,result:'production_profile_required'});continue;}
  const receipt=digest('alpha_2026:'+sourceEnvironment+':'+player);
  for(let attempt=0;attempt<3;attempt++){
   const to=await target.read(player),value=validateProfile(to.value);
   if(value.Deletion){results.push({player,result:'deletion_pending'});break;}
   if(value.Entitlements.some(e=>e.Id==='alpha_tester_2026')){results.push({player,result:'already_granted'});break;}
   if(!apply){results.push({player,result:'would_grant'});break;}
   grantEntitlement(value,'alpha_tester_2026',receipt,'alpha_2026',now);value.Revision++;
   value.Audit.push({eventId:receipt,playerId:player,supportId:value.SupportId,timestamp:now,correlationId:receipt,buildVersion:'admin',buildChannel:'Production',eventType:'entitlement.granted',operationId:receipt,result:'ok',reason:'verified_alpha_export'});value.Audit=value.Audit.slice(-64);
   if(await target.compareExchange(player,to.version,value)){results.push({player,result:'granted',receipt});break;}
  }
  if(!results.some(r=>r.player===player))results.push({player,result:'busy'});
 }
 return {dryRun:!apply,candidateCount:new Set(players).size,duplicateCount:players.length-new Set(players).size,alreadyGrantedCount:results.filter(r=>r.result==='already_granted').length,targetCount:results.filter(r=>['would_grant','granted'].includes(r.result)).length,grantPlan:results.filter(r=>['would_grant','granted'].includes(r.result)).map(r=>({player:r.player,operationId:digest('alpha_2026:'+sourceEnvironment+':'+r.player)})),results};
}
// Persist the operator journal before deleting anything; Auth is always the final service.
async function deleteAccount(admin,job,persist,{apply=false}={}){
 requireThat(job&&job.requestId&&job.playerId&&Array.isArray(job.environments)&&new Set(job.environments).size===job.environments.length&&job.environments.length>=3,'invalid_deletion_job');
 const actual=await admin.listEnvironments();requireThat(Array.isArray(actual)&&actual.length===job.environments.length&&actual.every(id=>job.environments.includes(id)),'incomplete_environment_cleanup');
 if(!apply)return {dryRun:true,player:job.playerId,environments:job.environments};
 await persist(job);job.completed=job.completed||[];
 if(!job.authenticationDisabled){await admin.disableAuthentication(job.playerId);job.authenticationDisabled=true;await persist(job);}
 for(const env of job.environments){if(job.completed.includes(env))continue;await admin.deletePlayerData(env,job.playerId);job.completed.push(env);await persist(job);}
 if(!job.authenticationDeleted){await admin.deleteAuthentication(job.playerId);job.authenticationDeleted=true;await persist(job);}
 return {complete:job.authenticationDeleted&&job.completed.length===job.environments.length};
}
module.exports={migrateAlpha,deleteAccount};
