'use strict';
const {requireThat}=require('./errors.cjs');const {signJwt}=require('./integrity-signing.cjs');
class GooglePlayApi{
 constructor(http,credentials,packageName,clock=()=>Date.now()){this.http=http;this.credentials=credentials;this.packageName=packageName;this.clock=clock;this.bearer=null;}
 async authorization(){if(this.bearer)return this.bearer;const c=this.credentials;requireThat(c?.client_email&&c.private_key&&this.packageName==='com.gltam.lowfire','iap_setup_required');const now=Math.floor(this.clock()/1000),jwt=signJwt({alg:'RS256',typ:'JWT'},{iss:c.client_email,scope:'https://www.googleapis.com/auth/androidpublisher',aud:'https://oauth2.googleapis.com/token',iat:now,exp:now+300},c.private_key);const reply=await this.http.post('https://oauth2.googleapis.com/token','grant_type=urn%3Aietf%3Aparams%3Aoauth%3Agrant-type%3Ajwt-bearer&assertion='+encodeURIComponent(jwt),{headers:{'Content-Type':'application/x-www-form-urlencoded'},timeout:4000});requireThat(typeof reply.data.access_token==='string','play_unavailable');return this.bearer=reply.data.access_token;}
 async options(){return {headers:{Authorization:'Bearer '+await this.authorization()},timeout:2500};}
 base(){return 'https://androidpublisher.googleapis.com/androidpublisher/v3/applications/'+encodeURIComponent(this.packageName);}
 async inspect(token){return (await this.http.get(this.base()+'/purchases/productsv2/tokens/'+encodeURIComponent(token),await this.options())).data;}
 async consume(product,token){await this.http.post(this.base()+'/purchases/products/'+encodeURIComponent(product)+'/tokens/'+encodeURIComponent(token)+':consume',null,await this.options());}
 async voided(pageToken){return (await this.http.get(this.base()+'/purchases/voidedpurchases?type=0&maxResults=1000'+(pageToken?'&token='+encodeURIComponent(pageToken):''),await this.options())).data;}
}
module.exports={GooglePlayApi};
