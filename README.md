# Kurum Arşivi
## 1. Proje Bilgileri
* **Ders:** Yazılım Geliştirme Laboratuvarı-II / Proje-1
* **Proje Adı:** Kurum Arşivi
* **Teslim Tarihi:** 05.04.2026
* **Ekip Üyeleri:**
  * Şevval Ceren Yıldız
  * Sudenaz Güldal
---
## 2. Giriş

### Problemin Tanımı
Kurumsal ortamlarda belgelerin yönetimi, aranması ve güvenli erişimi büyük önem taşımaktadır. Geleneksel dosya sistemleri ve tek parçalı uygulamalar, ölçeklenebilirlik ve bakım açısından ciddi sorunlar yaratmaktadır.

### Amaç
Bu proje, kurumsal belgelerin dijital ortamda güvenli bir şekilde saklanmasını, yönetilmesini ve aranmasını sağlayan mikroservis tabanlı bir arşiv sistemi geliştirmeyi amaçlamaktadır.

### Kapsam
Sistem aşağıdaki bileşenlerden oluşmaktadır:
- **Dispatcher (API Gateway):** Tüm dış isteklerin tek giriş noktası
- **Login Service:** Kullanıcı kimlik doğrulama ve yetkilendirme
- **Document Service:** Belge ekleme, silme, listeleme işlemleri
- **Search Service:** Belge arama ve indeksleme
- **Frontend:** Kullanıcı arayüzü
