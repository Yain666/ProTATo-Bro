using UnityEngine;
using UnityEngine.UI;

public class Commoditywidget : MonoBehaviour
{
    #region --- Properties ---

    private int _propertiesNum;
    private PropData _propData;
    private WeaponData _weaponData;
    private CommodityManager _commodityManagerPanel;
    private int CommodityIndex;
    private GameObject textProperties;
    
    // TODO:以后会交给这个来处理判断逻辑
    private RandomItem randomItem;
    
    #endregion --- Properties ---
    
    #region --- Components ---

    public IconSlot icon;
    public Text commodityName;
    public Text commodityType;
    public Text objectType; // 道具 或者 武器
    public BtnWidget btnWidget;
    public Image backgroundImage;
    
    #endregion --- Components ---
    
    #region --- Config ---

    public static readonly Vector2 SIZE_DELTA = new Vector2(310f, 43f);
    public static readonly Vector3 LOCAL_SCALE = new Vector3(0.5f, 0.5f, 1f);
    public const string PANEL_PROPTYPE_COLOR = "#FFFF00";
    private readonly string PropBackground = "column_card_bg_green";
    private readonly string WeaponBackground = "column_card_bg_yellow";

    #endregion --- Config ---

    public void Awake()
    {
        btnWidget?.CleanListener();
        btnWidget?.BindButton(delegate() { BuyCommodity();});
        textProperties = transform.Find("Panel_Properties").gameObject;
    }

    //购买商品时会调用上一层的代码，由上层处理购买逻辑
    public void BuyCommodity()
    {
        if(randomItem.c_type == CommodityType.prop)
            _commodityManagerPanel.BuyCommodity(_propData,btnWidget);
        else
            _commodityManagerPanel.BuyCommodity(_weaponData,btnWidget);
    }
    
    public void AutoShowPropertiesByProp(Transform contentTransform) // 这个方法必须在show完这个Panel才能调用，他会去找那个父对象的
    {
        DestoryAllPropertyText();
        Tools.LoadAndInitialize("Text_Properties_Prefab", (prefab) =>
        {
            int index = 0;
            foreach (var propInfo in _propData.PropAttr)
            {
                // var properties = DataReader.Instance.GetPropertiesDataById(propInfo.Key);
                // ShowProperty(prefab,properties.description,propInfo.Value.ToString(),ref index); // TODO:这里后面想办法弄一下那个委托的问题，先这样子写着
                PropertiesData propertiesData = GameManager.Instance.propertyController.GetPropertiesDataByID(propInfo.Key);
                ShowProperty(prefab,propertiesData.description,propInfo.Value.ToString(),ref index);
            }
            Destroy(prefab);
        });
    }

    public void AutoShowPropertiesByWeapon(Transform contentTransform) // 这个是生成道具属性的
    {
        DestoryAllPropertyText();
        Tools.LoadAndInitialize("Text_Properties_Prefab", ( propertyPrefab)=>
        {
            int index = 0;
            ShowProperty(propertyPrefab,"攻击射程",_weaponData.weaponRange.ToString(),ref index);
            ShowProperty(propertyPrefab,"攻击范围",_weaponData.weaponArea.ToString(),ref index);
            ShowProperty(propertyPrefab,"是否穿透",_weaponData.isPenetrate == 0?"是":"否",ref index);
            ShowProperty(propertyPrefab,"冷却时间",_weaponData.cooldownTime.ToString(),ref index);
            // 武器数值相关的文字信息
            foreach (var wpInfo in _weaponData.WeaponAttr)
            {
                // var properties = DataReader.Instance.GetPropertiesDataById(wpInfo.Key);
                // ShowProperty(propertyPrefab, properties.description,wpInfo.Value.ToString(),ref index); // TODO:这里后面想办法弄一下那个委托的问题，先这样子写着
                PropertiesData propertiesData = GameManager.Instance.propertyController.GetPropertiesDataByID(wpInfo.Key);
                ShowProperty(propertyPrefab, propertiesData.description,wpInfo.Value.ToString(),ref index);
            }
            
            Destroy(propertyPrefab);
        });
    }

    public void ShowProperty(GameObject prefab,string description,string value,ref int index)
    {
        index++;
        // 武器数值相关的文字信息
        GameObject target = Instantiate(prefab, textProperties.transform,false);
        target.name = $"Text_Properties_{index}";
        Text objtextPrefab = target.GetComponent<Text>();
        
        objtextPrefab.text = $"<color={PANEL_PROPTYPE_COLOR}>{description}:</color> {value}";
        
        objtextPrefab.rectTransform.sizeDelta = SIZE_DELTA;
        objtextPrefab.rectTransform.localScale = LOCAL_SCALE;
        objtextPrefab.fontSize = 30;
        objtextPrefab.horizontalOverflow = HorizontalWrapMode.Overflow;
    }
    
    public void AutoShowProperties(Transform contentTransform, CommodityType type)
    {
        switch (type)
        {
            case CommodityType.prop:
                AutoShowPropertiesByProp(contentTransform);
                break;
            case CommodityType.weapon:
                AutoShowPropertiesByWeapon(contentTransform);
                break;
            default:
                break;
        }
    }

    // 这个是用来加载道具的
    public void InitData(PropData propData)
    {
        this.gameObject.SetActive(true);
        _propData = propData;
        LoadAndSetSprite(PropBackground);
        
        objectType.text = "道具"; // 类型名字
        commodityName.text = propData.propName;// 名字
        commodityType.text = $"<color={PANEL_PROPTYPE_COLOR}>{propData.propType.ToString()}</color>";// 级别
        StartCoroutine(HttpHelper.Instance.HttpLoadSprite(propData.propImg, delegate(Sprite sprite)
        {
            icon.SetIconImage(sprite);
        }));// 图像
        
    }

    // 这个是用来加载武器的
    public void InitData(WeaponData weaponData)
    {
        _weaponData = weaponData;
        LoadAndSetSprite(WeaponBackground);
        
        objectType.text = "武器"; // 类型名字
        commodityName.text = weaponData.weaponName;// 名字
        commodityType.text = $"<color={PANEL_PROPTYPE_COLOR}>{weaponData.weaponType.ToString()}</color>"; // 级别
        StartCoroutine(HttpHelper.Instance.HttpLoadSprite(weaponData.weaponImg,delegate(Sprite sprite)
        {
            icon.SetIconImage(sprite);
        }));// 图像
        
    }

    public void InitData(RandomItem itemData,int index)
    {
        CommodityIndex = index;
        randomItem = itemData;
        if(itemData.c_type == CommodityType.prop)
            InitData((PropData) itemData);
        else
            InitData((WeaponData) itemData);
    }
    
    public void RegisterPropPanelManager(CommodityManager commodityManagerPanel)// 初始化方便待会购买按钮可以调用上面的代码
    {
        _commodityManagerPanel = commodityManagerPanel;
    }
    
    private void LoadAndSetSprite(string columnName)
    {
        if(columnName == null || backgroundImage == null) return;
        
        Tools.LoadAsset<Sprite>(columnName, sp=>
        {
            if (sp == null) return;
            if(backgroundImage != null)
                backgroundImage.sprite = sp;
        });
    }

    private void DestoryAllPropertyText()
    {
        Transform childTrs;
        for (int i = 0; i < textProperties.transform.childCount; i++)
        {
            childTrs = textProperties.transform.GetChild(i);
            GameObject.Destroy(childTrs.gameObject);
        }
    }

}
