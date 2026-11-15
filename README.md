# Flappy Bird

Boşluk tuşuyla zıplayıp boruların arasından geçtiğiniz bir Windows Forms oyunu. Her geçilen boru çifti bir puan kazandırır. Boruya veya zemine çarpınca oyun durur.

## Teknolojiler

- C# ve .NET 10
- Windows Forms ve `System.Windows.Forms.Timer`

## Kurulum ve çalıştırma

Windows ve .NET 10 SDK gerekir. Depoyu yerel olarak açıp kök klasörde şu komutları çalıştırın:

```powershell
dotnet build FlappyBird.slnx
dotnet run --project FlappyBird/FlappyBird.csproj
```

`FlappyBird.slnx` dosyasını .NET 10 destekleyen Visual Studio ile de açabilirsiniz.

## Kontroller ve oyun kuralları

- **Boşluk:** Oyunu başlatır veya kuşu yukarı iter. Her zıplama için tuşa yeniden basın. Game Over ekranında Boşluk yeni turu başlatır.
- İki boru çifti sağdan sola ilerler. Ekrandan çıkan çift sağ tarafta yeni bir açıklıkla tekrar belirir.
- **Yeni oyun:** Oyun durumunu ve skoru sıfırlar. Yeni turu Boşluk tuşuyla başlatabilirsiniz.

## Tasarım ve sınırlamalar

`GameState` hareket, çarpışma, engeller ve skoru yönetir. Form klavye olaylarını alır, zamanlayıcıyla oyun durumunu günceller ve ekrana çizer. Çarpışma sınır dikdörtgenleriyle hesaplanır. Tur skoru her yeni oyunda sıfırlanır. Rekor, tur sonunda yükselirse Windows kullanıcı profilindeki `%LOCALAPPDATA%/FlappyBird/high-score.txt` dosyasına kaydedilir. Kayıt sorunu varsa oyun devam eder ve arayüzde durum gösterilir.

## Ekran görüntüleri

**Başlangıç**

![Başlangıç ekranı ve klavye komutu](docs/screenshots/start.png)

**Oyun sırasında**

![Kuş ve iki boru çifti](docs/screenshots/gameplay.png)

**Çarpışma sonrası**

![Game Over bildirimi](docs/screenshots/game-over.png)
