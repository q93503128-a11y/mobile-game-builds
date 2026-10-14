using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
namespace Wildfolk
{
    [Serializable]public sealed class LanguageInfo {public string Id,Name;}
    [Serializable]public sealed class LocalizedEntry {public string Key;public string[] Values;}
    [Serializable]public sealed class LanguageCatalog {public LanguageInfo[] Languages;public LocalizedEntry[] Entries;}
    public static class L
    {
        private static LanguageCatalog catalog;
        private static readonly Dictionary<string,LocalizedEntry> entries=new Dictionary<string,LocalizedEntry>();
        private static int language;
        public static event Action Changed;
        public static string Language=>Catalog.Languages[language].Id;
        public static LanguageCatalog Catalog {get{if(catalog==null){catalog=JsonUtility.FromJson<LanguageCatalog>(Resources.Load<TextAsset>("Localization").text);entries.Clear();foreach(var entry in catalog.Entries)entries.Add(entry.Key,entry);}return catalog;}}
        public static bool Has(string key){var data=Catalog;return entries.ContainsKey(key);}
        public static string Text(string key){var data=Catalog;if(!entries.TryGetValue(key,out var e)){Debug.LogError("Missing localization key: "+key);return "…";}return e.Values[language];}
        public static string Format(string key,params object[] values)=>string.Format(CultureInfo.InvariantCulture,Text(key),values);
        public static void SetLanguage(string id)
        {var data=Catalog;var next=Array.FindIndex(data.Languages,x=>x.Id==id);if(next<0)next=0;if(next==language)return;language=next;Changed?.Invoke();}
    }
}
