"use strict";
const crypto=require('crypto');const {requireThat}=require('./account.cjs');
function requestHash(ctx,operationId){return crypto.createHash('sha256').update(ctx.playerId+'\n'+ctx.environmentId+'\n'+operationId+'\n'+ctx.buildVersion).digest('base64url');}
function validateVerdict(payload,policy,ctx,operationId,now=Date.now()){
 const request=payload?.requestDetails,app=payload?.appIntegrity,account=payload?.accountDetails;
 requireThat(request?.requestPackageName===policy.packageName&&request.requestHash===requestHash(ctx,operationId),'invalid_build_proof');
 const age=now-Number(request.timestampMillis);requireThat(Number.isFinite(age)&&age>=-30000&&age<=300000,'invalid_build_proof');
 requireThat(app?.appRecognitionVerdict==='PLAY_RECOGNIZED'&&app.packageName===policy.packageName&&policy.alphaVersionCodes.includes(String(app.versionCode))&&app.certificateSha256Digest?.some(v=>policy.certificates.includes(v))&&account?.appLicensingVerdict==='LICENSED','invalid_build_proof');return true;
}
async function verifyAlpha(axios,secrets,policy,ctx,input){
 requireThat(typeof input.integrityToken==='string'&&input.integrityToken.length>20&&input.integrityToken.length<=20000,'invalid_build_proof');
 requireThat(secrets&&policy.packageName&&policy.alphaVersionCodes?.length&&policy.certificates?.length,'setup_required');
 const credentials=JSON.parse((await secrets.getSecret('LOWFIRE_GOOGLE_INTEGRITY_SERVICE_ACCOUNT')).value);
 const now=Math.floor(Date.now()/1000);const encode=x=>Buffer.from(JSON.stringify(x)).toString('base64url');
 const data=encode({alg:'RS256',typ:'JWT'})+'.'+encode({iss:credentials.client_email,scope:'https://www.googleapis.com/auth/playintegrity',aud:'https://oauth2.googleapis.com/token',iat:now,exp:now+300});
 const jwt=data+'.'+crypto.sign('RSA-SHA256',Buffer.from(data),credentials.private_key).toString('base64url');
 const auth=await axios.post('https://oauth2.googleapis.com/token','grant_type=urn%3Aietf%3Aparams%3Aoauth%3Agrant-type%3Ajwt-bearer&assertion='+encodeURIComponent(jwt),{headers:{'Content-Type':'application/x-www-form-urlencoded'},timeout:4000});
 const response=await axios.post('https://playintegrity.googleapis.com/v1/'+encodeURIComponent(policy.packageName)+':decodeIntegrityToken',{integrity_token:input.integrityToken},{headers:{Authorization:'Bearer '+auth.data.access_token},timeout:4000});
 return validateVerdict(response.data.tokenPayloadExternal,policy,ctx,input.operationId);
}
module.exports={verifyAlpha,validateVerdict,requestHash};
