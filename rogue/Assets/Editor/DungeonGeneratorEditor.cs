using System;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(AbstractDungeonGenerator), true)]
public class DungeonGeneratorEditor : Editor
{
    private AbstractDungeonGenerator generator;

    private void Awake()
    {
        generator = (AbstractDungeonGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate Dungeon"))
        {
            generator.GeneratorDungeon();
        }
    }
}
