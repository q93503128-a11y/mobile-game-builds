using System.Collections.Generic;

namespace StackfallMobile.Runtime.UI
{
    internal sealed class StackfallShellInteractionState
    {
        public readonly HashSet<string> ClaimedMail = new();
        public readonly HashSet<string> ClaimedMissions = new();

        public bool AttendanceClaimed;
        public bool FreeDailySupplyClaimed;
        public int MissionTab;
        public int StoreTab;
        public int PartTab;
        public int SelectedPartIndex;
        public int GraphicsQuality = 2;

        public bool BgmEnabled = true;
        public bool SfxEnabled = true;
        public bool VibrationEnabled = true;
        public bool PowerSaveEnabled;

        public readonly string[] EquippedParts =
        {
            "펄서 코어",
            "바스티온 프레임",
            "벡터 드라이브",
            "절단 엣지",
            "정찰 오비터",
            "가속 리액터"
        };
    }
}
