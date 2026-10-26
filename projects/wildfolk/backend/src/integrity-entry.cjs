"use strict";
const crypto=require('./cryptography.cjs');const {Rejected,requireThat}=require('./errors.cjs');const {validateVerdict}=require('./integrity.cjs');
async function verifyAlpha(axios,secrets,policy,ctx,input){
 requireThat(typeof input.integrityToken==='string'&&input.integrityToken.length>20&&input.integrityToken.length<=20000,'invalid_build_proof');
 requireThat(secrets&&policy.packageName&&(policy.approvedVersionCodes||policy.alphaVersionCodes)?.length&&policy.certificates?.length,'setup_required');
 const credentials=JSON.parse((await secrets.getSecret('LOWFIRE_GOOGLE_INTEGRITY_SERVICE_ACCOUNT')).value);
 const now=Math.floor(Date.now()/1000);
 const jwt=require('./integrity-signing.cjs').signJwt({alg:'RS256',typ:'JWT'},{iss:credentials.client_email,scope:'https://www.googleapis.com/auth/playintegrity',aud:'https://oauth2.googleapis.com/token',iat:now,exp:now+300},credentials.private_key);
 const auth=await axios.post('https://oauth2.googleapis.com/token','grant_type=urn%3Aietf%3Aparams%3Aoauth%3Agrant-type%3Ajwt-bearer&assertion='+encodeURIComponent(jwt),{headers:{'Content-Type':'application/x-www-form-urlencoded'},timeout:4000});
 const response=await axios.post('https://playintegrity.googleapis.com/v1/'+encodeURIComponent(policy.packageName)+':decodeIntegrityToken',{integrity_token:input.integrityToken},{headers:{Authorization:'Bearer '+auth.data.access_token},timeout:4000});
 return validateVerdict(response.data.tokenPayloadExternal,policy,ctx,input.operationId);
}
module.exports=async({params,context,logger,secretManager})=>{
 try{
  requireThat(context.playerId&&context.accessToken,'authentication_required');
  const axios=require('axios-1.6'),{DataApi}=require('@unity-services/cloud-save-1.4');const api=new DataApi({accessToken:context.serviceToken});
  const result=await api.getPrivateCustomItems(context.projectId,'lowfire_backend',['policy']);const policy=result.data.results.find(i=>i.key==='policy')?.value;
  requireThat(policy&&policy.environmentId===context.environmentId&&policy.approvedBuilds.includes(params.buildVersion),'alpha_unavailable');
  if(params.purpose==='economy')requireThat(['Alpha','Production'].includes(policy.channel),'integrity_unavailable');
  else requireThat(!params.purpose&&policy.channel==='Alpha'&&policy.alphaOpen&&Date.now()>=Date.parse(policy.alphaStart)&&Date.now()<Date.parse(policy.alphaEnd),'alpha_unavailable');
  const identity=await axios.get('https://player-auth.services.api.unity.com/v1/users/'+encodeURIComponent(context.playerId),{headers:{Authorization:'Bearer '+context.accessToken,ProjectId:context.projectId},timeout:4000});
  requireThat(identity.data.id===context.playerId&&!identity.data.disabled&&identity.data.externalIds?.some(i=>i.providerId==='google-play-games'),'authentication_required');
  const ctx={playerId:context.playerId,environmentId:context.environmentId,buildVersion:params.buildVersion};
  return {verified:await verifyAlpha(axios,secretManager,policy,ctx,params)};
 }catch(e){const reason=e instanceof Rejected?e.code:'integrity_unavailable';logger.warning(JSON.stringify({reason,correlationId:context.correlationId}));return {verified:false,error:reason};}
};
module.exports.params={operationId:{type:'String',required:true},buildVersion:{type:'String',required:true},integrityToken:{type:'String',required:true},purpose:{type:'String',required:false}};
