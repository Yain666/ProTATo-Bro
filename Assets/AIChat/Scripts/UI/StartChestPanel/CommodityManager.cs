using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CommodityManager : MonoBehaviour
{
    #region --- Properties ---

    private int _selectedNum = 1;// 已经选择的物品数
    private int _money = 10000000; //TODO:后面User出来换一下
    
    #endregion --- Properties ---
    
    #region --- Sturctures ---
    
    private Queue<ChestData> _chestData = new Queue<ChestData>();
    private Dictionary<string, Commoditywidget> _commodityPanelDictionary = new Dictionary<string, Commoditywidget>();
    
    #endregion --- Sturctures ---
    
    #region --- Configs ---
    
    public int Level { private get; set; } = 1;
    
    #endregion --- Configs ---
    
    #region --- Component ---
    
    private RectTransform _parentRectTransform;//后面改成Content,这里把父物体位置给他绑定了
    private RectTransform contentRectTransform;
    private StartChestPanel startChestPanel;
    public CycleListChestPanel CycleListChestPanel;
    
    #endregion --- Component ---
    
    private void Start()
    {
        _parentRectTransform = this.gameObject.GetComponent<RectTransform>();
        List<ChestData> chestDatas = UIManager.Instance.GetChestData(Level, 0);
        startChestPanel = gameObject.GetComponentInParent<StartChestPanel>();
        Init_info(chestDatas);
        //_chestData.Dequeue();//TODO:测试专用
        ShowCommodityPanel(_chestData.Peek());
    }

    // 初始化面板数据，把这一次宝箱的所有数据都传进来，由内部进行判断。
    public void Init_info(List<ChestData> chestDatas)
    {
        foreach (var data in chestDatas)
        {
            _chestData.Enqueue(data);
        }
    }

    // 在CycleList出来之后,这里主要干两件事
    // 1. 生成商品ID集，交给CycleList的Data转换成RandomData
    // 2. 将生成Item的数量以及种类, 还有他这个manager传给CycleList，他会帮我们生成Item的
    public void ShowCommodityPanel(ChestData chestData)
    {
        int commodityPanelAmount = chestData.selTolNum;
        CommodityType panelType = (CommodityType)chestData.contentType;
        int requiredAmount = new int();
        List<int> requireIDList = new List<int>();// 指定ID集
        
        switch (panelType)
        {
            case CommodityType.prop: //这里生成出 指定的ID 和 随机的ID集
                requiredAmount = chestData.propSelData.require_ids.Count();
                List<int> propDataIDList = GameManager.Instance.propController.RandowLoadPropCommodity(chestData, commodityPanelAmount - requiredAmount);
                for (int i = 1; i <= commodityPanelAmount; i++)// 这里是按照总的生成数量进行循环
                {
                    if (chestData.propSelData.require_ids.Count >= i)// 这里是按照必出的物品生成
                    {
                        int index = i;
                        requireIDList.Add(chestData.propSelData.require_ids[index]);
                    }
                    else // 这里是按照配置数据中的随机数据获取List
                    {
                        int index = i;
                        requireIDList.Add(propDataIDList[index-1-requiredAmount]);
                    }
                }
                break;
            case CommodityType.weapon:
                requiredAmount = chestData.weaponSelData.require_ids.Count();
                List<int> weaponDataIDList = GameManager.Instance.weaponController.RandowLoadWeaponCommodity(chestData, commodityPanelAmount - requiredAmount);
                
                for (int i = 1; i <= commodityPanelAmount; i++)// 这里是按照总的生成数量进行循环
                {
                    if (chestData.weaponSelData.require_ids.Count >= i)// 这里是按照必出的物品生成
                    {
                        int index = i;
                        requireIDList.Add(chestData.weaponSelData.require_ids[index-1]);
                    }
                    else // 这里是按照配置数据中的随机数据获取List
                    {
                        int index = i;
                        requireIDList.Add(weaponDataIDList[index-1-requiredAmount]);
                    }
                }
                break;
            case CommodityType.Mix:
                // 初始宝箱暂时没有这个选项
                break;
        }
        
        CycleListChestPanel.InitComponents(commodityPanelAmount,requireIDList,panelType,gameObject.GetComponent<CommodityManager>());
    }
    
    public void BuyCommodity(PropData commodity,BtnWidget btnWidget)
    {
        if (PropJoyManager.Instance.IsPropColumnEmpty() && commodity.coin < _money)
        {
            PropJoyManager.Instance.TranslatePropByManager(commodity.id);
            //UIManager.Instance().GetPanel<StartChestPanel>("StartChestPanel").InitInfo_PropColumn(); TODO: 这里需要之后去把StartChestPanel给改了
            startChestPanel.InitInfo_PropColumn();
            CheckBuyQualifications(btnWidget);
        }
    }
    
    public void BuyCommodity(WeaponData commodity,BtnWidget btnWidget)
    {
        if (WeaponDepot.Instance.HasWeaponDepotEmpty() && commodity.coin < _money)
        {
            WeaponDepot.Instance.AddWeapon(commodity);
            //UIManager.Instance().GetPanel<StartChestPanel>("StartChestPanel").InitInfo_WeaponColumn();
            startChestPanel.InitInfo_WeaponColumn();
            CheckBuyQualifications(btnWidget);
        }
    }

    private void CheckBuyQualifications(BtnWidget btnWidget)// 检查是否还能够继续选择,若不能则刷新,不能刷就开始游戏,买完之后再来做这个的啊  笨蛋笨蛋
    {
        if(_chestData.Peek().selNum > _selectedNum)// 有选择资格,隐藏单个Panel
        {
            _selectedNum++;
            //HideTargetCommodityPanel(index);
            btnWidget.SetButtonInteractable(false);
        }
        else// 没有选择资格, HideALL之后, Dequeue一下数据, 刷新界面并跳转新界面
        {
            _selectedNum = 1;
            //HideAllCommodityPanel(); //这里后面交给重新设置Data的来更新,就不需要自己去更新数据了
            if (_chestData.Count > 1)// 如果还有可以选择的道具,就会更新面板
            {
                _chestData.Dequeue();
                ShowCommodityPanel(_chestData.Peek());
            }
            else// 没有可以选择的道具了,那就该开始游戏了
            {
                startChestPanel.ContinueGame();
            }
        }
    }
}
