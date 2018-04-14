 using System;
 using UnityEngine;
 using System.Collections;
 using System.IO;
 using System.Security.Permissions;
 using UnityEngine.Events;

[System.Serializable]
public class NewFishEvent : UnityEvent<string>
{
}

public class FileWatcher : MonoBehaviour
{
    public string fileToWatch = "*.png";
    private FileSystemWatcher watcher;
    public NewFishEvent NewFishFromFileEvent;

    // public GameObject FishManager;
       
    void Start ()
    {
        #if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        Environment.SetEnvironmentVariable("MONO_MANAGED_WATCHER", "enabled");
        #endif

    }

    void Update (){
        watcher = new FileSystemWatcher ();
        watcher.Path = Application.streamingAssetsPath;
        watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.DirectoryName
                | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite
                | NotifyFilters.Size;
        watcher.Filter = fileToWatch;
        watcher.Renamed += new RenamedEventHandler (OnRenamed);
        watcher.Changed += new FileSystemEventHandler (OnChanged);
        watcher.Created += new FileSystemEventHandler (OnChanged);
        watcher.Deleted += new FileSystemEventHandler (OnChanged);
        watcher.Error += new ErrorEventHandler(OnError);
       
        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;
    }
   
    private void OnChanged (object source, FileSystemEventArgs e)
    {
        WatcherChangeTypes wct = e.ChangeType;
        Debug.Log (e.FullPath + ": " + wct.ToString ());

        if(wct.ToString() == "Created"){
            NewFishFromFileEvent.Invoke(e.FullPath);
        }
    }
   
    private void OnRenamed (object source, RenamedEventArgs e)
    {
        WatcherChangeTypes wct = e.ChangeType;
        Debug.Log ("file " + e.OldFullPath + " renamed to " + e.FullPath + ": " + wct.ToString ());
    }
   
    private void OnError(object source, ErrorEventArgs e)
    {
        Debug.Log("Error detected: " + e.GetException().GetType().ToString());
    }
}