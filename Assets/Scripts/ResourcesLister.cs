using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using UnityEditor;

public class ResourcesLister: MonoBehaviour
{
    public class DirWithFiles
    {
        public string name;
        public string dirPath;
        public List<DirWithFiles> childs;
        public List<string> filenames;

        public DirWithFiles(string _name, string _dirPath, bool hasSubdir = false)
        {
            name = _name;
            dirPath = _dirPath;
            childs = hasSubdir ? new List<DirWithFiles>() : null;
            filenames = new List<string>();
        }
    }

    private DirWithFiles ListFiles(string targetDir)
    {
        DirectoryInfo dir = new DirectoryInfo(targetDir);
        string[] fileEntries = Directory.GetFiles(targetDir).Where(x => Path.GetExtension(x) != ".meta").ToArray();
        string[] subdirEntries = Directory.GetDirectories(targetDir);
        //FileInfo[] fileInfos = dir.GetFiles();
        Debug.Log("Searching files in " + dir.Name);

        if (fileEntries.Length > 0 || subdirEntries.Length > 0)
        {
            DirWithFiles files = new DirWithFiles(dir.Name, targetDir.Replace("\\", "/"), subdirEntries.Length > 0);
            foreach (string f in fileEntries)
            {
                Debug.Log("Found file " + f + " in " + dir.Name);
                files.filenames.Add(Path.GetFileNameWithoutExtension(f));
            }
            foreach (string subdir in subdirEntries)
            {
                files.childs.Add(ListFiles(subdir));
            }
            return files;
        }
        return null;
        //DirectoryInfo[] directoryInfos = dir.GetDirectories();
    }

    public void SaveAsJSON(string targetDir)
    {
        string json = JsonConvert.SerializeObject(ListFiles(targetDir), Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        File.WriteAllText(Application.dataPath + "/Resources/AssetsFileNames.json", json);
        AssetDatabase.Refresh();
    }
}
