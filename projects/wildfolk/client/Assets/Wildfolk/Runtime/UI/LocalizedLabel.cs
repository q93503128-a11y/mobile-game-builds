using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    [RequireComponent(typeof(Text))]
    public sealed class LocalizedLabel : MonoBehaviour
    {
        public string Key;
        private void OnEnable(){L.Changed+=Refresh;Refresh();}
        private void OnDisable()=>L.Changed-=Refresh;
        private void OnDestroy()=>L.Changed-=Refresh;
        public void Refresh(){if(this==null){L.Changed-=Refresh;return;}if(!string.IsNullOrEmpty(Key))GetComponent<Text>().text=L.Text(Key);}
    }
}
