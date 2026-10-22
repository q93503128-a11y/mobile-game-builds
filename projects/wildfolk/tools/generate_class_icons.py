from pathlib import Path
A=Path(__file__).resolve().parents[1]/"client/Assets/Wildfolk"
from PIL import Image,ImageDraw
import math
for n in ['survivor','warrior','hunter','medic','explorer','engineer']:
 im=Image.new('RGBA',(384,384));d=ImageDraw.Draw(im);ink=(222,211,166,255);dark=(40,59,45,255)
 d.ellipse((12,12,372,372),fill=dark,outline=(116,137,91,255),width=12)
 if n=='survivor':
  d.ellipse((102,64,270,302),fill=ink);d.line((176,302,214,110),fill=dark,width=15);d.line((190,225,133,158),fill=dark,width=12)
 if n=='warrior':d.polygon([(90,98),(192,65),(294,98),(276,228),(192,314),(108,228)],fill=ink);d.line((192,100,192,270),fill=dark,width=14)
 if n=='hunter':
  d.arc((62,53,291,332),-80,80,fill=ink,width=19);d.line((195,58,195,326),fill=ink,width=8);d.line((97,192,300,192),fill=ink,width=14);d.polygon([(305,192),(267,166),(267,218)],fill=ink)
 if n=='medic':d.rounded_rectangle((147,75,237,309),radius=10,fill=ink);d.rounded_rectangle((75,147,309,237),radius=10,fill=ink)
 if n=='explorer':
  d.ellipse((74,74,310,310),outline=ink,width=15);d.polygon([(230,96),(153,180),(154,284),(231,204)],fill=ink);d.ellipse((174,174,210,210),fill=dark)
 if n=='engineer':
  for j in range(8):
   a=j*math.pi/4;cx=192+108*math.cos(a);cy=192+108*math.sin(a);d.ellipse((cx-28,cy-28,cx+28,cy+28),fill=ink)
  d.ellipse((83,83,301,301),fill=ink);d.ellipse((137,137,247,247),fill=dark)
 im.resize((128,128),Image.Resampling.LANCZOS).save(A/f'Art/class_{n}.png')
