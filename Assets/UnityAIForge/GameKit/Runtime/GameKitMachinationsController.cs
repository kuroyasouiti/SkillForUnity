using UnityEngine;

namespace UnityAIForge.GameKit
{
    /// <summary>
    /// Optional controller for automatically executing Machinations diagram logic.
    /// Processes flows, converters, and triggers from GameKitMachinationsAsset.
    /// Attach this to a GameKitManager to enable automatic diagram execution.
    /// </summary>
    [AddComponentMenu("UnityAIForge/GameKit/Machinations Controller")]
    [RequireComponent(typeof(GameKitResourceManager))]
    public class GameKitMachinationsController : MonoBehaviour
    {
        [Header("Execution Settings")]
        [Tooltip("Automatically process flows every frame")]
        [SerializeField] private bool autoProcessFlows = true;
        
        [Tooltip("Automatically process converters every frame")]
        [SerializeField] private bool autoProcessConverters = false;
        
        [Tooltip("Automatically check triggers every frame")]
        [SerializeField] private bool autoCheckTriggers = true;
        
        [Header("Advanced Settings")]
        [Tooltip("Time scale multiplier for flow processing")]
        [SerializeField] private float timeScale = 1f;
        
        [Tooltip("Converter processing interval in seconds (0 = every frame)")]
        [SerializeField] private float converterInterval = 0f;
        
        [Tooltip("Log diagram execution for debugging")]
        [SerializeField] private bool logExecution = false;

        private GameKitResourceManager resourceManager;
        private float converterTimer = 0f;

        private void Awake()
        {
            resourceManager = GetComponent<GameKitResourceManager>();
            if (resourceManager == null)
            {
                Debug.LogError("[GameKitMachinationsController] GameKitResourceManager not found!");
                enabled = false;
            }
        }

        private void Update()
        {
            if (resourceManager == null || resourceManager.MachinationsAsset == null)
                return;

            float deltaTime = Time.deltaTime * timeScale;

            // Process flows
            if (autoProcessFlows)
            {
                resourceManager.ProcessFlows(deltaTime);
                if (logExecution)
                    Debug.Log($"[GameKitMachinationsController] Processed flows (deltaTime: {deltaTime:F3})");
            }

            // Process converters
            if (autoProcessConverters)
            {
                converterTimer += deltaTime;
                if (converterTimer >= converterInterval)
                {
                    resourceManager.ProcessConverters();
                    converterTimer = 0f;
                    if (logExecution)
                        Debug.Log("[GameKitMachinationsController] Processed converters");
                }
            }

            // Check triggers
            if (autoCheckTriggers)
            {
                resourceManager.CheckTriggers();
            }
        }

        /// <summary>
        /// Enable or disable automatic flow processing.
        /// </summary>
        public void SetAutoProcessFlows(bool enabled)
        {
            autoProcessFlows = enabled;
        }

        /// <summary>
        /// Enable or disable automatic converter processing.
        /// </summary>
        public void SetAutoProcessConverters(bool enabled)
        {
            autoProcessConverters = enabled;
        }

        /// <summary>
        /// Enable or disable automatic trigger checking.
        /// </summary>
        public void SetAutoCheckTriggers(bool enabled)
        {
            autoCheckTriggers = enabled;
        }

        /// <summary>
        /// Set time scale for flow processing.
        /// </summary>
        public void SetTimeScale(float scale)
        {
            timeScale = Mathf.Max(0f, scale);
        }

        /// <summary>
        /// Manually process flows once.
        /// </summary>
        public void ProcessFlowsOnce(float deltaTime)
        {
            if (resourceManager != null)
            {
                resourceManager.ProcessFlows(deltaTime);
            }
        }

        /// <summary>
        /// Manually process all converters once.
        /// </summary>
        public void ProcessConvertersOnce()
        {
            if (resourceManager != null)
            {
                resourceManager.ProcessConverters();
            }
        }

        /// <summary>
        /// Manually check all triggers once.
        /// </summary>
        public void CheckTriggersOnce()
        {
            if (resourceManager != null)
            {
                resourceManager.CheckTriggers();
            }
        }
    }
}

