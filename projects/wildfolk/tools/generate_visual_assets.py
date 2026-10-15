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
    # Facing eyes are separate sprites; the simple body never rotates.
    ellipse(im,(19,19,141,141),'#b48765','#634c3b',3)
    ellipse(im,(21,20,139,135),'#d6aa84')
    return im

def hand():
    im=canvas(64,64)
    ellipse(im,(10,10,54,54),'#ba8d68','#634c3b',3)
    ellipse(im,(13,12,51,47),'#dbb08a')
    return im

def enemy():
    im=canvas(160,160)
    ellipse(im,(19,19,141,141),'#485e4c','#2a3d31',3)
    ellipse(im,(21,20,139,134),'#75816b')
    poly(im,[(26,63),(31,49),(39,53),(36,68)],'#596c54')
    poly(im,[(115,105),(127,99),(134,111),(128,122),(117,118)],'#596c54')
    return im

def character_details():
    im=canvas(32,32)
    ellipse(im,(9,9,23,23),'#2b302c')
    save(im,'player_eye.png')
    im=canvas(32,32)
    poly(im,[(5,10),(27,13),(25,22),(8,20)],'#2c3b30')
    line(im,[(10,16),(22,17)],'#b1aa78',2)
    save(im,'zombie_eye.png')
    im=canvas(64,64)
    ellipse(im,(10,10,54,54),'#556c53','#2a3d31',3)
    ellipse(im,(13,12,51,47),'#829077')
    save(im,'zombie_hand.png')

def tools():
    im=canvas(128,192)
    poly(im,[(57,54),(72,54),(71,174),(58,177)],'#8b6b43','#343a2d',3)
    line(im,[(62,70),(62,162)],'#b39760',3)
    poly(im,[(36,24),(69,19),(78,30),(108,19),(116,38),(110,67),(80,73),(69,56),(36,54)],'#8e704b','#3d3c2d',3)
    poly(im,[(81,31),(108,23),(112,38),(106,64),(81,67)],'#c0a06b')
    line(im,[(40,31),(62,28)],'#b3935d',2)
    save(im,'axe.png')
    im=canvas(128,192)
    poly(im,[(57,41),(72,42),(71,174),(58,177)],'#8b6b43','#343a2d',3)
    line(im,[(62,64),(62,162)],'#b39760',3)
    poly(im,[(12,45),(31,23),(60,15),(86,19),(109,35),(119,62),(98,43),(81,35),(58,34),(34,37)],'#96794e','#3d3c2d',3)
    line(im,[(26,35),(53,23),(78,25),(103,40)],'#c1a574',4)
    save(im,'pickaxe.png')
    im=canvas(128,192)
    poly(im,[(57,116),(72,116),(73,178),(56,178)],'#735636','#333b2c',3)
    poly(im,[(64,10),(82,36),(78,122),(49,122),(45,36)],'#a98a57','#333b2c',3)
    poly(im,[(64,16),(65,112),(53,114),(50,38)],'#c4ab77')
    poly(im,[(33,117),(94,117),(94,130),(33,130)],'#7d613d','#333b2c',3)
    line(im,[(61,141),(69,144),(60,154),(69,158)],'#b0935e',2)
    save(im,'wood_sword.png')

def workbench():
    im=canvas(256,196)
    poly(im,[(40,67),(59,68),(57,162),(38,162)],'#665037',INK,3)
    poly(im,[(199,56),(219,55),(220,149),(201,155)],'#665037',INK,3)
    poly(im,[(54,127),(207,115),(207,132),(54,144)],'#806441',INK,3)
    poly(im,[(22,42),(223,29),(239,74),(41,90)],'#a98b59',INK,3)
    poly(im,[(41,90),(239,74),(235,95),(43,111)],'#795f3e',INK,3)
    line(im,[(35,58),(227,45)],'#6b5539',3)
    line(im,[(40,75),(232,61)],'#7d643f',3)
    poly(im,[(158,42),(186,39),(189,60),(162,64)],'#c2ab7b','#5a5137',2)
    poly(im,[(60,55),(80,52),(92,68),(67,71)],'#756d4c','#414d35',2)
    save(im,'workbench.png')

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
    im=canvas(64,64)
    ImageDraw.Draw(im).rectangle((14*S,10*S,25*S,54*S),fill='#ded6b8')
    ImageDraw.Draw(im).rectangle((39*S,10*S,50*S,54*S),fill='#ded6b8')
    save(im,'icon_pause.png')

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


def survival_art():
    im=canvas(160,128)
    poly(im,[(31,39),(62,23),(106,27),(135,54),(131,87),(94,104),(49,96),(24,69)],'#884d40',INK,4)
    poly(im,[(38,45),(65,33),(102,36),(116,54),(97,72),(58,78),(34,65)],'#b87863')
    line(im,[(62,39),(73,51),(61,70)],'#ddae89',5)
    ellipse(im,(90,47,124,85),'#d8c8a1','#674d39',3)
    ellipse(im,(100,56,115,74),'#826446')
    save(im,'raw_meat.png')
    im=canvas(160,128)
    poly(im,[(31,39),(62,23),(106,27),(135,54),(131,87),(94,104),(49,96),(24,69)],'#654b35',INK,4)
    poly(im,[(38,45),(65,33),(102,36),(116,54),(97,72),(58,78),(34,65)],'#a97c48')
    for x in [55,72,89]: line(im,[(x,42),(x+9,71)],'#664b32',5)
    ellipse(im,(99,58,123,82),'#c7b692','#624934',3)
    save(im,'cooked_meat.png')
    im=canvas(128,128)
    poly(im,[(64,12),(33,58),(27,82),(36,104),(60,115),(86,108),(101,87),(96,63)],'#669fa7',INK,4)
    poly(im,[(64,24),(43,65),(40,82),(54,94),(52,64)],'#a6c3bb')
    save(im,'water.png')
    im=canvas(220,180)
    # Top-down stone ring, wood beneath a restrained amber flame.
    ellipse(im,(23,29,197,154),'#524b36',INK,4)
    for x,y in [(49,63),(80,39),(124,36),(163,59),(175,100),(143,132),(98,144),(58,130),(34,99)]:
        ellipse(im,(x-17,y-13,x+17,y+13),'#788079','#39463d',3)
        line(im,[(x-10,y-6),(x+7,y-9)],'#a9ab97',2)
    line(im,[(70,116),(143,72)],'#403a2c',20);line(im,[(70,116),(143,72)],'#8d6340',12)
    line(im,[(68,73),(144,116)],'#403a2c',18);line(im,[(68,73),(144,116)],'#a37a49',10)
    poly(im,[(76,108),(78,82),(92,68),(103,26),(121,61),(137,79),(137,110),(116,126),(94,123)],'#b06d36','#654730',3)
    poly(im,[(96,108),(97,88),(108,64),(114,91),(127,105),(115,119)],'#d5a656')
    save(im,'campfire.png')
    im=canvas(190,160)
    # One small neutral boar, matte and distinct from the round zombie.
    ellipse(im,(37,36,150,130),'#6e6250',INK,4)
    poly(im,[(56,44),(88,29),(129,42),(139,68),(124,78),(87,60)],'#8c7d61')
    poly(im,[(45,72),(25,48),(27,88)],'#635440',INK,3)
    poly(im,[(143,69),(168,47),(163,91)],'#635440',INK,3)
    poly(im,[(59,111),(76,102),(118,102),(137,118),(126,142),(79,142)],'#8f8067',INK,3)
    ellipse(im,(79,123,86,129),'#353f32');ellipse(im,(108,123,115,129),'#353f32')
    ellipse(im,(54,92,60,98),'#292f29');ellipse(im,(136,92,142,98),'#292f29')
    line(im,[(73,137),(68,121)],'#c6bc99',4);line(im,[(123,137),(129,121)],'#c6bc99',4)
    save(im,'boar.png')


def progression_art():
    for tier,base,light,dark in [('stone','#7d8780','#a5ac9e','#535e57'),('iron','#a5b0aa','#c5cbbd','#626f6a')]:
        for kind in ['axe','pickaxe','sword']:
            im=canvas(128,192)
            if kind=='sword':
                poly(im,[(64,10),(82,34),(78,123),(49,123),(45,34)],base,INK,3)
                poly(im,[(64,17),(65,112),(53,114),(50,37)],light)
                poly(im,[(55,125),(73,125),(73,178),(55,178)],'#745a3d',INK,3)
                poly(im,[(33,118),(95,118),(95,132),(33,132)],dark,INK,3)
                line(im,[(62,142),(67,148),(61,157),(68,162)],'#b29669',3)
            else:
                poly(im,[(56,42),(70,42),(78,178),(58,182)],'#866645',INK,3)
                line(im,[(65,73),(69,169)],'#b59967',3)
                if kind=='axe':
                    poly(im,[(18,25),(62,16),(90,29),(98,60),(70,73),(29,66),(13,43)],base,INK,3)
                    poly(im,[(18,25),(58,23),(43,55),(27,59),(16,42)],light)
                    poly(im,[(69,26),(88,34),(90,57),(71,67)],dark)
                else:
                    poly(im,[(12,46),(33,20),(72,18),(109,45),(115,67),(91,48),(68,39),(42,39),(16,65)],base,INK,3)
                    line(im,[(24,42),(43,29),(73,28),(100,44)],light,4)
                for y in [55,66]:line(im,[(49,y),(78,y+3)],'#b29a72',4)
            save(im,tier+'_'+kind+'.png')
    im=rock(2)
    for pts in [[(46,58),(69,51),(79,69),(59,77)],[(121,85),(145,73),(154,95),(130,109)],[(85,118),(101,109),(111,128),(94,141)]]:
        poly(im,pts,'#906748','#45554a',3)
        line(im,[(pts[0][0]+4,pts[0][1]+2),(pts[1][0]-2,pts[1][1]+4)],'#bb9668',2)
    save(im,'iron_ore_node.png');save(im.resize((128*S,128*S)),'iron_ore.png')
    im=canvas(160,112)
    poly(im,[(28,34),(124,19),(146,48),(135,87),(40,96),(17,66)],'#76887e',INK,3)
    poly(im,[(28,34),(124,19),(137,40),(44,56)],'#b3bca9')
    poly(im,[(44,56),(137,40),(135,87),(40,96)],'#8b9c91')
    line(im,[(54,63),(120,53)],'#5e7168',3);save(im,'iron_ingot.png')
    im=canvas(210,170)
    poly(im,[(25,46),(172,38),(185,126),(157,151),(38,148),(21,118)],'#72593c',INK,4)
    poly(im,[(25,46),(61,18),(172,38),(170,82),(32,95)],'#a58754',INK,3)
    for y in [51,69]:line(im,[(38,y),(165,y-10)],'#634e36',3)
    for x in [46,143]:poly(im,[(x,34),(x+13,31),(x+15,140),(x+2,145)],'#637164',INK,2)
    poly(im,[(91,77),(111,76),(113,106),(94,108)],'#b5ae81',INK,2)
    ellipse(im,(100,88,105,96),'#465348');save(im,'storage_chest.png')
    im=canvas(240,230)
    poly(im,[(53,58),(72,19),(158,13),(181,59),(208,117),(202,194),(145,218),(62,205),(32,170),(32,108)],'#657267',INK,4)
    poly(im,[(72,19),(158,13),(162,40),(63,46)],'#8e9887',INK,3)
    poly(im,[(68,62),(161,58),(181,94),(60,103)],'#899484')
    for y in [103,131,160,184]:line(im,[(44,y),(183,y+3)],'#414f45',4)
    for x,y in [(74,106),(137,133),(80,162),(144,184)]:line(im,[(x,y),(x,y+22)],'#414f45',3)
    poly(im,[(81,119),(155,116),(166,167),(147,184),(78,181),(67,161)],'#26382e',INK,4)
    poly(im,[(85,157),(92,139),(104,148),(116,126),(127,147),(143,141),(148,164),(126,174),(99,172)],'#a5733e')
    line(im,[(87,167),(140,162)],'#c7a06b',5);save(im,'furnace.png')
    im=canvas(170,170)
    poly(im,[(55,131),(43,146),(62,141)],'#9d8957',INK,2);poly(im,[(106,132),(125,146),(106,143)],'#9d8957',INK,2)
    ellipse(im,(30,32,137,136),'#c0c1aa',INK,4)
    poly(im,[(34,54),(53,34),(88,25),(126,48),(123,66),(86,53),(51,67)],'#dedac0')
    poly(im,[(36,79),(58,60),(66,92),(57,115),(40,108)],'#8c9984',INK,2)
    poly(im,[(129,76),(110,59),(104,91),(114,114),(132,105)],'#8c9984',INK,2)
    poly(im,[(77,46),(68,34),(76,18),(85,27),(94,16),(104,36),(96,51)],'#95634c',INK,3)
    ellipse(im,(59,86,64,91),'#344437');ellipse(im,(105,86,110,91),'#344437')
    poly(im,[(75,104),(97,104),(86,126)],'#a18b52',INK,2)
    poly(im,[(71,122),(87,130),(101,122),(94,143),(81,145)],'#8c5b46',INK,2)
    save(im,'chicken.png')
    im=canvas(80,80);ellipse(im,(20,20,60,60),'#b7a16b',INK,3)
    for a in range(8):
        angle=math.tau*a/8;line(im,[(40+math.cos(angle)*26,40+math.sin(angle)*26),(40+math.cos(angle)*34,40+math.sin(angle)*34)],'#b7a16b',3)
    save(im,'sun.png')
    im=canvas(80,80);poly(im,[(51,9),(31,12),(17,28),(16,47),(28,65),(47,71),(65,57),(47,58),(34,45),(33,29)],'#c1c2a4',INK,3);save(im,'moon.png')

def world_pass_art():
    im=canvas(170,170)
    poly(im,[(29,73),(4,48),(15,99),(37,111)],'#74835a',INK,3)
    poly(im,[(139,73),(165,47),(155,99),(135,109)],'#74835a',INK,3)
    ellipse(im,(29,30,141,143),'#74835a',INK,4)
    poly(im,[(36,60),(48,32),(89,24),(126,43),(138,72),(96,51),(59,62)],'#9caa75')
    poly(im,[(36,113),(53,129),(115,130),(134,113),(135,145),(118,159),(49,159),(32,140)],'#665b43',INK,3)
    poly(im,[(82,50),(69,72),(94,80),(104,63)],'#4c6148')
    line(im,[(48,91),(65,87)],'#364334',4);line(im,[(106,87),(125,91)],'#364334',4)
    line(im,[(65,118),(103,118)],'#364334',3)
    save(im,'goblin.png')
    im=canvas(140,160)
    poly(im,[(23,125),(21,94),(40,48),(80,17),(111,13),(119,45),(103,79),(69,114)],'#c0c2ab',INK,3)
    line(im,[(30,146),(53,105),(100,31)],'#796e4d',4)
    for x,y in [(42,102),(53,82),(67,61),(82,41)]:line(im,[(x,y),(x-11,y-20)],'#889480',2)
    save(im,'feather.png')
    im=canvas(150,150)
    poly(im,[(37,19),(60,29),(100,23),(116,15),(126,44),(115,75),(137,108),(117,132),(83,121),(51,137),(20,112),(35,81),(22,49)],'#94704c',INK,4)
    poly(im,[(39,38),(73,42),(105,35),(109,66),(94,93),(117,109),(82,109),(52,119),(38,105),(49,79),(35,61)],'#b49362')
    line(im,[(50,57),(86,61),(91,98)],'#705c42',3);save(im,'leather.png')
    im=canvas(100,100)
    poly(im,[(24,59),(24,41),(32,37),(39,43),(39,19),(47,14),(54,19),(54,38),(60,32),(68,35),(72,31),(80,34),(83,64),(72,83),(41,84)],'#c5c3a3',INK,3)
    line(im,[(55,42),(55,60)],'#788574',2);line(im,[(69,42),(68,58)],'#788574',2);save(im,'icon_interact.png')


def water_container_art():
    for name,water in [('empty_cup',None),('raw_water_cup','#69795e'),('clean_water_cup','#73a5a6')]:
        im=canvas(128,128)
        poly(im,[(25,42),(99,42),(94,105),(83,116),(41,116),(30,103)],'#846844',INK,4)
        poly(im,[(35,59),(87,59),(83,105),(42,105)],'#a98a58')
        for x in [44,65,83]:line(im,[(x,62),(x-3,105)],'#705638',2)
        ellipse(im,(23,29,101,64),'#b49969',INK,4)
        ellipse(im,(32,37,93,56),water or '#453d2e',INK,2)
        if water:
            line(im,[(43,44),(73,44)],'#adc0a3' if name=='clean_water_cup' else '#879575',2)
            poly(im,[(64,66),(50,87),(52,98),(63,104),(75,98),(78,87)],water,INK,2)
        save(im,name+'.png')


def small_plant_art():
    im=canvas(150,100)
    for pts in [[(68,88),(59,53),(40,40)],[(72,89),(82,50),(111,33)],[(72,89),(72,44),(67,23)]]:
        line(im,pts,'#705738',6)
    for pts in [[(34,49),(24,32),(42,25),(58,38),(55,57)],[(61,37),(50,19),(69,9),(86,19),(83,42)],[(81,60),(86,34),(109,23),(123,39),(112,61)],[(51,73),(34,61),(39,44),(60,43),(70,63)],[(75,81),(70,58),(94,48),(112,65),(101,81)]]:
        poly(im,pts,'#637848',INK,3)
    line(im,[(61,78),(68,57)],'#8f794b',3);save(im,'small_shrub.png')


def character_growth_art():
    for material in ('leather','iron'):
        edge='#3e382e' if material=='leather' else '#344342'
        dark='#70543b' if material=='leather' else '#61716e'
        light='#94734e' if material=='leather' else '#9ba89d'
        # Head/body overlays are curated imagegen outputs; do not regenerate them as rings.
        im=canvas(34,38)
        poly(im,[(4,6),(30,6),(32,19),(27,25),(7,25),(2,19)],dark,edge,2)
        poly(im,[(6,7),(27,7),(27,12),(6,12)],light)
        save(im,material+'_hand_worn.png')
        im=canvas(112,112)
        poly(im,[(26,21),(45,15),(53,23),(61,23),(68,15),(86,21),(96,44),(85,55),(80,92),(32,92),(27,55),(16,44)],dark,edge,3)
        poly(im,[(33,27),(45,23),(53,32),(62,32),(72,23),(80,28),(78,79),(35,79)],light)
        line(im,[(56,36),(56,82)],edge,3)
        save(im,material+'_body.png')
        im=canvas(112,112)
        ellipse(im,(19,17,94,95),dark,edge,3)
        poly(im,[(20,64),(28,40),(47,28),(67,28),(86,41),(94,64)],light,edge,2)
        poly(im,[(33,67),(79,67),(78,85),(33,85)],'#bca284',edge,2)
        line(im,[(55,24),(55,47)],edge,3)
        save(im,material+'_head.png')
    im=canvas(140,140)
    ellipse(im,(3,3,137,137),'#253b32','#9c9b73',4)
    ellipse(im,(34,31,106,106),'#c99c74','#3f4230',2)
    ellipse(im,(57,78,62,84),'#2c3a32');ellipse(im,(79,78,84,84),'#2c3a32')
    save(im,'profile.png')
    # icon_dash.png is rasterized from the licensed Lucide chevrons-right source.
    for name in ('vitality','endurance','recovery','stride'):
        im=canvas(96,96)
        ellipse(im,(8,8,88,88),'#405647','#7d8967',2)
        if name=='vitality':poly(im,[(40,24),(56,24),(56,40),(72,40),(72,56),(56,56),(56,72),(40,72),(40,56),(24,56),(24,40),(40,40)],'#bead7e')
        elif name=='endurance':poly(im,[(53,20),(30,50),(46,50),(39,77),(69,41),(52,41)],'#bead7e')
        elif name=='recovery':
            line(im,[(69,63),(59,73),(39,73),(24,60),(24,39),(37,24),(60,24),(71,35)],'#bead7e',5)
            poly(im,[(71,21),(73,45),(52,39)],'#bead7e')
        else:
            poly(im,[(31,22),(50,24),(51,51),(73,61),(73,74),(26,74),(21,63),(31,55)],'#bead7e')
        save(im,'skill_'+name+'.png')
    # Tier II facility silhouettes remain wood/stone, with restrained metal joinery.
    im=canvas(240,200)
    poly(im,[(26,58),(176,26),(220,66),(69,104)],'#a18b5b',INK,4)
    poly(im,[(26,58),(69,104),(69,132),(26,86)],'#6d593c',INK,3)
    poly(im,[(69,104),(220,66),(220,96),(69,132)],'#735c3c',INK,3)
    for x,y in ((36,88),(183,101),(76,126)):
        poly(im,[(x,y),(x+17,y),(x+14,y+59),(x-3,y+59)],'#66533b',INK,3)
        poly(im,[(x-2,y+24),(x+16,y+24),(x+15,y+34),(x-3,y+34)],'#849086',INK,2)
    poly(im,[(139,40),(175,32),(189,51),(153,60)],'#74837b',INK,3)
    poly(im,[(97,66),(117,61),(118,77),(97,82)],'#434e47',INK,2)
    save(im,'workbench_ii.png')
    im=canvas(180,155)
    poly(im,[(22,46),(143,31),(165,52),(43,70)],'#9b875b',INK,3)
    poly(im,[(22,46),(43,70),(43,135),(22,112)],'#6f5a3d',INK,3)
    poly(im,[(43,70),(165,52),(165,112),(43,135)],'#816c46',INK,3)
    for x in (61,132):poly(im,[(x,65),(x+14,63),(x+14,121),(x,123)],'#74847a',INK,2)
    poly(im,[(93,75),(109,73),(109,94),(93,96)],'#b4af8c',INK,2)
    save(im,'reinforced_chest.png')

def ecology_art():
    # The same original polygon/ellipse language as the existing boar and goblin.
    im=canvas(150,170)
    poly(im,[(46,84),(28,14),(40,7),(65,69)],'#8c8771',INK,3)
    poly(im,[(88,71),(111,12),(126,15),(110,91)],'#8c8771',INK,3)
    line(im,[(37,22),(56,71)],'#af9982',5);line(im,[(117,25),(103,74)],'#af9982',5)
    ellipse(im,(38,63,116,151),'#827f6c',INK,3)
    poly(im,[(44,92),(59,68),(91,66),(109,96),(87,88),(67,92)],'#a3a18a')
    poly(im,[(47,113),(64,105),(88,107),(106,121),(91,149),(62,153)],'#99957e',INK,2)
    line(im,[(58,122),(62,124)],INK,3);line(im,[(88,124),(93,122)],INK,3)
    poly(im,[(73,141),(80,141),(76,146)],'#5d5b4d');save(im,'rabbit.png')
    im=canvas(180,240)
    poly(im,[(51,197),(49,226),(60,227),(64,199)],'#604f3b',INK,3)
    poly(im,[(116,195),(118,227),(130,226),(129,194)],'#604f3b',INK,3)
    poly(im,[(49,85),(59,41),(114,38),(139,88),(131,162),(115,204),(63,201),(43,160)],'#967752',INK,3)
    poly(im,[(59,58),(85,42),(117,51),(125,104),(103,131),(59,107)],'#b59b6f')
    poly(im,[(62,138),(37,127),(29,150),(62,164)],'#8a734f',INK,3);poly(im,[(121,141),(145,126),(153,148),(123,165)],'#8a734f',INK,3)
    poly(im,[(63,138),(115,135),(120,172),(101,212),(79,211),(58,174)],'#aa8c5f',INK,3)
    poly(im,[(71,177),(106,176),(100,207),(83,210)],'#c2b18b')
    poly(im,[(82,204),(100,204),(94,216),(86,216)],'#414536',INK,2)
    line(im,[(68,171),(75,174)],INK,3);line(im,[(104,174),(113,170)],INK,3)
    for sign in (-1,1):
        pts=[(90+sign*x,y) for x,y in [(29,146),(38,118),(51,100),(55,70),(49,47)]]
        line(im,pts,'#5f634e',6);line(im,[(90+sign*46,109),(90+sign*70,96),(90+sign*72,74)],'#5f634e',5)
        line(im,[(90+sign*53,85),(90+sign*68,64)],'#5f634e',4)
    save(im,'deer.png')
    im=canvas(175,190)
    poly(im,[(41,149),(39,177),(53,178),(57,148)],'#535a4e',INK,3);poly(im,[(120,147),(124,178),(138,176),(136,147)],'#535a4e',INK,3)
    wool=[(35,135),(25,101),(34,70),(29,48),(48,21),(71,27),(93,18),(111,27),(135,25),(151,53),(145,76),(154,105),(142,140),(119,158),(92,153),(64,162)]
    poly(im,wool,'#b8b9a3',INK,3)
    poly(im,[(41,72),(39,45),(57,34),(75,40),(98,29),(124,38),(139,63),(129,98),(105,104),(88,96),(64,103)],'#d0d0b8')
    poly(im,[(51,117),(28,109),(24,128),(50,138)],'#657061',INK,3);poly(im,[(125,118),(146,109),(153,130),(129,138)],'#657061',INK,3)
    poly(im,[(54,110),(122,110),(127,137),(109,172),(75,171),(53,142)],'#687062',INK,3)
    line(im,[(63,133),(70,136)],INK,3);line(im,[(108,135),(116,132)],INK,3)
    poly(im,[(82,157),(104,157),(99,168),(88,168)],'#444e42');save(im,'sheep.png')
    im=canvas(160,180)
    poly(im,[(54,100),(37,123),(44,148),(61,141),(66,165),(94,165),(101,141),(117,148),(124,121),(104,100)],'#909783',INK,3)
    for y,w in [(91,43),(110,39),(129,28)]:
        poly(im,[(80-w,y),(80,y-7),(80+w,y),(80+w-5,y+8),(80,y+1),(80-w+5,y+9)],'#c5c4a7',INK,3)
    line(im,[(80,80),(80,145)],'#d2ceb1',7)
    poly(im,[(39,38),(57,15),(108,16),(126,43),(117,79),(102,101),(63,99),(45,80)],'#c7c6ab',INK,3)
    poly(im,[(47,39),(61,23),(107,23),(117,42),(88,33),(63,39)],'#d8d3b7')
    poly(im,[(49,55),(70,52),(76,68),(58,73)],'#384639',INK,2);poly(im,[(88,68),(92,52),(113,55),(107,72)],'#384639',INK,2)
    poly(im,[(79,73),(73,83),(88,83)],'#66715f')
    line(im,[(61,90),(102,90)],'#626e5b',3);save(im,'skeleton.png')
    im=canvas(64,82)
    poly(im,[(22,64),(25,45),(16,31),(19,17),(27,16),(29,28),(34,10),(42,13),(39,30),(47,22),(54,29),(49,45),(39,59),(40,71)],'#b9bea4',INK,3)
    poly(im,[(26,47),(34,37),(44,37),(37,55)],'#d1d0b4');save(im,'bone_hand.png')
    im=canvas(100,180)
    poly(im,[(49,8),(65,40),(61,107),(43,108),(39,40)],'#8a9483',INK,3)
    poly(im,[(49,15),(50,103),(43,103),(43,41)],'#b4b7a0')
    poly(im,[(53,40),(64,43),(58,68),(52,62)],'#99734f');poly(im,[(44,83),(56,91),(52,105),(42,103)],'#99734f')
    poly(im,[(27,106),(74,106),(74,115),(28,118)],'#685d44',INK,3)
    poly(im,[(43,117),(60,116),(57,162),(46,162)],'#725a3d',INK,3);line(im,[(46,131),(57,136)],'#9f8154',3);save(im,'rusted_sword.png')
    # Wall top and front faces share a straight footprint so adjacent pieces join.
    im=canvas(240,110)
    poly(im,[(9,30),(230,30),(230,93),(9,93)],'#685336',INK,3)
    for y in (38,63):poly(im,[(10,y),(229,y),(229,y+24),(10,y+24)],'#907044',INK,2)
    poly(im,[(9,30),(24,13),(216,13),(231,30)],'#b09866',INK,3)
    line(im,[(24,20),(215,20)],'#7c6641',2)
    for x in (55,147,200):line(im,[(x,43),(x+17,45)],'#b39864',2)
    save(im,'wood_wall.png')
    im=canvas(240,110)
    poly(im,[(9,31),(230,31),(230,91),(9,91)],'#596b62',INK,3)
    for y,offset in ((34,0),(62,32)):
        for x in range(-23+offset,231,64):
            left=max(10,x);right=min(230,x+62)
            if right>left:poly(im,[(left,y),(right,y),(right,y+26),(left,y+26)],'#75857a',INK,2)
    poly(im,[(9,31),(22,13),(216,13),(231,31)],'#a4ad9a',INK,3)
    for x in (63,127,191):line(im,[(x,14),(x+7,30)],'#697c6b',2)
    save(im,'stone_wall.png')
    im=canvas(44,110)
    poly(im,[(7,27),(14,10),(30,10),(38,27),(38,96),(7,96)],'#77603f',INK,3)
    poly(im,[(7,27),(14,10),(30,10),(38,27)],'#b09866',INK,2);line(im,[(14,35),(14,87)],'#a88b59',3);save(im,'wall_post.png')
    im=canvas(240,110)
    for x in (10,207):
        poly(im,[(x,25),(x+8,12),(x+22,12),(x+22,96),(x,96)],'#796140',INK,3)
        poly(im,[(x,25),(x+8,12),(x+22,12),(x+22,26)],'#af9562',INK,2)
    poly(im,[(26,88),(215,88),(220,99),(21,99)],'#6a6048',INK,2)
    save(im,'door_frame.png')
    leaf=canvas(204,94)
    poly(leaf,[(4,21),(199,21),(199,83),(4,83)],'#83673e',INK,3)
    poly(leaf,[(4,21),(15,7),(188,7),(199,21)],'#b09665',INK,3)
    for x in (37,73,109,145,181):line(leaf,[(x,24),(x,79)],'#5e5036',2)
    for y in (34,65):line(leaf,[(5,y),(107,y)],'#425348',8)
    poly(leaf,[(170,48),(185,48),(185,58),(170,58)],'#c0b488',INK,2)
    save(leaf,'door_leaf.png')
    im.alpha_composite(leaf,(18*S,0));save(im,'wood_door.png')
    im=canvas(80,110)
    poly(im,[(34,98),(30,58),(11,27),(9,12),(20,9),(24,24),(39,43),(55,24),(58,9),(70,12),(67,29),(49,58),(47,98)],'#886b43',INK,3)
    line(im,[(36,71),(45,73)],'#b29764',4);save(im,'slingshot.png')
    im=canvas(320,240)
    poly(im,[(31,194),(64,44),(158,17),(282,181),(233,208),(118,189),(74,223)],'#686c4c',INK,4)
    poly(im,[(64,44),(158,17),(192,116),(116,177),(31,194)],'#899067',INK,3)
    poly(im,[(158,17),(282,181),(233,208),(192,116)],'#62674b',INK,3)
    poly(im,[(61,180),(87,104),(130,134),(118,191)],'#34493b',INK,3)
    line(im,[(50,201),(65,39),(157,17)],'#795f3e',8);line(im,[(156,22),(267,201)],'#795f3e',8)
    poly(im,[(181,103),(200,104),(216,134),(191,129)],'#a09d70');line(im,[(113,58),(126,98)],'#b3b08a',3);save(im,'goblin_tent.png')
    im=canvas(130,110)
    poly(im,[(14,37),(91,21),(116,40),(38,61)],'#9a8050',INK,3);poly(im,[(14,37),(38,61),(39,101),(15,78)],'#66543c',INK,3)
    poly(im,[(38,61),(116,40),(115,82),(39,101)],'#7c6745',INK,3)
    line(im,[(59,58),(60,92)],'#4d5744',5);line(im,[(98,48),(98,81)],'#4d5744',5);save(im,'camp_box.png')
    im=canvas(36,36);poly(im,[(6,16),(12,7),(25,9),(31,20),(24,29),(10,28)],'#8f9a86',INK,2);save(im,'pebble.png')

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('output',type=Path);p.add_argument('--ecology-only',action='store_true');a=p.parse_args()
    OUT=a.output;OUT.mkdir(parents=True,exist_ok=True)
    if a.ecology_only:
        ecology_art();raise SystemExit(0)
    ground()
    save(tree(14),'tree_oak.png');save(tree(29,True),'tree_olive.png')
    save(rock(1),'rock_wide.png');save(rock(2),'rock_split.png')
    save(body(),'player_body.png');save(hand(),'player_hand.png')
    save(enemy(),'enemy.png');save(grass(21),'grass.png');save(fern(),'fern.png');save(log(),'fallen_log.png')
    character_details();tools();workbench();character_growth_art()
    icons();ui();survival_art();progression_art();world_pass_art();water_container_art();small_plant_art();ecology_art()
    print(f'Wrote {len(list(OUT.glob("*.png")))} original sprites to {OUT}')
