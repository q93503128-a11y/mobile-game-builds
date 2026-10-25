"use strict";
const forge=require('node-forge/lib/forge');require('node-forge/lib/rsa');require('node-forge/lib/pem');require('node-forge/lib/sha256');
const {base64url}=require('./cryptography.cjs');
function signJwt(header,claims,pem){
 const payload=base64url(Buffer.from(JSON.stringify(header),'utf8'))+'.'+base64url(Buffer.from(JSON.stringify(claims),'utf8'));
 const message=forge.pem.decode(pem)[0];
 if(!['PRIVATE KEY','RSA PRIVATE KEY'].includes(message.type)||message.procType?.type==='ENCRYPTED')throw new Error('unsupported_signing_key');
 const key=forge.pki.privateKeyFromAsn1(forge.asn1.fromDer(message.body)),md=forge.md.sha256.create().update(payload,'utf8');
 return payload+'.'+base64url(Buffer.from(key.sign(md),'binary'));
}
module.exports={signJwt};
