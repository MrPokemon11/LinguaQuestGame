using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace World1BossFight
{
    public struct PlatformData
    {
        public bool IsValid;
        
        public Vector3 StartPosition;
        public List<Vector2Int> Positions;
        public float TileSize;
    }
    
    public class PlatformManager : MonoBehaviour
    {
        public static PlatformManager Instance { get; private set; }
        
        [SerializeField] private Vector2Int gridSize;
        [SerializeField] private float tileSize;

        private bool[][] _reservedTiles;
        
        private List<Vector2Int> _reservedPositions;
        private List<Vector2Int> _positions;
        
        private void OnEnable()
        {
            Instance = this;
        }

        private void Awake()
        {
            _reservedPositions = new List<Vector2Int>();
            _positions = new List<Vector2Int>();
            
            for (var x = 0; x < gridSize.x; x++)
            {
                for (var y = 0; y < gridSize.y; y++)
                {
                    _positions.Add(new Vector2Int(x, y));
                }
            }
        }

        public PlatformData FindAndReservePositions(Vector2Int bounds)
        {
            var unvisitedPositions = new List<Vector2Int>();
            var positions = new List<Vector2Int>();
            
            unvisitedPositions.AddRange(_positions);
            var invalidPosition = false;

            while (unvisitedPositions.Count > 0)
            {
                positions.Clear();
                var index = Random.Range(0, unvisitedPositions.Count);
                var position = unvisitedPositions[index];
                var offsetBounds = unvisitedPositions[index] + bounds;
                unvisitedPositions.Remove(position);
                
                if (offsetBounds.x >= gridSize.x || offsetBounds.y >= gridSize.y) continue;
                
                invalidPosition = false;
                for (var x = 0; x < bounds.x; x++)
                {
                    for (var y = 0; y < bounds.y; y++)
                    {
                        var offsetPosition = new Vector2Int(x, y) + position;
                        positions.Add(offsetPosition);
                        invalidPosition = _reservedPositions.Contains(offsetPosition);
                        if (invalidPosition) break;
                    }
                    if (invalidPosition) break;
                }
                
                if (!invalidPosition) break;
            }

            if (invalidPosition) return new PlatformData { IsValid = false };
            
            ReservePositions(positions);
            return new PlatformData { IsValid = true, StartPosition = (Vector2)positions[0] * tileSize + (Vector2)transform.position - (Vector2)transform.localScale / 2f, Positions = positions, TileSize = tileSize };
        }

        private void ReservePositions(List<Vector2Int> positions)
        {
            //Debug.Log("Reserved Positions");
            foreach (var position in positions)
            {
                //Debug.Log("\t" + position);
                _positions.Remove(position);
                _reservedPositions.Add(position);
            }
        }

        public void UnreservePositions(List<Vector2Int> positions)
        {
            foreach (var position in positions)
            {
                _reservedPositions.Remove(position);
                _positions.Add(position);
            }
        }
    }
}
