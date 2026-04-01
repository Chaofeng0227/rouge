using UnityEngine;

public class HealingPotion : MonoBehaviour
{
    [Header("回血量")]
    public int healAmount = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        // 尝试从碰到的物体身上获取 PlayerHealth 组件
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        // 如果获取到了，说明碰我们的就是玩家！
        if (playerHealth != null)
        {
            // 1. 让玩家回血
            playerHealth.Heal(healAmount);

            // 2. 可以在控制台打印一下，方便测试
            Debug.Log("玩家吃掉了血瓶，恢复了 " + healAmount + " 点血！");

            // 3. 销毁血瓶自己
            Destroy(gameObject);
        }
    }
}