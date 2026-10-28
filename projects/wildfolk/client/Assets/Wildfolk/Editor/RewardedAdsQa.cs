using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Editor
{
    public static class RewardedAdsQa
    {
        [Serializable]private sealed class Result{public bool Passed;public string[] Errors;}
        public static void Validate()
        {
            var target=Environment.GetEnvironmentVariable("LOWFIRE_BACKEND_RESULTS");if(string.IsNullOrEmpty(target)||!Application.isBatchMode)throw new InvalidOperationException("Explicit batch QA output required");
            var errors=ProjectDataValidator.Validate();EconomyDataValidator.Validate(errors);
            var settings=RewardedSettings.Load();
            if(settings.Enabled||settings.ProductionLiveEnabled||settings.PrivacyReviewed)errors.Add("Committed advertisement policy must remain disabled until reviewed setup");
            if(settings.Unit(AdPlacement.Gem)!=RewardedSettings.TestUnit||settings.Unit(AdPlacement.Revive)!=RewardedSettings.TestUnit)errors.Add("Development/Alpha official sample guard changed");
            var alpha=new RewardedSettings{Enabled=true,PrivacyReviewed=true,Audience="General"};if(RewardedAdsBuildValidator.Issues(alpha,BuildChannel.Alpha).Count!=0)errors.Add("Reviewed Alpha sample configuration should pass");
            if(RewardedAdsBuildValidator.Issues(alpha,BuildChannel.Production).Count==0)errors.Add("Accidental Production activation accepted");
            Directory.CreateDirectory(target);File.WriteAllText(Path.Combine(target,"data-validator.json"),JsonUtility.ToJson(new Result{Passed=errors.Count==0,Errors=errors.ToArray()},true));
            if(errors.Count>0)throw new InvalidOperationException(string.Join("\n",errors));
        }
    }
}
