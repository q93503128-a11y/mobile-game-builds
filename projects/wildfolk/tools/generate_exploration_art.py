"""Original Lowfire exploration sprites, using the project's established Pillow method."""
from pathlib import Path
from PIL import Image, ImageDraw
OUT=Path(__file__).resolve().parents[1]/'client/Assets/Wildfolk/Art'
def save(name,draw,size=(128,160)):
    im=Image.new('RGBA',(size[0]*3,size[1]*3));d=ImageDraw.Draw(im)
    draw(d,3);im.resize(size,Image.Resampling.LANCZOS).save(OUT/(name+'.png'))
def torch(d,s,placed=False):
    def poly(p,c):d.polygon([(x*s,y*s) for x,y in p],fill=c)
    if placed:
        d.ellipse((30*s,139*s,101*s,155*s),fill=(20,26,23,65))
        poly([(50,148),(57,80),(73,80),(79,148)],'#34392b')
    poly([(55,140),(53,62),(75,62),(70,140)],'#292a21')
    poly([(59,135),(58,66),(69,66),(66,135)],'#796044')
    poly([(59,130),(59,70),(63,70),(63,130)],'#a38a5b')
    poly([(48,72),(45,48),(80,48),(78,72)],'#39382b')
    for y in (53,60,67):poly([(48,y),(78,y-2),(78,y+3),(48,y+5)],'#8a7853')
    poly([(55,52),(43,39),(48,24),(57,30),(65,6),(79,29),(77,45),(68,54)],'#b9682d')
    poly([(58,48),(51,38),(58,24),(63,29),(67,17),(73,36),(68,48)],'#e5ae52')
    poly([(59,46),(59,37),(64,28),(69,40),(66,47)],'#f2d790')
save('torch',lambda d,s:torch(d,s))
save('torch_placed',lambda d,s:torch(d,s,True))
def compass(d,s):
    d.ellipse((9*s,9*s,119*s,119*s),fill='#262e29',outline='#a6925e',width=4*s)
    d.ellipse((18*s,18*s,110*s,110*s),fill='#38473d',outline='#6b7053',width=2*s)
    for x,y in ((64,24),(64,104),(24,64),(104,64)):
        d.ellipse(((x-2)*s,(y-2)*s,(x+2)*s,(y+2)*s),fill='#bcb68f')
    d.polygon([(64*s,30*s),(79*s,76*s),(64*s,68*s),(49*s,76*s)],fill='#c2a36a')
    d.polygon([(64*s,99*s),(76*s,67*s),(64*s,72*s),(52*s,67*s)],fill='#76867b')
    d.ellipse((58*s,58*s,70*s,70*s),fill='#d0c194')
save('compass',compass,(128,128))
save('bearing_arrow',lambda d,s:d.polygon([(32*s,3*s),(57*s,60*s),(32*s,47*s),(7*s,60*s)],fill='#bca86c'),(64,64))
print('4 original exploration sprites generated.')
