"""Original compact landscape landmarks, using the established Pillow polygon workflow."""
from pathlib import Path
from PIL import Image, ImageDraw
OUT=Path(__file__).resolve().parents[1]/'client/Assets/Wildfolk/Art'
S=3
def canvas():
 im=Image.new('RGBA',(280*S,240*S));return im,ImageDraw.Draw(im)
def poly(d,points,color,edge='#343e35'):
 d.polygon([(int(x*S),int(y*S)) for x,y in points],fill=color);d.line([(int(x*S),int(y*S)) for x,y in points+[points[0]]],fill=edge,width=2*S)
def shadow(d):d.ellipse((25*S,180*S,260*S,223*S),fill=(23,34,24,45))
def save(im,name):im.resize((280,240),Image.Resampling.LANCZOS).save(OUT/(name+'.png'))
im,d=canvas();shadow(d)
poly(d,[(30,190),(40,85),(78,36),(135,18),(213,50),(250,112),(250,192)],'#69715f')
poly(d,[(75,192),(81,117),(106,79),(156,69),(198,100),(216,190)],'#222c29')
poly(d,[(25,191),(76,169),(113,190),(231,180),(260,207),(45,215)],'#858974')
poly(d,[(44,92),(81,43),(118,65),(82,115)],'#818672');poly(d,[(170,42),(216,63),(236,114),(199,111)],'#505e52')
save(im,'world_cave')
im,d=canvas();shadow(d)
for x,y,w,h in [(35,80,30,115),(70,144,65,42),(165,55,38,140),(204,152,40,40)]:
 poly(d,[(x,y+h),(x,y),(x+w*.6,y-12),(x+w,y+5),(x+w,y+h)],'#8a8a75')
poly(d,[(65,185),(134,170),(164,190),(225,194),(230,215),(58,215)],'#626c58');save(im,'world_ruin')
im,d=canvas();shadow(d)
poly(d,[(50,132),(85,108),(191,108),(229,137),(214,202),(65,204)],'#838673')
poly(d,[(51,132),(88,109),(192,109),(228,137),(192,160),(86,160)],'#a2a18a')
poly(d,[(87,134),(108,124),(174,125),(194,137),(173,145),(108,145)],'#273c39')
for x in [81,194]:poly(d,[(x,144),(x,60),(x+10,54),(x+11,148)],'#6b6045')
poly(d,[(55,64),(140,32),(225,62),(207,79),(68,80)],'#6e6950');save(im,'world_well')
im,d=canvas();shadow(d)
for i in range(5):
 x=27+i*42;y=124-(i%3)*21
 poly(d,[(x,190),(x-8,y),(x+8,y-18),(x+40,y-8),(x+47,188)],['#777e6c','#8d907b','#656e62'][i%3])
poly(d,[(40,194),(214,168),(251,186),(232,217),(56,220)],'#9b987d');save(im,'world_quarry')
im,d=canvas();shadow(d)
for x,y in [(56,135),(124,106),(198,144)]:
 poly(d,[(x-13,y+57),(x-14,y),(x-5,y-15),(x+12,y-15),(x+21,y),(x+19,y+57)],'#929482')
poly(d,[(20,205),(84,191),(122,204),(193,187),(258,209),(254,218),(26,222)],'#5e7051');save(im,'world_graveyard')
im,d=canvas();shadow(d)
poly(d,[(32,185),(122,45),(220,192)],'#706e50');poly(d,[(122,45),(166,62),(252,189),(220,192)],'#4f6051');poly(d,[(89,185),(124,98),(151,185)],'#28362e')
for x in [37,228]:poly(d,[(x,180),(x+13,180),(x+13,216),(x,216)],'#776749')
save(im,'world_campsite')
im,d=canvas();shadow(d)
poly(d,[(31,200),(47,77),(101,43),(200,69),(253,202)],'#646f60')
poly(d,[(77,195),(86,90),(180,89),(214,194)],'#222c2a')
for x in [74,191]:poly(d,[(x,202),(x+4,86),(x+19,83),(x+13,204)],'#766649')
poly(d,[(72,78),(206,74),(211,94),(74,99)],'#8b7852')
poly(d,[(76,214),(203,200),(224,212),(90,226)],'#646a60');save(im,'world_mine')
print('Generated 7 original natural landmark sprites.')
