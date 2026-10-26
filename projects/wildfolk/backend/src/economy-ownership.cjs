'use strict';
// A protected, server-created ledger receipt is the ownership authority. Profile2 import is not.
function engineerOwnership(value,ctx){
 if(value?.Schema!==1||value.PlayerId!==ctx.playerId||value.EnvironmentId!==ctx.environmentId||value.Engineer?.Id!=='class_engineer')return false;
 return value.Ledger?.some(t=>t.TransactionId===value.Engineer.TransactionId&&t.PlayerId===ctx.playerId&&t.Reason==='class_unlock'&&t.SourceId==='class:engineer'&&Number.isSafeInteger(t.Delta)&&t.Delta<0)===true;
}
module.exports={engineerOwnership};
