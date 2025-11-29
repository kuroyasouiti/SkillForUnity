from __future__ import annotations

from typing import Any

import mcp.types as types
from mcp.server import Server

from bridge.bridge_manager import bridge_manager
from utils.json_utils import as_pretty_json


def _ensure_bridge_connected() -> None:
    if not bridge_manager.is_connected():
        raise RuntimeError(
            "Unity bridge is not connected. In the Unity Editor choose Tools/MCP Assistant to start the bridge."
        )


async def _call_bridge_tool(tool_name: str, payload: dict[str, Any]) -> list[types.Content]:
    _ensure_bridge_connected()

    timeout_ms = 45_000
    if "timeoutSeconds" in payload:
        unity_timeout = payload["timeoutSeconds"]
        timeout_ms = (unity_timeout + 20) * 1000

    try:
        response = await bridge_manager.send_command(tool_name, payload, timeout_ms=timeout_ms)
    except Exception as exc:  # pragma: no cover - surface bridge errors to client
        raise RuntimeError(f'Unity bridge tool "{tool_name}" failed: {exc}') from exc

    text = response if isinstance(response, str) else as_pretty_json(response)
    return [types.TextContent(type="text", text=text)]


def _schema_with_required(schema: dict[str, Any], required: list[str]) -> dict[str, Any]:
    enriched = dict(schema)
    enriched["required"] = required
    enriched["additionalProperties"] = False
    return enriched


def register_tools(server: Server) -> None:
    ping_schema: dict[str, Any] = {
        "type": "object",
        "properties": {},
        "additionalProperties": False,
    }

    scene_manage_schema = _schema_with_required(
        {
            "type": "object",
            "properties": {
                "operation": {
                    "type": "string",
                    "enum": [
                        "create",
                        "load",
                        "save",
                        "delete",
                        "duplicate",
                        "inspect",
                        "listBuildSettings",
                        "addToBuildSettings",
                        "removeFromBuildSettings",
                        "reorderBuildSettings",
                        "setBuildSettingsEnabled",
                    ],
                },
                "scenePath": {"type": "string"},
                "newSceneName": {"type": "string"},
                "additive": {"type": "boolean"},
                "includeOpenScenes": {"type": "boolean"},
                "includeHierarchy": {"type": "boolean"},
                "includeComponents": {"type": "boolean"},
                "filter": {"type": "string"},
                "enabled": {"type": "boolean"},
                "index": {"type": "integer"},
                "fromIndex": {"type": "integer"},
                "toIndex": {"type": "integer"},
            },
        },
        ["operation"],
    )

    game_object_manage_schema = _schema_with_required(
        {
            "type": "object",
            "properties": {
                "operation": {
                    "type": "string",
                    "enum": [
                        "create",
                        "delete",
                        "move",
                        "rename",
                        "update",
                        "duplicate",
                        "inspect",
                        "findMultiple",
                        "deleteMultiple",
                        "inspectMultiple",
                    ],
                },
                "gameObjectPath": {"type": "string"},
                "parentPath": {"type": "string"},
                "template": {"type": "string"},
                "name": {"type": "string"},
                "tag": {"type": "string"},
                "layer": {"oneOf": [{"type": "integer"}, {"type": "string"}]},
                "active": {"type": "boolean"},
                "static": {"type": "boolean"},
                "pattern": {"type": "string"},
                "useRegex": {"type": "boolean"},
                "includeComponents": {"type": "boolean"},
                "maxResults": {"type": "integer"},
            },
        },
        ["operation"],
    )

    component_manage_schema = _schema_with_required(
        {
            "type": "object",
            "properties": {
                "operation": {
                    "type": "string",
                    "enum": [
                        "add",
                        "remove",
                        "update",
                        "inspect",
                        "addMultiple",
                        "removeMultiple",
                        "updateMultiple",
                        "inspectMultiple",
                    ],
                },
                "gameObjectPath": {"type": "string"},
                "gameObjectGlobalObjectId": {"type": "string"},
                "componentType": {"type": "string"},
                "propertyChanges": {"type": "object", "additionalProperties": True},
                "applyDefaults": {"type": "boolean"},
                "pattern": {"type": "string"},
                "useRegex": {"type": "boolean"},
                "includeProperties": {"type": "boolean"},
                "propertyFilter": {"type": "array", "items": {"type": "string"}},
                "maxResults": {"type": "integer"},
                "stopOnError": {"type": "boolean"},
            },
        },
        ["operation", "componentType"],
    )

    asset_manage_schema = _schema_with_required(
        {
            "type": "object",
            "properties": {
                "operation": {
                    "type": "string",
                    "enum": [
                        "create",
                        "update",
                        "updateImporter",
                        "delete",
                        "rename",
                        "duplicate",
                        "inspect",
                        "findMultiple",
                        "deleteMultiple",
                        "inspectMultiple",
                    ],
                },
                "assetPath": {"type": "string"},
                "assetGuid": {"type": "string"},
                "content": {"type": "string"},
                "destinationPath": {"type": "string"},
                "propertyChanges": {"type": "object", "additionalProperties": True},
                "pattern": {"type": "string"},
                "useRegex": {"type": "boolean"},
                "includeProperties": {"type": "boolean"},
                "maxResults": {"type": "integer"},
            },
        },
        ["operation"],
    )

    project_settings_manage_schema = _schema_with_required(
        {
            "type": "object",
            "properties": {
                "operation": {
                    "type": "string",
                    "enum": ["read", "write", "list"],
                },
                "category": {
                    "type": "string",
                    "enum": ["player", "quality", "time", "physics", "audio", "editor", "tagsLayers"],
                },
                "property": {"type": "string"},
                "value": {},
            },
        },
        ["operation"],
    )

    scriptable_object_manage_schema = _schema_with_required(
        {
            "type": "object",
            "properties": {
                "operation": {
                    "type": "string",
                    "enum": [
                        "create",
                        "inspect",
                        "update",
                        "delete",
                        "duplicate",
                        "list",
                        "findByType",
                    ],
                },
                "typeName": {"type": "string"},
                "assetPath": {"type": "string"},
                "assetGuid": {"type": "string"},
                "properties": {"type": "object", "additionalProperties": True},
                "includeProperties": {"type": "boolean"},
                "propertyFilter": {"type": "array", "items": {"type": "string"}},
                "searchPath": {"type": "string"},
                "maxResults": {"type": "integer"},
                "offset": {"type": "integer"},
                "sourceAssetPath": {"type": "string"},
                "sourceAssetGuid": {"type": "string"},
                "destinationAssetPath": {"type": "string"},
            },
        },
        ["operation"],
    )

    prefab_manage_schema = _schema_with_required(
        {
            "type": "object",
            "properties": {
                "operation": {
                    "type": "string",
                    "enum": [
                        "create",
                        "update",
                        "inspect",
                        "instantiate",
                        "unpack",
                        "applyOverrides",
                        "revertOverrides",
                    ],
                    "description": "Prefab operation to perform.",
                },
                "gameObjectPath": {
                    "type": "string",
                    "description": "Hierarchy path of GameObject (for create/update/unpack operations).",
                },
                "prefabPath": {
                    "type": "string",
                    "description": "Asset path to prefab file (e.g., 'Assets/Prefabs/MyPrefab.prefab').",
                },
                "parentPath": {
                    "type": "string",
                    "description": "Parent GameObject path for instantiation.",
                },
                "position": {
                    "type": "object",
                    "properties": {
                        "x": {"type": "number"},
                        "y": {"type": "number"},
                        "z": {"type": "number"},
                    },
                    "description": "Position for instantiated prefab.",
                },
                "rotation": {
                    "type": "object",
                    "properties": {
                        "x": {"type": "number"},
                        "y": {"type": "number"},
                        "z": {"type": "number"},
                    },
                    "description": "Euler rotation for instantiated prefab.",
                },
                "unpackMode": {
                    "type": "string",
                    "enum": ["completely", "outermost"],
                    "description": "Unpack mode: 'completely' or 'outermost'.",
                },
                "includeOverrides": {
                    "type": "boolean",
                    "description": "Include override information in inspect operation.",
                },
            },
        },
        ["operation"],
    )

    vector_sprite_convert_schema = _schema_with_required(
        {
            "type": "object",
            "properties": {
                "operation": {
                    "type": "string",
                    "enum": [
                        "primitiveToSprite",
                        "svgToSprite",
                        "textureToSprite",
                        "createColorSprite",
                    ],
                    "description": "Vector/sprite conversion operation.",
                },
                "primitiveType": {
                    "type": "string",
                    "enum": ["square", "circle", "triangle", "polygon"],
                    "description": "Primitive shape type for sprite generation.",
                },
                "width": {
                    "type": "integer",
                    "description": "Width of generated sprite in pixels.",
                },
                "height": {
                    "type": "integer",
                    "description": "Height of generated sprite in pixels.",
                },
                "color": {
                    "type": "object",
                    "properties": {
                        "r": {"type": "number", "minimum": 0, "maximum": 1},
                        "g": {"type": "number", "minimum": 0, "maximum": 1},
                        "b": {"type": "number", "minimum": 0, "maximum": 1},
                        "a": {"type": "number", "minimum": 0, "maximum": 1},
                    },
                    "description": "RGBA color (0-1 range).",
                },
                "sides": {
                    "type": "integer",
                    "description": "Number of sides for polygon primitive.",
                },
                "svgPath": {
                    "type": "string",
                    "description": "Path to SVG file for conversion.",
                },
                "texturePath": {
                    "type": "string",
                    "description": "Path to texture file for sprite conversion.",
                },
                "outputPath": {
                    "type": "string",
                    "description": "Output path for generated sprite asset.",
                },
                "pixelsPerUnit": {
                    "type": "number",
                    "description": "Pixels per unit for sprite import settings.",
                },
                "spriteMode": {
                    "type": "string",
                    "enum": ["single", "multiple"],
                    "description": "Sprite mode: 'single' or 'multiple'.",
                },
            },
        },
        ["operation"],
    )

    transform_batch_schema = _schema_with_required(
        {
            "type": "object",
            "properties": {
                "operation": {
                    "type": "string",
                    "enum": [
                        "arrangeCircle",
                        "arrangeLine",
                        "renameSequential",
                        "renameFromList",
                        "createMenuList",
                    ],
                },
                "gameObjectPaths": {
                    "type": "array",
                    "items": {"type": "string"},
                    "description": "Target GameObject hierarchy paths.",
                },
                "center": {
                    "type": "object",
                    "properties": {
                        "x": {"type": "number"},
                        "y": {"type": "number"},
                        "z": {"type": "number"},
                    },
                },
                "radius": {"type": "number"},
                "startAngle": {"type": "number"},
                "clockwise": {"type": "boolean"},
                "plane": {"type": "string", "enum": ["XY", "XZ", "YZ"]},
                "localSpace": {"type": "boolean"},
                "startPosition": {
                    "type": "object",
                    "properties": {
                        "x": {"type": "number"},
                        "y": {"type": "number"},
                        "z": {"type": "number"},
                    },
                },
                "endPosition": {
                    "type": "object",
                    "properties": {
                        "x": {"type": "number"},
                        "y": {"type": "number"},
                        "z": {"type": "number"},
                    },
                },
                "spacing": {"type": "number"},
                "baseName": {"type": "string"},
                "startIndex": {"type": "integer"},
                "padding": {"type": "integer"},
                "names": {"type": "array", "items": {"type": "string"}},
                "parentPath": {"type": "string"},
                "prefabPath": {"type": "string"},
                "axis": {"type": "string", "enum": ["horizontal", "vertical"]},
                "offset": {"type": "number"},
            },
        },
        ["operation"],
    )

    rect_transform_batch_schema = _schema_with_required(
        {
            "type": "object",
            "properties": {
                "operation": {
                    "type": "string",
                    "enum": [
                        "setAnchors",
                        "setPivot",
                        "setSizeDelta",
                        "setAnchoredPosition",
                        "alignToParent",
                        "distributeHorizontal",
                        "distributeVertical",
                        "matchSize",
                    ],
                },
                "gameObjectPaths": {
                    "type": "array",
                    "items": {"type": "string"},
                    "description": "Target GameObject hierarchy paths (must have RectTransform).",
                },
                "anchorMin": {
                    "type": "object",
                    "properties": {
                        "x": {"type": "number"},
                        "y": {"type": "number"},
                    },
                },
                "anchorMax": {
                    "type": "object",
                    "properties": {
                        "x": {"type": "number"},
                        "y": {"type": "number"},
                    },
                },
                "pivot": {
                    "type": "object",
                    "properties": {
                        "x": {"type": "number"},
                        "y": {"type": "number"},
                    },
                },
                "sizeDelta": {
                    "type": "object",
                    "properties": {
                        "x": {"type": "number"},
                        "y": {"type": "number"},
                    },
                },
                "anchoredPosition": {
                    "type": "object",
                    "properties": {
                        "x": {"type": "number"},
                        "y": {"type": "number"},
                    },
                },
                "preset": {
                    "type": "string",
                    "enum": [
                        "topLeft",
                        "topCenter",
                        "topRight",
                        "middleLeft",
                        "middleCenter",
                        "middleRight",
                        "bottomLeft",
                        "bottomCenter",
                        "bottomRight",
                        "stretchLeft",
                        "stretchCenter",
                        "stretchRight",
                        "stretchTop",
                        "stretchMiddle",
                        "stretchBottom",
                        "stretchAll",
                    ],
                },
                "spacing": {"type": "number"},
                "matchWidth": {"type": "boolean"},
                "matchHeight": {"type": "boolean"},
                "sourceGameObjectPath": {"type": "string"},
            },
        },
        ["operation"],
    )

    physics_bundle_schema = _schema_with_required(
        {
            "type": "object",
            "properties": {
                "operation": {
                    "type": "string",
                    "enum": [
                        "applyPreset2D",
                        "applyPreset3D",
                        "updateRigidbody2D",
                        "updateRigidbody3D",
                        "updateCollider2D",
                        "updateCollider3D",
                        "inspect",
                    ],
                },
                "gameObjectPaths": {
                    "type": "array",
                    "items": {"type": "string"},
                    "description": "Target GameObject hierarchy paths.",
                },
                "preset": {
                    "type": "string",
                    "enum": [
                        "dynamic",
                        "kinematic",
                        "static",
                        "character",
                        "platformer",
                        "topDown",
                        "vehicle",
                        "projectile",
                    ],
                    "description": "Physics preset template.",
                },
                "colliderType": {
                    "type": "string",
                    "enum": ["box", "sphere", "capsule", "mesh", "circle", "polygon", "edge"],
                    "description": "Collider type to add (2D: box/circle/polygon/edge, 3D: box/sphere/capsule/mesh).",
                },
                "isTrigger": {"type": "boolean"},
                "rigidbodyType": {
                    "type": "string",
                    "enum": ["dynamic", "kinematic", "static"],
                },
                "mass": {"type": "number"},
                "drag": {"type": "number"},
                "angularDrag": {"type": "number"},
                "gravityScale": {"type": "number"},
                "useGravity": {"type": "boolean"},
                "isKinematic": {"type": "boolean"},
                "interpolate": {
                    "type": "string",
                    "enum": ["none", "interpolate", "extrapolate"],
                },
                "collisionDetection": {
                    "type": "string",
                    "enum": ["discrete", "continuous", "continuousDynamic", "continuousSpeculative"],
                },
                "constraints": {
                    "type": "object",
                    "properties": {
                        "freezePositionX": {"type": "boolean"},
                        "freezePositionY": {"type": "boolean"},
                        "freezePositionZ": {"type": "boolean"},
                        "freezeRotationX": {"type": "boolean"},
                        "freezeRotationY": {"type": "boolean"},
                        "freezeRotationZ": {"type": "boolean"},
                    },
                },
                "size": {
                    "type": "object",
                    "properties": {
                        "x": {"type": "number"},
                        "y": {"type": "number"},
                        "z": {"type": "number"},
                    },
                    "description": "Collider size (Vector2 for 2D, Vector3 for 3D).",
                },
                "center": {
                    "type": "object",
                    "properties": {
                        "x": {"type": "number"},
                        "y": {"type": "number"},
                        "z": {"type": "number"},
                    },
                    "description": "Collider center offset.",
                },
                "radius": {"type": "number", "description": "Radius for sphere/circle/capsule colliders."},
                "height": {"type": "number", "description": "Height for capsule colliders."},
                "material": {"type": "string", "description": "Physics material asset path."},
            },
        },
        ["operation"],
    )

    camera_rig_schema = _schema_with_required(
        {
            "type": "object",
            "properties": {
                "operation": {
                    "type": "string",
                    "enum": ["createRig", "updateRig", "inspect"],
                },
                "rigType": {
                    "type": "string",
                    "enum": ["follow", "orbit", "splitScreen", "fixed", "dolly"],
                    "description": "Camera rig preset type.",
                },
                "parentPath": {"type": "string", "description": "Parent GameObject path for the rig."},
                "rigName": {"type": "string", "description": "Name for the camera rig."},
                "targetPath": {"type": "string", "description": "Target GameObject to follow/orbit."},
                "offset": {
                    "type": "object",
                    "properties": {
                        "x": {"type": "number"},
                        "y": {"type": "number"},
                        "z": {"type": "number"},
                    },
                    "description": "Camera offset from target.",
                },
                "distance": {"type": "number", "description": "Distance from target (for orbit)."},
                "followSpeed": {"type": "number", "description": "Follow smoothing speed."},
                "lookAtTarget": {"type": "boolean", "description": "Whether camera should look at target."},
                "fieldOfView": {"type": "number", "description": "Camera field of view."},
                "orthographic": {"type": "boolean", "description": "Use orthographic projection."},
                "orthographicSize": {"type": "number", "description": "Orthographic camera size."},
                "splitScreenIndex": {"type": "integer", "description": "Split screen viewport index (0-3)."},
            },
        },
        ["operation"],
    )

    ui_foundation_schema = _schema_with_required(
        {
            "type": "object",
            "properties": {
                "operation": {
                    "type": "string",
                    "enum": ["createCanvas", "createPanel", "createButton", "createText", "createImage", "createInputField", "inspect"],
                },
                "parentPath": {"type": "string", "description": "Parent GameObject path."},
                "name": {"type": "string", "description": "UI element name."},
                "renderMode": {
                    "type": "string",
                    "enum": ["screenSpaceOverlay", "screenSpaceCamera", "worldSpace"],
                    "description": "Canvas render mode.",
                },
                "sortingOrder": {"type": "integer", "description": "Canvas sorting order."},
                "text": {"type": "string", "description": "Text content."},
                "fontSize": {"type": "integer", "description": "Font size."},
                "color": {
                    "type": "object",
                    "properties": {
                        "r": {"type": "number"},
                        "g": {"type": "number"},
                        "b": {"type": "number"},
                        "a": {"type": "number"},
                    },
                    "description": "Color (RGBA 0-1).",
                },
                "anchorPreset": {
                    "type": "string",
                    "enum": [
                        "topLeft", "topCenter", "topRight",
                        "middleLeft", "middleCenter", "middleRight",
                        "bottomLeft", "bottomCenter", "bottomRight",
                        "stretchAll",
                    ],
                    "description": "RectTransform anchor preset.",
                },
                "width": {"type": "number", "description": "Width of UI element."},
                "height": {"type": "number", "description": "Height of UI element."},
                "spritePath": {"type": "string", "description": "Sprite asset path for Image/Button."},
                "placeholder": {"type": "string", "description": "Placeholder text for InputField."},
            },
        },
        ["operation"],
    )

    audio_source_bundle_schema = _schema_with_required(
        {
            "type": "object",
            "properties": {
                "operation": {
                    "type": "string",
                    "enum": ["createAudioSource", "updateAudioSource", "inspect"],
                },
                "gameObjectPath": {"type": "string", "description": "Target GameObject path."},
                "preset": {
                    "type": "string",
                    "enum": ["music", "sfx", "ambient", "voice", "ui", "custom"],
                    "description": "Audio source preset type.",
                },
                "audioClipPath": {"type": "string", "description": "AudioClip asset path."},
                "volume": {"type": "number", "description": "Volume (0-1)."},
                "pitch": {"type": "number", "description": "Pitch (-3 to 3)."},
                "loop": {"type": "boolean", "description": "Loop playback."},
                "playOnAwake": {"type": "boolean", "description": "Play on awake."},
                "spatialBlend": {"type": "number", "description": "2D/3D blend (0=2D, 1=3D)."},
                "minDistance": {"type": "number", "description": "Min distance for 3D sound."},
                "maxDistance": {"type": "number", "description": "Max distance for 3D sound."},
                "priority": {"type": "integer", "description": "Priority (0-256, 0=highest)."},
                "mixerGroupPath": {"type": "string", "description": "Audio mixer group asset path."},
            },
        },
        ["operation"],
    )

    input_profile_schema = _schema_with_required(
        {
            "type": "object",
            "properties": {
                "operation": {
                    "type": "string",
                    "enum": ["createPlayerInput", "createInputActions", "inspect"],
                },
                "gameObjectPath": {"type": "string", "description": "Target GameObject path."},
                "preset": {
                    "type": "string",
                    "enum": ["player", "ui", "vehicle", "custom"],
                    "description": "Input profile preset type.",
                },
                "inputActionsAssetPath": {"type": "string", "description": "InputActions asset path."},
                "defaultActionMap": {"type": "string", "description": "Default action map name."},
                "notificationBehavior": {
                    "type": "string",
                    "enum": ["sendMessages", "broadcastMessages", "invokeUnityEvents", "invokeCSharpEvents"],
                    "description": "Input notification behavior.",
                },
                "actions": {
                    "type": "array",
                    "items": {
                        "type": "object",
                        "properties": {
                            "name": {"type": "string"},
                            "type": {"type": "string", "enum": ["button", "value", "passThrough"]},
                            "binding": {"type": "string"},
                        },
                    },
                    "description": "Custom action definitions.",
                },
            },
        },
        ["operation"],
    )

    gamekit_actor_schema = _schema_with_required(
        {
            "type": "object",
            "properties": {
                "operation": {
                    "type": "string",
                    "enum": ["create", "update", "inspect", "delete"],
                },
                "actorId": {"type": "string", "description": "Unique actor identifier."},
                "parentPath": {"type": "string", "description": "Parent GameObject path."},
                "behaviorProfile": {
                    "type": "string",
                    "enum": ["2dLinear", "2dPhysics", "2dTileGrid", "3dCharacterController", "3dPhysics", "3dNavMesh"],
                    "description": "Actor movement/behavior profile.",
                },
                "controlMode": {
                    "type": "string",
                    "enum": ["directController", "aiAutonomous", "uiCommand", "scriptTriggerOnly"],
                    "description": "How the actor receives input/commands.",
                },
                "position": {
                    "type": "object",
                    "properties": {"x": {"type": "number"}, "y": {"type": "number"}, "z": {"type": "number"}},
                },
                "stats": {
                    "type": "object",
                    "additionalProperties": {"type": "number"},
                    "description": "Actor stats (e.g., health, speed, attack).",
                },
                "abilities": {
                    "type": "array",
                    "items": {"type": "string"},
                    "description": "List of ability names.",
                },
                "weaponLoadout": {
                    "type": "array",
                    "items": {"type": "string"},
                    "description": "List of weapon/equipment names.",
                },
                "spritePath": {"type": "string", "description": "Sprite asset path for 2D actors."},
                "modelPath": {"type": "string", "description": "Model prefab path for 3D actors."},
            },
        },
        ["operation"],
    )

    gamekit_manager_schema = _schema_with_required(
        {
            "type": "object",
            "properties": {
                "operation": {
                    "type": "string",
                    "enum": ["create", "update", "inspect", "delete"],
                },
                "managerId": {"type": "string", "description": "Unique manager identifier."},
                "managerType": {
                    "type": "string",
                    "enum": ["turnBased", "realtime", "resourcePool", "eventHub", "stateManager"],
                    "description": "Manager type.",
                },
                "parentPath": {"type": "string", "description": "Parent GameObject path."},
                "persistent": {"type": "boolean", "description": "DontDestroyOnLoad flag."},
                "turnPhases": {
                    "type": "array",
                    "items": {"type": "string"},
                    "description": "Turn phase names for turn-based managers.",
                },
                "resourceTypes": {
                    "type": "array",
                    "items": {"type": "string"},
                    "description": "Resource type names for resource pool managers.",
                },
                "initialResources": {
                    "type": "object",
                    "additionalProperties": {"type": "number"},
                    "description": "Initial resource amounts.",
                },
            },
        },
        ["operation"],
    )

    gamekit_interaction_schema = _schema_with_required(
        {
            "type": "object",
            "properties": {
                "operation": {
                    "type": "string",
                    "enum": ["create", "update", "inspect", "delete"],
                },
                "interactionId": {"type": "string", "description": "Unique interaction identifier."},
                "parentPath": {"type": "string", "description": "Parent GameObject path."},
                "triggerType": {
                    "type": "string",
                    "enum": ["collision", "trigger", "raycast", "proximity", "input"],
                    "description": "Interaction trigger type.",
                },
                "triggerShape": {
                    "type": "string",
                    "enum": ["box", "sphere", "capsule"],
                    "description": "Collider shape for collision/trigger types.",
                },
                "triggerSize": {
                    "type": "object",
                    "properties": {"x": {"type": "number"}, "y": {"type": "number"}, "z": {"type": "number"}},
                },
                "actions": {
                    "type": "array",
                    "items": {
                        "type": "object",
                        "properties": {
                            "type": {"type": "string", "enum": ["spawnPrefab", "destroyObject", "playSound", "sendMessage", "changeScene"]},
                            "target": {"type": "string"},
                            "parameter": {"type": "string"},
                        },
                    },
                    "description": "Actions to execute on trigger.",
                },
                "conditions": {
                    "type": "array",
                    "items": {
                        "type": "object",
                        "properties": {
                            "type": {"type": "string", "enum": ["tag", "layer", "distance", "custom"]},
                            "value": {"type": "string"},
                        },
                    },
                    "description": "Conditions to check before executing actions.",
                },
            },
        },
        ["operation"],
    )

    gamekit_ui_command_schema = _schema_with_required(
        {
            "type": "object",
            "properties": {
                "operation": {
                    "type": "string",
                    "enum": ["createCommandPanel", "addCommand", "inspect", "delete"],
                },
                "panelId": {"type": "string", "description": "Unique command panel identifier."},
                "canvasPath": {"type": "string", "description": "Canvas GameObject path."},
                "targetActorId": {"type": "string", "description": "Target actor ID to send commands to."},
                "commands": {
                    "type": "array",
                    "items": {
                        "type": "object",
                        "properties": {
                            "name": {"type": "string"},
                            "label": {"type": "string"},
                            "icon": {"type": "string"},
                        },
                    },
                    "description": "List of commands to create as buttons.",
                },
                "layout": {
                    "type": "string",
                    "enum": ["horizontal", "vertical", "grid"],
                    "description": "Button layout style.",
                },
                "buttonSize": {
                    "type": "object",
                    "properties": {"width": {"type": "number"}, "height": {"type": "number"}},
                },
            },
        },
        ["operation"],
    )

    gamekit_sceneflow_schema = _schema_with_required(
        {
            "type": "object",
            "properties": {
                "operation": {
                    "type": "string",
                    "enum": ["create", "update", "inspect", "delete", "transition"],
                },
                "flowId": {"type": "string", "description": "Unique scene flow identifier."},
                "scenes": {
                    "type": "array",
                    "items": {
                        "type": "object",
                        "properties": {
                            "name": {"type": "string"},
                            "scenePath": {"type": "string"},
                            "loadMode": {"type": "string", "enum": ["single", "additive"]},
                            "sharedGroups": {"type": "array", "items": {"type": "string"}},
                        },
                    },
                    "description": "Scene definitions in the flow.",
                },
                "transitions": {
                    "type": "array",
                    "items": {
                        "type": "object",
                        "properties": {
                            "trigger": {"type": "string"},
                            "fromScene": {"type": "string"},
                            "toScene": {"type": "string"},
                        },
                    },
                    "description": "Scene transition definitions.",
                },
                "sharedSceneGroups": {
                    "type": "object",
                    "additionalProperties": {
                        "type": "array",
                        "items": {"type": "string"},
                    },
                    "description": "Shared scene groups (e.g., UI, Audio).",
                },
                "managerScenePath": {"type": "string", "description": "Persistent scene manager scene path."},
                "triggerName": {"type": "string", "description": "Transition trigger name for transition operation."},
            },
        },
        ["operation"],
    )

    tool_definitions = [
        types.Tool(
            name="unity_ping",
            description="Verify bridge connectivity and return the latest heartbeat information.",
            inputSchema=ping_schema,
        ),
        types.Tool(
            name="unity_scene_crud",
            description="Manage Unity scenes and inspect scene context, including build settings operations.",
            inputSchema=scene_manage_schema,
        ),
        types.Tool(
            name="unity_gameobject_crud",
            description="Create, delete, move, rename, duplicate, or inspect GameObjects in the active scenes.",
            inputSchema=game_object_manage_schema,
        ),
        types.Tool(
            name="unity_component_crud",
            description="Add, remove, update, or inspect components on GameObjects, with optional batch operations.",
            inputSchema=component_manage_schema,
        ),
        types.Tool(
            name="unity_asset_crud",
            description="Manage Unity assets under Assets/: create, update, delete, rename, duplicate, inspect, and update importer settings.",
            inputSchema=asset_manage_schema,
        ),
        types.Tool(
            name="unity_scriptableObject_crud",
            description="Create, inspect, update, delete, duplicate, list, or find ScriptableObject assets.",
            inputSchema=scriptable_object_manage_schema,
        ),
        types.Tool(
            name="unity_prefab_crud",
            description="Manage Unity prefabs: create from GameObject, update, inspect, instantiate in scene, unpack, apply/revert overrides.",
            inputSchema=prefab_manage_schema,
        ),
        types.Tool(
            name="unity_vector_sprite_convert",
            description="Convert vectors and primitives to sprites: generate primitive shapes (square, circle, triangle, polygon), import SVG, convert textures to sprites, create solid color sprites.",
            inputSchema=vector_sprite_convert_schema,
        ),
        types.Tool(
            name="unity_projectSettings_crud",
            description="Read, write, or list Unity Project Settings (Player, Quality, Time, Physics, Audio, Editor).",
            inputSchema=project_settings_manage_schema,
        ),
        types.Tool(
            name="unity_transform_batch",
            description="Mid-level batch utilities for arranging Transforms/RectTransforms, renaming objects, and creating menu lists.",
            inputSchema=transform_batch_schema,
        ),
        types.Tool(
            name="unity_rectTransform_batch",
            description="Mid-level batch utilities for UI RectTransforms: set anchors, pivot, size, position, align to parent, distribute, and match size.",
            inputSchema=rect_transform_batch_schema,
        ),
        types.Tool(
            name="unity_physics_bundle",
            description="Mid-level physics bundle: apply 2D/3D Rigidbody + Collider presets (dynamic, kinematic, static, character, platformer, topDown, vehicle, projectile) or update individual physics properties.",
            inputSchema=physics_bundle_schema,
        ),
        types.Tool(
            name="unity_camera_rig",
            description="Mid-level camera rig utilities: create follow, orbit, split-screen, fixed, or dolly camera rigs with target tracking and smooth movement.",
            inputSchema=camera_rig_schema,
        ),
        types.Tool(
            name="unity_ui_foundation",
            description="Mid-level UI foundation: create Canvas, Panel, Button, Text, Image, and InputField with preset anchors and styling.",
            inputSchema=ui_foundation_schema,
        ),
        types.Tool(
            name="unity_audio_source_bundle",
            description="Mid-level audio source utilities: create and configure AudioSource with presets (music, sfx, ambient, voice, ui) including 2D/3D spatial settings and mixer groups.",
            inputSchema=audio_source_bundle_schema,
        ),
        types.Tool(
            name="unity_input_profile",
            description="Mid-level input profile utilities: create PlayerInput with New Input System, configure action maps, and set up input notification behaviors.",
            inputSchema=input_profile_schema,
        ),
        types.Tool(
            name="unity_gamekit_actor",
            description="High-level GameKit Actor: create game actors with behavior profiles (2D/3D movement), control modes (direct/AI/UI command), stats, abilities, and equipment.",
            inputSchema=gamekit_actor_schema,
        ),
        types.Tool(
            name="unity_gamekit_manager",
            description="High-level GameKit Manager: create game managers (turn-based, realtime, resource pool, event hub, state manager) with persistence and configuration.",
            inputSchema=gamekit_manager_schema,
        ),
        types.Tool(
            name="unity_gamekit_interaction",
            description="High-level GameKit Interaction: create interaction triggers (collision, raycast, proximity, input) with declarative actions (spawn, destroy, sound, message, scene change).",
            inputSchema=gamekit_interaction_schema,
        ),
        types.Tool(
            name="unity_gamekit_ui_command",
            description="High-level GameKit UI Command: create command panels with buttons that send commands to actors with uiCommand control mode.",
            inputSchema=gamekit_ui_command_schema,
        ),
        types.Tool(
            name="unity_gamekit_sceneflow",
            description="High-level GameKit SceneFlow: manage scene transitions with state machine, additive loading, persistent manager scene, and shared scene groups.",
            inputSchema=gamekit_sceneflow_schema,
        ),
    ]

    tool_map = {tool.name: tool for tool in tool_definitions}

    @server.list_tools()
    async def list_tools() -> list[types.Tool]:
        return tool_definitions

    @server.call_tool()
    async def call_tool(name: str, arguments: dict | None) -> list[types.Content]:
        if name not in tool_map:
            raise RuntimeError(f"Unknown tool requested: {name}")

        args = arguments or {}

        if name == "unity_ping":
            _ensure_bridge_connected()
            heartbeat = bridge_manager.get_last_heartbeat()
            bridge_response = await bridge_manager.send_command("pingUnityEditor", {})
            payload = {
                "connected": True,
                "lastHeartbeatAt": heartbeat,
                "bridgeResponse": bridge_response,
            }
            return [types.TextContent(type="text", text=as_pretty_json(payload))]

        if name == "unity_scene_crud":
            return await _call_bridge_tool("sceneManage", args)

        if name == "unity_gameobject_crud":
            return await _call_bridge_tool("gameObjectManage", args)

        if name == "unity_component_crud":
            return await _call_bridge_tool("componentManage", args)

        if name == "unity_asset_crud":
            return await _call_bridge_tool("assetManage", args)

        if name == "unity_scriptableObject_crud":
            return await _call_bridge_tool("scriptableObjectManage", args)

        if name == "unity_prefab_crud":
            return await _call_bridge_tool("prefabManage", args)

        if name == "unity_vector_sprite_convert":
            return await _call_bridge_tool("vectorSpriteConvert", args)

        if name == "unity_projectSettings_crud":
            return await _call_bridge_tool("projectSettingsManage", args)

        if name == "unity_transform_batch":
            return await _call_bridge_tool("transformBatch", args)

        if name == "unity_rectTransform_batch":
            return await _call_bridge_tool("rectTransformBatch", args)

        if name == "unity_physics_bundle":
            return await _call_bridge_tool("physicsBundle", args)

        if name == "unity_camera_rig":
            return await _call_bridge_tool("cameraRig", args)

        if name == "unity_ui_foundation":
            return await _call_bridge_tool("uiFoundation", args)

        if name == "unity_audio_source_bundle":
            return await _call_bridge_tool("audioSourceBundle", args)

        if name == "unity_input_profile":
            return await _call_bridge_tool("inputProfile", args)

        if name == "unity_gamekit_actor":
            return await _call_bridge_tool("gamekitActor", args)

        if name == "unity_gamekit_manager":
            return await _call_bridge_tool("gamekitManager", args)

        if name == "unity_gamekit_interaction":
            return await _call_bridge_tool("gamekitInteraction", args)

        if name == "unity_gamekit_ui_command":
            return await _call_bridge_tool("gamekitUICommand", args)

        if name == "unity_gamekit_sceneflow":
            return await _call_bridge_tool("gamekitSceneFlow", args)

        raise RuntimeError(f"No handler registered for tool '{name}'.")

