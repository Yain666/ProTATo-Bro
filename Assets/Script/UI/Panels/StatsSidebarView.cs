using System.Collections.Generic;
using Script.Player.PlayerComponent;
using UnityEngine;

public class StatsSidebarView : MonoBehaviour
{
    public Transform primaryContainer;
    public Transform secondaryContainer;
    public GameObject rowPrefab;

    private readonly List<StatRowView> _rows = new List<StatRowView>();
    private bool _initialized;

    public void Refresh(PlayerStatus playerStatus)
    {
        EnsureRows();

        for (int i = 0; i < _rows.Count; i++)
        {
            _rows[i].Refresh(playerStatus);
        }
    }

    private void EnsureRows()
    {
        if (_initialized)
        {
            return;
        }

        LevelUpgradeConfigDataController.Initialize();
        IReadOnlyList<LevelUpgradeConfigData> configs = LevelUpgradeConfigDataController.Instance.GetAllConfigs();
        HashSet<int> usedPropertyIds = new HashSet<int>();

        for (int i = 0; i < configs.Count; i++)
        {
            LevelUpgradeConfigData config = configs[i];
            if (config == null || usedPropertyIds.Contains(config.propertyId))
            {
                continue;
            }

            usedPropertyIds.Add(config.propertyId);
            Transform parent = config.isPrimary ? primaryContainer : secondaryContainer;
            if (parent == null || rowPrefab == null)
            {
                continue;
            }

            GameObject rowObject = Instantiate(rowPrefab, parent);
            rowObject.SetActive(true);
            StatRowView rowView = rowObject.GetComponent<StatRowView>();
            if (rowView == null)
            {
                rowView = rowObject.AddComponent<StatRowView>();
            }

            rowView.Bind(config);
            _rows.Add(rowView);
        }

        _initialized = true;
    }
}
