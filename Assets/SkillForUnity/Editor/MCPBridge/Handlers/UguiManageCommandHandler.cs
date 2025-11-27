using System;
using System.Collections.Generic;
using System.Linq;
using MCP.Editor.Base;
using MCP.Editor.Interfaces;
using MCP.Editor.Handlers.UI;
using UnityEngine;

namespace MCP.Editor.Handlers
{
    /// <summary>
    /// UGUIManage操作のFacade。
    /// 内部的に機能別のサブハンドラー（Basic/Anchor）に委譲します。
    /// </summary>
    public class UguiManageCommandHandler : BaseCommandHandler
    {
        private readonly IRectTransformOperationHandler _basicHandler;
        private readonly IRectTransformOperationHandler _anchorHandler;
        
        public override string Category => "uguiManage";
        
        public override IEnumerable<string> SupportedOperations => 
            _basicHandler.SupportedOperations.Concat(_anchorHandler.SupportedOperations);
        
        public UguiManageCommandHandler() : base()
        {
            _basicHandler = new RectTransformBasicHandler(GameObjectResolver);
            _anchorHandler = new RectTransformAnchorHandler(GameObjectResolver);
        }
        
        public UguiManageCommandHandler(
            IPayloadValidator validator,
            IGameObjectResolver gameObjectResolver,
            IAssetResolver assetResolver,
            ITypeResolver typeResolver)
            : base(validator, gameObjectResolver, assetResolver, typeResolver)
        {
            _basicHandler = new RectTransformBasicHandler(gameObjectResolver);
            _anchorHandler = new RectTransformAnchorHandler(gameObjectResolver);
        }
        
        protected override object ExecuteOperation(string operation, Dictionary<string, object> payload)
        {
            // Log operation
            Debug.Log($"[UguiManageCommandHandler] Processing operation: {operation}");
            
            // Route to appropriate sub-handler based on operation
            if (_basicHandler.SupportedOperations.Contains(operation))
            {
                return _basicHandler.Execute(operation, payload);
            }
            
            if (_anchorHandler.SupportedOperations.Contains(operation))
            {
                return _anchorHandler.Execute(operation, payload);
            }
            
            throw new InvalidOperationException($"Unknown operation: {operation}");
        }
        
        protected override bool RequiresCompilationWait(string operation)
        {
            // Inspect operation doesn't require compilation wait
            return operation != "inspect";
        }
    }
}

