'use strict';
// A protected, server-created ledger receipt is the ownership authority. Profile2 import is not.
function engineerOwnership(value,ctx){
 if(![1,2].includes(value?.Schema)||value.PlayerId!==ctx.playerId||value.EnvironmentId!==ctx.environmentId||value.Engineer?.Id!=='class_engineer')return false;
 const proof=value.Schema===2&&value.Engineer.Proof?[value.Engineer.Proof]:value.Ledger;
 return proof?.some(t=>t.TransactionId===value.Engineer.TransactionId&&t.PlayerId===ctx.playerId&&t.Reason==='class_unlock'&&t.SourceId==='class:engineer'&&Number.isSafeInteger(t.Delta)&&t.Delta<0)===true;
}
module.exports={engineerOwnership};
