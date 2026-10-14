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
        public void Refresh(){if(!string.IsNullOrEmpty(Key))GetComponent<Text>().text=L.Text(Key);}
    }
}
