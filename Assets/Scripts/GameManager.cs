using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // ── Runtime state ────────────────────────────────────────────────────────
    public int gold  = 150;
    public int lives = 20;
    public int wave  = 0;
    public bool waveActive = false;

    // ── Path ────────────────────────────────────────────────────────────────
    public List<Vector3> enemyPath = new List<Vector3>();

    // ── UI refs (set by SceneBuilder) ────────────────────────────────────────
    [HideInInspector] public Text goldText, livesText, waveText, gameOverText;
    [HideInInspector] public Button startWaveBtn;
    [HideInInspector] public GameObject upgradePanel;
    [HideInInspector] public Text upgradeTitleText, upgradeCostText;
    [HideInInspector] public Button upgradeBtn, sellBtn;

    // ── Tower placement ──────────────────────────────────────────────────────
    private bool placing = false;
    private TowerType placingType;
    private static readonly int[] TowerCosts = { 50, 80, 100 }; // Archer Mage Knight

    // ── Selected tower ───────────────────────────────────────────────────────
    private Tower _selected;
    public  Tower SelectedTower => _selected;

    private List<Tower> allTowers = new List<Tower>();

    // ── Path collision grid ──────────────────────────────────────────────────
    private HashSet<Vector2Int> pathCells = new HashSet<Vector2Int>();

    void Awake() { Instance = this; }

    void Start()
    {
        // Build path cell set (for placement validation)
        foreach (var p in enemyPath)
            pathCells.Add(Vector2Int.RoundToInt(new Vector2(p.x, p.y)));

        UpdateHUD();
        if (upgradePanel != null) upgradePanel.SetActive(false);
    }

    void Update()
    {
        if (placing)
        {
            if (Input.GetMouseButtonDown(0)) TryPlace();
            if (Input.GetMouseButtonDown(1)) CancelPlace();
        }
        else
        {
            // Click on empty space → deselect
            if (Input.GetMouseButtonDown(0))
            {
                var hit = Physics2D.OverlapPoint(
                    Camera.main.ScreenToWorldPoint(Input.mousePosition));
                if (hit == null || hit.GetComponent<Tower>() == null)
                    Deselect();
            }
        }
    }

    // ── Placement ────────────────────────────────────────────────────────────

    public void BeginPlace(int typeIndex)
    {
        int cost = TowerCosts[typeIndex];
        if (gold < cost) { Debug.Log("Yeterli altın yok!"); return; }
        placingType = (TowerType)typeIndex;
        placing = true;
        Deselect();
    }

    void TryPlace()
    {
        Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        wp.z = 0f;
        Vector2Int cell = Vector2Int.RoundToInt(new Vector2(wp.x, wp.y));
        Vector3 snap = new Vector3(cell.x, cell.y, 0f);

        // Reject path tiles
        if (pathCells.Contains(cell)) { Debug.Log("Yola kule koyamazsın!"); return; }

        // Reject occupied tiles
        foreach (var t in allTowers)
        {
            if (t == null) continue;
            if (Vector2Int.RoundToInt(new Vector2(t.transform.position.x, t.transform.position.y)) == cell)
            { Debug.Log("Bu hücre dolu!"); return; }
        }

        SpendGold(TowerCosts[(int)placingType]);
        CreateTower(snap, placingType);
        placing = false;
    }

    void CancelPlace() { placing = false; }

    void CreateTower(Vector3 pos, TowerType type)
    {
        var go = new GameObject("Tower_" + type);
        go.transform.position = pos;

        // Visuals
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 2;
        sr.sprite = MakeTowerSprite(type);

        // Tower component
        var t = go.AddComponent<Tower>();
        t.towerType = type;
        switch (type)
        {
            case TowerType.Archer:
                t.damage=25f; t.range=3.5f; t.fireRate=1.2f; t.upgradeCost=60;  break;
            case TowerType.Mage:
                t.damage=40f; t.range=3.0f; t.fireRate=0.7f; t.upgradeCost=90;  break;
            case TowerType.Knight:
                t.damage=35f; t.range=1.2f; t.fireRate=1.5f; t.upgradeCost=75;  break;
        }
        sr.color = t.GetTowerColor();

        // Collider for raycasts
        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.42f;

        var clicker = go.AddComponent<TowerClickHandler>();
        clicker.tower = t;

        allTowers.Add(t);
        allTowers.RemoveAll(x => x == null);
    }

    // ── Wave spawning ────────────────────────────────────────────────────────

    public void StartWave()
    {
        if (waveActive) return;
        wave++;
        UpdateHUD();
        StartCoroutine(WaveCoroutine());
        startWaveBtn.interactable = false;
    }

    IEnumerator WaveCoroutine()
    {
        waveActive = true;
        int count = 6 + wave * 2;

        for (int i = 0; i < count; i++)
        {
            SpawnEnemy(ChooseType());
            float delay = Mathf.Max(0.5f, 1.4f - wave * 0.05f);
            yield return new WaitForSeconds(delay);
        }

        yield return new WaitUntil(() => FindObjectsOfType<Enemy>().Length == 0);

        waveActive = false;
        AddGold(15 + wave * 5);
        waveText.text = "Dalga " + wave + " Bitti! +" + (15 + wave * 5) + "G";
        startWaveBtn.interactable = true;
    }

    EnemyType ChooseType()
    {
        float r = Random.value;
        if (wave <= 2) return EnemyType.Normal;
        if (wave <= 4) return r < 0.55f ? EnemyType.Normal : (r < 0.8f ? EnemyType.Fast : EnemyType.Tank);
        return r < 0.35f ? EnemyType.Normal : (r < 0.65f ? EnemyType.Fast : EnemyType.Tank);
    }

    void SpawnEnemy(EnemyType type)
    {
        var go = new GameObject("Enemy_" + type);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 3;

        var e = go.AddComponent<Enemy>();
        e.enemyType = type;

        float scale = 1f;
        switch (type)
        {
            case EnemyType.Normal:
                e.maxHealth = 80f + wave * 18f; e.speed = 2f;   e.goldReward = 10; e.damage = 1;
                sr.color = new Color(0.85f, 0.3f, 0.3f);
                break;
            case EnemyType.Fast:
                e.maxHealth = 45f + wave * 9f;  e.speed = 4.2f; e.goldReward = 8;  e.damage = 1;
                sr.color = new Color(0.95f, 0.75f, 0.1f);
                scale = 0.75f;
                break;
            case EnemyType.Tank:
                e.maxHealth = 280f + wave * 50f; e.speed = 1f;  e.goldReward = 25; e.damage = 3;
                sr.color = new Color(0.4f, 0.45f, 0.9f);
                scale = 1.3f;
                break;
        }
        go.transform.localScale = Vector3.one * scale;
        sr.sprite = MakeEnemySprite(type);

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.35f;

        e.SetPath(enemyPath);
    }

    // ── Tower interaction ────────────────────────────────────────────────────

    public void OnTowerClicked(Tower t)
    {
        if (_selected != null && _selected != t)
            _selected.SetRangeVisible(false);

        _selected = t;
        _selected.SetRangeVisible(true);
        RefreshUpgradePanel();
    }

    void Deselect()
    {
        if (_selected != null) _selected.SetRangeVisible(false);
        _selected = null;
        if (upgradePanel != null) upgradePanel.SetActive(false);
    }

    void RefreshUpgradePanel()
    {
        if (upgradePanel == null || _selected == null) return;
        upgradePanel.SetActive(true);

        string name = _selected.towerType == TowerType.Archer ? "Okçu" :
                      _selected.towerType == TowerType.Mage   ? "Büyücü" : "Şövalye";
        upgradeTitleText.text = name + "  Lv." + _selected.level;

        if (_selected.level < 3)
            upgradeCostText.text = "Geliştir: " + _selected.upgradeCost + " G";
        else
            upgradeCostText.text = "Maksimum Seviye";

        upgradeBtn.interactable = _selected.level < 3;
    }

    public void UpgradeSelected()
    {
        if (_selected == null) return;
        if (_selected.Upgrade())
            RefreshUpgradePanel();
        else
            Debug.Log("Geliştirme başarısız (altın yetersiz veya max level).");
    }

    public void SellSelected()
    {
        if (_selected == null) return;
        int refund = TowerCosts[(int)_selected.towerType] / 2 * _selected.level;
        AddGold(refund);
        allTowers.Remove(_selected);
        Destroy(_selected.gameObject);
        Deselect();
    }

    // ── Economy ──────────────────────────────────────────────────────────────

    public void AddGold(int amount)  { gold += amount; UpdateHUD(); }

    public bool SpendGold(int amount)
    {
        if (gold < amount) return false;
        gold -= amount; UpdateHUD(); return true;
    }

    public void TakeDamage(int dmg)
    {
        lives -= dmg; UpdateHUD();
        if (lives <= 0) GameOver();
    }

    void UpdateHUD()
    {
        if (goldText  != null) goldText.text  = "💰 " + gold;
        if (livesText != null) livesText.text = "❤️ " + Mathf.Max(0, lives);
        if (waveText  != null && !waveActive) waveText.text = "Dalga: " + wave;
    }

    void GameOver()
    {
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = "OYUN BİTTİ\nDalga: " + wave;
        }
        Time.timeScale = 0f;
    }

    public void SetPath(List<Vector3> p) { enemyPath = p; }

    // ── Sprite builders ──────────────────────────────────────────────────────

    Sprite MakeTowerSprite(TowerType type)
    {
        int s = 16;
        var tex = new Texture2D(s, s);
        Color fill = Color.white;
        ClearTex(tex, s);

        if (type == TowerType.Archer)
        {
            FillRect(tex, 4, 0, 8, 11, fill);
            FillRect(tex, 3, 11, 3, 5, fill);
            FillRect(tex, 7, 11, 3, 5, fill);
            FillRect(tex, 11, 11, 2, 5, fill);
        }
        else if (type == TowerType.Mage)
        {
            FillRect(tex, 4, 0, 8, 10, fill);
            for (int y = 10; y < 16; y++)
            {
                int m = y - 10;
                FillRect(tex, 4 + m, y, Mathf.Max(1, 8 - m * 2), 1, fill);
            }
        }
        else // Knight
        {
            FillRect(tex, 2, 0, 12, 6, fill);
            FillRect(tex, 4, 6, 8, 8, fill);
            FillRect(tex, 6, 13, 4, 3, fill);
        }

        tex.filterMode = FilterMode.Point;
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.2f), 16f);
    }

    Sprite MakeEnemySprite(EnemyType type)
    {
        int s = 12;
        var tex = new Texture2D(s, s);
        Color fill = Color.white;
        ClearTex(tex, s);

        if (type == EnemyType.Normal)
        {
            FillRect(tex, 3, 2, 6, 6, fill);
            FillRect(tex, 4, 8, 4, 3, fill);
        }
        else if (type == EnemyType.Fast)
        {
            FillRect(tex, 4, 0, 4, 12, fill);
            FillRect(tex, 2, 4, 8, 4, fill);
        }
        else
        {
            FillRect(tex, 1, 2, 10, 7, fill);
            FillRect(tex, 3, 9, 6, 3, fill);
        }

        tex.filterMode = FilterMode.Point;
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 12f);
    }

    void ClearTex(Texture2D tex, int s)
    {
        for (int x = 0; x < s; x++)
            for (int y = 0; y < s; y++)
                tex.SetPixel(x, y, Color.clear);
    }

    void FillRect(Texture2D tex, int x0, int y0, int w, int h, Color c)
    {
        for (int x = x0; x < x0 + w && x < tex.width;  x++)
        for (int y = y0; y < y0 + h && y < tex.height; y++)
            tex.SetPixel(x, y, c);
    }
}
