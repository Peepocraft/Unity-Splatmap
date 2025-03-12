using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.EditorTools;

[CustomEditor(typeof(UnitySplatmapAssigner))]
public class UnitySplatmapAssignerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        UnitySplatmapAssigner t = (UnitySplatmapAssigner)target;

        if (GUILayout.Button("Apply Terrain Splatmaps"))
        {
            t.AssignSplat();
        }
    }
}


