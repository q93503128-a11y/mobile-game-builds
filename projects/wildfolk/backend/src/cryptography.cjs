"use strict";
const forge=require('node-forge/lib/forge');require('node-forge/lib/sha256');
function sha256(text){return forge.md.sha256.create().update(text,'utf8').digest().toHex();}
function base64url(bytes){return Buffer.from(bytes).toString('base64').replace(/=/g,'').replace(/\+/g,'-').replace(/\//g,'_');}
module.exports={sha256,base64url};
