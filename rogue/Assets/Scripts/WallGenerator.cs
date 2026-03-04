using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 바닥 타일들의 가장자리를 탐색해서 알맞은 형태의 벽 타일 좌표를 계산하고 생성해주는 클래스
/// </summary>
public static class WallGenerator
{
    /// <summary>
    /// 바닥 좌표를 기반으로 기본 벽(상하좌우)과 코너 벽(대각선)의 위치를 찾아내어 타일맵에 그리는 메서드
    /// </summary>
    /// <param name="floorPositions">생성된 모든 바닥 타일의 좌표 집합</param>
    /// <param name="tilemapVisualizer">실제 타일을 화면에 그려주는 시각화 클래스</param>
    public static void CreateWalls(HashSet<Vector2Int> floorPositions, TilemapVisualizer tilemapVisualizer)
    {
        // 1. 바닥 타일 기준으로 상하좌우를 탐색해 기본 벽이 들어갈 위치를 찾음
        var basicWallPosition = FindWallsInDirections(floorPositions, Direction2D.CardinalDirectionsList);
        // 2. 바닥 타일 기준으로 대각선을 탐색해 코너(모서리) 벽이 들어갈 위치를 찾음
        var cornerWallPosition = FindWallsInDirections(floorPositions, Direction2D.DiagonalDirectionsList);

        // 3. 찾은 위치에 비트마스킹을 적용하여 알맞은 형태의 타일을 그림
        CreateBasicWall(tilemapVisualizer, basicWallPosition, floorPositions);
        CreateCornerWall(tilemapVisualizer, cornerWallPosition, floorPositions);
    }

    /// <summary>
    /// 코너 벽이 배치될 위치 주변의 8방향을 검사해서 상황에 맞는 "모서리 타일"을 그려주는 메서드
    /// </summary>
    private static void CreateCornerWall(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> basicWallPosition, HashSet<Vector2Int> floorPositions)
    {
        foreach (var position in basicWallPosition)
        {
            // 주변 바닥 유무를 저장할 8자리 이진수 문자열
            string neighboursBinaryType = "";

            // 주변 8방향(상하좌우+대각선)을 모두 검사
            foreach (var direction in Direction2D.EightDiagonalDirectionsList)
            {
                var neighbourPosition = position + direction;

                // 해당 방향에 바닥이 있으면 "1", 없으면 "0"을 대입
                if (floorPositions.Contains(neighbourPosition))
                {
                    neighboursBinaryType += "1";
                }
                else
                {
                    neighboursBinaryType += "0";
                }
            }

            // 완성된 8자리 이진수를 넘겨 알맞은 코너 타일을 그려줌
            tilemapVisualizer.PaintSingleCornerWall(position, neighboursBinaryType);
        }
    }

    /// <summary>
    /// 기본 벽이배치될 위치 주변의 4방향을 검사해서 상황에 맞는 "평면"벽 타일을 그려주는 메서드
    /// </summary>
    private static void CreateBasicWall(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> basicWallPosition, HashSet<Vector2Int> floorPositions)
    {
        foreach (var position in basicWallPosition)
        {
            // 주변 바닥 유무를 저장할 4자리 이진수 문자열
            string neighboursBinaryType = "";

            // 주변 4방향만 검사(상하좌우)
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

    /// <summary>
    /// 지정된 방향 리스트를 순회하여, 바닥의 가장자리를 찾아 벽이 세워질 좌표를 반환해주는 메서드
    /// </summary>
    /// <param name="floorPositions">현재 바닥 타일 좌표들</param>
    /// <param name="directionsList">탐색할 방향 리스트</param>
    /// <returns>벽이 세워져야 할 좌표들의 집합</returns>
    private static HashSet<Vector2Int> FindWallsInDirections(HashSet<Vector2Int> floorPositions, List<Vector2Int> directionsList)
    {
        HashSet<Vector2Int> wallPosition = new HashSet<Vector2Int>();

        // 모든 바닥 타일에 대해 반복
        foreach (var position in floorPositions)
        {
            // 주어진 방향으로 한 칸씩 확인
            foreach (var direction in directionsList)
            {
                var neighbourPosition = position + direction;

                // 내 옆칸이 바닥이 아니면 가장자리이므로 벽 좌표를 추가
                if (!floorPositions.Contains(neighbourPosition))
                {
                    wallPosition.Add(neighbourPosition);
                }
            }
        }
        
        return wallPosition;
    }
}
