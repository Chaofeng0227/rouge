using UnityEngine;

public class Room : MonoBehaviour
{
    // 在 Inspector 面板里勾选这个房间有哪些出口
    public bool hasTop, hasBottom, hasLeft, hasRight;

    [HideInInspector]
    public Vector2Int gridPos; // 记录在网格中的坐标 (0,0), (0,1) 等
}
