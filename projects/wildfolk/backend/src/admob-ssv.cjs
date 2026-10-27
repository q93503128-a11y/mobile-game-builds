'use strict';
const {requireThat}=require('./errors.cjs');
const {sha256}=require('./cryptography.cjs');
const L=require('./ledger.cjs');
const forge=require('node-forge/lib/forge');require('node-forge/lib/hmac');
const KEY_URL='https://www.gstatic.com/admob/reward/verifier-keys.json';
function parse(raw){
 requireThat(typeof raw==='string'&&raw.length>=40&&raw.length<=8192&&!/[\r\n#]/.test(raw),'ad_invalid_proof');
 const match=/^(.*)&signature=([A-Za-z0-9_%=-]+)&key_id=([0-9]{1,12})$/.exec(raw);requireThat(match&&match[1]&&!match[1].includes('&signature='),'ad_invalid_proof');
 const values={};for(const part of raw.split('&')){const at=part.indexOf('=');requireThat(at>0,'ad_invalid_proof');const key=part.slice(0,at);requireThat(/^[a-z_]+$/.test(key)&&!Object.prototype.hasOwnProperty.call(values,key),'ad_invalid_proof');try{values[key]=decodeURIComponent(part.slice(at+1).replace(/\+/g,' '));}catch{requireThat(false,'ad_invalid_proof');}}
 requireThat(/^[A-Za-z0-9_-]{1,128}$/.test(values.transaction_id||'')&&/^[A-Za-z0-9_-]{1,96}$/.test(values.user_id||'')&&/^[0-9]{1,20}$/.test(values.ad_network||'')&&/^[0-9]{10,16}$/.test(values.timestamp||''),'ad_invalid_proof');
 const custom=/^lf3\.([a-f0-9-]{36})\.([a-f0-9]{32})\.([a-f0-9]{64})\.(rewarded_gem|rewarded_revive)$/.exec(values.custom_data||'');requireThat(custom,'ad_invalid_proof');
 const timestamp=Number(values.timestamp),amount=Number(values.reward_amount);requireThat(Number.isSafeInteger(timestamp)&&Number.isSafeInteger(amount)&&amount>0&&amount<=100000&&typeof values.reward_item==='string'&&values.reward_item.length<=64,'ad_invalid_proof');
 const unit=(values.ad_unit||'').split('/').at(-1);requireThat(/^[0-9]{1,24}$/.test(unit),'ad_invalid_proof');
 return {content:match[1],signature:values.signature,keyId:values.key_id,Provider:'admob',ProviderRewardId:values.transaction_id,ReceiptHash:sha256(values.transaction_id),PlayerId:values.user_id,EnvironmentId:custom[1],OperationId:custom[2],Nonce:custom[3],Placement:custom[4],AdUnit:unit,ProviderTimestamp:timestamp,ProviderAmount:amount,ProviderItem:values.reward_item,AdNetwork:values.ad_network};
}
function seal(envelope,secret){requireThat(typeof secret==='string'&&secret.length>=32&&secret.length<=1024,'ads_setup_required');const h=forge.hmac.create();h.start('sha256',secret);h.update('lowfire-admob-ingress-v1\n'+JSON.stringify(L.canonical(envelope)));return h.digest().toHex();}
function equal(a,b){if(typeof a!=='string'||typeof b!=='string'||a.length!==b.length)return false;let n=0;for(let i=0;i<a.length;i++)n|=a.charCodeAt(i)^b.charCodeAt(i);return n===0;}
function attest(input,secret,ctx,now){
 const e=input?.envelope;requireThat(e?.Schema===1&&e.ProjectId===ctx.projectId&&e.EnvironmentId===ctx.environmentId&&e.Channel===ctx.channel&&e.KeyId==='ads-ssv-v1'&&Number.isSafeInteger(e.ReceivedAt)&&e.ReceivedAt<=now+60000&&now-e.ReceivedAt<=7*86400000&&typeof e.RawQuery==='string','ad_invalid_proof');
 requireThat(/^[a-f0-9]{64}$/.test(input.seal||'')&&equal(input.seal,seal(e,secret)),'ad_invalid_proof');const proof=parse(e.RawQuery);requireThat(proof.EnvironmentId===ctx.environmentId&&proof.ProviderTimestamp<=e.ReceivedAt+60000&&e.ReceivedAt-proof.ProviderTimestamp<=600000,'ad_expired_proof');return {...proof,ReceivedAt:e.ReceivedAt};
}
module.exports={KEY_URL,parse,seal,attest,equal};
