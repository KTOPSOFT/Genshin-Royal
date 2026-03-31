using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace AssetOptimizerPro
{
    public class AssetOptimizer
    {
        public float Progress { get; private set; }
        public string CurrentAsset { get; private set; }
        
        private bool createBackup;
        
        public delegate void OptimizationProgressCallback(float progress, string currentAsset);
        
        public AssetOptimizer()
        {
            createBackup = EditorPrefs.GetBool("AOP_CreateBackup", true);
        }
        
        public void OptimizeAssets(List<ScannedAsset> assets, OptimizationProfile profile, OptimizationProgressCallback callback)
        {
            Progress = 0f;
            var totalAssets = assets.Count;
            var processedAssets = 0;
            
            // Create backup folder if needed
            string backupFolder = null;
            if (createBackup)
            {
                backupFolder = CreateBackupFolder();
            }
            
            AssetDatabase.StartAssetEditing();
            
            try
            {
                foreach (var asset in assets)
                {
                    CurrentAsset = asset.name;
                    
                    // Create backup
                    if (createBackup)
                    {
                        BackupAsset(asset.path, backupFolder);
                    }
                    
                    // Optimize based on type
                    bool optimized = false;
                    switch (asset.type)
                    {
                        case AssetType.Texture:
                            optimized = OptimizeTexture(asset, profile.textureSettings);
                            break;
                        case AssetType.Model:
                            optimized = OptimizeModel(asset, profile.modelSettings);
                            break;
                        case AssetType.Audio:
                            optimized = OptimizeAudio(asset, profile.audioSettings);
                            break;
                        case AssetType.Animation:
                            optimized = OptimizeAnimation(asset, profile.animationSettings);
                            break;
                        case AssetType.Material:
                            optimized = OptimizeMaterial(asset, profile);
                            break;
                        case AssetType.Shader:
                            optimized = OptimizeShader(asset, profile);
                            break;
                    }
                    
                    if (optimized)
                    {
                        Debug.Log($"[Asset Optimizer Pro] Optimized: {asset.name}");
                    }
                    
                    processedAssets++;
                    Progress = (float)processedAssets / totalAssets;
                    callback?.Invoke(Progress, CurrentAsset);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            
            callback?.Invoke(1f, "Optimization complete");
        }
        
        private string CreateBackupFolder()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupPath = $"AssetOptimizerPro_Backups/Backup_{timestamp}";
            
            if (!Directory.Exists(backupPath))
            {
                Directory.CreateDirectory(backupPath);
            }
            
            return backupPath;
        }
        
        private void BackupAsset(string assetPath, string backupFolder)
        {
            try
            {
                var fileName = Path.GetFileName(assetPath);
                var relativePath = Path.GetDirectoryName(assetPath);
                var backupPath = Path.Combine(backupFolder, relativePath);
                
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }
                
                var sourcePath = assetPath;
                var destPath = Path.Combine(backupPath, fileName);
                
                File.Copy(sourcePath, destPath, true);
                
                // Also copy meta file
                var metaSource = sourcePath + ".meta";
                var metaDest = destPath + ".meta";
                if (File.Exists(metaSource))
                {
                    File.Copy(metaSource, metaDest, true);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to backup {assetPath}: {e.Message}");
            }
        }
        
        private bool OptimizeTexture(ScannedAsset asset, TextureOptimizationSettings settings)
        {
            var importer = AssetImporter.GetAtPath(asset.path) as TextureImporter;
            if (importer == null) return false;
            
            bool changed = false;
            
            // Apply max texture size
            if (importer.maxTextureSize > settings.maxSize)
            {
                importer.maxTextureSize = settings.maxSize;
                changed = true;
            }
            
            // Apply compression
            var targetCompression = GetTextureCompression(settings.compression);
            if (importer.textureCompression != targetCompression)
            {
                importer.textureCompression = targetCompression;
                changed = true;
            }
            
            // Apply format based on platform
            var platformSettings = GetPlatformTextureSettings(settings);
            foreach (var platform in platformSettings)
            {
                var currentSettings = importer.GetPlatformTextureSettings(platform.Key);
                
                if (!currentSettings.overridden || currentSettings.format != platform.Value.format)
                {
                    currentSettings.overridden = true;
                    currentSettings.format = platform.Value.format;
                    currentSettings.maxTextureSize = platform.Value.maxTextureSize;
                    currentSettings.compressionQuality = platform.Value.compressionQuality;
                    
                    importer.SetPlatformTextureSettings(currentSettings);
                    changed = true;
                }
            }
            
            // Mipmaps
            if (importer.textureType == TextureImporterType.Default)
            {
                if (importer.mipmapEnabled != settings.generateMipmaps)
                {
                    importer.mipmapEnabled = settings.generateMipmaps;
                    changed = true;
                }
                
                if (settings.generateMipmaps && importer.streamingMipmaps != settings.streamingMipmaps)
                {
                    importer.streamingMipmaps = settings.streamingMipmaps;
                    changed = true;
                }
            }
            
            // Disable Read/Write if enabled
            if (importer.isReadable && !settings.readWriteEnabled)
            {
                importer.isReadable = false;
                changed = true;
            }
            
            // Apply POT (Power of Two) scaling if needed
            if (settings.forcePowerOfTwo && importer.npotScale != TextureImporterNPOTScale.ToNearest)
            {
                importer.npotScale = TextureImporterNPOTScale.ToNearest;
                changed = true;
            }
            
            // Note: spritePackingTag is deprecated in newer Unity versions
            // Users should use SpriteAtlas instead for sprite packing
            
            if (changed)
            {
                importer.SaveAndReimport();
            }
            
            return changed;
        }
        
        private bool OptimizeModel(ScannedAsset asset, ModelOptimizationSettings settings)
        {
            var importer = AssetImporter.GetAtPath(asset.path) as ModelImporter;
            if (importer == null) return false;
            
            bool changed = false;
            
            // Mesh compression
            if (importer.meshCompression != settings.meshCompression)
            {
                importer.meshCompression = settings.meshCompression;
                changed = true;
            }
            
            // Optimize mesh - using new API
            if (settings.optimizeMesh)
            {
                if (importer.optimizeMeshPolygons != settings.optimizeMeshPolygons)
                {
                    importer.optimizeMeshPolygons = settings.optimizeMeshPolygons;
                    changed = true;
                }
                
                if (importer.optimizeMeshVertices != settings.optimizeMeshVertices)
                {
                    importer.optimizeMeshVertices = settings.optimizeMeshVertices;
                    changed = true;
                }
            }
            else
            {
                // When optimization is off, set polygons to true and vertices to false
                // This matches the old optimizeMesh = false behavior
                if (importer.optimizeMeshPolygons != true)
                {
                    importer.optimizeMeshPolygons = true;
                    changed = true;
                }
                
                if (importer.optimizeMeshVertices != false)
                {
                    importer.optimizeMeshVertices = false;
                    changed = true;
                }
            }
            
            // Disable unnecessary imports
            if (importer.importCameras)
            {
                importer.importCameras = false;
                changed = true;
            }
            
            if (importer.importLights)
            {
                importer.importLights = false;
                changed = true;
            }
            
            // Material settings
            if (settings.importMaterials == false && importer.materialImportMode != ModelImporterMaterialImportMode.None)
            {
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                changed = true;
            }
            
            // Animation settings
            if (importer.animationType != ModelImporterAnimationType.None)
            {
                if (importer.animationCompression != settings.animationCompression)
                {
                    importer.animationCompression = settings.animationCompression;
                    changed = true;
                }
                
                // Set compression errors
                if (settings.animationCompression != ModelImporterAnimationCompression.Off)
                {
                    importer.animationPositionError = settings.animationPositionError;
                    importer.animationRotationError = settings.animationRotationError;
                    importer.animationScaleError = settings.animationScaleError;
                    changed = true;
                }
                
                // Optimize animation curves
                if (importer.optimizeGameObjects != settings.optimizeGameObjects)
                {
                    importer.optimizeGameObjects = settings.optimizeGameObjects;
                    changed = true;
                }
            }
            
            // Tangent settings
            if (importer.importTangents != settings.importTangents)
            {
                importer.importTangents = settings.importTangents;
                changed = true;
            }
            
            // Normal settings
            if (importer.importNormals != settings.importNormals)
            {
                importer.importNormals = settings.importNormals;
                changed = true;
            }
            
            if (settings.importNormals == ModelImporterNormals.Calculate)
            {
                importer.normalSmoothingAngle = settings.normalSmoothingAngle;
            }
            
            // Blend shapes
            if (importer.importBlendShapes != settings.importBlendShapes)
            {
                importer.importBlendShapes = settings.importBlendShapes;
                changed = true;
            }
            
            // Swap UVs if needed
            if (importer.swapUVChannels != settings.swapUVChannels)
            {
                importer.swapUVChannels = settings.swapUVChannels;
                changed = true;
            }
            
            // Generate secondary UV for lightmapping
            if (importer.generateSecondaryUV != settings.generateSecondaryUV)
            {
                importer.generateSecondaryUV = settings.generateSecondaryUV;
                changed = true;
            }
            
            if (changed)
            {
                importer.SaveAndReimport();
            }
            
            return changed;
        }
        
        private bool OptimizeAudio(ScannedAsset asset, AudioOptimizationSettings settings)
        {
            var importer = AssetImporter.GetAtPath(asset.path) as AudioImporter;
            if (importer == null) return false;
            
            bool changed = false;
            var audioSettings = importer.defaultSampleSettings;
            
            // Force to mono
            if (settings.forceToMono && importer.forceToMono != true)
            {
                importer.forceToMono = true;
                changed = true;
            }
            
            // Load type
            if (audioSettings.loadType != settings.loadType)
            {
                audioSettings.loadType = settings.loadType;
                changed = true;
            }
            
            // Compression format
            var targetFormat = GetAudioCompressionFormat(settings);
            if (audioSettings.compressionFormat != targetFormat)
            {
                audioSettings.compressionFormat = targetFormat;
                changed = true;
            }
            
            // Quality
            if (audioSettings.quality != settings.quality)
            {
                audioSettings.quality = settings.quality;
                changed = true;
            }
            
            // Sample rate
            if (audioSettings.sampleRateSetting != settings.sampleRateSetting)
            {
                audioSettings.sampleRateSetting = settings.sampleRateSetting;
                changed = true;
            }
            
            // Apply platform-specific overrides
            var platformSettings = GetPlatformAudioSettings(settings);
            foreach (var platform in platformSettings)
            {
                var overrideSettings = importer.GetOverrideSampleSettings(platform.Key);
                
                if (!importer.ContainsSampleSettingsOverride(platform.Key) || 
                    overrideSettings.compressionFormat != platform.Value.compressionFormat ||
                    overrideSettings.quality != platform.Value.quality)
                {
                    importer.SetOverrideSampleSettings(platform.Key, platform.Value);
                    changed = true;
                }
            }
            
            if (changed)
            {
                importer.defaultSampleSettings = audioSettings;
                importer.SaveAndReimport();
            }
            
            return changed;
        }
        
        private bool OptimizeAnimation(ScannedAsset asset, AnimationOptimizationSettings settings)
        {
            // Check if it's from a model
            var modelImporter = AssetImporter.GetAtPath(asset.path) as ModelImporter;
            if (modelImporter != null)
            {
                bool changed = false;
                
                if (modelImporter.animationCompression != settings.compression)
                {
                    modelImporter.animationCompression = settings.compression;
                    changed = true;
                }
                
                if (modelImporter.animationPositionError != settings.positionError)
                {
                    modelImporter.animationPositionError = settings.positionError;
                    changed = true;
                }
                
                if (modelImporter.animationRotationError != settings.rotationError)
                {
                    modelImporter.animationRotationError = settings.rotationError;
                    changed = true;
                }
                
                if (modelImporter.animationScaleError != settings.scaleError)
                {
                    modelImporter.animationScaleError = settings.scaleError;
                    changed = true;
                }
                
                if (changed)
                {
                    modelImporter.SaveAndReimport();
                }
                
                return changed;
            }
            
            // For standalone animation clips, we can optimize keyframes
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(asset.path);
            if (clip != null && settings.optimizeKeyframes)
            {
                OptimizeAnimationKeyframes(clip, settings);
                EditorUtility.SetDirty(clip);
                return true;
            }
            
            return false;
        }
        
        private void OptimizeAnimationKeyframes(AnimationClip clip, AnimationOptimizationSettings settings)
        {
            var bindings = AnimationUtility.GetCurveBindings(clip);
            
            foreach (var binding in bindings)
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                if (curve.keys.Length > settings.maxKeyframesPerCurve)
                {
                    // Simplify curve by reducing keyframes
                    var simplifiedCurve = SimplifyAnimationCurve(curve, settings.keyframeReductionError);
                    AnimationUtility.SetEditorCurve(clip, binding, simplifiedCurve);
                }
            }
        }
        
        private AnimationCurve SimplifyAnimationCurve(AnimationCurve curve, float tolerance)
        {
            if (curve.keys.Length <= 2) return curve;
            
            var simplified = new AnimationCurve();
            var keys = curve.keys;
            
            // Always keep first and last keys
            simplified.AddKey(keys[0]);
            
            // Douglas-Peucker algorithm for curve simplification
            var toKeep = new bool[keys.Length];
            toKeep[0] = true;
            toKeep[keys.Length - 1] = true;
            
            SimplifyRecursive(curve, 0, keys.Length - 1, tolerance, toKeep);
            
            // Add kept keys to simplified curve
            for (int i = 1; i < keys.Length - 1; i++)
            {
                if (toKeep[i])
                {
                    simplified.AddKey(keys[i]);
                }
            }
            
            simplified.AddKey(keys[keys.Length - 1]);
            
            return simplified;
        }
        
        private void SimplifyRecursive(AnimationCurve curve, int start, int end, float tolerance, bool[] toKeep)
        {
            if (end - start <= 1) return;
            
            float maxDistance = 0;
            int maxIndex = 0;
            
            var keys = curve.keys;
            
            for (int i = start + 1; i < end; i++)
            {
                float distance = PerpendicularDistance(keys[start], keys[end], keys[i]);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    maxIndex = i;
                }
            }
            
            if (maxDistance > tolerance)
            {
                toKeep[maxIndex] = true;
                SimplifyRecursive(curve, start, maxIndex, tolerance, toKeep);
                SimplifyRecursive(curve, maxIndex, end, tolerance, toKeep);
            }
        }
        
        private float PerpendicularDistance(Keyframe start, Keyframe end, Keyframe point)
        {
            float dx = end.time - start.time;
            float dy = end.value - start.value;
            
            float mag = Mathf.Sqrt(dx * dx + dy * dy);
            if (mag > 0)
            {
                dx /= mag;
                dy /= mag;
            }
            
            float pvx = point.time - start.time;
            float pvy = point.value - start.value;
            
            float pvdot = dx * pvx + dy * pvy;
            
            float ax = pvx - pvdot * dx;
            float ay = pvy - pvdot * dy;
            
            return Mathf.Sqrt(ax * ax + ay * ay);
        }
        
        private bool OptimizeMaterial(ScannedAsset asset, OptimizationProfile profile)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(asset.path);
            if (material == null) return false;
            
            bool changed = false;
            
            // Remove default textures
            var shader = material.shader;
            var propertyCount = ShaderUtil.GetPropertyCount(shader);
            
            for (int i = 0; i < propertyCount; i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    var propName = ShaderUtil.GetPropertyName(shader, i);
                    var texture = material.GetTexture(propName);
                    
                    if (texture != null && IsDefaultTexture(texture))
                    {
                        material.SetTexture(propName, null);
                        changed = true;
                    }
                }
            }
            
            // Switch to mobile shader if applicable
            if (profile.useMobileShaders && shader.name.Contains("Standard") && !shader.name.Contains("Mobile"))
            {
                var mobileShader = Shader.Find("Mobile/Diffuse");
                if (mobileShader != null)
                {
                    material.shader = mobileShader;
                    changed = true;
                }
            }
            
            // Optimize render queue
            if (material.renderQueue > 3000 && material.HasProperty("_Color"))
            {
                var color = material.GetColor("_Color");
                if (color.a >= 0.99f)
                {
                    material.renderQueue = 2000; // Opaque
                    changed = true;
                }
            }
            
            // Enable GPU instancing if supported
            if (profile.enableGPUInstancing && material.enableInstancing == false)
            {
                material.enableInstancing = true;
                changed = true;
            }
            
            if (changed)
            {
                EditorUtility.SetDirty(material);
            }
            
            return changed;
        }
        
        private bool OptimizeShader(ScannedAsset asset, OptimizationProfile profile)
        {
            // Shader optimization is complex and usually involves:
            // 1. Stripping unused variants
            // 2. Simplifying shader code
            // 3. Using shader variant collections
            
            // For now, we'll just log recommendations
            Debug.Log($"[Asset Optimizer Pro] Shader '{asset.name}' may benefit from variant stripping. Consider using Shader Variant Collections.");
            
            return false;
        }
        
        private TextureImporterCompression GetTextureCompression(TextureCompression compression)
        {
            return compression switch
            {
                TextureCompression.None => TextureImporterCompression.Uncompressed,
                TextureCompression.LowQuality => TextureImporterCompression.CompressedLQ,
                TextureCompression.NormalQuality => TextureImporterCompression.Compressed,
                TextureCompression.HighQuality => TextureImporterCompression.CompressedHQ,
                _ => TextureImporterCompression.Compressed
            };
        }
        
        private Dictionary<string, TextureImporterPlatformSettings> GetPlatformTextureSettings(TextureOptimizationSettings settings)
        {
            var platformSettings = new Dictionary<string, TextureImporterPlatformSettings>();
            
            // Android
            var android = new TextureImporterPlatformSettings
            {
                name = "Android",
                overridden = true,
                maxTextureSize = settings.maxSize,
                format = settings.androidFormat,
                compressionQuality = (int)settings.compressionQuality
            };
            platformSettings["Android"] = android;
            
            // iOS
            var ios = new TextureImporterPlatformSettings
            {
                name = "iPhone",
                overridden = true,
                maxTextureSize = settings.maxSize,
                format = settings.iosFormat,
                compressionQuality = (int)settings.compressionQuality
            };
            platformSettings["iPhone"] = ios;
            
            // WebGL
            var webgl = new TextureImporterPlatformSettings
            {
                name = "WebGL",
                overridden = true,
                maxTextureSize = settings.maxSize,
                format = settings.webglFormat,
                compressionQuality = (int)settings.compressionQuality
            };
            platformSettings["WebGL"] = webgl;
            
            return platformSettings;
        }
        
        private AudioCompressionFormat GetAudioCompressionFormat(AudioOptimizationSettings settings)
        {
            return settings.compressionFormat;
        }
        
        private Dictionary<string, AudioImporterSampleSettings> GetPlatformAudioSettings(AudioOptimizationSettings settings)
        {
            var platformSettings = new Dictionary<string, AudioImporterSampleSettings>();
            
            // Android
            var android = new AudioImporterSampleSettings
            {
                loadType = settings.loadType,
                compressionFormat = settings.androidFormat,
                quality = settings.quality,
                sampleRateSetting = settings.sampleRateSetting
            };
            platformSettings["Android"] = android;
            
            // iOS
            var ios = new AudioImporterSampleSettings
            {
                loadType = settings.loadType,
                compressionFormat = settings.iosFormat,
                quality = settings.quality,
                sampleRateSetting = settings.sampleRateSetting
            };
            platformSettings["iOS"] = ios;
            
            return platformSettings;
        }
        
        private bool IsDefaultTexture(Texture texture)
        {
            var path = AssetDatabase.GetAssetPath(texture);
            return string.IsNullOrEmpty(path) || path.StartsWith("Library/") || path.StartsWith("Packages/");
        }
    }
}