using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


/// <summary>
/// 절차적 던전 생성에 필요한 알고리즘을 모아둔 Static 클래스
/// </summary>
public static class ProceduralGenerationAlgorithms
{
    /// <summary>
    /// Random Walk 알고리즘
    /// startPosition에서 시작해서 walkLength만큼 상하좌우 중 무작위방향으로 1칸씩 이동해서 경로를 생성하는 메서드
    /// </summary>
    /// <param name="startPosition">초기 좌표</param>
    /// <param name="walkLength">이동할 총 횟수</param>
    /// <returns>생성된 경로의 모든 좌표 집합 / HashSet으로 중복 제거</returns>
    public static HashSet<Vector2Int> SimpleRandomWalk(Vector2Int startPosition, int walkLength)
    {
        HashSet<Vector2Int> path = new HashSet<Vector2Int>();

        path.Add(startPosition);
        var previousPosition = startPosition;

        // 지정된 길이만큼 반복하여 한 칸씩 이동
        for (int i = 0; i < walkLength; i++)
        {
            var newPosition = previousPosition + Direction2D.GetRandomCardinalDirection();
            path.Add(newPosition);
            previousPosition = newPosition;
        }

        return path;
    }

    /*
    public static List<Vector2Int> RandomWalkCorridor(Vector2Int startPosition, int corridorLength)
    {
        List<Vector2Int> corridor = new List<Vector2Int>();
        var direction = Direction2D.GetRandomCardinalDirection();
        var currentPosition = startPosition;
        corridor.Add(currentPosition);

        for (int i = 0; i < corridorLength; i++)
        {
            currentPosition += direction;
            corridor.Add(currentPosition);
        }

        return corridor;
    }
    */

    /// <summary>
    /// BSP 알고리즘
    /// 거대한 하나의 공간을 최소 크기 조건에 도달할 때까지 가로 / 세로로 계속해서 반씩 쪼개는 메서드
    /// </summary>
    /// <param name="spaceToSplit">분할을 시작할 전체 공간 크기</param>
    /// <param name="minWidth">방이 될 수 있는 최소 넓이</param>
    /// <param name="minHeight">방이 될 수 있는 최소 높이</param>
    /// <returns>최종적으로 쪼갤 수 없는 크기까지 분할된 방의 리스트</returns>
    public static List<BoundsInt> BinarySpacePartitioning(BoundsInt spaceToSplit, int minWidth, int minHeight)
    {
        Queue<BoundsInt> roomsQueue = new Queue<BoundsInt>();
        List<BoundsInt> roomsList = new List<BoundsInt>();
        
        // 1. 쪼개야할 전체 공간을 큐에 넣고 시작
        roomsQueue.Enqueue(spaceToSplit);

        // 2. 큐에 쪼갤 공간이 남아있는 동안 무한 루프
        while (roomsQueue.Count > 0)
        {
            var room = roomsQueue.Dequeue();

            // 현재 공간이 쪼갤 수 있는 최소한의 크기(minHeight, minWidth)를 만족하는지 확인
            if (room.size.y >= minHeight && room.size.x >= minWidth)
            {
                // 50% 확률로 가로분할 / 세로분할을 시도할지 결정
                if (Random.value < 0.5f)
                {
                    // 공간의 높이가 최소 높이의 2배 이상이면 가로로 쪼개기
                    if (room.size.y >= minHeight * 2)
                    {
                        SplitHorizontally(minHeight, roomsQueue, room);
                    }
                    // 공간의 넓이가 최소 넓이의 2배 이상이면 세로로 쪼개기
                    else if (room.size.x >= minWidth * 2)
                    {
                        SplitVertically(minWidth, roomsQueue, room);
                    }
                    // 더 이상 쪼갤 수 없지만 최소 크기만 만족하면 방 목록에 추가
                    else if (room.size.x >= minWidth && room.size.y >= minHeight)
                    {
                        roomsList.Add(room);
                    }
                }
                // 위와 같음
                else
                {
                    if (room.size.x >= minWidth * 2)
                    {
                        SplitVertically(minWidth, roomsQueue, room);
                    }
                    else if (room.size.y >= minHeight * 2)
                    {
                        SplitHorizontally(minHeight, roomsQueue, room);
                    }
                    else if (room.size.x >= minWidth && room.size.y >= minHeight)
                    {
                        roomsList.Add(room);
                    }
                }
            }
        }

        return roomsList;
    }

    /// <summary>
    /// 공간을 세로로 분할하여 좌/우 두 개의 새로운 공간을 큐에 넣는 메서드
    /// </summary>
    private static void SplitVertically(int minWidth, Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        int xSplit = Random.Range(1, room.size.x);
        BoundsInt room1 = new BoundsInt(room.min, new Vector3Int(xSplit, room.size.y, room.size.z));
        BoundsInt room2 = new BoundsInt(new Vector3Int(room.min.x + xSplit, room.min.y, room.min.z),
            new Vector3Int(room.size.x - xSplit, room.size.y, room.size.z));

        roomsQueue.Enqueue(room1);
        roomsQueue.Enqueue(room2);
    }

    /// <summary>
    /// 공간을 가로로 분할하여 상/하 두 개의 새로운 공간을 큐에 넣는 메서드
    /// </summary>
    private static void SplitHorizontally(int minHeight, Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        int ySplit = Random.Range(1, room.size.y);
        BoundsInt room1 = new BoundsInt(room.min, new Vector3Int(room.size.x, ySplit, room.size.z));
        BoundsInt room2 = new BoundsInt(new Vector3Int(room.min.x, room.min.y + ySplit, room.min.z),
            new Vector3Int(room.size.x, room.size.y - ySplit, room.size.z));

        roomsQueue.Enqueue(room1);
        roomsQueue.Enqueue(room2);
    }
}

/// <summary>
/// 2D Grid 시스템에서 사용하는 방향벡터를 미리 정의해둔 클래스
/// </summary>
public static class Direction2D
{
    /// <summary> 상하좌우 4방향 </summary>
    public static readonly List<Vector2Int> CardinalDirectionsList = new List<Vector2Int>()
    {
        new Vector2Int(0, 1),  // UP
        new Vector2Int(1, 0),  // RIGHT
        new Vector2Int(0, -1), // DOWN
        new Vector2Int(-1, 0)  // LEFT
    };

    /// <summary> 대각선 4방향 </summary>
    public static readonly List<Vector2Int> DiagonalDirectionsList = new List<Vector2Int>()
    {
        new Vector2Int(1, 1),   // UP-RIGHT
        new Vector2Int(1, -1),  // RIGHT-DOWN
        new Vector2Int(-1, -1), // DOWN-LEFT
        new Vector2Int(-1, 1)   // LEFT-UP
    };

    /// <summary> 상하좌우 및 대각선을 포함한 8방향 </summary>
    // 4방향을 나태내는 두 메서드가 있는데 8방향이 있는 이유
    // => 각각 4방향 메서드로만 검사해서는 대각선쪽으로 파고든 빈 공간을 찾아낼 수 없어서
    public static readonly List<Vector2Int> EightDiagonalDirectionsList = new List<Vector2Int>()
    {
        new Vector2Int(0, 1), // UP
        new Vector2Int(1, 1), // UP-RIGHT
        new Vector2Int(1, 0), // RIGHT
        new Vector2Int(1, -1), // RIGHT-DOWN
        new Vector2Int(0, -1), // DOWN
        new Vector2Int(-1, -1), // DOWN-LEFT
        new Vector2Int(-1, 0), // LEFT
        new Vector2Int(-1, 1) // LEFT-UP
    };

    public static Vector2Int GetRandomCardinalDirection()
    {
        return CardinalDirectionsList[Random.Range(0, CardinalDirectionsList.Count)];
    }
}
