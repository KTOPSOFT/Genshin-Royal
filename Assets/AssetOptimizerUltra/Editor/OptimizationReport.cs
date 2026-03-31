using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace AssetOptimizerPro
{
    [Serializable]
    public class OptimizationReport
    {
        public string generatedDate;
        public int totalAssetsScanned;
        public int assetsOptimized;
        public long totalSizeBefore;
        public long totalSizeAfter;
        public long totalSavings;
        public float savingsPercentage;
        public List<AssetOptimizationResult> results;
        public Dictionary<AssetType, CategoryStats> categoryStats;
        public PlatformTarget targetPlatform;
        public string profileUsed;
        
        public OptimizationReport()
        {
            results = new List<AssetOptimizationResult>();
            categoryStats = new Dictionary<AssetType, CategoryStats>();
            generatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
    
    [Serializable]
    public class AssetOptimizationResult
    {
        public string assetPath;
        public string assetName;
        public AssetType assetType;
        public long sizeBefore;
        public long sizeAfter;
        public long savings;
        public float savingsPercentage;
        public List<string> optimizationsApplied;
        public bool success;
        public string errorMessage;
    }
    
    [Serializable]
    public class CategoryStats
    {
        public AssetType type;
        public int count;
        public long totalSizeBefore;
        public long totalSizeAfter;
        public long totalSavings;
        public float averageSavingsPercentage;
    }
    
    public class ReportGenerator
    {
        private const string REPORTS_PATH = "AssetOptimizerPro_Reports";
        private OptimizationReport currentReport;
        
        public bool HasReport()
        {
            return currentReport != null;
        }
        
        public OptimizationReport GetLatestReport()
        {
            return currentReport;
        }
        
        public void GenerateReport(List<ScannedAsset> allAssets, List<ScannedAsset> optimizedAssets)
        {
            currentReport = new OptimizationReport
            {
                totalAssetsScanned = allAssets.Count,
                assetsOptimized = optimizedAssets.Count,
                targetPlatform = PlatformTarget.Mobile, // Should be passed from main window
                profileUsed = "Default Mobile" // Should be passed from main window
            };
            
            // Calculate totals
            foreach (var asset in optimizedAssets)
            {
                var result = new AssetOptimizationResult
                {
                    assetPath = asset.path,
                    assetName = asset.name,
                    assetType = asset.type,
                    sizeBefore = asset.currentSize,
                    sizeAfter = asset.optimizedSize,
                    savings = asset.currentSize - asset.optimizedSize,
                    savingsPercentage = ((float)(asset.currentSize - asset.optimizedSize) / asset.currentSize) * 100,
                    optimizationsApplied = asset.optimizationReasons ?? new List<string>(),
                    success = true
                };
                
                currentReport.results.Add(result);
                currentReport.totalSizeBefore += asset.currentSize;
                currentReport.totalSizeAfter += asset.optimizedSize;
            }
            
            currentReport.totalSavings = currentReport.totalSizeBefore - currentReport.totalSizeAfter;
            currentReport.savingsPercentage = currentReport.totalSizeBefore > 0 
                ? ((float)currentReport.totalSavings / currentReport.totalSizeBefore) * 100 
                : 0;
            
            // Calculate category stats
            var groupedAssets = optimizedAssets.GroupBy(a => a.type);
            foreach (var group in groupedAssets)
            {
                var stats = new CategoryStats
                {
                    type = group.Key,
                    count = group.Count(),
                    totalSizeBefore = group.Sum(a => a.currentSize),
                    totalSizeAfter = group.Sum(a => a.optimizedSize),
                    totalSavings = group.Sum(a => a.currentSize - a.optimizedSize)
                };
                
                stats.averageSavingsPercentage = stats.totalSizeBefore > 0
                    ? ((float)stats.totalSavings / stats.totalSizeBefore) * 100
                    : 0;
                
                currentReport.categoryStats[group.Key] = stats;
            }
        }
        
        public void ExportCSV()
        {
            if (currentReport == null) return;
            
            var fileName = $"OptimizationReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var path = Path.Combine(REPORTS_PATH, fileName);
            
            EnsureReportsDirectory();
            
            var csv = new StringBuilder();
            
            // Header
            csv.AppendLine("Asset Optimizer Pro - Optimization Report");
            csv.AppendLine($"Generated: {currentReport.generatedDate}");
            csv.AppendLine($"Platform: {currentReport.targetPlatform}");
            csv.AppendLine($"Profile: {currentReport.profileUsed}");
            csv.AppendLine();
            
            // Summary
            csv.AppendLine("SUMMARY");
            csv.AppendLine($"Total Assets Scanned,{currentReport.totalAssetsScanned}");
            csv.AppendLine($"Assets Optimized,{currentReport.assetsOptimized}");
            csv.AppendLine($"Total Size Before,{FormatBytes(currentReport.totalSizeBefore)}");
            csv.AppendLine($"Total Size After,{FormatBytes(currentReport.totalSizeAfter)}");
            csv.AppendLine($"Total Savings,{FormatBytes(currentReport.totalSavings)}");
            csv.AppendLine($"Savings Percentage,{currentReport.savingsPercentage:F2}%");
            csv.AppendLine();
            
            // Category breakdown
            csv.AppendLine("CATEGORY BREAKDOWN");
            csv.AppendLine("Type,Count,Size Before,Size After,Savings,Savings %");
            foreach (var stat in currentReport.categoryStats.Values)
            {
                csv.AppendLine($"{stat.type},{stat.count},{FormatBytes(stat.totalSizeBefore)},{FormatBytes(stat.totalSizeAfter)},{FormatBytes(stat.totalSavings)},{stat.averageSavingsPercentage:F2}%");
            }
            csv.AppendLine();
            
            // Detailed results
            csv.AppendLine("DETAILED RESULTS");
            csv.AppendLine("Asset Name,Type,Path,Size Before,Size After,Savings,Savings %,Optimizations Applied");
            foreach (var result in currentReport.results)
            {
                var optimizations = string.Join("; ", result.optimizationsApplied);
                csv.AppendLine($"\"{result.assetName}\",{result.assetType},\"{result.assetPath}\",{FormatBytes(result.sizeBefore)},{FormatBytes(result.sizeAfter)},{FormatBytes(result.savings)},{result.savingsPercentage:F2}%,\"{optimizations}\"");
            }
            
            File.WriteAllText(path, csv.ToString());
            
            EditorUtility.RevealInFinder(path);
            EditorUtility.DisplayDialog("Export Complete", $"Report exported to:\n{path}", "OK");
        }
        
        public void ExportJSON()
        {
            if (currentReport == null) return;
            
            var fileName = $"OptimizationReport_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var path = Path.Combine(REPORTS_PATH, fileName);
            
            EnsureReportsDirectory();
            
            var json = JsonUtility.ToJson(currentReport, true);
            File.WriteAllText(path, json);
            
            EditorUtility.RevealInFinder(path);
            EditorUtility.DisplayDialog("Export Complete", $"Report exported to:\n{path}", "OK");
        }
        
        public void ExportHTML()
        {
            if (currentReport == null) return;
            
            var fileName = $"OptimizationReport_{DateTime.Now:yyyyMMdd_HHmmss}.html";
            var path = Path.Combine(REPORTS_PATH, fileName);
            
            EnsureReportsDirectory();
            
            var html = GenerateHTMLReport();
            File.WriteAllText(path, html);
            
            EditorUtility.RevealInFinder(path);
            Application.OpenURL("file:///" + path);
        }
        
        private string GenerateHTMLReport()
        {
            var html = new StringBuilder();
            
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<title>Asset Optimizer Pro - Optimization Report</title>");
            html.AppendLine("<style>");
            html.AppendLine(@"
                body {
                    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
                    background-color: #1a1a1a;
                    color: #e0e0e0;
                    margin: 0;
                    padding: 20px;
                    line-height: 1.6;
                }
                .container {
                    max-width: 1200px;
                    margin: 0 auto;
                }
                .header {
                    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                    padding: 40px;
                    border-radius: 12px;
                    box-shadow: 0 10px 30px rgba(0,0,0,0.3);
                    margin-bottom: 30px;
                    text-align: center;
                }
                h1 {
                    margin: 0;
                    font-size: 2.5em;
                    color: white;
                    text-shadow: 2px 2px 4px rgba(0,0,0,0.3);
                }
                .subtitle {
                    color: rgba(255,255,255,0.9);
                    font-size: 1.2em;
                    margin-top: 10px;
                }
                .summary-grid {
                    display: grid;
                    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
                    gap: 20px;
                    margin: 30px 0;
                }
                .stat-card {
                    background: #2a2a2a;
                    padding: 25px;
                    border-radius: 10px;
                    box-shadow: 0 5px 15px rgba(0,0,0,0.3);
                    text-align: center;
                    transition: transform 0.3s ease;
                }
                .stat-card:hover {
                    transform: translateY(-5px);
                }
                .stat-value {
                    font-size: 2.5em;
                    font-weight: bold;
                    color: #667eea;
                    display: block;
                    margin-bottom: 10px;
                }
                .stat-label {
                    color: #999;
                    font-size: 0.9em;
                    text-transform: uppercase;
                    letter-spacing: 1px;
                }
                .section {
                    background: #2a2a2a;
                    padding: 30px;
                    border-radius: 10px;
                    margin-bottom: 20px;
                    box-shadow: 0 5px 15px rgba(0,0,0,0.3);
                }
                .section h2 {
                    color: #667eea;
                    margin-top: 0;
                    font-size: 1.8em;
                    border-bottom: 2px solid #444;
                    padding-bottom: 10px;
                }
                table {
                    width: 100%;
                    border-collapse: collapse;
                    margin-top: 20px;
                }
                th, td {
                    padding: 12px;
                    text-align: left;
                    border-bottom: 1px solid #444;
                }
                th {
                    background: #333;
                    color: #667eea;
                    font-weight: bold;
                    text-transform: uppercase;
                    font-size: 0.9em;
                    letter-spacing: 1px;
                }
                tr:hover {
                    background: #333;
                }
                .savings-positive {
                    color: #4ade80;
                    font-weight: bold;
                }
                .chart-container {
                    margin: 20px 0;
                    height: 300px;
                }
                .progress-bar {
                    background: #444;
                    height: 30px;
                    border-radius: 15px;
                    overflow: hidden;
                    margin: 10px 0;
                }
                .progress-fill {
                    background: linear-gradient(90deg, #667eea 0%, #764ba2 100%);
                    height: 100%;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    color: white;
                    font-weight: bold;
                    transition: width 0.5s ease;
                }
                .footer {
                    text-align: center;
                    color: #666;
                    margin-top: 50px;
                    padding: 20px;
                    border-top: 1px solid #444;
                }
                .badge {
                    display: inline-block;
                    padding: 4px 12px;
                    border-radius: 20px;
                    font-size: 0.8em;
                    font-weight: bold;
                    margin: 0 4px;
                }
                .badge-texture { background: #3b82f6; }
                .badge-model { background: #10b981; }
                .badge-audio { background: #f59e0b; }
                .badge-animation { background: #ef4444; }
                .badge-material { background: #8b5cf6; }
                .badge-shader { background: #ec4899; }
            </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("<div class='container'>");
            
            // Header
            html.AppendLine("<div class='header'>");
            html.AppendLine("<h1>Asset Optimizer Pro</h1>");
            html.AppendLine("<div class='subtitle'>Optimization Report</div>");
            html.AppendLine("</div>");
            
            // Summary cards
            html.AppendLine("<div class='summary-grid'>");
            
            html.AppendLine("<div class='stat-card'>");
            html.AppendLine($"<span class='stat-value'>{currentReport.totalAssetsScanned}</span>");
            html.AppendLine("<span class='stat-label'>Assets Scanned</span>");
            html.AppendLine("</div>");
            
            html.AppendLine("<div class='stat-card'>");
            html.AppendLine($"<span class='stat-value'>{currentReport.assetsOptimized}</span>");
            html.AppendLine("<span class='stat-label'>Assets Optimized</span>");
            html.AppendLine("</div>");
            
            html.AppendLine("<div class='stat-card'>");
            html.AppendLine($"<span class='stat-value'>{FormatBytes(currentReport.totalSavings)}</span>");
            html.AppendLine("<span class='stat-label'>Total Savings</span>");
            html.AppendLine("</div>");
            
            html.AppendLine("<div class='stat-card'>");
            html.AppendLine($"<span class='stat-value'>{currentReport.savingsPercentage:F1}%</span>");
            html.AppendLine("<span class='stat-label'>Size Reduction</span>");
            html.AppendLine("</div>");
            
            html.AppendLine("</div>");
            
            // Overall progress bar
            html.AppendLine("<div class='section'>");
            html.AppendLine("<h2>Overall Optimization Impact</h2>");
            html.AppendLine("<div class='progress-bar'>");
            html.AppendLine($"<div class='progress-fill' style='width: {currentReport.savingsPercentage}%'>");
            html.AppendLine($"{currentReport.savingsPercentage:F1}% Size Reduction");
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            html.AppendLine($"<p>Original Size: <strong>{FormatBytes(currentReport.totalSizeBefore)}</strong> → ");
            html.AppendLine($"Optimized Size: <strong>{FormatBytes(currentReport.totalSizeAfter)}</strong></p>");
            html.AppendLine("</div>");
            
            // Category breakdown
            html.AppendLine("<div class='section'>");
            html.AppendLine("<h2>Optimization by Asset Type</h2>");
            html.AppendLine("<table>");
            html.AppendLine("<tr><th>Type</th><th>Count</th><th>Original Size</th><th>Optimized Size</th><th>Savings</th><th>Reduction</th></tr>");
            
            foreach (var stat in currentReport.categoryStats.Values.OrderByDescending(s => s.totalSavings))
            {
                html.AppendLine("<tr>");
                html.AppendLine($"<td><span class='badge badge-{stat.type.ToString().ToLower()}'>{stat.type}</span></td>");
                html.AppendLine($"<td>{stat.count}</td>");
                html.AppendLine($"<td>{FormatBytes(stat.totalSizeBefore)}</td>");
                html.AppendLine($"<td>{FormatBytes(stat.totalSizeAfter)}</td>");
                html.AppendLine($"<td class='savings-positive'>{FormatBytes(stat.totalSavings)}</td>");
                html.AppendLine($"<td class='savings-positive'>{stat.averageSavingsPercentage:F1}%</td>");
                html.AppendLine("</tr>");
            }
            
            html.AppendLine("</table>");
            html.AppendLine("</div>");
            
            // Top optimized assets
            html.AppendLine("<div class='section'>");
            html.AppendLine("<h2>Top 10 Optimized Assets</h2>");
            html.AppendLine("<table>");
            html.AppendLine("<tr><th>Asset Name</th><th>Type</th><th>Original</th><th>Optimized</th><th>Savings</th></tr>");
            
            var topAssets = currentReport.results.OrderByDescending(r => r.savings).Take(10);
            foreach (var result in topAssets)
            {
                html.AppendLine("<tr>");
                html.AppendLine($"<td>{result.assetName}</td>");
                html.AppendLine($"<td><span class='badge badge-{result.assetType.ToString().ToLower()}'>{result.assetType}</span></td>");
                html.AppendLine($"<td>{FormatBytes(result.sizeBefore)}</td>");
                html.AppendLine($"<td>{FormatBytes(result.sizeAfter)}</td>");
                html.AppendLine($"<td class='savings-positive'>{FormatBytes(result.savings)} ({result.savingsPercentage:F1}%)</td>");
                html.AppendLine("</tr>");
            }
            
            html.AppendLine("</table>");
            html.AppendLine("</div>");
            
            // Report details
            html.AppendLine("<div class='section'>");
            html.AppendLine("<h2>Report Details</h2>");
            html.AppendLine($"<p><strong>Generated:</strong> {currentReport.generatedDate}</p>");
            html.AppendLine($"<p><strong>Platform:</strong> {currentReport.targetPlatform}</p>");
            html.AppendLine($"<p><strong>Profile Used:</strong> {currentReport.profileUsed}</p>");
            html.AppendLine("</div>");
            
            // Footer
            html.AppendLine("<div class='footer'>");
            html.AppendLine("<p>Generated by Asset Optimizer Pro v2.0.0 | © 2024 Your Company</p>");
            html.AppendLine("</div>");
            
            html.AppendLine("</div>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            return html.ToString();
        }
        
        private void EnsureReportsDirectory()
        {
            if (!Directory.Exists(REPORTS_PATH))
            {
                Directory.CreateDirectory(REPORTS_PATH);
            }
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
    }
}