from audit_session import *
import re, zipfile, collections

ui_files=list((ASSETS/'Runtime/UI').glob('*.cs'))
extra=[]
for p in (ASSETS/'Runtime').rglob('*.cs'):
    if p in ui_files:continue
    text=p.read_text(encoding='utf-8-sig')
    if any(token in text for token in ['UnityEngine.UI','IPointer','IDragHandler','IPlayerInteractable','SaveRepository','class SceneFlow','class NetworkSession']):extra.append(p)
builders=[]
for p in (ASSETS/'Editor').glob('*.cs'):
    if any(word in p.stem for word in ['UiBuilder','SceneBuilder','UiFactory','PresentationBuilder','VisualArtImporter','WorldArt','JournalSymbol','JournalContent']):builders.append(p)
files=sorted(set(ui_files+extra+builders))
manifest=[]
for p in files:
    text=p.read_text(encoding='utf-8-sig'); lines=text.splitlines()
    methods=[]; bindings=[]
    for n,line in enumerate(lines,1):
        for m in re.finditer(r'\b(public|private|internal|protected)\s+(?:static\s+|async\s+|override\s+)*(?:[\w<>\[\],.?]+)\s+(\w+)\s*\(',line):methods.append({'line':n,'access':m[1],'method':m[2]})
        if any(word in line for word in ['AddPersistentListener','AddListener','onClick','onValueChanged','SetActive(','new GameObject(','OpenPanel','Show(','SetPage(']):bindings.append({'line':n,'text':line.strip()})
    manifest.append({'file':p.relative_to(CAN).as_posix(),'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'lines':len(lines),'methods':methods,'bindings':bindings,'group':'runtime-ui' if p in ui_files else 'editor-authoring' if p in builders else 'related-runtime'})
save('source-manifest',manifest)

scenes=[]; controls=[]; scene_surfaces=[]
for path in sorted((ASSETS/'Scenes').glob('*.unity')):
    text=path.read_text(encoding='utf-8-sig'); blocks={}
    for m in re.finditer(r'^--- !u!(\d+) &(-?\d+)(?: stripped)?\n(.*?)(?=^--- !u!|\Z)',text,re.M|re.S):blocks[m[2]]={'type':int(m[1]),'body':m[3],'line':text[:m.start()].count('\n')+1}
    gos={};parents={};components=collections.defaultdict(list)
    for ident,block in blocks.items():
        body=block['body']; game=re.search(r'^  m_GameObject: \{fileID: (-?\d+)\}',body,re.M)
        if game:components[game[1]].append((ident,block))
        if block['type']==1:
            name=re.search(r'^  m_Name: (.*)$',body,re.M);active=re.search(r'^  m_IsActive: (\d+)',body,re.M)
            gos[ident]={'name':name[1] if name else ident,'active':bool(int(active[1])) if active else True,'line':block['line']}
        if block['type'] in (4,224) and game:
            parent=re.search(r'^  m_Father: \{fileID: (-?\d+)\}',body,re.M)
            if parent:parents[ident]=(game[1],parent[1])
    go_transform={go:ident for ident,(go,parent) in parents.items()}
    def hierarchy(go):
        names=[];seen=set();active=True
        while go in gos and go not in seen:
            seen.add(go);names.append(gos[go]['name']);active&=gos[go]['active'];transform=go_transform.get(go);parent=parents.get(transform,(None,'0'))[1]
            go=parents.get(parent,('0',None))[0]
        return '/'.join(reversed(names)),active
    type_counts=collections.Counter()
    for go,items in components.items():
        for ident,block in items:
            body=block['body'];marker=re.search(r'^  m_EditorClassIdentifier: (.*)$',body,re.M)
            typename=marker[1].strip() if marker else 'UnityType'+str(block['type']);type_counts[typename]+=1
            name,active=hierarchy(go)
            if any(typename.endswith('::'+candidate) or typename.endswith('.'+candidate) for candidate in ['Button','Slider','Toggle','Dropdown','InputField','ScrollRect']):
                callbacks=[{'method':m[1],'target':m[0]} for m in re.findall(r'm_Target: \{fileID: (-?\d+)\}.*?m_MethodName: ([^\r\n]+)',body,re.S)]
                interactable=re.search(r'^  m_Interactable: (\d)',body,re.M)
                controls.append({'scene':path.name,'path':name,'gameObject':go,'component':ident,'type':typename.split('::')[-1],'activeInHierarchyAtAuthoring':active,'interactableAtAuthoring':None if interactable is None else bool(int(interactable[1])),'callbacks':callbacks,'line':block['line']})
            if typename.startswith('Wildfolk.Runtime::') and any(k in typename for k in ['Screen','Panel','Hud','Menu','Overlay','View','InventoryEquipment']):scene_surfaces.append({'scene':path.name,'path':name,'type':typename,'line':block['line'],'activeInHierarchyAtAuthoring':active})
    scenes.append({'file':path.relative_to(CAN).as_posix(),'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'gameObjects':len(gos),'rectTransforms':sum(b['type']==224 for b in blocks.values()),'canvasCount':sum(b['type']==223 for b in blocks.values()),'types':dict(type_counts)})
save('serialized-controls',controls);save('scene-surfaces',scene_surfaces);save('scene-inventory',scenes)

art=[]
(ROOT/'evidence/source-art').mkdir(parents=True,exist_ok=True)
for p in (PROJECT/'art-source').rglob('*'):
    if not p.is_file():continue
    row={'file':p.relative_to(CAN).as_posix(),'bytes':p.stat().st_size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()}
    if p.suffix=='.kra':
        with zipfile.ZipFile(p) as z:
            row['entries']=z.namelist()
            for name in ['maindoc.xml','documentinfo.xml']:
                if name in z.namelist():row[name]=z.read(name).decode('utf-8')
            if 'mergedimage.png' in z.namelist():
                target=ROOT/'evidence/source-art'/ (p.stem+'.png');target.write_bytes(z.read('mergedimage.png'));row['preview']=target.relative_to(ROOT).as_posix()
    art.append(row)
save('source-art-index',art)

history_roots=['commercial-ui-mobile','post-commercial-ui-qa-tools','ui-redesign','forensic-presentation-world-audit-2026-11-03']
screens=[]
for folder in history_roots:
    for p in (PROJECT/'validation'/folder).rglob('*.png'):screens.append(p)
paths=[p.relative_to(CAN).as_posix() for p in screens]
history=subprocess.check_output(['git','log','--format=COMMIT:%H %aI','--name-only','--']+[str(PROJECT.relative_to(CAN)/'validation'/folder) for folder in history_roots],cwd=CAN,text=True)
provenance={};commit=None
for line in history.splitlines():
    if line.startswith('COMMIT:'):commit=line[7:]
    elif line.endswith('.png') and line not in provenance:provenance[line]=commit
save('historical-image-index',[{'file':p.relative_to(CAN).as_posix(),'bytes':p.stat().st_size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'lastImageCommitAndDate':provenance.get(p.relative_to(CAN).as_posix()),'visuallyOpenedInThisAudit':False} for p in screens])
print(json.dumps({'sourceFiles':len(manifest),'runtimeUI':len(ui_files),'relatedRuntime':len(extra),'builders':len(builders),'scenes':len(scenes),'serializedControlInstances':len(controls),'attachedSurfaceComponents':len(scene_surfaces),'priorScreenshotsIndexed':len(screens),'sourceArtFiles':len(art)}))
