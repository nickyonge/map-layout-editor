using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DataManager : DataDownloader
{


    public void LoadAllDataFiles()
    {

        string path = Path.Combine(Application.dataPath, "Data");

        List<string> files = Directory
            // .GetFiles(path, "*.*", SearchOption.AllDirectories)
            .EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
            .Where(file =>
                file.ToLower().EndsWith("csv") ||
                file.ToLower().EndsWith("json") ||
                file.ToLower().EndsWith("xml")
                ).ToList();

        for (int i = 0; i < files.Count; i++) {
            files[i] = files[i].Substring(files[i].LastIndexOf("\\") + 1);
            files[i] = i.ToString() + " " + files[i];
        }

            Debug.Log("Filecount: " + files.Count);
        Debug.Log("Files:\n" + string.Join("\n", files));

    }

}
