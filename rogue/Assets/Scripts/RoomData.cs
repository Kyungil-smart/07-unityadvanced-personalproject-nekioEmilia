using UnityEngine;

public enum RoomType { StartRoom, MonsterRoom, ClearRoom} // 차후 ShopRoom, BoosRoom 추가

public class RoomData
{
    public BoundsInt bounds;
    public RoomType roomType;
    public Vector2Int centerPos;

    public RoomData(BoundsInt bounds)
    {
        this.bounds = bounds;
        this.roomType = RoomType.MonsterRoom;
        this.centerPos = new Vector2Int(Mathf.RoundToInt(bounds.center.x), Mathf.RoundToInt(bounds.center.y));
    }
}
