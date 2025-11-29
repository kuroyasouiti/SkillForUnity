using UnityEngine;
using System.Collections;

namespace UnityAIForge.GameKit
{
    /// <summary>
    /// Tile-based grid movement component for 2D games.
    /// Moves in discrete grid units with smooth interpolation.
    /// </summary>
    [RequireComponent(typeof(GameKitActor))]
    public class TileGridMovement : MonoBehaviour
    {
        [Header("Grid Settings")]
        [Tooltip("Size of each grid cell in world units")]
        [SerializeField] private float gridSize = 1f;
        
        [Tooltip("Time to move from one tile to another")]
        [SerializeField] private float moveSpeed = 0.2f;
        
        [Tooltip("Snap to grid on start")]
        [SerializeField] private bool snapToGridOnStart = true;
        
        [Header("Movement Settings")]
        [Tooltip("Allow diagonal movement")]
        [SerializeField] private bool allowDiagonal = false;
        
        [Tooltip("Queue next move while moving")]
        [SerializeField] private bool allowMoveQueue = true;
        
        [Tooltip("Layer mask for collision detection")]
        [SerializeField] private LayerMask obstacleLayer;
        
        [Header("Animation")]
        [Tooltip("Use smooth interpolation")]
        [SerializeField] private bool smoothMovement = true;
        
        [Tooltip("Animation curve for movement")]
        [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private GameKitActor actor;
        private Vector3 targetPosition;
        private Vector3 queuedDirection = Vector3.zero;
        private bool isMoving = false;
        private Coroutine moveCoroutine;

        // Current grid position
        public Vector2Int GridPosition => WorldToGrid(transform.position);
        public bool IsMoving => isMoving;
        public float GridSize => gridSize;

        private void Awake()
        {
            actor = GetComponent<GameKitActor>();
            targetPosition = transform.position;
            
            // Subscribe to actor input events early for editor tests
            if (actor != null && actor.OnMoveInput != null)
            {
                actor.OnMoveInput.AddListener(HandleMoveInput);
            }
        }

        private void Start()
        {
            if (snapToGridOnStart)
            {
                SnapToGrid();
            }

            // Ensure subscription (defensive programming)
            if (actor != null && actor.OnMoveInput != null)
            {
                // Remove first to avoid duplicate listeners
                actor.OnMoveInput.RemoveListener(HandleMoveInput);
                actor.OnMoveInput.AddListener(HandleMoveInput);
            }
        }

        private void OnDestroy()
        {
            if (actor != null)
            {
                actor.OnMoveInput.RemoveListener(HandleMoveInput);
            }
        }

        /// <summary>
        /// Handles move input from the actor hub.
        /// </summary>
        private void HandleMoveInput(Vector3 direction)
        {
            // Normalize to grid directions
            Vector2Int gridDirection = NormalizeToGridDirection(direction);
            
            if (gridDirection == Vector2Int.zero)
                return;

            if (isMoving)
            {
                if (allowMoveQueue)
                {
                    queuedDirection = new Vector3(gridDirection.x, gridDirection.y, 0);
                }
            }
            else
            {
                TryMove(gridDirection);
            }
        }

        /// <summary>
        /// Attempts to move in the specified grid direction.
        /// </summary>
        public bool TryMove(Vector2Int direction)
        {
            if (isMoving)
                return false;

            Vector3 newPosition = GridToWorld(GridPosition + direction);
            
            // Check for obstacles
            if (IsBlocked(newPosition))
                return false;

            // Start movement
            targetPosition = newPosition;
            
            if (smoothMovement)
            {
                if (moveCoroutine != null)
                    StopCoroutine(moveCoroutine);
                moveCoroutine = StartCoroutine(SmoothMove(targetPosition));
            }
            else
            {
                transform.position = targetPosition;
            }

            return true;
        }

        /// <summary>
        /// Smoothly interpolates to target position.
        /// </summary>
        private IEnumerator SmoothMove(Vector3 target)
        {
            isMoving = true;
            Vector3 startPosition = transform.position;
            float elapsed = 0f;

            while (elapsed < moveSpeed)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / moveSpeed);
                float curveValue = movementCurve.Evaluate(t);
                transform.position = Vector3.Lerp(startPosition, target, curveValue);
                yield return null;
            }

            transform.position = target;
            isMoving = false;

            // Process queued move
            if (queuedDirection != Vector3.zero)
            {
                Vector2Int queuedDir = new Vector2Int(
                    Mathf.RoundToInt(queuedDirection.x),
                    Mathf.RoundToInt(queuedDirection.y)
                );
                queuedDirection = Vector3.zero;
                TryMove(queuedDir);
            }
        }

        /// <summary>
        /// Snaps current position to nearest grid cell.
        /// </summary>
        public void SnapToGrid()
        {
            Vector2Int gridPos = GridPosition;
            transform.position = GridToWorld(gridPos);
            targetPosition = transform.position;
        }

        /// <summary>
        /// Checks if a position is blocked by obstacles.
        /// </summary>
        private bool IsBlocked(Vector3 worldPosition)
        {
            if (obstacleLayer == 0)
                return false;

            // Check for colliders at target position
            Collider2D hit = Physics2D.OverlapCircle(worldPosition, gridSize * 0.4f, obstacleLayer);
            return hit != null;
        }

        /// <summary>
        /// Converts world position to grid coordinates.
        /// Uses floor + 0.5 offset to ensure 0.5 rounds up consistently.
        /// </summary>
        private Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            return new Vector2Int(
                Mathf.FloorToInt((worldPosition.x / gridSize) + 0.5f),
                Mathf.FloorToInt((worldPosition.y / gridSize) + 0.5f)
            );
        }

        /// <summary>
        /// Converts grid coordinates to world position.
        /// </summary>
        private Vector3 GridToWorld(Vector2Int gridPosition)
        {
            return new Vector3(
                gridPosition.x * gridSize,
                gridPosition.y * gridSize,
                transform.position.z
            );
        }

        /// <summary>
        /// Normalizes input direction to grid direction (4 or 8 directions).
        /// </summary>
        private Vector2Int NormalizeToGridDirection(Vector3 direction)
        {
            if (direction.magnitude < 0.1f)
                return Vector2Int.zero;

            // Get dominant axis
            float absX = Mathf.Abs(direction.x);
            float absY = Mathf.Abs(direction.y);

            if (!allowDiagonal)
            {
                // 4-directional movement
                if (absX > absY)
                {
                    return new Vector2Int(direction.x > 0 ? 1 : -1, 0);
                }
                else
                {
                    return new Vector2Int(0, direction.y > 0 ? 1 : -1);
                }
            }
            else
            {
                // 8-directional movement
                int x = 0, y = 0;
                
                if (absX > 0.3f)
                    x = direction.x > 0 ? 1 : -1;
                
                if (absY > 0.3f)
                    y = direction.y > 0 ? 1 : -1;

                return new Vector2Int(x, y);
            }
        }

        /// <summary>
        /// Teleports to a specific grid position.
        /// </summary>
        public void TeleportToGrid(Vector2Int gridPosition)
        {
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
                isMoving = false;
            }

            transform.position = GridToWorld(gridPosition);
            targetPosition = transform.position;
            queuedDirection = Vector3.zero;
        }

        /// <summary>
        /// Gets the world position of a grid coordinate.
        /// </summary>
        public Vector3 GetWorldPosition(Vector2Int gridPosition)
        {
            return GridToWorld(gridPosition);
        }

        /// <summary>
        /// Checks if a grid position is blocked.
        /// </summary>
        public bool IsGridPositionBlocked(Vector2Int gridPosition)
        {
            return IsBlocked(GridToWorld(gridPosition));
        }

        private void OnDrawGizmosSelected()
        {
            // Draw current grid cell
            Gizmos.color = Color.green;
            Vector3 center = transform.position;
            if (Application.isPlaying)
            {
                Vector2Int gridPos = GridPosition;
                center = GridToWorld(gridPos);
            }
            Gizmos.DrawWireCube(center, new Vector3(gridSize, gridSize, 0.1f));

            // Draw target position if moving
            if (isMoving)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(targetPosition, new Vector3(gridSize, gridSize, 0.1f));
            }

            // Draw grid around current position
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            for (int x = -2; x <= 2; x++)
            {
                for (int y = -2; y <= 2; y++)
                {
                    Vector3 gridCenter = center + new Vector3(x * gridSize, y * gridSize, 0);
                    Gizmos.DrawWireCube(gridCenter, new Vector3(gridSize, gridSize, 0.1f));
                }
            }
        }
    }
}

