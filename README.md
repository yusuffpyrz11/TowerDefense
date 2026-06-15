# 🏰 Kingdom Tower Defense

Unity 2022.3 LTS ile yapılmış 2D Tower Defense oyunu.

## Özellikler

### Kuleler
| Kule | Maliyet | Hasar | Menzil | Hız | Özellik |
|------|---------|-------|--------|-----|---------|
| 🏹 Okçu | 50G | 25 | 3.5 | Hızlı | Tek hedef |
| 🔮 Büyücü | 80G | 40 | 3.0 | Orta | Alan hasarı (splash) |
| ⚔️ Şövalye | 100G | 35 | 1.2 | Çok Hızlı | Yakın alan hasarı |

Her kule 3. seviyeye kadar geliştirilebilir (hasar x1.5, menzil +0.3).

### Düşmanlar
| Düşman | Can | Hız | Ödül | Özellik |
|--------|-----|-----|------|---------|
| 🔴 Normal | 80+ | 2 | 10G | Dengeli |
| 🟡 Hızlı | 40+ | 4 | 8G | Kırılgan ama hızlı |
| 🔵 Tank | 250+ | 1 | 25G | Yavaş ama çok sağlam, 2 can götürür |

> Her dalgada düşmanların canı artar. 3. dalgadan itibaren karışık düşmanlar gelir.

---

## Kurulum

### Gereksinimler
- **Unity 2022.3 LTS** (diğer 2022.x sürümleri de çalışır)

### Adımlar

1. Bu repoyu klonlayın:
   ```bash
   git clone https://github.com/kullanici_adi/TowerDefense.git
   ```

2. **Unity Hub** → **Open** → proje klasörünü seçin

3. Unity projeyi import edecek (birkaç dakika sürebilir)

4. `Assets/Scenes/` altında **SampleScene** açın (boş kalabilir)

5. Hierarchy'de sağ tık → **Create Empty** → adını `GameBootstrap` yapın

6. Inspector'da **Add Component** → `SceneBuilder` ekleyin

7. ▶️ **Play** tuşuna basın — oyun otomatik kurulur!

---

## Nasıl Oynanır

| Eylem | Kontrol |
|-------|---------|
| Kule seç | Alt bardaki düğmeler |
| Kule yerleştir | Yeşil alana sol tık |
| İptalet | Sağ tık |
| Kule geliştir/sat | Kuleye tık → sağ panel |
| Dalga başlat | "Dalgayı Başlat" düğmesi |

**İpuçları:**
- Büyücü, Tank'lara karşı etkilidir (alan hasarı)
- Okçu, Hızlı düşmanlar için idealdir (uzun menzil)
- Şövalye yolu üzerine koyun, girişi kilitlesin
- Geliştirmek yeni kule almaktan çok daha verimlidir

---

## Proje Yapısı

```
TowerDefense/
├── Assets/
│   └── Scripts/
│       ├── Enemy.cs           # Düşman AI, hareket, can sistemi
│       ├── Tower.cs           # Kule saldırı, menzil, geliştirme
│       ├── Projectile.cs      # Mermi hareketi ve çarpma
│       ├── GameManager.cs     # Oyun durumu, altın, can, dalgalar
│       ├── SceneBuilder.cs    # Sahne + UI oluşturucu (prefab gerektirmez)
│       └── TowerClickHandler.cs # Kule tıklama
├── Packages/
│   └── manifest.json
├── ProjectSettings/
│   └── ProjectVersion.txt
└── README.md
```

---

## GitHub'a Yükleme

```bash
cd TowerDefense
git init
echo "Library/" >> .gitignore
echo "Temp/" >> .gitignore
echo "obj/" >> .gitignore
echo "Logs/" >> .gitignore
echo "*.csproj" >> .gitignore
echo "*.sln" >> .gitignore
git add .
git commit -m "Initial commit: Kingdom Tower Defense"
git remote add origin https://github.com/kullanici_adi/TowerDefense.git
git push -u origin main
```

---

## Genişletme Fikirleri

- Farklı haritalar / path layoutları
- Özel kule animasyonları (Animator)
- Ses efektleri (AudioSource)
- Highscore kaydı (PlayerPrefs)
- Daha fazla düşman tipi (Uçan, Zırhı olan)
