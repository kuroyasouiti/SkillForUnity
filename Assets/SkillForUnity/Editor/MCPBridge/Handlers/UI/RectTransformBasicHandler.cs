using System;
using System.Collections.Generic;
using System.Linq;
using MCP.Editor.Interfaces;
using MCP.Editor.Helpers.UI;
using UnityEditor;
using UnityEngine;

namespace MCP.Editor.Handlers.UI
{
    /// <summary>
    /// RectTransformの基本操作を処理します（サイズ調整、検査、更新）。
    /// </summary>
    internal class RectTransformBasicHandler : IRectTransformOperationHandler
    {
        private readonly IGameObjectResolver _gameObjectResolver;
        
        public IEnumerable<string> SupportedOperations => new[]
        {
            "rectAdjust",   // サイズをworld cornersから計算して調整
            "inspect",      // 現在の状態を詳細に検査
            "updateRect"    // プロパティを直接更新
        };
        
        public RectTransformBasicHandler(IGameObjectResolver gameObjectResolver)
        {
            _gameObjectResolver = gameObjectResolver ?? throw new ArgumentNullException(nameof(gameObjectResolver));
        }
        
        public object Execute(string operation, Dictionary<string, object> payload)
        {
            var target = ResolveRectTransform(payload);
            var canvas = RectTransformHelper.GetCanvas(target);
            
            return operation switch
            {
                "rectAdjust" => ExecuteRectAdjust(target, canvas, payload),
                "inspect" => ExecuteInspect(target, canvas),
                "updateRect" => ExecuteUpdateRect(target, payload),
                _ => throw new InvalidOperationException($"Unsupported operation: {operation}")
            };
        }
        
        #region Operation Implementations
        
        /// <summary>
        /// RectTransformのサイズをワールドコーナーから計算して調整します。
        /// </summary>
        private object ExecuteRectAdjust(RectTransform target, Canvas canvas, Dictionary<string, object> payload)
        {
            var beforeState = RectTransformHelper.CaptureState(target);
            
            // Calculate size from world corners
            var worldCorners = new Vector3[4];
            target.GetWorldCorners(worldCorners);
            
            var width = Vector3.Distance(worldCorners[3], worldCorners[0]);
            var height = Vector3.Distance(worldCorners[1], worldCorners[0]);
            var scaleFactor = canvas.scaleFactor == 0f ? 1f : canvas.scaleFactor;
            
            target.sizeDelta = new Vector2(width / scaleFactor, height / scaleFactor);
            EditorUtility.SetDirty(target);
            
            return new Dictionary<string, object>
            {
                ["before"] = beforeState,
                ["after"] = RectTransformHelper.CaptureState(target),
                ["operation"] = "rectAdjust",
                ["scaleFactor"] = scaleFactor
            };
        }
        
        /// <summary>
        /// RectTransformの現在の状態を詳細に検査します。
        /// </summary>
        private object ExecuteInspect(RectTransform target, Canvas canvas)
        {
            var state = RectTransformHelper.CaptureState(target);
            
            // Add canvas info
            state["canvasName"] = canvas.name;
            state["scaleFactor"] = canvas.scaleFactor;
            
            // Add world corners
            var worldCorners = new Vector3[4];
            target.GetWorldCorners(worldCorners);
            state["worldCorners"] = worldCorners.Select(c => new Dictionary<string, object>
            {
                ["x"] = c.x, ["y"] = c.y, ["z"] = c.z
            }).ToList();
            
            // Add rect dimensions
            state["rectWidth"] = target.rect.width;
            state["rectHeight"] = target.rect.height;
            
            return new Dictionary<string, object>
            {
                ["state"] = state,
                ["operation"] = "inspect"
            };
        }
        
        /// <summary>
        /// RectTransformのプロパティを更新します。
        /// </summary>
        private object ExecuteUpdateRect(RectTransform target, Dictionary<string, object> payload)
        {
            var beforeState = RectTransformHelper.CaptureState(target);
            
            // Update properties using helper
            RectTransformHelper.UpdateProperties(target, payload);
            EditorUtility.SetDirty(target);
            
            return new Dictionary<string, object>
            {
                ["before"] = beforeState,
                ["after"] = RectTransformHelper.CaptureState(target),
                ["operation"] = "updateRect"
            };
        }
        
        #endregion
        
        #region Helper Methods
        
        private RectTransform ResolveRectTransform(Dictionary<string, object> payload)
        {
            var path = payload.TryGetValue("gameObjectPath", out var pathObj) && pathObj is string p ? p : null;
            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException("gameObjectPath is required");
            }
            
            var go = _gameObjectResolver.Resolve(path);
            var rectTransform = go.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                throw new InvalidOperationException($"GameObject '{path}' does not contain a RectTransform");
            }
            
            return rectTransform;
        }
        
        #endregion
    }
}

