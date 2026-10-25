"use strict";
const test=require('node:test'),assert=require('node:assert/strict');
const {CloudSaveStore}=require('../src/cloud-save.cjs');
const copy=x=>JSON.parse(JSON.stringify(x));
class Api {
 constructor(){this.gate={value:{owner:''},writeLock:'1'};this.items=new Map();this.creates=0;this.fail=false;}
 async getProtectedItems(project,player){return {data:{results:this.items.has(player)?[{key:'lf_account',...copy(this.items.get(player))}]:[]}};}
 async getPrivateCustomItems(){return {data:{results:this.gate?[{key:'creation_gate',...copy(this.gate)}]:[]}};}
 async setPrivateCustomItem(project,id,body){if(this.gate.writeLock!==body.writeLock){const e=new Error('conflict');e.response={status:409};throw e;}this.gate={value:body.value,writeLock:String(Number(this.gate.writeLock)+1)};return {data:{writeLock:this.gate.writeLock}};}
 async setProtectedItemBatch(project,player,body){if(this.fail)throw new Error('unknown network outcome');this.creates++;this.items.set(player,{value:copy(body.data[0].value),writeLock:'1'});}
 async setProtectedItem(project,player,body){if(this.items.get(player).writeLock!==body.writeLock){const e=new Error('conflict');e.response={status:409};throw e;}this.items.set(player,{value:body.value,writeLock:String(Number(body.writeLock)+1)});}
}
test('first creation and existing path never re-import through unchecked upsert',async()=>{const api=new Api(),store=new CloudSaveStore(api,'p');await store.create('a',()=>({SupportId:'s',xp:1}));await store.create('a',()=>({SupportId:'s',xp:999}));assert.equal(api.creates,1);assert.equal((await store.read('a')).value.xp,1);assert.equal(api.gate.value.owner,'');});
test('concurrent first requests use existing-gate CAS, retry reads winner',async()=>{const api=new Api(),store=new CloudSaveStore(api,'p');const results=await Promise.allSettled([store.create('a',()=>({SupportId:'s',xp:1})),store.create('a',()=>({SupportId:'s',xp:999}))]);assert.equal(results.filter(r=>r.status==='fulfilled').length,1);await store.create('a',()=>({SupportId:'s',xp:999}));assert.equal(api.creates,1);assert.equal((await store.read('a')).value.xp,1);});
test('missing bootstrap gate fails closed without account write',async()=>{const api=new Api();api.gate=null;await assert.rejects(new CloudSaveStore(api,'p').create('a',()=>({SupportId:'s'})),e=>e.code==='setup_required');assert.equal(api.creates,0);});
test('unknown initial write outcome retains gate; no unsafe lease expiry',async()=>{const api=new Api();api.fail=true;await assert.rejects(new CloudSaveStore(api,'p').create('a',()=>({SupportId:'s'})));assert.notEqual(api.gate.value.owner,'');});
test('existing updates require write lock and reject stale revision',async()=>{const api=new Api(),store=new CloudSaveStore(api,'p');await store.create('a',()=>({SupportId:'s'}));assert.equal(await store.compareExchange('a','1',{SupportId:'s',xp:2}),true);assert.equal(await store.compareExchange('a','1',{SupportId:'s',xp:99}),false);await assert.rejects(store.compareExchange('a',null,{}));assert.equal((await store.read('a')).value.xp,2);});
