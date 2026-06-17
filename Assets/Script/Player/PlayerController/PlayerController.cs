using System.Collections;
using System.Collections.Generic;
using Script.Player.PlayerComponent;
using UnityEngine;

public class PlayerController : MonoBehaviour ,IDamageable
{
    #region --- properties ---

    public int icon;
    public PlayerStatus status;
    
    #endregion --- properties ---

    private void Awake()
    {
        if (status == null)
        {
            status = GetComponent<PlayerStatus>();
        }

        if (status != null)
        {
            status.Initialize(this);
        }
    }
    
    
    public void OnPickUp(ItemData data, int count)
    {
        // 处理一下金币增长还有捡到的道具的效果, 这里是DEMO版本的捡到金币的效果
        if (data.type == ItemType.Currency)
        {
            if(icon < 9999999)
                icon += count;
            //TODO:更新UI
        }
    }
    
    public void TakeDamage(float amount)
    {
        throw new System.NotImplementedException();
    }

    public void Die()
    {
        // PlayDeathSFX();
        Debug.Log("Player 死啦！！");
    }
    
}
