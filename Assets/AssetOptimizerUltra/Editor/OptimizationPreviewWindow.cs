using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetOptimizerPro
{
    public class OptimizationPreviewWindow : EditorWindow
    {
        private List<ScannedAsset> assets;
        private OptimizationProfile profile;
        private Vector2 scrollPosition;
        private int selectedAssetIndex = -1;
        private Texture2D previewTexture;
        private Editor assetPreviewEditor;
        
        // Preview comparison
        private bool showSideBySide = true;
        private float splitPosition = 0.5f;
        private bool isDraggingSplitter = false;
        
        // Colors
        private readonly Color bgDark = new Color(0.18f, 0.18f, 0.18f);
        private readonly Color bgMedium = new Color(0.22f, 0.22f, 0.22f);
        private readonly Color primaryColor = new Color(0.2f, 0.6f, 1f);
        private readonly Color accentColor = new Color(0.1f, 0.9f, 0.5f);
        
        public static OptimizationPreviewWindow ShowWindow()
        {
            var window = GetWindow<OptimizationPreviewWindow>("Optimization Preview");
            window.minSize = new Vector2(800, 600);
            return window;
        }
        
        public void SetAssets(List<ScannedAsset> assetsToPreview, OptimizationProfile optimizationProfile)
        {
            assets = assetsToPreview;
            profile = optimizationProfile;
            
            if (assets != null && assets.Count > 0)
            {
                selectedAssetIndex = 0;
                LoadAssetPreview(assets[0]);
            }
        }
        
        private void OnGUI()
        {
            if (assets == null || assets.Count == 0)
            {
                EditorGUILayout.HelpBox("No assets to preview. Please select assets from the main window.", MessageType.Info);
                return;
            }
            
            DrawBackground();
            DrawHeader();
            
            EditorGUILayout.BeginHorizontal();
            
            // Left panel - Asset list
            DrawAssetList();
            
            // Right panel - Preview
            DrawPreviewPanel();
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawBackground()
        {
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), bgDark);
        }
        
        private void DrawHeader()
        {
            var headerRect = new Rect(0, 0, position.width, 50);
            EditorGUI.DrawRect(headerRect, bgMedium);
            
            GUILayout.BeginArea(headerRect);
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            
            var headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            
            GUILayout.Label("Optimization Preview", headerStyle);
            GUILayout.EndVertical();
            
            GUILayout.FlexibleSpace();
            
            // Preview options
            GUILayout.BeginVertical();
            GUILayout.Space(15);
            
            showSideBySide = GUILayout.Toggle(showSideBySide, "Side-by-Side Comparison", GUILayout.Width(150));
            
            GUILayout.EndVertical();
            
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            
            // Separator
            var separatorRect = new Rect(0, headerRect.height - 1, position.width, 1);
            EditorGUI.DrawRect(separatorRect, primaryColor * 0.5f);
        }
        
        private void DrawAssetList()
        {
            var listWidth = 250f;
            var listRect = new Rect(0, 50, listWidth, position.height - 50);
            
            GUILayout.BeginArea(listRect);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            
            // List header
            EditorGUILayout.LabelField($"Selected Assets ({assets.Count})", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Asset list
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            for (int i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                var isSelected = selectedAssetIndex == i;
                
                var itemStyle = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(10, 10, 5, 5)
                };
                
                if (isSelected)
                {
                    itemStyle.normal.background = CreateColorTexture(primaryColor * 0.3f);
                    itemStyle.normal.textColor = Color.white;
                }
                
                if (GUILayout.Button(new GUIContent(asset.name, GetAssetIcon(asset.type)), itemStyle))
                {
                    selectedAssetIndex = i;
                    LoadAssetPreview(asset);
                }
                
                // Show optimization info
                if (isSelected)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField($"Current: {FormatBytes(asset.currentSize)}", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"Optimized: {FormatBytes(asset.optimizedSize)}", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"Savings: {FormatBytes(asset.PotentialSavings)} ({(asset.OptimizationRatio * 100):F0}%)", 
                        new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = accentColor } });
                    EditorGUI.indentLevel--;
                }
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        private void DrawPreviewPanel()
        {
            var previewRect = new Rect(250, 50, position.width - 250, position.height - 50);
            
            GUILayout.BeginArea(previewRect);
            EditorGUILayout.BeginVertical();
            
            if (selectedAssetIndex < 0 || selectedAssetIndex >= assets.Count)
            {
                EditorGUILayout.HelpBox("Select an asset to preview optimization changes", MessageType.Info);
                EditorGUILayout.EndVertical();
                GUILayout.EndArea();
                return;
            }
            
            var selectedAsset = assets[selectedAssetIndex];
            
            // Asset info header
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField(selectedAsset.name, new GUIStyle(EditorStyles.boldLabel) { fontSize = 16 });
            EditorGUILayout.LabelField($"Type: {selectedAsset.type} | Path: {selectedAsset.path}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // Optimization details
            DrawOptimizationDetails(selectedAsset);
            
            EditorGUILayout.Space();
            
            // Visual preview (for applicable assets)
            if (showSideBySide && (selectedAsset.type == AssetType.Texture || selectedAsset.type == AssetType.Model))
            {
                DrawSideBySidePreview(selectedAsset);
            }
            else
            {
                DrawSinglePreview(selectedAsset);
            }
            
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        private void DrawOptimizationDetails(ScannedAsset asset)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Optimization Details", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Size comparison
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.BeginVertical(GUILayout.Width(150));
            EditorGUILayout.LabelField("Current Size", EditorStyles.miniLabel);
            EditorGUILayout.LabelField(FormatBytes(asset.currentSize), new GUIStyle(EditorStyles.boldLabel) { fontSize = 18 });
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.LabelField("→", new GUIStyle(EditorStyles.boldLabel) { fontSize = 24, alignment = TextAnchor.MiddleCenter }, GUILayout.Width(50));
            
            EditorGUILayout.BeginVertical(GUILayout.Width(150));
            EditorGUILayout.LabelField("Optimized Size", EditorStyles.miniLabel);
            EditorGUILayout.LabelField(FormatBytes(asset.optimizedSize), 
                new GUIStyle(EditorStyles.boldLabel) { fontSize = 18, normal = { textColor = accentColor } });
            EditorGUILayout.EndVertical();
            
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.BeginVertical(GUILayout.Width(150));
            EditorGUILayout.LabelField("Savings", EditorStyles.miniLabel);
            var savingsPercent = ((float)(asset.currentSize - asset.optimizedSize) / asset.currentSize) * 100;
            EditorGUILayout.LabelField($"{savingsPercent:F0}%", 
                new GUIStyle(EditorStyles.boldLabel) { fontSize = 24, normal = { textColor = accentColor } });
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Optimization steps
            if (asset.optimizationReasons != null && asset.optimizationReasons.Count > 0)
            {
                EditorGUILayout.LabelField("Optimizations to Apply:", EditorStyles.boldLabel);
                foreach (var reason in asset.optimizationReasons)
                {
                    EditorGUILayout.LabelField($"• {reason}", EditorStyles.wordWrappedLabel);
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawSideBySidePreview(ScannedAsset asset)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true));
            EditorGUILayout.LabelField("Visual Comparison", EditorStyles.boldLabel);
            
            var previewArea = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            
            // Calculate split areas
            var leftWidth = previewArea.width * splitPosition - 5;
            var rightWidth = previewArea.width * (1 - splitPosition) - 5;
            
            var leftRect = new Rect(previewArea.x, previewArea.y, leftWidth, previewArea.height);
            var splitterRect = new Rect(previewArea.x + leftWidth, previewArea.y, 10, previewArea.height);
            var rightRect = new Rect(previewArea.x + leftWidth + 10, previewArea.y, rightWidth, previewArea.height);
            
            // Draw panels
            GUI.Box(leftRect, "");
            GUI.Box(rightRect, "");
            
            // Labels
            var labelStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(leftRect.x, leftRect.y, leftRect.width, 20), "Original", labelStyle);
            GUI.Label(new Rect(rightRect.x, rightRect.y, rightRect.width, 20), "Optimized", labelStyle);
            
            // Draw previews
            if (asset.type == AssetType.Texture)
            {
                DrawTextureComparison(asset, 
                    new Rect(leftRect.x + 10, leftRect.y + 30, leftRect.width - 20, leftRect.height - 40),
                    new Rect(rightRect.x + 10, rightRect.y + 30, rightRect.width - 20, rightRect.height - 40));
            }
            else if (asset.type == AssetType.Model)
            {
                DrawModelComparison(asset, 
                    new Rect(leftRect.x + 10, leftRect.y + 30, leftRect.width - 20, leftRect.height - 40),
                    new Rect(rightRect.x + 10, rightRect.y + 30, rightRect.width - 20, rightRect.height - 40));
            }
            
            // Draw splitter
            EditorGUI.DrawRect(splitterRect, new Color(0.3f, 0.3f, 0.3f));
            EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeHorizontal);
            
            // Handle splitter dragging
            var e = Event.current;
            if (e.type == EventType.MouseDown && splitterRect.Contains(e.mousePosition))
            {
                isDraggingSplitter = true;
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && isDraggingSplitter)
            {
                splitPosition = Mathf.Clamp((e.mousePosition.x - previewArea.x) / previewArea.width, 0.2f, 0.8f);
                Repaint();
                e.Use();
            }
            else if (e.type == EventType.MouseUp)
            {
                isDraggingSplitter = false;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawSinglePreview(ScannedAsset asset)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true));
            EditorGUILayout.LabelField("Asset Preview", EditorStyles.boldLabel);
            
            var previewRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, 
                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            
            // Draw asset-specific preview
            switch (asset.type)
            {
                case AssetType.Texture:
                    DrawTexturePreview(asset, previewRect);
                    break;
                case AssetType.Model:
                    DrawModelPreview(asset, previewRect);
                    break;
                case AssetType.Audio:
                    DrawAudioPreview(asset, previewRect);
                    break;
                case AssetType.Animation:
                    DrawAnimationPreview(asset, previewRect);
                    break;
                default:
                    EditorGUI.LabelField(previewRect, "Preview not available for this asset type", 
                        new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter });
                    break;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawTextureComparison(ScannedAsset asset, Rect leftRect, Rect rightRect)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(asset.path);
            if (texture != null)
            {
                // Original
                GUI.DrawTexture(leftRect, texture, ScaleMode.ScaleToFit);
                
                // Optimized (simulated)
                GUI.DrawTexture(rightRect, texture, ScaleMode.ScaleToFit);
                
                // Overlay optimization info
                var overlayStyle = new GUIStyle(GUI.skin.box)
                {
                    normal = { background = CreateColorTexture(new Color(0, 0, 0, 0.7f)) },
                    padding = new RectOffset(5, 5, 5, 5)
                };
                
                var infoRect = new Rect(rightRect.x, rightRect.yMax - 60, rightRect.width, 50);
                GUI.Box(infoRect, "", overlayStyle);
                
                var labelStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.white } };
                GUI.Label(new Rect(infoRect.x + 5, infoRect.y + 5, infoRect.width - 10, 20), 
                    $"Resolution: {texture.width}x{texture.height}", labelStyle);
                GUI.Label(new Rect(infoRect.x + 5, infoRect.y + 25, infoRect.width - 10, 20), 
                    $"Compression: {profile.textureSettings.compression}", labelStyle);
            }
        }
        
        private void DrawModelComparison(ScannedAsset asset, Rect leftRect, Rect rightRect)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(asset.path);
            if (model != null && assetPreviewEditor == null)
            {
                assetPreviewEditor = Editor.CreateEditor(model);
            }
            
            if (assetPreviewEditor != null)
            {
                assetPreviewEditor.OnPreviewGUI(leftRect, GUI.skin.box);
                assetPreviewEditor.OnPreviewGUI(rightRect, GUI.skin.box);
            }
        }
        
        private void DrawTexturePreview(ScannedAsset asset, Rect rect)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(asset.path);
            if (texture != null)
            {
                GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
            }
        }
        
        private void DrawModelPreview(ScannedAsset asset, Rect rect)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(asset.path);
            if (model != null && assetPreviewEditor == null)
            {
                assetPreviewEditor = Editor.CreateEditor(model);
            }
            
            if (assetPreviewEditor != null)
            {
                assetPreviewEditor.OnPreviewGUI(rect, GUI.skin.box);
            }
        }
        
        private void DrawAudioPreview(ScannedAsset asset, Rect rect)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(asset.path);
            if (clip != null)
            {
                EditorGUI.BeginDisabledGroup(true);
                
                var waveformRect = new Rect(rect.x + 20, rect.y + 20, rect.width - 40, rect.height - 100);
                DrawAudioWaveform(clip, waveformRect);
                
                // Audio info
                var infoY = waveformRect.yMax + 10;
                EditorGUI.LabelField(new Rect(rect.x + 20, infoY, rect.width - 40, 20), 
                    $"Length: {clip.length:F2}s | Frequency: {clip.frequency}Hz | Channels: {clip.channels}");
                
                EditorGUI.EndDisabledGroup();
            }
        }
        
        private void DrawAnimationPreview(ScannedAsset asset, Rect rect)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(asset.path);
            if (clip != null)
            {
                var infoRect = new Rect(rect.x + 20, rect.y + 20, rect.width - 40, rect.height - 40);
                
                EditorGUI.LabelField(new Rect(infoRect.x, infoRect.y, infoRect.width, 20), 
                    $"Animation: {clip.name}", EditorStyles.boldLabel);
                
                EditorGUI.LabelField(new Rect(infoRect.x, infoRect.y + 30, infoRect.width, 20), 
                    $"Length: {clip.length:F2}s");
                
                EditorGUI.LabelField(new Rect(infoRect.x, infoRect.y + 50, infoRect.width, 20), 
                    $"Frame Rate: {clip.frameRate:F0} FPS");
                
                var bindings = AnimationUtility.GetCurveBindings(clip);
                EditorGUI.LabelField(new Rect(infoRect.x, infoRect.y + 70, infoRect.width, 20), 
                    $"Curves: {bindings.Length}");
            }
        }
        
        private void DrawAudioWaveform(AudioClip clip, Rect rect)
        {
            // Simple waveform visualization
            EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f));
            
            var samples = new float[1024];
            clip.GetData(samples, 0);
            
            var waveColor = primaryColor;
            for (int i = 1; i < samples.Length; i++)
            {
                var x1 = rect.x + (rect.width * (i - 1) / samples.Length);
                var x2 = rect.x + (rect.width * i / samples.Length);
                var y1 = rect.y + rect.height / 2 + (samples[i - 1] * rect.height / 2);
                var y2 = rect.y + rect.height / 2 + (samples[i] * rect.height / 2);
                
                Handles.color = waveColor;
                Handles.DrawLine(new Vector3(x1, y1), new Vector3(x2, y2));
            }
        }
        
        private void LoadAssetPreview(ScannedAsset asset)
        {
            // Clean up previous preview
            if (assetPreviewEditor != null)
            {
                DestroyImmediate(assetPreviewEditor);
                assetPreviewEditor = null;
            }
            
            // Load new preview
            if (asset.type == AssetType.Texture)
            {
                previewTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(asset.path);
            }
        }
        
        private Texture2D CreateColorTexture(Color color)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }
        
        private Texture2D GetAssetIcon(AssetType type)
        {
            string iconName = type switch
            {
                AssetType.Texture => "d_Texture Icon",
                AssetType.Model => "d_PrefabModel Icon",
                AssetType.Audio => "d_AudioClip Icon",
                AssetType.Animation => "d_AnimationClip Icon",
                AssetType.Material => "d_Material Icon",
                AssetType.Shader => "d_Shader Icon",
                _ => "d_DefaultAsset Icon"
            };
            
            var content = EditorGUIUtility.IconContent(iconName);
            return content?.image as Texture2D;
        }
        
        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
        
        private void OnDestroy()
        {
            if (assetPreviewEditor != null)
            {
                DestroyImmediate(assetPreviewEditor);
            }
        }
    }
}