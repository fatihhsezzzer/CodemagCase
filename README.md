# GS1 L3 Serialization & Aggregation System

## 📋 Proje Açıklaması

Bu proje, GS1 L3 standartlarına uygun bir seri numarası üretimi ve agregasyon sistemidir. Ürünlerin benzersiz seri numaralarıyla izlenmesini, koli ve palet seviyesinde agregasyonunu destekler.

## 🏗️ Mimari

Proje, Clean Architecture prensiplerine uygun olarak 4 katmandan oluşmaktadır:

```
┌─────────────────────────────────────────────────────────────┐
│                     GS1.WinClient                           │
│              (Windows Forms - Presentation)                 │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      GS1.WebAPI                             │
│                (ASP.NET Core Web API)                       │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                   GS1.Infrastructure                        │
│        (EF Core, Repositories, Services)                    │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                       GS1.Core                              │
│           (Domain Entities, Interfaces, DTOs)               │
└─────────────────────────────────────────────────────────────┘
```

## 🔧 Teknolojiler

- **.NET 8.0** - Ana framework
- **ASP.NET Core Web API** - REST API backend
- **Entity Framework Core 8.0** - ORM
- **MS SQL Server** - Veritabanı
- **Windows Forms** - Desktop istemci
- **Serilog** - Structured logging
- **FluentValidation** - Input validation
- **Swagger/OpenAPI** - API dokümantasyonu

## 📦 GS1 Standartları

Sistem aşağıdaki GS1 Application Identifier'larını destekler:

| AI  | Açıklama                              | Format      |
| --- | ------------------------------------- | ----------- |
| 01  | GTIN (Global Trade Item Number)       | 14 haneli   |
| 21  | Serial Number                         | Alfanümerik |
| 17  | Expiry Date                           | YYMMDD      |
| 10  | Batch/Lot Number                      | Alfanümerik |
| 00  | SSCC (Serial Shipping Container Code) | 18 haneli   |

### SSCC Hesaplama

SSCC formatı: `00` + Extension Digit + GS1 Company Prefix (7 hane) + Serial Reference (9 hane) + Check Digit

Check digit, Mod10 algoritması kullanılarak hesaplanır.

## 📊 Veritabanı Şeması

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│  Customers   │────<│   Products   │────<│  WorkOrders  │
└──────────────┘     └──────────────┘     └──────────────┘
                                                  │
                          ┌───────────────────────┼───────────────────────┐
                          │                       │                       │
                          ▼                       ▼                       ▼
                   ┌──────────────┐       ┌──────────────┐       ┌──────────────┐
                   │SerialNumbers │       │ SSCCs (Box)  │       │SSCCs (Pallet)│
                   └──────────────┘       └──────────────┘       └──────────────┘
                          │                       │
                          └───────────────────────┘
                            (Agregasyon İlişkisi)
```

## 🚀 Kurulum

### Gereksinimler

- .NET 8.0 SDK
- SQL Server (LocalDB veya Express)
- Visual Studio 2022 veya VS Code

### 1. Veritabanı Bağlantı Dizesi

`src/GS1.WebAPI/appsettings.json` dosyasındaki connection string'i kendi SQL Server örneğinize göre güncelleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GS1_Serialization;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 2. Veritabanı Migration

```bash
cd src/GS1.WebAPI
dotnet ef database update --project ../GS1.Infrastructure/GS1.Infrastructure.csproj
```


### 3. API'yi Başlatma

```bash
cd src/GS1.WebAPI
dotnet run
```


### 4. Windows Client'ı Başlatma

```bash
cd src/GS1.WinClient
dotnet run
```

## 📡 API Endpoints

### Müşteriler (Customers)

| Method | Endpoint            | Açıklama                |
| ------ | ------------------- | ----------------------- |
| GET    | /api/customers      | Tüm müşterileri listele |
| GET    | /api/customers/{id} | Müşteri detayı          |
| POST   | /api/customers      | Yeni müşteri oluştur    |
| PUT    | /api/customers/{id} | Müşteri güncelle        |
| DELETE | /api/customers/{id} | Müşteri sil             |

### Ürünler (Products)

| Method | Endpoint                     | Açıklama             |
| ------ | ---------------------------- | -------------------- |
| GET    | /api/products                | Tüm ürünleri listele |
| GET    | /api/products/{id}           | Ürün detayı          |
| GET    | /api/products/by-gtin/{gtin} | GTIN ile ürün bul    |
| POST   | /api/products                | Yeni ürün oluştur    |
| PUT    | /api/products/{id}           | Ürün güncelle        |
| DELETE | /api/products/{id}           | Ürün sil             |

### İş Emirleri (Work Orders)

| Method | Endpoint                      | Açıklama                             |
| ------ | ----------------------------- | ------------------------------------ |
| GET    | /api/workorders               | Tüm iş emirlerini listele            |
| GET    | /api/workorders/{id}          | İş emri detayı                       |
| GET    | /api/workorders/{id}/details  | İş emri tam detay (agregasyon dahil) |
| POST   | /api/workorders               | Yeni iş emri oluştur                 |
| PUT    | /api/workorders/{id}          | İş emri güncelle                     |
| POST   | /api/workorders/{id}/start    | İş emrini başlat                     |
| POST   | /api/workorders/{id}/complete | İş emrini tamamla                    |

### Seri Numaraları (Serial Numbers)

| Method | Endpoint                                      | Açıklama                       |
| ------ | --------------------------------------------- | ------------------------------ |
| GET    | /api/serialnumbers/by-workorder/{workOrderId} | İş emrine göre seri numaraları |
| POST   | /api/serialnumbers/generate                   | Seri numarası üret             |
| POST   | /api/serialnumbers/generate-batch             | Toplu seri numarası üret       |
| POST   | /api/serialnumbers/{id}/print                 | Yazdırıldı olarak işaretle     |

### Agregasyon (Aggregation)

| Method | Endpoint                                        | Açıklama                             |
| ------ | ----------------------------------------------- | ------------------------------------ |
| POST   | /api/aggregation/box                            | Koli oluştur (ürünleri koliye ekle)  |
| POST   | /api/aggregation/pallet                         | Palet oluştur (kolileri palete ekle) |
| GET    | /api/aggregation/sscc/{ssccCode}                | SSCC içeriğini görüntüle             |
| POST   | /api/aggregation/box/{ssccCode}/disaggregate    | Koli agregasyonunu çöz               |
| POST   | /api/aggregation/pallet/{ssccCode}/disaggregate | Palet agregasyonunu çöz              |

## 💡 Kullanım Senaryoları

### 1. Yeni Ürün Seri Numarası Üretimi

```http
POST /api/serialnumbers/generate-batch
{
    "workOrderId": 1,
    "quantity": 100
}
```

### 2. Koli Oluşturma (Agregasyon)

```http
POST /api/aggregation/box
{
    "workOrderId": 1,
    "serialNumberIds": [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]
}
```

### 3. Palet Oluşturma

```http
POST /api/aggregation/pallet
{
    "workOrderId": 1,
    "boxSsccIds": [1, 2, 3, 4, 5]
}
```

## 🔐 Güvenlik

- Validation: FluentValidation ile tüm input'lar doğrulanmaktadır

## 📝 Logging

Serilog ile yapılandırılmış logging:

- Console output (development)
- File output: `logs/gs1-serialization-.log` (rolling daily)

## 📁 Proje Yapısı

```
GS1_Serialization/
├── src/
│   ├── GS1.Core/                 # Domain katmanı
│   │   ├── Entities/             # Domain entity'leri
│   │   ├── Enums/                # Enum tanımları
│   │   ├── Interfaces/           # Repository ve servis arayüzleri
│   │   ├── DTOs/                 # Data Transfer Objects
│   │   └── Exceptions/           # Özel exception'lar
│   │
│   ├── GS1.Infrastructure/       # Altyapı katmanı
│   │   ├── Data/                 # DbContext ve konfigürasyonlar
│   │   ├── Services/             # Servis implementasyonları
│   │   ├── Repositories/         # Repository implementasyonları
│   │   └── Migrations/           # EF Core migration'ları
│   │
│   ├── GS1.WebAPI/               # Web API katmanı
│   │   ├── Controllers/          # API controller'ları
│   │   └── Middleware/           # Custom middleware'ler
│   │
│   └── GS1.WinClient/            # Windows Forms istemci
│       ├── Forms/                # Form'lar
│       └── Services/             # API istemci servisleri
│
├── GS1_Serialization.sln         # Solution dosyası
└── README.md                     # Bu dosya
```

## 🔄 Geliştirme Notları

- Entity Framework Core migration'ları `GS1.Infrastructure/Migrations` klasöründe bulunur
