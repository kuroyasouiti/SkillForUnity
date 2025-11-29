using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SkillForUnity.GameKit
{
    /// <summary>
    /// GameKit UI Command component: manages command buttons that send commands to actors.
    /// </summary>
    [AddComponentMenu("SkillForUnity/GameKit/UI Command")]
    public class GameKitUICommand : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string panelId;
        
        [Header("Target")]
        [SerializeField] private string targetActorId;
        
        [Header("Commands")]
        [SerializeField] private List<CommandButton> commandButtons = new List<CommandButton>();

        public string PanelId => panelId;
        public string TargetActorId => targetActorId;

        public void Initialize(string id, string actorId)
        {
            panelId = id;
            targetActorId = actorId;
        }

        public void RegisterButton(string commandName, Button button)
        {
            commandButtons.Add(new CommandButton
            {
                commandName = commandName,
                button = button
            });

            button.onClick.AddListener(() => SendCommand(commandName));
        }

        private void SendCommand(string commandName)
        {
            if (string.IsNullOrEmpty(targetActorId))
            {
                Debug.LogWarning($"[GameKitUICommand] No target actor set for command: {commandName}");
                return;
            }

            // Find target actor
            var actors = FindObjectsByType<GameKitActor>(FindObjectsSortMode.None);
            GameKitActor targetActor = null;
            foreach (var actor in actors)
            {
                if (actor.ActorId == targetActorId)
                {
                    targetActor = actor;
                    break;
                }
            }

            if (targetActor == null)
            {
                Debug.LogWarning($"[GameKitUICommand] Target actor '{targetActorId}' not found");
                return;
            }

            // Send command via SendMessage
            targetActor.gameObject.SendMessage($"OnCommand_{commandName}", SendMessageOptions.DontRequireReceiver);
            Debug.Log($"[GameKitUICommand] Sent command '{commandName}' to actor '{targetActorId}'");
        }

        public void SetTargetActor(string actorId)
        {
            targetActorId = actorId;
        }

        [Serializable]
        public class CommandButton
        {
            public string commandName;
            public Button button;
        }
    }
}

