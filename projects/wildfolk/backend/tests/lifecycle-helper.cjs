'use strict';
const {LedgerStorage}=require('../src/ledger.cjs');
const clone=x=>structuredClone(x);
class PrivateApi{
 constructor(){this.data=new Map();this.version=0;this.conflicts=0;this.calls=0;}
 async getPrivateCustomItems(project,id,keys){this.calls++;return {data:{results:keys.map(key=>this.data.get(id+':'+key)).filter(Boolean).map(clone)}};}
 async setPrivateCustomItem(project,id,body){
  this.calls++;const key=id+':'+body.key,old=this.data.get(key);
  if(body.writeLock&&(this.conflicts-->0||body.writeLock!==old?.writeLock)){const e=Error('conflict');e.response={status:409};throw e;}
  const item={key:body.key,value:clone(body.value),writeLock:String(++this.version)};this.data.set(key,item);return {data:clone(item)};
 }
}
class Store{
 constructor(environment='dev'){this.records=new Map();this.api=new PrivateApi();this.ledger=new LedgerStorage(this.api,'project',environment);this.lost=false;this.conflicts=0;}
 async read(player){return this.records.has(player)?clone(this.records.get(player)):null;}
 async create(player,factory){if(!this.records.has(player))this.records.set(player,{value:factory(),version:'1'});}
 async compareExchange(player,version,value){
  const old=this.records.get(player);if(this.conflicts-->0||old?.version!==version)return false;
  this.records.set(player,{value:clone(value),version:String(Number(version)+1)});if(this.lost){this.lost=false;throw Error('unknown root commit');}return true;
 }
 async withCreationGate(work){return await work();}
}
const ctx={playerId:'player',projectId:'project',environmentId:'dev',channel:'Development',correlationId:'qa',buildVersion:'1.0',supportId:'LF-QA',linked:false};
module.exports={Store,PrivateApi,ctx,clone};
