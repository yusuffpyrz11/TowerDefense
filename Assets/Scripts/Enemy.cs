using UnityEngine;
using System.Collections.Generic;

public enum EnemyType { Normal, Fast, Tank }

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public EnemyType enemyType = EnemyType.Normal;
    public float maxHealth = 100f;
    public float speed = 2f;
    public int goldReward = 10;
    public int damage = 1;

    [HideInInspector] public float currentHealth;
    [HideInInspector] public List<Vector3> path;
    [HideInInspector] public int pathIndex = 0;

    private Transform hpFillTransform;
    private bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        CreateHealthBar();
    }

    void CreateHealthBar()
    {
        // Background (red)
        var bgGo = new GameObject("HpBg");
        bgGo.transform.SetParent(transform);
        bgGo.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        bgGo.transform.localScale = new Vector3(0.65f, 0.1f, 1f);
        var bgSr = bgGo.AddComponent<SpriteRenderer>();
        bgSr.sprite = MakeSolidSprite(Color.red);
        bgSr.sortingOrder = 10;

        // Fill (green)
        var fillGo = new GameObject("HpFill");
        fillGo.transform.SetParent(bgGo.transform);
        fillGo.transform.localPosition = new Vector3(0f, 0f, -0.01f);
        fillGo.transform.localScale = Vector3.one;
        var fillSr = fillGo.AddComponent<SpriteRenderer>();
        fillSr.sprite = MakeSolidSprite(new Color(0.1f, 0.9f, 0.2f));
        fillSr.sortingOrder = 11;
        hpFillTransform = fillGo.transform;
    }

    Sprite MakeSolidSprite(Color c)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, c);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    }

    void Update()
    {
        if (isDead) return;
        MoveAlongPath();
    }

    void MoveAlongPath()
    {
        if (path == null || pathIndex >= path.Count) return;

        Vector3 target = path[pathIndex];
        target.z = 0f;
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.08f)
        {
            pathIndex++;
            if (pathIndex >= path.Count)
                ReachedEnd();
        }
    }

    void ReachedEnd()
    {
        if (isDead) return;
        isDead = true;
        GameManager.Instance.TakeDamage(damage);
        Destroy(gameObject);
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;
        currentHealth -= dmg;
        RefreshHealthBar();
        if (currentHealth <= 0f) Die();
    }

    void RefreshHealthBar()
    {
        if (hpFillTransform == null) return;
        float pct = Mathf.Clamp01(currentHealth / maxHealth);
        hpFillTransform.localScale = new Vector3(pct, 1f, 1f);
        hpFillTransform.localPosition = new Vector3((pct - 1f) * 0.5f, 0f, -0.01f);
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        GameManager.Instance.AddGold(goldReward);
        Destroy(gameObject);
    }

    public void SetPath(List<Vector3> p)
    {
        path = new List<Vector3>(p);
        pathIndex = 0;
        if (path.Count > 0)
            transform.position = new Vector3(path[0].x, path[0].y, 0f);
    }
}
