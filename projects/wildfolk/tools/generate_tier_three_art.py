"""Original code-authored III sprites; uses the existing Lowfire palette and silhouettes."""
from pathlib import Path
from PIL import Image, ImageDraw
import math
OUT=Path(__file__).resolve().parents[1]/'client/Assets/Wildfolk/Art'
INK='#292d25';WOOD='#89714e';EDGE='#b5a075';METAL='#858b80';BRONZE='#a28750';GEM='#8daead';SHADE='#526f77'
def asset(name,paint,size=(160,180)):
 im=Image.new('RGBA',(size[0]*3,size[1]*3));d=ImageDraw.Draw(im)
 def p(points,c,o=INK,w=2):
  q=[(int(x*3),int(y*3))for x,y in points];d.polygon(q,fill=c);d.line(q+q[:1],fill=o,width=w*3,joint='curve')
 def e(b,c,o=INK,w=2):d.ellipse(tuple(int(v*3)for v in b),fill=c,outline=o,width=w*3)
 def l(points,c,w=2):d.line([(int(x*3),int(y*3))for x,y in points],fill=c,width=w*3,joint='curve')
 paint(p,e,l);im.resize(size,Image.Resampling.LANCZOS).save(OUT/(name+'.png'))
def gem(p,e,l,crystal=False,node=False):
 if node:p([(15,136),(30,106),(77,93),(133,111),(146,146),(97,165),(40,161)],'#59615c')
 for x,y,k in ([(53,102,.7),(95,112,1),(124,130,.55)]if node else [(80,134,1)]):
  points=[(x-29*k,y-24*k),(x-17*k,y-66*k),(x+13*k,y-78*k),(x+31*k,y-28*k),(x,y+8*k)]
  p(points,GEM if not crystal else '#92a5b3');p([points[0],points[2],points[4]],SHADE);l([points[1],points[2],points[3]],'#c2d0c8',2)
for n,c,b in [('diamond',False,False),('crystal',True,False),('diamond_node',False,True),('crystal_node',True,True)]:asset(n,lambda p,e,l,c=c,b=b:gem(p,e,l,c,b))
def recolor(source,target):
 im=Image.open(OUT/(source+'.png')).convert('RGBA');px=im.load()
 for y in range(im.height):
  for x in range(im.width):
   r,g,b,a=px[x,y]
   if a and abs(r-g)<30 and max(r,g,b)-min(r,g,b)<55 and r>65:
    v=(r+g+b)/3;px[x,y]=(int(v*.83),min(255,int(v*1.03)),min(255,int(v*1.09)),a)
 im.save(OUT/(target+'.png'))
for suffix in ['axe','pickaxe','sword','head','body']:
 recolor('steel_'+suffix,'diamond_'+suffix)
for suffix in ['head_worn','body_worn','hand_worn']:
 if(OUT/('steel_'+suffix+'.png')).exists():recolor('steel_'+suffix,'diamond_'+suffix)
def spear(p,e,l,diamond=False):
 p([(75,174),(75,56),(85,56),(85,174)],WOOD);p([(57,55),(80,6),(103,55),(81,77)],GEM if diamond else METAL);p([(80,12),(81,67),(99,52)],SHADE if diamond else '#5f6b65')
 for y in range(74,92,6):l([(74,y),(87,y-3)],EDGE,3)
asset('steel_spear',spear);asset('diamond_spear',lambda p,e,l:spear(p,e,l,True))
def mechanical(p,e,l):
 q=[]
 for i in range(32):
  a=i*math.pi/16;r=48 if i%4<2 else 37;q.append((80+math.cos(a)*r,89+math.sin(a)*r))
 p(q,BRONZE);e((58,67,102,111),'#434b45');e((66,75,94,103),METAL);l([(39,61),(118,121)],'#c3b687',3)
asset('mechanical_part',mechanical)
def telescope(p,e,l):
 p([(29,123),(111,43),(139,69),(54,148)],BRONZE);p([(43,111),(91,64),(117,90),(69,137)],'#8a8c7d');p([(18,123),(32,109),(66,142),(51,158)],'#424c49');e((108,42,143,76),GEM);l([(42,121),(107,57)],EDGE,4)
asset('telescope',telescope)
def beacon(p,e,l):
 e((25,137,139,167),'#00000033',o='#00000000');p([(43,143),(53,126),(110,126),(120,143),(106,156),(56,156)],'#5d675e');p([(72,135),(72,59),(89,59),(89,135)],METAL)
 p([(58,59),(69,22),(92,22),(104,59),(81,82)],GEM);l([(81,25),(81,72)],'#d1d5bd',3);p([(48,84),(63,72),(104,86),(91,98)],BRONZE)
asset('signal_beacon',beacon)
recolor('lantern','crystal_lantern')
# Add a restrained crystal lens to the existing lantern housing.
im=Image.open(OUT/'crystal_lantern.png').convert('RGBA');d=ImageDraw.Draw(im);w,h=im.size
points=[(w*.43,h*.43),(w*.52,h*.32),(w*.64,h*.44),(w*.55,h*.64),(w*.43,h*.55)]
d.polygon(points,fill=GEM,outline=INK);d.line([points[1],points[3]],fill='#c7d3c6',width=2);im.save(OUT/'crystal_lantern.png')
def bench(p,e,l,turret=False):
 p([(25,134),(25,61),(139,61),(139,134),(125,144),(125,85),(41,85),(41,143)],WOOD);p([(12,60),(36,39),(141,44),(152,66),(130,89),(24,83)],'#9c8861')
 for x in [35,67,100,130]:l([(x,49),(x-5,76)],'#68583e',2)
 p([(49,45),(74,23),(101,33),(94,58),(67,65)],METAL);p([(81,28),(92,9),(105,15),(99,40)],BRONZE)
 if turret:p([(113,51),(112,21),(120,21),(122,54)],BRONZE);l([(104,18),(129,17)],METAL,6)
 else:p([(21,57),(32,27),(47,36),(46,60)],GEM)
asset('workbench_iii',bench);asset('turret_workbench',lambda p,e,l:bench(p,e,l,True))
def turret_base(p,e,l):
 e((14,128,146,163),'#00000033',o='#00000000');p([(37,128),(48,75),(111,75),(128,128),(108,148),(47,148)],WOOD);p([(29,125),(130,125),(131,143),(28,143)],METAL);e((50,71,111,126),BRONZE)
asset('turret_base',turret_base)
def turret(p,e,l,repeater=False):
 p([(66,157),(65,33),(91,33),(94,157)],WOOD);p([(67,112),(95,112),(95,140),(67,140)],'#514936')
 p([(9,57),(31,31),(58,28),(80,39),(103,28),(132,31),(151,57),(144,69),(115,52),(80,56),(46,52),(17,69)],METAL if repeater else WOOD)
 l([(15,64),(80,100),(146,64)],'#b4ac83',3);p([(73,103),(73,14),(87,14),(87,103)],METAL)
 if repeater:p([(95,73),(129,73),(129,123),(95,123)],BRONZE);l([(102,82),(122,82)],EDGE,3)
asset('ballista',turret);asset('repeater',lambda p,e,l:turret(p,e,l,True))
recolor('crossbow_bolt','ballista_bolt')
def module(p,e,l,kind):
 p([(34,50),(111,42),(131,63),(129,139),(43,147),(26,126)],WOOD if kind==2 else METAL);p([(34,50),(111,42),(131,63),(51,75)],EDGE if kind==2 else '#a4aa96')
 if kind==0:
  for y in range(86,127,14):l([(52,y),(111,y-8)],BRONZE,7)
 elif kind==1:e((53,76,108,131),BRONZE);e((65,88,96,120),GEM);l([(79,82),(79,125)],SHADE,2)
 elif kind==2:l([(43,89),(119,80)],METAL,8);l([(51,133),(119,126)],METAL,7)
 elif kind==3:mechanical(p,e,l)
 else:p([(55,126),(58,80),(101,76),(107,118),(81,139)],'#58675e');l([(64,91),(97,90)],BRONZE,5)
for i,name in enumerate(['auto_loader','auto_aim','ammo_box','rotation_gear','reinforced_frame']):asset(name,lambda p,e,l,i=i:module(p,e,l,i))
def industrial(p,e,l):
 p([(24,66),(123,53),(142,82),(138,140),(37,154),(19,128)],WOOD);p([(24,66),(123,53),(142,82),(40,96)],'#ac9366')
 for x in [42,112]:p([(x,69),(x+13,67),(x+14,144),(x,147)],METAL)
 p([(74,105),(93,102),(94,123),(76,126)],BRONZE)
asset('industrial_container',industrial)
def site(p,e,l,kind):
 if kind<2:
  for x,y in [(40,98),(85,77),(115,109)]:p([(x-25,y+35),(x-17,y-10),(x+8,y-36),(x+24,y+12),(x+3,y+40)],GEM if kind==0 else '#8a9e9b')
 else:
  p([(13,143),(27,78),(41,77),(36,140)],WOOD);p([(122,142),(119,78),(134,78),(145,145)],WOOD);p([(20,73),(129,70),(140,88),(20,94)],'#655a43')
  if kind==2:
   for x in [57,95]:l([(x,100),(x-11,157)],METAL,5)
   for y in [119,138,153]:l([(41,y),(109,y)],WOOD,5)
  else:industrial(p,e,l);mechanical(p,e,l)
for i,name in enumerate(['crystal_pocket','diamond_pocket','deep_mine','abandoned_workshop']):asset(name,lambda p,e,l,i=i:site(p,e,l,i))
print('Tier III original sprites generated')
