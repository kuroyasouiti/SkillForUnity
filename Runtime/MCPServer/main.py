from __future__ import annotations

import asyncio
import sys
import contextlib
from typing import Any

import uvicorn
from mcp.server import NotificationOptions
from mcp.server.websocket import websocket_server as mcp_websocket_server
from starlette.applications import Starlette
from starlette.requests import Request
from starlette.responses import JSONResponse, PlainTextResponse
from starlette.routing import Route, WebSocketRoute
from starlette.websockets import WebSocket, WebSocketDisconnect

from .bridge.bridge_connector import bridge_connector
from .bridge.bridge_manager import bridge_manager
from .config.env import env
from .logger import logger
from .server.create_mcp_server import create_mcp_server
from .services.editor_log_watcher import editor_log_watcher
from .version import SERVER_NAME, SERVER_VERSION

mcp_server = create_mcp_server()


def _bridge_connected() -> None:
    logger.info("Unity bridge handshake completed")


def _bridge_disconnected() -> None:
    logger.warning("Unity bridge disconnected")


def _bridge_context_updated(context: dict[str, Any]) -> None:
    active_scene = context.get("activeScene") or {}
    logger.debug(
        "Unity context updated (scene=%s updatedAt=%s)",
        active_scene.get("name"),
        context.get("updatedAt"),
    )


bridge_manager.on("connected", _bridge_connected)
bridge_manager.on("disconnected", _bridge_disconnected)
bridge_manager.on("contextUpdated", _bridge_context_updated)


async def health_endpoint(_: Request) -> JSONResponse:
    return JSONResponse(
        {
            "status": "ok",
            "bridgeConnected": bridge_manager.is_connected(),
            "lastHeartbeatAt": bridge_manager.get_last_heartbeat(),
            "server": SERVER_NAME,
            "version": SERVER_VERSION,
        }
    )


async def default_endpoint(_: Request) -> PlainTextResponse:
    return PlainTextResponse("Not Found", status_code=404)


async def mcp_ws_endpoint(websocket: WebSocket) -> None:
    client = websocket.client
    logger.info(
        "MCP client connected via WebSocket (address=%s:%s user_agent=%s)",
        client.host if client else "unknown",
        client.port if client else "unknown",
        websocket.headers.get("user-agent"),
    )

    try:
        async with mcp_websocket_server(websocket.scope, websocket.receive, websocket.send) as (
            read_stream,
            write_stream,
        ):
            init_options = mcp_server.create_initialization_options(
                notification_options=NotificationOptions(
                    resources_changed=True,
                    tools_changed=True,
                )
            )
            await mcp_server.run(read_stream, write_stream, init_options)
    except WebSocketDisconnect:
        logger.info("MCP client disconnected")
    except Exception as exc:  # pragma: no cover - defensive
        logger.exception("Failed to serve MCP client: %s", exc)
    finally:
        with contextlib.suppress(Exception):
            await websocket.close()


async def startup() -> None:
    logger.info(
        "Starting Unity MCP server (host=%s port=%s)",
        env.host,
        env.port,
    )
    await editor_log_watcher.start()
    bridge_connector.start()


async def shutdown() -> None:
    logger.info("Shutting down Unity MCP server")
    await bridge_connector.stop()
    await editor_log_watcher.stop()


routes = [
    Route("/healthz", health_endpoint, methods=["GET"]),
    Route("/{path:path}", default_endpoint, methods=["GET", "POST", "PUT", "PATCH", "DELETE"]),
    WebSocketRoute("/mcp", mcp_ws_endpoint),
]

app = Starlette(routes=routes, on_startup=[startup], on_shutdown=[shutdown])


def main() -> None:
    log_level = {
        "trace": "debug",
        "debug": "debug",
        "info": "info",
        "warn": "warning",
        "error": "error",
        "fatal": "critical",
        "silent": "critical",
    }.get(env.log_level, "info")

    config = uvicorn.Config(
        app,
        host=env.host,
        port=env.port,
        log_level=log_level,
        loop="asyncio",
        lifespan="on",
    )
    server = uvicorn.Server(config)

    try:
        asyncio.run(server.serve())
    except KeyboardInterrupt:
        logger.info("Received interrupt, shutting down")
    except Exception:
        logger.exception("Uvicorn server failed")
        sys.exit(1)


__all__ = ["app", "main"]
