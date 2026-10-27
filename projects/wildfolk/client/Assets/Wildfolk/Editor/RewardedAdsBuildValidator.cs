using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
namespace Wildfolk.Editor
{
    public sealed class RewardedAdsBuildValidator:IPreprocessBuildWithReport
    {
        public int callbackOrder=>5;
        public static List<string> Issues(RewardedSettings s,BuildChannel channel){var e=new List<string>();if(s.Schema!=1)e.Add("Rewarded configuration schema");if(!s.Enabled)return e;if(!s.PrivacyReviewed||s.Audience!="General"&&s.Audience!="Child")e.Add("Ads audience/privacy review required");if(channel==BuildChannel.Production){if(!s.ProductionLiveEnabled)e.Add("Production ads explicitly disabled");if(!RewardedSettings.ValidUnit(s.GemAdUnit)||!RewardedSettings.ValidUnit(s.ReviveAdUnit)||s.GemAdUnit==s.ReviveAdUnit)e.Add("Two real distinct AdMob rewarded units required");}return e;}
        public void OnPreprocessBuild(BuildReport report){var s=RewardedSettings.Load();var errors=Issues(s,BackendPolicy.Channel);if(s.Enabled&&BackendPolicy.Channel==BuildChannel.Production){var asset=File.ReadAllText("Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset");if(asset.Contains("ca-app-pub-3940256099942544~3347511713"))errors.Add("Production AdMob app ID required");}if(errors.Count>0)throw new BuildFailedException(string.Join("; ",errors));}
    }
}
