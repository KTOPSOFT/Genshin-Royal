using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetOptimizerPro
{
    public class AssetOptimizerProWindow : EditorWindow
    {
        // Constants
        private const string WINDOW_TITLE = "Asset Optimizer Pro";
        private const string VERSION = "2.0.0";
        private const float WINDOW_MIN_WIDTH = 1000f;
        private const float WINDOW_MIN_HEIGHT = 700f;
        
        // Workflow states
        private enum WorkflowState
        {
            Welcome,
            Scanning,
            Selection,
            Optimizing,
            Complete
        }
        
        private WorkflowState currentState = WorkflowState.Welcome;
        
        // UI Constants
        private const float BUTTON_HEIGHT = 45f;
        private const float BUTTON_WIDTH = 250f;
        private const float SMALL_BUTTON_WIDTH = 120f;
        private const float SECTION_SPACING = 20f;
        private const float ELEMENT_SPACING = 10f;
        private const float STANDARD_FONT_SIZE = 14;
        private const float HEADER_FONT_SIZE = 24;
        private const float SUBHEADER_FONT_SIZE = 18;
        
        // Styles
        private GUIStyle headerStyle;
        private GUIStyle subHeaderStyle;
        private GUIStyle sectionStyle;
        private GUIStyle buttonStyle;
        private GUIStyle secondaryButtonStyle;
        private GUIStyle statsStyle;
        private GUIStyle stepIndicatorStyle;
        private GUIStyle activeStepStyle;
        private GUIStyle completedStepStyle;
        private GUIStyle cardStyle;
        
        // Colors
        private readonly Color primaryColor = new Color(0.2f, 0.6f, 1f);
        private readonly Color accentColor = new Color(0.1f, 0.9f, 0.5f);
        private readonly Color warningColor = new Color(1f, 0.7f, 0.1f);
        private readonly Color dangerColor = new Color(0.9f, 0.3f, 0.3f);
        private readonly Color bgDark = new Color(0.18f, 0.18f, 0.18f);
        private readonly Color bgMedium = new Color(0.22f, 0.22f, 0.22f);
        private readonly Color bgLight = new Color(0.26f, 0.26f, 0.26f);
        private readonly Color textPrimary = new Color(0.95f, 0.95f, 0.95f);
        private readonly Color textSecondary = new Color(0.7f, 0.7f, 0.7f);
        
        // Systems
        private AssetScannerSystem scanner;
        private AssetOptimizer optimizer;
        private ProfileManager profileManager;
        private ReportGenerator reportGenerator;
        
        // Data
        private List<ScannedAsset> scannedAssets = new List<ScannedAsset>();
        private List<ScannedAsset> selectedAssets = new List<ScannedAsset>();
        private OptimizationProfile currentProfile;
        private OptimizationReport lastReport;
        private bool isProcessing = false;
        private float processProgress = 0f;
        private string processStatus = "";
        
        // UI State
        private Vector2 scrollPosition;
        private Dictionary<AssetType, bool> assetTypeFilters;
        private PlatformTarget selectedPlatform = PlatformTarget.Mobile;
        private string searchFilter = "";
        private AssetType? filterByType = null;
        private bool selectAll = false;
        
        [MenuItem("Window/Asset Optimizer Pro")]
        public static void ShowWindow()
        {
            var window = GetWindow<AssetOptimizerProWindow>();
            window.titleContent = new GUIContent(WINDOW_TITLE, EditorGUIUtility.IconContent("d_PreMatCube").image);
            window.minSize = new Vector2(WINDOW_MIN_WIDTH, WINDOW_MIN_HEIGHT);
            window.Show();
        }
        
        private void OnEnable()
        {
            InitializeSystems();
            LoadPreferences();
        }
        
        private void InitializeSystems()
        {
            scanner = new AssetScannerSystem();
            optimizer = new AssetOptimizer();
            profileManager = new ProfileManager();
            reportGenerator = new ReportGenerator();
            
            // Initialize asset type filters
            assetTypeFilters = new Dictionary<AssetType, bool>();
            foreach (AssetType type in Enum.GetValues(typeof(AssetType)))
            {
                assetTypeFilters[type] = true;
            }
            
            // Load default profile
            currentProfile = profileManager.GetProfile(selectedPlatform);
        }
        
        private void InitializeStyles()
        {
            if (headerStyle != null) return; // Already initialized
            
            // Header style
            headerStyle = new GUIStyle("label")
            {
                fontSize = (int)HEADER_FONT_SIZE,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(20, 20, 10, 10),
                normal = { textColor = textPrimary }
            };
            
            // Sub header style
            subHeaderStyle = new GUIStyle("label")
            {
                fontSize = (int)SUBHEADER_FONT_SIZE,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(0, 0, 5, 5),
                normal = { textColor = textPrimary }
            };
            
            // Section style
            sectionStyle = new GUIStyle("box")
            {
                padding = new RectOffset(20, 20, 20, 20),
                margin = new RectOffset(20, 20, 10, 10),
                normal = { background = CreateColorTexture(bgMedium) }
            };
            
            // Primary button style
            buttonStyle = new GUIStyle("button")
            {
                fontSize = (int)STANDARD_FONT_SIZE,
                fontStyle = FontStyle.Bold,
                fixedHeight = BUTTON_HEIGHT,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(20, 20, 10, 10),
                normal = { 
                    background = CreateColorTexture(primaryColor),
                    textColor = Color.white 
                },
                hover = { 
                    background = CreateColorTexture(primaryColor * 1.2f),
                    textColor = Color.white 
                },
                active = { 
                    background = CreateColorTexture(primaryColor * 0.8f),
                    textColor = Color.white 
                }
            };
            
            // Secondary button style
            secondaryButtonStyle = new GUIStyle(buttonStyle)
            {
                normal = { 
                    background = CreateColorTexture(bgLight),
                    textColor = textPrimary 
                },
                hover = { 
                    background = CreateColorTexture(bgLight * 1.3f),
                    textColor = textPrimary 
                }
            };
            
            // Card style
            cardStyle = new GUIStyle("box")
            {
                padding = new RectOffset(15, 15, 15, 15),
                margin = new RectOffset(5, 5, 5, 5),
                normal = { background = CreateColorTexture(bgLight) }
            };
        }
        
        private void OnGUI()
        {
            UpdateStyles();
            
            DrawBackground();
            DrawHeader();
            DrawWorkflowSteps();
            
            GUILayout.BeginArea(new Rect(0, 180, position.width, position.height - 230));
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            
            switch (currentState)
            {
                case WorkflowState.Welcome:
                    DrawWelcomeScreen();
                    break;
                case WorkflowState.Scanning:
                    DrawScanningScreen();
                    break;
                case WorkflowState.Selection:
                    DrawSelectionScreen();
                    break;
                case WorkflowState.Optimizing:
                    DrawOptimizingScreen();
                    break;
                case WorkflowState.Complete:
                    DrawCompleteScreen();
                    break;
            }
            
            GUILayout.EndScrollView();
            GUILayout.EndArea();
            
            DrawFooter();
            
            if (isProcessing)
            {
                Repaint();
            }
        }
        
        private void UpdateStyles()
        {
            if (headerStyle == null) InitializeStyles();
        }
        
        private void DrawBackground()
        {
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), bgDark);
        }
        
        private void DrawHeader()
        {
            var headerRect = new Rect(0, 0, position.width, 80);
            EditorGUI.DrawRect(headerRect, bgMedium);
            
            GUILayout.BeginArea(headerRect);
            GUILayout.Space(10);
            
            // Title
            GUILayout.Label(WINDOW_TITLE, headerStyle);
            
            // Subtitle
            var subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = textSecondary }
            };
            GUILayout.Label($"Professional Asset Optimization for Unity • v{VERSION}", subtitleStyle);
            
            GUILayout.EndArea();
            
            // Separator
            EditorGUI.DrawRect(new Rect(0, headerRect.height - 2, position.width, 2), primaryColor * 0.5f);
        }
        
        private void DrawWorkflowSteps()
        {
            var stepsRect = new Rect(0, 80, position.width, 100);
            EditorGUI.DrawRect(stepsRect, bgMedium * 0.8f);
            
            GUILayout.BeginArea(stepsRect);
            GUILayout.Space(20);
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            // Step indicators
            DrawStepIndicator(1, "Setup", WorkflowState.Welcome);
            DrawStepConnector(WorkflowState.Welcome);
            
            DrawStepIndicator(2, "Scan", WorkflowState.Scanning);
            DrawStepConnector(WorkflowState.Scanning);
            
            DrawStepIndicator(3, "Select Assets", WorkflowState.Selection);
            DrawStepConnector(WorkflowState.Selection);
            
            DrawStepIndicator(4, "Optimize", WorkflowState.Optimizing);
            DrawStepConnector(WorkflowState.Optimizing);
            
            DrawStepIndicator(5, "Complete", WorkflowState.Complete);
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.EndArea();
        }
        
        private void DrawStepIndicator(int number, string label, WorkflowState state)
        {
            var width = 140f;
            var rect = GUILayoutUtility.GetRect(width, 60);
            
            // Determine state
            bool isActive = currentState == state;
            bool isCompleted = (int)currentState > (int)state;
            
            // Circle
            var circleSize = 40f;
            var circleRect = new Rect(rect.x + (rect.width - circleSize) / 2, rect.y, circleSize, circleSize);
            
            Color circleColor = isCompleted ? accentColor : (isActive ? primaryColor : bgLight);
            DrawCircle(circleRect, circleColor);
            
            // Number/Check
            var numberStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = isCompleted || isActive ? Color.white : textSecondary }
            };
            
            if (isCompleted)
            {
                GUI.Label(circleRect, "✓", numberStyle);
            }
            else
            {
                GUI.Label(circleRect, number.ToString(), numberStyle);
            }
            
            // Label
            var labelRect = new Rect(rect.x, rect.y + circleSize + 5, rect.width, 20);
            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = isActive ? textPrimary : textSecondary }
            };
            GUI.Label(labelRect, label, labelStyle);
        }
        
        private void DrawStepConnector(WorkflowState fromState)
        {
            if (fromState == WorkflowState.Complete) return;
            
            var rect = GUILayoutUtility.GetRect(40, 60);
            var lineY = rect.y + 20;
            var lineRect = new Rect(rect.x, lineY - 2, rect.width, 4);
            
            bool isCompleted = (int)currentState > (int)fromState;
            Color lineColor = isCompleted ? accentColor * 0.5f : bgLight * 0.5f;
            
            EditorGUI.DrawRect(lineRect, lineColor);
        }
        
        private void DrawWelcomeScreen()
        {
            GUILayout.Space(SECTION_SPACING);
            
            // Welcome message
            GUILayout.BeginVertical(sectionStyle);
            
            var welcomeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                normal = { textColor = textPrimary }
            };
            
            GUILayout.Label("Welcome to Asset Optimizer Pro!", subHeaderStyle);
            GUILayout.Space(ELEMENT_SPACING);
            GUILayout.Label("This tool will help you optimize assets in your Unity project,\nreduce build size and improve performance.", welcomeStyle);
            
            GUILayout.EndVertical();
            
            GUILayout.Space(SECTION_SPACING);
            
            // Configuration section
            GUILayout.BeginVertical(sectionStyle);
            GUILayout.Label("Scan Configuration", subHeaderStyle);
            GUILayout.Space(ELEMENT_SPACING);
            
            // Platform selection
            GUILayout.BeginHorizontal();
            GUILayout.Label("Target Platform:", GUILayout.Width(150));
            selectedPlatform = (PlatformTarget)EditorGUILayout.EnumPopup(selectedPlatform, GUILayout.Width(200));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.Space(ELEMENT_SPACING);
            
            // Asset types
            GUILayout.Label("Asset Types to Scan:");
            GUILayout.Space(5);
            
            GUILayout.BeginHorizontal();
            int columnCount = 0;
            foreach (AssetType type in Enum.GetValues(typeof(AssetType)))
            {
                if (columnCount > 0 && columnCount % 3 == 0)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
                
                assetTypeFilters[type] = GUILayout.Toggle(assetTypeFilters[type], type.ToString(), GUILayout.Width(150));
                columnCount++;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
            
            // Quick presets
            GUILayout.Space(SECTION_SPACING);
            
            GUILayout.BeginVertical(sectionStyle);
            GUILayout.Label("Quick Presets", subHeaderStyle);
            GUILayout.Space(ELEMENT_SPACING);
            
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Mobile Game", secondaryButtonStyle, GUILayout.Height(60)))
            {
                selectedPlatform = PlatformTarget.Mobile;
                SetMobilePreset();
            }
            
            if (GUILayout.Button("VR Application", secondaryButtonStyle, GUILayout.Height(60)))
            {
                selectedPlatform = PlatformTarget.VR;
                SetVRPreset();
            }
            
            if (GUILayout.Button("PC/Console", secondaryButtonStyle, GUILayout.Height(60)))
            {
                selectedPlatform = PlatformTarget.Desktop;
                SetDesktopPreset();
            }
            
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
            
            // Action button
            GUILayout.FlexibleSpace();
            GUILayout.Space(SECTION_SPACING);
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Start Scanning →", buttonStyle, GUILayout.Width(BUTTON_WIDTH)))
            {
                StartScanning();
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.Space(SECTION_SPACING);
        }
        
        private void DrawScanningScreen()
        {
            GUILayout.Space(SECTION_SPACING);
            
            GUILayout.BeginVertical(sectionStyle);
            
            if (isProcessing)
            {
                // Progress
                GUILayout.Label("Scanning Project...", subHeaderStyle);
                GUILayout.Space(ELEMENT_SPACING);
                
                DrawProgressBar("Scan Progress", processProgress, processStatus);
                
                GUILayout.Space(ELEMENT_SPACING);
                
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Cancel", secondaryButtonStyle, GUILayout.Width(SMALL_BUTTON_WIDTH)))
                {
                    CancelScanning();
                }
                
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            else if (scannedAssets.Count > 0)
            {
                // Results summary
                GUILayout.Label("Scan Results", subHeaderStyle);
                GUILayout.Space(ELEMENT_SPACING);
                
                DrawScanSummary();
                
                GUILayout.Space(SECTION_SPACING);
                
                // Action buttons
                GUILayout.BeginHorizontal();
                
                if (GUILayout.Button("← Back", secondaryButtonStyle, GUILayout.Width(SMALL_BUTTON_WIDTH)))
                {
                    currentState = WorkflowState.Welcome;
                }
                
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Continue to Selection →", buttonStyle, GUILayout.Width(BUTTON_WIDTH)))
                {
                    currentState = WorkflowState.Selection;
                    PrepareSelection();
                }
                
                GUILayout.EndHorizontal();
            }
            
            GUILayout.EndVertical();
            
            GUILayout.Space(SECTION_SPACING);
        }
        
        private void DrawSelectionScreen()
        {
            GUILayout.Space(SECTION_SPACING);
            
            // Filters and search
            GUILayout.BeginVertical(sectionStyle);
            GUILayout.Label("Filters and Search", subHeaderStyle);
            GUILayout.Space(ELEMENT_SPACING);
            
            GUILayout.BeginHorizontal();
            
            // Search
            GUILayout.Label("Search:", GUILayout.Width(60));
            searchFilter = EditorGUILayout.TextField(searchFilter, GUILayout.Width(200));
            
            GUILayout.Space(20);
            
            // Type filter
            GUILayout.Label("Type:", GUILayout.Width(40));
            var typeOptions = new string[] { "All Types" }.Concat(
                Enum.GetNames(typeof(AssetType))
            ).ToArray();
            
            int selectedIndex = filterByType.HasValue ? (int)filterByType.Value + 1 : 0;
            selectedIndex = EditorGUILayout.Popup(selectedIndex, typeOptions, GUILayout.Width(150));
            filterByType = selectedIndex > 0 ? (AssetType?)(selectedIndex - 1) : null;
            
            GUILayout.FlexibleSpace();
            
            // Select all
            bool newSelectAll = GUILayout.Toggle(selectAll, "Select All");
            if (newSelectAll != selectAll)
            {
                selectAll = newSelectAll;
                SetAllAssetsSelection(selectAll);
            }
            
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
            
            GUILayout.Space(ELEMENT_SPACING);
            
            // Asset list
            GUILayout.BeginVertical(sectionStyle);
            GUILayout.Label($"Assets to Optimize ({GetFilteredAssets().Count})", subHeaderStyle);
            GUILayout.Space(ELEMENT_SPACING);
            
            DrawAssetList();
            
            GUILayout.EndVertical();
            
            GUILayout.Space(ELEMENT_SPACING);
            
            // Selected summary
            DrawSelectionSummary();
            
            GUILayout.Space(SECTION_SPACING);
            
            // Action buttons
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("← Back", secondaryButtonStyle, GUILayout.Width(SMALL_BUTTON_WIDTH)))
            {
                currentState = WorkflowState.Scanning;
            }
            
            GUILayout.FlexibleSpace();
            
            var selectedCount = scannedAssets.Count(a => a.isSelected);
            GUI.enabled = selectedCount > 0;
            
            if (GUILayout.Button($"Optimize ({selectedCount}) →", buttonStyle, GUILayout.Width(BUTTON_WIDTH)))
            {
                StartOptimization();
            }
            
            GUI.enabled = true;
            
            GUILayout.EndHorizontal();
            
            GUILayout.Space(SECTION_SPACING);
        }
        
        private void DrawOptimizingScreen()
        {
            GUILayout.Space(SECTION_SPACING);
            
            GUILayout.BeginVertical(sectionStyle);
            
            if (isProcessing)
            {
                // Progress
                GUILayout.Label("Optimizing Assets...", subHeaderStyle);
                GUILayout.Space(ELEMENT_SPACING);
                
                DrawProgressBar("Optimization Progress", processProgress, processStatus);
                
                GUILayout.Space(ELEMENT_SPACING);
                
                // Current stats
                if (selectedAssets.Count > 0)
                {
                    var processed = (int)(processProgress * selectedAssets.Count);
                    GUILayout.Label($"Processed: {processed} of {selectedAssets.Count} assets", 
                        new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
                }
            }
            else if (lastReport != null)
            {
                // Show optimization preview
                GUILayout.Label("Optimization Preview", subHeaderStyle);
                GUILayout.Space(ELEMENT_SPACING);
                
                DrawOptimizationPreview();
                
                GUILayout.Space(SECTION_SPACING);
                
                // Confirm buttons
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Apply Optimization", buttonStyle, GUILayout.Width(BUTTON_WIDTH)))
                {
                    ApplyOptimization();
                }
                
                GUILayout.Space(10);
                
                if (GUILayout.Button("Cancel", secondaryButtonStyle, GUILayout.Width(SMALL_BUTTON_WIDTH)))
                {
                    currentState = WorkflowState.Selection;
                }
                
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            
            GUILayout.EndVertical();
            
            GUILayout.Space(SECTION_SPACING);
        }
        
        private void DrawCompleteScreen()
        {
            GUILayout.Space(SECTION_SPACING);
            
            // Success message
            GUILayout.BeginVertical(sectionStyle);
            
            var successStyle = new GUIStyle(headerStyle)
            {
                normal = { textColor = accentColor }
            };
            
            GUILayout.Label("✓ Optimization Complete!", successStyle);
            
            GUILayout.EndVertical();
            
            GUILayout.Space(SECTION_SPACING);
            
            // Results
            if (lastReport != null)
            {
                GUILayout.BeginVertical(sectionStyle);
                GUILayout.Label("Optimization Results", subHeaderStyle);
                GUILayout.Space(ELEMENT_SPACING);
                
                DrawOptimizationResults();
                
                GUILayout.EndVertical();
                
                GUILayout.Space(SECTION_SPACING);
                
                // Export options
                GUILayout.BeginVertical(sectionStyle);
                GUILayout.Label("Export Report", subHeaderStyle);
                GUILayout.Space(ELEMENT_SPACING);
                
                GUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Export as HTML", secondaryButtonStyle))
                {
                    reportGenerator.ExportHTML();
                }
                
                if (GUILayout.Button("Export as CSV", secondaryButtonStyle))
                {
                    reportGenerator.ExportCSV();
                }
                
                if (GUILayout.Button("Export as JSON", secondaryButtonStyle))
                {
                    reportGenerator.ExportJSON();
                }
                
                GUILayout.EndHorizontal();
                
                GUILayout.EndVertical();
            }
            
            GUILayout.Space(SECTION_SPACING);
            
            // Action buttons
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Start New Scan", buttonStyle, GUILayout.Width(BUTTON_WIDTH)))
            {
                ResetWorkflow();
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.Space(SECTION_SPACING);
        }
        
        private void DrawProgressBar(string label, float progress, string sublabel = "")
        {
            GUILayout.Label(label, new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            
            var progressRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(30));
            
            // Background
            GUI.Box(progressRect, GUIContent.none, new GUIStyle(GUI.skin.box) 
            { 
                normal = { background = CreateColorTexture(bgDark) } 
            });
            
            // Fill
            if (progress > 0)
            {
                var fillRect = new Rect(progressRect.x, progressRect.y, progressRect.width * progress, progressRect.height);
                GUI.Box(fillRect, GUIContent.none, new GUIStyle(GUI.skin.box) 
                { 
                    normal = { background = CreateColorTexture(primaryColor) } 
                });
            }
            
            // Percentage
            var percentStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            GUI.Label(progressRect, $"{(progress * 100):F0}%", percentStyle);
            
            // Sublabel
            if (!string.IsNullOrEmpty(sublabel))
            {
                GUILayout.Label(sublabel, new GUIStyle(GUI.skin.label) 
                { 
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 12,
                    normal = { textColor = textSecondary }
                });
            }
        }
        
        private void DrawScanSummary()
        {
            var stats = CalculateStats();
            
            GUILayout.BeginHorizontal();
            
            DrawStatCard("Assets Found", scannedAssets.Count.ToString(), primaryColor);
            DrawStatCard("Total Size", FormatBytes(stats.totalSize), Color.white);
            DrawStatCard("Potential Savings", FormatBytes(stats.potentialSavings), accentColor);
            DrawStatCard("Savings Percent", $"{stats.savingsPercentage:F1}%", warningColor);
            
            GUILayout.EndHorizontal();
        }
        
        private void DrawStatCard(string label, string value, Color valueColor)
        {
            GUILayout.BeginVertical(cardStyle, GUILayout.ExpandWidth(true));
            
            var valueStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = valueColor }
            };
            
            GUILayout.Label(value, valueStyle);
            
            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = textSecondary }
            };
            
            GUILayout.Label(label, labelStyle);
            
            GUILayout.EndVertical();
        }
        
        private void DrawAssetList()
        {
            var filteredAssets = GetFilteredAssets();
            var groupedAssets = filteredAssets.GroupBy(a => a.type).OrderBy(g => g.Key.ToString());
            
            foreach (var group in groupedAssets)
            {
                GUILayout.Space(5);
                
                // Category header
                GUILayout.BeginHorizontal();
                
                var categoryStyle = new GUIStyle(GUI.skin.label)
                {
                    fontStyle = FontStyle.Bold,
                    fontSize = 14,
                    normal = { textColor = GetAssetTypeColor(group.Key) }
                };
                
                GUILayout.Label($"{group.Key} ({group.Count()})", categoryStyle);
                
                GUILayout.FlexibleSpace();
                
                // Select all in category
                if (GUILayout.Button("Select All", GUILayout.Width(SMALL_BUTTON_WIDTH)))
                {
                    foreach (var asset in group)
                    {
                        asset.isSelected = true;
                    }
                }
                
                GUILayout.EndHorizontal();
                
                GUILayout.Space(5);
                
                // Assets
                foreach (var asset in group.OrderByDescending(a => a.currentSize))
                {
                    DrawAssetItem(asset);
                }
                
                GUILayout.Space(10);
            }
        }
        
        private void DrawAssetItem(ScannedAsset asset)
        {
            var rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(40));
            
            // Background
            var bgColor = rect.Contains(Event.current.mousePosition) ? bgLight * 1.3f : bgLight;
            EditorGUI.DrawRect(rect, bgColor);
            
            // Layout
            var padding = 10f;
            var checkboxWidth = 30f;
            var iconWidth = 30f;
            var sizeWidth = 100f;
            var savingsWidth = 150f;
            
            // Checkbox
            var checkRect = new Rect(rect.x + padding, rect.y + 10, checkboxWidth, 20);
            asset.isSelected = EditorGUI.Toggle(checkRect, asset.isSelected);
            
            // Icon
            var iconRect = new Rect(checkRect.xMax, rect.y + 5, iconWidth, 30);
            var icon = GetAssetIcon(asset.type);
            if (icon != null) GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
            
            // Name
            var nameRect = new Rect(iconRect.xMax + 5, rect.y, 
                rect.width - checkboxWidth - iconWidth - sizeWidth - savingsWidth - padding * 4, rect.height);
            GUI.Label(nameRect, asset.name, new GUIStyle(GUI.skin.label) 
            { 
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold 
            });
            
            // Current size
            var sizeRect = new Rect(rect.xMax - sizeWidth - savingsWidth - padding * 2, rect.y, sizeWidth, rect.height);
            GUI.Label(sizeRect, FormatBytes(asset.currentSize), new GUIStyle(GUI.skin.label) 
            { 
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = textSecondary }
            });
            
            // Potential savings
            var savingsRect = new Rect(rect.xMax - savingsWidth - padding, rect.y, savingsWidth, rect.height);
            var savingsStyle = new GUIStyle(GUI.skin.label) 
            { 
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = accentColor }
            };
            GUI.Label(savingsRect, $"▼ {FormatBytes(asset.PotentialSavings)} ({(asset.OptimizationRatio * 100):F0}%)", savingsStyle);
        }
        
        private void DrawSelectionSummary()
        {
            var selected = scannedAssets.Where(a => a.isSelected).ToList();
            if (selected.Count == 0) return;
            
            GUILayout.BeginVertical(cardStyle);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Selected Assets: {selected.Count}", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Total Savings: {FormatBytes(selected.Sum(a => a.PotentialSavings))}", 
                new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, normal = { textColor = accentColor } });
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
        }
        
        private void DrawOptimizationPreview()
        {
            // This would show a preview of changes
            GUILayout.Label("The following optimizations will be applied:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            GUILayout.Space(10);
            
            // Show optimization settings by type
            var typeGroups = selectedAssets.GroupBy(a => a.type);
            foreach (var group in typeGroups)
            {
                GUILayout.BeginVertical(cardStyle);
                GUILayout.Label(group.Key.ToString(), new GUIStyle(GUI.skin.label) 
                { 
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = GetAssetTypeColor(group.Key) }
                });
                
                // Show specific optimizations
                switch (group.Key)
                {
                    case AssetType.Texture:
                        GUILayout.Label($"• Max Size: {currentProfile.textureSettings.maxSize}");
                        GUILayout.Label($"• Compression: {currentProfile.textureSettings.compression}");
                        break;
                    case AssetType.Model:
                        GUILayout.Label($"• Mesh Compression: {currentProfile.modelSettings.meshCompression}");
                        GUILayout.Label($"• Optimize Mesh: {(currentProfile.modelSettings.optimizeMesh ? "Yes" : "No")}");
                        break;
                    case AssetType.Audio:
                        GUILayout.Label($"• Format: {currentProfile.audioSettings.compressionFormat}");
                        GUILayout.Label($"• Quality: {(currentProfile.audioSettings.quality * 100):F0}%");
                        break;
                }
                
                GUILayout.EndVertical();
                GUILayout.Space(5);
            }
        }
        
        private void DrawOptimizationResults()
        {
            GUILayout.BeginHorizontal();
            
            DrawStatCard("Optimized", lastReport.assetsOptimized.ToString(), primaryColor);
            DrawStatCard("Size Before", FormatBytes(lastReport.totalSizeBefore), textSecondary);
            DrawStatCard("Size After", FormatBytes(lastReport.totalSizeAfter), accentColor);
            DrawStatCard("Total Saved", $"{FormatBytes(lastReport.totalSavings)} ({lastReport.savingsPercentage:F1}%)", accentColor);
            
            GUILayout.EndHorizontal();
        }
        
        private void DrawFooter()
        {
            var footerRect = new Rect(0, position.height - 50, position.width, 50);
            EditorGUI.DrawRect(footerRect, bgMedium);
            
            GUILayout.BeginArea(footerRect);
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            
            GUILayout.BeginVertical();
            GUILayout.Space(15);
            
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Settings", EditorStyles.linkLabel))
            {
                SettingsWindow.ShowWindow();
            }
            
            GUILayout.Space(20);
            
            if (GUILayout.Button("Help", EditorStyles.linkLabel))
            {
                Application.OpenURL("https://assetoptimizerpro.com/docs");
            }
            
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            
            GUILayout.FlexibleSpace();
            
            GUILayout.BeginVertical();
            GUILayout.Space(15);
            GUILayout.Label($"Asset Optimizer Pro v{VERSION}", new GUIStyle(GUI.skin.label) 
            { 
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = textSecondary }
            });
            GUILayout.EndVertical();
            
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        
        #region Workflow Methods
        
        private void StartScanning()
        {
            currentState = WorkflowState.Scanning;
            isProcessing = true;
            processProgress = 0f;
            scannedAssets.Clear();
            selectedAssets.Clear();
            
            EditorApplication.delayCall += () =>
            {
                scanner.ScanProject(assetTypeFilters, (progress, status, assets) =>
                {
                    processProgress = progress;
                    processStatus = status;
                    
                    if (progress >= 1f)
                    {
                        isProcessing = false;
                        scannedAssets = assets;
                        
                        ShowNotification(new GUIContent($"Scan complete! Found {scannedAssets.Count} assets."));
                    }
                });
            };
        }
        
        private void CancelScanning()
        {
            // Cancel scanning logic
            isProcessing = false;
            currentState = WorkflowState.Welcome;
        }
        
        private void PrepareSelection()
        {
            // Pre-select assets with high optimization potential
            foreach (var asset in scannedAssets)
            {
                asset.isSelected = asset.PotentialSavings > 1024 * 100; // > 100KB savings
            }
        }
        
        private void StartOptimization()
        {
            selectedAssets = scannedAssets.Where(a => a.isSelected).ToList();
            currentState = WorkflowState.Optimizing;
            
            // Load profile
            currentProfile = profileManager.GetProfile(selectedPlatform);
            
            // Generate preview report
            reportGenerator.GenerateReport(scannedAssets, selectedAssets);
            lastReport = reportGenerator.GetLatestReport();
        }
        
        private void ApplyOptimization()
        {
            isProcessing = true;
            processProgress = 0f;
            
            EditorApplication.delayCall += () =>
            {
                optimizer.OptimizeAssets(selectedAssets, currentProfile, (progress, currentAsset) =>
                {
                    processProgress = progress;
                    processStatus = $"Optimizing: {currentAsset}";
                    
                    if (progress >= 1f)
                    {
                        isProcessing = false;
                        currentState = WorkflowState.Complete;
                        
                        // Generate final report
                        reportGenerator.GenerateReport(scannedAssets, selectedAssets);
                        lastReport = reportGenerator.GetLatestReport();
                        
                        ShowNotification(new GUIContent($"Optimization complete! Saved {FormatBytes(lastReport.totalSavings)}."));
                        
                        AssetDatabase.Refresh();
                    }
                });
            };
        }
        
        private void ResetWorkflow()
        {
            currentState = WorkflowState.Welcome;
            scannedAssets.Clear();
            selectedAssets.Clear();
            lastReport = null;
            processProgress = 0f;
            processStatus = "";
            isProcessing = false;
        }
        
        #endregion
        
        #region Helper Methods
        
        private List<ScannedAsset> GetFilteredAssets()
        {
            var filtered = scannedAssets.AsEnumerable();
            
            // Type filter
            if (filterByType.HasValue)
            {
                filtered = filtered.Where(a => a.type == filterByType.Value);
            }
            
            // Search filter
            if (!string.IsNullOrEmpty(searchFilter))
            {
                filtered = filtered.Where(a => a.name.ToLower().Contains(searchFilter.ToLower()));
            }
            
            return filtered.ToList();
        }
        
        private void SetAllAssetsSelection(bool selected)
        {
            var filtered = GetFilteredAssets();
            foreach (var asset in filtered)
            {
                asset.isSelected = selected;
            }
        }
        
        private void SetMobilePreset()
        {
            foreach (var kvp in assetTypeFilters.ToList())
            {
                assetTypeFilters[kvp.Key] = true;
            }
        }
        
        private void SetVRPreset()
        {
            foreach (var kvp in assetTypeFilters.ToList())
            {
                assetTypeFilters[kvp.Key] = kvp.Key != AssetType.Shader;
            }
        }
        
        private void SetDesktopPreset()
        {
            foreach (var kvp in assetTypeFilters.ToList())
            {
                assetTypeFilters[kvp.Key] = kvp.Key != AssetType.Material && kvp.Key != AssetType.Shader;
            }
        }
        
        private Color GetAssetTypeColor(AssetType type)
        {
            return type switch
            {
                AssetType.Texture => new Color(0.2f, 0.5f, 1f),
                AssetType.Model => new Color(0.1f, 0.7f, 0.5f),
                AssetType.Audio => new Color(0.9f, 0.6f, 0.1f),
                AssetType.Animation => new Color(0.9f, 0.3f, 0.3f),
                AssetType.Material => new Color(0.5f, 0.4f, 0.8f),
                AssetType.Shader => new Color(0.9f, 0.2f, 0.6f),
                _ => Color.gray
            };
        }
        
        private void DrawCircle(Rect rect, Color color)
        {
            var tex = CreateColorTexture(color);
            GUI.DrawTexture(rect, tex, ScaleMode.StretchToFill, true, 0, Color.white, 0, rect.width / 2);
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
        
        private (long totalSize, long optimizableSize, long potentialSavings, float savingsPercentage) CalculateStats()
        {
            long totalSize = scannedAssets.Sum(a => a.currentSize);
            long optimizableSize = scannedAssets.Where(a => a.canOptimize).Sum(a => a.currentSize);
            long potentialSavings = scannedAssets.Sum(a => a.currentSize - a.optimizedSize);
            float savingsPercentage = totalSize > 0 ? (potentialSavings / (float)totalSize) * 100 : 0;
            
            return (totalSize, optimizableSize, potentialSavings, savingsPercentage);
        }
        
        private void LoadPreferences()
        {
            // Load saved preferences
        }
        
        private void SavePreferences()
        {
            // Save current preferences
        }
        
        #endregion
    }
    
    // Settings window
    public class SettingsWindow : EditorWindow
    {
        public static void ShowWindow()
        {
            var window = GetWindow<SettingsWindow>("Settings");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Asset Optimizer Pro Settings", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            EditorGUILayout.HelpBox("Settings will be available here", MessageType.Info);
        }
    }
    
    #region Enums
    
    public enum AssetType
    {
        Texture,
        Model,
        Audio,
        Animation,
        Material,
        Shader
    }
    
    public enum PlatformTarget
    {
        Mobile,
        VR,
        Desktop,
        Console,
        WebGL
    }
    
    public enum TextureCompression
    {
        None,
        LowQuality,
        NormalQuality,
        HighQuality
    }
    
    #endregion
}