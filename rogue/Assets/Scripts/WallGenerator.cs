using System.Collections.Generic;
using UnityEngine;

public static class WallGenerator
{
    public static void CreateWalls(HashSet<Vector2Int> floorPositions, TilemapVisualizer tilemapVisualizer)
    {
        var basicWallPosition = FindWallsInDirections(floorPositions, Direction2D.CardinalDirectionsList);
        var cornerWallPosition = FindWallsInDirections(floorPositions, Direction2D.DiagonalDirectionsList);

        CreateBasicWall(tilemapVisualizer, basicWallPosition, floorPositions);
        CreateCornerWall(tilemapVisualizer, cornerWallPosition, floorPositions);
    }

    private static void CreateCornerWall(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> basicWallPosition, HashSet<Vector2Int> floorPositions)
    {
        foreach (var position in basicWallPosition)
        {
            string neighboursBinaryType = "";

            foreach (var direction in Direction2D.EightDiagonalDirectionsList)
            {
                var neighbourPosition = position + direction;

                if (floorPositions.Contains(neighbourPosition))
                {
                    neighboursBinaryType += "1";
                }
                else
                {
                    neighboursBinaryType += "0";
                }
            }

            tilemapVisualizer.PaintSingleCornerWall(position, neighboursBinaryType);
        }
    }

    private static void CreateBasicWall(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> basicWallPosition, HashSet<Vector2Int> floorPositions)
    {
        foreach (var position in basicWallPosition)
        {
            string neighboursBinaryType = "";

            foreach (var direction in Direction2D.CardinalDirectionsList)
            {
                var neighbourPosition = position + direction;

                if (floorPositions.Contains(neighbourPosition))
                {
                    neighboursBinaryType += "1";
                }
                else
                {
                    neighboursBinaryType += "0";
                }
            }
            
            tilemapVisualizer.PaintSingleBasicWall(position, neighboursBinaryType);
        }
    }

    private static HashSet<Vector2Int> FindWallsInDirections(HashSet<Vector2Int> floorPositions, List<Vector2Int> directionsList)
    {
        HashSet<Vector2Int> wallPosition = new HashSet<Vector2Int>();

        foreach (var position in floorPositions)
        {
            foreach (var direction in directionsList)
            {
                var neighbourPosition = position + direction;

                if (!floorPositions.Contains(neighbourPosition))
                {
                    wallPosition.Add(neighbourPosition);
                }
            }
        }
        
        return wallPosition;
    }
}
