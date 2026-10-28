const test=require('node:test'),assert=require('node:assert/strict');const {server}=require('../ingress/server.cjs');
test('HTTP receiver preserves original encoded query, waits for durability, and fails closed for worker auth',async()=>{
 let seen,release;const durable=new Promise(r=>release=r),runtime={receive:async raw=>{seen=raw;await durable;},work:async(packet,auth)=>{if(auth!=='Bearer fixture')throw Error('worker_unauthorized');}};
 const app=server(runtime);await new Promise(resolve=>app.listen(0,'127.0.0.1',resolve));const base='http://127.0.0.1:'+app.address().port;try{
  let ack=false;const request=fetch(base+'/admob/ssv?custom_data=a%2Fb&signature=x&key_id=1').then(r=>{ack=true;return r;});
  for(let n=0;n<100&&!seen;n++)await new Promise(r=>setTimeout(r,5));assert.equal(seen,'custom_data=a%2Fb&signature=x&key_id=1');assert.equal(ack,false);release();assert.equal((await request).status,200);
  assert.equal((await fetch(base+'/deliver',{method:'POST',body:'{}'})).status,400);
  assert.equal((await fetch(base+'/deliver',{method:'POST',headers:{Authorization:'Bearer fixture'},body:'{}'})).status,200);
  assert.equal((await fetch(base+'/deliver',{method:'POST',body:'x'.repeat(16001)})).status,400);
  assert.equal((await fetch(base+'/missing')).status,404);
 }finally{app.closeAllConnections();await new Promise(r=>app.close(r));}
});
