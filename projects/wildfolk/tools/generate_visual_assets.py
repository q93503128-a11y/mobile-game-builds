"""Original, deterministic Wildfolk sprite artwork. Requires Pillow; no external art."""
from pathlib import Path
import argparse
import math
import random
from PIL import Image, ImageDraw, ImageFilter

S = 3
INK = '#293b32'

def canvas(w, h):
    return Image.new('RGBA', (w*S, h*S))

def poly(im, pts, fill, outline=None, width=2):
    d = ImageDraw.Draw(im)
    pts = [(round(x*S), round(y*S)) for x,y in pts]
    d.polygon(pts, fill=fill)
    if outline:
        d.line(pts+[pts[0]], fill=outline, width=width*S, joint='curve')

def ellipse(im, box, fill, outline=None, width=2):
    ImageDraw.Draw(im).ellipse(tuple(round(x*S) for x in box), fill, outline, width*S)

def line(im, pts, fill, width=2):
    ImageDraw.Draw(im).line([(round(x*S), round(y*S)) for x,y in pts], fill, width*S, joint='curve')

def save(im, name):
    im.resize((im.width//S, im.height//S), Image.Resampling.LANCZOS).save(OUT/name)

def tree(seed, warm=False):
    rng=random.Random(seed)
    im=canvas(320,360)
    poly(im,[(134,185),(181,180),(185,290),(212,313),(176,306),(163,333),(145,307),(111,318),(135,285)], '#66523a', '#344334',3)
    poly(im,[(142,213),(156,199),(157,295),(146,307)],'#92724b')
    line(im,[(176,230),(169,276),(180,300)],'#443d2e',3)
    line(im,[(146,274),(123,250)],'#443d2e',3)
    pal=['#344b37','#3d5a3b','#496841','#547348','#617e4c','#6b8550']
    if warm: pal=['#3d4e35','#4c613a','#5c7342','#6c8048','#788a50','#85935a']
    clusters=[(99,175,61),(211,164,62),(165,192,71),(68,122,56),(244,119,54),(170,120,71),(106,83,64),(207,77,58),(152,51,55)]
    if seed%2:
        clusters=[(220,175,56),(114,182,74),(53,127,46),(243,112,52),(169,134,65),(86,86,57),(201,62,62),(142,67,69)]
    for cx,cy,r in clusters:
        pts=[]
        for j in range(26):
            a=math.tau*j/26
            rr=r*(1+0.14*math.sin(j*2.6+seed)+rng.uniform(-.055,.055))
            pts.append((cx+math.cos(a)*rr,cy+math.sin(a)*rr*.83))
        col=pal[min(5,max(0,int((210-cy)/38)))]
        poly(im,pts,col,INK,3)
        # broad, matte lit planes, no circular candy highlights
        top=[(cx-r*.72,cy-r*.12),(cx-r*.47,cy-r*.56),(cx+r*.05,cy-r*.7),(cx+r*.59,cy-r*.27),(cx+r*.26,cy-r*.35),(cx-r*.15,cy-r*.37)]
        poly(im,top,pal[min(5,max(1,int((210-cy)/38)+1))])
        for k in range(4):
            x=cx+rng.uniform(-r*.55,r*.5); y=cy+rng.uniform(-r*.3,r*.28)
            line(im,[(x-6,y+2),(x,y-1),(x+7,y+1)],'#46603b' if cy<140 else '#314b35',1)
    return im

def rock(seed):
    im=canvas(220,180)
    pts=[(20,105),(43,52),(91,24),(151,35),(194,78),(203,127),(159,153),(72,157),(29,138)]
    poly(im,pts,'#596765','#303f3d',3)
    poly(im,[(43,52),(91,24),(151,35),(158,68),(112,94),(62,94)],'#a3aba0')
    poly(im,[(62,94),(112,94),(138,133),(72,157),(29,138),(20,105)],'#73817a')
    poly(im,[(112,94),(158,68),(194,78),(203,127),(159,153),(138,133)],'#66746f')
    poly(im,[(43,52),(91,24),(151,35),(129,41),(89,37),(53,61)],'#bbc0ae')
    line(im,[(113,95),(99,112),(105,122)],'#44554f',2)
    line(im,[(158,68),(165,85),(179,93)],'#495a52',2)
    poly(im,[(30,127),(44,116),(60,121),(70,145),(46,145)],'#526c40')
    line(im,[(48,125),(56,131)],'#83914e',2)
    if seed==2:
        # second silhouette is a taller broken rock, not just a recolor
        im=im.rotate(-19,Image.Resampling.BICUBIC,expand=False)
    return im

def body():
    im=canvas(160,160)
    ellipse(im,(19,17,142,146),'#435855','#253a34',4)
    ellipse(im,(23,18,137,136),'#8b9d93')
    # Matte lower body, restrained lit rim, neutral eyes
    poly(im,[(28,107),(42,124),(78,133),(119,121),(136,103),(129,131),(110,143),(52,139),(30,125)],'#607970')
    line(im,[(39,41),(57,29),(82,26),(111,33)],'#b1bfb0',3)
    ellipse(im,(56,68,65,79),'#25332e')
    ellipse(im,(94,68,103,79),'#25332e')
    return im

def hand():
    im=canvas(64,64)
    poly(im,[(16,15),(34,10),(48,18),(53,38),(43,51),(22,53),(11,43),(9,27)],'#a39b7b',INK,3)
    poly(im,[(15,22),(23,15),(36,15),(44,22),(43,30),(16,30)],'#c5b996')
    line(im,[(21,35),(29,37),(38,34)],'#807a61',2)
    return im

def enemy():
    im=canvas(180,180)
    poly(im,[(22,121),(30,81),(19,70),(45,68),(48,48),(64,52),(77,28),(91,39),(110,26),(122,48),(146,43),(140,71),(160,85),(147,119),(131,147),(95,157),(52,145)],'#4b544b','#263a32',4)
    poly(im,[(45,68),(63,52),(83,49),(101,40),(119,53),(137,72),(125,104),(68,112),(44,93)],'#74816b')
    poly(im,[(53,114),(82,128),(119,112),(140,98),(131,147),(95,157),(52,145)],'#394b40')
    line(im,[(57,81),(74,84)],'#c3a16c',4)
    line(im,[(105,84),(121,79)],'#c3a16c',4)
    line(im,[(55,74),(75,77)],'#35473b',3)
    line(im,[(103,77),(123,71)],'#35473b',3)
    return im

def grass(seed):
    rng=random.Random(seed); im=canvas(128,96)
    for j in range(14):
        x=rng.uniform(19,110); y=rng.uniform(60,85); h=rng.uniform(20,55)
        poly(im,[(x-3,y),(x+rng.uniform(-13,10),y-h),(x+3,y-2)],rng.choice(['#536940','#627a49','#82915a','#45613d']))
    return im

def fern():
    im=canvas(150,130)
    for a in [-2.7,-2.2,-1.75,-1.3,-.8,-.3]:
        end=(75+math.cos(a)*62,112+math.sin(a)*83)
        line(im,[(75,113),end],'#344e35',3)
        for t in [.25,.4,.55,.7,.85]:
            x=75+(end[0]-75)*t;y=112+(end[1]-112)*t
            r=15*(1-t)+5
            for sign in [-1,1]:
                tip=(x+math.cos(a+sign*1.1)*r,y+math.sin(a+sign*1.1)*r)
                poly(im,[(x,y+3),tip,(x+(end[0]-75)*.07,y+(end[1]-112)*.07)],'#71884c' if sign<0 else '#476b41')
    return im

def log():
    im=canvas(220,110)
    poly(im,[(31,33),(174,14),(194,41),(187,74),(48,98),(24,76)],'#705c40',INK,3)
    poly(im,[(31,33),(174,14),(180,29),(42,51)],'#a18355')
    line(im,[(53,67),(146,52),(179,42)],'#4e4633',3)
    line(im,[(75,82),(122,74)],'#4e4633',2)
    ellipse(im,(18,32,63,98),'#b89b68',INK,3)
    ellipse(im,(26,40,55,89),None,'#866c43',2)
    line(im,[(36,43),(43,64),(32,82)],'#866c43',2)
    return im

def icons():
    im=canvas(128,128)
    poly(im,[(27,49),(31,31),(47,27),(52,32),(64,23),(77,28),(81,25),(96,30),(101,57),(95,70),(89,81),(89,105),(47,105),(46,89),(25,75),(19,60)],'#ded6bd','#172820',4)
    line(im,[(51,34),(53,55),(43,66),(30,53)],'#726f5c',3)
    line(im,[(64,30),(65,51)],'#726f5c',3)
    line(im,[(79,32),(80,51)],'#726f5c',3)
    line(im,[(94,37),(94,54)],'#726f5c',3)
    poly(im,[(48,89),(88,85),(89,105),(47,105)],'#a3a088')
    save(im,'icon_fist.png')
    im=canvas(128,128)
    ImageDraw.Draw(im).rounded_rectangle((27*S,30*S,101*S,112*S),12*S,'#c0b796','#23382e',4*S)
    line(im,[(48,29),(48,19),(80,19),(80,29)],'#e0d7b9',5)
    poly(im,[(27,37),(101,37),(101,63),(88,72),(40,72),(27,62)],'#8f967b','#23382e',3)
    line(im,[(64,58),(64,77)],'#e0d7b9',6)
    line(im,[(41,88),(88,88)],'#686f58',3)
    save(im,'icon_inventory.png')
    im=canvas(128,128)
    poly(im,[(33,40),(81,23),(105,43),(108,78),(55,105),(24,82)],'#9f8254','#26392e',3)
    poly(im,[(33,40),(81,23),(105,43),(53,66)],'#c3a674')
    ellipse(im,(24,41,61,105),'#c8ae78','#4c4c35',3)
    ellipse(im,(32,51,52,95),None,'#92713f',2)
    line(im,[(62,74),(95,57)],'#6c5637',3)
    save(im,'icon_wood.png')
    save(rock(1).resize((128*S,128*S)),'icon_stone.png')
    im=canvas(128,128)
    for b in [(24,48,68,94),(57,46,101,92),(45,72,89,114)]:
        ellipse(im,b,'#916b55','#26392e',3)
        line(im,[(b[0]+8,b[1]+13),(b[0]+17,b[1]+7)],'#c29971',2)
    poly(im,[(59,51),(40,36),(44,22),(68,34),(85,21),(101,27),(81,47)],'#7e9259','#354b35',2)
    save(im,'icon_food.png')

def ui():
    im=canvas(128,128)
    ImageDraw.Draw(im).rounded_rectangle((2*S,2*S,126*S,126*S),12*S,(24,40,33,235),(116,129,100,200),2*S)
    line(im,[(18,5),(110,5)],(188,193,158,110),1)
    save(im,'panel.png')
    im=canvas(256,256)
    ellipse(im,(7,7,249,249),(24,40,33,62),(219,219,188,95),3)
    ellipse(im,(23,23,233,233),None,(28,44,33,70),2)
    for x,y in [(128,21),(128,235),(21,128),(235,128)]:
        ellipse(im,(x-3,y-3,x+3,y+3),(224,223,192,130))
    save(im,'joystick_base.png')
    im=canvas(128,128)
    ellipse(im,(5,5,123,123),(29,47,37,125),(226,225,194,175),3)
    ellipse(im,(13,13,115,115),None,(132,147,105,125),1)
    save(im,'joystick_thumb.png')
    im=canvas(256,256)
    ellipse(im,(5,5,251,251),(26,41,33,225),'#929b79',4)
    ellipse(im,(16,16,240,240),None,'#4e614b',2)
    save(im,'attack_button.png')
    im=canvas(256,128)
    ellipse(im,(5,8,250,123),(19,33,24,90))
    im=im.filter(ImageFilter.GaussianBlur(10*S))
    save(im,'contact_shadow.png')
    Image.new('RGBA',(8,8),'white').save(OUT/'white.png')

def ground():
    # Unique continuous field, no tile boundaries. Fixed seed keeps review reproducible.
    rng=random.Random(814)
    w,h=2560,1440
    im=Image.new('RGB',(w,h),'#71834e')
    wash=Image.new('RGBA',(w,h));d=ImageDraw.Draw(wash)
    for k in range(230):
        x=rng.randint(-150,w+150);y=rng.randint(-150,h+150);r=rng.randint(80,260)
        col=rng.choice([(50,81,49,28),(126,143,79,30),(149,149,83,25),(62,100,61,20)])
        d.ellipse((x-r,y-r*.6,x+r,y+r*.6),fill=col)
    wash=wash.filter(ImageFilter.GaussianBlur(45))
    im=Image.alpha_composite(im.convert('RGBA'),wash)
    path=Image.new('RGBA',(w,h));d=ImageDraw.Draw(path)
    pts=[(-100,1270),(340,1110),(750,940),(1080,825),(1390,700),(1750,510),(2100,280),(2560,170)]
    d.line(pts,fill=(158,140,98,185),width=170,joint='curve')
    path=path.filter(ImageFilter.GaussianBlur(36))
    im=Image.alpha_composite(im,path)
    earth=Image.new('RGBA',(w,h));d=ImageDraw.Draw(earth)
    for wx,wy,r in [(-4.7,2.2,72),(-6.5,1.5,46),(6.8,3.4,61),(12.2,-3,92),(-13.5,-2.1,62),(-3.8,-4,63),(-10.1,3.5,84),(8.1,-5.7,80)]:
        cx=(wx+24)/48*w;cy=(13.5-wy)/27*h
        points=[]
        for j in range(22):
            a=math.tau*j/22;rr=r*rng.uniform(.8,1.2)
            points.append((cx+math.cos(a)*rr,cy+math.sin(a)*rr*.65))
        d.polygon(points,fill=(139,126,85,90))
    earth=earth.filter(ImageFilter.GaussianBlur(14))
    im=Image.alpha_composite(im,earth)
    d=ImageDraw.Draw(im)
    for k in range(6000):
        x=rng.randrange(w);y=rng.randrange(h)
        col=rng.choice(['#697c4b','#7a8a51','#849057','#617649','#75844e'])
        if rng.random()<.18:
            d.line([(x-3,y+4),(x,y-3),(x+2,y+1)],fill=col,width=1)
        else:
            d.line([(x,y),(x+rng.randint(2,6),y+rng.randint(-1,1))],fill=col,width=1)
    # Tiny earth flecks only along the trail, with irregular coverage.
    for k in range(600):
        t=rng.random()*(len(pts)-1);i=min(int(t),len(pts)-2);f=t-i
        x=pts[i][0]*(1-f)+pts[i+1][0]*f+rng.gauss(0,57)
        y=pts[i][1]*(1-f)+pts[i+1][1]*f+rng.gauss(0,40)
        d.ellipse((x,y,x+rng.randint(1,4),y+2),fill=rng.choice(['#9c9462','#8d885b','#a69c6d']))
    im.convert('RGB').save(OUT/'ground.png')

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('output',type=Path);a=p.parse_args()
    OUT=a.output;OUT.mkdir(parents=True,exist_ok=True)
    ground()
    save(tree(14),'tree_oak.png');save(tree(29,True),'tree_olive.png')
    save(rock(1),'rock_wide.png');save(rock(2),'rock_split.png')
    save(body(),'player_body.png');save(hand(),'player_hand.png')
    save(enemy(),'enemy.png');save(grass(21),'grass.png');save(fern(),'fern.png');save(log(),'fallen_log.png')
    icons();ui()
    print(f'Wrote {len(list(OUT.glob("*.png")))} original sprites to {OUT}')
