"use strict";
const fs=require('node:fs'),path=require('node:path');
const names=['account.cjs','integrity.cjs','cloud-save.cjs','class-catalog.json','entry.cjs'];
const modules=names.map(n=>JSON.stringify('./'+n)+':function(module,exports,require){\n'+(n.endsWith('.json')?'module.exports='+fs.readFileSync(path.join(__dirname,'../src',n),'utf8')+';':fs.readFileSync(path.join(__dirname,'../src',n),'utf8'))+'\n}').join(',\n');
const code='// Generated from backend/src by node tools/bundle.cjs. Deploy this single script as LowfireAccount.\nconst nativeRequire=require;const factories={'+modules+'};const cache={};function load(id){if(!factories[id])return nativeRequire(id);if(!cache[id]){const m={exports:{}};cache[id]=m;factories[id](m,m.exports,load);}return cache[id].exports;}\nmodule.exports=load("./entry.cjs");\n';
fs.mkdirSync(path.join(__dirname,'../deploy'),{recursive:true});fs.writeFileSync(path.join(__dirname,'../deploy/LowfireAccount.js'),code);
