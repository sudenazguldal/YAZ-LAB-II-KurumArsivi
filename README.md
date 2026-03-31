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
---
## 3. Sistem Tasarımı

### Richardson Olgunluk Modeli (RMM)
Bu projede REST API tasarımında **Seviye 2** uygulanmıştır:

- **Seviye 0:** Tek endpoint, tek metot
- **Seviye 1:** Kaynaklar URI ile tanımlanır (`/api/documents`, `/api/auth`)
- **Seviye 2 (Uygulanan):** HTTP metodları doğru kullanılır:
  - `GET` → Listeleme/okuma
  - `POST` → Oluşturma
  - `PUT` → Güncelleme
  - `DELETE` → Silme
  - Uygun HTTP durum kodları: `200`, `201`, `204`, `401`, `403`, `404`, `409`

### Mikroservis Mimarisi
```mermaid
graph TD
    Frontend -->|HTTP| Dispatcher
    Dispatcher -->|/api/auth| LoginService
    Dispatcher -->|/api/documents| DocumentService
    Dispatcher -->|/api/search| SearchService
    LoginService --> MongoDB_Login
    DocumentService --> MongoDB_Document
    DocumentService -->|index| SearchService
    SearchService --> MongoDB_Search
    Dispatcher --> MongoDB_Dispatcher
```

### Sequence Diyagramı - Login
```mermaid
sequenceDiagram
    participant F as Frontend
    participant D as Dispatcher
    participant L as Login Service
    participant DB as MongoDB

    F->>D: POST /api/auth/login
    D->>L: Forward request
    L->>DB: Find user
    DB-->>L: User data
    L-->>D: JWT Token
    D-->>F: JWT Token
```

### Sequence Diyagramı - Belge Ekleme
```mermaid
sequenceDiagram
    participant F as Frontend
    participant D as Dispatcher
    participant Doc as Document Service
    participant S as Search Service
    participant DB as MongoDB

    F->>D: POST /api/documents (JWT)
    D->>D: Verify JWT
    D->>Doc: Forward request
    Doc->>DB: Save document
    Doc->>S: POST /api/search/index
    S->>DB: Index document
    Doc-->>D: 204 No Content
    D-->>F: 204 No Content
```
---
## 4. Proje Yapısı ve Modüller

### Klasör Yapısı
```mermaid
graph TD
    A[KurumArsivi] --> B[src]
    A --> C[frontend]
    A --> D[grafana]
    B --> E[Dispatcher.API]
    B --> F[Login.Service]
    B --> G[Document.Service]
    B --> H[Search.Service]
    E --> E1[Middlewares]
    F --> F1[Controllers]
    F --> F2[Services]
    F --> F3[Repositories]
    F --> F4[Models]
    G --> G1[Controllers]
    G --> G2[Services]
    G --> G3[Repositories]
    G --> G4[Models]
    G --> G5[DTOs]
```

### Servisler

| Servis | Port | Görev |
|--------|------|-------|
| Dispatcher | 5000 | API Gateway, JWT doğrulama, yönlendirme |
| Login Service | 8080 | Kullanıcı kayıt, giriş, JWT üretme |
| Document Service | 8080 | Belge CRUD işlemleri |
| Search Service | 8080 | Belge arama ve indeksleme |
| Frontend | 3000 | Kullanıcı arayüzü |
| Grafana | 3001 | Log görselleştirme |

### Docker Ağ Yapısı
```mermaid
graph LR
    subgraph public-net
        Frontend
        Dispatcher
        Grafana
    end
    subgraph internal-net
        Dispatcher
        LoginService
        DocumentService
        SearchService
        MongoDB_Login
        MongoDB_Document
        MongoDB_Search
        MongoDB_Dispatcher
        Loki
        Promtail
    end
```
---
