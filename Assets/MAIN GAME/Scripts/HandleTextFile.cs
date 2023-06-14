using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
public class HandleTextFile: MonoBehaviour
{
    public List<Transform> listMaps = new List<Transform>();
    public string newName;
    public int currentLevel;

    void OnEnable()
    {
        for (int i = 0; i < listMaps.Count; i++)
        {
            newName = "lv" + i.ToString() + ".txt";
            var map = listMaps[i];
            WriteString(map);
        }
    }

    //[MenuItem("Tools/Write file")]
    public void WriteString(Transform map)
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, newName);

        //Write some text to the maps.txt file
        StreamWriter writer = new StreamWriter(path, true);
        
        foreach (Transform child in map)
        {
            string color = ColorUtility.ToHtmlStringRGB(child.GetComponent<Renderer>().sharedMaterial.color).ToString();
            writer.WriteLine(child.position.x + " " + child.position.y + " " + (child.position.z - 12) + " " + color + " " + child.transform.localScale.x);
            //writer.WriteLine("\n");
        }
        writer.Close();

        //Re-import the file to update the reference in the editor
        AssetDatabase.ImportAsset(path);
        TextAsset asset = Resources.Load<TextAsset>(path);
    }


    public void ReadString()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, newName);

        //Read the text from directly from the maps.txt file
        StreamReader reader = new StreamReader(path);
        Debug.Log(reader.ReadToEnd());
        reader.Close();
    }   
}
#endif