using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
namespace Wildfolk
{
    // Separate account cache: never materialize cloud data over the guest's original Profile2 file.
    public sealed class AccountCloudCache:IAccountProgressionStore
    {
        [Serializable]private sealed class Selection {public string Player,Environment;}
        public CloudAccountProfile Profile{get;private set;}public bool Dirty=>File.Exists(dirtyPath);
        public event Action Changed;private LocalAccountProgressionStore store;private string directory,dirtyPath;
        private static string Root=>Path.Combine(Path.GetDirectoryName(SaveRepository.Default().Path),"CloudAccounts");
        private static string Key(string value){using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-","").ToLowerInvariant();}
        private static string SelectionPath=>Path.Combine(Root,"selected.json");
        public static AccountCloudCache Open(string player,string environment)
        {var dir=Path.Combine(Root,Key(environment+":"+player));var c=new AccountCloudCache{directory=dir,dirtyPath=Path.Combine(dir,"dirty")};c.store=new LocalAccountProgressionStore(dir);var f=Path.Combine(dir,"cloud.json");if(File.Exists(f)){c.Profile=JsonUtility.FromJson<CloudAccountProfile>(File.ReadAllText(f));c.Profile.Validate(player);}return c;}
        public static AccountCloudCache Restore()
        {try{if(!File.Exists(SelectionPath))return null;var s=JsonUtility.FromJson<Selection>(File.ReadAllText(SelectionPath));if(s.Environment!=BackendSettings.Current.Environment.Name)return null;var cache=Open(s.Player,s.Environment);return File.Exists(Path.Combine(cache.directory,"deletion-request"))?null:cache;}catch(Exception e)when(e is IOException||e is UnauthorizedAccessException||e is ArgumentException||e is InvalidOperationException){return null;}}
        public void Materialize(CloudAccountProfile profile,bool discardLocal=false)
        {
            profile.Validate(profile.PlayerId);if(Dirty&&!discardLocal)throw new AccountConflictException("cloud_conflict");
            Directory.CreateDirectory(directory);
            if(discardLocal){var existing=Path.Combine(directory,"class-"+ClassProfiles.LocalId+".json");if(File.Exists(existing))File.Copy(existing,existing+".archive-"+DateTime.UtcNow.Ticks);}
            if(!store.Write(ClassProfiles.LocalId,profile.ClassProfile.Local()))throw new IOException("Account cache unavailable");
            AtomicText(Path.Combine(directory,"cloud.json"),JsonUtility.ToJson(profile));Profile=profile;if(File.Exists(dirtyPath))File.Delete(dirtyPath);
            AtomicText(SelectionPath,JsonUtility.ToJson(new Selection{Player=profile.PlayerId,Environment=BackendSettings.Current.Environment.Name}));Changed?.Invoke();
        }
        private static void AtomicText(string path,string text)
        {Directory.CreateDirectory(Path.GetDirectoryName(path));var bytes=Encoding.UTF8.GetBytes(text);using(var s=new FileStream(path+".temporary",FileMode.Create,FileAccess.Write,FileShare.None)){s.Write(bytes,0,bytes.Length);s.Flush(true);}if(File.Exists(path))File.Replace(path+".temporary",path,path+".backup");else File.Move(path+".temporary",path);}
        public AccountProgression Read(string profile)=>store.Read(profile);
        public bool Write(string profile,AccountProgression data)
        {try{AtomicText(dirtyPath,"pending class sync");if(!store.Write(profile,data))return false;Changed?.Invoke();return true;}catch(Exception e)when(e is IOException||e is UnauthorizedAccessException||e is ArgumentException||e is InvalidOperationException){return false;}}
        public static void Deselect(){if(File.Exists(SelectionPath))File.Delete(SelectionPath);ClassProfiles.AttachCloud(null);}
        public void ClearForDeletion(string requestId)
        {
            if(string.IsNullOrEmpty(requestId)||Profile==null)throw new InvalidOperationException("Deletion receipt required");
            var root=Path.GetFullPath(Root)+Path.DirectorySeparatorChar;var target=Path.GetFullPath(directory);
            if(!target.StartsWith(root,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Invalid account cache path");
            AtomicText(Path.Combine(directory,"deletion-request"),requestId);
            foreach(var suffix in new[]{"",".backup",".temporary"}){
                var path=SelectionPath+suffix;if(!File.Exists(path))continue;var selection=JsonUtility.FromJson<Selection>(File.ReadAllText(path));
                if(selection.Player==Profile.PlayerId&&selection.Environment==BackendSettings.Current.Environment.Name)File.Delete(path);
            }
            ClassProfiles.AttachCloud(null);Directory.Delete(target,true);Profile=null;
        }
    }
}
