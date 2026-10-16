"""Original early-metal sprites; deterministic Pillow polygons, no external assets."""
from pathlib import Path
import argparse
from PIL import Image, ImageDraw

S=3
INK='#293b32'
def art(size):return Image.new('RGBA',(size[0]*S,size[1]*S))
def poly(im,points,color,edge=INK,width=2):
 d=ImageDraw.Draw(im);p=[(int(x*S),int(y*S)) for x,y in points];d.polygon(p,fill=color)
 if edge:d.line(p+[p[0]],fill=edge,width=width*S,joint='curve')
def save(im,name,out):im.resize((im.width//S,im.height//S),Image.Resampling.LANCZOS).save(out/(name+'.png'))
def copper():
 im=art((190,154))
 # Low, broad iron-stained ledge with exposed winding copper seams.
 poly(im,[(8,109),(19,64),(48,39),(107,30),(159,52),(179,88),(173,123),(117,144),(42,137)],'#665b45')
 poly(im,[(19,64),(48,39),(107,30),(159,52),(138,92),(66,104)],'#98917a')
 poly(im,[(66,104),(138,92),(159,52),(179,88),(173,123),(117,144)],'#766d52')
 for p in [[(37,61),(61,52),(79,64),(115,53),(142,63),(135,75),(110,68),(76,81),(55,70),(39,75)],[(66,102),(81,93),(95,103),(100,125),(88,128),(80,110)],[(129,98),(148,86),(161,96),(155,108),(138,117)]]:poly(im,p,'#b17b4b','#4f533f',2)
 for p in [[(28,99),(41,89),(48,107),(41,119)],[(112,40),(121,44),(114,51)]]:poly(im,p,'#617261',None)
 return im
def tin():
 im=art((176,172))
 # Taller, split pale-grey outcrop; horizontal silvery bands, no copper nodules.
 poly(im,[(18,119),(27,66),(58,26),(92,20),(117,51),(149,58),(163,123),(140,153),(61,161)],'#717d79')
 poly(im,[(27,66),(58,26),(92,20),(100,71),(75,105),(18,119)],'#adb4a5')
 poly(im,[(100,71),(117,51),(149,58),(163,123),(140,153),(112,127)],'#8c9790')
 poly(im,[(75,105),(100,71),(112,127),(101,154),(61,161)],'#616f6a')
 for p in [[(34,66),(84,47),(86,56),(35,79)],[(26,100),(70,80),(67,92),(25,111)],[(119,90),(150,97),(151,109),(123,103)],[(127,122),(152,128),(146,138),(129,132)]]:poly(im,p,'#c4c5ae','#68756e',1)
 poly(im,[(88,26),(95,51),(82,82),(78,99),(85,119),(81,136),(90,156)],'#3e514a',None)
 return im
def coal():
 im=art((186,140))
 # A low pile of layered angular charcoal slabs, with broad dark matte facets.
 for p,c in [([(9,106),(32,68),(66,53),(91,91),(68,128),(29,127)],'#303a37'),([(65,107),(66,47),(105,24),(143,38),(155,87),(131,122),(87,132)],'#38413d'),([(113,116),(130,77),(164,69),(178,101),(163,126),(137,134)],'#29342f')]:
  poly(im,p,c,'#26352f',3)
 poly(im,[(66,47),(105,24),(143,38),(112,58)],'#59625a')
 poly(im,[(32,68),(66,53),(82,77),(48,89)],'#515d53')
 poly(im,[(130,77),(164,69),(169,84),(143,94)],'#515b53')
 for p in [[(77,72),(114,61),(132,66)],[(17,108),(48,99),(63,103)],[(90,100),(126,88),(142,91)],[(133,117),(156,105),(166,107)]]:poly(im,p,'#687165',None)
 return im
def lump(color,top):
 im=art((88,84));poly(im,[(12,53),(20,27),(49,14),(72,34),(75,61),(46,73)],color);poly(im,[(20,27),(49,14),(72,34),(43,47)],top);return im
def bar(color,top,alloy=False):
 im=art((94,76));poly(im,[(10,43),(25,24),(67,20),(84,38),(72,57),(24,62)],color);poly(im,[(25,24),(67,20),(84,38),(38,43),(10,43)],top);poly(im,[(38,43),(84,38),(72,57),(24,62)],color)
 if alloy:poly(im,[(40,44),(51,43),(47,59),(36,60)],top,None)
 return im
def main(out):
 out.mkdir(parents=True,exist_ok=True)
 for n,im in [('copper_ore_node',copper()),('tin_ore_node',tin()),('coal_node',coal()),('copper_ore',lump('#946946','#b18a5d')),('tin_ore',lump('#7b8a82','#b1b6a4')),('coal',lump('#303c35','#606b5d')),('copper_ingot',bar('#9e6942','#c19763')),('tin_ingot',bar('#82938b','#c0c6b1')),('bronze',bar('#827042','#b49e62',True)),('steel',bar('#5e7471','#a1b3a8',True))]:save(im,n,out)
if __name__=='__main__':
 p=argparse.ArgumentParser();p.add_argument('--out',type=Path,default=Path(__file__).resolve().parents[1]/'client/Assets/Wildfolk/Art');main(p.parse_args().out)
