using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColumnWidget : MonoBehaviour
{
    #region --- Components ---

    private BtnWidget weaponBtn;
    private BtnWidget propBtn;
    private IconSlot iconSlot1;
    private IconSlot iconSlot2;
    private IconSlot iconSlot3;

    #endregion --- Components ---

    #region --- Config ---

    private const string BTN_SELECTCOLOR = "#FFEE00";
    private const string BTN_NORMALCOLOR = "#FFFFFF";
    private const int propColumnNum = 3;
    private const int weaponColumnNum = 3;
    
    #endregion --- Config ---
    
    public void InitComponents()
    {
        weaponBtn = transform.Find("Btn_ChangeWeaponColumn").GetComponent<BtnWidget>();
        propBtn = transform.Find("Btn_ChangePropColumn").GetComponent<BtnWidget>();
        iconSlot1 = transform.Find("Image_ObjectColumnBackground_1").GetComponent<IconSlot>();
        iconSlot2 = transform.Find("Image_ObjectColumnBackground_2").GetComponent<IconSlot>();
        iconSlot3 = transform.Find("Image_ObjectColumnBackground_3").GetComponent<IconSlot>();
        BindAction();
    }

    public void BindAction()
    {
        weaponBtn.BindButton(InitInfo_WeaponColumn);
        propBtn.BindButton(InitInfo_PropColumn);
    }
    
    public void InitInfo_PropColumn()//  初始化道具栏数据信息,切换时、初始化时使用
    {
        List<PropData> propJoyData = PropJoyManager.Instance.GetJoyPropDatasByManager();
        ChangeImageColor(weaponBtn, BTN_NORMALCOLOR);
        ChangeImageColor(propBtn, BTN_SELECTCOLOR);
        int dataCount = 0;
        for (int i = 0; i < propColumnNum; i++)
        {
            if (propJoyData[i] != null)
            {
                SetIconSlot(i+1, propJoyData[i].propImg);
                dataCount++;
            }
            else
                SetIconSlot(i+1, null);
        }
        propBtn.SetDescription($"({dataCount}/3)");
    }
    
    public void InitInfo_WeaponColumn()//  初始化武器栏数据信息,切换时、初始化时使用
    {
        List<WeaponData> weaponDepotDatas = WeaponDepot.Instance.GetAllWeaponsByDepot();
        ChangeImageColor(weaponBtn, BTN_SELECTCOLOR);
        ChangeImageColor(propBtn, BTN_NORMALCOLOR);
        
        for (int i = 0; i < weaponColumnNum; i++)
        {
            if (weaponDepotDatas.Count >= i + 1)
            {
                SetIconSlot(i+1, weaponDepotDatas[i].weaponImg);
            }
            else SelectTargetIconSlot(i+1).SetIconImageActive(false);
        }
        weaponBtn.SetDescription($"({weaponDepotDatas.Count}/3)");
    }
    
    public void ChangeImageColor(BtnWidget targetBtn,string color)
    {
        Color nowCol = new Color();
        if(ColorUtility.TryParseHtmlString(color,out nowCol))
            targetBtn.backgroundImage.color = nowCol;
    }

    public void SetIconSlot(int index,string imageUrl)
    {
        IconSlot targetIconSlot = SelectTargetIconSlot(index);
        
        if (imageUrl != null)
        {
            targetIconSlot.SetIconImageActive(true);
            StartCoroutine(HttpHelper.Instance.HttpLoadSprite
            (imageUrl, delegate(Sprite sprite)
            {
                targetIconSlot.SetIconImage(sprite);
            }));
        }
        else targetIconSlot.SetIconImageActive(false);
    }

    public IconSlot SelectTargetIconSlot(int index)
    {
        IconSlot targetIconSlot;
        switch (index)
        {
            case 1:
                targetIconSlot = iconSlot1;
                break;
            case 2:
                targetIconSlot = iconSlot2;
                break;
            case 3:
                targetIconSlot = iconSlot3;
                break;
            default:
                return null;
        }
        return targetIconSlot;
    }
}
