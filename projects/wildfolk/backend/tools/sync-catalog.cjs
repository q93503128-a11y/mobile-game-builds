"use strict";
const fs=require('node:fs'),path=require('node:path');
const p=path.join(__dirname,'../../client/Assets/Wildfolk/Resources/Classes.json'),raw=JSON.parse(fs.readFileSync(p,'utf8'));
const data={Milestones:raw.Milestones,Classes:raw.Classes.map(c=>({Id:c.Id,Currency:c.Currency,Price:c.Price,Nodes:c.Nodes.map(n=>({Id:n.Id,Prerequisite:n.Prerequisite}))}))};
fs.writeFileSync(path.join(__dirname,'../src/class-catalog.json'),JSON.stringify(data,null,2)+'\n');
