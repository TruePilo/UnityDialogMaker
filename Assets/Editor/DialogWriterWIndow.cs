using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using System.Linq;

public class DialogWriterWIndow : EditorWindow
{
    // Window visuals
    private Color gridColor = new Color(0.25f, 0.25f, 0.25f);
    private Color bgColor = new Color(0.05f, 0.05f, 0.05f);
    private Color upperMenuColor = new Color(0.235f, 0.235f, 0.235f);
    private Color upperMenuBtnColor = new Color(0.318f, 0.318f, 0.318f);
    private Color connectionColor = new Color(0.85f, 0.85f, 0.85f, 1.0f);

    private int lastID = 0;

    // Dialog nodes
    public List<DialogNode> nodes;
    private Vector2 offset;
    private Vector2 drag;

    // Window functionality
    public DialogNode connectingTo = null;
    private string fileName = "default";
    private string jsonContent = "{}";
    private string folderPath = "/Dialogs";
    private string[] jsonFiles; // Array to store JSON file names
    private int selectedFileIndex; // Index of the selected file in the dropdown
    private string selectedFileName;


    [MenuItem("Custom Tools/Dialog Writer")]
    private static void OpenWindow()
    {
        DialogWriterWIndow window = GetWindow<DialogWriterWIndow>();
        window.titleContent = new GUIContent("Dialog Writer");
    }

    private void OnEnable()
    {
        nodes = new List<DialogNode>();
    }

    private void OnGUI()
    {

        // Uložení původní barvy pozadí
        Color originalBackgroundColor = GUI.backgroundColor;

        // Set background color
        GUI.backgroundColor = bgColor;
        GUI.Box(new Rect(0, 0, position.width, position.height), GUIContent.none);

        // Obnovení původní barvy pozadí
        GUI.backgroundColor = originalBackgroundColor;

        // Draw grid
        DrawGrid(20, 0.2f, gridColor);
        DrawGrid(100, 0.4f, gridColor);

        // Draw nodes
        DrawConnections();
        DrawNodes();

        // Draw upper menu
        EditorGUI.DrawRect(new Rect(0, 0, position.width, 25), upperMenuColor);

        // Přidání textového vstupu a tlačítka na horní lištu okna
        GUILayout.BeginArea(new Rect(10, 2.5f, position.width - 20, 20));
        GUILayout.BeginHorizontal();
        GUILayout.Label("File Name:", GUILayout.Width(70));
        fileName = GUILayout.TextField(fileName, GUILayout.Width(200));
        if (GUILayout.Button("Save Dialog", GUILayout.Width(100)))
        {
            CreateJsonFile();
        }

        GUILayout.FlexibleSpace(); // Přidání flexibilního prostoru

        // Dropdown menu for JSON files
        LoadJsonFileNames();
        if (jsonFiles.Length > 0)
        {
            selectedFileIndex = EditorGUILayout.Popup(selectedFileIndex, jsonFiles, GUILayout.Width(200));
            selectedFileName = jsonFiles[selectedFileIndex];
        }

        if (GUILayout.Button("Load Dialog", GUILayout.Width(100)))
        {
            LoadJsonFile();
        }

        GUILayout.FlexibleSpace(); // Přidání flexibilního prostoru
        if (GUILayout.Button("New Dialog", GUILayout.Width(100)))
        {
            CreateNewDialog();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        ProcessNodeEvents(Event.current);
        ProcessEvents(Event.current);

        GUI.changed = true;
        if (GUI.changed) Repaint();
    }

    private void LoadJsonFileNames()
    {
        if (Directory.Exists(Application.dataPath + folderPath))
        {
            jsonFiles = Directory.GetFiles(Application.dataPath + folderPath, "*.json").Select(Path.GetFileNameWithoutExtension).ToArray();
        }
        else
        {
            jsonFiles = new string[0];
        }
    }

    private void CreateJsonFile()
    {
        jsonContent = "[\n";

        foreach (DialogNode node in nodes)
        {
            jsonContent += JsonUtility.ToJson(node.getJson(), true);
            //jsonContent += node.getJson();
            if (node != nodes[nodes.Count - 1])
            {
                jsonContent += ",\n";
            }
        }

        jsonContent += "\n]";

        if (!Directory.Exists(Application.dataPath + folderPath))
        {
            Directory.CreateDirectory(Application.dataPath + folderPath);
        }

        string path = Path.Combine(Application.dataPath + folderPath, fileName + ".json");
        File.WriteAllText(path, jsonContent);
        AssetDatabase.Refresh();
        Debug.Log($"Dialog file created at: {path}");
    }

    private void LoadJsonFile()
    {
        nodes.Clear();
        lastID = 0;

        string path = Path.Combine(Application.dataPath + folderPath, selectedFileName + ".json");
        if (File.Exists(path))
        {
            jsonContent = File.ReadAllText(path);

            // oříznutí
            jsonContent = jsonContent.Substring(2, jsonContent.Length - 3);

            // definuje pattern k rozdělení na - },\n{
            string pattern = @"(?<=}\s*),\s*(?=\{)";
            string[] jsonStringSplit = Regex.Split(jsonContent, pattern);

            foreach (string jsonBit in jsonStringSplit)
            {
                DialogJsonObject loadedObject = JsonUtility.FromJson<DialogJsonObject>(jsonBit);
                DialogNode loadedNode = new DialogNode(loadedObject);
                nodes.Add(loadedNode);

                if(loadedNode.id > lastID)
                {
                    lastID = loadedNode.id;
                }
            }

            if(nodes.Count != 0)
            {
                lastID++;
            }


            // loads connections
            foreach (DialogNode node in nodes)
            {
                foreach (int loadedID in node.loadedIDs)
                {
                    foreach (DialogNode node2 in nodes)
                    {
                        if (node2.id == loadedID)
                        {
                            node.connectedNodes.Add(node2);
                        }
                    }
                }
            }

            Debug.Log($"Dialog file loaded from: {path}");
            //Debug.Log($"Number of loaded nodes: {loadedObjects.Length}");
        }
        else
        {
            Debug.LogError($"Dialog file not found at: {path}");
        }
    }

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        offset += drag * 0.5f;
        Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
        }

        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    private void DrawNodes()
    {
        if (nodes != null)
        {
            foreach (DialogNode node in nodes)
            {
                node.Draw();
            }
        }
    }

    private void DrawConnections()
    {
        foreach (DialogNode node in nodes)
        {
            foreach (DialogNode connectedNode in node.connectedNodes)
            {
                Handles.BeginGUI();

                // Set the color for the connection line
                Handles.color = connectionColor;

                // Convert Vector2 to Vector3 for the line drawing
                Vector3 start = (Vector3)connectedNode.rect.center;
                Vector3 end = (Vector3)node.rect.center;

                // Calculate direction for arrow drawing
                Vector3 direction = (end - start).normalized;
                float arrowLength = 7.5f; // Length of each arrow segment
                float arrowSpacing = 10f; // Distance between arrows

                // Calculate the number of arrows to draw
                float distance = Vector3.Distance(start, end);
                int numArrows = Mathf.FloorToInt(distance / arrowSpacing);

                for (int i = 1; i < numArrows; i++)
                {
                    // Calculate the position for each arrow
                    Vector3 arrowPos = start + direction * (i * arrowSpacing);

                    // Draw an arrow at the calculated position
                    DrawArrow(arrowPos, direction, arrowLength);
                }

                Handles.EndGUI();
            }
        }

        if (connectingTo != null)
        {
            Handles.BeginGUI();

            // Set the color for the connection line
            Handles.color = connectionColor;

            // Draw a thicker line by using DrawAAPolyLine with the specified thickness
            float lineThickness = 5.0f; // Adjust the thickness as needed
            Handles.DrawAAPolyLine(lineThickness, new Vector3[] { connectingTo.rect.center, Event.current.mousePosition });

            Handles.EndGUI();
        }
    }

    private void DrawArrow(Vector3 position, Vector3 direction, float length)
    {
        // Calculate perpendicular direction for arrow wings
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized;

        // Calculate arrowhead points
        Vector3 left = position - direction * length + perpendicular * (length * 0.5f);
        Vector3 right = position - direction * length - perpendicular * (length * 0.5f);

        // Draw arrowhead lines
        Handles.DrawAAPolyLine(3f, new Vector3[] { position, left });
        Handles.DrawAAPolyLine(3f, new Vector3[] { position, right });
    }

    private void CreateNewDialog()
    {
        nodes.Clear();
        fileName = "default";
        jsonContent = "{}";
        lastID = 0;
        CreateInitialNode();
    }

    private void CreateInitialNode()
    {
        if (nodes == null)
        {
            nodes = new List<DialogNode>();
        }

        DialogNode initialNode = new DialogNode(new Vector2(200, 200), 200, 50, "Start Node", lastID);
        lastID++;
        initialNode.IsDeletable = false; // Nastavení, že node nelze smazat
        initialNode.isStarting = true; // Nastavení, že node je startovní
        nodes.Add(initialNode);
    }

    private void ProcessEvents(Event e)
    {
        drag = Vector2.zero;

        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 1)
                {
                    ProcessContextMenu(e.mousePosition);
                }
                break;

            case EventType.MouseDrag:
                if (e.button == 0)
                {
                    OnDrag(e.delta);
                }
                break;

            case EventType.MouseMove:
                OnMouseMove(e.delta);
                break;
        }
    }

    private void ProcessNodeEvents(Event e)
    {
        if (nodes != null)
        {
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                bool guiChanged = nodes[i].ProcessEvents(e);

                if (guiChanged)
                {
                    GUI.changed = true;
                }
            }
        }
    }

    private void ProcessContextMenu(Vector2 mousePosition)
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Add Dialog Node"), false, () => OnClickAddNode(mousePosition));
        genericMenu.ShowAsContext();
    }

    private void OnClickAddNode(Vector2 mousePosition)
    {
        if (nodes == null)
        {
            nodes = new List<DialogNode>();
        }

        nodes.Add(new DialogNode(mousePosition, 200, 50, "New Node", lastID));
        lastID++;
        //Debug.Log("New node added");
    }

    public void AddNode(DialogNode node)
    {
        if (nodes == null)
        {
            nodes = new List<DialogNode>();
        }

        nodes.Add(new DialogNode(node.getSpawnPosition(), 200, 50, "New Node", node, lastID));
        lastID++;
        //Debug.Log("New node added");
    }

    public void AddConnection(DialogNode node)
    {
        connectingTo = node;
    }

    private void OnMouseMove(Vector2 delta)
    {
        Debug.Log("Mouse moved: " + delta);
        Repaint(); // Ensure the window repaints if needed
    }

    private void OnDrag(Vector2 delta)
    {
        drag = delta;

        if (nodes != null)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Drag(delta);
            }
        }

        GUI.changed = true;
    }

    public void RemoveNode(DialogNode node)
    {
        foreach (DialogNode nodeBig in nodes)
        {
            if (nodeBig.connectedNodes.Contains(node))
            {
                nodeBig.connectedNodes.Remove(node);
            }
        }
        nodes.Remove(node);
    }
}