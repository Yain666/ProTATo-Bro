using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;

    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<UIManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("UIManager");
                    instance = go.AddComponent<UIManager>();
                }
            }

            return instance;
        }
    }

    public const string DefaultPanelRootPath = "UI/Panels/";

    [Header("UI Root")]
    public Canvas rootCanvas;
    public RectTransform hudLayer;
    public RectTransform panelLayer;
    public RectTransform popupLayer;
    public RectTransform topLayer;

    private readonly Dictionary<Type, BasePanel> _panelCache = new Dictionary<Type, BasePanel>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureUIRoot();
        EnsureUnityEventSystem();
    }

    public T OpenPanel<T>(string panelPath = null, UILayer layer = UILayer.Panel, object args = null) where T : BasePanel
    {
        T panel = GetOrCreatePanel<T>(panelPath, layer);
        if (panel == null) return null;

        panel.transform.SetAsLastSibling();
        panel.Open(args);
        return panel;
    }

    public void ClosePanel<T>() where T : BasePanel
    {
        Type panelType = typeof(T);
        if (_panelCache.TryGetValue(panelType, out BasePanel panel))
        {
            panel.Close();
        }
    }

    public T GetPanel<T>() where T : BasePanel
    {
        Type panelType = typeof(T);
        return _panelCache.TryGetValue(panelType, out BasePanel panel) ? panel as T : null;
    }

    public void CloseAllPanels()
    {
        foreach (BasePanel panel in _panelCache.Values)
        {
            panel.Close();
        }
    }

    private T GetOrCreatePanel<T>(string panelPath, UILayer layer) where T : BasePanel
    {
        Type panelType = typeof(T);
        if (_panelCache.TryGetValue(panelType, out BasePanel cachedPanel))
        {
            return cachedPanel as T;
        }

        string finalPath = string.IsNullOrEmpty(panelPath) ? DefaultPanelRootPath + panelType.Name : panelPath;
        EnsureResourceManager();
        GameObject prefab = ResourceManager.Instance.GetPrefab(finalPath);
        if (prefab == null)
        {
            Debug.LogError($"[UIManager] 找不到 Panel 预制体: {finalPath}");
            return null;
        }

        Transform parent = GetLayerRoot(layer);
        GameObject instanceObject = Instantiate(prefab, parent);
        instanceObject.name = panelType.Name;

        RectTransform rectTransform = instanceObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            StretchToParent(rectTransform);
        }

        T panel = instanceObject.GetComponent<T>();
        if (panel == null)
        {
            Debug.LogError($"[UIManager] Panel 预制体缺少脚本: {panelType.Name}");
            Destroy(instanceObject);
            return null;
        }

        instanceObject.SetActive(false);
        _panelCache.Add(panelType, panel);
        return panel;
    }

    private void EnsureUIRoot()
    {
        if (rootCanvas == null)
        {
            GameObject canvasObject = new GameObject("UIRoot", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            rootCanvas = canvasObject.GetComponent<Canvas>();
            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            rootCanvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        hudLayer = EnsureLayer("HudLayer", hudLayer);
        panelLayer = EnsureLayer("PanelLayer", panelLayer);
        popupLayer = EnsureLayer("PopupLayer", popupLayer);
        topLayer = EnsureLayer("TopLayer", topLayer);
    }

    private RectTransform EnsureLayer(string layerName, RectTransform currentLayer)
    {
        if (currentLayer != null) return currentLayer;

        Transform existing = rootCanvas.transform.Find(layerName);
        if (existing != null) return existing as RectTransform;

        GameObject layerObject = new GameObject(layerName, typeof(RectTransform));
        RectTransform rectTransform = layerObject.GetComponent<RectTransform>();
        rectTransform.SetParent(rootCanvas.transform, false);
        StretchToParent(rectTransform);
        return rectTransform;
    }

    private Transform GetLayerRoot(UILayer layer)
    {
        switch (layer)
        {
            case UILayer.Hud:
                return hudLayer;
            case UILayer.Popup:
                return popupLayer;
            case UILayer.Top:
                return topLayer;
            default:
                return panelLayer;
        }
    }

    private void EnsureUnityEventSystem()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null) return;

        GameObject eventSystemObject = new GameObject("UnityEventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(StandaloneInputModule));
        DontDestroyOnLoad(eventSystemObject);
    }

    private void EnsureResourceManager()
    {
        if (ResourceManager.Instance != null) return;

        GameObject resourceManagerObject = new GameObject("ResourceManager");
        resourceManagerObject.AddComponent<ResourceManager>();
        DontDestroyOnLoad(resourceManagerObject);
    }

    private void StretchToParent(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localScale = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;
    }
}
