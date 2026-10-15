import json, time, sys
from pathlib import Path

root=Path(sys.argv[1])
sequence={'host':0,'client':0}
results=[]
def state(role):
    try:return json.loads((root/(role+'-state.json')).read_text(encoding='utf-8-sig'))
    except (OSError,ValueError):return None
def actor(role,owner):
    data=state(role)
    return next((a for a in data['Actors'] if a['Owner']==owner),None) if data else None
def command(role,**kw):
    sequence[role]+=1
    value=dict(Sequence=sequence[role],Actor=-1,HP=-1,X=0,Y=0)
    value.update(kw)
    value['Fixture']=value['Actor']>=0
    target=root/(role+'-command.json')
    pending=target.with_suffix('.next')
    pending.write_text(json.dumps(value),encoding='utf-8')
    pending.replace(target)
def wait(label,predicate,timeout=4):
    until=time.monotonic()+timeout
    while time.monotonic()<until:
        try:
            if predicate():
                results.append(dict(check=label,passed=True))
                print('PASS:',label,flush=True)
                for role in sequence:
                    (root/('proof-'+label+'-'+role+'.json')).write_text(json.dumps(state(role),indent=2),encoding='utf-8')
                return
        except (OSError,ValueError,TypeError,KeyError):pass
        time.sleep(.02)
    results.append(dict(check=label,passed=False))
    raise AssertionError(label)
def both(owner,predicate):return all((a:=actor(role,owner)) is not None and predicate(a) for role in sequence)
try:
    wait('two-spawn',lambda:all(len(state(r)['Actors'])==2 for r in sequence))
    a0=actor('host',0)['Position'];b0=actor('host',1)['Position']
    command('host',Y=.5)
    wait('host-walk',lambda:both(0,lambda a:a['Position']['y']>a0['y']+.6 and a['Facing']['y']>.9))
    command('host');time.sleep(.35)
    command('client',X=-.5)
    wait('client-walk',lambda:both(1,lambda a:a['Position']['x']<b0['x']-.6 and a['Facing']['x']<-.9))
    command('client');time.sleep(.35)
    command('host',Y=1)
    wait('host-sprint',lambda:both(0,lambda a:a['Motion']&2))
    command('host',X=-1,Dash=True)
    wait('host-dash',lambda:both(0,lambda a:a['Motion']&4))
    command('host');time.sleep(.4)
    command('client',Y=-1)
    wait('client-sprint',lambda:both(1,lambda a:a['Motion']&2))
    command('client',X=1,Dash=True)
    wait('client-dash',lambda:both(1,lambda a:a['Motion']&4))
    command('client');time.sleep(.4)
    command('host',Actor=0,HP=73,Head='iron_head',Body='iron_body',Held='wood_sword')
    wait('host-appearance',lambda:both(0,lambda a:a['HP']==73 and a['Head']=='iron_head' and a['Body']=='iron_body' and a['Held']=='wood_sword' and a['HeldSprite']=='wood_sword'))
    command('host',Actor=1,HP=58,Head='leather_head',Body='leather_body',Held='axe')
    wait('client-appearance',lambda:both(1,lambda a:a['HP']==58 and a['Head']=='leather_head' and a['Body']=='leather_body' and a['Held']=='axe' and a['HeldSprite']=='axe'))
finally:
    (root/'review-results.json').write_text(json.dumps(results,indent=2),encoding='utf-8')
