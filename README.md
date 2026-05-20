# NextJS Tarayıcı

<div align="center">

![Version](https://img.shields.io/badge/version-1.0.0-blue?style=flat-square)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey?style=flat-square)
![.NET](https://img.shields.io/badge/.NET-10.0-purple?style=flat-square)
![WebView2](https://img.shields.io/badge/WebView2-required-orange?style=flat-square)
![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)

**Next.js yerel sunucuları için geliştirilmiş özel Windows tarayıcısı.**  
Geliştirme sürecinizi hızlandırmak için tasarlanmış, minimal ve şık bir araç.

</div>

---

## Ekran Görüntüsü

```
[ ⚙  ‹  ›  ↻  ·  </>  ⛶  ⤓ ]   [ 🌐  http://localhost:3000  %100 ]
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                         [ WebView2 İçerik Alanı ]
```

---

## Özellikler

- **Dark Tema** — Göze yormayan koyu arayüz, Windows 11 yuvarlak köşeler
- **Akıllı URL Bar** — Odaklanınca mavi çerçeve, 🔒/🌐 protokol ikonu
- **Yükleme Çubuğu** — Sayfa yüklenirken ince progress bar
- **Zoom Kontrolü** — %50 ile %200 arası 10 seviye, başlık çubuğunda gösterge
- **Tray Entegrasyonu** — X butonu tray'e küçültür, çift tıkla geri getir
- **Ayarlar Modalı** — Dark temalı, custom scroll, kayıt defterine kalıcı kayıt
- **Oto Yenileme** — Sunucu hazır olana kadar belirtilen aralıkta yeniden dener
- **Pencere Hafızası** — Boyut ve konum otomatik kaydedilir
- **WebView2 Kontrolü** — Kurulu değilse hata ekranı ve indirme yönlendirmesi

---

## Gereksinimler

| Gereksinim | Minimum |
|---|---|
| İşletim Sistemi | Windows 10 (64-bit) |
| .NET Runtime | 10.0 *(self-contained builds için gerekmez)* |
| Microsoft WebView2 | Kurulu olmalı |

> **Not:** Windows 11 kullanıcıları için WebView2, Microsoft Edge ile birlikte varsayılan olarak yüklüdür.  
> Windows 10 kullanıcıları için: [WebView2 Runtime İndir](https://developer.microsoft.com/microsoft-edge/webview2/)

---

## Kurulum

### Hazır Binary

1. [Releases](https://github.com/Scotuch/nextjs-tarayici/releases) sayfasından son sürümü indir
2. `publish` klasörünü istediğin bir yere kopyala
3. `NextJS_Tarayici.exe` dosyasını çalıştır

### Kaynak Koddan Derleme

```bash
# Repoyu klonla
git clone https://github.com/Scotuch/nextjs-tarayici.git
cd nextjs-tarayici

# Tek komutla derle ve yayınla
dotnet publish -c Release
```

Çıktı: `bin\Release\net10.0-windows\win-x64\publish\`

---

## Kullanım

### Temel Kullanım

Uygulamayı başlat, `http://localhost:3000` otomatik açılır.  
Açılış URL'ini **Ayarlar (F1)** üzerinden değiştirebilirsin.

### Komut Satırı Parametreleri

```bash
NextJS_Tarayici.exe [parametreler]
```

| Parametre | Kısa | Açıklama |
|---|---|---|
| `--url <adres>` | `-url` | Belirtilen URL ile başlat |
| `-fullscreen` | `-f` | Tam ekran başlat |
| `-tray` | `-min` | Simge durumunda (tray) başlat |

**Örnekler:**

```bash
# Farklı port ile aç
NextJS_Tarayici.exe -url localhost:4000

# Arka planda başlat
NextJS_Tarayici.exe -tray

# Farklı URL, tam ekran
NextJS_Tarayici.exe -url localhost:3001 -fullscreen
```

> `http://` yazmasan da otomatik eklenir. Parametreler birleştirilebilir, sıra önemli değil.

---

## Klavye Kısayolları

| Kısayol | Eylem |
|---|---|
| `F1` | Ayarları aç |
| `F11` | Tam ekran aç / kapat |
| `F12` | Geliştirici araçları |
| `Ctrl + R` | Sayfayı yenile |
| `Ctrl + L` | URL çubuğuna odaklan |
| `Ctrl + +` | Yakınlaştır |
| `Ctrl + −` | Uzaklaştır |
| `Ctrl + 0` | Zoom sıfırla (%100) |
| `Alt + ←` | Önceki sayfa |
| `Alt + →` | Sonraki sayfa |

---

## Ayarlar

**F1** veya araç çubuğundaki ⚙ ikonuna tıklayarak ayarlara ulaşabilirsin.

| Bölüm | Seçenekler |
|---|---|
| **Başlangıç** | Windows ile başlat, tray'e başlat, tam ekran başlat, açılış URL |
| **Görünüm** | Varsayılan zoom seviyesi |
| **Davranış** | X tray'e küçültsün, oto yenile, yenileme aralığı, kapatma onayı |
| **Gizlilik** | Önbellek temizle, çerezleri temizle, çıkışta temizle |
| **Kısayollar** | Tüm klavye kısayollarının listesi |

Tüm ayarlar Windows Kayıt Defteri'nde (`HKCU\Software\NextJSTarayici`) saklanır.

---

## Tray Davranışı

| Eylem | Sonuç |
|---|---|
| **X butonu** *(varsayılan)* | Tray'e küçülür, arka planda çalışır |
| **Tray → Çift tıkla** | Pencereyi geri getirir |
| **Tray → Sağ tıkla → Kapat** | Uygulamayı tamamen kapatır |
| **X butonu** *(ayardan değiştirilirse)* | Uygulamayı doğrudan kapatır |

---

## Proje Yapısı

```
NextJS_Tarayici/
├── Form1.cs                 # Ana pencere — mantık ve olaylar
├── Form1.Designer.cs        # Toolbar, URL bar, buton yerleşimi
├── SettingsForm.cs          # Ayarlar modalı (custom dark scroll)
├── AppSettings.cs           # Kayıt defteri kayıt/okuma modeli
├── Program.cs               # Giriş noktası, WebView2 kontrolü
├── app.ico                  # Uygulama ikonu (7 boyut)
└── NextJS_Tarayici.csproj   # Proje ve publish yapılandırması
```

---

## Teknolojiler

- **[.NET 10](https://dotnet.microsoft.com/)** — Windows Forms
- **[Microsoft WebView2](https://developer.microsoft.com/microsoft-edge/webview2/)** — Chromium tabanlı web motoru
- **GDI+** — Custom çizimler (yuvarlak URL bar, grup kutuları, scroll)
- **Windows Registry** — Ayar kalıcılığı
- **DWM API** — Windows 11 yuvarlak köşeler

---

## Geliştirici

**Samed CIMEN (Scotuch)**  
[github.com/scotuch](https://github.com/scotuch)

---

## Lisans

MIT License — Dilediğin gibi kullanabilir, değiştirebilir ve dağıtabilirsin.

---

<div align="center">
  <sub>Next.js geliştirme deneyimini iyileştirmek için ❤️ ile yapıldı.</sub>
</div>
