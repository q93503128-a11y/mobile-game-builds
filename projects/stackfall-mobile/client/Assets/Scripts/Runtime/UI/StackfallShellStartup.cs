using UnityEngine;
using UnityEngine.UIElements;

namespace StackfallMobile.Runtime.UI
{
    public sealed partial class StackfallShellController
    {
        public void ShowStartup()
        {
            var root = BeginScreen(false);
            root.style.backgroundColor = new Color(0.004f, 0.01f, 0.028f, 1f);
            AddSpaceField(root);

            var center = new VisualElement();
            center.style.flexGrow = 1;
            center.style.alignItems = Align.Center;
            center.style.justifyContent = Justify.Center;
            Pad(center, IsCompactDevice ? 34 : 60, 36);
            root.Add(center);

            var mark = Card(new Color(0.025f, 0.12f, 0.19f, 0.88f));
            mark.style.width = IsCompactDevice ? 132 : 160;
            mark.style.height = IsCompactDevice ? 132 : 160;
            mark.style.alignItems = Align.Center;
            mark.style.justifyContent = Justify.Center;
            SetRadius(mark, IsCompactDevice ? 66 : 80);
            mark.Add(ShipVisualFactory.BuildUiShip(IsCompactDevice ? 88f : 108f));
            center.Add(mark);

            var title = Label("STACKFALL", IsCompactDevice ? 42 : 52, FontStyle.Bold);
            title.style.marginTop = 26;
            center.Add(title);
            center.Add(Label("ORBITAL SURVIVAL SYSTEM", IsCompactDevice ? 13 : 15, FontStyle.Bold, Accent));

            var line = new VisualElement();
            line.style.width = Length.Percent(IsCompactDevice ? 58f : 46f);
            line.style.height = 3;
            line.style.marginTop = 28;
            line.style.backgroundColor = new Color(Accent.r, Accent.g, Accent.b, 0.72f);
            center.Add(line);
        }
    }
}
