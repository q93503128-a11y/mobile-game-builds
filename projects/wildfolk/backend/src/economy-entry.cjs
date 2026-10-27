'use strict';
const {EconomyBackend,operations}=require('./economy.cjs');
const {CloudSaveStore}=require('./cloud-save.cjs');
const {Rejected,requireThat}=require('./errors.cjs');
module.exports=async({params,context,logger})=>{
 const correlationId=context.correlationId||'unavailable';
 try{
  requireThat(operations.has(params.operation),'operation_forbidden');requireThat(context.playerId&&context.accessToken&&context.serviceToken,'authentication_required');
  const axios=require('axios-1.6'),{DataApi}=require('@unity-services/cloud-save-1.4');const api=new DataApi({accessToken:context.serviceToken});
  const settings=await api.getPrivateCustomItems(context.projectId,'lowfire_backend',['policy','economy_config']);const policy=settings.data.results.find(r=>r.key==='policy')?.value,config=settings.data.results.find(r=>r.key==='economy_config')?.value;
  requireThat(policy&&policy.environmentId===context.environmentId&&params.channel===policy.channel,'wrong_environment');requireThat(config,'setup_required');
  const identity=await axios.get('https://player-auth.services.api.unity.com/v1/users/'+encodeURIComponent(context.playerId),{headers:{Authorization:'Bearer '+context.accessToken,ProjectId:context.projectId},timeout:5000});
  requireThat(identity.data.id===context.playerId&&!identity.data.disabled,'authentication_required');
  const account=(await new CloudSaveStore(api,context.projectId,correlationId).read(context.playerId))?.value;requireThat(account?.Schema===1&&account.PlayerId===context.playerId,'profile_missing');requireThat(!account.Deletion,'deletion_pending');
  const ctx={playerId:context.playerId,projectId:context.projectId,environmentId:context.environmentId,channel:policy.channel,buildVersion:params.buildVersion,correlationId,supportId:account.SupportId,linked:identity.data.externalIds?.some(i=>i.providerId==='google-play-games')===true};
  if(params.operation==='SubmitProgress'&&policy.channel!=='Development'){
   requireThat(ctx.linked&&policy.approvedBuilds.includes(params.buildVersion),'integrity_required');
   const existing=(await new CloudSaveStore(api,context.projectId,correlationId,'lf_economy',context.environmentId).read(context.playerId))?.value;
   if(existing?.Operations?.some(o=>o.Id===params.input?.operationId&&o.Result?.ok===true))ctx.trustedBuild=true;
   else {const proof=await axios.post('https://cloud-code.services.api.unity.com/v1/projects/'+context.projectId+'/scripts/LowfireIntegrityVerify',{params:{purpose:'economy',operationId:params.input?.operationId,buildVersion:params.buildVersion,integrityToken:params.input?.integrityToken}},{headers:{Authorization:'Bearer '+context.accessToken},timeout:10000});ctx.trustedBuild=proof.data.output?.verified===true;}
  }
  const result=await new EconomyBackend(new CloudSaveStore(api,context.projectId,correlationId,'lf_economy',context.environmentId),policy,config).execute(ctx,params.operation,params.input||{});
  logger.info(JSON.stringify({correlationId,operation:params.operation,result:result.ok?'ok':'rejected',reason:result.error||'',playerId:context.playerId}));return {...result,correlationId};
 }catch(e){const error=e instanceof Rejected?e.code:'cloud_unavailable';logger.warning(JSON.stringify({correlationId,result:'error',reason:error}));return {ok:false,error,correlationId};}
};
module.exports.params={operation:{type:'String',required:true},channel:{type:'String',required:true},buildVersion:{type:'String',required:true},input:{type:'JSON',required:false}};
