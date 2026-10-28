'use strict';
const {sha256,base64url}=require('./cryptography.cjs');const {requireThat,Rejected}=require('./errors.cjs');
const hash=s=>sha256(s);const tokenHash=token=>hash('lowfire-play-token-v1\n'+token);
const accountBinding=ctx=>hash(JSON.stringify(['lowfire-play-account-v1',ctx.projectId,ctx.environmentId,ctx.playerId]));
const profileBinding=(ctx,id)=>hash(JSON.stringify(['lowfire-play-intent-v1',accountBinding(ctx),id]));
function recordId(environment,fingerprint){return 'P_'+base64url(Buffer.from(hash(JSON.stringify(['lowfire-play-receipt-v1',environment,fingerprint])),'hex'));}
function token(value){requireThat(typeof value==='string'&&value.length>=8&&value.length<=4096&&!/[\s\x00-\x1f]/.test(value),'invalid_purchase_token');return value;}
class PurchaseRecords{
 constructor(api,project,environment){this.api=api;this.project=project;this.environment=environment;}
 async read(fingerprint){requireThat(/^[a-f0-9]{64}$/.test(fingerprint),'invalid_purchase_reference');try{return (await this.api.getPrivateCustomItems(this.project,recordId(this.environment,fingerprint),['purchase'])).data.results.find(x=>x.key==='purchase')||null;}catch(e){if(e.response?.status===404)return null;throw e;}}
 async create(value){requireThat(!await this.read(value.TokenHash),'purchase_record_exists');await this.api.setPrivateCustomItem(this.project,recordId(this.environment,value.TokenHash),{key:'purchase',value});return value;}
 async change(fingerprint,edit){for(let n=0;n<3;n++){const r=await this.read(fingerprint);requireThat(r?.writeLock,'purchase_record_missing');const value=JSON.parse(JSON.stringify(r.value));edit(value);try{await this.api.setPrivateCustomItem(this.project,recordId(this.environment,fingerprint),{key:'purchase',value,writeLock:r.writeLock});return value;}catch(e){if(e.response?.status!==409)throw e;}}throw new Rejected('busy');}
}
function safeView(p){return {ProductId:p.ProductId,GemAmount:p.GemAmount,State:p.Phase,Reference:p.TransactionId||'P-'+p.TokenHash.slice(0,12),GrantedAt:p.GrantedAt||'',CompletedAt:p.CompletedAt||'',RefundedAt:p.RefundedAt||'',Reclaimed:p.Reclaimed||0,Shortfall:p.Shortfall||0};}
module.exports={tokenHash,accountBinding,profileBinding,recordId,token,PurchaseRecords,safeView};
