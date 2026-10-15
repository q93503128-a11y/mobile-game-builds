using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Wildfolk.Tests
{
    internal sealed class PlayPointerDriver : IDisposable
    {
        private readonly Mouse mouse;
        private readonly List<InputDevice> disabled = new List<InputDevice>();
        private readonly InputSettings.BackgroundBehavior oldBackground;
#if UNITY_EDITOR
        private readonly InputSettings.EditorInputBehaviorInPlayMode oldEditor;
#endif
        internal PlayPointerDriver()
        {
            oldBackground=InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            oldEditor=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            foreach(var device in InputSystem.devices)
                if(device.native && device.enabled && (device is UnityEngine.InputSystem.Pointer || device is Keyboard))
                {disabled.Add(device);InputSystem.DisableDevice(device);}
            mouse=InputSystem.AddDevice<Mouse>();
        }
        internal static Vector2 Point(Component target)=>RectTransformUtility.WorldToScreenPoint(null,target.transform.position);
        internal void Pointer(Vector2 position,bool down)
        {
            var state=new MouseState {position=position};
            if(down) state=state.WithButton(MouseButton.Left);
            InputSystem.QueueStateEvent(mouse,state);
        }
        internal IEnumerator Click(Component target)
        {
            var point=Point(target);Pointer(point,true);yield return null;
            Pointer(point,false);yield return null;yield return null;
        }
        internal IEnumerator Inventory(GameplayHud hud)=>Click(hud.Session.State==SessionState.Inventory ? WorldTestScene.ModalCloseButton() : hud.InventoryButton);
        internal IEnumerator Drag(Component source,Vector2 destination,Action inspect=null)
        {
            var start=Point(source);Pointer(start,true);yield return null;
            Pointer(start+new Vector2(20,20),true);yield return null;yield return null;
            inspect?.Invoke();
            for(var i=1;i<=5;i++){Pointer(Vector2.Lerp(start,destination,i/5f),true);yield return null;}
            Pointer(destination,false);yield return null;yield return null;
        }
        public void Dispose()
        {
            InputSystem.RemoveDevice(mouse);
            foreach(var device in disabled) if(device.added) InputSystem.EnableDevice(device);
            InputSystem.settings.backgroundBehavior=oldBackground;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode=oldEditor;
#endif
        }
    }
}
