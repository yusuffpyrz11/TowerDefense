using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Attach to any GameObject in the scene.
/// Builds the entire game at runtime — no extra prefabs or assets needed.
/// Tested with Unity 2022.3 LTS.
/// </summary>
public class SceneBuilder : MonoBehaviour
{
    // ── Path definition (world-space waypoints) ──────────────────────────────
    static readonly Vector3[] WayPoints =
    {
        new Vector3(-1f, 4f, 0f),
        new Vector3(4f,  4f, 0f),
        new Vector3(4f,  1f, 0f),
        new Vector3(9f,  1f, 0f),
        new Vector3(9f,  6f, 0f),
        new Vector3(14f, 6f, 0f),
        new Vector3(17f, 6f, 0f),   // exit (off screen)
    };

    void Awake()
    {
        SetupCamera();

        var path = new List<Vector3>(WayPoints);
        BuildMap(path);

        // GameManager must exist before UI (UI wires buttons to it)
        var gm = FindObjectOfType<GameManager>();
        if (gm == null)
        {
            gm = new GameObject("GameManager").AddComponent<GameManager>();
        }
        gm.SetPath(path);

        BuildUI(gm);
    }

    // ── Camera ───────────────────────────────────────────────────────────────
    void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            cam = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
        }
        cam.orthographic     = true;
        cam.orthographicSize = 6f;
        cam.transform.position = new Vector3(8f, 3.5f, -10f);
        cam.backgroundColor = new Color(0.12f, 0.17f, 0.12f);
        cam.clearFlags = CameraClearFlags.SolidColor;
    }

    // ── Map ──────────────────────────────────────────────────────────────────
    void BuildMap(List<Vector3> path)
    {
        // Grass background
        Quad("Grass", new Vector3(8f, 3.5f, 1f), new Vector3(20f, 10f, 1f),
             new Color(0.22f, 0.38f, 0.18f), -5);

        // Path segments
        Color dirt = new Color(0.62f, 0.50f, 0.34f);
        for (int i = 0; i < path.Count - 1; i++)
            DrawSegment(path[i], path[i+1], dirt, 0.9f);

        // Start / End markers
        Marker(path[0],              new Color(0.3f, 0.9f, 0.3f, 0.8f), "▶ GIRIŞ");
        Marker(path[path.Count - 1], new Color(0.9f, 0.3f, 0.3f, 0.8f), "✖ ÇIKIŞ");
    }

    void DrawSegment(Vector3 a, Vector3 b, Color c, float width)
    {
        Vector3 mid = (a + b) * 0.5f;
        float len = Vector3.Distance(a, b) + width; // overlap corners
        bool horiz = Mathf.Abs(b.x - a.x) > Mathf.Abs(b.y - a.y);
        Vector3 scale = horiz ? new Vector3(len, width, 1f) : new Vector3(width, len, 1f);
        Quad("Path", new Vector3(mid.x, mid.y, 0.1f), scale, c, -3);
    }

    void Marker(Vector3 pos, Color c, string label)
    {
        var go = Quad("Marker", pos, Vector3.one * 0.7f, c, -1);
        var txt = new GameObject("Lbl");
        txt.transform.SetParent(go.transform);
        txt.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        txt.transform.localScale = Vector3.one * 0.18f;
        var tm = txt.AddComponent<TextMesh>();
        tm.text = label;
        tm.fontSize = 24;
        tm.color = Color.white;
        tm.anchor = TextAnchor.LowerCenter;
        tm.alignment = TextAlignment.Center;
    }

    GameObject Quad(string name, Vector3 pos, Vector3 scale, Color c, int order)
    {
        var go = new GameObject(name);
        go.transform.position = pos;
        go.transform.localScale = scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SolidSprite(c);
        sr.sortingOrder = order;
        return go;
    }

    // ── UI ───────────────────────────────────────────────────────────────────
    void BuildUI(GameManager gm)
    {
        var canvasGo = new GameObject("Canvas");
        var canvas   = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        // ── Top HUD bar ──────────────────────────────────────────────────────
        var topBar = Panel(canvasGo.transform,
            anchorMin: new Vector2(0,1), anchorMax: new Vector2(1,1),
            pivot: new Vector2(0.5f,1),
            offsetMin: new Vector2(0,-52), offsetMax: new Vector2(0,0),
            color: new Color(0,0,0,0.72f));

        gm.goldText  = Txt(topBar, "💰 150",  20, new Vector2(10,-8),  new Vector2(180,36), TextAnchor.MiddleLeft);
        gm.livesText = Txt(topBar, "❤️ 20",   20, new Vector2(200,-8), new Vector2(180,36), TextAnchor.MiddleLeft);
        gm.waveText  = Txt(topBar, "Dalga: 0",20, new Vector2(0,-8),   new Vector2(380,36), TextAnchor.MiddleCenter);

        // ── Bottom toolbar ───────────────────────────────────────────────────
        var bot = Panel(canvasGo.transform,
            anchorMin: new Vector2(0,0), anchorMax: new Vector2(1,0),
            pivot: new Vector2(0.5f,0),
            offsetMin: new Vector2(0,0), offsetMax: new Vector2(0,70),
            color: new Color(0,0,0,0.82f));

        gm.startWaveBtn = Btn(bot, "▶ Dalgayı Başlat",
            new Vector2(10,5), new Vector2(170,60), new Color(0.65f,0.15f,0.15f));
        gm.startWaveBtn.onClick.AddListener(gm.StartWave);

        var bArcher = Btn(bot, "🏹 Okçu\n50G",   new Vector2(195,5), new Vector2(110,60), new Color(0.18f,0.55f,0.18f));
        var bMage   = Btn(bot, "🔮 Büyücü\n80G", new Vector2(315,5), new Vector2(110,60), new Color(0.38f,0.15f,0.75f));
        var bKnight = Btn(bot, "⚔️ Şövalye\n100G",new Vector2(435,5),new Vector2(120,60), new Color(0.6f,0.55f,0.1f));

        bArcher.onClick.AddListener(() => gm.BeginPlace(0));
        bMage.onClick.AddListener(  () => gm.BeginPlace(1));
        bKnight.onClick.AddListener(() => gm.BeginPlace(2));

        // ── Legend ───────────────────────────────────────────────────────────
        var leg = Panel(canvasGo.transform,
            anchorMin: new Vector2(0,0.5f), anchorMax: new Vector2(0,0.5f),
            pivot: new Vector2(0,0.5f),
            offsetMin: new Vector2(5,-80), offsetMax: new Vector2(155,80),
            color: new Color(0,0,0,0.6f));

        Txt(leg, "Düşmanlar", 14, new Vector2(8,-8), new Vector2(130,24), TextAnchor.UpperLeft);
        ColorRow(leg, "🔴 Normal",  new Color(0.85f,0.3f,0.3f),   new Vector2(8,-36));
        ColorRow(leg, "🟡 Hızlı",   new Color(0.95f,0.75f,0.1f),  new Vector2(8,-60));
        ColorRow(leg, "🔵 Tank",    new Color(0.4f,0.45f,0.9f),   new Vector2(8,-84));
        ColorRow(leg, "💰 Altın kazanırsın!", Color.clear,         new Vector2(8,-108));

        // ── Upgrade panel ────────────────────────────────────────────────────
        var upgPanel = Panel(canvasGo.transform,
            anchorMin: new Vector2(1,0.5f), anchorMax: new Vector2(1,0.5f),
            pivot: new Vector2(1,0.5f),
            offsetMin: new Vector2(-210,-100), offsetMax: new Vector2(-5,100),
            color: new Color(0.05f,0.05f,0.12f,0.95f));
        gm.upgradePanel = upgPanel;

        gm.upgradeTitleText = Txt(upgPanel, "Kule",      18, new Vector2(8,-10), new Vector2(190,30), TextAnchor.UpperCenter);
        gm.upgradeCostText  = Txt(upgPanel, "Geliştir:", 15, new Vector2(8,-45), new Vector2(190,26), TextAnchor.UpperCenter);
        gm.upgradeBtn = Btn(upgPanel, "⬆ Geliştir",  new Vector2(8,-76),  new Vector2(190,40), new Color(0.15f,0.45f,0.8f));
        gm.sellBtn    = Btn(upgPanel, "💸 Sat (½ iade)", new Vector2(8,-122), new Vector2(190,36), new Color(0.65f,0.25f,0.1f));
        gm.upgradeBtn.onClick.AddListener(gm.UpgradeSelected);
        gm.sellBtn.onClick.AddListener(gm.SellSelected);
        upgPanel.SetActive(false);

        // ── Game Over text ───────────────────────────────────────────────────
        var goTxt = Txt(canvasGo.transform, "", 52,
            new Vector2(-320,-120), new Vector2(640,240), TextAnchor.MiddleCenter);
        goTxt.color = new Color(1f,0.3f,0.3f);
        goTxt.gameObject.SetActive(false);
        gm.gameOverText = goTxt;
    }

    // ── UI Helpers ───────────────────────────────────────────────────────────

    RectTransform Panel(Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        var go  = new GameObject("Panel");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        var rt  = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot     = pivot;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        return rt;
    }

    Text Txt(Transform parent, string content, int size,
             Vector2 anchoredPos, Vector2 sizeDelta, TextAnchor anchor)
    {
        var go  = new GameObject("Text");
        go.transform.SetParent(parent, false);
        var txt = go.AddComponent<Text>();
        txt.text      = content;
        txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize  = size;
        txt.alignment = anchor;
        txt.color     = Color.white;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin      = new Vector2(0,1);
        rt.anchorMax      = new Vector2(0,1);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta      = sizeDelta;
        return txt;
    }

    Button Btn(Transform parent, string label, Vector2 pos, Vector2 size, Color c)
    {
        var go  = new GameObject("Btn");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = c;
        var btn = go.AddComponent<Button>();
        var cols = btn.colors;
        cols.highlightedColor = Lighten(c, 0.18f);
        cols.pressedColor     = Lighten(c, -0.12f);
        btn.colors = cols;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0,0);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var lgo  = new GameObject("Lbl");
        lgo.transform.SetParent(go.transform, false);
        var txt  = lgo.AddComponent<Text>();
        txt.text = label;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize  = 14;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color     = Color.white;
        var lrt = lgo.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;

        return btn;
    }

    void ColorRow(Transform parent, string label, Color swatch, Vector2 pos)
    {
        if (swatch != Color.clear)
        {
            var sq  = new GameObject("Sq");
            sq.transform.SetParent(parent, false);
            var img = sq.AddComponent<Image>();
            img.color = swatch;
            var rt  = sq.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0,1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(14,14);
        }
        float lx = swatch == Color.clear ? pos.x : pos.x + 18;
        var lbl = Txt(parent, label, 12, new Vector2(lx, pos.y + 2), new Vector2(130,18), TextAnchor.UpperLeft);
        lbl.color = new Color(0.9f,0.9f,0.9f);
    }

    // ── Sprite ───────────────────────────────────────────────────────────────

    Sprite SolidSprite(Color c)
    {
        var tex = new Texture2D(1,1);
        tex.SetPixel(0,0,c);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,1,1), new Vector2(0.5f,0.5f));
    }

    Color Lighten(Color c, float amt) =>
        new Color(Mathf.Clamp01(c.r+amt), Mathf.Clamp01(c.g+amt), Mathf.Clamp01(c.b+amt), c.a);
}
