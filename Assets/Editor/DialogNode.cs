using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using System;

public class DialogNode
{
    // Node Colors
    private Color nodeBgColor = new Color(0.114f, 0.114f, 0.114f, 1f);
    private Color startNodeBgColor = new Color(0.255f, 0.398f, 0.152f, 1f);
    private Color startNodeBorderColor = new Color(0.316f, 0.493f, 0.188f, 1f);
    private Color nodeBorderColor = new Color(0.22f, 0.22f, 0.22f, 1f);
    private Color nodeInputColor = new Color(0.22f, 0.22f, 0.22f, 0.5f);

    // Node Data
    public string title;
    public string fullText;
    public List<DialogNode> connectedNodes;
    //public DialogNode previousNode;

    // Node Properties
    public int id;
    public bool isDragged;
    public bool isSelected;
    public bool isStarting = false;
    public bool IsDeletable = true;
    public int[] loadedIDs;

    // Node Constructor
    public Rect rect;

    public DialogJsonObject getJson()
    {
        return new DialogJsonObject(this);
    }

    public DialogNode(Vector2 position, float width, float height, string title, int id)
    {
        rect = new Rect(position.x, position.y, width, height);
        this.title = title;
        fullText = "Placeholder text";
        connectedNodes = new List<DialogNode>();
        this.id = id;
    }

    public DialogNode(Vector2 position, float width, float height, string title, DialogNode previousNode, int id)
    {
        rect = new Rect(position.x, position.y, width, height);
        this.title = title;
        fullText = "Placeholder text";
        connectedNodes = new List<DialogNode>();
        this.id = id;
        connectedNodes.Add(previousNode);
    }

    public DialogNode(DialogJsonObject json)
    {
        rect = new Rect(json.position.x, json.position.y, 200, 50);
        title = json.title;
        fullText = json.fullText;
        id = json.id;
        isStarting = json.isStarting;
        IsDeletable = json.isDeletable;
        loadedIDs = json.connectedNodes.ToArray();
        connectedNodes = new List<DialogNode>();
    }

    public Vector2 getSpawnPosition()
    {
        return new Vector2(rect.x + 300, rect.y);
    }

    public void Drag(Vector2 delta)
    {
        rect.position += delta;
    }
    public void Draw()
    {
        // Create a style for the entire box
        GUIStyle boxStyle = new GUIStyle(EditorStyles.helpBox);
        boxStyle.padding = new RectOffset(15, 15, 15, 15);

        // Create a border texture
        Texture2D borderTex = new Texture2D(4, 4);
        Color[] pix = new Color[16];

        // Fill the texture with the border color and transparent center
        for (int i = 0; i < 16; i++)
        {
            if (i < 4 || i >= 12 || i % 4 == 0 || i % 4 == 3)
            {
                if (!isStarting)
                {
                    pix[i] = nodeBorderColor;
                }
                else
                {
                    pix[i] = startNodeBorderColor;
                }
            }
            else
            {
                if (!isStarting)
                {
                    pix[i] = nodeBgColor;
                }
                else
                {
                    pix[i] = startNodeBgColor;
                }
            }
        }

        borderTex.SetPixels(pix);
        borderTex.Apply();

        // Set border
        boxStyle.border = new RectOffset(2, 2, 2, 2); // Adjust the border thickness as needed
        boxStyle.normal.background = borderTex; // Set the border texture as background
        boxStyle.normal.textColor = Color.white; // Optional: Set text color if needed


        // Create a style for the header
        GUIStyle headerStyle = new GUIStyle(GUI.skin.box);
        headerStyle.fontSize = 18; // Adjust the font size as needed
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.normal.textColor = Color.white;
        headerStyle.normal.background = MakeTex(2, 2, nodeInputColor);

        // Create a style for the full text input field
        GUIStyle textStyle = new GUIStyle(GUI.skin.box);
        textStyle.fontSize = 12; // Adjust the font size as needed
        textStyle.wordWrap = true;
        textStyle.alignment = TextAnchor.UpperLeft;
        textStyle.normal.textColor = Color.white;
        textStyle.normal.background = MakeTex(2, 2, nodeInputColor);

        // Calculate the height needed for the full text
        float textHeight = textStyle.CalcHeight(new GUIContent(fullText), rect.width - 20);
        // Adjust the node height to fit the content
        rect.height = 50 + textHeight;
        // Draw the entire box
        GUI.Box(rect, GUIContent.none, boxStyle);

        if (!isStarting)
        {
            title = GUI.TextField(new Rect(rect.x + 10, rect.y + 10, rect.width - 20, 20), title, headerStyle);
        }
        else
        {
            GUI.Box(new Rect(rect.x + 10, rect.y + 10, rect.width - 20, 20), title, headerStyle);
        }
        fullText = GUI.TextField(new Rect(rect.x + 10, rect.y + 40, rect.width - 20, textHeight), fullText, textStyle);
    }

    // Helper method to create a texture with a specific color
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    public bool ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                DialogWriterWIndow window = (DialogWriterWIndow)EditorWindow.GetWindow(typeof(DialogWriterWIndow));
                if (e.button == 1 && window.connectingTo != null)
                {
                    window.connectingTo = null;
                }

                if (e.button == 0)
                {
                    if (rect.Contains(e.mousePosition))
                    {
                        isDragged = true;
                        GUI.changed = true;
                        isSelected = true;
                    }
                    else
                    {
                        GUI.changed = true;
                        isSelected = false;
                    }

                    if (rect.Contains(e.mousePosition) && window.connectingTo != null && window.connectingTo != this)
                    {
                        connectedNodes.Add(window.connectingTo);
                        window.connectingTo = null;
                    }
                }

                if (e.button == 1 && rect.Contains(e.mousePosition))
                {
                    ProcessContextMenu();
                    e.Use();
                }

                break;

            case EventType.MouseUp:
                isDragged = false;
                break;

            case EventType.MouseDrag:
                if (e.button == 0 && isDragged)
                {
                    Drag(e.delta);
                    e.Use();
                    return true;
                }
                break;
        }

        return false;
    }

    private List<DialogNode> GetConnectedNodesReversed()
    {
        DialogWriterWIndow window = (DialogWriterWIndow)EditorWindow.GetWindow(typeof(DialogWriterWIndow));
        List<DialogNode> connectedNodesReversed = new List<DialogNode>();
        foreach (DialogNode node in window.nodes)
        {
            if (node.connectedNodes.Contains(this))
            {
                connectedNodesReversed.Add(node);
            }
        }
        return connectedNodesReversed;
    }

    private void ProcessContextMenu()
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Add Connected Node"), false, OnClickAddNode);
        genericMenu.AddItem(new GUIContent("Add New Connection"), false, OnClickAddConnection);
        genericMenu.AddSeparator("");
        foreach (DialogNode node in connectedNodes)
        {
            genericMenu.AddItem(new GUIContent("Disconnect From " + node.title), false, () => OnClickDisconnect(node));
        }
        foreach (DialogNode node in GetConnectedNodesReversed())
        {
            genericMenu.AddItem(new GUIContent("Disconnect From " + node.title), false, () => OnClickDisconnect(node));
        }
        if (connectedNodes.Count > 0 || GetConnectedNodesReversed().Count > 0)
        {
            genericMenu.AddSeparator("");
        }
        if (IsDeletable)
            genericMenu.AddItem(new GUIContent("Remove Node"), false, OnClickRemoveNode);
        genericMenu.ShowAsContext();
    }

    private void OnClickRemoveNode()
    {
        if (IsDeletable)
        {
            DialogWriterWIndow window = (DialogWriterWIndow)EditorWindow.GetWindow(typeof(DialogWriterWIndow));
            window.RemoveNode(this);
        }
    }

    private void OnClickAddNode()
    {
        DialogWriterWIndow window = (DialogWriterWIndow)EditorWindow.GetWindow(typeof(DialogWriterWIndow));
        window.AddNode(this);
    }

    private void OnClickAddConnection()
    {
        DialogWriterWIndow window = (DialogWriterWIndow)EditorWindow.GetWindow(typeof(DialogWriterWIndow));
        window.AddConnection(this);
    }

    private void OnClickDisconnect(DialogNode node)
    {
        if (connectedNodes.Contains(node))
        {
            connectedNodes.Remove(node);
        }

        if (node.connectedNodes.Contains(this))
        {
            node.connectedNodes.Remove(this);
        }
    }
}

public class DialogJsonObject
{
    public string title;
    public string fullText;
    public int id;
    public Vector2 position;
    public bool isStarting;
    public bool isDeletable;
    public List<int> connectedNodes;

    public DialogJsonObject(DialogNode node)
    {
        title = node.title;
        fullText = node.fullText;
        id = node.id;
        position = node.rect.position;
        isStarting = node.isStarting;
        isDeletable = node.IsDeletable;
        connectedNodes = new List<int>();
        foreach (DialogNode connectedNode in node.connectedNodes)
        {
            connectedNodes.Add(connectedNode.id);
        }
    }
}