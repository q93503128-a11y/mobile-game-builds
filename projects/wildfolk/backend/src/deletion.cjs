"use strict";
const {requireThat,Rejected}=require('./errors.cjs');
// Short gate only: mark first, release, then perform the potentially long archive
// cleanup. Provider workers check the mark inside the receipt-creation gate.
async function prepareDeletion(store,player,requestId,now=new Date().toISOString()){
 requireThat(typeof requestId==='string'&&requestId.length>0&&requestId.length<=128,'deletion_request_required');
 if(!await store.read(player))return {skipped:true};
 return await store.withCreationGate(async()=>{
  for(let n=0;n<3;n++){
   const r=await store.read(player);if(!r)return {skipped:true};
   requireThat(r.value.Schema===1&&r.value.PlayerId===player,'invalid_deletion_profile');
   if(r.value.Deletion)return {prepared:true};
   const p=structuredClone(r.value);p.Deletion={Id:requestId,At:now,Status:'Requested',CompletedEnvironments:[],AuthenticationDeleted:false};p.Revision++;
   if(await store.compareExchange(player,r.version,p))return {prepared:true};
  }return {busy:true};
 },'account-delete:'+player).then(result=>{if(result.busy)throw new Rejected('busy');return result;});
}
module.exports={prepareDeletion};
