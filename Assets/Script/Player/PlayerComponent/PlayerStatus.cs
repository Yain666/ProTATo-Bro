using UnityEngine;

namespace Script.Player.PlayerComponent
{
    public class PlayerStatus : CharacterStatus
    {
        private PlayerController _player;

        // 每个玩家实体，身上自带一个道具库组件
        public PropInventory Inventory { get; private set; }

        public void Initialize(PlayerController player)
        {
            _player = player;
            
            // 初始化玩家自己的道具库，并把当前状态系统注入进去
            Inventory = new PropInventory(this);
        }

        protected override void OnDeath()
        {
            if (_player != null)
            {
                _player.Die();
            }
        }

        /// <summary>
        /// 【安全应用接口】只给 PropInventory 调用的内部方法
        /// </summary>
        internal void AlterPropModifiers(PropData prop, int count, bool isAdding)
        {
            if (prop == null || prop.PropertyModifiers == null) return;

            foreach (var modifier in prop.PropertyModifiers)
            {
                PropertyType type = modifier.Key;
                // 判断是穿戴还是脱下道具（或者是买入/卖出）
                float factor = isAdding ? 1f : -1f;
                float totalValue = modifier.Value * count * factor; 
                ModifyAttribute(type, totalValue);
            }
            
            string opName = isAdding ? "附加" : "剥离";
            Debug.Log($"<color=#00FF00>[属性系统]</color> 成功{opName}道具加成: {prop.name} x{count}");
        }
        
    }
}