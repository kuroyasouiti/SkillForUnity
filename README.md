# SkillForUnity - Unity Editor Integration via Model Context Protocol

**Enable AI assistants to control Unity Editor in real-time through the Model Context Protocol.**

[![Python](https://img.shields.io/badge/Python-3.10%2B-blue)](https://www.python.org/)
[![Unity](https://img.shields.io/badge/Unity-2021.3%2B-black)](https://unity.com/)
[![MCP](https://img.shields.io/badge/MCP-0.9.0%2B-green)](https://modelcontextprotocol.io/)
[![Version](https://img.shields.io/badge/Version-1.8.0-brightgreen)](https://github.com/kuroyasouiti/SkillForUnity/releases)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## 🆕 What's New in v1.8.0

- **New Tools**: Prefab and Vector Sprite management
  - `unity_prefab_crud`: Create, update, inspect, instantiate, unpack prefabs, apply/revert overrides
  - `unity_vector_sprite_convert`: Generate sprites from primitives (square, circle, triangle, polygon), import SVG, convert textures, create solid color sprites

- **GameKit Framework**: High-level game development tools
  - `unity_gamekit_actor`: Create game actors with behavior profiles (2D/3D movement), control modes (direct/AI/UI command), stats, abilities
  - `unity_gamekit_manager`: Game managers (turn-based, realtime, resource pool) with Machinations framework support
  - `unity_gamekit_interaction`: Interaction triggers with declarative actions and conditions
  - `unity_gamekit_ui_command`: Command panels for UI-driven actor control
  - `unity_gamekit_sceneflow`: Scene state machine with additive loading and shared scene groups

- **Mid-Level Tools**: Batch operations and presets
  - Transform/RectTransform batch operations (arrange, align, distribute)
  - Physics bundles (2D/3D presets: dynamic, kinematic, character, platformer, vehicle)
  - Camera rigs (follow, orbit, split-screen, fixed, dolly)
  - UI foundation (Canvas, Panel, Button, Text, Image, InputField)
  - Audio source bundles (music, sfx, ambient, voice, ui presets)
  - Input profiles (New Input System integration)

- **Compilation Wait Feature**: Automatic compilation handling
  - Operations execute first, then wait for compilation if triggered
  - Bridge reconnection detection for early wait release
  - Transparent wait information in responses

- **Comprehensive Test Suite**: 100+ unit tests
  - Unity Test Framework integration
  - 97.7% pass rate across all tool categories
  - CI/CD with GitHub Actions
  - Editor menu integration (`Tools > SkillForUnity > Run All Tests`)

- **Documentation**: Complete overhaul
  - Test suite documentation and results
  - Tooling roadmap (Japanese)
  - Compilation wait feature guide
  - Legacy cleanup summary
  - [Full Release Notes](docs/Release_Notes_v1.8.0.md)
  - [Changelog](CHANGELOG.md)

## 📦 Skill Package Structure

SkillForUnity is structured as a **Claude Agent Skill** for easier setup and distribution!

```
SkillForUnity/
├── Assets/SkillForUnity/Editor/MCPBridge/    # Unity C# WebSocket Bridge + bundled Claude Skill zip
└── SkillForUnity/                            # ⭐ Claude Skill source (Python MCP server, docs, tools)
    ├── src/                     # Python MCP Server
    ├── setup/                   # Installation scripts
    ├── examples/                # Practical tutorials
    ├── docs/                    # Comprehensive documentation
    └── config/                  # Configuration templates
```

## 🚀 Quick Start

### 1. Install Unity Package

**Option A: Via Unity Package Manager (Recommended)**

1. Open Unity Editor
2. Open **Window > Package Manager**
3. Click **+ (Plus)** button → **Add package from git URL...**
4. Enter: `https://github.com/kuroyasouiti/SkillForUnity.git?path=/Assets/SkillForUnity`
5. Click **Add**

**Option B: Manual Installation**

1. Download this repository
2. Copy `Assets/SkillForUnity` to your Unity project's `Assets/` folder

### 2. Install Claude Skill Package

The Unity package already bundles the Claude Skill archive at `Assets/SkillForUnity/SkillForUnity.zip`.

**Option A: Copy the bundled zip to Claude Desktop's skills folder**

```bash
# Copy the Claude Skill zip
cp Assets/SkillForUnity/SkillForUnity.zip ~/.claude/skills/

# Extract to create ~/.claude/skills/SkillForUnity
cd ~/.claude/skills
unzip -o SkillForUnity.zip
```

**Option B: Register via MCP Window**

1. Open Claude Desktop
2. Open MCP Settings Window
3. Add new MCP server with the skill configuration

**Option C: Manual Configuration**

Add to your Claude Desktop config (`~/.claude/claude_desktop_config.json`):
```json
{
  "mcpServers": {
    "skill-for-unity": {
      "command": "uv",
      "args": ["run", "--directory", "/path/to/SkillForUnity", "src/main.py"],
      "env": {
        "MCP_SERVER_TRANSPORT": "stdio",
        "MCP_LOG_LEVEL": "info"
      }
    }
  }
}
```

### 3. Start Unity Bridge

1. Open Unity Editor with your project
2. Go to **Tools > MCP Assistant**
3. Click **Start Bridge**
4. Wait for "Connected" status

### 4. Test Connection

In Claude Desktop, ask:
```
Can you test the Unity MCP connection?
```

The AI should call `unity_ping()` and show Unity version information.

## 📚 Documentation

### For Users

- **[Claude Skill QUICKSTART](SkillForUnity/QUICKSTART.md)** - Get started in 5 minutes
- **[Claude Skill README](SkillForUnity/README.md)** - Complete skill documentation
- **[Claude Skill examples](SkillForUnity/examples/)** - Practical tutorials and walkthroughs

### For Developers

- **[Claude Skill docs](SkillForUnity/docs/)** - API reference and guides
- **[CLAUDE.md](CLAUDE.md)** - Instructions for Claude Code integration
- **[Best Practices guide](SkillForUnity/docs/guides/best-practices.md)** - Repository guidelines and tips
- **[Test Suite](Assets/SkillForUnity/Tests/Editor/README.md)** - Comprehensive test suite for all tools

## 🏗️ Architecture

```
AI Client (Claude/Cursor) <--(MCP)--> Python MCP Server <--(WebSocket)--> Unity C# Bridge
                                      (SkillForUnity/src/)   (Assets/SkillForUnity/Editor/)
```

### Components

| Component | Location | Description |
|-----------|----------|-------------|
| **Unity C# Bridge** | `Assets/SkillForUnity/Editor/MCPBridge/` | WebSocket server running inside Unity Editor |
| **Python MCP Server** | `SkillForUnity/src/` | MCP protocol implementation |
| **Setup Scripts** | `SkillForUnity/setup/` | Installation and configuration helpers |
| **Examples** | `SkillForUnity/examples/` | Practical tutorials and guides |
| **Documentation** | `SkillForUnity/docs/` | API reference and best practices |

## 🧪 Testing

Comprehensive test suite powered by Unity Test Framework:

- **100+ unit tests** covering all tool categories
- **Automated CI/CD** with GitHub Actions
- **Editor menu integration** for quick test execution
- **Command-line test runners** for batch testing

Run tests via:
- Unity Editor: `Tools > SkillForUnity > Run All Tests`
- PowerShell: `.\run-tests.ps1`
- Bash: `./run-tests.sh`

See [Test Suite Documentation](Assets/SkillForUnity/Tests/Editor/README.md) for details.

## ✨ Features

### Core Tools

- **Scene Management** - Create, load, save, delete, inspect scenes
- **GameObject CRUD** - Full hierarchy manipulation with batch operations
- **Component CRUD** - Add, update, remove components with batch support
- **Asset Operations** - Rename, duplicate, delete, inspect, update importer settings
- **ScriptableObject Management** - Create, inspect, update, delete, duplicate, find ScriptableObject assets
- **Prefab Management** (`unity_prefab_crud`) - Create prefabs from GameObjects, update, inspect, instantiate in scene, unpack, apply/revert overrides
- **Vector Sprite Conversion** (`unity_vector_sprite_convert`) - Generate sprites from primitives (square, circle, triangle, polygon), import SVG, convert textures, create solid color sprites
- **Project Settings** - Configure player, quality, time, physics, audio, and editor settings
- **Tags & Layers** - Add or remove tags and layers via the project settings tool

### Mid-Level Batch Tools

- **Transform Batch** (`unity_transform_batch`) - Arrange objects in circles/lines, sequential/list-based renaming, auto-generate menu hierarchies
- **RectTransform Batch** (`unity_rectTransform_batch`) - Set anchors/pivot/size/position, align to parent presets, distribute horizontally/vertically, match size from source
- **Physics Bundle** (`unity_physics_bundle`) - Apply 2D/3D Rigidbody + Collider presets (dynamic, kinematic, static, character, platformer, topDown, vehicle, projectile), update individual physics properties, inspect physics components
- **Camera Rig** (`unity_camera_rig`) - Create camera rigs (follow, orbit, split-screen, fixed, dolly) with target tracking, smooth movement, and viewport configuration
- **UI Foundation** (`unity_ui_foundation`) - Create UI elements (Canvas, Panel, Button, Text, Image, InputField) with anchor presets, TextMeshPro support, and automatic layout
- **Audio Source Bundle** (`unity_audio_source_bundle`) - Create and configure AudioSource with presets (music, sfx, ambient, voice, ui), 2D/3D spatial audio, and mixer group integration
- **Input Profile** (`unity_input_profile`) - Create PlayerInput with New Input System, configure action maps, set notification behaviors, and create InputActions assets

### High-Level GameKit Tools

- **GameKit Actor** (`unity_gamekit_actor`) - Create game actors with behavior profiles (2D/3D movement types), control modes (direct/AI/UI command), stats, abilities, and equipment loadouts
- **GameKit Manager** (`unity_gamekit_manager`) - Create game managers (turn-based, realtime, resource pool, event hub, state manager) with persistence, turn phases, and resource management
- **GameKit Interaction** (`unity_gamekit_interaction`) - Create interaction triggers (collision, raycast, proximity, input) with declarative actions (spawn, destroy, sound, message, scene change) and conditions
- **GameKit UI Command** (`unity_gamekit_ui_command`) - Create command panels with buttons that send commands to actors with UI command control mode, supporting horizontal/vertical/grid layouts
- **GameKit SceneFlow** (`unity_gamekit_sceneflow`) - Manage scene transitions with state machine, additive loading, persistent manager scene, shared scene groups (UI/Audio), and trigger-based transitions

## 📦 ScriptableObject Management Example

```python
# Create a ScriptableObject asset
unity_scriptableobject_manage({
    "operation": "create",
    "typeName": "MyGame.Data.GameConfig",
    "assetPath": "Assets/Data/DefaultConfig.asset",
    "properties": {
        "gameName": "Adventure Quest",
        "maxPlayers": 8,
        "gameSpeed": 1.5,
        "enableDebugMode": True
    }
})

# Inspect properties
config_info = unity_scriptableobject_manage({
    "operation": "inspect",
    "assetPath": "Assets/Data/DefaultConfig.asset",
    "includeProperties": True
})

# Update selected values
unity_scriptableobject_manage({
    "operation": "update",
    "assetPath": "Assets/Data/DefaultConfig.asset",
    "properties": {
        "maxPlayers": 16,
        "gameSpeed": 2.0
    }
})

# Duplicate for experimentation
unity_scriptableobject_manage({
    "operation": "duplicate",
    "sourceAssetPath": "Assets/Data/DefaultConfig.asset",
    "destinationAssetPath": "Assets/Data/HighSpeedConfig.asset"
})

# List all configs in a folder
all_configs = unity_scriptableobject_manage({
    "operation": "findByType",
    "typeName": "MyGame.Data.GameConfig",
    "searchPath": "Assets/Data",
    "includeProperties": False
})
```

## 🛠️ Development

### Project Structure

```
SkillForUnity/
├── Assets/
│   └── SkillForUnity/
│       ├── SkillForUnity.zip        # Bundled Claude Skill MCP server package
│       └── Editor/
│           └── MCPBridge/           # Unity C# Bridge
│               ├── McpBridgeService.cs
│               ├── McpCommandProcessor.cs
│               └── McpContextCollector.cs
│
├── .claude/
│   └── skills/
│       └── SkillForUnity/           # Claude Skill (Python MCP server)
│           ├── src/                 # Server source
│           │   ├── bridge/          # Unity Bridge communication
│           │   ├── tools/           # MCP tool definitions
│           │   ├── resources/       # MCP resources
│           │   └── main.py          # Entry point
│           ├── setup/               # Installation scripts
│           ├── examples/            # Tutorials
│           ├── docs/                # Documentation
│           ├── config/              # Configuration templates
│           ├── skill.yml            # Skill manifest
│           └── pyproject.toml       # Python package config
│
├── ProjectSettings/                 # Unity project settings
├── Packages/                        # Unity packages
└── README.md                        # This file
```

### Install Dev Dependencies

```bash
cd SkillForUnity
uv sync --dev
```

### Run Tests

```bash
cd SkillForUnity
pytest
```

### Format Code

```bash
cd SkillForUnity
black src/
ruff check src/
```

## 🤝 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests and documentation
5. Submit a pull request

See [SkillForUnity/docs/guides/best-practices.md](SkillForUnity/docs/guides/best-practices.md) for coding guidelines.

## 📄 License

MIT License - see [MIT License](https://opensource.org/licenses/MIT) for details.

## 🙏 Acknowledgments

- **Model Context Protocol** by Anthropic
- **Unity Technologies** for the amazing game engine
- All contributors and community members

## 🆘 Support

- **Quick Start**: [SkillForUnity/QUICKSTART.md](SkillForUnity/QUICKSTART.md)
- **Examples**: [SkillForUnity/examples/](SkillForUnity/examples/)
- **Troubleshooting**: [SkillForUnity/docs/troubleshooting.md](SkillForUnity/docs/troubleshooting.md)
- **Issues**: [GitHub Issues](https://github.com/yourusername/SkillForUnity/issues)

## 🔄 Migration from Old Structure

If you were using the old structure (`Assets/Runtime/MCPServer/` or `SkillPackage/`):

1. **Unity Side**: Install via Unity Package Manager (see installation instructions above)
   - The Unity Bridge remains at `Assets/SkillForUnity/Editor/MCPBridge/` (unchanged)
2. **Claude Skill Side**: Extract `Assets/SkillForUnity/SkillForUnity.zip` into your Claude Desktop skills folder (creates `~/.claude/skills/SkillForUnity`)
   - Or configure via MCP Window by pointing to the extracted `skill.yml`
   - Or manually add to `claude_desktop_config.json`
3. Remove old installation files if desired

---

**Made with ❤️ for the Unity and AI community**

**Start building amazing Unity projects with AI assistance today!** 🚀
