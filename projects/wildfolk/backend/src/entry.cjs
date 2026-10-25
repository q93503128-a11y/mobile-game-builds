"use strict";
const {AccountBackend,Rejected}=require('./account.cjs');
const {CloudSaveStore}=require('./cloud-save.cjs');
// Dependencies are provided by the official Cloud Code runtime, pinned by versioned names.
module.exports=async ({params,context,logger,secretManager})=>{
 const correlationId=require('crypto').randomUUID();
 try{
  if(!require('./account.cjs').operations.has(params.operation))throw new Rejected('operation_forbidden');
  if(!context.playerId||!context.accessToken||!context.serviceToken)throw new Rejected('authentication_required');
  const axios=require('axios-1.6');const {DataApi}=require('@unity-services/cloud-save-1.4');
  const api=new DataApi({accessToken:context.serviceToken});
  const configResult=await api.getPrivateCustomItems(context.projectId,'lowfire_backend',['policy']);
  const policy=configResult.data.results.find(i=>i.key==='policy')?.value;
  if(!policy||policy.environmentId!==context.environmentId)throw new Rejected('setup_required');
  if(params.channel!==policy.channel)throw new Rejected('wrong_environment');
  // The provider is checked by Authentication, never trusted from request JSON.
  const identity=await axios.get('https://player-auth.services.api.unity.com/v1/users/'+encodeURIComponent(context.playerId),{headers:{Authorization:'Bearer '+context.accessToken,ProjectId:context.projectId},timeout:5000});
  if(identity.data.id!==context.playerId||identity.data.disabled)throw new Rejected('authentication_required');
  const linked=identity.data.externalIds?.some(i=>i.providerId==='google-play-games')===true;
  const ctx={playerId:context.playerId,projectId:context.projectId,environmentId:context.environmentId,linked,channel:params.channel,buildVersion:params.buildVersion,correlationId};
  if(params.operation==='EnrollAlphaParticipation'){const record=await new CloudSaveStore(api,context.projectId).read(context.playerId);const previous=record?.value.Operations?.find(r=>r.id===params.input?.operationId);ctx.trustedAlphaBuild=!!previous||await require('./integrity.cjs').verifyAlpha(axios,secretManager,policy,ctx,params.input||{});}
  const backend=new AccountBackend(new CloudSaveStore(api,context.projectId),policy);
  const result=await backend.execute(ctx,params.operation,params.input||{});
  // No payload, credential, free text, external identity, HTTP exception or filesystem paths in logs.
  logger.info(JSON.stringify({correlationId,operation:params.operation,result:result.ok?'ok':'rejected',reason:result.error||'',ticketId:result.ticketId,playerId:context.playerId}));
  return {...result,correlationId};
 }catch(e){const code=e instanceof Rejected?e.code:'cloud_unavailable';logger.warning(JSON.stringify({correlationId,result:'error',reason:code}));return {ok:false,error:code,correlationId};}
};
module.exports.params={operation:{type:'String',required:true},channel:{type:'String',required:true},buildVersion:{type:'String',required:true},input:{type:'JSON',required:false}};
