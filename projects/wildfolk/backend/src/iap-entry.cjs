'use strict';
const {Rejected,requireThat}=require('./errors.cjs');const {CloudSaveStore}=require('./cloud-save.cjs');const {IapBackend}=require('./iap.cjs');const {PurchaseRecords}=require('./iap-receipts.cjs');const {authenticate}=require('./iap-auth.cjs');
module.exports=async({params,context,logger})=>{
 try{
  requireThat(['GetStoreState','BeginPurchase','VerifyPurchase','RecoverPurchases'].includes(params.operation),'operation_forbidden');requireThat(params.input&&typeof params.input==='object'&&!Array.isArray(params.input)&&JSON.stringify(params.input).length<=10000,'invalid_request');
  const http=require('axios-1.6'),{DataApi}=require('@unity-services/cloud-save-1.4'),api=new DataApi({accessToken:context.serviceToken});const {ctx,policy,config}=await authenticate(api,http,context,params);
  const call=async(operation,input)=>{const response=await http.post('https://cloud-code.services.api.unity.com/v1/projects/'+context.projectId+'/scripts/LowfirePlayVerify',{params:{operation,input,channel:ctx.channel,buildVersion:params.buildVersion}},{headers:{Authorization:'Bearer '+context.accessToken},timeout:12000});const value=response.data.output;requireThat(value?.ok,value?.error||'play_unavailable');return value;};
  const verifier={inspect:async token=>(await call('Inspect',{token})).proof,consume:async(product,token)=>await call('Consume',{token})};
  const b=new IapBackend(new CloudSaveStore(api,context.projectId,context.correlationId,'lf_economy',context.environmentId),new CloudSaveStore(api,context.projectId,context.correlationId),new PurchaseRecords(api,context.projectId,context.environmentId),verifier,config,policy,require('./store-catalog.json'));
  let result;
  if(params.operation==='GetStoreState')result=await b.state(ctx);
  else if(params.operation==='BeginPurchase'){
   requireThat(ctx.economyEnabled===true&&policy.channel==='Production'&&policy.realMoneyPurchase===true&&config.PurchaseEnabled===true&&config.ProductionLiveEnabled===true,'iap_unavailable');
   const integrity=await http.post('https://cloud-code.services.api.unity.com/v1/projects/'+context.projectId+'/scripts/LowfireIntegrityVerify',{params:{purpose:'economy',operationId:params.input.operationId,buildVersion:params.buildVersion,integrityToken:params.input.integrityToken}},{headers:{Authorization:'Bearer '+context.accessToken},timeout:12000});ctx.trustedBuild=integrity.data.output?.verified===true;result=await b.begin(ctx,params.input);
  }else result=params.operation==='VerifyPurchase'?await b.verify(ctx,params.input):await b.recover(ctx);
  logger.info(JSON.stringify({event:'iap.request',correlationId:context.correlationId,operation:params.operation,result:result.ok?'ok':'rejected'}));return result;
 }catch(e){const error=e instanceof Rejected?e.code:'purchase_unavailable';logger.warning(JSON.stringify({event:'iap.request_failed',correlationId:context.correlationId,reason:error}));return {ok:false,error};}
};
module.exports.params={operation:{type:'String',required:true},channel:{type:'String',required:true},buildVersion:{type:'String',required:true},input:{type:'JSON',required:false}};
