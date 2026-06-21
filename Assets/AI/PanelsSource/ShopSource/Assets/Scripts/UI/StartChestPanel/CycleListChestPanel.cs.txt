using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class ChestPanel : CycleListItem
{
    public GameObject gameObject { get; set; }//Item上面的,在创建Item的时候顺便带上
    public RectTransform transform { get; set; }//Item上面的,在创建Item的时候顺便带上
    public int index { get; set; }//这个是创建的时候顺便带上的
    public Commoditywidget commodityPanel { get; set; }
    public CycleListData data { get; set; }
}

class ChestPanelData: CycleListData//考虑一下需要些什么数据,现在是生成的逻辑搬到了这里,还是说生成的逻辑放在上层,这里只负责接收Item和Data
{
    public int index;
    public RandomItem objectData;// 对应的数据
    //public Sprite targetSprite;// 不要由Item去加载,丢进缓存里,不然重复加载太消耗性能快速拖动,用点空间
    public CommodityManager commodityManager;
}

public class CycleListChestPanel : MonoBehaviour
{
    #region properties

    private int commodityCount;
    private CommodityType commodityType;

    #endregion
    
    #region Component
        
    ScrollRect scrollRect;
    RectTransform scrollRectTrs;
    RectTransform contentTrs;
    CycleList cycList;
    
    #endregion
    
    #region Structure
    
    //List<ChestPanel> chestPanels = new List<ChestPanel>();
    private readonly List<CycleListData> chestPanelDatas = new List<CycleListData>();//这个是生成出来的ListChestData
    private readonly List<CycleListItem> CycleItems = new List<CycleListItem>();//生成出来的样式
    
    #endregion
    
    private int count;//生成的样式数量
    private float itemSpace;
    
    //void Start() => InitComponents();
    
    #region cyclelist
    
    public void InitComponents(int commodityCount, List<int> IDList,CommodityType commodityType,CommodityManager manager)//外部调用此方法进行初始化,需要在Data导入之后进行
    {
        scrollRect = transform.Find("Scroll View").GetComponent<ScrollRect>();
        scrollRectTrs = (RectTransform)scrollRect.transform;// 这里是整个遮罩
        contentTrs = scrollRect.content;
        InitData(commodityCount, IDList, commodityType,manager);
        //InitCycleItems(commodityCount,commodityType);
        //InitCycleList();
    }

    private void InitCycleItems(int count,CommodityType type)//Item和Data都由Manager生成，这里后面改成接收吧
    {
        var ItemBaseGO = contentTrs.Find("BaseClone").gameObject;// 这里指的就是那个起始物件
        var ItemBaseTrs = (RectTransform)ItemBaseGO.transform;
        var itemWidth = ItemBaseTrs.rect.width;
        itemSpace = itemWidth + 10;//设置间隙
        ItemBaseGO.SetActive(false); 
        for (int i = 0; i < count; i++)// 关于生成Item样式
        {
            var itemGo = Instantiate(ItemBaseGO, contentTrs);
            var item = new ChestPanel//这里的数据类型到时候就根据自己的需求,最主要的是需要把位置和索引赋予上去,这样子后面就方便我去调用里面的方法了
            {
                gameObject = itemGo,
                transform = (RectTransform)itemGo.transform,
                index = i + 1,
                commodityPanel = itemGo.GetComponentInChildren<Commoditywidget>()// 这上面做的是一些初始化的操作
            };
            itemGo.name = Tools.Key(ItemBaseGO.name, item.index);
            CycleItems.Add(item);//存储
        }
    }
    
    private void InitCycleList()// 关于如何配置,更新的话要不重新配置一次,就不改动原结构了,到时候问过了再优化
    {
        var dataList = chestPanelDatas;
        scrollRect.enabled = dataList.Count > Mathf.FloorToInt(scrollRectTrs.rect.width / itemSpace);
        
        CycleListSetting setting = new CycleListSetting // 关于CycleList的配置,有点像之前的那个Marker
        {
            grid = new GridSetting
            {
              isVertical = false,//上下能动否
              itemSpace = itemSpace,
            },
            
            scrollRect = scrollRect,
            content = contentTrs,
            
            items = CycleItems,
            dataList = dataList,
            
            updateFunc = UpdateCycleList,
        };

        void UpdateCycleList(CycleListItem item, CycleListData data)
        {
            var _item = (ChestPanel)item;
            var _data = (ChestPanelData)data;
            _item.data = _data;
            _item.commodityPanel.InitData(_data.objectData,_data.index);
            _item.commodityPanel.AutoShowProperties(contentTrs,_data.objectData.c_type);
            _item.commodityPanel.RegisterPropPanelManager(_data.commodityManager);
        }
        
        cycList = new CycleList(setting);//用配置创建cycle对象
        cycList.SetItem();
    }
    #endregion
    
    #region DataManage

    /// <summary>
    /// 外部初始化CycleList的方法
    /// </summary>
    /// <param name="commodityCount"> 需要生成的商品数量</param>
    /// <param name="IDList"> 随机出来的商店道具ID列表</param>
    /// <param name="commodityType"> 商品类型</param>
    public void InitData(int commodityCount, List<int> IDList,CommodityType commodityType,CommodityManager manager)
    {
        this.commodityCount = commodityCount;
        this.commodityType = commodityType;
        chestPanelDatas.Clear();
        for (int i = 0; i < IDList.Count; i++)
        {
            int index = i;
            if (commodityType == CommodityType.prop)
            {
                var dataCell = new ChestPanelData
                {
                    index = index,
                    objectData = GameManager.Instance.propController.GetPropData(IDList[index]),
                    commodityManager = manager,
                };
                chestPanelDatas.Add(dataCell);
            }
            else if (commodityType == CommodityType.weapon)
            {
                var dataCell = new ChestPanelData
                {
                    index = index,
                    objectData = GameManager.Instance.weaponController.FindWeaponData(IDList[index]),
                    commodityManager = manager,
                };
                chestPanelDatas.Add(dataCell);
            }
            
        }
        InitCycleItems(commodityCount,commodityType);
        InitCycleList();
    }
    
    #endregion
}
