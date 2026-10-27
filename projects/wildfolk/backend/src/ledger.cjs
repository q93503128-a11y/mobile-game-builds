'use strict';
const {requireThat}=require('./errors.cjs');
const {sha256}=require('./cryptography.cjs');
const clone=x=>JSON.parse(JSON.stringify(x));
const SEGMENT=256, SOFT=512, HARD=768, ROOT_BYTES=3*1024*1024, INDEX_BYTES=4*1024*1024;
function bytes(value){let n=0;for(const c of JSON.stringify(value)){const x=c.codePointAt(0);n+=x<128?1:x<2048?2:x<65536?3:4;}return n;}
function canonical(value){return Array.isArray(value)?value.map(canonical):value&&typeof value==='object'?Object.fromEntries(Object.keys(value).sort().filter(k=>value[k]!==undefined).map(k=>[k,canonical(value[k])])):value;}
function hash(value){return sha256(JSON.stringify(canonical(value)));}
function compactHash(hex){
 const alphabet='ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_';let result='';
 for(let i=0;i<hex.length;i+=6){const chunk=hex.slice(i,i+6),n=parseInt(chunk.padEnd(6,'0'),16);for(let j=0;j<Math.ceil(chunk.length*2/3);j++)result+=alphabet[(n>>>(18-j*6))&63];}
 return result;
}
function sourceHash(player,environment,source){return hash(['lowfire-source-v1',player,environment,source]);}
function custom(player,environment,kind,suffix){return kind[0].toUpperCase()+'_'+compactHash(hash([player,environment,kind,suffix]));}
function financialCustom(environment,kind,provider,receiptHash){return 'F_'+compactHash(hash([environment,kind,provider,receiptHash]));}
function upgrade(value){
 const p=clone(value);
 if(p.Schema===1){
  p.Schema=2;p.Archive={Count:0,Through:0,Balance:0,Hash:''};
  if(p.Engineer){const tx=p.Ledger.find(t=>t.TransactionId===p.Engineer.TransactionId);requireThat(tx,'engineer_receipt_missing');p.Engineer.Proof=clone(tx);}
 }
 requireThat(p.Schema===2&&p.Archive&&Number.isSafeInteger(p.Archive.Count)&&p.Archive.Count>=0&&p.Archive.Through===p.Archive.Count*SEGMENT,'invalid_economy_schema');
 requireThat(Number.isSafeInteger(p.Archive.Balance)&&p.Archive.Balance>=0&&(p.Archive.Count===0?p.Archive.Balance===0&&p.Archive.Hash==='':/^[a-f0-9]{64}$/.test(p.Archive.Hash)),'archive_checkpoint_invalid');
 return p;
}
function sequence(p){return (p.Archive?.Through||0)+p.Ledger.length;}
function segmentValue(p){
 const rows=clone(p.Ledger.slice(0,SEGMENT));
 const value={Schema:1,HashVersion:1,PlayerId:p.PlayerId,EnvironmentId:p.EnvironmentId,Number:p.Archive.Count+1,PreviousHash:p.Archive.Hash,OpeningBalance:p.Archive.Balance,ClosingBalance:rows[rows.length-1].BalanceAfter,Rows:rows};
 return {...value,Hash:hash(value)};
}
function validateSegment(s,player,environment,number){
 requireThat(s?.Schema===1&&s.HashVersion===1&&s.PlayerId===player&&s.EnvironmentId===environment&&s.Number===number&&s.Rows?.length===SEGMENT,'archive_invalid');
 const {Hash,...payload}=s;requireThat(Hash===hash(payload),'archive_hash_mismatch');let balance=s.OpeningBalance;
 s.Rows.forEach((t,i)=>{requireThat(t.PlayerId===player&&t.Sequence===(number-1)*SEGMENT+i+1&&t.BalanceBefore===balance&&t.BalanceAfter===balance+t.Delta,'archive_chain_invalid');balance=t.BalanceAfter;});
 requireThat(balance===s.ClosingBalance,'archive_chain_invalid');return s;
}
async function parallel(items,work,concurrency=4){
 // Bounded service concurrency, independent of account size.
 for(let i=0;i<items.length;i+=concurrency)await Promise.all(items.slice(i,i+concurrency).map(work));
}
class LedgerStorage{
 constructor(api,project,environment){this.api=api;this.project=project;this.environment=environment;}
 async get(id,key){try{const r=await this.api.getPrivateCustomItems(this.project,id,[key]);return r.data.results.find(i=>i.key===key)||null;}catch(e){if(e.response?.status===404)return null;throw e;}}
 async archive(player,number){return (await this.get(custom(player,this.environment,'ledger',number),'segment'))?.value||null;}
 async putArchive(player,segment){
  const id=custom(player,this.environment,'ledger',segment.Number),old=await this.get(id,'segment');
  if(old)requireThat(old.value.Hash===segment.Hash&&hash(old.value)===hash(segment),'archive_conflict');
  else await this.api.setPrivateCustomItem(this.project,id,{key:'segment',value:segment});
  // Content is committed immutable prefix, so competing first writes have identical bytes.
  const saved=await this.get(id,'segment');requireThat(saved&&hash(saved.value)===hash(segment),'archive_write_unconfirmed');
 }
 async index(player,source){
  const h=sourceHash(player,this.environment,source),item=await this.get(custom(player,this.environment,'receipts',h.slice(0,2)),'s_'+compactHash(h));
  if(!item)return null;requireThat(item.value.PlayerId===player&&item.value.EnvironmentId===this.environment&&item.value.SourceId===source&&item.value.Schema===1,'receipt_hash_collision');return item.value.Sequence;
 }
 async resolution(player,source){
  const h=sourceHash(player,this.environment,'resolution:'+source),item=await this.get(custom(player,this.environment,'receipts',h.slice(0,2)),'r_'+compactHash(h));
  if(!item)return null;requireThat(item.value.PlayerId===player&&item.value.EnvironmentId===this.environment&&item.value.Revive?.Source===source&&item.value.Schema===1,'receipt_hash_collision');return item.value.Revive;
 }
 async putIndexes(player,rows=[],revives=[]){
  const entries=[];
  for(const [kind,values] of [['Sources',rows],['Revives',revives]])for(const value of values){
   const source=kind==='Sources'?value.SourceId:'resolution:'+value.Source;
   const h=sourceHash(player,this.environment,source),bucket=h.slice(0,2);
   entries.push({bucket,key:(kind==='Sources'?'s_':'r_')+compactHash(h),value:{Schema:1,PlayerId:player,EnvironmentId:this.environment,...(kind==='Sources'?{SourceId:value.SourceId,Sequence:value.Sequence}:{Revive:value})}});
  }
  // Values describe already committed immutable rows/terminal outcomes. Concurrent
  // retries and unknown late writes have identical payloads and never erase another key.
  await parallel(entries,async e=>{await this.api.setPrivateCustomItem(this.project,custom(player,this.environment,'receipts',e.bucket),{key:e.key,value:e.value});},16);
  return entries;
 }
 async bindVerifiedFinancialReceipt(receipt,gate){
  // INTERNAL server boundary only. No player/Cloud Code operation calls this until a
  // future provider verifier supplies a verified receipt. Raw purchase tokens never enter storage.
  requireThat(receipt?.Verified===true&&['ad','iap'].includes(receipt.Kind)&&/^[A-Za-z0-9_-]{1,48}$/.test(receipt.Provider)&&/^[a-f0-9]{64}$/.test(receipt.ReceiptHash)&&/^[A-Za-z0-9_-]{1,96}$/.test(receipt.PlayerId)&&/^[A-Za-z0-9_-]{1,64}$/.test(receipt.Placement)&&Number.isSafeInteger(receipt.VerifiedAt),'unverified_financial_receipt');
  const id=financialCustom(this.environment,receipt.Kind,receipt.Provider,receipt.ReceiptHash);
  const value={Schema:1,Kind:receipt.Kind,Provider:receipt.Provider,ReceiptHash:receipt.ReceiptHash,PlayerId:receipt.PlayerId,Placement:receipt.Placement,VerifiedAt:receipt.VerifiedAt,SourceId:receipt.Kind+':'+receipt.Provider+':'+receipt.ReceiptHash};
  const check=old=>{requireThat(old.value.PlayerId===value.PlayerId&&old.value.SourceId===value.SourceId&&old.value.Placement===value.Placement,'financial_receipt_owner_conflict');return old.value;};
  const old=await this.get(id,'receipt');if(old)return check(old);
  return await gate(async()=>{const prior=await this.get(id,'receipt');if(prior)return check(prior);await this.api.setPrivateCustomItem(this.project,id,{key:'receipt',value});return value;});
 }
}
async function findSource(store,p,source){
 const hot=p.Ledger.find(t=>t.SourceId===source);if(hot)return hot;
 if(!p.Archive?.Count)return null;
 requireThat(store.ledger,'archive_storage_required');const seq=await store.ledger.index(p.PlayerId,source);if(!seq)return null;
 requireThat(Number.isSafeInteger(seq)&&seq>0&&seq<=sequence(p),'receipt_index_invalid');
 const number=Math.floor((seq-1)/SEGMENT)+1;
 const segment=validateSegment(await store.ledger.archive(p.PlayerId,number),p.PlayerId,p.EnvironmentId,number);
 const row=segment.Rows[(seq-1)%SEGMENT];requireThat(row.SourceId===source,'receipt_hash_collision');return row;
}
async function compact(store,p){
 let changed=false;
 const indexEntries=(rows,revives=[])=>{
  const stats=clone(p.ReceiptBuckets||{});
  for(const [kind,entries] of [['source',rows],['resolution',revives]])for(const value of entries){
   const source=kind==='source'?value.SourceId:'resolution:'+value.Source,h=sourceHash(p.PlayerId,p.EnvironmentId,source),bucket=h.slice(0,2),key=(kind==='source'?'s_':'r_')+compactHash(h);
   const item={Schema:1,PlayerId:p.PlayerId,EnvironmentId:p.EnvironmentId,...(kind==='source'?{SourceId:value.SourceId,Sequence:value.Sequence}:{Revive:value})};
   if(!stats[bucket])stats[bucket]={Keys:0,Bytes:0};stats[bucket].Keys++;stats[bucket].Bytes+=bytes(item)+key.length;
   requireThat(stats[bucket].Keys<=2000&&stats[bucket].Bytes<INDEX_BYTES,'receipt_capacity');
  }return stats;
 };
 if(p.Ledger.length>=SOFT){
  requireThat(store.ledger,'archive_storage_required');const segment=segmentValue(p);
  const stats=indexEntries(segment.Rows);await store.ledger.putArchive(p.PlayerId,segment);await store.ledger.putIndexes(p.PlayerId,segment.Rows);p.ReceiptBuckets=stats;
  p.Ledger=p.Ledger.slice(SEGMENT);p.Archive={Count:segment.Number,Through:segment.Number*SEGMENT,Balance:segment.ClosingBalance,Hash:segment.Hash};changed=true;
 }
 const resolved=Object.values(p.Revives).filter(r=>r.State!=='Reserved');
 if(resolved.length>32){requireThat(store.ledger,'archive_storage_required');const old=resolved.slice(0,resolved.length-32),stats=indexEntries([],old);await store.ledger.putIndexes(p.PlayerId,[],old);p.ReceiptBuckets=stats;for(const r of old)delete p.Revives[r.Source];changed=true;}
 return changed;
}
function capacity(p){
 const rootBytes=bytes(p),indexes=Object.values(p.ReceiptBuckets||{});return {HotRows:p.Ledger.length,ArchiveSegments:p.Archive?.Count||0,TotalRows:sequence(p),RootBytes:rootBytes,ReceiptIndexEntries:indexes.reduce((n,s)=>n+s.Keys,0),ReceiptIndexBytes:indexes.reduce((n,s)=>n+s.Bytes,0),IndexShards:256,IndexHardKeys:2000,IndexHardBytes:INDEX_BYTES,Status:rootBytes>=ROOT_BYTES*.9||p.Ledger.length>=HARD||indexes.some(s=>s.Keys>=1800||s.Bytes>=INDEX_BYTES*.9)?'Critical':rootBytes>=ROOT_BYTES*.75||p.Ledger.length>=SOFT||indexes.some(s=>s.Keys>=1500||s.Bytes>=INDEX_BYTES*.75)?'Warning':'Healthy'};
}
async function history(store,p,before=sequence(p)+1,limit=20){
 requireThat(Number.isSafeInteger(before)&&before>0&&before<=sequence(p)+1&&Number.isSafeInteger(limit)&&limit>0&&limit<=50,'invalid_page');
 const rows=[],cache=new Map();
 for(let seq=before-1;seq>0&&rows.length<limit;seq--){
  if(seq>p.Archive.Through)rows.push(p.Ledger[seq-p.Archive.Through-1]);
  else {const n=Math.floor((seq-1)/SEGMENT)+1;if(!cache.has(n))cache.set(n,validateSegment(await store.ledger.archive(p.PlayerId,n),p.PlayerId,p.EnvironmentId,n));rows.push(cache.get(n).Rows[(seq-1)%SEGMENT]);}
 }return {transactions:rows,nextBefore:rows.length?rows[rows.length-1].Sequence:before,transactionCount:sequence(p)};
}
async function transaction(store,p,id){
 const match=/^G-([1-9][0-9]*)-[a-f0-9]{12}$/.exec(id||'');requireThat(match,'invalid_transaction_id');
 const seq=Number(match[1]);if(!Number.isSafeInteger(seq)||seq>sequence(p))return null;
 const rows=(await history(store,p,seq+1,1)).transactions;return rows[0]?.TransactionId===id?rows[0]:null;
}
async function reconcile(store,p,cursor={Next:1,Balance:0,Hash:'',Count:p.Archive.Count},limit=4){
 requireThat(cursor.Count===p.Archive.Count&&Number.isSafeInteger(cursor.Next)&&cursor.Next>=1&&cursor.Next<=cursor.Count+1&&Number.isSafeInteger(limit)&&limit>0&&limit<=8,'reconciliation_restart_required');
 let {Next,Balance,Hash,Count}=cursor;
 for(let n=0;n<limit&&Next<=Count;n++,Next++){
  const s=validateSegment(await store.ledger.archive(p.PlayerId,Next),p.PlayerId,p.EnvironmentId,Next);
  requireThat(s.PreviousHash===Hash&&s.OpeningBalance===Balance,'archive_chain_invalid');Balance=s.ClosingBalance;Hash=s.Hash;
 }
 const complete=Next>Count;
 if(complete){requireThat(Balance===p.Archive.Balance&&Hash===p.Archive.Hash,'archive_checkpoint_invalid');for(const t of p.Ledger){requireThat(t.BalanceBefore===Balance,'ledger_inconsistent');Balance=t.BalanceAfter;}requireThat(Balance===p.Balance,'ledger_inconsistent');}
 return {complete,balance:complete?Balance:undefined,cursor:{Next,Balance:complete?p.Archive.Balance:Balance,Hash,Count}};
}
module.exports={SEGMENT,SOFT,HARD,ROOT_BYTES,INDEX_BYTES,bytes,hash,canonical,compactHash,custom,financialCustom,sourceHash,upgrade,sequence,segmentValue,validateSegment,parallel,LedgerStorage,findSource,compact,capacity,history,transaction,reconcile};
