#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class BatchImportFromTXT : EditorWindow
{
    private GameObject prefabToSpawn;  // 要实例化的Prefab
    private TextAsset coordinatesFile; // 包含坐标的TXT文件
    private Vector3 offset = Vector3.zero; // 全局坐标偏移

    [MenuItem("Tools/Batch Import from TXT")]
    static void Init()
    {
        BatchImportFromTXT window = GetWindow<BatchImportFromTXT>();
        window.titleContent = new GUIContent("批量导入工具");
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("批量导入设置", EditorStyles.boldLabel);
        
        // 选择Prefab
        prefabToSpawn = (GameObject)EditorGUILayout.ObjectField("Prefab模型", prefabToSpawn, typeof(GameObject), false);
        
        // 选择TXT文件
        coordinatesFile = (TextAsset)EditorGUILayout.ObjectField("坐标文件 (TXT)", coordinatesFile, typeof(TextAsset), false);
        
        // 全局偏移
        offset = EditorGUILayout.Vector3Field("全局偏移", offset);

        if (GUILayout.Button("开始导入", GUILayout.Height(30)))
        {
            if (prefabToSpawn == null || coordinatesFile == null)
            {
                EditorUtility.DisplayDialog("错误", "请先选择Prefab和坐标文件！", "确定");
                return;
            }
            ImportFromTXT();
        }
    }

    void ImportFromTXT()
    {
        string[] lines = coordinatesFile.text.Split('\n');
        List<Vector3> positions = new List<Vector3>();

        // 解析TXT中的坐标
        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine)) continue;

            string[] parts = trimmedLine.Split(',');
            if (parts.Length != 3)
            {
                Debug.LogWarning($"忽略无效行: {line}");
                continue;
            }

            if (float.TryParse(parts[0], out float x) &&
                float.TryParse(parts[1], out float y) &&
                float.TryParse(parts[2], out float z))
            {
                positions.Add(new Vector3(x, y, z) + offset);
            }
            else
            {
                Debug.LogWarning($"解析失败: {line}");
            }
        }

        // 实例化Prefab到每个坐标
        foreach (Vector3 pos in positions)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn);
            instance.transform.position = pos;
            Undo.RegisterCreatedObjectUndo(instance, "Import from TXT"); // 支持撤销
            Debug.Log($"已生成: {instance.name} 在 {pos}");
        }

        Debug.Log($"完成！共生成 {positions.Count} 个实例。");
    }
}
#endif