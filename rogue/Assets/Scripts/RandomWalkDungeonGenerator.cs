using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Random Walk 알고리즘을 사용하여 던전을 생성할 수 있게 해주는 기본 클래스
/// RommFirstDungeonGenerator 클래스의 부모 클래스 역할을 함
/// </summary>
public class RandomWalkDungeonGenerator : AbstractDungeonGenerator
{
    /// <summary>
    /// Random Walk 알고리즘에 필요한 설정값(반복 횟수, 이동 길이 등)을 담고 있는 변수
    /// ScriptableObejct를 사용하여 에디터에서 코드를 수정하지 않고 수치를 쉽게 조정할 수 있다.
    /// </summary>
    [SerializeField] protected RandomWalkSO randomWalkParameters;

    /// <summary>
    /// AbstractDungeonGenerator의 추상 메서드를 구현한 절차적 생성의 진입점
    /// 실제 바닥 위치를 계산하고 타일맵을 초기화한 뒤 바닥과 벽을 그린다.
    /// </summary>
    protected override void RunProceduralGeneration()
    {
        // 1. 지정된 파라미터와 시작 위치를 기반으로 Random Walk 알고리즘을 실행하여 바닥 타일 좌표들을 획득
        HashSet<Vector2Int> floorPositions = RunRandomWalk(randomWalkParameters, startPosition);
        
        // 2. 이전 생성 결과물이 남아있을 수도 있으니 타일맵을 Clear함
        tilemapVisualizer.Clear();
        
        // 3. 계산된 바닥 좌표들에 실제 바닥 타일을 그림
        tilemapVisualizer.PaintFloorTiles(floorPositions);
        
        // 4. 바닥의 외곽선을 계산하여 벽 타일을 생성
        WallGenerator.CreateWalls(floorPositions, tilemapVisualizer);
    }

    /// <summary>
    /// Random Walk 알고리즘을 여러 번 반복해 겹치는 좌표들을 하나의 방(바닥) 덩어리로 만든다. 
    /// </summary>
    /// <param name="parameters">반복 횟수와 걷는 길이를 포함한 SO 매개변수</param>
    /// <param name="position">무작위 행보를 시작할 초기 기준 좌표</param>
    /// <returns>생성된 모든 바닥의 중복없는 타일 좌표의 집합(HashSet)</returns>
    private HashSet<Vector2Int> RunRandomWalk(RandomWalkSO parameters, Vector2Int position)
    {
        var currentPosition = position;
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();

        // 매개변수에 설정된 횟수 iterations만큼 무작위 행보를 반복
        for (int i = 0; i < parameters.iterations; i++)
        {
            // 한 번의 무작위 행보(선 긋기)를 통해 경로 좌표들을 가져옴
            var path = ProceduralGenerationAlgorithms.SimpleRandomWalk(currentPosition, parameters.walkLength);
            
            // 가져온 경로를 전체 바닥 좌표 집합에 합침 (UnionWith은 중복을 자동으로 무시 처리할 수 있음)
            floorPositions.UnionWith(path);
        }
        
        return floorPositions;
    }
}
