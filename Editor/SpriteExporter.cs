using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpriteExporter
{
    public class SpriteExporter : EditorWindow
    {
        private Texture2D targetTexture;
        private List<Sprite> subSprites = new List<Sprite>();
        private List<bool> selectionStatus = new List<bool>();
        private Vector2 scrollPos;
        private string searchText = "";

        private const float CellWidth = 100f;
        private const float CellHeight = 120f;
        private const float Padding = 10f;

        [MenuItem("Tools/Sprite 子图导出工具")]
        public static void ShowWindow()
        {
            GetWindow<SpriteExporter>("Sprite 导出器", true);
        }

        private void OnGUI()
        {
            GUILayout.Label("Sprite 子图导出工具", EditorStyles.boldLabel);

            // 1. 顶部拖入区域
            EditorGUI.BeginChangeCheck();
            targetTexture = (Texture2D)EditorGUILayout.ObjectField("目标纹理 (Sprite)", targetTexture, typeof(Texture2D), false);
            if (EditorGUI.EndChangeCheck())
            {
                LoadSubSprites();
            }

            if (targetTexture == null)
            {
                EditorGUILayout.HelpBox("请拖入一张已切割的 Sprite 纹理。", MessageType.Info);
                return;
            }

            // 检查是否是Sprite类型
            string assetPath = AssetDatabase.GetAssetPath(targetTexture);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                EditorGUILayout.HelpBox("所选纹理未设置为 Sprite 类型，请在 Inspector 中修改纹理类型。", MessageType.Warning);
                return;
            }

            // 2. 搜索框
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            EditorGUILayout.LabelField("搜索子图:", GUILayout.Width(60));
            searchText = EditorGUILayout.TextField(searchText, EditorStyles.toolbarSearchField);
            if (GUILayout.Button("清除", EditorStyles.miniButton, GUILayout.Width(40)))
            {
                searchText = "";
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();

            // 3. 控制按钮
            DrawControlButtons();

            EditorGUILayout.Space(5);

            // 4. 网格列表渲染
            if (subSprites.Count == 0)
            {
                EditorGUILayout.HelpBox("未检测到子图，请确认纹理已正确切割。", MessageType.Info);
                return;
            }
            DrawSortedSpriteGrid();

            EditorGUILayout.Space(5);

            // 5. 执行按钮
            GUI.enabled = selectionStatus.Contains(true);
            if (GUILayout.Button("导出选中项", GUILayout.Height(40)))
            {
                ExportSelectedSprites();
            }
            GUI.enabled = true;
        }

        private void LoadSubSprites()
        {
            subSprites.Clear();
            selectionStatus.Clear();
            if (targetTexture == null) return;

            string path = AssetDatabase.GetAssetPath(targetTexture);
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (var asset in assets)
            {
                if (asset is Sprite s)
                {
                    subSprites.Add(s);
                    selectionStatus.Add(true);
                }
            }
        }

        private void DrawControlButtons()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("全选"))
                for (int i = 0; i < selectionStatus.Count; i++) selectionStatus[i] = true;
            if (GUILayout.Button("全不选"))
                for (int i = 0; i < selectionStatus.Count; i++) selectionStatus[i] = false;
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSortedSpriteGrid()
        {
            List<int> matchedIndices = new List<int>();
            List<int> unmatchedIndices = new List<int>();

            for (int i = 0; i < subSprites.Count; i++)
            {
                if (string.IsNullOrEmpty(searchText) || subSprites[i].name.ToLower().Contains(searchText.ToLower()))
                {
                    matchedIndices.Add(i);
                }
                else
                {
                    unmatchedIndices.Add(i);
                }
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            // 渲染匹配项
            if (matchedIndices.Count > 0)
            {
                if (!string.IsNullOrEmpty(searchText))
                    EditorGUILayout.LabelField($"搜索结果 ({matchedIndices.Count})", EditorStyles.whiteMiniLabel);
                
                RenderGrid(matchedIndices);
            }

            // 渲染分割线和剩余项
            if (!string.IsNullOrEmpty(searchText) && unmatchedIndices.Count > 0)
            {
                EditorGUILayout.Space(10);
                Rect rect = GUILayoutUtility.GetRect(Screen.width, 2);
                EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f, 1));
                EditorGUILayout.Space(10);
                
                EditorGUILayout.LabelField("其他子图", EditorStyles.whiteMiniLabel);
                RenderGrid(unmatchedIndices);
            }

            EditorGUILayout.EndScrollView();
        }

        private void RenderGrid(List<int> indices)
        {
            float viewWidth = position.width - 25;
            int columns = Mathf.Max(1, Mathf.FloorToInt(viewWidth / (CellWidth + Padding)));
            int rows = Mathf.CeilToInt((float)indices.Count / columns);

            for (int r = 0; r < rows; r++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int c = 0; c < columns; c++)
                {
                    int listIdx = r * columns + c;
                    if (listIdx < indices.Count)
                    {
                        DrawSpriteCell(indices[listIdx]);
                    }
                    else
                    {
                        GUILayout.Space(CellWidth + Padding);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawSpriteCell(int index)
        {
            GUIStyle style = new GUIStyle(EditorStyles.helpBox);
            Rect cellRect = EditorGUILayout.BeginVertical(style, GUILayout.Width(CellWidth), GUILayout.Height(CellHeight));
            
            // 勾选框
            selectionStatus[index] = EditorGUILayout.Toggle(selectionStatus[index], GUILayout.Width(20));

            // 图片预览
            Rect imgArea = new Rect(cellRect.x + 5, cellRect.y + 25, CellWidth - 10, CellHeight - 55);
            DrawSpriteAspectCorrected(imgArea, subSprites[index]);

            // 底部名字
            GUILayout.FlexibleSpace();
            string name = subSprites[index].name;
            // 截断过长的名称
            if (name.Length > 12)
                name = name.Substring(0, 12) + "...";
            EditorGUILayout.LabelField(name, EditorStyles.miniLabel, GUILayout.Width(CellWidth - 10));
            
            EditorGUILayout.EndVertical();
            GUILayout.Space(Padding);
        }

        private void DrawSpriteAspectCorrected(Rect area, Sprite sprite)
        {
            if (sprite == null || sprite.texture == null) return;
            
            float scale = Mathf.Min(area.width / sprite.rect.width, area.height / sprite.rect.height);
            float dW = sprite.rect.width * scale;
            float dH = sprite.rect.height * scale;
            Rect drawRect = new Rect(area.x + (area.width - dW) / 2, area.y + (area.height - dH) / 2, dW, dH);
            Rect t = sprite.rect;
            Rect uv = new Rect(t.x / sprite.texture.width, t.y / sprite.texture.height, t.width / sprite.texture.width, t.height / sprite.texture.height);
            GUI.DrawTextureWithTexCoords(drawRect, sprite.texture, uv);
        }

        private void ExportSelectedSprites()
        {
            try
            {
                string assetPath = AssetDatabase.GetAssetPath(targetTexture);
                string folderPath = Path.Combine(Path.GetDirectoryName(assetPath), targetTexture.name + "_Exported");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                EnsureTextureIsReadable(assetPath);

                int count = 0;
                for (int i = 0; i < subSprites.Count; i++)
                {
                    if (selectionStatus[i])
                    {
                        var rect = subSprites[i].textureRect;
                        // 防止尺寸为0的异常
                        if (rect.width <= 0 || rect.height <= 0)
                        {
                            Debug.LogWarning($"跳过无效子图：{subSprites[i].name}（尺寸为0）");
                            continue;
                        }

                        var newTex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
                        newTex.SetPixels(subSprites[i].texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height));
                        newTex.Apply();
                        
                        string savePath = Path.Combine(folderPath, subSprites[i].name + ".png");
                        // 处理重复文件名
                        int duplicateCount = 1;
                        while (File.Exists(savePath))
                        {
                            savePath = Path.Combine(folderPath, $"{subSprites[i].name}_{duplicateCount}.png");
                            duplicateCount++;
                        }
                        
                        File.WriteAllBytes(savePath, newTex.EncodeToPNG());
                        DestroyImmediate(newTex);
                        count++;
                    }
                }

                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("导出完成", $"已成功导出 {count} 个文件到：\n{folderPath}", "确定");
                // 打开导出目录
                EditorUtility.RevealInFinder(folderPath);
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("导出失败", $"导出过程中出现错误：\n{e.Message}", "确定");
                Debug.LogError($"Sprite导出失败：{e}");
            }
        }

        private void EnsureTextureIsReadable(string path)
        {
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti != null && !ti.isReadable)
            {
                // 提示用户权限变更
                if (EditorUtility.DisplayDialog("权限提示", "为了导出子图，需要将纹理设置为 Readable 模式，是否确认？", "确认", "取消"))
                {
                    ti.isReadable = true;
                    ti.SaveAndReimport();
                }
                else
                {
                    throw new System.Exception("用户取消了纹理权限修改，导出终止。");
                }
            }
        }
    }
}