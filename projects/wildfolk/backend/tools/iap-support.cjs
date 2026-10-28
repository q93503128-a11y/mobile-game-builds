'use strict';
const L=require('../src/ledger.cjs'),R=require('../src/iap-receipts.cjs');const {requireThat}=require('../src/errors.cjs');
async function lookup(store,records,p,before,limit=20){const page=await L.history(store,L.upgrade(p),before||L.sequence(p)+1,limit),ids=new Set([...(p.Iap?.Pending||[]),...(p.Iap?.Recent||[]),...page.transactions.filter(t=>t.Provider==='google_play').map(t=>t.PurchaseTokenHash)]);const purchases=[];for(const h of ids){if(!h)continue;const r=(await records.read(h))?.value;if(r?.PlayerId===p.PlayerId)purchases.push({...R.safeView(r),TokenFingerprint:h.slice(0,16),Validation:r.PreparedAt?'server_google_verified':'unknown',Test:r.Test===true,DuplicateAttempts:r.DuplicateAttempts||0});}return {PlayerId:p.PlayerId,ReviewRequired:p.Iap?.ReviewRequired===true,ReviewDisposition:p.Iap?.ReviewDisposition||null,Purchases:purchases,NextBefore:page.nextBefore,HistoryComplete:page.nextBefore<=1};}
async function minimize(storage,records,player,rows,pending,remove,retention){
 const hashes=new Set([...pending,...rows.filter(r=>r.Provider==='google_play'&&r.PurchaseTokenHash).map(r=>r.PurchaseTokenHash)]);if(!hashes.size)return;
 requireThat(retention&&/^[A-Za-z0-9._-]{1,96}$/.test(retention.PolicyReference||'')&&Number.isFinite(Date.parse(retention.ReviewAt)),'financial_retention_policy_required');
 for(const h of hashes){const id=L.financialCustom(storage.environment,'iap','google_play',h),old=await storage.get(id,'receipt');
  if(old){requireThat(old.value.PlayerId===player||old.value.DeletedOwner===true,'financial_receipt_owner_conflict');const v=old.value;await storage.api.setPrivateCustomItem(storage.project,id,{key:'receipt',writeLock:old.writeLock,value:{Schema:1,Kind:'iap',Provider:'google_play',ReceiptHash:h,Placement:v.Placement,SourceId:v.SourceId,DeletedOwner:true,RetentionPolicy:retention.PolicyReference,ReviewAt:retention.ReviewAt}});}
  const r=await records.read(h);if(r){requireThat(r.value.PlayerId===player,'financial_receipt_owner_conflict');await remove(R.recordId(storage.environment,h),'purchase');}
 }
}
module.exports={lookup,minimize};
