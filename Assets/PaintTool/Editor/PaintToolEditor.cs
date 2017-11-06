
// ----- Credits
// ----- Riccardo Runci
// ----- PaintTool v1.3
/*
 * Notes 1.2:
 * + Opacity Feature
 * 
 * Notes 1.3:
 * + Custom GameObject parent
 * + Groups logic
 */

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PaintTool))]
public class PaintToolEditor : Editor
{
    private static PaintTool tool;
    private Vector3 m_hLastPoint;
    private Vector3 m_hOriginalObjectPos;
    private bool m_bErease;
    private bool m_bPlace;

    [MenuItem("GameObject/PainterTool")]
    private static void Init()
    {
        GameObject hObj = new GameObject("PaintTool");
        hObj.AddComponent<PaintTool>();
        Selection.activeGameObject = hObj;
    }

    private void OnSceneGUI()
    {
        if (tool == null)
            return;

        ManageKeyCombination();
        if (Event.current.type == EventType.mouseMove || Event.current.type == EventType.mouseDown || Event.current.type == EventType.mouseDrag)
        {
            HandleUtility.Repaint();
        }

        if (tool.Painting)
        {
            if (Event.current.type == EventType.mouseDown)
            {
                for (int i = 0; i < (int)(tool.Opacity * PaintTool.PaintSize * tool.BrushSize) ; i++)
                { 
                    Paint();
                }
            }
            if (Event.current.type == EventType.mouseDrag)
            {
                for (int i = 0; i < (int)(tool.Opacity * PaintTool.PaintSize * tool.BrushSize); i++)
                {
                    Paint();
                }
            }
        }
        if (Event.current.type == EventType.mouseUp)
        {
            m_hLastPoint = Vector3.zero;
        }
        else
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Keyboard));
        }
    }

    private void Paint()
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition + Random.insideUnitCircle * 10 * tool.BrushSize);
        RaycastHit hHit = new RaycastHit();
        if (Physics.Raycast(ray, out hHit, 1000.0f, tool.Layer))
        {
            if (m_bPlace && !m_bErease)
            { 
                    m_hLastPoint = hHit.point;
                    if (tool.Groups.Count <= 0)
                    {
                        Debug.Log("Your Groups list is empty!");
                        return;
                    }
                    int iRand = Random.Range(0, tool.Groups[tool.GroupIndex].Prefabs.Count);
                    if (tool.Groups[tool.GroupIndex].Prefabs[iRand] == null)
                    {
                        Debug.LogError("A prefab in your list is NULL");
                        return;
                    }
                    GameObject hObj = PrefabUtility.InstantiatePrefab(tool.Groups[tool.GroupIndex].Prefabs[iRand]) as GameObject;
                    Undo.RegisterCreatedObjectUndo(hObj, "Created go");
                    hObj.transform.position = hHit.point;
                    hObj.transform.rotation = Quaternion.LookRotation(hHit.normal);
                    hObj.transform.Rotate(new Vector3(90, 0, 0), Space.Self);
                    hObj.transform.Rotate(new Vector3(0, Random.Range(tool.MinRotation, tool.MaxRotation), 0), Space.Self);
                    hObj.transform.SetParent(tool.Parent == null || EditorUtility.IsPersistent(tool.Parent) ? tool.gameObject.transform : tool.Parent.transform);
                    tool.Cache.Add(hObj);

            }
            else if (m_bErease && !m_bPlace)
            {
                CheckErease(hHit.point);
            }
        }
    }

    private void CheckErease(Vector3 hPoint)
    {
        Transform[] hObjects = FindObjectsOfType<Transform>();
        List<GameObject> hRangeObjects = new List<GameObject>();
        for (int i = 0; i < hObjects.Length; i++)
        {
            if (Vector3.Distance(hObjects[i].position, hPoint) <= tool.BrushSize)
            {
                hRangeObjects.Add(hObjects[i].gameObject);
            }

            for (int j = 0; j < hRangeObjects.Count; j++)
            {
                if (hRangeObjects[j] != null && PrefabUtility.GetPrefabType(hRangeObjects[j]) == PrefabType.PrefabInstance)
                {
                    for (int k = 0; k < tool.Groups[tool.GroupIndex].Prefabs.Count; k++)
                    {
                        if (hRangeObjects[j] != null && hRangeObjects[j].name == tool.Groups[tool.GroupIndex].Prefabs[k].name)
                        {
                            float fRand = Random.Range(0f, 1f);
                            if (fRand <= tool.Opacity)
                            {
                                DestroyImmediate(hRangeObjects[j]);
                            }
                        }    
                    }
                }
            }
        }
    }

    private void ManageKeyCombination()
    {
        Event e = Event.current;
        switch (e.type)
        {
            case EventType.KeyDown:
                if (Event.current.keyCode == KeyCode.X)
                    m_bPlace = true;
                if (Event.current.keyCode == KeyCode.C)
                    m_bErease = true;
                break;
            case EventType.KeyUp:
                if (Event.current.keyCode == KeyCode.X)
                    m_bPlace = false;
                if (Event.current.keyCode == KeyCode.C)
                    m_bErease = false;
                break;
        }
    }

    public override void OnInspectorGUI()
    {
        tool = target as PaintTool;

        if (tool != null && tool.Painting)
        {
            if (GUILayout.Button("Stop Painting"))
            {
                tool.Painting = false;
            }
            EditorGUILayout.LabelField("X + Click to Place");
            EditorGUILayout.LabelField("C + Click to Erease");
            tool.BrushSize = EditorGUILayout.FloatField("Brush Size", tool.BrushSize);
            tool.Opacity = EditorGUILayout.Slider("Opacity", tool.Opacity, 0.1f, 1f);
            EditorGUILayout.MinMaxSlider(string.Format("Random Size {0}-{1}", tool.MinSize.ToString("###0.#"), tool.MaxSize.ToString("###0.#")), ref tool.MinSize, ref tool.MaxSize, 0.1f, 10);
            EditorGUILayout.MinMaxSlider(string.Format("Random Rotation {0}-{1}", tool.MinRotation.ToString("###0.#"), tool.MaxRotation.ToString("###0.#")), ref tool.MinRotation, ref tool.MaxRotation, 0, 360);
            tool.Layer = EditorTools.LayerMaskField("Mask", tool.Layer);
        }
        else
        {
            if (GUILayout.Button("Start Painting"))
            {
                if (tool != null)
                {
                    tool.Painting = true;
                    m_hOriginalObjectPos = tool.gameObject.transform.position;
                }
            }
        }

        tool.Parent = (GameObject)EditorGUILayout.ObjectField("Root Parent", tool.Parent, typeof(GameObject), true);

        EditorGUILayout.Space();

        if (GUILayout.Button("Add Group"))
        {
            if (tool != null) tool.Groups.Add(new PaintGroup());
        }

        for (int i = 0; i < tool.Groups.Count; i++)
        {
            EditorGUILayout.BeginVertical(EditorStyles.colorField);
            EditorGUILayout.BeginHorizontal();
            if(i == tool.GroupIndex)
                GUI.contentColor = Color.green;
            else
                GUI.contentColor = Color.white;
            if (GUILayout.Button(string.Format("Select Group {0}", i + 1)))
            {
                tool.GroupIndex = i;
            }
            GUI.contentColor = Color.white;
            if (GUILayout.Button("X"))
            {
                tool.RemoveElement(i);
                return;
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Add Prefab"))
            {
                if (tool != null) tool.Groups[i].Prefabs.Add(null);
            }
            for (int j = 0; j < tool.Groups[i].Prefabs.Count; j++)
            {
                EditorGUILayout.BeginHorizontal();
                tool.Groups[i].Prefabs[j] = (GameObject)EditorGUILayout.ObjectField(string.Format("Item {0}", j), tool.Groups[i].Prefabs[j], typeof(GameObject), true);
                if (GUILayout.Button("X"))
                {
                    tool.Groups[i].Prefabs.RemoveAt(j);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        EditorGUILayout.Space();

        if (GUILayout.Button("Clear Prefabs"))
        {
            if (tool != null)
            {
                for (int i = 0; i < tool.Cache.Count; i++)
                {
                    if(tool.Cache[i] != null)
                        DestroyImmediate(tool.Cache[i]);
                }
                tool.Cache.Clear();
            }
        }

        if (GUILayout.Button("Force Clear"))
        {
            if (tool != null)
            {
                tool.GetComponentsInChildren<Transform>().ToList().ForEach(x =>
                {
                    if(x != null && x.name != tool.name)
                        DestroyImmediate(x.gameObject);
                });
            }
        }

        if (tool != null)
        {
          
        }
    }
}


public class EditorTools
{

    static List<string> layers;
    static string[] layerNames;

    public static LayerMask LayerMaskField(string label, LayerMask selected)
    {

        if (layers == null)
        {
            layers = new List<string>();
            layerNames = new string[4];
        }
        else
        {
            layers.Clear();
        }

        int emptyLayers = 0;
        for (int i = 0; i < 32; i++)
        {
            string layerName = LayerMask.LayerToName(i);

            if (layerName != "")
            {

                for (; emptyLayers > 0; emptyLayers--) layers.Add("Layer " + (i - emptyLayers));
                layers.Add(layerName);
            }
            else
            {
                emptyLayers++;
            }
        }

        if (layerNames.Length != layers.Count)
        {
            layerNames = new string[layers.Count];
        }
        for (int i = 0; i < layerNames.Length; i++) layerNames[i] = layers[i];

        selected.value = EditorGUILayout.MaskField(label, selected.value, layerNames);

        return selected;
    }
}