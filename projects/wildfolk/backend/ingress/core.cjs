'use strict';
const crypto=require('node:crypto');const S=require('../src/admob-ssv.cjs'),N=require('../src/admob-ssv-node.cjs');const {requireThat}=require('../src/errors.cjs');
// The public receiver acknowledges only after a durable task exists. The task's
// server HMAC covers the original query AND the public key observed on receipt.
// Worker authentication/HMAC protect this retained key across provider rotation.
class AdMobIngress{
 constructor({projectId,environmentId,channel,gemAdUnit,secret,keys=new N.GoogleKeys(),queue,verifyWorker,deliver,clock=()=>Date.now()}){Object.assign(this,{projectId,environmentId,channel,gemAdUnit,secret,keys,queue,verifyWorker,deliver,clock});}
 async receive(raw){
  const now=this.clock(),{proof,pem}=await N.verify(raw,this.keys,now);requireThat(proof.EnvironmentId===this.environmentId&&proof.Placement==='rewarded_gem'&&proof.AdUnit===this.gemAdUnit&&proof.ProviderItem==='lowfire_gem','ad_placement_mismatch');
  const envelope={Schema:1,ProjectId:this.projectId,EnvironmentId:this.environmentId,Channel:this.channel,KeyId:'ads-ssv-v1',ReceivedAt:now,RawQuery:raw,PublicKeyPem:pem};const packet={envelope,seal:S.seal(envelope,this.secret)};
  const taskId=crypto.createHash('sha256').update(this.environmentId+':admob:'+proof.ProviderRewardId).digest('hex');await this.queue.enqueue(taskId,packet);return {accepted:true};
 }
 async work(packet,authorization){
  requireThat(await this.verifyWorker(authorization),'worker_unauthorized');const ctx={projectId:this.projectId,environmentId:this.environmentId,channel:this.channel};S.attest(packet,this.secret,ctx,this.clock());
  N.verifyWithKey(packet.envelope.RawQuery,packet.envelope.PublicKeyPem,packet.envelope.ReceivedAt);const result=await this.deliver(packet);requireThat(result?.ok,'ad_delivery_retry');return {delivered:true};
 }
}
module.exports={AdMobIngress};
