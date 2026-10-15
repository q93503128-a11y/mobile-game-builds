using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
namespace Wildfolk
{
    public sealed class SaveRepository
    {
        [Serializable]private sealed class Envelope {public string Payload,Checksum;}
        public static SaveRepository Default()
        {
#if UNITY_EDITOR
            var directory=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../UserData"));
#else
            var directory=Application.persistentDataPath;
#endif
            return new SaveRepository(System.IO.Path.Combine(directory,"world-v1.json"));
        }
        public string Path {get;}
        public bool Exists=>File.Exists(Path) || File.Exists(Path+".backup");
        public bool UsedBackup {get;private set;}
        public SaveRepository(string path)=>Path=path;
        private static string Hash(string data){using(var hash=SHA256.Create())return Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(data)));}
        public static string Encode(SavedGame game){var payload=JsonUtility.ToJson(game);return JsonUtility.ToJson(new Envelope{Payload=payload,Checksum=Hash(payload)});}
        public static SavedGame Decode(string data)
        {
            var envelope=JsonUtility.FromJson<Envelope>(data);
            if(envelope==null || string.IsNullOrEmpty(envelope.Payload) || envelope.Checksum!=Hash(envelope.Payload))throw new InvalidDataException("Save checksum mismatch");
            var game=JsonUtility.FromJson<SavedGame>(envelope.Payload);
            if(game==null || game.Version<1 || game.Version>4)throw new InvalidDataException("Unsupported save version");return game;
        }
        public void Write(SavedGame game,Action<SavedGame> validate)
        {
            validate(game);Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));var temporary=Path+".temporary";
            var bytes=Encoding.UTF8.GetBytes(Encode(game));
            using(var stream=new FileStream(temporary,FileMode.Create,FileAccess.Write,FileShare.None)){stream.Write(bytes,0,bytes.Length);stream.Flush(true);}
            validate(Decode(File.ReadAllText(temporary,Encoding.UTF8)));
            if(File.Exists(Path))
            {
                var validPrevious=true;
                try{validate(Decode(File.ReadAllText(Path,Encoding.UTF8)));}catch(Exception error)when(error is InvalidDataException || error is IOException || error is ArgumentException || error is InvalidOperationException){validPrevious=false;}
                File.Replace(temporary,Path,validPrevious ? Path+".backup" : null);
            }
            else File.Move(temporary,Path);
        }
        public SavedGame Read(Action<SavedGame> validate)
        {
            UsedBackup=false;Exception primary=null;
            foreach(var candidate in new[]{Path,Path+".backup"})
            {
                if(!File.Exists(candidate))continue;
                try{var game=Decode(File.ReadAllText(candidate,Encoding.UTF8));validate(game);UsedBackup=candidate!=Path;return game;}
                catch(Exception error)when(error is InvalidDataException || error is IOException || error is ArgumentException || error is InvalidOperationException){primary=error;}
            }
            throw new InvalidDataException("No valid save is available",primary);
        }
    }
}
