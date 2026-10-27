'use strict';
// Only the HTTPS receiver/worker use Node crypto. Never bundled into Cloud Code.
const crypto=require('node:crypto');const {parse,KEY_URL}=require('./admob-ssv.cjs');const {requireThat}=require('./errors.cjs');
class GoogleKeys{
 constructor(fetcher=fetch,clock=()=>Date.now()){this.fetcher=fetcher;this.clock=clock;this.keys=new Map();this.expires=0;this.loading=null;}
 async load(){if(!this.loading)this.loading=(async()=>{const r=await this.fetcher(KEY_URL,{signal:AbortSignal.timeout(5000)});requireThat(r.ok,'ad_keys_unavailable');const d=await r.json();requireThat(Array.isArray(d.keys)&&d.keys.length>0&&d.keys.length<=16,'ad_keys_unavailable');const next=new Map();for(const k of d.keys){requireThat(Number.isSafeInteger(k.keyId)&&typeof k.pem==='string'&&k.pem.length<2048,'ad_keys_unavailable');const key=crypto.createPublicKey(k.pem);requireThat(key.asymmetricKeyType==='ec'&&key.asymmetricKeyDetails?.namedCurve==='prime256v1','ad_keys_unavailable');next.set(String(k.keyId),k.pem);}this.keys=next;this.expires=this.clock()+23*3600000;})().finally(()=>{this.loading=null;});return this.loading;}
 async key(id){if(this.clock()>=this.expires)await this.load();if(!this.keys.has(id))await this.load();requireThat(this.keys.has(id),'ad_unknown_key');return this.keys.get(id);}
}
function verifyWithKey(raw,pem,receivedAt){const p=parse(raw);requireThat(Number.isSafeInteger(receivedAt)&&p.ProviderTimestamp<=receivedAt+60000&&receivedAt-p.ProviderTimestamp<=600000,'ad_expired_proof');const key=crypto.createPublicKey(pem);requireThat(key.asymmetricKeyType==='ec'&&key.asymmetricKeyDetails?.namedCurve==='prime256v1','ad_invalid_proof');requireThat(crypto.verify('sha256',Buffer.from(p.content,'utf8'),key,Buffer.from(p.signature,'base64url')),'ad_invalid_signature');return p;}
async function verify(raw,keys,receivedAt){const p=parse(raw),pem=await keys.key(p.keyId);return {proof:verifyWithKey(raw,pem,receivedAt),pem};}
module.exports={GoogleKeys,verify,verifyWithKey};
