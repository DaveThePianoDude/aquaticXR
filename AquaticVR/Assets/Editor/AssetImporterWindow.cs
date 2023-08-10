using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Text;

public class AssetImporterWindow : EditorWindow
{
    [MenuItem("Window/Asset Importer")]
    static void OpenWindow()
    {
        var window = ScriptableObject.CreateInstance<AssetImporterWindow>();
        window.title = "Asset Importer";
        window.Show();
    }
    public Object destinationFolder;
    public Object templateAsset;
    public string assetsPaths = "\n";
    private string newAssetPath = null;
    Vector2 scrollPosition = new Vector2();

    void OnGUI()
    {
        if (Event.current.type == EventType.Layout)
        {
            if (newAssetPath != null)
            {
                GUIUtility.keyboardControl = 0;
                GUIUtility.hotControl = 0;
                assetsPaths = newAssetPath;
                newAssetPath = null;
            }
            if (assetsPaths == null)
            {
                assetsPaths = "\n";
            }
            else
            {
                if (!assetsPaths.EndsWith("\n"))
                {
                    assetsPaths = assetsPaths + "\n";
                }
            }
            if (assetsPaths.IndexOf('\\') >= 0)
            {
                assetsPaths = assetsPaths.Replace('\\', '/');
            }
            if (destinationFolder != null)
            {
                string destpath = AssetDatabase.GetAssetPath(destinationFolder);
                if (string.IsNullOrEmpty(destpath))
                {
                    destinationFolder = null;
                }
                else if (!System.IO.Directory.Exists(destpath))
                {
                    destpath = destpath.Substring(0, destpath.LastIndexOf('/'));
                    destinationFolder = AssetDatabase.LoadMainAssetAtPath(destpath);
                }
            }
        }

        GUILayout.BeginHorizontal("Toolbar"); GUILayout.Label("");  GUILayout.EndHorizontal();

        templateAsset = EditorGUILayout.ObjectField("Template Asset", templateAsset, typeof(Object), false);
        destinationFolder = EditorGUILayout.ObjectField("Destination Folder", destinationFolder, typeof(Object), false);

        GUILayout.Label("Assets to be imported (Drag and drop, or write full path)");
        GUILayout.Space(2);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        assetsPaths = EditorGUILayout.TextArea(assetsPaths);
        Event evt = Event.current;
        Rect drop_area = GUILayoutUtility.GetLastRect();
        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!drop_area.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    StringBuilder sb = new StringBuilder();
                    foreach (var path in assetsPaths.Split('\n', '\r'))
                    {
                        if (string.IsNullOrEmpty(path)) continue;
                        sb.AppendFormat("{0}\n", path.ToString());
                    }
                    foreach (var path in DragAndDrop.paths)
                    {
                        if (string.IsNullOrEmpty(path)) continue;
                        sb.AppendFormat("{0}\n", path.ToString());
                    }
                    newAssetPath = sb.ToString();
                }
                break;
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Import"))
        {
            var start = System.DateTime.Now;
            string destpath;
            if( destinationFolder != null )
            {
                destpath = AssetDatabase.GetAssetPath(destinationFolder);
            }
            else
            {
                destpath = AssetDatabase.GetAssetPath(templateAsset);
                destpath = destpath.Substring(0,destpath.LastIndexOf('/'));
            }

            List<Object> assets = new List<Object>();

            foreach (var assetPath in assetsPaths.Split('\n', '\r'))
            {
                if (string.IsNullOrEmpty(assetPath)) continue;

                var assetDest = destpath + assetPath.Substring(assetPath.LastIndexOf('/'));
                assetDest = AssetDatabase.GenerateUniqueAssetPath(assetDest);
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(templateAsset.GetInstanceID()), assetDest);
                System.IO.File.Copy(assetPath, assetDest, true);
                //AssetDatabase.ImportAsset(assetDest);
                assets.Add(AssetDatabase.LoadAssetAtPath(assetDest, templateAsset.GetType()));
            }
            AssetDatabase.SaveAssets();
           
            Selection.instanceIDs = new int[0];
            Selection.objects = assets.ToArray();

            AssetDatabase.Refresh();
            Debug.Log(string.Format("Asset Importer: Importing {0} assets took {1} seconds", assets.Count,(System.DateTime.Now - start).TotalSeconds));
        }
        GUILayout.Space(6);
    }
}
