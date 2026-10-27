"use strict";
const L=require('../src/ledger.cjs');const {requireThat}=require('../src/errors.cjs');
function summary(p){return {Schema:1,PlayerId:p.PlayerId,EnvironmentId:p.EnvironmentId,Balance:p.Balance,Days:p.Ads?.Days||{},Operations:Object.values(p.Ads?.Operations||{}).map(o=>({OperationId:o.OperationId,Placement:'rewarded_gem',State:o.State,Day:o.Day,Amount:o.Amount,TransactionId:o.TransactionId||null,ReceiptHash:o.Receipt?.ReceiptHash||null})),Recent:p.Ads?.Audit||[],Security:p.Security,Trust:'Revive reports are client-reported diagnostics, never financial proof.'};}
async function removeBindings(storage,player,rows,operations,remove){
 const hashes=new Set(rows.filter(r=>r.Reason==='rewarded_ad'&&r.Provider==='admob').map(r=>r.ReceiptHash));
 for(const o of Object.values(operations||{}))if(o.Receipt?.Provider==='admob')hashes.add(o.Receipt.ReceiptHash);
 for(const h of hashes){requireThat(/^[a-f0-9]{64}$/.test(h),'invalid_receipt_manifest');const id=L.financialCustom(storage.environment,'ad','admob',h),r=await storage.get(id,'receipt');if(!r)continue;requireThat(r.value.PlayerId===player&&r.value.Kind==='ad'&&r.value.Provider==='admob'&&r.value.ReceiptHash===h,'financial_receipt_owner_conflict');await remove(id,'receipt');}
}
module.exports={summary,removeBindings};
