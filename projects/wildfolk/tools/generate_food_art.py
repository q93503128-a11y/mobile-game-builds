"""Original Lowfire meal icons, using existing Pillow drawing primitives."""
from pathlib import Path
import generate_visual_assets as art
def build():
    art.OUT=Path(__file__).resolve().parents[1]/'client/Assets/Wildfolk/Art'
    for name,color in [('hunters_stew','#8e6c48'),('forager_soup','#70805a'),('trail_broth','#8b9a8d')]:
        im=art.canvas(180,180)
        if name=='trail_broth':
            art.ellipse(im,(113,76,158,132),'#576253',art.INK,4);art.ellipse(im,(122,84,149,122),'#00000000',art.INK,3)
            art.poly(im,[(34,68),(128,68),(118,145),(94,158),(59,152),(41,135)],'#6d7760',art.INK,4)
        else:art.poly(im,[(22,79),(154,79),(140,130),(118,152),(58,152),(33,124)],'#6a6750',art.INK,4)
        art.ellipse(im,(20,52,155,106) if name!='trail_broth' else (31,50,130,92),'#b1a582',art.INK,4)
        art.ellipse(im,(29,60,146,98) if name!='trail_broth' else (39,57,123,84),color,art.INK,2)
        if name=='hunters_stew':
            art.poly(im,[(52,69),(81,62),(97,78),(79,91),(49,84)],'#a37f61',art.INK,2)
            art.line(im,[(100,78),(131,44)],'#d2c5a2',8);art.ellipse(im,(124,34,138,48),'#d2c5a2',art.INK,2)
        elif name=='forager_soup':
            for x,y in [(55,77),(83,84),(114,73)]:art.ellipse(im,(x-12,y-8,x+12,y+5),'#a99a72',art.INK,2)
            art.line(im,[(117,83),(140,41)],'#465c3f',4);art.poly(im,[(128,63),(112,49),(122,43),(137,50)],'#8a9d6e',art.INK,2)
        else:
            art.poly(im,[(62,69),(77,60),(98,66),(109,60),(107,78),(97,73),(77,78)],'#c6ba8e',art.INK,2)
            art.line(im,[(56,123),(92,139),(111,121)],'#bdb28c',3)
        art.save(im,name+'.png')
if __name__=='__main__':build()
