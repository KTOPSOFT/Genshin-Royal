using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace AssetOptimizerPro
{
    public class AssetScannerSystem
    {
        private Dictionary<AssetType, string[]> assetExtensions = new Dictionary<AssetType, string[]>
        {
            { AssetType.Texture, new[] { ".png", ".jpg", ".jpeg", ".tga", ".bmp", ".psd", ".tiff", ".tif", ".gif", ".exr" } },
            { AssetType.Model, new[] { ".fbx", ".obj", ".dae", ".3ds", ".blend", ".max", ".ma", ".mb", ".ase" } },
            { AssetType.Audio, new[] { ".wav", ".mp3", ".ogg", ".aiff", ".aif", ".mod", ".it", ".s3m", ".xm" } },
            { AssetType.Animation, new[] { ".anim", ".controller", ".overrideController", ".mask" } },
            { AssetType.Material, new[] { ".mat" } },
            { AssetType.Shader, new[] { ".shader", ".shadergraph", ".shadersubgraph" } }
        };
        
        public delegate void ScanProgressCallback(float progress, string status, List<ScannedAsset> assets);
        
        public void ScanProject(Dictionary<AssetType, bool> typeFilters, ScanProgressCallback callback)
        {
            var allAssets = new List<ScannedAsset>();
            var assetPaths = GetAllAssetPaths(typeFilters);
            var totalAssets = assetPaths.Count;
            var processedAssets = 0;
            
            foreach (var path in assetPaths)
            {
                var asset = AnalyzeAsset(path);
                if (asset != null && asset.canOptimize)
                {
                    allAssets.Add(asset);
                }
                
                processedAssets++;
                float progress = (float)processedAssets / totalAssets;
                string status = $"Scanning: {Path.GetFileName(path)} ({processedAssets}/{totalAssets})";
                
                callback?.Invoke(progress, status, allAssets);
                
                // Update progress bar periodically
                if (processedAssets % 10 == 0)
                {
                    EditorUtility.DisplayProgressBar("Asset Scanner", status, progress);
                }
            }
            
            EditorUtility.ClearProgressBar();
            callback?.Invoke(1f, "Scan complete", allAssets);
        }
        
        private List<string> GetAllAssetPaths(Dictionary<AssetType, bool> typeFilters)
        {
            var paths = new List<string>();
            var searchFolders = new[] { "Assets" };
            
            foreach (var kvp in typeFilters)
            {
                if (!kvp.Value) continue;
                
                var extensions = assetExtensions[kvp.Key];
                foreach (var ext in extensions)
                {
                    var filter = $"t:{GetUnityTypeFilter(kvp.Key)}";
                    var guids = AssetDatabase.FindAssets(filter, searchFolders);
                    
                    foreach (var guid in guids)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        if (path.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                        {
                            paths.Add(path);
                        }
                    }
                }
            }
            
            return paths.Distinct().ToList();
        }
        
        private string GetUnityTypeFilter(AssetType type)
        {
            return type switch
            {
                AssetType.Texture => "Texture",
                AssetType.Model => "Model",
                AssetType.Audio => "AudioClip",
                AssetType.Animation => "AnimationClip",
                AssetType.Material => "Material",
                AssetType.Shader => "Shader",
                _ => ""
            };
        }
        
        private ScannedAsset AnalyzeAsset(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (asset == null) return null;
            
            var fileInfo = new FileInfo(path);
            var assetType = GetAssetType(path);
            
            var scannedAsset = new ScannedAsset
            {
                path = path,
                name = asset.name,
                type = assetType,
                currentSize = fileInfo.Length,
                isSelected = true
            };
            
            // Analyze based on type
            switch (assetType)
            {
                case AssetType.Texture:
                    AnalyzeTexture(scannedAsset, asset as Texture2D);
                    break;
                case AssetType.Model:
                    AnalyzeModel(scannedAsset, path);
                    break;
                case AssetType.Audio:
                    AnalyzeAudio(scannedAsset, asset as AudioClip);
                    break;
                case AssetType.Animation:
                    AnalyzeAnimation(scannedAsset, asset as AnimationClip);
                    break;
                case AssetType.Material:
                    AnalyzeMaterial(scannedAsset, asset as Material);
                    break;
                case AssetType.Shader:
                    AnalyzeShader(scannedAsset, asset as Shader);
                    break;
            }
            
            return scannedAsset;
        }
        
        private AssetType GetAssetType(string path)
        {
            var extension = Path.GetExtension(path).ToLower();
            
            foreach (var kvp in assetExtensions)
            {
                if (kvp.Value.Contains(extension))
                {
                    return kvp.Key;
                }
            }
            
            return AssetType.Texture; // Default
        }
        
        private void AnalyzeTexture(ScannedAsset asset, Texture2D texture)
        {
            if (texture == null) return;
            
            var importer = AssetImporter.GetAtPath(asset.path) as TextureImporter;
            if (importer == null) return;
            
            asset.canOptimize = false;
            asset.optimizationReasons = new List<string>();
            
            // Check size
            if (texture.width > 2048 || texture.height > 2048)
            {
                asset.canOptimize = true;
                asset.optimizationReasons.Add($"Large texture size: {texture.width}x{texture.height}");
            }
            
            // Check compression
            if (importer.textureCompression == TextureImporterCompression.Uncompressed)
            {
                asset.canOptimize = true;
                asset.optimizationReasons.Add("No compression applied");
            }
            
            // Check format
            if (importer.textureType == TextureImporterType.Default && !importer.sRGBTexture)
            {
                // Linear textures that might be used as sRGB
                asset.canOptimize = true;
                asset.optimizationReasons.Add("Linear color space for regular texture");
            }
            
            // Check mipmaps
            if (importer.textureType == TextureImporterType.Default && !importer.mipmapEnabled)
            {
                asset.canOptimize = true;
                asset.optimizationReasons.Add("Mipmaps disabled");
            }
            
            // Check Read/Write
            if (importer.isReadable)
            {
                asset.canOptimize = true;
                asset.optimizationReasons.Add("Read/Write enabled (doubles memory usage)");
            }
            
            // Calculate optimized size
            if (asset.canOptimize)
            {
                var currentFormat = texture.format;
                var pixelCount = texture.width * texture.height;
                var currentBpp = GetBitsPerPixel(currentFormat);
                var optimizedBpp = 4; // Compressed format
                
                if (importer.DoesSourceTextureHaveAlpha())
                {
                    optimizedBpp = 8; // DXT5/ETC2 RGBA
                }
                
                asset.optimizedSize = (long)(pixelCount * optimizedBpp / 8);
                
                // Add mipmap overhead
                if (!importer.mipmapEnabled)
                {
                    asset.optimizedSize = (long)(asset.optimizedSize * 1.33f);
                }
            }
            else
            {
                asset.optimizedSize = asset.currentSize;
            }
        }
        
        private void AnalyzeModel(ScannedAsset asset, string path)
        {
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) return;
            
            asset.canOptimize = false;
            asset.optimizationReasons = new List<string>();
            
            // Check mesh compression
            if (importer.meshCompression == ModelImporterMeshCompression.Off)
            {
                asset.canOptimize = true;
                asset.optimizationReasons.Add("No mesh compression");
            }
            
            // Check mesh optimization using new API
            bool isOptimized = importer.optimizeMeshPolygons || importer.optimizeMeshVertices;
            if (!isOptimized)
            {
                asset.canOptimize = true;
                asset.optimizationReasons.Add("Mesh optimization disabled");
            }
            
            // Check unnecessary imports
            if (importer.importCameras || importer.importLights)
            {
                asset.canOptimize = true;
                asset.optimizationReasons.Add("Importing cameras/lights");
            }
            
            // Check material import
            if (importer.materialImportMode == ModelImporterMaterialImportMode.ImportViaMaterialDescription)
            {
                asset.canOptimize = true;
                asset.optimizationReasons.Add("Importing embedded materials");
            }
            
            // Check animation compression
            if (importer.animationCompression == ModelImporterAnimationCompression.Off)
            {
                asset.canOptimize = true;
                asset.optimizationReasons.Add("No animation compression");
            }
            
            // Estimate optimized size
            if (asset.canOptimize)
            {
                float compressionRatio = 0.7f; // Typical compression ratio
                if (importer.meshCompression == ModelImporterMeshCompression.Off)
                {
                    compressionRatio *= 0.6f; // Additional savings from mesh compression
                }
                
                asset.optimizedSize = (long)(asset.currentSize * compressionRatio);
            }
            else
            {
                asset.optimizedSize = asset.currentSize;
            }
        }
        
        private void AnalyzeAudio(ScannedAsset asset, AudioClip clip)
        {
            if (clip == null) return;
            
            var importer = AssetImporter.GetAtPath(asset.path) as AudioImporter;
            if (importer == null) return;
            
            asset.canOptimize = false;
            asset.optimizationReasons = new List<string>();
            
            var sampleSettings = importer.defaultSampleSettings;
            
            // Check compression
            if (sampleSettings.loadType == AudioClipLoadType.DecompressOnLoad)
            {
                asset.canOptimize = true;
                asset.optimizationReasons.Add("Decompress on load (high memory usage)");
            }
            
            // Check quality
            if (sampleSettings.compressionFormat == AudioCompressionFormat.PCM)
            {
                asset.canOptimize = true;
                asset.optimizationReasons.Add("Uncompressed PCM format");
            }
            
            // Check sample rate
            if (sampleSettings.sampleRateSetting == AudioSampleRateSetting.PreserveSampleRate && clip.frequency > 44100)
            {
                asset.canOptimize = true;
                asset.optimizationReasons.Add($"High sample rate: {clip.frequency}Hz");
            }
            
            // Check stereo for sounds that could be mono
            if (clip.channels == 2 && clip.length < 2f) // Short sounds
            {
                asset.canOptimize = true;
                asset.optimizationReasons.Add("Short stereo sound (could be mono)");
            }
            
            // Calculate optimized size
            if (asset.canOptimize)
            {
                float compressionRatio = 1f;
                
                if (sampleSettings.compressionFormat == AudioCompressionFormat.PCM)
                {
                    compressionRatio *= 0.1f; // Vorbis compression
                }
                
                if (clip.channels == 2 && clip.length < 2f)
                {
                    compressionRatio *= 0.5f; // Mono conversion
                }
                
                if (clip.frequency > 44100)
                {
                    compressionRatio *= (44100f / clip.frequency);
                }
                
                asset.optimizedSize = (long)(asset.currentSize * compressionRatio);
            }
            else
            {
                asset.optimizedSize = asset.currentSize;
            }
        }
        
        private void AnalyzeAnimation(ScannedAsset asset, AnimationClip clip)
        {
            if (clip == null) return;
            
            asset.canOptimize = false;
            asset.optimizationReasons = new List<string>();
            
            // Get animation settings from model importer if it's from a model
            var importer = AssetImporter.GetAtPath(asset.path);
            if (importer is ModelImporter modelImporter)
            {
                if (modelImporter.animationCompression == ModelImporterAnimationCompression.Off)
                {
                    asset.canOptimize = true;
                    asset.optimizationReasons.Add("No animation compression");
                }
                
                // Check for unnecessary precision
                if (modelImporter.animationPositionError < 0.01f || 
                    modelImporter.animationRotationError < 0.01f ||
                    modelImporter.animationScaleError < 0.01f)
                {
                    asset.canOptimize = true;
                    asset.optimizationReasons.Add("Very high precision settings");
                }
            }
            
            // Check keyframe density
            var bindings = AnimationUtility.GetCurveBindings(clip);
            var totalKeyframes = 0;
            foreach (var binding in bindings)
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                totalKeyframes += curve.keys.Length;
            }
            
            var keyframesPerSecond = totalKeyframes / clip.length;
            if (keyframesPerSecond > 60) // More than 60 keys per second is excessive
            {
                asset.canOptimize = true;
                asset.optimizationReasons.Add($"High keyframe density: {keyframesPerSecond:F0} keys/sec");
            }
            
            // Calculate optimized size
            if (asset.canOptimize)
            {
                float compressionRatio = 0.5f; // Typical compression ratio
                
                if (keyframesPerSecond > 60)
                {
                    compressionRatio *= (30f / keyframesPerSecond); // Reduce to 30 keys/sec
                }
                
                asset.optimizedSize = (long)(asset.currentSize * compressionRatio);
            }
            else
            {
                asset.optimizedSize = asset.currentSize;
            }
        }
        
        private void AnalyzeMaterial(ScannedAsset asset, Material material)
        {
            if (material == null) return;
            
            asset.canOptimize = false;
            asset.optimizationReasons = new List<string>();
            
            // Check for unused texture slots
            var shader = material.shader;
            var propertyCount = ShaderUtil.GetPropertyCount(shader);
            var unusedTextures = 0;
            
            for (int i = 0; i < propertyCount; i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    var propName = ShaderUtil.GetPropertyName(shader, i);
                    var texture = material.GetTexture(propName);
                    
                    // Check if it's a default texture that could be removed
                    if (texture != null && IsDefaultTexture(texture))
                    {
                        unusedTextures++;
                    }
                }
            }
            
            if (unusedTextures > 0)
            {
                asset.canOptimize = true;
                asset.optimizationReasons.Add($"{unusedTextures} default textures that could be removed");
            }
            
            // Check for complex shaders on mobile
            if (shader.name.Contains("Standard") && !shader.name.Contains("Mobile"))
            {
                asset.canOptimize = true;
                asset.optimizationReasons.Add("Using non-mobile shader variant");
            }
            
            // Check render queue
            if (material.renderQueue > 3000) // Transparent
            {
                // Check if alpha is actually used
                if (material.HasProperty("_Color"))
                {
                    var color = material.GetColor("_Color");
                    if (color.a >= 0.99f)
                    {
                        asset.canOptimize = true;
                        asset.optimizationReasons.Add("Transparent shader with opaque alpha");
                    }
                }
            }
            
            asset.optimizedSize = asset.currentSize; // Materials are small, size optimization is minimal
        }
        
        private void AnalyzeShader(ScannedAsset asset, Shader shader)
        {
            if (shader == null) return;
            
            asset.canOptimize = false;
            asset.optimizationReasons = new List<string>();
            
            // Check shader complexity
            var variantCount = GetShaderVariantCount(shader);
            if (variantCount > 1000)
            {
                asset.canOptimize = true;
                asset.optimizationReasons.Add($"High variant count: {variantCount}");
            }
            
            // Check for unused features
            if (shader.name.Contains("Standard") && !shader.name.Contains("Mobile"))
            {
                asset.canOptimize = true;
                asset.optimizationReasons.Add("Consider using mobile shader variant");
            }
            
            asset.optimizedSize = asset.currentSize; // Shader size optimization is complex
        }
        
        private int GetBitsPerPixel(TextureFormat format)
        {
            return format switch
            {
                TextureFormat.Alpha8 => 8,
                TextureFormat.RGB24 => 24,
                TextureFormat.RGBA32 => 32,
                TextureFormat.ARGB32 => 32,
                TextureFormat.RGB565 => 16,
                TextureFormat.RGBA4444 => 16,
                TextureFormat.ARGB4444 => 16,
                TextureFormat.DXT1 => 4,
                TextureFormat.DXT5 => 8,
                TextureFormat.BC7 => 8,
                TextureFormat.ETC_RGB4 => 4,
                TextureFormat.ETC2_RGB => 4,
                TextureFormat.ETC2_RGBA8 => 8,
                TextureFormat.PVRTC_RGB2 => 2,
                TextureFormat.PVRTC_RGBA2 => 2,
                TextureFormat.PVRTC_RGB4 => 4,
                TextureFormat.PVRTC_RGBA4 => 4,
                TextureFormat.ASTC_4x4 => 8,
                TextureFormat.ASTC_5x5 => 5,
                TextureFormat.ASTC_6x6 => 4,
                TextureFormat.ASTC_8x8 => 2,
                TextureFormat.ASTC_10x10 => 1,
                TextureFormat.ASTC_12x12 => 1,
                _ => 32
            };
        }
        
        private bool IsDefaultTexture(Texture texture)
        {
            // Check if texture is a default Unity texture
            var path = AssetDatabase.GetAssetPath(texture);
            return string.IsNullOrEmpty(path) || path.StartsWith("Library/") || path.StartsWith("Packages/");
        }
        
        private int GetShaderVariantCount(Shader shader)
        {
            // This is a simplified estimation
            // In a real implementation, you'd use ShaderUtil.GetVariantCount or similar
            var keywords = shader.keywordSpace.keywordNames;
            return keywords.Length > 0 ? (int)Mathf.Pow(2, Mathf.Min(keywords.Length, 10)) : 1;
        }
    }
    
    public class ScannedAsset
    {
        public string path;
        public string name;
        public AssetType type;
        public long currentSize;
        public long optimizedSize;
        public bool canOptimize;
        public bool isSelected;
        public List<string> optimizationReasons;
        
        public float OptimizationRatio => currentSize > 0 ? (float)optimizedSize / currentSize : 1f;
        public long PotentialSavings => currentSize - optimizedSize;
    }
}