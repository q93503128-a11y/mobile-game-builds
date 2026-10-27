"use strict";
const fs=require('node:fs'),path=require('node:path'),{buildSync}=require('esbuild');
for(const [file,name] of [['entry.cjs','LowfireAccount'],['integrity-entry.cjs','LowfireIntegrityVerify'],['economy-entry.cjs','LowfireEconomy'],['premium-entry.cjs','LowfirePremiumAdmission'],['rewarded-entry.cjs','LowfireRewarded']]){
 const source=path.join(__dirname,'../src',file),params=require(source).params,outfile=path.join(__dirname,'../deploy',name+'.js');
 buildSync({entryPoints:[source],outfile,bundle:true,platform:'browser',format:'cjs',target:'es2020',external:['axios-1.6','@unity-services/cloud-save-1.4'],minify:true,legalComments:'none',banner:{js:'/* node-forge1.4.0: Copyright (c) 2010 Digital Bazaar, Inc. BSD-3-Clause; full notice in backend/THIRD_PARTY_NOTICES.md. */'},footer:{js:'module.exports.params = '+JSON.stringify(params)+';'}});
 if(fs.statSync(outfile).size>=128000)throw new Error('Cloud Code script exceeds128KB: '+name);
}
