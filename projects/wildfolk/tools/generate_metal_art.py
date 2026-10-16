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
 im=art((224,192))
 # A weathered vertical rock face with exposed copper seams.
 poly(im,[(14,145),(27,66),(63,29),(158,22),(201,61),(212,155),(175,179),(63,175)],'#615f4b')
 poly(im,[(27,66),(63,29),(158,22),(201,61),(158,80),(66,86)],'#a19b7f')
 poly(im,[(66,86),(158,80),(175,179),(63,175)],'#7d7960')
 poly(im,[(158,80),(201,61),(212,155),(175,179)],'#545e4b')
 for points in [[(39,106),(70,94),(101,103),(133,91),(162,98),(163,110),(135,105),(101,118),(70,108),(36,120)],[(68,139),(99,126),(128,138),(166,128),(168,141),(127,153),(98,142),(66,152)]]:poly(im,points,'#b1804f','#5c6049',2)
 poly(im,[(63,31),(57,78),(69,103),(59,128),(65,173)],'#4b5b47',None)
 poly(im,[(26,133),(38,127),(48,144),(40,158)],'#667853',None)
 return im
def tin():
 im=art((224,114))
 # Flat alluvial gravel fan and damp silt channels, rather than a mineable boulder.
 poly(im,[(9,64),(32,30),(79,17),(120,23),(167,17),(209,42),(218,70),(189,96),(129,101),(83,96),(39,100)],'#817e62','#4a5b4a',2)
 poly(im,[(20,65),(47,44),(84,47),(115,69),(158,67),(199,44),(211,63),(179,83),(132,87),(81,68),(46,65)],'#6b8077',None)
 for x,y,w,h in [(40,37,18,9),(73,27,15,10),(106,40,20,10),(155,32,19,11),(179,49,16,9),(52,83,22,10),(91,78,15,8),(151,86,18,8),(186,75,14,10)]:poly(im,[(x,y),(x+w*.65,y-2),(x+w,y+h*.5),(x+w*.65,y+h),(x-2,y+h*.7)],'#a7ad97','#596b5d',1)
 for x,y in [(64,48),(132,35),(120,82),(166,59),(89,55)]:poly(im,[(x,y),(x+6,y-2),(x+8,y+4),(x+1,y+5)],'#c4c9b0','#748276',1)
 return im
def coal():
 im=art((246,144))
 # Exposed black coal strata crossing an ordinary rocky ledge.
 poly(im,[(10,112),(25,55),(66,27),(156,20),(215,45),(236,100),(218,127),(167,134),(79,129)],'#626a5a')
 poly(im,[(25,55),(66,27),(156,20),(215,45),(171,65),(83,62)],'#8a8c73')
 poly(im,[(20,79),(81,68),(166,73),(222,60),(229,86),(167,100),(80,93),(15,103)],'#29352e','#35443a',2)
 poly(im,[(21,115),(78,103),(168,111),(231,94),(223,117),(168,129),(78,120)],'#364038',None)
 for p in [[(38,82),(80,77),(141,83),(179,80)],[(96,91),(158,94),(208,81)],[(48,114),(79,109),(131,115)]]:poly(im,p,'#546050',None)
 poly(im,[(162,27),(154,58),(163,76),(155,98),(163,126)],'#384b3c',None)
 return im
def lump(color,top):
 im=art((88,84));poly(im,[(12,53),(20,27),(49,14),(72,34),(75,61),(46,73)],color);poly(im,[(20,27),(49,14),(72,34),(43,47)],top);return im
def bar(color,top,alloy=False):
 im=art((94,76));poly(im,[(10,43),(25,24),(67,20),(84,38),(72,57),(24,62)],color);poly(im,[(25,24),(67,20),(84,38),(38,43),(10,43)],top);poly(im,[(38,43),(84,38),(72,57),(24,62)],color)
 if alloy:poly(im,[(40,44),(51,43),(47,59),(36,60)],top,None)
 return im
def pan():
 im=art((132,116));poly(im,[(17,59),(29,28),(59,15),(104,28),(120,62),(104,92),(61,104),(27,88)],'#927044');poly(im,[(27,58),(38,35),(63,27),(96,36),(107,61),(93,82),(61,91),(37,79)],'#b9905d');poly(im,[(37,59),(49,45),(75,43),(97,57),(85,75),(60,82),(43,73)],'#646f5a')
 for x in [40,47,54]:poly(im,[(x,41),(x+2,39),(x+12,48),(x+9,50)],'#d2b47b',None)
 return im
def blast():
 im=art((258,256));poly(im,[(30,221),(48,115),(65,85),(72,31),(151,25),(169,74),(196,94),(213,213),(185,239),(63,242)],'#4e5b50',INK,4);poly(im,[(72,31),(151,25),(157,50),(80,57)],'#879078');poly(im,[(53,110),(83,74),(167,68),(196,94),(180,124),(91,133)],'#7f8771');poly(im,[(90,137),(169,129),(181,208),(88,216)],'#303d34',INK,4);poly(im,[(105,152),(152,147),(159,196),(106,203)],'#614b33',None);poly(im,[(112,178),(123,157),(129,177),(140,161),(153,195),(111,199)],'#b58b4f',None)
 for y in [99,136,220]:poly(im,[(43,y+7),(83,y+19),(185,y+9),(204,y-8),(207,y+4),(185,y+20),(83,y+30),(40,y+19)],'#4a5d57')
 poly(im,[(188,153),(223,136),(245,154),(245,199),(218,218),(197,206)],'#755b3e');poly(im,[(220,144),(231,150),(231,201),(220,207)],'#ad8d50');return im
def steel_tool(kind):
 im=art((128,192));poly(im,[(57,53),(71,52),(72,176),(58,178)],'#795b3d');poly(im,[(60,87),(69,86),(69,108),(60,109)],'#a68b60',None)
 if kind=='axe':
  poly(im,[(26,28),(62,20),(78,32),(112,20),(119,43),(106,77),(80,75),(67,55),(28,57)],'#627d78',INK,3);poly(im,[(85,37),(111,27),(114,43),(102,70),(84,68)],'#bdc7b3',None)
 elif kind=='pickaxe':
  poly(im,[(8,58),(26,28),(56,16),(84,19),(112,38),(123,65),(101,48),(79,36),(56,34),(30,42)],'#718a80',INK,3);poly(im,[(23,37),(54,22),(82,26),(105,43),(84,36),(55,30),(29,44)],'#c0c9b6',None)
 else:
  im=art((128,192));poly(im,[(56,119),(73,119),(73,178),(55,178)],'#75563b');poly(im,[(64,9),(82,36),(78,120),(49,120),(46,36)],'#6c8780',INK,3);poly(im,[(64,14),(65,113),(53,114),(51,38)],'#c1cbbb',None);poly(im,[(31,117),(96,117),(96,129),(31,129)],'#7d8671');poly(im,[(59,138),(70,138),(70,167),(59,167)],'#9d8356',None)
 return im
def steel_armor(head,worn=False):
 im=art((150,150) if worn else (112,112));sx=150/112 if worn else 1
 def p(points,c):poly(im,[(x*sx,y*sx) for x,y in points],c,INK,2)
 if head:
  p([(22,66),(27,35),(42,21),(70,21),(87,35),(91,66),(78,75),(77,47),(35,47),(35,75)],'#617f78');p([(31,33),(45,25),(67,25),(81,35),(76,44),(37,44)],'#b6c5af');p([(52,20),(61,20),(63,45),(50,45)],'#8e9d8b')
 else:
  for points in [[(13,60),(20,31),(36,25),(42,45),(31,75),(18,82)],[(99,60),(92,31),(76,25),(70,45),(81,75),(94,82)],[(34,26),(44,16),(68,16),(78,26),(69,40),(43,40)]]:p(points,'#6d8980')
  for points in [[(18,57),(23,36),(32,31),(35,44),(27,65)],[(94,57),(89,36),(80,31),(77,44),(85,65)],[(40,26),(47,21),(65,21),(72,26),(66,34),(46,34)]]:p(points,'#b6c4ae')
 if not worn:p([(35,76),(78,76),(73,92),(40,92)],'#786c4c')
 return im
def main(out):
 out.mkdir(parents=True,exist_ok=True)
 for n,im in [('copper_ore_node',copper()),('tin_ore_node',tin()),('coal_node',coal()),('copper_ore',lump('#946946','#b18a5d')),('tin_ore',lump('#7b8a82','#b1b6a4')),('coal',lump('#303c35','#606b5d')),('copper_ingot',bar('#9e6942','#c19763')),('tin_ingot',bar('#82938b','#c0c6b1')),('bronze',bar('#827042','#b49e62',True)),('steel',bar('#5e7471','#a1b3a8',True))]:save(im,n,out)
 save(pan(),'prospecting_pan',out);save(blast(),'blast_furnace',out)
 for kind in ['axe','pickaxe','sword']:save(steel_tool(kind),'steel_'+kind,out)
 for head in [True,False]:
  for worn in [True,False]:save(steel_armor(head,worn),'steel_'+('head' if head else 'body')+('_worn' if worn else ''),out)
 hand=art((34,38));poly(hand,[(4,6),(30,6),(32,19),(27,25),(7,25),(2,19)],'#6d8980');poly(hand,[(6,7),(27,7),(27,12),(6,12)],'#b6c4ae');save(hand,'steel_hand_worn',out)
if __name__=='__main__':
 p=argparse.ArgumentParser();p.add_argument('--out',type=Path,default=Path(__file__).resolve().parents[1]/'client/Assets/Wildfolk/Art');main(p.parse_args().out)
