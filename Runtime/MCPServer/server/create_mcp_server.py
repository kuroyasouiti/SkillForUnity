from __future__ import annotations

from mcp.server import Server

from ..resources.register_resources import register_resources
from ..tools.register_tools import register_tools
from ..version import SERVER_NAME, SERVER_VERSION


def create_mcp_server() -> Server:
    server = Server(
        SERVER_NAME,
        version=SERVER_VERSION,
        instructions="\n".join(
            [
                "Unityプロジェクトと連携するMCPサーバーです。",
                "resourcesからプロジェクト構造、シーン、ログ、アセットを参照できます。",
                "toolsを利用する際はUnityブリッジの接続状態を確認してください。",
            ]
        ),
    )

    register_resources(server)
    register_tools(server)

    return server
