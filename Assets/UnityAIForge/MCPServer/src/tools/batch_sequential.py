"""
Sequential batch execution tool with resume capability.

Executes operations sequentially, stops on error, and allows resuming from the failed point.
"""

import json
import logging
from pathlib import Path
from typing import Any, Dict, List, Optional
from datetime import datetime

from mcp.types import Tool, TextContent
from ..bridge.client import UnityBridgeClient

logger = logging.getLogger(__name__)

# State file to persist queue
STATE_FILE = Path(__file__).parent.parent.parent / ".batch_queue_state.json"

class BatchQueueState:
    """Manages the state of the batch queue."""
    
    def __init__(self):
        self.operations: List[Dict[str, Any]] = []
        self.current_index: int = 0
        self.last_error: Optional[str] = None
        self.last_error_index: Optional[int] = None
        self.started_at: Optional[str] = None
        self.last_updated: Optional[str] = None
        
    def to_dict(self) -> Dict[str, Any]:
        """Convert state to dictionary."""
        return {
            "operations": self.operations,
            "current_index": self.current_index,
            "last_error": self.last_error,
            "last_error_index": self.last_error_index,
            "started_at": self.started_at,
            "last_updated": self.last_updated,
            "remaining_count": len(self.operations) - self.current_index,
            "completed_count": self.current_index,
            "total_count": len(self.operations)
        }
    
    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> 'BatchQueueState':
        """Create state from dictionary."""
        state = cls()
        state.operations = data.get("operations", [])
        state.current_index = data.get("current_index", 0)
        state.last_error = data.get("last_error")
        state.last_error_index = data.get("last_error_index")
        state.started_at = data.get("started_at")
        state.last_updated = data.get("last_updated")
        return state
    
    def save(self):
        """Save state to file."""
        try:
            STATE_FILE.parent.mkdir(parents=True, exist_ok=True)
            with open(STATE_FILE, 'w', encoding='utf-8') as f:
                json.dump(self.to_dict(), f, indent=2, ensure_ascii=False)
            logger.info(f"Batch queue state saved: {self.current_index}/{len(self.operations)}")
        except Exception as e:
            logger.error(f"Failed to save batch queue state: {e}")
    
    @classmethod
    def load(cls) -> 'BatchQueueState':
        """Load state from file."""
        try:
            if STATE_FILE.exists():
                with open(STATE_FILE, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                logger.info(f"Batch queue state loaded: {data.get('current_index', 0)}/{data.get('total_count', 0)}")
                return cls.from_dict(data)
        except Exception as e:
            logger.error(f"Failed to load batch queue state: {e}")
        return cls()
    
    def clear(self):
        """Clear the state."""
        self.operations = []
        self.current_index = 0
        self.last_error = None
        self.last_error_index = None
        self.started_at = None
        self.last_updated = None
        if STATE_FILE.exists():
            STATE_FILE.unlink()
        logger.info("Batch queue state cleared")


# Global state instance
_batch_state = BatchQueueState.load()


def get_batch_state() -> BatchQueueState:
    """Get the current batch state."""
    return _batch_state


async def execute_batch_sequential(
    bridge_client: UnityBridgeClient,
    operations: List[Dict[str, Any]],
    resume: bool = False,
    stop_on_error: bool = True
) -> Dict[str, Any]:
    """
    Execute operations sequentially with resume capability.
    
    Args:
        bridge_client: Unity bridge client
        operations: List of operations to execute. Each operation should have:
                   - tool: Tool name (e.g., "unity_gameobject_crud")
                   - arguments: Tool arguments as dict
        resume: If True, resume from previous error point. If False, start fresh.
        stop_on_error: If True, stop on first error. If False, continue (not recommended for sequential).
    
    Returns:
        Dict with execution results and status
    """
    global _batch_state
    
    current_time = datetime.utcnow().isoformat()
    
    # Initialize or resume
    if not resume or not _batch_state.operations:
        # Start fresh
        _batch_state.operations = operations
        _batch_state.current_index = 0
        _batch_state.last_error = None
        _batch_state.last_error_index = None
        _batch_state.started_at = current_time
        logger.info(f"Starting new batch execution with {len(operations)} operations")
    else:
        # Resume from saved state
        logger.info(f"Resuming batch execution from operation {_batch_state.current_index}/{len(_batch_state.operations)}")
    
    _batch_state.last_updated = current_time
    _batch_state.save()
    
    results = []
    errors = []
    
    # Execute operations sequentially
    while _batch_state.current_index < len(_batch_state.operations):
        idx = _batch_state.current_index
        operation = _batch_state.operations[idx]
        
        tool_name = operation.get("tool")
        arguments = operation.get("arguments", {})
        
        logger.info(f"Executing operation {idx + 1}/{len(_batch_state.operations)}: {tool_name}")
        
        try:
            # Send operation to Unity bridge
            response = await bridge_client.send_command(tool_name, arguments)
            
            if response.get("success"):
                results.append({
                    "index": idx,
                    "tool": tool_name,
                    "success": True,
                    "result": response.get("result")
                })
                logger.info(f"Operation {idx + 1} completed successfully")
            else:
                # Operation failed
                error_msg = response.get("error", "Unknown error")
                errors.append({
                    "index": idx,
                    "tool": tool_name,
                    "error": error_msg
                })
                _batch_state.last_error = error_msg
                _batch_state.last_error_index = idx
                logger.error(f"Operation {idx + 1} failed: {error_msg}")
                
                if stop_on_error:
                    _batch_state.save()
                    return {
                        "success": False,
                        "stopped_at_index": idx,
                        "completed": results,
                        "errors": errors,
                        "remaining_operations": len(_batch_state.operations) - _batch_state.current_index,
                        "message": f"Execution stopped at operation {idx + 1} due to error. Use resume=true to continue.",
                        "last_error": error_msg
                    }
        
        except Exception as e:
            error_msg = str(e)
            errors.append({
                "index": idx,
                "tool": tool_name,
                "error": error_msg,
                "exception": True
            })
            _batch_state.last_error = error_msg
            _batch_state.last_error_index = idx
            logger.exception(f"Exception in operation {idx + 1}")
            
            if stop_on_error:
                _batch_state.save()
                return {
                    "success": False,
                    "stopped_at_index": idx,
                    "completed": results,
                    "errors": errors,
                    "remaining_operations": len(_batch_state.operations) - _batch_state.current_index,
                    "message": f"Execution stopped at operation {idx + 1} due to exception. Use resume=true to continue.",
                    "last_error": error_msg
                }
        
        # Move to next operation
        _batch_state.current_index += 1
        _batch_state.save()
    
    # All operations completed
    _batch_state.clear()
    
    return {
        "success": len(errors) == 0,
        "completed": results,
        "errors": errors,
        "total_operations": len(operations),
        "message": f"All {len(operations)} operations completed successfully." if len(errors) == 0 
                  else f"Completed with {len(errors)} error(s)."
    }


# Tool definition
TOOL = Tool(
    name="unity_batch_sequential_execute",
    description="""Execute multiple Unity operations sequentially with resume capability.

This tool executes operations one by one in order. If an error occurs, execution stops and the remaining operations are saved. You can resume from the failed operation by calling the tool again with resume=true.

Key features:
- Sequential execution (one operation at a time)
- Stops on first error
- Saves remaining operations for resume
- Check remaining operations via unity_batch_queue_status resource

Use cases:
- Multi-step scene setup that might fail midway
- Batch GameObject creation with dependencies
- Sequential configuration that must succeed in order
- Any workflow where you want to retry from failure point""",
    inputSchema={
        "type": "object",
        "properties": {
            "operations": {
                "type": "array",
                "description": "List of operations to execute. Each operation has 'tool' and 'arguments' fields.",
                "items": {
                    "type": "object",
                    "properties": {
                        "tool": {
                            "type": "string",
                            "description": "Tool name (e.g., 'unity_gameobject_crud', 'unity_component_crud')"
                        },
                        "arguments": {
                            "type": "object",
                            "description": "Tool arguments as a dictionary"
                        }
                    },
                    "required": ["tool", "arguments"]
                }
            },
            "resume": {
                "type": "boolean",
                "description": "If true, resume from previous failure point. If false, start fresh (clears saved queue).",
                "default": False
            },
            "stop_on_error": {
                "type": "boolean",
                "description": "If true, stop on first error. If false, continue (not recommended for sequential workflows).",
                "default": True
            }
        },
        "required": []
    }
)


async def handle_batch_sequential(arguments: Dict[str, Any], bridge_client: UnityBridgeClient) -> List[TextContent]:
    """Handle the unity_batch_sequential_execute tool call."""
    operations = arguments.get("operations", [])
    resume = arguments.get("resume", False)
    stop_on_error = arguments.get("stop_on_error", True)
    
    # Validate operations
    if not resume and not operations:
        return [TextContent(
            type="text",
            text=json.dumps({
                "success": False,
                "error": "No operations provided. Specify 'operations' array or set 'resume' to true."
            }, indent=2)
        )]
    
    # Execute batch
    result = await execute_batch_sequential(
        bridge_client=bridge_client,
        operations=operations,
        resume=resume,
        stop_on_error=stop_on_error
    )
    
    return [TextContent(
        type="text",
        text=json.dumps(result, indent=2, ensure_ascii=False)
    )]

