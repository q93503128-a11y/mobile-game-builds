"""Original small stylized Lowfire sprites. No downloaded assets or generated imagery."""
from pathlib import Path
from PIL import Image, ImageDraw
import math
OUT=Path(__file__).resolve().parents[1]/'client/Assets/Wildfolk/Art'
INK='#292d25';WOOD='#89714e';EDGE='#b5a075';METAL='#858b80';BRONZE='#a28750';LEAF='#556d42'
def asset(name,paint,size=(128,160)):
 im=Image.new('RGBA',(size[0]*3,size[1]*3));d=ImageDraw.Draw(im)
 def polygon(p,c,o=INK,w=2):d.polygon([(int(x*3),int(y*3)) for x,y in p],fill=c);d.line([(int(x*3),int(y*3)) for x,y in p+p[:1]],fill=o,width=w*3,joint='curve')
 def ellipse(b,c,o=INK,w=2):d.ellipse(tuple(int(v*3) for v in b),fill=c,outline=o,width=w*3)
 def line(p,c,w=2):d.line([(int(x*3),int(y*3)) for x,y in p],fill=c,width=w*3,joint='curve')
 paint(polygon,ellipse,line);im.resize(size,Image.Resampling.LANCZOS).save(OUT/(name+'.png'))
def spear(p,e,l,rank):
 p([(60,150),(60,48),(68,48),(68,150)],WOOD);l([(62,143),(63,50)],EDGE,2)
 p([(48,47),(64,5),(80,47),(65,64)],WOOD if rank==0 else '#9a9e8b' if rank==1 else '#9ca49d');p([(64,12),(64,55),(77,45)],'#716c52' if rank==0 else '#69766f')
 for y in range(57,77,6):l([(59,y),(70,y-3)],'#b0a27b',3)
for i,n in enumerate(['wood','stone','iron']):asset(n+'_spear',lambda p,e,l,i=i:spear(p,e,l,i))
def bow(p,e,l,reinforced=False):
 p([(34,12),(52,19),(73,40),(82,77),(73,112),(51,141),(34,148),(44,131),(59,105),(66,78),(58,47),(44,27)],WOOD)
 l([(36,16),(62,43),(74,77),(63,111),(36,145)],EDGE,3);l([(35,15),(33,145)],'#c0b898',2)
 for y in range(65,92,5):l([(63,y),(82,y-3)],'#4b4130',3)
 if reinforced:
  for y in [35,109]:p([(52,y),(66,y+6),(71,y+19),(58,y+13)],BRONZE)
asset('bow',bow);asset('reinforced_bow',lambda p,e,l:bow(p,e,l,True))
def arrow(p,e,l,metal=False,bolt=False):
 y=45 if bolt else 22;p([(61,145),(61,y+18),(67,y+18),(67,145)],WOOD);p([(53,y+20),(64,y),(75,y+20),(64,y+29)],BRONZE if metal else '#9a9c8b');l([(64,y+2),(64,134)],EDGE,2)
 p([(61,120),(49,108),(49,133),(61,144)],'#b7b19a');p([(68,120),(78,108),(78,133),(68,144)],'#827f6b')
for n,m,b in [('arrow',False,False),('bronze_arrow',True,False),('crossbow_bolt',False,True),('steel_bolt',True,True)]:asset(n,lambda p,e,l,m=m,b=b:arrow(p,e,l,m,b))
def crossbow(p,e,l):
 p([(57,145),(55,42),(73,42),(71,145)],WOOD);p([(14,43),(29,26),(49,20),(64,28),(79,20),(100,26),(115,43),(109,51),(88,41),(64,40),(40,41),(20,51)],BRONZE);l([(19,47),(64,75),(110,47)],'#c2b991',2);p([(55,103),(46,112),(49,131),(61,131),(64,116)],'#443e2d');p([(59,87),(59,20),(68,20),(68,87)],'#697468');l([(64,19),(64,80)],'#c6c5a8',2)
asset('crossbow',crossbow)
def fiber(p,e,l):
 for i in range(6):
  x=30+i*13;l([(64,135),(x,67),(x-7,28+i%2*13)],'#414d32',7);l([(64,133),(x+1,66),(x-5,28+i%2*13)],'#9b9b68',3)
 p([(42,105),(78,96),(88,113),(49,122)],'#746249');l([(47,110),(83,104)],'#b6a16e',3)
asset('plant_fiber',fiber)
def berry(p,e,l,bush=False):
 if bush:
  for x,y in [(34,82),(63,56),(94,80),(65,102)]:p([(x-28,y),(x-14,y-29),(x+16,y-22),(x+30,y+4),(x+8,y+23),(x-22,y+20)],LEAF)
  l([(64,139),(65,92)],WOOD,8)
 for x,y in ([(36,73),(75,51),(97,82),(59,106),(40,99)] if bush else [(44,70),(75,55),(85,91),(50,103)]):
  e((x-12,y-12,x+12,y+12),'#8a4447');e((x-6,y-7,x,y-1),'#b16a63',o='#b16a63',w=1)
asset('berry',lambda p,e,l:berry(p,e,l));asset('berry_bush',lambda p,e,l:berry(p,e,l,True),(144,160))
def mushroom(p,e,l,source=False,roast=False):
 def one(x,y,s):
  p([(x-7*s,y),(x-8*s,y+34*s),(x+8*s,y+37*s),(x+7*s,y)],'#aeaa84' if not roast else '#93815b')
  p([(x-30*s,y),(x-23*s,y-19*s),(x-8*s,y-29*s),(x+10*s,y-26*s),(x+27*s,y-11*s),(x+30*s,y+2*s),(x+13*s,y+10*s),(x-15*s,y+10*s)],'#9a6f4e' if not roast else '#70503a')
  l([(x-21*s,y-5*s),(x+19*s,y-3*s)],'#c0a077',3)
 one(60,78,1)
 if source:one(98,103,.55);one(29,110,.4)
asset('mushroom',mushroom);asset('mushroom_patch',lambda p,e,l:mushroom(p,e,l,True));asset('roasted_mushroom',lambda p,e,l:mushroom(p,e,l,False,True))
def fish(p,e,l,cooked=False):
 p([(23,90),(8,69),(10,111)],'#746e50' if cooked else '#688a87');p([(20,91),(36,58),(70,47),(99,62),(118,86),(103,106),(71,121),(38,112)],'#99805b' if cooked else '#8fa49a');p([(40,93),(69,66),(106,84),(78,106)],'#baa77d' if cooked else '#b4bd9e');e((96,72,102,78),INK,w=1);l([(85,65),(78,91),(88,108)],'#655b43' if cooked else '#536b65',3)
 if cooked:
  for x in [47,61,75]:l([(x,73),(x-5,99)],'#735339',3)
asset('raw_fish',fish);asset('cooked_fish',lambda p,e,l:fish(p,e,l,True))
def rod(p,e,l):
 p([(36,145),(79,17),(85,17),(45,149)],WOOD);l([(41,141),(82,21)],EDGE,2);e((29,106,58,132),'#676f5d');e((36,112,50,126),'#aba079');l([(83,20),(102,67),(95,126)],'#b5b397',1);e((90,121,100,137),'#a6a68b');p([(91,124),(99,124),(99,130),(91,130)],'#9b5645')
asset('fishing_rod',rod)
asset('fishing_bobber',lambda p,e,l:(e((45,45,83,113),'#bab491'),p([(46,62),(82,62),(82,85),(46,85)],'#9b5146')))
asset('fishing_bite',lambda p,e,l:(p([(47,25),(80,25),(74,88),(53,88)],'#d4b46b'),e((51,104,75,129),'#d4b46b')))
def pot(p,e,l,food=False,color='#807143'):
 e((12,55,116,130),'#3a423a');p([(18,75),(28,124),(45,142),(85,142),(105,119),(111,76)],'#525c51');e((16,46,113,98),INK);e((25,55,104,88),color if food else '#6b7668');l([(18,68),(5,63),(4,86),(20,91)],METAL,5);l([(112,68),(123,63),(125,85),(108,91)],METAL,5)
 if food:
  for x,y,c in [(44,68,'#9a7654'),(70,70,'#b0a576'),(84,61,'#7a8b4e'),(52,81,'#9a5149')]:e((x-5,y-3,x+5,y+4),c,w=1)
 else:l([(39,38),(52,31),(79,31),(92,39)],METAL,5)
asset('cooking_pot',pot)
for n,c in [('mushroom_stew','#8e8054'),('meat_stew','#846448'),('fish_stew','#7c8460'),('mixed_stew','#a0844f')]:asset(n,lambda p,e,l,c=c:pot(p,e,l,True,c))
def skin(p,e,l):
 p([(48,36),(48,17),(78,17),(78,39),(99,64),(103,107),(89,135),(56,143),(31,129),(25,98),(31,60)],'#8f704a');p([(45,55),(68,43),(91,69),(92,109),(74,128),(47,121),(36,94)],'#a98b5c');p([(45,21),(81,21),(81,36),(45,36)],'#4c4530');l([(39,49),(29,64),(33,113),(57,139),(89,133)],'#c3aa79',2);l([(37,57),(99,70)],'#504b32',3)
asset('waterskin',skin)
def lantern(p,e,l,large=False):
 l([(38,40),(39,20),(64,9),(89,21),(89,40)],INK,6);l([(40,38),(41,22),(64,13),(86,23),(87,39)],BRONZE,3)
 p([(25,46),(39,32),(88,32),(103,47),(97,58),(31,58)],BRONZE);p([(33,58),(95,58),(100,124),(29,124)],'#686a45');p([(43,60),(85,60),(88,116),(40,116)],'#bfac6a');p([(57,111),(50,94),(61,73),(66,86),(77,102),(71,114)],'#dfb85c');p([(25,123),(102,123),(109,139),(19,139)],BRONZE);l([(36,58),(34,124)],'#655537',5);l([(92,58),(94,124)],'#655537',5)
 if large:p([(42,142),(85,142),(90,158),(38,158)],'#62685b')
asset('lantern',lantern);asset('brazier',lambda p,e,l:lantern(p,e,l,True),(128,176))
def barrier(p,e,l,stone=False,gate=False,reinforced=False,fence=False):
 c='#828779' if stone else WOOD;edge='#acaf9b' if stone else EDGE
 for x in [14,231]:p([(x-9,36),(x,22),(x+10,36),(x+10,137),(x-9,137)],'#636b60' if stone else '#776140')
 if fence:
  for y in [62,99]:p([(21,y),(224,y),(224,y+18),(21,y+18)],c);l([(24,y+2),(218,y+2)],edge,2)
  for x in range(34,220,31):p([(x-5,40),(x+5,40),(x+5,132),(x-5,132)],c)
 else:
  for x in range(25,220,27):p([(x,36),(x+25,36),(x+25,134),(x,134)],c)
  for y in [53,110]:p([(22,y),(224,y),(224,y+12),(22,y+12)],BRONZE if reinforced else '#5c604e' if stone else '#645134')
 if gate:
  l([(25,125),(219,42)],'#5b604d' if stone else '#ad9564',8);e((199,78,211,91),BRONZE)
 if reinforced:
  for x in [25,83,143,212]:
   for y in [59,116]:e((x-3,y-3,x+3,y+3),'#c5b779',w=1)
for n in ['wood_fence','wood_fence_gate','wood_gate','stone_door','stone_fence','stone_fence_gate','stone_gate','reinforced_wall','reinforced_door','reinforced_gate']:
 asset(n,lambda p,e,l,n=n:barrier(p,e,l,'stone' in n,'gate' in n or 'door' in n,'reinforced' in n,'fence' in n),(248,160))
def trap(p,e,l):
 p([(10,102),(64,63),(118,102),(65,145)],'#6d5b3d');l([(18,102),(66,135),(111,101)],EDGE,3)
 for x,y in [(36,100),(65,119),(92,101),(63,86)]:p([(x-8,y),(x,y-42),(x+9,y)],'#969b8c');p([(x,y-39),(x,y-3),(x+8,y)],'#69776b')
asset('spike_trap',trap)
print('Original content sprites exported.')

for n in ['wood_fence_gate','wood_gate','stone_door','stone_fence_gate','stone_gate','reinforced_door','reinforced_gate']:
 im=Image.open(OUT/f'{n}.png').convert('RGBA');leaf=im.copy();frame=im.copy()
 leaf.paste((0,0,0,0),(0,0,25,160));leaf.paste((0,0,0,0),(224,0,248,160))
 frame.paste((0,0,0,0),(25,0,224,160));leaf.save(OUT/f'{n}_leaf.png');frame.save(OUT/f'{n}_frame.png')
