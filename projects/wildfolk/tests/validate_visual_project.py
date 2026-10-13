"""Validate the committed playable Unity source without requiring the Editor."""
from pathlib import Path
import json
import re
import sys

root=Path(sys.argv[1] if len(sys.argv)>1 else Path(__file__).resolve().parents[1]/'client')
errors=[]
def check(condition,message):
    if not condition: errors.append(message)

scene=root/'Assets/Wildfolk/Scenes/Wildfolk.unity'
check(scene.is_file(),'Playable scene is missing')
required=['Assets','Packages','ProjectSettings']
for name in required: check((root/name).is_dir(),f'Missing Unity directory: {name}')
check('6000.3.19f1' in (root/'ProjectSettings/ProjectVersion.txt').read_text(),'Unexpected Unity version')
settings=(root/'ProjectSettings/ProjectSettings.asset').read_text()
for token in ['defaultScreenOrientation: 3','defaultScreenWidth: 1920','defaultScreenHeight: 1080','allowedAutorotateToPortrait: 0','allowedAutorotateToPortraitUpsideDown: 0','allowedAutorotateToLandscapeLeft: 1','allowedAutorotateToLandscapeRight: 1']:
    check(token in settings,f'Mobile setting missing: {token}')
build=(root/'ProjectSettings/EditorBuildSettings.asset').read_text()
check('Assets/Wildfolk/Scenes/Wildfolk.unity' in build,'Playable scene absent from build settings')
check('WildfolkVisual' not in build,'Replaced visual scene still enabled')
check('SampleScene' not in build,'Replaced sample scene is still enabled')
manifest=json.loads((root/'Packages/manifest.json').read_text())
check(manifest['dependencies'].get('com.unity.render-pipelines.universal')=='17.3.0','Unexpected URP version')
for name in ['Player','Enemy','WorldPickup']:
    check((root/f'Assets/Wildfolk/Prefabs/{name}.prefab').is_file(),f'Missing playable prefab: {name}')
for name in ['PlayerVisual','EnemyVisual']:
    check(not (root/f'Assets/Wildfolk/Prefabs/{name}.prefab').exists(),f'Unused visual prefab remains: {name}')
check(not (root/'Assets/Wildfolk/Art/icon_food.png').exists(),'Unused food icon remains')
for name in ['VisualPrototypeBuilder','VisualHudBuilder']:
    check(not (root/f'Assets/Wildfolk/Editor/{name}.cs').exists(),f'Replaced builder remains: {name}')

guids={}
for meta in (root/'Assets').rglob('*.meta'):
    match=re.search(r'^guid: ([0-9a-f]{32})$',meta.read_text(),re.M)
    if not match: continue
    guid=match[1]
    check(guid not in guids,f'Duplicate GUID: {meta}')
    guids[guid]=meta
    check(Path(str(meta)[:-5]).exists(),f'Orphaned meta: {meta}')

for file in (root/'Assets/Wildfolk').rglob('*'):
    if file.is_file() and file.suffix!='.meta':
        check(Path(str(file)+'.meta').exists(),f'Missing meta: {file}')
    if file.suffix not in ['.unity','.prefab']: continue
    content=file.read_text()
    for script_guid in re.findall(r'm_Script: \{fileID: 11500000, guid: ([0-9a-f]{32})',content):
        # UI package scripts live outside Assets. Runtime project scripts must have metas.
        if script_guid in guids: check(Path(str(guids[script_guid])[:-5]).suffix=='.cs',f'Invalid script reference in {file}')
    for guid in re.findall(r'm_Sprite: \{fileID: [0-9-]+, guid: ([0-9a-f]{32})',content):
        check(guid in guids,f'Missing sprite in {file}: {guid}')
    for visible in re.findall(r'^\s*m_Text: (.*)$',content,re.M):
        check(not re.search(r'DEBUG|TEST|TODO|WIP|DMG|Hotfix|Override|placeholder',visible,re.I),f'Forbidden visible text: {visible}')

if errors:
    print('\n'.join(errors));sys.exit(1)
print(f'PASS: Unity source structure, Landscape settings, {len(guids)} unique asset GUIDs, sprite references and visible text.')
