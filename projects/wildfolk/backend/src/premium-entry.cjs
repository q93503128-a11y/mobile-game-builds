'use strict';
const {CloudSaveStore}=require('./cloud-save.cjs');
const {PremiumAdmissionBackend}=require('./premium-admission.cjs');
const {requireThat,Rejected}=require('./errors.cjs');
module.exports=async({params,context,logger})=>{
 const correlationId=context.correlationId||'unavailable';
 try{
  requireThat(context.playerId&&context.accessToken&&context.serviceToken,'authentication_required');
  requireThat(['IssuePremiumAdmission','VerifyPremiumAdmission'].includes(params.operation),'operation_forbidden');
  const axios=require('axios-1.6'),{DataApi}=require('@unity-services/cloud-save-1.4');const api=new DataApi({accessToken:context.serviceToken});
  const config=(await api.getPrivateCustomItems(context.projectId,'lowfire_backend',['policy','economy_config'])).data.results;
  const policy=config.find(i=>i.key==='policy')?.value,economy=config.find(i=>i.key==='economy_config')?.value;
  requireThat(policy?.environmentId===context.environmentId&&policy.channel===params.channel,'wrong_environment');requireThat(economy?.Enabled===true,'economy_unavailable');
  const identity=await axios.get('https://player-auth.services.api.unity.com/v1/users/'+encodeURIComponent(context.playerId),{headers:{Authorization:'Bearer '+context.accessToken,ProjectId:context.projectId},timeout:5000});
  requireThat(identity.data.id===context.playerId&&!identity.data.disabled,'authentication_required');
  const accounts=new CloudSaveStore(api,context.projectId,correlationId),account=(await accounts.read(context.playerId))?.value;
  requireThat(account?.Schema===1&&!account.Deletion,'account_unavailable');
  const ctx={playerId:context.playerId,projectId:context.projectId,environmentId:context.environmentId,channel:policy.channel,correlationId,buildVersion:params.buildVersion,supportId:account.SupportId,linked:identity.data.externalIds?.some(i=>i.providerId==='google-play-games')===true};
  requireThat(typeof ctx.buildVersion==='string'&&/^[0-9A-Za-z._-]{1,32}$/.test(ctx.buildVersion),'invalid_build');
  const result=await new PremiumAdmissionBackend(new CloudSaveStore(api,context.projectId,correlationId,'lf_economy',context.environmentId),accounts,policy).execute(ctx,params.operation,params.input||{});
  logger.info(JSON.stringify({correlationId,operation:params.operation,playerId:context.playerId,result:result.ok?'ok':'rejected',reason:result.error||''}));return result;
 }catch(e){const error=e instanceof Rejected?e.code:'cloud_unavailable';logger.warning(JSON.stringify({correlationId,result:'error',reason:error}));return {ok:false,error};}
};
module.exports.params={operation:{type:'String',required:true},channel:{type:'String',required:true},buildVersion:{type:'String',required:true},input:{type:'JSON',required:true}};
