'use strict';
const {requireThat,Rejected}=require('./errors.cjs');const {CloudSaveStore}=require('./cloud-save.cjs');const {RewardedBackend,policy:adPolicy}=require('./rewarded.cjs');const SSV=require('./admob-ssv.cjs');
module.exports=async({params,context,logger,secretManager})=>{
 let backend,ctx;
 try{
  requireThat(context.serviceToken,'authentication_required');const axios=require('axios-1.6'),{DataApi}=require('@unity-services/cloud-save-1.4'),api=new DataApi({accessToken:context.serviceToken});
  const settings=(await api.getPrivateCustomItems(context.projectId,'lowfire_backend',['policy','ads_config'])).data.results;const policy=settings.find(x=>x.key==='policy')?.value,config=settings.find(x=>x.key==='ads_config')?.value;
  requireThat(policy?.environmentId===context.environmentId&&config?.Schema===1,'ads_setup_required');requireThat(params.channel===policy.channel,'wrong_environment');
  ctx={playerId:context.playerId,projectId:context.projectId,environmentId:context.environmentId,channel:policy.channel,correlationId:context.correlationId,buildVersion:params.buildVersion,linked:false};
  const store=new CloudSaveStore(api,context.projectId,context.correlationId,'lf_economy',context.environmentId),accounts=new CloudSaveStore(api,context.projectId,context.correlationId);backend=new RewardedBackend(store,accounts,config,policy);
  if(params.operation==='ReceiveAdMobSsv'){
   requireThat(config.IngressKeyName==='LOWFIRE_ADS_INGRESS_HMAC_V1'&&secretManager,'ads_setup_required');const key=(await secretManager.getSecret(config.IngressKeyName)).value;
   const proof=SSV.attest(params.input,key,ctx,Date.now());const result=await backend.receive(ctx,proof);logger.info(JSON.stringify({event:'ads.ssv_processed',correlationId:context.correlationId,result:result.ok?'ok':'rejected'}));return result;
  }
  adPolicy(config,ctx.channel);
  requireThat(context.playerId&&context.accessToken&&typeof params.buildVersion==='string'&&/^[0-9A-Za-z._-]{1,32}$/.test(params.buildVersion),'authentication_required');
  // Reuse the existing authenticated provider boundary. No client-owned bool,
  // cached social-provider field, or device clock authorizes a financial ad.
  const r=await axios.get('https://player-auth.services.api.unity.com/v1/users/'+encodeURIComponent(context.playerId),{headers:{Authorization:'Bearer '+context.accessToken,ProjectId:context.projectId},timeout:4000});requireThat(r.data.id===context.playerId&&!r.data.disabled,'authentication_required');ctx.linked=r.data.externalIds?.some(x=>x.providerId==='google-play-games')===true;
  const account=(await accounts.read(ctx.playerId))?.value;ctx.supportId=account?.SupportId;
  const result=await backend.execute(ctx,params.operation,params.input||{});if(!result.ok&&['ad_invalid_proof','ad_placement_mismatch','ad_receipt_conflict','ad_operation_reused'].includes(result.error))await backend.rejected(ctx,result.error);return result;
 }catch(e){const error=e instanceof Rejected?e.code:'ad_unavailable';if(backend&&ctx?.playerId&&['ad_invalid_proof','ad_placement_mismatch','ad_receipt_conflict','ad_operation_reused'].includes(error))try{await backend.rejected(ctx,error);}catch{}logger.warning(JSON.stringify({event:'ads.request_failed',correlationId:context.correlationId,reason:error}));return {ok:false,error};}
};
module.exports.params={operation:{type:'String',required:true},channel:{type:'String',required:true},buildVersion:{type:'String',required:true},input:{type:'JSON',required:false}};
