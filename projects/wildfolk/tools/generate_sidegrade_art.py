"""Original Lowfire processed bone braces, leather binding and retained iron point.
Separate compact inventory icon / long held shaft; no external assets.
"""
from pathlib import Path
from PIL import Image,ImageDraw
OUT=Path(__file__).resolve().parents[1]/'client/Assets/Wildfolk/Art'
def paint(name,height):
 s=3;im=Image.new('RGBA',(128*s,height*s));d=ImageDraw.Draw(im)
 def poly(points,color):
  p=[(int(x*s),int(y*s)) for x,y in points];d.polygon(p,fill=color);d.line(p+p[:1],fill='#292d25',width=2*s,joint='curve')
 def line(points,color,w=2):d.line([(int(x*s),int(y*s)) for x,y in points],fill=color,width=w*s,joint='curve')
 top=8;bottom=height-9;grip=int(height*.53)
 poly([(59,bottom),(59,45),(68,45),(68,bottom)],'#89714e');line([(62,bottom-5),(62,51)],'#b5a075')
 poly([(54,39),(64,top),(74,39),(65,55)],'#9ca49d');poly([(64,top+8),(64,46),(72,38)],'#69766f')
 # Narrow polished bone splints reinforce the original shaft, rather than a fantasy bone blade.
 poly([(52,49),(58,45),(62,grip+9),(57,grip+20)],'#b6af91');poly([(69,48),(74,51),(71,grip+18),(66,grip+10)],'#9d987e')
 for y in [56,65,grip-7,grip,grip+7]:line([(52,y),(74,y-4)],'#65513b',4);line([(53,y-1),(72,y-5)],'#ac916b',1)
 line([(59,77),(61,grip-17)],'#d1c6a1',1)
 im.resize((128,height),Image.Resampling.LANCZOS).save(OUT/(name+'.png'))
paint('hunters_spear',160)
paint('hunters_spear_held',353)
