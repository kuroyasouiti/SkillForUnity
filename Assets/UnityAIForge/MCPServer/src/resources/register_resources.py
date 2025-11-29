"""
MCP Resources for Unity-AI-Forge

Resources are disabled as all functionality is available through tools:
- Project structure: Use unity_scene_crud, unity_gameobject_crud
- Scene information: Use unity_scene_crud with inspect operation
- Asset access: Use unity_asset_crud with inspect operation

This module is kept for compatibility but registers no resources.
"""

from __future__ import annotations

from mcp.server import Server


def register_resources(server: Server) -> None:
    """
    Register MCP resources (currently disabled).
    
    All Unity context and asset information is accessible through tools,
    making resources redundant. This function is kept for API compatibility
    but does not register any resources.
    """
    # Resources disabled - all functionality available through tools
    pass
