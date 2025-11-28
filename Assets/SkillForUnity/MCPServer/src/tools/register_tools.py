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
            name="unity_projectSettings_crud",
            description="Read, write, or list Unity Project Settings (Player, Quality, Time, Physics, Audio, Editor).",
            inputSchema=project_settings_manage_schema,
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

        if name == "unity_projectSettings_crud":
            return await _call_bridge_tool("projectSettingsManage", args)

        raise RuntimeError(f"No handler registered for tool '{name}'.")

