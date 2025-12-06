from __future__ import annotations

import os
import secrets
from dataclasses import dataclass
from pathlib import Path
from typing import Literal

from dotenv import load_dotenv

load_dotenv()

LogLevel = Literal["fatal", "error", "warn", "info", "debug", "trace", "silent"]


def _parse_bool(value: str | None, default: bool) -> bool:
    if value is None:
        return default
    return value.strip().lower() in {"1", "true", "yes", "on"}


def _parse_int(
    value: str | None, default: int, minimum: int | None = None, maximum: int | None = None
) -> int:
    try:
        parsed = int(value) if value is not None else default
    except ValueError:
        return default

    if minimum is not None and parsed < minimum:
        return default
    if maximum is not None and parsed > maximum:
        return default
    return parsed


def _resolve_path(value: str | None, default: Path) -> Path:
    if not value:
        return default

    path = Path(value)
    if not path.is_absolute():
        path = Path.cwd() / path
    return path.resolve()


def _default_editor_log() -> Path:
    if os.name == "nt":
        local_app_data = os.environ.get("LOCALAPPDATA", "")
        return Path(local_app_data) / "Unity" / "Editor" / "Editor.log"

    home = os.environ.get("HOME", "")
    return Path(home) / "Library" / "Logs" / "Unity" / "Editor.log"


def _parse_log_level(value: str | None) -> LogLevel:
    normalized = (value or "").strip().lower()
    allowed: tuple[LogLevel, ...] = (
        "fatal",
        "error",
        "warn",
        "info",
        "debug",
        "trace",
        "silent",
    )
    return normalized if normalized in allowed else "info"


def _load_or_create_token(project_root: Path) -> str | None:
    """
    Resolve bridge token from a local file if env is unset; create one if absent.
    Priority: current working directory (.mcp_bridge_token), then project_root/.mcp_bridge_token.
    """
    # 1) current working directory (for installed server run from its install dir)
    cwd_path = Path.cwd() / ".mcp_bridge_token"
    if cwd_path.exists():
        try:
            content = cwd_path.read_text(encoding="utf-8").strip()
            if content:
                return content
        except OSError:
            pass

    # 2) project root
    token_path = project_root / ".mcp_bridge_token"

    # Try load existing
    if token_path.exists():
        try:
            content = token_path.read_text(encoding="utf-8").strip()
            if content:
                return content
        except OSError:
            return None

    # Create new
    token = secrets.token_urlsafe(32)
    try:
        token_path.write_text(token, encoding="utf-8")
    except OSError:
        # If persisting fails, still return the generated token
        return token
    return token


@dataclass(frozen=True)
class ServerEnv:
    port: int
    host: str
    log_level: LogLevel
    unity_project_root: Path
    unity_editor_log_path: Path
    enable_file_watcher: bool
    bridge_token: str | None
    unity_bridge_host: str
    unity_bridge_port: int
    bridge_reconnect_ms: int


env = ServerEnv(
    port=_parse_int(os.environ.get("MCP_SERVER_PORT"), default=6007, minimum=1, maximum=65535),
    host=os.environ.get("MCP_SERVER_HOST", "127.0.0.1"),
    log_level=_parse_log_level(os.environ.get("MCP_SERVER_LOG_LEVEL")),
    unity_project_root=_resolve_path(
        os.environ.get("UNITY_PROJECT_ROOT"), Path.cwd()
    ),
    unity_editor_log_path=_resolve_path(
        os.environ.get("UNITY_EDITOR_LOG_PATH"), _default_editor_log()
    ),
    enable_file_watcher=_parse_bool(os.environ.get("MCP_ENABLE_FILE_WATCHER"), True),
    bridge_token=os.environ.get("MCP_BRIDGE_TOKEN")
    or _load_or_create_token(_resolve_path(os.environ.get("UNITY_PROJECT_ROOT"), Path.cwd())),
    unity_bridge_host=os.environ.get("UNITY_BRIDGE_HOST", "127.0.0.1"),
    unity_bridge_port=_parse_int(
        os.environ.get("UNITY_BRIDGE_PORT"), default=7070, minimum=1, maximum=65535
    ),
    bridge_reconnect_ms=_parse_int(
        os.environ.get("MCP_BRIDGE_RECONNECT_MS"), default=5000, minimum=0
    ),
)
