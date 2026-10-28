using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Wildfolk.Editor
{
    public sealed class StoreBuildValidator:IPreprocessBuildWithReport
    {
        public int callbackOrder=>6;
        public static void Validate(List<string> errors){try{var c=StoreCatalog.Load();c.Validate();var s=StoreSettings.Load();if(s.Schema!=1)errors.Add("Store settings schema");if(BackendPolicy.Channel!=BuildChannel.Production&&s.PurchaseEnabled)errors.Add("Non-production real billing forbidden");if(s.PurchaseEnabled&&(!s.Allows(BackendPolicy.Channel)||c.Proposed||!BackendSettings.Current.Enabled||!BackendPolicy.ValidWeb(BackendSettings.Current.PrivacyUrl)||!BackendPolicy.ValidWeb(BackendSettings.Current.DeletionWebUrl)))errors.Add("Store activation setup incomplete");var file="Assets/Plugins/Android/LowfireBilling.androidlib/build.gradle";if(!File.Exists(file)||!File.ReadAllText(file).Contains("com.android.billingclient:billing:9.1.0"))errors.Add("Pinned Billing9.1.0 library missing");}catch{errors.Add("Store catalog/settings unavailable");}}
        public void OnPreprocessBuild(BuildReport report){var errors=new List<string>();Validate(errors);if(errors.Count>0)throw new BuildFailedException(string.Join("; ",errors));}
    }
}
