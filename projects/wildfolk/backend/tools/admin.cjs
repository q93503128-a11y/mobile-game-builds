"use strict";
const fs=require('node:fs'),path=require('node:path');
const {CloudSaveStore}=require('../src/cloud-save.cjs');
const {requireThat}=require('../src/account.cjs');
const {migrateAlpha,deleteAccount}=require('../src/operations.cjs');
function parse(argv){const args={command:argv[0],apply:false};for(let i=1;i<argv.length;i++){if(argv[i]==='--apply')args.apply=true;else if(argv[i]==='--dry-run')args.apply=false;else {requireThat(argv[i].startsWith('--')&&argv[i+1],'invalid_arguments');args[argv[i].slice(2)]=argv[++i];}}return args;}
class AdminApi {
 constructor(project,environment){requireThat(/^[a-zA-Z0-9_-]+$/.test(project)&&/^[a-zA-Z0-9_-]+$/.test(environment),'invalid_scope');this.project=project;this.environment=environment;this.base='https://services.api.unity.com/cloud-save/v1/data/projects/'+project+'/environments/'+environment;}
 async request(method,url,body){
  const key=process.env.LOWFIRE_UGS_KEY_ID,secret=process.env.LOWFIRE_UGS_SECRET_KEY;requireThat(key&&secret,'credentials_required');
  const r=await fetch(url,{method,headers:{Authorization:'Basic '+Buffer.from(key+':'+secret).toString('base64'),'Content-Type':'application/json'},body:body===undefined?undefined:JSON.stringify(body),signal:AbortSignal.timeout(10000)});
  if(!r.ok){const error=new Error('admin_request_failed');error.response={status:r.status};throw error;}const text=await r.text();return {data:text?JSON.parse(text):{}};
 }
 getProtectedItems(project,player,keys){return this.request('GET',this.base+'/players/'+encodeURIComponent(player)+'/protected/items?keys='+keys.map(encodeURIComponent).join(','));}
 setProtectedItem(project,player,body){return this.request('POST',this.base+'/players/'+encodeURIComponent(player)+'/protected/items',body);}
 setProtectedItemBatch(project,player,body){return this.request('POST',this.base+'/players/'+encodeURIComponent(player)+'/protected/item-batch',body);}
 getPrivateCustomItems(project,custom,keys){return this.request('GET',this.base+'/custom/'+encodeURIComponent(custom)+'/private/items?keys='+keys.map(encodeURIComponent).join(','));}
 setPrivateCustomItem(project,custom,body){return this.request('POST',this.base+'/custom/'+encodeURIComponent(custom)+'/private/items',body);}
 async lookup(support){const r=await this.request('POST',this.base+'/queries/players/protected',{fields:[{key:'lf_support',op:'EQ',value:support,asc:true}],returnKeys:['lf_account']});requireThat(r.data.results?.length===1,'support_not_unique_or_missing');return r.data.results[0];}
 async deletePlayerData(env,player){
  const api=new AdminApi(this.project,env);
  for(const access of ['', '/public','/protected'])for(let page=0;page<100;page++){
   let data;try{data=(await api.request('GET',api.base+'/players/'+encodeURIComponent(player)+access+'/items?limit=100')).data;}catch(e){if(e.response?.status===404)break;throw e;}
   const keys=(data.results||[]).map(i=>i.key);if(!keys.length)break;
   for(const key of keys)try{await api.request('DELETE',api.base+'/players/'+encodeURIComponent(player)+access+'/items/'+encodeURIComponent(key));}catch(e){if(e.response?.status!==404)throw e;}
   requireThat(page<99,'cleanup_limit');
  }
 }
 async disableAuthentication(player){try{await this.request('POST','https://services.api.unity.com/player-identity/v1/projects/'+this.project+'/users/'+encodeURIComponent(player)+'/disable');}catch(e){if(e.response?.status!==404)throw e;}}
 async listEnvironments(){const result=await this.request('GET','https://services.api.unity.com/remote-config/v1/projects/'+this.project+'/environments');return result.data.environments.map(e=>e.id);}
 async deleteAuthentication(player){try{await this.request('DELETE','https://services.api.unity.com/player-identity/v1/projects/'+this.project+'/users/'+encodeURIComponent(player));}catch(e){if(e.response?.status!==404)throw e;}}
}
async function main(){
 const a=parse(process.argv.slice(2)),api=new AdminApi(a.project,a.environment),store=new CloudSaveStore(api,a.project);
 if(a.command==='setup'){
  requireThat(a.policy,'policy_required');const policy=JSON.parse(fs.readFileSync(a.policy,'utf8'));
  requireThat(policy.environmentId===a.environment&&['Development','Alpha','Production'].includes(policy.channel)&&policy.realMoneyPurchase===false&&!(policy.channel==='Production'&&policy.allowLegacyImport),'invalid_policy');
  const current=(await api.getPrivateCustomItems(a.project,'lowfire_backend',['policy','creation_gate'])).data.results;
  if(!a.apply){console.log(JSON.stringify({dryRun:true,environment:a.environment,policy:policy.channel,gateExists:current.some(i=>i.key==='creation_gate')}));return;}
  if(!current.some(i=>i.key==='creation_gate'))await api.setPrivateCustomItem(a.project,'lowfire_backend',{key:'creation_gate',value:{owner:''}});
  const old=current.find(i=>i.key==='policy');await api.setPrivateCustomItem(a.project,'lowfire_backend',{key:'policy',value:policy,...(old?{writeLock:old.writeLock}:{})});console.log('Environment policy provisioned; existing gate ownership preserved.');return;
 }
 if(a.command==='export-alpha'){
  requireThat(a.out,'output_required');let page=a.cursor||'',players=[];const limit=Math.min(1000,Math.max(1,Number(a.limit||1000)));
  requireThat(Number.isFinite(limit),'invalid_export_limit');
  do {const response=await api.request('GET','https://services.api.unity.com/player-identity/v1/projects/'+a.project+'/users?limit=100'+(page?'&page='+encodeURIComponent(page):''));
   // Finish the current page before returning its cursor; stopping mid-page would skip candidates.
   for(const player of response.data.results||[]){if(player.disabled)continue;const record=await store.read(player.id);if(record?.value.Alpha?.Program==='alpha_2026'&&record.value.Alpha.ProviderLinked&&!record.value.Deletion)players.push(player.id);}
   page=response.data.next||'';
  }while(page&&players.length<limit);
  fs.writeFileSync(a.out,JSON.stringify({sourceEnvironment:a.environment,players,next:page,exportedAt:new Date().toISOString()},null,2),{mode:0o600});console.log(JSON.stringify({exported:players.length,next:page||null}));return;
 }
 if(a.command==='lookup'){console.log(JSON.stringify(a.support?await api.lookup(a.support):await store.read(a.player),null,2));return;}
 if(a.command==='migrate-alpha'){
  requireThat(a.source&&a.target&&a.source!==a.target&&a.players,'invalid_migration_scope');
  const manifest=JSON.parse(fs.readFileSync(a.players,'utf8'));requireThat(Array.isArray(manifest)||manifest.sourceEnvironment===a.source,'source_environment_mismatch');const players=Array.isArray(manifest)?manifest:manifest.players;const from=new CloudSaveStore(new AdminApi(a.project,a.source),a.project),to=new CloudSaveStore(new AdminApi(a.project,a.target),a.project);
  console.log(JSON.stringify(await migrateAlpha(from,to,players,{apply:a.apply,sourceEnvironment:a.source,targetEnvironment:a.target}),null,2));return;
 }
 if(a.command==='delete-account'){
  requireThat(a.journal&&a.player,'deletion_journal_required');let job;
  if(fs.existsSync(a.journal))job=JSON.parse(fs.readFileSync(a.journal,'utf8'));
  else {const record=await store.read(a.player);requireThat(record?.value.Deletion,'deletion_not_requested');requireThat(a.environments,'all_environments_required');job={project:a.project,requestId:record.value.Deletion.Id,playerId:a.player,environments:a.environments.split(','),completed:[],authenticationDeleted:false};}
  requireThat(job.playerId===a.player&&job.project===a.project&&(!a.apply||a.confirm==='DELETE'),'deletion_confirmation_required');
  const persist=async job=>{fs.mkdirSync(path.dirname(path.resolve(a.journal)),{recursive:true});const fd=fs.openSync(a.journal+'.temporary','w',0o600);try{fs.writeFileSync(fd,JSON.stringify(job,null,2));fs.fsyncSync(fd);}finally{fs.closeSync(fd);}fs.renameSync(a.journal+'.temporary',a.journal);};
  console.log(JSON.stringify(await deleteAccount(api,job,persist,{apply:a.apply}),null,2));return;
 }
 throw new Error('unknown_command');
}
if(require.main===module)main().catch(()=>{console.error('Admin operation failed; check scope, role, setup and retained journal. No credentials were logged.');process.exitCode=1;});
module.exports={parse,AdminApi};
