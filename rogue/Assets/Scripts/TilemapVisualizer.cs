using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// PCG 알고리즘으로 계산된 좌표 데이터를 받아서 실제 유니티 타일맵에 타일을 그려주는 시각화 담당 클래스
/// </summary>
public class TilemapVisualizer : MonoBehaviour
{
    [Header("Grid Tilemap")]
    [SerializeField]
    private Tilemap floorTilemap, wallTilemap;

    [Header("기본 타일 스프라이트")]
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase wallTop;
    [SerializeField] private TileBase wallSideRight;
    [SerializeField] private TileBase wallSideLeft;
    [SerializeField] private TileBase wallBottom;
    [SerializeField] private TileBase wallFull;
    
    [Header("모서리용 타일 스프라이트")]
    [SerializeField] private TileBase wallInnerCornerDownLeft;
    [SerializeField] private TileBase wallInnerCornerDownRight;
    [SerializeField] private TileBase wallDiagonalCornerDownRight;
    [SerializeField] private TileBase wallDiagonalCornerDownLeft;
    [SerializeField] private TileBase wallDiagonalCornerUpRight;
    [SerializeField] private TileBase wallDiagonalCornerUpLeft;

    /// <summary>
    /// 전달받은 모든 바닥 좌표에 바닥 타일을 칠한다.
    /// </summary>
    /// <param name="floorPositions">바닥이 될 좌표들의 집합</param>
    public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions)
    {
        PaintTiles(floorPositions, floorTilemap, floorTile);
    }

    /// <summary>
    /// 특정 타일맵의 여러 좌표에 동일한 타입을 반복해서 칠하는 메서드
    /// </summary>
    private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase tile)
    {
        foreach (var position in positions)
        {
            PaintSingleTile(tilemap, tile, position);
        }
    }

    /// <summary>
    /// 단일 좌표(Vector2Int)를 타일맵의 셀 좌표계(Vector3Int)로 변환하여 타일을 배치하는 메서드
    /// </summary>
    /// <param name="tilemap"></param>
    /// <param name="tile"></param>
    /// <param name="position"></param>
    private void PaintSingleTile(Tilemap tilemap, TileBase tile, Vector2Int position)
    {
        // 2D 좌표를 Tilemap이 이해할 수 있는 3D 그리드 좌표로 변환
        var tilePosition = tilemap.WorldToCell((Vector3Int)position);
        tilemap.SetTile(tilePosition, tile);
    }

    /// <summary>
    /// 주변 바닥 배치 상태(2진수 문자열)를 분석해서 알맞은 방향의 기본 벽 타일을 칠하는 메서드
    /// </summary>
    /// <param name="position">벽을 칠할 좌표</param>
    /// <param name="binaryType">주변 바닥 유무를 나타내는 2진수 문자열</param>
    public void PaintSingleBasicWall(Vector2Int position, string binaryType)
    {
        var typeAsInt = Convert.ToInt32(binaryType, 2);
        TileBase tile = null;

        // WallTypeHelper에 미리 정의된 10진수 값 목록과 비교해서 어떤 타일인지 결정해주는 구문
        if (WallTypesHelper.wallTop.Contains(typeAsInt))
        {
            tile = wallTop;
        }
        else if (WallTypesHelper.wallSideRight.Contains(typeAsInt))
        {
            tile = wallSideRight;
        }
        else if (WallTypesHelper.wallSideLeft.Contains(typeAsInt))
        {
            tile = wallSideLeft;
        }
        else if (WallTypesHelper.wallBottom.Contains(typeAsInt))
        {
            tile = wallBottom;
        }
        else if (WallTypesHelper.wallFull.Contains(typeAsInt))
        {
            tile = wallFull;
        }
        
        // 모두 다 돌고 일치하는 타일이 있다면 칠하기
        if (tile != null)
            PaintSingleTile(wallTilemap, tile, position);
    }

    /// <summary>
    /// 맵 생성을 다시할 때 기존에 그려져 있던 타일맵의 모든 타일들을 지워 초기화해주는 메서드
    /// </summary>
    public void Clear()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
    }

    /// <summary>
    /// 대각선 및 모서리 주변 상황(8방향)을 분석해서 알맞은 타일을 칠해주는 메서드
    /// </summary>
    /// <param name="position"></param>
    /// <param name="binaryType"></param>
    public void PaintSingleCornerWall(Vector2Int position, string binaryType)
    {
        int typeAsInt = Convert.ToInt32(binaryType, 2);
        TileBase tile = null;

        // 내용은 PaintSingleBasicWall의 if문과 같음
        if (WallTypesHelper.wallInnerCornerDownLeft.Contains(typeAsInt))
        {
            tile = wallInnerCornerDownLeft;
        }
        else if (WallTypesHelper.wallInnerCornerDownRight.Contains(typeAsInt))
        {
            tile = wallInnerCornerDownRight;
        }
        else if (WallTypesHelper.wallDiagonalCornerDownLeft.Contains(typeAsInt))
        {
            tile = wallDiagonalCornerDownLeft;
        }
        else if (WallTypesHelper.wallDiagonalCornerDownRight.Contains(typeAsInt))
        {
            tile = wallDiagonalCornerDownRight;
        }
        else if (WallTypesHelper.wallDiagonalCornerUpLeft.Contains(typeAsInt))
        {
            tile = wallDiagonalCornerUpLeft;
        }
        else if (WallTypesHelper.wallDiagonalCornerUpRight.Contains(typeAsInt))
        {
            tile = wallDiagonalCornerUpRight;
        }
        else if (WallTypesHelper.wallFullEightDirections.Contains(typeAsInt))
        {
            tile = wallFull;
        }
        else if (WallTypesHelper.wallBottomEightDirections.Contains(typeAsInt))
        {
            tile = wallBottom;
        }
        
        if (tile != null)
            PaintSingleTile(wallTilemap, tile, position);
        
    }
}
