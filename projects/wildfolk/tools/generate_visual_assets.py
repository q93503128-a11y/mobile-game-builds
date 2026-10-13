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
    im=canvas(400,300)
    pts=[(29,106),(58,54),(135,26),(190,42),(264,25),(349,70),(370,121),(346,182),(279,240),(213,254),(145,239),(79,258),(31,218),(17,164)]
    poly(im,pts,'#847d53','#576946',4)
    poly(im,[(47,110),(74,75),(137,49),(196,63),(263,49),(330,88),(347,132),(323,176),(270,219),(211,229),(146,219),(83,231),(52,201),(40,160)],'#477c7b','#344f46',3)
    poly(im,[(64,112),(92,85),(142,66),(195,82),(262,66),(319,105),(331,135),(312,160),(271,176),(214,165),(155,184),(99,173),(59,154)],'#64948e')
    for x,y in [(97,123),(215,103),(271,188),(126,204)]: line(im,[(x-20,y),(x,y-3),(x+23,y+1)],'#8bada0',2)
    for x,y in [(36,179),(78,59),(338,189),(301,48)]:
        line(im,[(x,y+12),(x-7,y-15)],'#465e3d',4);line(im,[(x,y+9),(x+3,y-24)],'#778a52',3)
    save(im,'pond.png')
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
    im=canvas(240,120)
    for x in [15,220]:poly(im,[(x,20),(x+12,18),(x+12,107),(x,110)],'#755b3d',INK,3)
    for y in [24,60]:
        poly(im,[(9,y),(228,y-7),(232,y+24),(11,y+32)],'#977747',INK,3)
        line(im,[(21,y+8),(218,y+2)],'#b49665',2)
        for x in [29,108,211]:ellipse(im,(x,y+11,x+4,y+15),'#374435')
    save(im,'wood_wall.png')
    im=canvas(220,120)
    poly(im,[(12,22),(207,18),(207,103),(12,108)],'#614d36',INK,3)
    for x in [24,60,96,132,168]:poly(im,[(x,29),(x+29,28),(x+28,95),(x,99)],'#927344',INK,2)
    line(im,[(29,86),(184,41)],'#bf9b60',8)
    ellipse(im,(176,62,187,73),'#6b7466',INK,2)
    save(im,'wood_door.png')
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

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('output',type=Path);a=p.parse_args()
    OUT=a.output;OUT.mkdir(parents=True,exist_ok=True)
    ground()
    save(tree(14),'tree_oak.png');save(tree(29,True),'tree_olive.png')
    save(rock(1),'rock_wide.png');save(rock(2),'rock_split.png')
    save(body(),'player_body.png');save(hand(),'player_hand.png')
    save(enemy(),'enemy.png');save(grass(21),'grass.png');save(fern(),'fern.png');save(log(),'fallen_log.png')
    character_details();tools();workbench()
    icons();ui();survival_art();progression_art()
    print(f'Wrote {len(list(OUT.glob("*.png")))} original sprites to {OUT}')
