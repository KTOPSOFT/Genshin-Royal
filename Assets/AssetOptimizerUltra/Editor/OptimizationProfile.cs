using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace AssetOptimizerPro
{
    [Serializable]
    public class OptimizationProfile
    {
        public string name;
        public string description;
        public bool isDefault;
        public PlatformTarget targetPlatform;
        
        public TextureOptimizationSettings textureSettings;
        public ModelOptimizationSettings modelSettings;
        public AudioOptimizationSettings audioSettings;
        public AnimationOptimizationSettings animationSettings;
        
        public bool useMobileShaders = true;
        public bool enableGPUInstancing = true;
        public bool stripUnusedAssets = true;
        
        public OptimizationProfile()
        {
            textureSettings = new TextureOptimizationSettings();
            modelSettings = new ModelOptimizationSettings();
            audioSettings = new AudioOptimizationSettings();
            animationSettings = new AnimationOptimizationSettings();
        }
        
        public OptimizationProfile Clone()
        {
            var json = JsonUtility.ToJson(this);
            return JsonUtility.FromJson<OptimizationProfile>(json);
        }
    }
    
    [Serializable]
    public class TextureOptimizationSettings
    {
        public int maxSize = 2048;
        public TextureCompression compression = TextureCompression.NormalQuality;
        public bool generateMipmaps = true;
        public bool streamingMipmaps = false;
        public bool readWriteEnabled = false;
        public bool forcePowerOfTwo = false;
        // Note: spritePackingTag is deprecated - use SpriteAtlas instead
        public TextureImporterFormat androidFormat = TextureImporterFormat.ETC2_RGBA8;
        public TextureImporterFormat iosFormat = TextureImporterFormat.PVRTC_RGBA4;
        public TextureImporterFormat webglFormat = TextureImporterFormat.DXT5;
        public int compressionQuality = 50;
    }
    
    [Serializable]
    public class ModelOptimizationSettings
    {
        public ModelImporterMeshCompression meshCompression = ModelImporterMeshCompression.Medium;
        public bool optimizeMesh = true;
        public bool optimizeMeshPolygons = true;
        public bool optimizeMeshVertices = true;
        public bool importMaterials = false;
        public bool importBlendShapes = false;
        public ModelImporterAnimationCompression animationCompression = ModelImporterAnimationCompression.Optimal;
        public float animationPositionError = 0.5f;
        public float animationRotationError = 0.5f;
        public float animationScaleError = 0.5f;
        public bool optimizeGameObjects = true;
        public ModelImporterTangents importTangents = ModelImporterTangents.None;
        public ModelImporterNormals importNormals = ModelImporterNormals.Import;
        public float normalSmoothingAngle = 60f;
        public bool swapUVChannels = false;
        public bool generateSecondaryUV = false;
    }
    
    [Serializable]
    public class AudioOptimizationSettings
    {
        public bool forceToMono = false;
        public AudioClipLoadType loadType = AudioClipLoadType.CompressedInMemory;
        public AudioCompressionFormat compressionFormat = AudioCompressionFormat.Vorbis;
        public float quality = 0.7f;
        public AudioSampleRateSetting sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;
        public AudioCompressionFormat androidFormat = AudioCompressionFormat.Vorbis;
        public AudioCompressionFormat iosFormat = AudioCompressionFormat.MP3;
    }
    
    [Serializable]
    public class AnimationOptimizationSettings
    {
        public ModelImporterAnimationCompression compression = ModelImporterAnimationCompression.Optimal;
        public float positionError = 0.5f;
        public float rotationError = 0.5f;
        public float scaleError = 0.5f;
        public bool optimizeKeyframes = true;
        public int maxKeyframesPerCurve = 100;
        public float keyframeReductionError = 0.01f;
    }
    
    public class ProfileManager
    {
        private const string PROFILES_PATH = "Assets/AssetOptimizerPro/Profiles";
        private const string PROFILE_EXTENSION = ".aoprofile";
        
        private Dictionary<string, OptimizationProfile> profiles;
        
        public ProfileManager()
        {
            LoadProfiles();
        }
        
        public OptimizationProfile GetProfile(PlatformTarget platform)
        {
            // Try to find a matching profile
            var profile = profiles.Values.FirstOrDefault(p => p.targetPlatform == platform);
            
            if (profile == null)
            {
                // Return default profile for platform
                profile = CreateDefaultProfile(platform);
            }
            
            return profile.Clone();
        }
        
        public List<OptimizationProfile> GetAllProfiles()
        {
            return profiles.Values.ToList();
        }
        
        public void SaveProfile(OptimizationProfile profile)
        {
            if (!Directory.Exists(PROFILES_PATH))
            {
                Directory.CreateDirectory(PROFILES_PATH);
            }
            
            var fileName = SanitizeFileName(profile.name) + PROFILE_EXTENSION;
            var path = Path.Combine(PROFILES_PATH, fileName);
            
            var json = JsonUtility.ToJson(profile, true);
            File.WriteAllText(path, json);
            
            AssetDatabase.Refresh();
            
            profiles[profile.name] = profile;
        }
        
        public void DeleteProfile(OptimizationProfile profile)
        {
            if (profile.isDefault) return;
            
            var fileName = SanitizeFileName(profile.name) + PROFILE_EXTENSION;
            var path = Path.Combine(PROFILES_PATH, fileName);
            
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
            }
            
            profiles.Remove(profile.name);
        }
        
        private void LoadProfiles()
        {
            profiles = new Dictionary<string, OptimizationProfile>();
            
            // Create default profiles
            CreateAndAddDefaultProfiles();
            
            // Load custom profiles
            if (Directory.Exists(PROFILES_PATH))
            {
                var files = Directory.GetFiles(PROFILES_PATH, "*" + PROFILE_EXTENSION);
                foreach (var file in files)
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var profile = JsonUtility.FromJson<OptimizationProfile>(json);
                        if (profile != null && !string.IsNullOrEmpty(profile.name))
                        {
                            profiles[profile.name] = profile;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to load profile from {file}: {e.Message}");
                    }
                }
            }
        }
        
        private void CreateAndAddDefaultProfiles()
        {
            // Mobile Profile
            var mobileProfile = CreateDefaultProfile(PlatformTarget.Mobile);
            profiles[mobileProfile.name] = mobileProfile;
            
            // VR Profile
            var vrProfile = CreateDefaultProfile(PlatformTarget.VR);
            profiles[vrProfile.name] = vrProfile;
            
            // Desktop Profile
            var desktopProfile = CreateDefaultProfile(PlatformTarget.Desktop);
            profiles[desktopProfile.name] = desktopProfile;
            
            // Console Profile
            var consoleProfile = CreateDefaultProfile(PlatformTarget.Console);
            profiles[consoleProfile.name] = consoleProfile;
            
            // WebGL Profile
            var webglProfile = CreateDefaultProfile(PlatformTarget.WebGL);
            profiles[webglProfile.name] = webglProfile;
        }
        
        private OptimizationProfile CreateDefaultProfile(PlatformTarget platform)
        {
            var profile = new OptimizationProfile
            {
                name = $"Default {platform}",
                description = $"Optimized settings for {platform} platform",
                isDefault = true,
                targetPlatform = platform
            };
            
            switch (platform)
            {
                case PlatformTarget.Mobile:
                    ConfigureMobileProfile(profile);
                    break;
                case PlatformTarget.VR:
                    ConfigureVRProfile(profile);
                    break;
                case PlatformTarget.Desktop:
                    ConfigureDesktopProfile(profile);
                    break;
                case PlatformTarget.Console:
                    ConfigureConsoleProfile(profile);
                    break;
                case PlatformTarget.WebGL:
                    ConfigureWebGLProfile(profile);
                    break;
            }
            
            return profile;
        }
        
        private void ConfigureMobileProfile(OptimizationProfile profile)
        {
            // Textures - aggressive optimization for mobile
            profile.textureSettings.maxSize = 1024;
            profile.textureSettings.compression = TextureCompression.NormalQuality;
            profile.textureSettings.generateMipmaps = true;
            profile.textureSettings.streamingMipmaps = true;
            profile.textureSettings.androidFormat = TextureImporterFormat.ETC2_RGBA8;
            profile.textureSettings.iosFormat = TextureImporterFormat.PVRTC_RGBA4;
            
            // Models - optimize for performance
            profile.modelSettings.meshCompression = ModelImporterMeshCompression.High;
            profile.modelSettings.optimizeMesh = true;
            profile.modelSettings.optimizeMeshPolygons = true;
            profile.modelSettings.optimizeMeshVertices = true;
            profile.modelSettings.importBlendShapes = false;
            profile.modelSettings.importTangents = ModelImporterTangents.None;
            
            // Audio - balance quality and size
            profile.audioSettings.forceToMono = true;
            profile.audioSettings.loadType = AudioClipLoadType.CompressedInMemory;
            profile.audioSettings.quality = 0.5f;
            
            // Animation - aggressive compression
            profile.animationSettings.compression = ModelImporterAnimationCompression.Optimal;
            profile.animationSettings.positionError = 1f;
            profile.animationSettings.rotationError = 1f;
            profile.animationSettings.scaleError = 1f;
            
            profile.useMobileShaders = true;
            profile.enableGPUInstancing = true;
        }
        
        private void ConfigureVRProfile(OptimizationProfile profile)
        {
            // Textures - high quality but optimized
            profile.textureSettings.maxSize = 2048;
            profile.textureSettings.compression = TextureCompression.HighQuality;
            profile.textureSettings.generateMipmaps = true;
            profile.textureSettings.streamingMipmaps = false;
            
            // Models - balance quality and performance
            profile.modelSettings.meshCompression = ModelImporterMeshCompression.Medium;
            profile.modelSettings.optimizeMesh = true;
            profile.modelSettings.optimizeMeshPolygons = true;
            profile.modelSettings.optimizeMeshVertices = true;
            profile.modelSettings.importBlendShapes = true; // Often needed for avatars
            
            // Audio - spatial audio is important
            profile.audioSettings.forceToMono = false;
            profile.audioSettings.loadType = AudioClipLoadType.CompressedInMemory;
            profile.audioSettings.quality = 0.7f;
            
            // Animation - good quality for smooth VR
            profile.animationSettings.compression = ModelImporterAnimationCompression.Optimal;
            profile.animationSettings.positionError = 0.5f;
            profile.animationSettings.rotationError = 0.5f;
            profile.animationSettings.scaleError = 0.5f;
            
            profile.enableGPUInstancing = true;
        }
        
        private void ConfigureDesktopProfile(OptimizationProfile profile)
        {
            // Textures - high quality
            profile.textureSettings.maxSize = 4096;
            profile.textureSettings.compression = TextureCompression.HighQuality;
            profile.textureSettings.generateMipmaps = true;
            
            // Models - high quality
            profile.modelSettings.meshCompression = ModelImporterMeshCompression.Off;
            profile.modelSettings.optimizeMesh = true;
            profile.modelSettings.optimizeMeshPolygons = true;
            profile.modelSettings.optimizeMeshVertices = true;
            profile.modelSettings.importBlendShapes = true;
            profile.modelSettings.importTangents = ModelImporterTangents.CalculateMikk;
            
            // Audio - high quality
            profile.audioSettings.forceToMono = false;
            profile.audioSettings.loadType = AudioClipLoadType.CompressedInMemory;
            profile.audioSettings.quality = 1f;
            
            // Animation - high quality
            profile.animationSettings.compression = ModelImporterAnimationCompression.Off;
            
            profile.useMobileShaders = false;
            profile.enableGPUInstancing = true;
        }
        
        private void ConfigureConsoleProfile(OptimizationProfile profile)
        {
            // Similar to desktop but with some optimizations
            profile.textureSettings.maxSize = 2048;
            profile.textureSettings.compression = TextureCompression.HighQuality;
            profile.textureSettings.generateMipmaps = true;
            profile.textureSettings.streamingMipmaps = true;
            
            profile.modelSettings.meshCompression = ModelImporterMeshCompression.Low;
            profile.modelSettings.optimizeMesh = true;
            profile.modelSettings.optimizeMeshPolygons = true;
            profile.modelSettings.optimizeMeshVertices = true;
            
            profile.audioSettings.quality = 0.9f;
            
            profile.animationSettings.compression = ModelImporterAnimationCompression.Optimal;
            profile.animationSettings.positionError = 0.2f;
            profile.animationSettings.rotationError = 0.2f;
            
            profile.enableGPUInstancing = true;
        }
        
        private void ConfigureWebGLProfile(OptimizationProfile profile)
        {
            // Aggressive optimization for web
            profile.textureSettings.maxSize = 1024;
            profile.textureSettings.compression = TextureCompression.LowQuality;
            profile.textureSettings.generateMipmaps = true;
            profile.textureSettings.webglFormat = TextureImporterFormat.DXT5;
            
            profile.modelSettings.meshCompression = ModelImporterMeshCompression.High;
            profile.modelSettings.optimizeMesh = true;
            profile.modelSettings.optimizeMeshPolygons = true;
            profile.modelSettings.optimizeMeshVertices = true;
            profile.modelSettings.importBlendShapes = false;
            
            profile.audioSettings.forceToMono = true;
            profile.audioSettings.loadType = AudioClipLoadType.Streaming;
            profile.audioSettings.quality = 0.3f;
            
            profile.animationSettings.compression = ModelImporterAnimationCompression.Optimal;
            profile.animationSettings.positionError = 2f;
            profile.animationSettings.rotationError = 2f;
            
            profile.stripUnusedAssets = true;
        }
        
        private string SanitizeFileName(string fileName)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return string.Join("_", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
        }
    }
    
    // Profile Editor Window
    public class ProfileEditorWindow : EditorWindow
    {
        private OptimizationProfile profile;
        private ProfileManager profileManager;
        private Vector2 scrollPosition;
        
        public static void ShowWindow(OptimizationProfile existingProfile = null)
        {
            var window = GetWindow<ProfileEditorWindow>("Profile Editor");
            window.minSize = new Vector2(500, 600);
            window.Initialize(existingProfile);
            window.Show();
        }
        
        private void Initialize(OptimizationProfile existingProfile)
        {
            profileManager = new ProfileManager();
            
            if (existingProfile != null)
            {
                profile = existingProfile.Clone();
            }
            else
            {
                profile = new OptimizationProfile
                {
                    name = "New Profile",
                    description = "Custom optimization profile"
                };
            }
        }
        
        private void OnGUI()
        {
            if (profile == null) return;
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // Header
            EditorGUILayout.LabelField("Profile Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Basic info
            GUI.enabled = !profile.isDefault;
            profile.name = EditorGUILayout.TextField("Name", profile.name);
            GUI.enabled = true;
            
            profile.description = EditorGUILayout.TextField("Description", profile.description);
            profile.targetPlatform = (PlatformTarget)EditorGUILayout.EnumPopup("Target Platform", profile.targetPlatform);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
            
            // Texture settings
            DrawTextureSettings();
            
            EditorGUILayout.Space();
            
            // Model settings
            DrawModelSettings();
            
            EditorGUILayout.Space();
            
            // Audio settings
            DrawAudioSettings();
            
            EditorGUILayout.Space();
            
            // Animation settings
            DrawAnimationSettings();
            
            EditorGUILayout.Space();
            
            // General settings
            DrawGeneralSettings();
            
            EditorGUILayout.EndScrollView();
            
            // Bottom buttons
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Save", GUILayout.Height(30)))
            {
                SaveProfile();
            }
            
            if (GUILayout.Button("Cancel", GUILayout.Height(30)))
            {
                Close();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawTextureSettings()
        {
            profile.textureSettings = profile.textureSettings ?? new TextureOptimizationSettings();
            
            EditorGUILayout.LabelField("Texture Optimization", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            profile.textureSettings.maxSize = EditorGUILayout.IntPopup("Max Size", 
                profile.textureSettings.maxSize,
                new string[] { "256", "512", "1024", "2048", "4096", "8192" },
                new int[] { 256, 512, 1024, 2048, 4096, 8192 });
            
            profile.textureSettings.compression = (TextureCompression)EditorGUILayout.EnumPopup("Compression", profile.textureSettings.compression);
            profile.textureSettings.generateMipmaps = EditorGUILayout.Toggle("Generate Mipmaps", profile.textureSettings.generateMipmaps);
            
            if (profile.textureSettings.generateMipmaps)
            {
                EditorGUI.indentLevel++;
                profile.textureSettings.streamingMipmaps = EditorGUILayout.Toggle("Streaming Mipmaps", profile.textureSettings.streamingMipmaps);
                EditorGUI.indentLevel--;
            }
            
            profile.textureSettings.readWriteEnabled = EditorGUILayout.Toggle("Read/Write Enabled", profile.textureSettings.readWriteEnabled);
            profile.textureSettings.forcePowerOfTwo = EditorGUILayout.Toggle("Force Power of Two", profile.textureSettings.forcePowerOfTwo);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Platform Formats", EditorStyles.miniLabel);
            profile.textureSettings.androidFormat = (TextureImporterFormat)EditorGUILayout.EnumPopup("Android", profile.textureSettings.androidFormat);
            profile.textureSettings.iosFormat = (TextureImporterFormat)EditorGUILayout.EnumPopup("iOS", profile.textureSettings.iosFormat);
            profile.textureSettings.webglFormat = (TextureImporterFormat)EditorGUILayout.EnumPopup("WebGL", profile.textureSettings.webglFormat);
            
            profile.textureSettings.compressionQuality = EditorGUILayout.IntSlider("Compression Quality", profile.textureSettings.compressionQuality, 0, 100);
            
            EditorGUI.indentLevel--;
        }
        
        private void DrawModelSettings()
        {
            profile.modelSettings = profile.modelSettings ?? new ModelOptimizationSettings();
            
            EditorGUILayout.LabelField("Model Optimization", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            profile.modelSettings.meshCompression = (ModelImporterMeshCompression)EditorGUILayout.EnumPopup("Mesh Compression", profile.modelSettings.meshCompression);
            profile.modelSettings.optimizeMesh = EditorGUILayout.Toggle("Optimize Mesh", profile.modelSettings.optimizeMesh);
            
            if (profile.modelSettings.optimizeMesh)
            {
                EditorGUI.indentLevel++;
                profile.modelSettings.optimizeMeshPolygons = EditorGUILayout.Toggle("Optimize Polygons", profile.modelSettings.optimizeMeshPolygons);
                profile.modelSettings.optimizeMeshVertices = EditorGUILayout.Toggle("Optimize Vertices", profile.modelSettings.optimizeMeshVertices);
                EditorGUI.indentLevel--;
            }
            
            profile.modelSettings.importMaterials = EditorGUILayout.Toggle("Import Materials", profile.modelSettings.importMaterials);
            profile.modelSettings.importBlendShapes = EditorGUILayout.Toggle("Import Blend Shapes", profile.modelSettings.importBlendShapes);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Animation", EditorStyles.miniLabel);
            profile.modelSettings.animationCompression = (ModelImporterAnimationCompression)EditorGUILayout.EnumPopup("Compression", profile.modelSettings.animationCompression);
            
            if (profile.modelSettings.animationCompression != ModelImporterAnimationCompression.Off)
            {
                profile.modelSettings.animationPositionError = EditorGUILayout.Slider("Position Error", profile.modelSettings.animationPositionError, 0.01f, 5f);
                profile.modelSettings.animationRotationError = EditorGUILayout.Slider("Rotation Error", profile.modelSettings.animationRotationError, 0.01f, 5f);
                profile.modelSettings.animationScaleError = EditorGUILayout.Slider("Scale Error", profile.modelSettings.animationScaleError, 0.01f, 5f);
            }
            
            profile.modelSettings.optimizeGameObjects = EditorGUILayout.Toggle("Optimize Game Objects", profile.modelSettings.optimizeGameObjects);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Import Settings", EditorStyles.miniLabel);
            profile.modelSettings.importTangents = (ModelImporterTangents)EditorGUILayout.EnumPopup("Tangents", profile.modelSettings.importTangents);
            profile.modelSettings.importNormals = (ModelImporterNormals)EditorGUILayout.EnumPopup("Normals", profile.modelSettings.importNormals);
            
            if (profile.modelSettings.importNormals == ModelImporterNormals.Calculate)
            {
                profile.modelSettings.normalSmoothingAngle = EditorGUILayout.Slider("Smoothing Angle", profile.modelSettings.normalSmoothingAngle, 0f, 180f);
            }
            
            profile.modelSettings.swapUVChannels = EditorGUILayout.Toggle("Swap UV Channels", profile.modelSettings.swapUVChannels);
            profile.modelSettings.generateSecondaryUV = EditorGUILayout.Toggle("Generate Lightmap UVs", profile.modelSettings.generateSecondaryUV);
            
            EditorGUI.indentLevel--;
        }
        
        private void DrawAudioSettings()
        {
            profile.audioSettings = profile.audioSettings ?? new AudioOptimizationSettings();
            
            EditorGUILayout.LabelField("Audio Optimization", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            profile.audioSettings.forceToMono = EditorGUILayout.Toggle("Force to Mono", profile.audioSettings.forceToMono);
            profile.audioSettings.loadType = (AudioClipLoadType)EditorGUILayout.EnumPopup("Load Type", profile.audioSettings.loadType);
            profile.audioSettings.compressionFormat = (AudioCompressionFormat)EditorGUILayout.EnumPopup("Compression Format", profile.audioSettings.compressionFormat);
            profile.audioSettings.quality = EditorGUILayout.Slider("Quality", profile.audioSettings.quality, 0.01f, 1f);
            profile.audioSettings.sampleRateSetting = (AudioSampleRateSetting)EditorGUILayout.EnumPopup("Sample Rate", profile.audioSettings.sampleRateSetting);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Platform Formats", EditorStyles.miniLabel);
            profile.audioSettings.androidFormat = (AudioCompressionFormat)EditorGUILayout.EnumPopup("Android", profile.audioSettings.androidFormat);
            profile.audioSettings.iosFormat = (AudioCompressionFormat)EditorGUILayout.EnumPopup("iOS", profile.audioSettings.iosFormat);
            
            EditorGUI.indentLevel--;
        }
        
        private void DrawAnimationSettings()
        {
            profile.animationSettings = profile.animationSettings ?? new AnimationOptimizationSettings();
            
            EditorGUILayout.LabelField("Animation Optimization", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            profile.animationSettings.compression = (ModelImporterAnimationCompression)EditorGUILayout.EnumPopup("Compression", profile.animationSettings.compression);
            
            if (profile.animationSettings.compression != ModelImporterAnimationCompression.Off)
            {
                profile.animationSettings.positionError = EditorGUILayout.Slider("Position Error", profile.animationSettings.positionError, 0.01f, 5f);
                profile.animationSettings.rotationError = EditorGUILayout.Slider("Rotation Error", profile.animationSettings.rotationError, 0.01f, 5f);
                profile.animationSettings.scaleError = EditorGUILayout.Slider("Scale Error", profile.animationSettings.scaleError, 0.01f, 5f);
            }
            
            profile.animationSettings.optimizeKeyframes = EditorGUILayout.Toggle("Optimize Keyframes", profile.animationSettings.optimizeKeyframes);
            
            if (profile.animationSettings.optimizeKeyframes)
            {
                EditorGUI.indentLevel++;
                profile.animationSettings.maxKeyframesPerCurve = EditorGUILayout.IntSlider("Max Keyframes/Curve", profile.animationSettings.maxKeyframesPerCurve, 10, 500);
                profile.animationSettings.keyframeReductionError = EditorGUILayout.Slider("Reduction Error", profile.animationSettings.keyframeReductionError, 0.001f, 0.1f);
                EditorGUI.indentLevel--;
            }
            
            EditorGUI.indentLevel--;
        }
        
        private void DrawGeneralSettings()
        {
            EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            profile.useMobileShaders = EditorGUILayout.Toggle("Use Mobile Shaders", profile.useMobileShaders);
            profile.enableGPUInstancing = EditorGUILayout.Toggle("Enable GPU Instancing", profile.enableGPUInstancing);
            profile.stripUnusedAssets = EditorGUILayout.Toggle("Strip Unused Assets", profile.stripUnusedAssets);
            
            EditorGUI.indentLevel--;
        }
        
        private void SaveProfile()
        {
            if (string.IsNullOrEmpty(profile.name))
            {
                EditorUtility.DisplayDialog("Error", "Profile name cannot be empty", "OK");
                return;
            }
            
            profileManager.SaveProfile(profile);
            EditorUtility.DisplayDialog("Success", $"Profile '{profile.name}' saved successfully", "OK");
            Close();
        }
    }
}