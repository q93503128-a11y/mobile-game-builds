'use strict';
const {requireThat}=require('./errors.cjs');
// Actual REST v2 schema. A client receipt, signature string or success bool never enters here.
function parsePurchase(value){
 const state=value?.purchaseStateContext?.purchaseState;requireThat(['PURCHASED','PENDING','CANCELLED'].includes(state)&&value.productLineItem?.length===1,'invalid_play_purchase');
 const item=value.productLineItem[0],offer=item.productOfferDetails;
 requireThat(/^[a-z][a-z0-9_.]{1,47}$/.test(item.productId||'')&&offer&&offer.quantity===1&&!offer.rentOfferDetails&&!offer.preorderOfferDetails&&!offer.offerId,'unsupported_play_purchase');
 requireThat(['CONSUMPTION_STATE_YET_TO_BE_CONSUMED','CONSUMPTION_STATE_CONSUMED'].includes(offer.consumptionState),'invalid_play_purchase');
 requireThat(/^[a-f0-9]{64}$/.test(value.obfuscatedExternalAccountId||'')&&/^[a-f0-9]{64}$/.test(value.obfuscatedExternalProfileId||''),'purchase_binding_missing');
 if(state==='PURCHASED')requireThat(Number.isFinite(Date.parse(value.purchaseCompletionTime))&&['ACKNOWLEDGEMENT_STATE_PENDING','ACKNOWLEDGEMENT_STATE_ACKNOWLEDGED'].includes(value.acknowledgementState),'invalid_play_purchase');
 return {State:state,ProductId:item.productId,Quantity:1,Consumed:offer.consumptionState==='CONSUMPTION_STATE_CONSUMED',AccountBinding:value.obfuscatedExternalAccountId,ProfileBinding:value.obfuscatedExternalProfileId,Test:value.testPurchaseContext?.fopType==='TEST',OrderId:typeof value.orderId==='string'?value.orderId.slice(0,128):'',PurchasedAt:value.purchaseCompletionTime||''};
}
module.exports={parsePurchase};
