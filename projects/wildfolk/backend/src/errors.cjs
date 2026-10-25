"use strict";
class Rejected extends Error {constructor(code){super(code);this.code=code;}}
const requireThat=(ok,code)=>{if(!ok)throw new Rejected(code);};
module.exports={Rejected,requireThat};
