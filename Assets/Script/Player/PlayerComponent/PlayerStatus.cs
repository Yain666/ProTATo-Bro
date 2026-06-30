using UnityEngine;
using System.Collections.Generic;

namespace Script.Player.PlayerComponent
{
    public class PlayerStatus : CharacterStatus
    {
        private PlayerController _player;
        private PlayerDamageFlash _damageFlash;

        // 每个玩家实体，身上自带一个道具库组件
        public PropInventory Inventory { get; private set; }

        public void Initialize(PlayerController player)
        {
            _player = player;
            _damageFlash = GetComponent<PlayerDamageFlash>();
            if (_damageFlash == null)
            {
                _damageFlash = gameObject.AddComponent<PlayerDamageFlash>();
            }

            PlayerMovement movement = GetComponent<PlayerMovement>();
            if (movement != null)
            {
                PlayerVisualController visualController = movement.GetComponent<PlayerVisualController>();
                if (visualController == null)
                {
                    visualController = movement.GetComponentInParent<PlayerVisualController>();
                }

                _damageFlash.targetRenderer = visualController != null ? visualController.PrimaryRenderer : movement.spriteRenderer;
            }
            
            // 初始化玩家自己的道具库，并把当前状态系统注入进去
            Inventory = new PropInventory(this);
        }

        public override int TakeDamage(int incomingDamage, string whoTakeDamage)
        {
            int finalDamage = base.TakeDamage(incomingDamage, whoTakeDamage);
            if (finalDamage > 0)
            {
                if (_damageFlash == null)
                {
                    _damageFlash = GetComponent<PlayerDamageFlash>();
                }

                _damageFlash?.PlayFlash();
            }

            return finalDamage;
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
            //Debug.Log($"<color=#00FF00>[属性系统]</color> 成功{opName}道具加成: {prop.name} x{count}");
        }

        public void ApplyCharacterData(CharacterData characterData)
        {
            if (characterData == null) return;

            List<int> attrIds = characterData.attrIds != null
                ? new List<int>(characterData.attrIds)
                : new List<int>();

            List<float> attrValues = new List<float>();
            if (characterData.attrData != null)
            {
                for (int i = 0; i < characterData.attrData.Length; i++)
                {
                    attrValues.Add(characterData.attrData[i]);
                }
            }

            InitStatus(attrIds, attrValues);
            //Debug.Log($"[PlayerStatus] 已应用角色数据: {characterData.characterName} ({characterData.id})");

            PlayerController playerController = GetComponent<PlayerController>();
            playerController?.ApplyCharacterVisual(characterData);
        }
        
    }
}
