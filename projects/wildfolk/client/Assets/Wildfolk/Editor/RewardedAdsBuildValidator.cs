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
        public static List<string> Issues(RewardedSettings s,BuildChannel channel){var e=new List<string>();if(s.Schema!=1)e.Add("Rewarded configuration schema");if(!s.Enabled)return e;if(!s.PrivacyReviewed||s.Audience!="General"&&s.Audience!="Child")e.Add("Ads audience/privacy review required");if(channel!=BuildChannel.Production&&s.UseRegisteredTestUnits&&(!s.ValidOwnedUnits||!s.ValidTestDevices))e.Add("Owned test units and registered device IDs required");if(channel==BuildChannel.Production){if(!s.ProductionLiveEnabled)e.Add("Production ads explicitly disabled");if(!RewardedSettings.ValidUnit(s.GemAdUnit)||!RewardedSettings.ValidUnit(s.ReviveAdUnit)||s.GemAdUnit==s.ReviveAdUnit)e.Add("Two real distinct AdMob rewarded units required");}return e;}
        public static void Validate(List<string> errors){try{var s=RewardedSettings.Load();errors.AddRange(Issues(s,BackendPolicy.Channel));if(!File.Exists("Assets/GoogleMobileAds/Editor/GoogleMobileAdsDependencies.xml")||!File.Exists("Assets/Plugins/Android/googlemobileads-unity.aar"))errors.Add("AdMob native dependency assets missing");}catch{errors.Add("Rewarded settings unavailable");}}
        public void OnPreprocessBuild(BuildReport report){var s=RewardedSettings.Load();var errors=Issues(s,BackendPolicy.Channel);if(s.Enabled&&(BackendPolicy.Channel==BuildChannel.Production||s.UsesOwnedTestUnits)){var asset=File.ReadAllText("Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset");if(asset.Contains("ca-app-pub-3940256099942544~3347511713"))errors.Add("Owned-unit AdMob app ID required");}if(errors.Count>0)throw new BuildFailedException(string.Join("; ",errors));}
    }
}
