using UnityEngine;

public class NextLevelPortal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检查碰撞体是否是玩家
        if (other.CompareTag("Player"))
        {
            Debug.Log("进入传送门，前往下一层！");

            // 找到场景中的生成器
            DungeonGenerator generator = Object.FindFirstObjectByType<DungeonGenerator>();

            if (generator != null)
            {
                // 重新生成地牢
                generator.GenerateDungeon();
            }
        }
    }
}