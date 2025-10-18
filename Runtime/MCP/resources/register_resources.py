from __future__ import annotations

from pathlib import Path
from typing import Iterable
from urllib.parse import urlparse

import mcp.types as types
from mcp.server import Server

from ..bridge.bridge_manager import bridge_manager
from ..config.env import env
from ..logger import logger
from ..services.editor_log_watcher import editor_log_watcher
from ..utils.fs_utils import path_exists
from ..utils.project_structure import build_project_structure_summary


def _sanitize_unity_path(asset_path: str) -> str:
    return asset_path.replace("\\", "/")


def _is_likely_text_asset(file_path: Path) -> bool:
    return file_path.suffix.lower() in {
        ".cs",
        ".shader",
        ".cginc",
        ".compute",
        ".json",
        ".txt",
        ".md",
        ".meta",
        ".asmdef",
        ".uxml",
        ".uss",
        ".prefab",
        ".unity",
    }


def _render_hierarchy(node: dict, depth: int = 0) -> Iterable[str]:
    prefix = "  " * depth
    components = node.get("components") or []
    component_summary = ", ".join(comp.get("type", "") for comp in components if comp.get("type"))
    header = f"{prefix}- {node.get('name', '(unknown)')} ({node.get('type', 'Unknown')}"
    if component_summary:
        header += f" | {component_summary}"
    header += ")"

    yield header
    for child in node.get("children") or []:
        yield from _render_hierarchy(child, depth + 1)


def register_resources(server: Server) -> None:
    @server.list_resources()
    async def list_resources() -> list[types.Resource]:
        base_resources = [
            types.Resource(
                uri="unity://project/structure",
                name="Unity Project Structure",
                description="主要フォルダとファイル拡張子の概要",
                mimeType="text/markdown",
            ),
            types.Resource(
                uri="unity://editor/log",
                name="Unity Editor Log",
                description="最新のUnity Editorログから抜粋",
                mimeType="text/plain",
            ),
            types.Resource(
                uri="unity://scene/active",
                name="Active Unity Scene",
                description="ブリッジが把握しているアクティブシーン構造",
                mimeType="text/markdown",
            ),
        ]

        context = bridge_manager.get_context() or {}
        assets = context.get("assets") or []

        asset_resources = [
            types.Resource(
                uri=f"unity://asset/{asset['guid']}",
                name=_sanitize_unity_path(asset.get("path", asset["guid"])),
                description=asset.get("label") or asset.get("type"),
            )
            for asset in assets
            if asset.get("guid")
        ]

        return base_resources + asset_resources

    @server.list_resource_templates()
    async def list_resource_templates() -> list[types.ResourceTemplate]:
        return [
            types.ResourceTemplate(
                uriTemplate="unity://asset/{guid}",
                name="Unity Asset (GUID)",
                description="Unityブリッジが提供するアセットインデックスからGUIDで参照します。",
            )
        ]

    @server.read_resource()
    async def read_resource(uri: types.AnyUrl) -> Iterable[types.ReadResourceContents]:
        parsed = urlparse(str(uri))
        category = parsed.netloc
        path = parsed.path.lstrip("/")

        if category == "project" and path == "structure":
            try:
                summary = await build_project_structure_summary(env.unity_project_root)
                return [
                    types.TextResourceContents(
                        uri="unity://project/structure",
                        text=summary,
                        mimeType="text/markdown",
                    )
                ]
            except Exception as exc:  # pragma: no cover - defensive
                logger.error("Failed to build project summary: %s", exc)
                return [
                    types.TextResourceContents(
                        uri="unity://project/structure",
                        text=f"プロジェクト構造の取得に失敗しました: {exc}",
                        mimeType="text/plain",
                    )
                ]

        if category == "editor" and path == "log":
            snapshot = editor_log_watcher.get_snapshot(800)
            body = (
                "\n".join(snapshot.lines)
                if snapshot.lines
                else "ログエントリが見つかりません。Unity Editorのログパスを確認してください。"
            )
            return [
                types.TextResourceContents(
                    uri="unity://editor/log",
                    text=body,
                    mimeType="text/plain",
                )
            ]

        if category == "scene" and path == "active":
            context = bridge_manager.get_context()
            if not context or not context.get("hierarchy"):
                return [
                    types.TextResourceContents(
                        uri="unity://scene/active",
                        text="## アクティブシーン\nUnityブリッジからシーン情報を取得できませんでした。",
                        mimeType="text/markdown",
                    )
                ]

            hierarchy_lines = "\n".join(_render_hierarchy(context["hierarchy"]))
            active_scene = context.get("activeScene") or {}
            header = (
                f"{active_scene.get('name')} ({active_scene.get('path')})"
                if active_scene
                else "未保存シーン"
            )
            return [
                types.TextResourceContents(
                    uri="unity://scene/active",
                    text=f"## アクティブシーン: {header}\n\n{hierarchy_lines}",
                    mimeType="text/markdown",
                )
            ]

        if category == "asset" and path:
            guid = path
            context = bridge_manager.get_context() or {}
            asset = next(
                (
                    item
                    for item in context.get("assets") or []
                    if item.get("guid") == guid
                ),
                None,
            )

            if not asset:
                return [
                    types.TextResourceContents(
                        uri=str(uri),
                        text=f"GUID {guid} に対応するアセットがブリッジに登録されていません。",
                        mimeType="text/plain",
                    )
                ]

            project_path = env.unity_project_root / Path(asset.get("path", ""))
            if not path_exists(project_path):
                return [
                    types.TextResourceContents(
                        uri=str(uri),
                        text=f"アセットファイルが見つかりません: {project_path}",
                        mimeType="text/plain",
                    )
                ]

            if not _is_likely_text_asset(project_path):
                return [
                    types.TextResourceContents(
                        uri=str(uri),
                        text=f"バイナリアセットのためテキスト表示に対応していません ({asset.get('path')})",
                        mimeType="text/plain",
                    )
                ]

            try:
                contents = project_path.read_text(encoding="utf-8")
                return [
                    types.TextResourceContents(
                        uri=str(uri),
                        text=contents,
                        mimeType="text/plain",
                    )
                ]
            except UnicodeDecodeError:
                return [
                    types.TextResourceContents(
                        uri=str(uri),
                        text="アセットをUTF-8として読み取れませんでした。",
                        mimeType="text/plain",
                    )
                ]
            except OSError as exc:
                logger.error("Failed to read asset %s: %s", project_path, exc)
                return [
                    types.TextResourceContents(
                        uri=str(uri),
                        text=f"アセットの読み込みに失敗しました: {exc}",
                        mimeType="text/plain",
                    )
                ]

        return [
            types.TextResourceContents(
                uri=str(uri),
                text=f"未知のUnityリソースです: {uri}",
                mimeType="text/plain",
            )
        ]
