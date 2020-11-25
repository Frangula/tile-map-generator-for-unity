using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ResourcesLister))]
public class ResourcesListerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ResourcesLister lister = (ResourcesLister)target;
        if (GUILayout.Button("Save"))
        {
            lister.SaveAsJSON("Assets/Resources");
        }
    }
}
