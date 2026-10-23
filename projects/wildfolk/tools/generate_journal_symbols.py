"""Original Lowfire UI ink stamps. Pillow only; no imported reference artwork.

Six class identities, six common skills and 54 lightweight node compositions share
one vocabulary. Node variants combine an object with an effect cue, not 54 paintings.
Run with the project's Pillow runtime. Output: one 1408x768 transparent atlas.
"""
from pathlib import Path
import json
import math
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / 'client/Assets/Wildfolk/Art'
INK = (49, 63, 48, 255)
S = 3
CELL = 128
CLASSES = ['survivor', 'warrior', 'hunter', 'medic', 'explorer', 'engineer']
COMMON = ['vitality', 'endurance', 'recovery', 'stride', 'metabolism', 'field_care']
# Explicit semantic mapping follows Classes.json order; changing gameplay is unnecessary.
NODES = {
    'survivor': [('heart','plus'), ('lungs','return'), ('fire','clock'),
                 ('lungs','heart'), ('medicine','heart'), ('boot','shield'),
                 ('bowl','plus'), ('bowl','clock'), ('camp','leaf')],
    'warrior': [('shield','lungs'), ('boot','arrow'), ('shield','impact'),
                ('sword','clock'), ('sword','return'), ('sword','lungs'),
                ('shield','arrow'), ('lungs','release'), ('shield','wide')],
    'hunter': [('paw','clock'), ('paw','eye'), ('boot','paw'),
               ('bow','long'), ('bow','target'), ('arrow','heart'),
               ('spear','long'), ('spear','arrow'), ('spear','paw')],
    'medic': [('medicine','plus'), ('medicine','clock'), ('bandage','fast'),
              ('rescue','clock'), ('rescue','heart'), ('rescue','shield'),
              ('heart','hold'), ('clock','plus'), ('bandage','heart')],
    'explorer': [('telescope','long'), ('map','eye'), ('flag','clock'),
                 ('survey','wide'), ('survey','clock'), ('survey','return'),
                 ('signal','wide'), ('signal','clock'), ('flag','directions')],
    'engineer': [('wrench','clock'), ('wall','plus'), ('wrench','return'),
                 ('turret','clock'), ('turret','drop'), ('turret','target'),
                 ('wall','wrench'), ('trap','wrench'), ('turret','wrench')],
}


class Pen:
    def __init__(self, im, scale=1, offset=(0, 0)):
        self.d = ImageDraw.Draw(im)
        self.scale, self.offset = scale, offset

    def p(self, p):
        return tuple(round((v*self.scale+self.offset[i])*S) for i,v in enumerate(p))

    def box(self, box):
        return self.p(box[:2]) + self.p(box[2:])

    def line(self, pts, width=7, fill=INK):
        self.d.line([self.p(p) for p in pts], fill=fill,
                    width=max(1, round(width*self.scale*S)), joint='curve')
        r = width*self.scale*S/2
        for pt in (pts[0], pts[-1]):
            x,y = self.p(pt)
            self.d.ellipse((x-r,y-r,x+r,y+r), fill=fill)

    def poly(self, pts, fill=INK):
        self.d.polygon([self.p(p) for p in pts], fill=fill)

    def ellipse(self, box, fill=None, width=7):
        self.d.ellipse(self.box(box), fill=fill, outline=INK if fill is None else None,
                       width=max(1, round(width*self.scale*S)))

    def rect(self, box, radius=6, fill=None, width=7):
        self.d.rounded_rectangle(self.box(box), radius=round(radius*self.scale*S),
                                fill=fill, outline=INK if fill is None else None,
                                width=max(1, round(width*self.scale*S)))

    def arc(self, box, start, end, width=7):
        self.d.arc(self.box(box), start, end, fill=INK, width=round(width*self.scale*S))

    def curve(self, pts, width=7, fill=INK):
        a,b,c,d = pts
        self.line([tuple((1-t)**3*a[j]+3*(1-t)**2*t*b[j]+3*(1-t)*t*t*c[j]+t**3*d[j]
                         for j in (0,1)) for t in [i/32 for i in range(33)]], width, fill)


def mark(p, name):
    """All motifs use the same 7 px rounded pen and generous transparent cuts."""
    if name in ('fire','camp'):
        p.poly([(62,12),(56,37),(39,28),(43,49),(27,65),(31,84),(47,98),
                (73,99),(96,84),(99,65),(82,41),(80,61),(69,46)])
        p.poly([(61,53),(50,77),(54,89),(67,91),(78,82),(71,67)], (0,0,0,0))
        p.line([(24,109),(97,103)],8);p.line([(25,101),(97,111)],8)
        if name == 'camp': p.line([(14,57),(13,24),(40,24)],6)
    elif name == 'heart':
        p.curve([(63,35),(30,1),(7,42),(31,66)],10)
        p.curve([(63,35),(94,1),(122,39),(98,66)],10)
        p.line([(31,66),(64,103),(98,66)],10)
    elif name == 'lungs':
        p.line([(63,16),(63,46),(48,57)],7);p.line([(63,46),(79,57)],7)
        p.curve([(47,36),(19,39),(9,90),(29,100)],9)
        p.curve([(29,100),(50,110),(52,91),(49,65)],9)
        p.curve([(78,36),(105,39),(119,90),(99,100)],9)
        p.curve([(99,100),(77,110),(74,91),(78,65)],9)
    elif name == 'return':
        p.arc((20,22,107,109),30,300,9);p.poly([(105,14),(108,48),(76,40)])
    elif name == 'boot':
        p.poly([(38,17),(81,17),(77,69),(104,79),(111,96),(103,106),
                (23,106),(20,94),(34,80)])
        p.line([(44,36),(68,40)],6,(0,0,0,0));p.line([(42,51),(67,55)],6,(0,0,0,0))
        p.line([(35,91),(94,91)],6,(0,0,0,0))
    elif name == 'bowl':
        p.line([(19,59),(108,59)],8)
        p.curve([(23,65),(28,115),(96,115),(104,65)],9)
        p.line([(49,112),(79,112)],7)
        p.curve([(50,42),(38,31),(60,27),(50,15)],6)
        p.curve([(77,42),(65,31),(86,27),(77,15)],6)
    elif name == 'bandage':
        p.poly([(24,27),(40,17),(109,87),(92,108),(77,109),(18,49)])
        p.poly([(42,51),(53,40),(87,75),(76,87)],(0,0,0,0))
        for x,y in [(32,36),(40,44),(84,91),(93,85)]:p.ellipse((x,y,x+6,y+6),fill=(0,0,0,0))
    elif name == 'shield':
        p.poly([(21,26),(63,13),(107,26),(102,69),(90,89),(63,110),(37,90),(25,69)])
        p.poly([(33,35),(59,27),(59,88),(46,77),(38,62)],(0,0,0,0))
    elif name == 'sword':
        p.poly([(83,16),(108,13),(105,39),(57,86),(42,72)])
        p.line([(30,62),(67,99)],9);p.line([(43,85),(23,106)],10)
        p.line([(86,33),(56,64)],5,(0,0,0,0))
    elif name == 'paw':
        for b in [(20,39,40,66),(39,17,59,46),(65,16,85,45),(87,37,107,64)]:p.ellipse(b,INK)
        p.curve([(30,94),(20,81),(48,59),(61,61)],13)
        p.curve([(61,61),(77,57),(106,81),(97,94)],13)
        p.curve([(30,94),(40,112),(50,92),(64,96)],12)
        p.curve([(64,96),(79,91),(83,111),(97,94)],12)
    elif name == 'bow':
        p.curve([(41,15),(113,26),(113,101),(41,113)],9)
        p.line([(41,15),(61,64),(41,113)],5)
        p.line([(19,64),(102,64)],7);p.poly([(115,64),(96,53),(96,75)])
    elif name in ('spear','arrow'):
        p.line([(26,105),(88,43)],8)
        p.poly([(112,14),(101,52),(77,29)])
        if name=='spear':p.line([(72,40),(87,55)],9)
        else:
            p.line([(25,83),(25,105),(45,105)],7)
            p.line([(14,94),(14,116),(34,116)],6)
    elif name == 'medicine':
        p.rect((20,40,108,108),9,width=8);p.arc((40,16,88,64),180,360,8)
        p.rect((54,54,74,95),2,INK);p.rect((43,66,86,84),2,INK)
    elif name == 'rescue':
        p.ellipse((49,15,75,41),INK)
        p.line([(62,50),(62,91),(42,111)],10);p.line([(62,91),(82,110)],10)
        p.line([(29,65),(61,53),(99,40)],9)
        p.line([(15,94),(31,78),(42,82)],7)
    elif name == 'telescope':
        p.poly([(21,65),(90,19),(110,49),(42,95)])
        p.line([(81,26),(101,56)],6,(0,0,0,0));p.line([(31,59),(51,88)],6,(0,0,0,0))
        p.line([(64,83),(51,111)],7);p.line([(64,83),(84,111)],7)
    elif name == 'compass':
        p.ellipse((22,22,106,106),width=7)
        p.line([(64,11),(64,28)],6);p.line([(64,100),(64,117)],6)
        p.poly([(84,38),(73,75),(42,94),(53,57)])
        p.poly([(60,62),(72,50),(64,72)],(0,0,0,0))
    elif name == 'map':
        p.line([(16,26),(48,17),(80,29),(110,19),(110,101),(80,111),(48,99),(16,109),(16,26)],7)
        p.line([(48,17),(48,99)],5);p.line([(80,29),(80,111)],5)
        p.line([(25,80),(36,61),(54,65),(67,42),(96,46)],6)
    elif name == 'flag':
        p.line([(31,15),(31,114)],8)
        p.poly([(34,19),(102,19),(84,43),(102,65),(34,65)])
        p.line([(53,31),(76,31)],5,(0,0,0,0))
    elif name == 'survey':
        p.line([(63,45),(63,112)],8);p.line([(41,112),(86,112)],7)
        p.arc((18,16,109,94),195,345,8);p.arc((33,32,95,82),193,347,6)
        p.poly([(64,25),(76,54),(64,72),(52,54)])
    elif name == 'signal':
        p.poly([(30,77),(52,62),(69,80),(56,104),(38,103),(23,91)])
        p.line([(58,63),(81,45)],8);p.arc((62,22,108,70),245,350,7)
        p.arc((57,9,122,82),245,350,6)
    elif name == 'wrench':
        p.poly([(74,14),(62,32),(68,50),(86,56),(104,43),(109,65),(94,80),
                (73,78),(41,110),(24,109),(16,100),(18,86),(52,52),(50,33),(59,18)])
        p.ellipse((25,89,36,100),fill=(0,0,0,0))
    elif name == 'wall':
        p.rect((17,27,111,106),3,width=8)
        for y in (53,79):p.line([(21,y),(107,y)],6)
        for x,y in [(49,29),(83,29),(33,55),(71,55),(49,81),(88,81)]:p.line([(x,y),(x,y+23)],5)
    elif name == 'turret':
        p.poly([(18,30),(83,22),(111,31),(110,50),(44,58),(30,51)])
        p.line([(59,57),(59,93)],10);p.line([(29,110),(59,88),(95,110)],9)
        p.line([(88,29),(88,48)],6,(0,0,0,0))
    elif name == 'trap':
        p.poly([(13,99),(115,99),(112,112),(16,112)])
        for x in (26,53,82):p.poly([(x,91),(x+5,34),(x+17,91)])
    elif name == 'clock':
        p.ellipse((21,23,108,110),width=8);p.line([(64,40),(64,69),(84,80)],7)
        p.line([(52,12),(77,12)],7);p.line([(64,12),(64,23)],6)
    elif name == 'drop':
        p.poly([(63,12),(37,48),(24,76),(30,97),(49,110),(77,111),(99,96),(104,75),(90,47)])
        p.curve([(40,72),(37,88),(46,94),(54,95)],7,(0,0,0,0))
    elif name == 'leaf':
        p.curve([(22,102),(12,29),(63,18),(108,17)],10)
        p.curve([(108,17),(107,72),(77,107),(22,102)],10)
        p.line([(26,97),(85,43)],8);p.line([(53,72),(49,49)],6)
    else:
        raise ValueError(name)


def accent(p, name):
    # A large, open cue; no letters, rank digits or decorative microtexture.
    if name in ('clock','heart','lungs','shield','paw','wrench','drop','leaf','return'):
        mark(p, name)
    elif name=='plus':p.line([(64,22),(64,106)],21);p.line([(22,64),(106,64)],21)
    elif name in ('arrow','long'):
        p.line([(13,64),(100,64)],13);p.poly([(112,64),(76,34),(76,94)])
        if name=='long':p.line([(17,35),(17,94)],10)
    elif name=='eye':
        p.line([(11,64),(40,36),(86,36),(117,64),(86,92),(40,92),(11,64)],11)
        p.ellipse((49,48,80,80),INK)
    elif name=='target':
        p.ellipse((28,28,100,100),width=11);p.line([(64,12),(64,43)],10)
        p.line([(64,85),(64,116)],10);p.line([(12,64),(43,64)],10);p.line([(85,64),(116,64)],10)
    elif name=='wide':
        p.arc((13,18,115,112),205,335,13);p.line([(19,77),(10,45),(43,52)],10)
        p.line([(109,77),(118,45),(85,52)],10)
    elif name=='impact':
        for a in (-65,-35,0):
            t=math.radians(a);p.line([(24+20*math.cos(t),99+20*math.sin(t)),(24+94*math.cos(t),99+94*math.sin(t))],12)
    elif name in ('release','fast'):
        p.line([(14,36),(45,64),(14,92)],13);p.line([(66,36),(97,64),(66,92)],13)
    elif name=='hold':
        p.line([(38,21),(38,108)],19);p.line([(89,21),(89,108)],19)
    elif name=='directions':
        p.line([(64,100),(64,33)],12);p.line([(64,65),(21,39)],12);p.line([(64,65),(108,39)],12)
        p.poly([(64,11),(45,40),(83,40)]);p.poly([(9,29),(18,60),(37,28)]);p.poly([(119,29),(91,28),(110,60)])
    else:raise ValueError(name)


def symbol(base, cue=None):
    im=Image.new('RGBA',(CELL*S,CELL*S))
    mark(Pen(im,.79,(1,1)) if cue else Pen(im),base)
    if cue:
        # Clear overlap to transparent paper; keeps the two silhouettes separate.
        ImageDraw.Draw(im).ellipse((76*S,76*S,125*S,125*S),fill=(0,0,0,0))
        accent(Pen(im,.34,(78,78)),cue)
    return im.resize((CELL,CELL),Image.Resampling.LANCZOS)


def generate():
    identities=['fire','shield','paw','medicine','compass','wrench']
    common=['heart','lungs','return','boot','bowl','bandage']
    entries=[('class_'+c,base,None) for c,base in zip(CLASSES,identities)]
    entries += [('skill_'+c,base,None) for c,base in zip(COMMON,common)]
    catalog=json.loads((ROOT/'client/Assets/Wildfolk/Resources/Classes.json').read_text(encoding='utf-8-sig'))
    for c in catalog['Classes']:
        entries += [('node_'+c['Id']+'_'+n['Id'],base,cue)
                    for n,(base,cue) in zip(c['Nodes'],NODES[c['Id']],strict=True)]
    atlas=Image.new('RGBA',(CELL*11,CELL*6))
    for i,(name,base,cue) in enumerate(entries):atlas.paste(symbol(base,cue),(i%11*CELL,i//11*CELL))
    ART.mkdir(parents=True,exist_ok=True)
    atlas.save(ART/'journal_symbols.png')
    print(f'{len(entries)} original symbols, one atlas {atlas.size}; class/common/node = 6/6/54')


if __name__=='__main__':generate()
