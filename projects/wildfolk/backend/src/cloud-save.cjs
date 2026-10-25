"use strict";
const {requireThat}=require('./account.cjs');
// One pre-provisioned Private Game Data gate makes first creation safe. No unchecked upsert race.
// A crashed holder blocks new creation until an operator verifies execution has stopped and clears it.
class CloudSaveStore {
 constructor(api,projectId,invocationId){this.api=api;this.projectId=projectId;this.invocationId=invocationId;}
 async read(player){const r=await this.api.getProtectedItems(this.projectId,player,['lf_account']);const item=r.data.results.find(i=>i.key==='lf_account');return item?{value:item.value,version:item.writeLock}:null;}
 async compareExchange(player,version,value){requireThat(typeof version==='string'&&version.length>0,'missing_write_lock');try{await this.api.setProtectedItem(this.projectId,player,{key:'lf_account',value,writeLock:version});return true;}catch(e){if(e.response?.status===409)return false;throw e;}}
 async create(player,factory){
  let current=await this.read(player);if(current)return {created:false,...current};
  const gateResult=await this.api.getPrivateCustomItems(this.projectId,'lowfire_backend',['creation_gate']);const gate=gateResult.data.results.find(i=>i.key==='creation_gate');
  requireThat(gate&&gate.value.owner===''&&gate.writeLock,'setup_required');
  requireThat(typeof this.invocationId==='string'&&this.invocationId.length>0,'missing_invocation_id');const owner=require('./cryptography.cjs').sha256(this.invocationId+':'+player);let acquired;
  try{acquired=await this.api.setPrivateCustomItem(this.projectId,'lowfire_backend',{key:'creation_gate',value:{owner},writeLock:gate.writeLock});}
  catch(e){if(e.response?.status===409)throw new (require('./account.cjs').Rejected)('busy');throw e;}
  let safeToRelease=false;
  try{
   current=await this.read(player);if(current){safeToRelease=true;return {created:false,...current};}
   const value=factory();await this.api.setProtectedItemBatch(this.projectId,player,{data:[{key:'lf_account',value},{key:'lf_support',value:value.SupportId}]});safeToRelease=true;return {created:true,value};
  }finally{
   // A timeout has an unknown commit outcome. Never unlock and allow a late initial upsert to overwrite a retry.
   if(safeToRelease){const latest=await this.api.getPrivateCustomItems(this.projectId,'lowfire_backend',['creation_gate']);const item=latest.data.results.find(i=>i.key==='creation_gate');
   if(item?.value.owner===owner)await this.api.setPrivateCustomItem(this.projectId,'lowfire_backend',{key:'creation_gate',value:{owner:''},writeLock:item.writeLock});}
  }
 }
}
module.exports={CloudSaveStore};
