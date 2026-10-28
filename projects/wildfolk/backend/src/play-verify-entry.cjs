'use strict';
const {Rejected,requireThat}=require('./errors.cjs');const R=require('./iap-receipts.cjs');const {parsePurchase}=require('./play-purchase.cjs');const {authenticate}=require('./iap-auth.cjs');
module.exports=async({params,context,logger,secretManager})=>{
 try{
  requireThat(['Inspect','Consume'].includes(params.operation),'operation_forbidden');const http=require('axios-1.6'),{DataApi}=require('@unity-services/cloud-save-1.4'),api=new DataApi({accessToken:context.serviceToken});const {ctx,config}=await authenticate(api,http,context,params);
  requireThat(ctx.channel==='Production'&&config.SettlementEnabled===true&&config.PackageName==='com.gltam.lowfire'&&config.ExternalSetupReviewed===true,'iap_unavailable');
  const token=R.token(params.input?.token),fingerprint=R.tokenHash(token);requireThat(secretManager,'iap_setup_required');
  const credential=JSON.parse((await secretManager.getSecret('LOWFIRE_GOOGLE_PLAY_SERVICE_ACCOUNT')).value),google=new(require('./play-api.cjs').GooglePlayApi)(http,credential,config.PackageName);
  const proof=parsePurchase(await google.inspect(token));requireThat(proof.AccountBinding===R.accountBinding(ctx)&&(!config.TestOnly||proof.Test),'purchase_owner_conflict');
  const records=new R.PurchaseRecords(api,context.projectId,context.environmentId),record=(await records.read(fingerprint))?.value;
  if(params.operation==='Consume'){
   requireThat(record?.PlayerId===ctx.playerId&&record.EnvironmentId===ctx.environmentId&&['Granted','Completed'].includes(record.Phase)&&record.ProductId===proof.ProductId&&record.ProfileBinding===proof.ProfileBinding&&record.AccountBinding===proof.AccountBinding&&record.TransactionId,'purchase_not_granted');
   if(proof.State==='CANCELLED')return {ok:true,status:'Reversed'};requireThat(proof.State==='PURCHASED','purchase_not_completed');if(!proof.Consumed)await google.consume(record.ProductId,token);return {ok:true,status:'Completed'};
  }
  const root=(await api.getProtectedItems(context.projectId,context.playerId,['lf_economy'])).data.results.find(x=>x.key==='lf_economy')?.value;
  requireThat(record?.PlayerId===ctx.playerId||Object.values(root?.Iap?.Intents||{}).some(i=>i.ProductId===proof.ProductId&&i.ProfileBinding===proof.ProfileBinding),'purchase_intent_missing');return {ok:true,proof};
 }catch(e){const error=e instanceof Rejected?e.code:'play_unavailable';logger.warning(JSON.stringify({event:'iap.google_request_failed',correlationId:context.correlationId,reason:error}));return {ok:false,error};}
};
module.exports.params={operation:{type:'String',required:true},channel:{type:'String',required:true},buildVersion:{type:'String',required:true},input:{type:'JSON',required:false}};
