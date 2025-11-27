using System;
using System.Collections.Generic;
using System.Linq;
using MCP.Editor.Base;
using MCP.Editor.Interfaces;
using UnityEngine;

namespace MCP.Editor.Handlers
{
    /// <summary>
    /// UI要素の重なり（オーバーラップ）を検出するコマンドハンドラー。
    /// 特定のGameObjectの重なりチェック、またはシーン内すべてのUI要素の重なりチェックをサポート。
    /// </summary>
    public class UguiDetectOverlapsHandler : BaseCommandHandler
    {
        public override string Category => "uguiDetectOverlaps";
        
        public override IEnumerable<string> SupportedOperations => new[]
        {
            "detect" // 重なり検出（メイン操作）
        };
        
        public UguiDetectOverlapsHandler() : base()
        {
        }
        
        public UguiDetectOverlapsHandler(
            IPayloadValidator validator,
            IGameObjectResolver gameObjectResolver,
            IAssetResolver assetResolver,
            ITypeResolver typeResolver)
            : base(validator, gameObjectResolver, assetResolver, typeResolver)
        {
        }
        
        protected override object ExecuteOperation(string operation, Dictionary<string, object> payload)
        {
            var gameObjectPath = GetString(payload, "gameObjectPath");
            var checkAll = GetBool(payload, "checkAll", false);
            var includeChildren = GetBool(payload, "includeChildren", false);
            var threshold = GetFloat(payload, "threshold", 0f);
            
            Debug.Log($"[UguiDetectOverlapsHandler] Detecting overlaps - checkAll={checkAll}, includeChildren={includeChildren}, threshold={threshold}");
            
            var overlaps = new List<Dictionary<string, object>>();
            
            if (checkAll)
            {
                // Check all UI elements for overlaps with each other
                var allRects = UnityEngine.Object.FindObjectsOfType<RectTransform>();
                var rectList = new List<RectTransform>();
                
                foreach (var rect in allRects)
                {
                    // Only include RectTransforms that are under a Canvas
                    if (rect.GetComponentInParent<Canvas>() != null)
                    {
                        rectList.Add(rect);
                    }
                }
                
                Debug.Log($"[UguiDetectOverlapsHandler] Checking {rectList.Count} UI elements");
                
                for (int i = 0; i < rectList.Count; i++)
                {
                    for (int j = i + 1; j < rectList.Count; j++)
                    {
                        var overlap = DetectRectOverlap(rectList[i], rectList[j], threshold);
                        if (overlap != null)
                        {
                            overlaps.Add(overlap);
                        }
                    }
                }
            }
            else
            {
                // Check specific GameObject for overlaps
                if (string.IsNullOrEmpty(gameObjectPath))
                {
                    throw new InvalidOperationException("gameObjectPath is required when checkAll is false");
                }
                
                var targetGo = GameObjectResolver.Resolve(gameObjectPath);
                var targetRects = new List<RectTransform>();
                
                if (includeChildren)
                {
                    targetRects.AddRange(targetGo.GetComponentsInChildren<RectTransform>());
                }
                else
                {
                    var rect = targetGo.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        targetRects.Add(rect);
                    }
                }
                
                // Get all other RectTransforms in the scene
                var allRects = UnityEngine.Object.FindObjectsOfType<RectTransform>();
                var otherRects = new List<RectTransform>();
                
                foreach (var rect in allRects)
                {
                    // Only include RectTransforms that are under a Canvas and not in targetRects
                    if (rect.GetComponentInParent<Canvas>() != null && !targetRects.Contains(rect))
                    {
                        otherRects.Add(rect);
                    }
                }
                
                Debug.Log($"[UguiDetectOverlapsHandler] Checking {targetRects.Count} target elements against {otherRects.Count} other elements");
                
                foreach (var targetRect in targetRects)
                {
                    foreach (var otherRect in otherRects)
                    {
                        var overlap = DetectRectOverlap(targetRect, otherRect, threshold);
                        if (overlap != null)
                        {
                            overlaps.Add(overlap);
                        }
                    }
                }
            }
            
            Debug.Log($"[UguiDetectOverlapsHandler] Found {overlaps.Count} overlaps");
            
            return new Dictionary<string, object>
            {
                ["overlaps"] = overlaps,
                ["count"] = overlaps.Count,
            };
        }
        
        #region Helper Methods
        
        /// <summary>
        /// 2つのRectTransformがワールド空間で重なっているかを検出します。
        /// </summary>
        private Dictionary<string, object> DetectRectOverlap(RectTransform rect1, RectTransform rect2, float threshold)
        {
            // Get world corners for both rectangles
            Vector3[] corners1 = new Vector3[4];
            Vector3[] corners2 = new Vector3[4];
            rect1.GetWorldCorners(corners1);
            rect2.GetWorldCorners(corners2);
            
            // Calculate bounds in 2D (using x and y coordinates)
            float rect1MinX = Mathf.Min(corners1[0].x, corners1[1].x, corners1[2].x, corners1[3].x);
            float rect1MaxX = Mathf.Max(corners1[0].x, corners1[1].x, corners1[2].x, corners1[3].x);
            float rect1MinY = Mathf.Min(corners1[0].y, corners1[1].y, corners1[2].y, corners1[3].y);
            float rect1MaxY = Mathf.Max(corners1[0].y, corners1[1].y, corners1[2].y, corners1[3].y);
            
            float rect2MinX = Mathf.Min(corners2[0].x, corners2[1].x, corners2[2].x, corners2[3].x);
            float rect2MaxX = Mathf.Max(corners2[0].x, corners2[1].x, corners2[2].x, corners2[3].x);
            float rect2MinY = Mathf.Min(corners2[0].y, corners2[1].y, corners2[2].y, corners2[3].y);
            float rect2MaxY = Mathf.Max(corners2[0].y, corners2[1].y, corners2[2].y, corners2[3].y);
            
            // Check for overlap
            bool overlapsX = rect1MinX < rect2MaxX && rect1MaxX > rect2MinX;
            bool overlapsY = rect1MinY < rect2MaxY && rect1MaxY > rect2MinY;
            
            if (overlapsX && overlapsY)
            {
                // Calculate overlap area
                float overlapWidth = Mathf.Min(rect1MaxX, rect2MaxX) - Mathf.Max(rect1MinX, rect2MinX);
                float overlapHeight = Mathf.Min(rect1MaxY, rect2MaxY) - Mathf.Max(rect1MinY, rect2MinY);
                float overlapArea = overlapWidth * overlapHeight;
                
                if (overlapArea >= threshold)
                {
                    return new Dictionary<string, object>
                    {
                        ["element1"] = GetGameObjectPath(rect1.gameObject),
                        ["element2"] = GetGameObjectPath(rect2.gameObject),
                        ["overlapArea"] = overlapArea,
                        ["overlapWidth"] = overlapWidth,
                        ["overlapHeight"] = overlapHeight,
                        ["element1Bounds"] = new Dictionary<string, object>
                        {
                            ["minX"] = rect1MinX,
                            ["maxX"] = rect1MaxX,
                            ["minY"] = rect1MinY,
                            ["maxY"] = rect1MaxY,
                            ["width"] = rect1MaxX - rect1MinX,
                            ["height"] = rect1MaxY - rect1MinY,
                        },
                        ["element2Bounds"] = new Dictionary<string, object>
                        {
                            ["minX"] = rect2MinX,
                            ["maxX"] = rect2MaxX,
                            ["minY"] = rect2MinY,
                            ["maxY"] = rect2MaxY,
                            ["width"] = rect2MaxX - rect2MinX,
                            ["height"] = rect2MaxY - rect2MinY,
                        },
                    };
                }
            }
            
            return null;
        }
        
        private string GetGameObjectPath(GameObject go)
        {
            string path = go.name;
            Transform parent = go.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }
        
        #endregion
        
        protected override bool RequiresCompilationWait(string operation)
        {
            // Overlap detection doesn't require compilation wait
            return false;
        }
    }
}

