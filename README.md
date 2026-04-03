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
 
 #### Uygulanan REST örnekleri

- `POST /api/auth/login` → kullanıcı girişi
- `POST /api/auth/register` → yeni kullanıcı oluşturma
- `GET /api/auth/users` → kullanıcıları listeleme
- `DELETE /api/auth/users/{username}` → kullanıcı silme
- `PUT /api/auth/users/{username}/role` → kullanıcı rolü güncelleme
- `GET /api/documents` → belgeleri listeleme
- `GET /api/documents/{id}` → tek belge getirme
- `POST /api/documents` → belge oluşturma
- `PUT /api/documents/{id}` → belge güncelleme
- `DELETE /api/documents/{id}` → belge silme
- `GET /api/search?q=...` → belge arama
- `POST /api/search/index` → indeks oluşturma/güncelleme
- `DELETE /api/search/index/{documentId}` → arama indeksinden belge silme

Bu yapı sayesinde proje, `.../deleteUser?id=1` gibi RPC benzeri tasarım yerine kaynak odaklı REST yaklaşımını kullanmaktadır.

### Mikroservis Mimarisi
```mermaid
graph TD
    Frontend -->|HTTP| Dispatcher
    Dispatcher -->|/api/auth| LoginService
    Dispatcher -->|/api/documents| DocumentService
    Dispatcher -->|/api/search| SearchService
    LoginService --> MongoDB_Login
    DocumentService --> MongoDB_Document
    DocumentService -->|JSON ile indeksleme isteği| SearchService
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


## Network Isolation

Bu projede mikroservisler arası güvenlik ve erişim kontrolü için **network isolation** yaklaşımı uygulanmıştır. Amaç, sistemde dış dünyaya yalnızca **Dispatcher (API Gateway)** servisinin açık olması; Login, Document ve Search mikroservislerinin ise yalnızca Docker iç ağı üzerinden erişilebilir tutulmasıdır.

### Uygulanan yapı

- **Dispatcher**
  - Hem `public-net` hem `internal-net` ağına bağlıdır.
  - Host makineye `5000:8080` port eşlemesi ile açılmıştır.
  - Sistemin dış dünyaya açık olan ana backend giriş noktasıdır.

- **Login Service**
  - Yalnızca `internal-net` ağına bağlıdır.
  - Host makineye publish edilmiş bir portu yoktur.

- **Document Service**
  - Yalnızca `internal-net` ağına bağlıdır.
  - Host makineye publish edilmiş bir portu yoktur.

- **Search Service**
  - Yalnızca `internal-net` ağına bağlıdır.
  - Host makineye publish edilmiş bir portu yoktur.

- **MongoDB servisleri**
  - Her servis için ayrı MongoDB container’ı kullanılmıştır.
  - Bu veritabanı servisleri de yalnızca `internal-net` ağı üzerinde çalışmaktadır.

Bu tasarım sayesinde dış istemciler mikroservislere doğrudan erişemez. Tüm dış istekler önce Dispatcher’a gelir, Dispatcher da uygun mikroservise yönlendirme yapar.

### Docker Compose düzeyinde izolasyon

Network isolation, Docker Compose yapılandırmasında aşağıdaki prensiplerle sağlanmıştır:

1. **Dispatcher hem public hem internal ağa bağlıdır.**
2. **Mikroservisler yalnızca internal ağda tanımlanmıştır.**
3. **Login, Document ve Search servisleri için host port publish edilmemiştir.**
4. **MongoDB servisleri de dış dünyaya açılmamıştır.**

Bu nedenle:
- dış istemci yalnızca Dispatcher’a erişebilir,
- mikroservisler yalnızca Docker iç ağı üzerinden haberleşir,
- veri tabanı katmanı dış erişime kapalı tutulur.

### Kanıtlayıcı ekran görüntüleri

#### 1. Servislerin port görünümü (`docker compose ps`)
Aşağıdaki çıktıda yalnızca Dispatcher servisinin host port eşlemesine sahip olduğu, diğer mikroservislerin ise yalnızca container iç portları ile çalıştığı görülmektedir.
<img width="1693" height="184" alt="network" src="https://github.com/user-attachments/assets/67ea0e38-b4ae-488d-b7ec-e2fdae9c9691" />



**Şekil Açıklaması:**  
`docker compose ps` çıktısında Dispatcher için `5000->8080` port eşlemesi görünürken, `login-service`, `document-service` ve `search-service` için host port publish edilmediği görülmektedir. Bu durum mikroservislerin doğrudan dış erişime kapalı tutulduğunu göstermektedir.

---

#### 2. Dispatcher servis yapılandırması
Aşağıdaki yapılandırmada Dispatcher servisinin hem `public-net` hem `internal-net` üzerinde çalıştığı ve `5000:8080` port eşlemesi ile dış erişime açıldığı görülmektedir.


<img width="643" height="623" alt="disp-network1" src="https://github.com/user-attachments/assets/acfa605d-a035-4024-aae7-c089a7a841d8" />


**Şekil Açıklaması:**  
Dispatcher servisi hem dış ağ hem iç ağ arasında köprü görevi görmekte ve sistemin tek backend giriş noktası olarak çalışmaktadır.

---

#### 3. Login Service yapılandırması
Aşağıdaki yapılandırmada Login Service’in yalnızca `internal-net` üzerinde tanımlandığı ve host’a publish edilmiş bir portunun bulunmadığı görülmektedir.

<img width="620" height="263" alt="login-net1" src="https://github.com/user-attachments/assets/67b73c7a-dd0d-45dd-a6b3-4597d102bf27" />



**Şekil Açıklaması:**  
Login Service yalnızca iç ağda çalışmakta, dış dünyaya doğrudan açılmamaktadır.

---

#### 4. Document Service yapılandırması
Aşağıdaki yapılandırmada Document Service’in yalnızca `internal-net` üzerinde çalıştığı görülmektedir.


<img width="622" height="262" alt="doc-net" src="https://github.com/user-attachments/assets/a994d752-8aed-4cce-9008-b2f174769dd7" />

**Şekil Açıklaması:**  
Document Service host makineye publish edilmemiştir; bu nedenle yalnızca Docker iç ağı üzerinden erişilebilir.

---

#### 5. Search Service yapılandırması
Aşağıdaki yapılandırmada Search Service’in yalnızca `internal-net` ağı üzerinde bulunduğu görülmektedir.
<img width="601" height="243" alt="serach-net" src="https://github.com/user-attachments/assets/11b551b6-3b28-402f-b504-3dd306b3fe27" />



**Şekil Açıklaması:**  
Search Service dış istemcilere doğrudan açık değildir ve sadece iç ağ üzerinden erişilir.

### Sonuç

Bu projede network isolation, Docker Compose ağ yapısı ve port publish tercihleriyle sağlanmıştır. Dispatcher dış dünyaya açık tek backend bileşeni olarak konumlandırılmış; Login, Document ve Search servisleri ile bunlara ait MongoDB bileşenleri yalnızca iç ağ üzerinde tutulmuştur. Böylece mikroservis mimarisinde güvenlik, erişim kontrolü ve merkezi trafik yönetimi hedeflerine uygun bir yapı elde edilmiştir.
