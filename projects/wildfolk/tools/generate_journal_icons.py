"""Original small UI marks; existing Lowfire journal/font/class/item art is reused."""
from pathlib import Path
from PIL import Image, ImageDraw
import math
A=Path(__file__).resolve().parents[1]/'client/Assets/Wildfolk/Art'
def make(name,draw):
 im=Image.new('RGBA',(384,384));draw(ImageDraw.Draw(im));im.resize((128,128),Image.Resampling.LANCZOS).save(A/(name+'.png'))
ink=(71,70,49,255);paper=(234,222,187,255);ember=(137,88,56,255)
def node(d):
 pts=[(192+171*math.cos(i*math.pi/18)*(1+.012*math.sin(i*3)),192+171*math.sin(i*math.pi/18)) for i in range(36)]
 d.polygon(pts,fill=paper);d.line(pts+[pts[0]],fill=ink,width=9);d.ellipse((42,42,342,342),outline=(178,156,109,255),width=4)
make('journal_node',node)
make('journal_focus',lambda d:d.ellipse((10,10,374,374),outline=ember,width=14))
def seal(d):
 d.ellipse((26,26,358,358),fill=ember,outline=ink,width=8);d.line([(109,193),(165,246),(279,129)],fill=paper,width=29)
make('journal_seal',seal)
def lock(d):
 d.arc((104,45,280,268),180,360,fill=ink,width=25);d.rounded_rectangle((78,171,306,333),radius=21,fill=paper,outline=ink,width=15);d.ellipse((176,214,208,246),fill=ink);d.line((192,240,192,278),fill=ink,width=12)
make('journal_lock',lock)
def coin(d):
 d.ellipse((36,36,348,348),fill=(192,148,65,255),outline=ink,width=12);d.ellipse((70,70,314,314),outline=paper,width=9);d.line((192,111,192,273),fill=ink,width=18);d.line((139,142,245,142),fill=ink,width=13);d.line((139,242,245,242),fill=ink,width=13)
make('journal_gold',coin)
def gem(d):
 d.polygon([(108,75),(276,75),(343,158),(192,330),(41,158)],fill=(111,151,145,255),outline=ink,width=10);d.line([(41,158),(343,158),(276,75),(224,158),(192,330),(160,158),(108,75)],fill=paper,width=9)
make('journal_gem',gem)
