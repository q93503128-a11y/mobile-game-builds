"""Original segmented dead warrior king, royal tomb and reward art. Pillow only."""
from pathlib import Path
from PIL import Image, ImageDraw
import math
A=Path(__file__).resolve().parents[1]/'client/Assets/Wildfolk/Art'
S=3
INK='#29362f';STONE='#60665b';EDGE='#9a9b7e';BONE='#c6c4a5';CLOTH='#394b43';TEAL='#7eaa96';GOLD='#b09b61'
class Pen:
 def __init__(self,im):self.d=ImageDraw.Draw(im)
 def poly(self,p,c,outline=INK,w=3):self.d.polygon([(int(x*S),int(y*S)) for x,y in p],fill=c,outline=outline,width=w*S)
 def line(self,p,c=INK,w=3):self.d.line([(int(x*S),int(y*S)) for x,y in p],fill=c,width=w*S,joint='curve')
 def box(self,b,c,r=3):self.d.rounded_rectangle(tuple(int(v*S) for v in b),radius=r*S,fill=c,outline=INK,width=3*S)
 def ellipse(self,b,c):self.d.ellipse(tuple(int(v*S) for v in b),fill=c,outline=INK,width=3*S)
def make(name,fn,size=(256,256)):
 im=Image.new('RGBA',(128*S,128*S));fn(Pen(im));im.resize(size,Image.Resampling.LANCZOS).save(A/(name+'.png'))
def head(p):
 p.poly([(37,22),(76,17),(94,36),(91,70),(80,95),(55,104),(37,83),(29,55)],BONE)
 p.poly([(38,43),(54,44),(56,56),(40,58)],INK);p.poly([(73,43),(87,38),(85,54),(73,57)],INK)
 p.line([(41,49),(51,49)],TEAL,3);p.line([(76,48),(84,45)],TEAL,3)
 p.poly([(64,59),(57,73),(69,73)],INK);p.line([(45,82),(77,79)],INK,4)
 for x in (52,61,70):p.line([(x,81),(x+1,90)],STONE,2)
 p.line([(61,25),(57,38),(65,43)],STONE,2)
def crown(p,broken=False):
 p.poly([(17,84),(14,40),(37,57),(47,20),(66,53),(83,12),(94,50),(115,34),(108,88)],GOLD)
 p.poly([(20,84),(108,85),(103,103),(25,103)],STONE)
 p.line([(25,89),(101,90)],GOLD,4)
 p.poly([(62,62),(70,70),(63,79),(56,70)],TEAL)
 p.poly([(80,17),(84,11),(89,41),(77,34)],'#00000000') if broken else p.line([(85,15),(82,43),(91,53)],INK,4)
 p.line([(43,57),(46,77)],INK,3)
def torso(p):
 p.poly([(12,26),(39,17),(63,26),(87,17),(117,31),(107,59),(93,55),(85,108),(36,111),(25,54),(13,57)],STONE)
 p.poly([(14,29),(40,22),(42,46),(18,51)],'#858977');p.poly([(88,23),(113,34),(104,54),(83,48)],'#444f47')
 p.poly([(44,38),(65,44),(85,37),(80,83),(63,100),(43,84)],'#7e8371')
 p.line([(63,45),(61,90)],INK,3);p.line([(41,89),(85,87)],GOLD,5)
 p.poly([(49,53),(60,59),(54,72),(43,63)],INK);p.poly([(72,53),(82,59),(72,70),(65,62)],INK)
def cloak(p):
 p.poly([(34,9),(90,14),(108,61),(119,119),(99,106),(90,124),(71,114),(59,127),(41,115),(27,122),(12,101),(24,51)],CLOTH)
 p.poly([(34,12),(46,21),(32,91),(22,110),(19,91)],'#536459');p.poly([(89,20),(99,66),(105,110),(88,102)],'#283c35')
 p.line([(47,29),(38,71),(41,108)],'#697465',2);p.line([(78,30),(83,80),(72,111)],'#263b32',3)
 p.poly([(83,92),(89,104),(84,123),(76,116)],'#00000000')
def arm(p):
 p.poly([(49,12),(78,18),(87,49),(78,66),(83,100),(66,117),(44,103),(48,68),(39,46)],STONE)
 p.line([(52,57),(75,59)],GOLD,4);p.poly([(48,95),(77,94),(73,112),(56,113)],BONE)
def legs(p):
 p.poly([(27,17),(57,17),(58,68),(52,100),(58,114),(20,114),(20,99),(29,64)],STONE)
 p.poly([(69,19),(100,17),(96,68),(105,100),(107,114),(68,114),(65,99),(72,64)],'#4a574d')
 p.line([(32,45),(48,47)],EDGE,3);p.line([(77,49),(92,47)],EDGE,3)
def sword(p):
 p.poly([(63,3),(78,24),(74,82),(64,94),(51,80),(51,25)],'#929b8c')
 p.poly([(63,7),(65,81),(72,77),(75,25)],'#c0c1a4');p.line([(62,25),(61,69)],STONE,2)
 p.poly([(34,87),(61,82),(91,89),(89,99),(63,94),(38,99)],GOLD)
 p.box((57,96,69,119),CLOTH,2);p.ellipse((57,117,70,127),GOLD)
def tomb(p):
 p.poly([(6,81),(59,43),(122,73),(121,100),(70,124),(8,105)],'#515a50')
 p.poly([(6,81),(62,49),(122,73),(69,106)],'#929580')
 p.poly([(20,56),(60,28),(106,47),(105,83),(66,106),(23,85)],STONE)
 p.poly([(23,61),(63,36),(104,52),(65,80)],'#777f6e')
 p.poly([(29,74),(65,91),(99,72),(99,84),(65,104),(29,85)],'#48594f')
 p.line([(65,91),(65,103)],INK,3)
 for x,y in [(9,35),(110,42)]:p.box((x,y,x+9,y+38),STONE,2);p.poly([(x-3,y),(x+5,y-7),(x+13,y),(x+11,y+8),(x,y+8)],EDGE)
def lid(p):
 p.poly([(19,56),(60,29),(107,48),(106,61),(66,87),(20,69)],'#858a75')
 p.line([(27,57),(63,36),(98,50),(65,72),(27,57)],EDGE,2)
 p.poly([(49,49),(58,44),(63,49),(71,43),(74,51),(81,48),(77,60),(59,63)],GOLD)
 p.line([(54,63),(75,59)],INK,3)
def core(p):
 p.poly([(35,21),(80,17),(110,46),(104,91),(73,117),(26,102),(15,59)],STONE)
 p.poly([(51,31),(78,35),(91,64),(75,91),(45,87),(32,59)],TEAL)
 p.poly([(59,39),(71,44),(77,65),(63,83),(49,73),(46,55)],'#c0c5a0')
 p.line([(36,23),(45,39),(31,54)],INK,3);p.line([(101,86),(84,81),(79,101)],INK,3)
def skull(p):
 p.poly([(30,25),(60,15),(92,26),(101,53),(89,78),(78,84),(78,109),(44,109),(42,84),(28,71),(23,48)],BONE)
 p.ellipse((33,43,54,66),INK);p.ellipse((67,43,89,66),INK);p.poly([(60,66),(52,82),(67,82)],INK)
 p.line([(48,90),(74,90)],INK,3);p.line([(56,88),(56,104)],INK,3);p.line([(66,88),(66,104)],INK,3)
def crack(p):
 p.line([(64,6),(55,24),(71,38),(52,58),(67,78),(58,104),(66,123)],'#6d6d57',4)
 p.line([(70,38),(90,48),(97,69)],'#919379',3);p.line([(52,58),(29,70),(24,90)],'#919379',3)
make('king_head',head);make('king_crown',crown);make('king_crown_broken',lambda p:crown(p,True));make('king_torso',torso);make('king_cloak',cloak);make('king_arm',arm);make('king_legs',legs);make('king_sword',sword)
make('royal_tomb',tomb,(512,384));make('royal_tomb_lid',lid,(512,384));make('boss_core_t1',core);make('undead_kings_crown',crown);make('skull_mark',skull);make('grave_crack',crack)
def ash(p):
 for i in range(18):
  x=64+math.cos(i*2.4)*(16+i*2);y=64+math.sin(i*2.4)*(16+i*2)
  p.poly([(x,y-3),(x+3,y),(x,y+4),(x-2,y)],'#6c7566',w=1)
make('king_ash',ash)
print('15 original boss/tomb/reward sprite sources generated')
