using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomFirstDungeonGenerator : RandomWalkDungeonGenerator
{
    [SerializeField] private int minRoomWidth = 4, minRoomHeight = 4;
    [SerializeField] private int dungeonWidth = 20, dungeonHeight = 20;
    
    [Header("스폰 프리팹")]
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject monsterPrefab;
    [SerializeField] GameObject clearPortalPrefab;
    
    // offset 값이 커지면 방 크기가 작아지고 방 사이의 간격이 넓어짐
    [SerializeField] [Range(0, 5)] private int offset = 1;

    private void Start()
    {
        tilemapVisualizer.Clear();
        RunProceduralGeneration();
    }

    protected override void RunProceduralGeneration()
    {
        CreateRooms();
    }

    /// <summary>
    /// BSP 알고리즘을 사용하여 전체 던전 공간을 쪼개고, 방을 생성한 뒤 복도로 연결하는 메인 로직
    /// </summary>
    private void CreateRooms()
    {
        // 1. BSP 알고리즘으로 던전 전체 크기를 최소 방 크기에 맞춰 여러 개의 구역(BoundsInt)으로 나눔
        var roomsList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(new BoundsInt((Vector3Int)startPosition,
            new Vector3Int(dungeonWidth, dungeonHeight, 0)), minRoomWidth, minRoomHeight);

        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        // 2. 나뉜 구역 정보를 바탕으로 실제 방의 바닥 타일 좌표들을 생성
        floor = CreateRoom(roomsList);
        // floor = CreateRoomsRandomly(roomsList);
        // -> 두 번째 방식이 나중에 셀룰러 오토마타 알고리즘을 추가해 고점 노려볼 수 있음
        
        // 3. 각 방의 중심점 좌표를 저장할 리스트 생성 (복도 연결용)
        List<Vector2Int> roomCenters = new List<Vector2Int>();

        foreach (var room in roomsList)
        {
            // BoundsInt의 center값을 RoundToInt메서드를 사용해 반올림하여 Vector2Int로 변환 후 저장
            roomCenters.Add((Vector2Int)Vector3Int.RoundToInt(room.center));
        }

        // 4. 방의 중심점들을 서로 연결하여 복도 좌표들을 생성
        HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);
        
        // 5. 방 바닥 좌표와 복도 바닥 좌표를 하나로 함침
        floor.UnionWith(corridors);

        // 6. 타일맵에 바닥을 그리고, 바닥의 외곽선을 따라 벽을 생성
        tilemapVisualizer.PaintFloorTiles(floor);
        WallGenerator.CreateWalls(floor, tilemapVisualizer);

        var roomsDataList = RoomTypes(roomsList);
        SpawnPlayer(roomsDataList[0].centerPos);
        SpawnMonster(roomsDataList);
        ClearPortal(roomsDataList);
        
        Debug.Log($"총 방의 개수: {roomsDataList.Count}");
        Debug.Log($"시작 방 좌표: {roomsDataList[0].centerPos}");
    }

    private void SpawnPlayer(Vector2Int startPosition)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Instantiate(playerPrefab, (Vector2)startPosition, Quaternion.identity);
            Debug.Log("플레이어 스폰 완료");
        }
        else
        {
            player.transform.position = (Vector2)startPosition;
        }
    }

    private void SpawnMonster(List<RoomData> roomDataList)
    {
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");

        foreach (var monster in monsters)
        {
            if (monster != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(monster);
                }
                else
                {
                    DestroyImmediate(monster);
                }
            }
        }
        
        
        foreach (var room in roomDataList)
        {
            if (room.roomType == RoomType.MonsterRoom)
            {
                if (monsterPrefab != null)
                {
                    Instantiate(monsterPrefab, (Vector2)room.centerPos, Quaternion.identity);
                }
            }
        }
    }
    
    

    /// <summary>
    /// 모든 방의 중심점을 가장 가까운 방끼리 차례대로 연결하여 복도를 생성
    /// </summary>
    /// <param name="roomCenters">모든 방의 중심점 리스트</param>
    /// <returns>생성된 복도의 모든 타일 좌표</returns>
    private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
    {
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();

        // 임의의 방 하나를 시작점으로 선택(Random)하고 리스트에서 제거
        var currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(currentRoomCenter);

        // 리스트에 남은 방이 없을 때까지 반복
        while (roomCenters.Count > 0)
        {
            // 현재 방에서 가장 가까운 방의 중심점을 찾음
            Vector2Int closest = FindClosestPointTo(currentRoomCenter, roomCenters);
            roomCenters.Remove(closest);

            // 현재 방과 가장 가까운 방을 잇는 L자형 복도 생성
            HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest);
            
            // 다음 연결을 위해 현재 위치를 갱신하고 복도 좌표를 누적
            currentRoomCenter = closest;
            corridors.UnionWith(newCorridor);
        }

        return corridors;
    }
    
    private List<RoomData> RoomTypes(List<BoundsInt> roomsList)
    {
        List<RoomData> roomDataList = new List<RoomData>();

        foreach (var bounds in roomsList)
        {
            roomDataList.Add(new RoomData(bounds));
        }

        roomDataList[0].roomType = RoomType.StartRoom;
        Vector2Int startCenter = roomDataList[0].centerPos;

        float maxDistance = 0f;
        RoomData furthestRoom = null;

        for (int i = 1; i < roomDataList.Count; i++)
        {
            float distance = Vector2.Distance(startCenter, roomDataList[i].centerPos);

            if (distance > maxDistance)
            {
                maxDistance = distance;
                furthestRoom = roomDataList[i];
            }
        }

        if (furthestRoom != null)
        {
            furthestRoom.roomType = RoomType.ClearRoom;
        }
        
        return roomDataList;
    }

    /// <summary>
    /// 두 지점을 연결하는 L자형 복도(경로)를 생성, Y축으로 먼저 이동한 후 X축으로 이동
    /// </summary>
    /// <param name="currentRoomCenter"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    private HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
    {
        HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();
        var position = currentRoomCenter;
        corridor.Add(position);

        // 1. Y축 맞추기 - 목표 지점의 Y 좌표에 도달할 때까지 위 또는 아래로 한 칸씩 이동
        while (position.y != destination.y)
        {
            if (destination.y > position.y)
            {
                position += Vector2Int.up;
            }
            else if (destination.y < position.y)
            {
                position += Vector2Int.down;
            }

            corridor.Add(position);
        }

        // 2. X축 맞추기 - 목표 지점의 X 좌표에 도달할 때까지 오른쪽 또는 왼쪽으로 한 칸씩 이동
        while (position.x != destination.x)
        {
            if (destination.x > position.x)
            {
                position += Vector2Int.right;
            }
            else if (destination.x < position.x)
            {
                position += Vector2Int.left;
            }

            corridor.Add(position);
        }

        return corridor;
    }

    /// <summary>
    /// 주어진 기준점에서 가장 가까운 위치에 있는 좌표를 탐색
    /// </summary>
    /// <param name="currentRoomCenter">기준이 되는 현재 방의 중심점</param>
    /// <param name="roomCenters">비교할 대상 방들의 중심점 리스트</param>
    /// <returns>가장 가까운 방의 중심점 좌표</returns>
    private Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
    {
        Vector2Int closest = Vector2Int.zero;
        float distance = float.MaxValue;

        foreach (var position in roomCenters)
        {
            // Vector2.Distance 메서드를 사용해 두 점 사이의 직선거리를 계산
            float currentDistance = Vector2.Distance(position, currentRoomCenter);

            // 기존에 찾은 최소 거리보다 더 가까우면 값 갱신
            if (currentDistance < distance)
            {
                distance = currentDistance;
                closest = position;
            }
        }

        return closest;
    }

    /// <summary>
    /// BSP로 나뉜 구역(BoundsInt) 정보에 지정된 여백(offset)을 적용하여 실제 네모난 방의 바닥 좌표를 생성
    /// </summary>
    /// <param name="roomsList">BSP 알고리즘으로 분할된 구역 리스트</param>
    /// <returns>생성된 모든 방의 바닥 타일 좌표</returns>
    private HashSet<Vector2Int> CreateRoom(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        foreach (var room in roomsList)
        {
            // 방의 테두리에 offset만큼 여백을 주기 위해 반복문 범위를 축소
            for (int col = offset; col < room.size.x - offset; col++)
            {
                for (int row = offset; row < room.size.y - offset; row++)
                {
                    // 구역의 시작점(min)을 기준으로 col, row만큼 더해 실제 타일 위치 계산
                    Vector2Int position = (Vector2Int)room.min + new Vector2Int(col, row);
                    floor.Add(position);
                }
            }
        }

        return floor;
    }

    private void ClearPortal(List<RoomData> roomDataList)
    {
        GameObject clearPortal = GameObject.FindGameObjectWithTag("ClearPortal");

        if (clearPortal != null)
        {
            if (Application.isPlaying)
            {
                Destroy(clearPortal);
            }
            else
            {
                DestroyImmediate(clearPortal);
            }
        }
        
        foreach (var room in roomDataList)
        {
            if (room.roomType == RoomType.ClearRoom)
            {
                if (clearPortalPrefab != null)
                {
                    Instantiate(clearPortalPrefab, (Vector2)room.centerPos, Quaternion.identity);
                    Debug.Log("포탈 배치");

                    break;
                }
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("게임 클리어");
        }
    }


    /*
    private HashSet<Vector2Int> CreateRoomsRandomly(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        for (int i = 0; i < roomsList.Count; i++)
        {
            var roomBounds = roomsList[i];
            var roomCenter = new Vector2Int(Mathf.RoundToInt(roomBounds.center.x), Mathf.RoundToInt(roomBounds.center.y));
            var roomFloor = RunRandomWalk(randomWalkParameters, roomCenter);

            foreach (var position in roomFloor)
            {
                if (position.x >= (roomBounds.xMin + offset) && position.x <= (roomBounds.xMax - offset) &&
                    position.y >= (roomBounds.yMin - offset) && position.y <= (roomBounds.yMax - offset))
                {
                    floor.Add(position);
                }
            }
        }

        return floor;
    }
    */
}