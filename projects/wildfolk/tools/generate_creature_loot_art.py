"""Original Lowfire creature/loot sprites; shares the canonical Pillow art primitives."""
from pathlib import Path
import argparse
import math
from PIL import ImageDraw
import generate_visual_assets as art


def build(output):
    art.OUT = output
    output.mkdir(parents=True, exist_ok=True)
    canvas, poly, ellipse, line, save = art.canvas, art.poly, art.ellipse, art.line, art.save
    ink = art.INK
    # Bat: narrow furry body, long ears and articulated membrane wings.
    im = canvas(120, 160)
    poly(im, [(40,60),(32,9),(49,22),(60,48),(72,19),(91,7),(79,65)], '#74695b', ink, 3)
    ellipse(im, (28,46,90,131), '#655c50', ink, 3)
    poly(im, [(40,57),(60,46),(79,58),(74,82),(60,95),(44,80)], '#93836c', ink, 2)
    for x in (46,70): ellipse(im,(x,65,x+6,70),'#242e28')
    line(im,[(47,111),(43,140),(36,145)], '#544c40', 5)
    line(im,[(73,111),(78,140),(85,144)], '#544c40', 5)
    save(im,'bat.png')
    im=canvas(160,125)
    poly(im,[(150,56),(109,15),(46,7),(15,25),(42,52),(34,95),(76,77),(99,115),(119,78),(151,87)], '#73645e',ink,3)
    for p in ([(150,60),(46,8),(41,51)],[(149,62),(76,77),(35,95)],[(149,65),(101,114)]):line(im,p,'#ac9580',3)
    save(im,'bat_wing.png')
    # Rock camouflage reuses the project's own stone silhouette, with discoverable tiny eyes/cracks.
    im=art.rock(1)
    line(im,[(83,85),(94,92),(100,108)],'#58695b',2)
    ellipse(im,(85,94,90,98),'#bbac6f');ellipse(im,(101,94,106,98),'#bbac6f')
    save(im,'stone_crawler_rest.png')
    im=canvas(200,180)
    poly(im,[(31,113),(38,61),(76,31),(143,42),(172,88),(158,134),(109,154),(55,145)],'#899287',ink,4)
    poly(im,[(40,66),(80,39),(137,49),(119,88),(70,107)],'#b3b6a2',ink,2)
    poly(im,[(70,107),(119,88),(162,91),(153,128),(105,147),(61,139)],'#6d7a70',ink,2)
    for x in (72,111):ellipse(im,(x,118,x+10,124),'#d0b76d',ink,1)
    save(im,'stone_crawler.png')
    im=canvas(130,135)
    for y in (26,64,103):poly(im,[(118,y),(70,y-12),(21,y+4),(6,y+29),(52,y+13),(100,y+20)],'#6b7467',ink,3)
    save(im,'crawler_legs.png')
    # Crystal crawler: long segmented carapace and asymmetric crystal crown, not a rock recolor.
    im=canvas(180,210)
    for y,w in ((62,49),(101,60),(142,48)):
        ellipse(im,(90-w,y-26,90+w,y+43),'#5e796d',ink,3)
        line(im,[(52,y+23),(87,y+32),(129,y+19)],'#b4c1a2',3)
    for pts in ([(66,84),(50,30),(68,6),(87,72)],[(89,119),(103,20),(125,40),(116,124)],[(112,140),(144,65),(160,85),(143,154)]):
        poly(im,pts,'#9bb8ac',ink,3)
    poly(im,[(67,159),(91,151),(117,163),(105,196),(78,195)],'#4b6259',ink,3)
    for x in (78,101):ellipse(im,(x,175,x+6,181),'#dcc782')
    save(im,'crystal_crawler.png')
    # Leech: ringed soft body, flattened tail, visible pale suction mouth.
    im=canvas(110,190)
    poly(im,[(44,14),(63,18),(82,70),(86,120),(73,168),(55,181),(26,158),(21,116),(29,61)],'#535f41',ink,3)
    for y in range(48,155,13):line(im,[(28+(y%3),y),(55,y+7),(80,y)],'#889068',3)
    ellipse(im,(31,134,76,177),'#ac9b73',ink,3);ellipse(im,(43,144,66,167),'#3d4937',ink,2)
    save(im,'bog_leech.png')
    # Wolf: pointed muzzle, ear triangles, long tail and narrow waist.
    im=canvas(150,215)
    poly(im,[(59,138),(48,172),(64,207),(87,199),(91,161)],'#687063',ink,3)
    ellipse(im,(39,61,110,172),'#7e8878',ink,3)
    poly(im,[(42,77),(24,20),(48,29),(69,47),(96,27),(125,17),(108,81),(88,114),(65,113)],'#909985',ink,3)
    poly(im,[(61,69),(87,68),(96,92),(78,132),(61,94)],'#c2c4aa',ink,2)
    poly(im,[(66,112),(87,112),(78,128)],'#3b463b')
    for x in (45,96):ellipse(im,(x,74,x+8,79),'#413c2d')
    for x in (33,101):poly(im,[(x,114),(x+11,121),(x+12,168),(x+1,175)],'#677464',ink,3)
    save(im,'wolf.png')
    # Goat: short thick hooked horns, split hooves, broad muzzle and shoulder cape.
    im=canvas(160,215)
    ellipse(im,(32,66,125,172),'#c0b99a',ink,3)
    poly(im,[(36,77),(25,45),(42,31),(74,44),(115,31),(138,50),(120,83),(97,119),(62,117)],'#a6a589',ink,3)
    for sign in (-1,1):
        pts=[(80+sign*x,y) for x,y in [(26,56),(41,30),(39,13),(25,9),(18,22),(27,16),(29,30),(17,57)]]
        poly(im,pts,'#685e48',ink,2)
    poly(im,[(59,85),(99,85),(105,116),(81,133),(57,115)],'#ded1ae',ink,2)
    line(im,[(76,108),(89,108)],'#4b4d3e',3)
    for x in (30,113):
        poly(im,[(x,126),(x+15,126),(x+13,183),(x,186)],'#b3ac8b',ink,2)
        poly(im,[(x-3,179),(x+17,179),(x+17,194),(x+7,198),(x-4,192)],'#5a5949',ink,2);line(im,[(x+7,180),(x+7,194)],ink,2)
    save(im,'mountain_goat.png')
    # Crow: fan tail, swept feather wings and short ivory beak.
    im=canvas(130,180)
    poly(im,[(41,111),(30,171),(50,158),(64,177),(76,158),(96,168),(86,108)],'#3d4a43',ink,3)
    ellipse(im,(28,45,100,137),'#4a5850',ink,3)
    ellipse(im,(35,18,95,79),'#536258',ink,3)
    poly(im,[(48,40),(65,11),(83,42),(66,62)],'#aaa58a',ink,2)
    for x in (40,83):ellipse(im,(x,45,x+5,50),'#1d3029')
    save(im,'crow.png')
    im=canvas(130,145)
    poly(im,[(121,22),(77,17),(42,38),(10,114),(32,102),(31,136),(55,114),(64,140),(90,111),(124,61)],'#536259',ink,3)
    for i in range(4):line(im,[(112-i*12,34),(27+i*15,115+i%2*12)],'#859080',2)
    save(im,'crow_wing.png')
    # Giant boar: wide shoulder shield, visible tusks, heavy bristles and small eyes.
    im=canvas(220,230)
    ellipse(im,(25,41,197,202),'#735d43',ink,4)
    for x in (67,91,117,141):poly(im,[(x-9,55),(x+2,15),(x+14,58)],'#544c3a',ink,2)
    poly(im,[(39,100),(16,67),(56,70),(106,101),(171,66),(207,69),(186,113),(154,164),(67,166)],'#8c7252',ink,4)
    ellipse(im,(60,120,161,207),'#a28560',ink,3)
    for sign in (-1,1):poly(im,[(110+sign*40,150),(110+sign*61,132),(110+sign*70,97),(110+sign*74,146),(110+sign*48,178)],'#ded0aa',ink,3)
    for x in (77,131):ellipse(im,(x,177,x+12,188),'#514a36')
    for x in (51,154):ellipse(im,(x,111,x+9,117),'#302f27')
    save(im,'giant_boar.png')
    # New item vocabulary. Only the raw materials are separate; no unused trophies.
    for name in ('bone','bone_arrow','bone_trap','bone_broth','chitin','carapace_vest','barbed_bolt','medicinal_gland','healing_salve','restorative_broth','trail_ration'):
        im=canvas(128,128)
        if name=='bone':
            line(im,[(33,90),(91,34)],'#ded1aa',19)
            for x,y in ((27,86),(36,98),(85,26),(98,36)):ellipse(im,(x-11,y-11,x+11,y+11),'#ded1aa',ink,2)
        elif name in ('bone_arrow','barbed_bolt'):
            line(im,[(25,103),(95,30)],'#92744e',9)
            poly(im,[(68,25),(111,13),(101,55),(85,41)],'#cdc8a4' if name=='bone_arrow' else '#879b85',ink,3)
            poly(im,[(20,79),(40,91),(45,116),(26,108)],'#aeb295',ink,2)
        elif name=='bone_trap':
            poly(im,[(12,90),(87,68),(115,91),(38,113)],'#785e40',ink,3)
            for x in (30,60,85):poly(im,[(x,90),(x+6,37),(x+16,84)],'#d2c6a0',ink,3)
        elif name in ('chitin','carapace_vest'):
            poly(im,[(29,17),(62,9),(99,22),(113,76),(89,111),(43,115),(14,79)],'#78886e',ink,3)
            for y in (43,65,87):line(im,[(26,y),(63,y+12),(103,y)],'#bdc1a0',4)
            if name=='carapace_vest':poly(im,[(47,12),(63,41),(82,14),(76,6),(53,5)],'#293b32')
        elif name=='medicinal_gland':
            ellipse(im,(27,29,102,103),'#8c8d5b',ink,3);ellipse(im,(43,47,79,84),'#b4b27a',ink,2);line(im,[(36,38),(21,16)],'#696f48',10)
        elif name=='healing_salve':
            poly(im,[(26,31),(102,31),(96,109),(33,109)],'#b5b28e',ink,3)
            poly(im,[(21,22),(105,22),(109,42),(18,42)],'#697b5e',ink,3)
            line(im,[(65,55),(65,92)],'#536b51',9);line(im,[(46,74),(84,74)],'#536b51',9)
        elif name=='trail_ration':
            poly(im,[(18,31),(92,20),(111,86),(79,112),(32,103)],'#8d7854',ink,3)
            for y in (43,61,82):line(im,[(31,y),(88,y-8)],'#b2a174',8)
            line(im,[(25,48),(102,91)],'#465b41',6)
        else:
            ellipse(im,(10,25,119,112),'#7b6d4c',ink,3)
            ellipse(im,(15,29,114,91),'#b5a976',ink,3)
            ellipse(im,(25,37,105,80),'#677d50' if name=='restorative_broth' else '#a19a70')
            for x,y in ((47,57),(73,47),(80,67)):ellipse(im,(x-6,y-4,x+6,y+4),'#c5b68b')
        save(im,name+'.png')
    im=canvas(160,160)
    poly(im,[(33,22),(52,11),(78,37),(109,12),(129,27),(127,135),(81,149),(33,136)],'#71846c',ink,3)
    for y in (59,85,111):line(im,[(36,y),(80,y+13),(124,y)],'#a9b298',6)
    save(im,'carapace_vest_worn.png')
    im=canvas(100,100)
    ImageDraw.Draw(im).ellipse((5*art.S,5*art.S,95*art.S,95*art.S),outline='#c6b780',width=3*art.S)
    save(im,'wildlife_tell.png')
    for opened in (False,True):
        im=canvas(160,160)
        poly(im,[(24,65),(116,52),(144,81),(137,128),(49,145),(20,110)],'#6b6546',ink,4)
        poly(im,[(29,72),(115,60),(132,85),(47,100)],'#302f25',ink,3)
        poly(im,[(26,60 if opened else 67),(40,19 if opened else 40),(127,9 if opened else 28),(140,53 if opened else 66),(48,96)],'#8a8055',ink,4)
        line(im,[(53,91),(53,135)],'#b2ac83',8);line(im,[(120,85),(118,128)],'#b2ac83',8)
        if not opened:poly(im,[(78,78),(96,75),(97,98),(79,102)],'#c5b477',ink,2)
        save(im,'camp_supplies_open.png' if opened else 'camp_supplies.png')


if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('output',type=Path)
    build(parser.parse_args().output)
