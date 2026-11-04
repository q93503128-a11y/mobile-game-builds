"""Read/navigation-only inspection of an already isolated Play session. No authoring."""
import sys,json,time
from pathlib import Path
sys.dont_write_bytecode=True
import audit_session as a
OUT=a.ROOT/'evidence/live'
OUT.mkdir(parents=True,exist_ok=True)
def run(body):
    result=a.code(body)
    return result
def capture(label):
    path=(OUT/(label+'.png')).as_posix()
    result=run('Canvas.ForceUpdateCanvases();UnityEngine.ScreenCapture.CaptureScreenshot('+json.dumps(path)+''');return new {utc=System.DateTime.UtcNow.ToString("o"),scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,screen=new[]{Screen.width,Screen.height},language=Wildfolk.L.Language,labels=UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Text>(UnityEngine.FindObjectsSortMode.None).Where(t=>t.isActiveAndEnabled&&!string.IsNullOrEmpty(t.text)).Select(t=>new {name=t.name,text=t.text,size=t.fontSize,pixelFont=t.fontSize*t.transform.lossyScale.y,width=t.rectTransform.rect.width,height=t.rectTransform.rect.height,preferredHeight=t.preferredHeight,overflow=t.verticalOverflow.ToString()}).ToArray(),controls=UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Selectable>(UnityEngine.FindObjectsSortMode.None).Where(b=>b.gameObject.activeInHierarchy).Select(b=>new {name=b.name,type=b.GetType().Name,enabled=b.IsInteractable(),width=((RectTransform)b.transform).rect.width,height=((RectTransform)b.transform).rect.height,pixelWidth=((RectTransform)b.transform).rect.width*b.transform.lossyScale.x,pixelHeight=((RectTransform)b.transform).rect.height*b.transform.lossyScale.y}).ToArray()};''')
    (OUT/(label+'.json')).write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf8')
    time.sleep(.5)
    return {'label':label,'exists':Path(path).exists(),'screen':result.get('screen')}
if __name__=='__main__':
    if sys.argv[1]=='code': print(json.dumps(run(sys.argv[2]),ensure_ascii=True))
    elif sys.argv[1]=='shot':print(json.dumps(capture(sys.argv[2])))
