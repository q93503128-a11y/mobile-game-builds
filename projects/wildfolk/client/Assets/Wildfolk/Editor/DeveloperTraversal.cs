using System;
using UnityEngine;
namespace Wildfolk.Editor
{
    public static class DeveloperTraversal
    {
        public static DeveloperMovement Set(int multiplier)
        {
            if(multiplier!=1&&multiplier!=4&&multiplier!=10&&multiplier!=25)throw new ArgumentOutOfRangeException(nameof(multiplier));
            var player=DeveloperHarness.Context().Player;
            if(player.GetComponent<NetworkPlayer>()!=null)throw new InvalidOperationException("QA speed requires local single-player; remote authority is unchanged.");
            var tool=player.GetComponent<DeveloperMovement>();if(tool==null)tool=player.gameObject.AddComponent<DeveloperMovement>();tool.hideFlags=HideFlags.DontSave;tool.Configure(multiplier);return tool;
        }
    }
}
