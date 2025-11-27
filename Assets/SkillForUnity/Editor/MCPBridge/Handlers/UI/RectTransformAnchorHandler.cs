using System;
using System.Collections.Generic;
using MCP.Editor.Interfaces;
using MCP.Editor.Helpers.UI;
using UnityEditor;
using UnityEngine;

namespace MCP.Editor.Handlers.UI
{
    /// <summary>
    /// RectTransformのアンカー操作を処理します。
    /// </summary>
    internal class RectTransformAnchorHandler : IRectTransformOperationHandler
    {
        private readonly IGameObjectResolver _gameObjectResolver;
        
        public IEnumerable<string> SupportedOperations => new[]
        {
            "setAnchor",         // カスタムアンカー値を設定
            "setAnchorPreset",   // プリセット（top-left, center等）を適用
            "convertToAnchored", // 絶対位置 → アンカー位置
            "convertToAbsolute"  // アンカー位置 → 絶対位置（読み取り専用）
        };
        
        public RectTransformAnchorHandler(IGameObjectResolver gameObjectResolver)
        {
            _gameObjectResolver = gameObjectResolver ?? throw new ArgumentNullException(nameof(gameObjectResolver));
        }
        
        public object Execute(string operation, Dictionary<string, object> payload)
        {
            var target = ResolveRectTransform(payload);
            var beforeState = RectTransformHelper.CaptureState(target);
            
            switch (operation)
            {
                case "setAnchor":
                    RectTransformHelper.SetAnchor(target, payload);
                    break;
                    
                case "setAnchorPreset":
                    RectTransformHelper.SetAnchorPreset(target, payload);
                    break;
                    
                case "convertToAnchored":
                    RectTransformHelper.ConvertToAnchored(target, payload);
                    break;
                    
                case "convertToAbsolute":
                    // Read-only operation, state is captured but no changes made
                    break;
                    
                default:
                    throw new InvalidOperationException($"Unsupported operation: {operation}");
            }
            
            EditorUtility.SetDirty(target);
            
            return new Dictionary<string, object>
            {
                ["before"] = beforeState,
                ["after"] = RectTransformHelper.CaptureState(target),
                ["operation"] = operation
            };
        }
        
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

