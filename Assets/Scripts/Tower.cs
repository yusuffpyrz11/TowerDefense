using UnityEngine;

public enum TowerType { Archer, Mage, Knight }

public class Tower : MonoBehaviour
{
    public TowerType towerType = TowerType.Archer;
    public float range = 3f;
    public float fireRate = 1f;
    public float damage = 20f;
    public int level = 1;
    public int upgradeCost = 50;

    private float cooldown = 0f;
    private Enemy target;
    private SpriteRenderer sr;
    private GameObject rangeIndicator;
    private bool showRange = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        BuildRangeCircle();
    }

    void Update()
    {
        cooldown -= Time.deltaTime;
        AcquireTarget();

        if (target != null && cooldown <= 0f)
        {
            Fire();
            cooldown = 1f / fireRate;
        }
    }

    void AcquireTarget()
    {
        // Validate existing target
        if (target != null && target.gameObject != null &&
            Vector3.Distance(transform.position, target.transform.position) <= range)
            return;

        target = null;
        float bestProgress = -1f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range);
        foreach (var h in hits)
        {
            var e = h.GetComponent<Enemy>();
            if (e == null) continue;
            float prog = e.pathIndex;
            if (prog > bestProgress)
            {
                bestProgress = prog;
                target = e;
            }
        }
    }

    void Fire()
    {
        if (towerType == TowerType.Knight)
        {
            // Melee splash
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range);
            foreach (var h in hits)
            {
                var e = h.GetComponent<Enemy>();
                if (e != null) e.TakeDamage(damage);
            }
        }
        else
        {
            SpawnProjectile();
        }
    }

    void SpawnProjectile()
    {
        if (target == null) return;
        var go = new GameObject("Proj");
        go.transform.position = transform.position;

        var proj = go.AddComponent<Projectile>();
        proj.target = target;
        proj.damage = damage;
        proj.isSplash = (towerType == TowerType.Mage);
        proj.splashRadius = 1.2f;

        var psr = go.AddComponent<SpriteRenderer>();
        Color pc = towerType == TowerType.Mage ? new Color(0.8f, 0.3f, 1f) : new Color(1f, 0.8f, 0.1f);
        psr.sprite = MakeCircleSprite(pc, 8);
        psr.sortingOrder = 6;
        float sz = towerType == TowerType.Mage ? 0.28f : 0.18f;
        go.transform.localScale = Vector3.one * sz;
    }

    // ── Public API ──────────────────────────────────────────────────────────

    public bool Upgrade()
    {
        if (level >= 3) return false;
        if (!GameManager.Instance.SpendGold(upgradeCost)) return false;
        level++;
        damage   *= 1.5f;
        range    += 0.3f;
        fireRate *= 1.25f;
        upgradeCost = (int)(upgradeCost * 1.8f);
        RefreshVisual();
        BuildRangeCircle(); // rebuild with new range
        return true;
    }

    void RefreshVisual()
    {
        if (sr == null) return;
        Color c = GetTowerColor();
        float b = 0.15f * (level - 1);
        sr.color = new Color(Mathf.Clamp01(c.r + b), Mathf.Clamp01(c.g + b), Mathf.Clamp01(c.b + b));
    }

    public Color GetTowerColor()
    {
        switch (towerType)
        {
            case TowerType.Archer: return new Color(0.2f, 0.72f, 0.22f);
            case TowerType.Mage:   return new Color(0.5f, 0.2f,  0.85f);
            case TowerType.Knight: return new Color(0.75f, 0.72f, 0.15f);
            default: return Color.white;
        }
    }

    public void SetRangeVisible(bool v)
    {
        showRange = v;
        if (rangeIndicator != null) rangeIndicator.SetActive(v);
    }

    void BuildRangeCircle()
    {
        if (rangeIndicator != null) Destroy(rangeIndicator);
        rangeIndicator = new GameObject("Range");
        rangeIndicator.transform.SetParent(transform);
        rangeIndicator.transform.localPosition = Vector3.zero;

        var lr = rangeIndicator.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.startWidth = lr.endWidth = 0.04f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = new Color(1, 1, 1, 0.2f);
        lr.sortingOrder = -1;

        int seg = 36;
        lr.positionCount = seg + 1;
        for (int i = 0; i <= seg; i++)
        {
            float a = i / (float)seg * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * range, Mathf.Sin(a) * range, 0f));
        }
        rangeIndicator.SetActive(showRange);
    }

    // ── Sprite helpers ───────────────────────────────────────────────────────

    public static Sprite MakeCircleSprite(Color c, int size)
    {
        var tex = new Texture2D(size, size);
        float r = size / 2f;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                tex.SetPixel(x, y, d < r ? c : Color.clear);
            }
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
