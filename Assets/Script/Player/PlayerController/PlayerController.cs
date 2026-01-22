using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region --- properties ---

    public int icon;
    
    #endregion --- properties ---
    
    
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
}
