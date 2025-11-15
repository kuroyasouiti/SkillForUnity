# UI Tools API Reference

Complete API reference for Unity MCP UI (uGUI) management tools.

## unity_ugui_createFromTemplate

Create UI elements from templates with one command.

### Available Templates

- **Button** - Button with Image, Text, and Button components
- **Text** - Text display element
- **Image** - Image display element
- **RawImage** - Raw texture display
- **Panel** - Panel container with Image
- **ScrollView** - Scrollable view with scrollbars
- **InputField** - Text input field
- **Slider** - Value slider control
- **Toggle** - Checkbox/toggle control
- **Dropdown** - Dropdown selection menu

### Examples

```python
# Create button
unity_ugui_createFromTemplate({
    "template": "Button",
    "name": "StartButton",
    "parentPath": "Canvas",
    "text": "Start Game",
    "width": 200,
    "height": 50,
    "anchorPreset": "middle-center"
})

# Create text
unity_ugui_createFromTemplate({
    "template": "Text",
    "name": "ScoreText",
    "parentPath": "Canvas",
    "text": "Score: 0",
    "fontSize": 24,
    "anchorPreset": "top-center"
})

# Create input field
unity_ugui_createFromTemplate({
    "template": "InputField",
    "name": "NameInput",
    "parentPath": "Canvas/Panel",
    "text": "Enter your name",
    "width": 300,
    "height": 40
})

# Use TextMeshPro (if package installed)
unity_ugui_createFromTemplate({
    "template": "Text",
    "name": "Title",
    "text": "Game Title",
    "fontSize": 48,
    "useTextMeshPro": True
})
```

### Common Parameters

- **template** - UI element type
- **name** - GameObject name (optional)
- **parentPath** - Parent GameObject path (must be under Canvas)
- **anchorPreset** - Anchor preset (top-left, center, stretch-all, etc.)
- **width** / **height** - Element size
- **positionX** / **positionY** - Anchored position
- **text** - Text content (for Button, Text, InputField, etc.)
- **fontSize** - Font size
- **interactable** - Whether element is interactable (default: true)
- **useTextMeshPro** - Use TMP components (default: false)

---

## unity_ugui_layoutManage

Manage layout components on UI GameObjects.

### Layout Types

- **HorizontalLayoutGroup** - Arrange children horizontally
- **VerticalLayoutGroup** - Arrange children vertically
- **GridLayoutGroup** - Arrange children in a grid
- **ContentSizeFitter** - Auto-size based on content
- **LayoutElement** - Control layout properties
- **AspectRatioFitter** - Maintain aspect ratio

### Examples

```python
# Add vertical layout
unity_ugui_layoutManage({
    "operation": "add",
    "gameObjectPath": "Canvas/Panel",
    "layoutType": "VerticalLayoutGroup",
    "spacing": 10,
    "padding": {"left": 20, "right": 20, "top": 20, "bottom": 20},
    "childControlWidth": True,
    "childControlHeight": False
})

# Add grid layout
unity_ugui_layoutManage({
    "operation": "add",
    "gameObjectPath": "Canvas/InventoryPanel",
    "layoutType": "GridLayoutGroup",
    "cellSizeX": 80,
    "cellSizeY": 80,
    "spacing": 10,
    "constraint": "FixedColumnCount",
    "constraintCount": 5
})

# Add content size fitter
unity_ugui_layoutManage({
    "operation": "add",
    "gameObjectPath": "Canvas/Text",
    "layoutType": "ContentSizeFitter",
    "horizontalFit": "PreferredSize",
    "verticalFit": "PreferredSize"
})

# Update existing layout
unity_ugui_layoutManage({
    "operation": "update",
    "gameObjectPath": "Canvas/Panel",
    "layoutType": "VerticalLayoutGroup",
    "spacing": 15
})

# Remove layout
unity_ugui_layoutManage({
    "operation": "remove",
    "gameObjectPath": "Canvas/Panel",
    "layoutType": "VerticalLayoutGroup"
})
```

---

## unity_ugui_manage

Unified UGUI management tool for RectTransform operations.

### Operations

- **rectAdjust** - Adjust RectTransform size
- **setAnchor** - Set custom anchor values
- **setAnchorPreset** - Apply anchor presets
- **convertToAnchored** - Convert absolute to anchored position
- **convertToAbsolute** - Convert anchored to absolute position
- **inspect** - Retrieve RectTransform state
- **updateRect** - Update RectTransform properties

### Examples

```python
# Set anchor preset
unity_ugui_manage({
    "operation": "setAnchorPreset",
    "gameObjectPath": "Canvas/Button",
    "preset": "bottom-center",
    "preservePosition": True
})

# Set custom anchors
unity_ugui_manage({
    "operation": "setAnchor",
    "gameObjectPath": "Canvas/Panel",
    "anchorMinX": 0.2,
    "anchorMinY": 0.2,
    "anchorMaxX": 0.8,
    "anchorMaxY": 0.8,
    "preservePosition": False
})

# Update RectTransform
unity_ugui_manage({
    "operation": "updateRect",
    "gameObjectPath": "Canvas/Image",
    "anchoredPositionX": 100,
    "anchoredPositionY": 50,
    "sizeDeltaX": 200,
    "sizeDeltaY": 100
})

# Inspect RectTransform
unity_ugui_manage({
    "operation": "inspect",
    "gameObjectPath": "Canvas/Button"
})
```

### Anchor Presets

**Corner Presets:**
- top-left, top-center, top-right
- middle-left, middle-center (or "center"), middle-right
- bottom-left, bottom-center, bottom-right

**Stretch Presets:**
- stretch-horizontal, stretch-vertical, stretch-all (or "stretch")
- stretch-top, stretch-middle, stretch-bottom
- stretch-left, stretch-center-vertical, stretch-right

---

## unity_ugui_detectOverlaps

Detect overlapping UI elements in the scene.

### Examples

```python
# Check specific GameObject for overlaps
unity_ugui_detectOverlaps({
    "gameObjectPath": "Canvas/Button",
    "includeChildren": False
})

# Check all UI elements for overlaps
unity_ugui_detectOverlaps({
    "checkAll": True
})

# With threshold
unity_ugui_detectOverlaps({
    "checkAll": True,
    "threshold": 100  # Only report overlaps > 100 square units
})
```

---

## Common UI Workflows

### Create a Menu System

```python
# 1. Setup UI scene
unity_scene_quickSetup({"setupType": "UI"})

# 2. Create panel
unity_ugui_createFromTemplate({
    "template": "Panel",
    "name": "MenuPanel",
    "anchorPreset": "stretch-all"
})

# 3. Add layout to panel
unity_ugui_layoutManage({
    "operation": "add",
    "gameObjectPath": "Canvas/MenuPanel",
    "layoutType": "VerticalLayoutGroup",
    "spacing": 20,
    "padding": {"left": 50, "right": 50, "top": 50, "bottom": 50},
    "childControlWidth": True,
    "childForceExpandWidth": True
})

# 4. Create buttons
for button_text in ["Start", "Options", "Quit"]:
    unity_ugui_createFromTemplate({
        "template": "Button",
        "name": f"{button_text}Button",
        "parentPath": "Canvas/MenuPanel",
        "text": button_text,
        "height": 60
    })
```

### Create Inventory Grid

```python
# Create panel with grid layout
unity_ugui_createFromTemplate({
    "template": "Panel",
    "name": "InventoryPanel"
})

unity_ugui_layoutManage({
    "operation": "add",
    "gameObjectPath": "Canvas/InventoryPanel",
    "layoutType": "GridLayoutGroup",
    "cellSizeX": 80,
    "cellSizeY": 80,
    "spacing": 10,
    "constraint": "FixedColumnCount",
    "constraintCount": 5
})

# Add slots
for i in range(20):
    unity_ugui_createFromTemplate({
        "template": "Image",
        "name": f"Slot{i}",
        "parentPath": "Canvas/InventoryPanel"
    })
```

## Performance Tips

- Always create UI under a Canvas
- Use anchor presets instead of absolute positioning
- Use layouts for dynamic UI organization
- Test UI at different resolutions

See CLAUDE.md for complete API details.
