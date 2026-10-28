'use strict';
const {requireThat}=require('./errors.cjs');const {accountBinding}=require('./iap-receipts.cjs');
async function authenticate(api,http,context,params){
 requireThat(context.playerId&&context.accessToken&&context.serviceToken&&typeof params.buildVersion==='string'&&/^[0-9A-Za-z._-]{1,32}$/.test(params.buildVersion),'authentication_required');
 const settings=(await api.getPrivateCustomItems(context.projectId,'lowfire_backend',['policy','iap_config','economy_config'])).data.results,policy=settings.find(x=>x.key==='policy')?.value,config=settings.find(x=>x.key==='iap_config')?.value||require('./iap-config.json');
 requireThat(policy?.environmentId===context.environmentId&&params.channel===policy.channel,'wrong_environment');
 const identity=await http.get('https://player-auth.services.api.unity.com/v1/users/'+encodeURIComponent(context.playerId),{headers:{Authorization:'Bearer '+context.accessToken,ProjectId:context.projectId},timeout:4000});
 requireThat(identity.data.id===context.playerId&&!identity.data.disabled&&identity.data.externalIds?.some(x=>x.providerId==='google-play-games'),'purchase_account_required');
 const account=(await api.getProtectedItems(context.projectId,context.playerId,['lf_account'])).data.results.find(x=>x.key==='lf_account')?.value;requireThat(account?.PlayerId===context.playerId&&!account.Deletion,'account_unavailable');
 const ctx={playerId:context.playerId,projectId:context.projectId,environmentId:context.environmentId,channel:policy.channel,correlationId:context.correlationId,buildVersion:params.buildVersion,supportId:account.SupportId,linked:true,economyEnabled:settings.find(x=>x.key==='economy_config')?.value?.Enabled===true};return {ctx,policy,config,binding:accountBinding(ctx)};
}
module.exports={authenticate};
