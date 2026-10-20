"""Original Lowfire medicine art, using the existing Pillow palette and primitives."""
from pathlib import Path
import argparse
import generate_visual_assets as art

def build(output):
    art.OUT=output;output.mkdir(parents=True,exist_ok=True)
    for patch in (False,True):
        im=art.canvas(180,180)
        for x,y in ((88,148),(58,155),(122,155)) if patch else ((88,148),):
            art.line(im,[(x,y),(x-5,y-42),(x+3,y-96)],'#5d6e49',5)
            for dx,dy in ((-27,-40),(22,-56),(-19,-76),(18,-94)):
                art.poly(im,[(x,y+dy+10),(x+dx,y+dy+2),(x+dx*1.2,y+dy-12),(x+dx*.3,y+dy-10)],'#7f9262',art.INK,3)
                art.line(im,[(x,y+dy+7),(x+dx*.9,y+dy-7)],'#b1b481',2)
            for dx,dy in ((-5,-103),(4,-110),(11,-102)):
                art.ellipse(im,(x+dx-4,y+dy-4,x+dx+4,y+dy+4),'#c5bf91',art.INK,2)
        art.save(im,'medicinal_herb_patch.png' if patch else 'medicinal_herb.png')
    im=art.canvas(180,180)
    art.poly(im,[(44,67),(104,45),(143,85),(113,129),(49,126),(34,102)],'#c2b594',art.INK,4)
    art.ellipse(im,(35,52,110,135),'#d3c7a7',art.INK,4)
    art.ellipse(im,(48,66,97,121),'#a99d7d',art.INK,3)
    art.ellipse(im,(61,78,86,109),'#786e59',art.INK,3)
    art.line(im,[(106,57),(135,89),(113,117)],'#e2d7ba',5)
    art.poly(im,[(115,96),(121,71),(137,75),(142,96),(130,109)],'#758561',art.INK,3)
    art.save(im,'field_dressing.png')

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--output',type=Path,default=Path(__file__).parents[1]/'client/Assets/Wildfolk/Art')
    build(parser.parse_args().output)
