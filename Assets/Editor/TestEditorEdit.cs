using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MyComponent))]
public class TestEditorEdit : Editor
{
    public override void OnInspectorGUI()
    {
        MyComponent myComponent = (MyComponent)target;

        myComponent.myInt = EditorGUILayout.IntField("My Int", myComponent.myInt);
        myComponent.myFloat = EditorGUILayout.FloatField("My Float", myComponent.myFloat);
        myComponent.myString = EditorGUILayout.TextField("My String", myComponent.myString);

        if (GUILayout.Button("Reset Values"))
        {
            myComponent.myInt = 0;
            myComponent.myFloat = 0.0f;
            myComponent.myString = string.Empty;
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(myComponent);
        }
    }
}