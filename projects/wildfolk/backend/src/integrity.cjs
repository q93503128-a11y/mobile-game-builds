"use strict";
const crypto=require('./cryptography.cjs');const {requireThat}=require('./errors.cjs');
function requestHash(ctx,operationId){return crypto.base64url(Buffer.from(crypto.sha256(ctx.playerId+'\n'+ctx.environmentId+'\n'+operationId+'\n'+ctx.buildVersion),'hex'));}
function validateVerdict(payload,policy,ctx,operationId,now=Date.now()){
 const request=payload?.requestDetails,app=payload?.appIntegrity,account=payload?.accountDetails;
 requireThat(request?.requestPackageName===policy.packageName&&request.requestHash===requestHash(ctx,operationId),'invalid_build_proof');
 const age=now-Number(request.timestampMillis);requireThat(Number.isFinite(age)&&age>=-30000&&age<=300000,'invalid_build_proof');
 requireThat(app?.appRecognitionVerdict==='PLAY_RECOGNIZED'&&app.packageName===policy.packageName&&(policy.approvedVersionCodes||policy.alphaVersionCodes).includes(String(app.versionCode))&&app.certificateSha256Digest?.some(v=>policy.certificates.includes(v))&&account?.appLicensingVerdict==='LICENSED','invalid_build_proof');return true;
}
module.exports={validateVerdict,requestHash};
