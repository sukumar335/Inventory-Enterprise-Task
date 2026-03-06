# StockMaster (Inventory Enterprise Task)

A robust, enterprise-grade Inventory Management System built with **ASP.NET Core MVC 8** and **PostgreSQL**. The application features a clean N-Tier architecture, a modern dashboard UI built with **Tailwind CSS**, secure Cookie Authentication, and comprehensive entity auditing mechanisms.

---

## 🚀 Key Features

*   **Dashboards & UI:** A sleek, fully responsive modern dashboard design crafted with native HTML, Tailwind CSS, and embedded SVGs mimicking premium UI dashboards.
*   **Inventory Management (CRUD):** 
    *   Add, list, update, and manage inventory stock exactly as requested.
    *   Tracks complex attributes: `SKU`, `Name`, `Category`, `Price`, and `Quantity`.
    *   Status badges dynamically compute and visually represent stock levels (In Stock, Low Stock, Out of Stock).
*   **Secure Authentication:** Solid, straightforward Cookie-based Authentication flow including Registration and Login portals seamlessly integrated into the custom modern branding.
*   **Advanced Database Handling (EF Core & PostgreSQL):** 
    *   Automatically tracks granular audit meta-data across all tables globally via a custom `AppDbContext` implementation (`CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`, `DeletedAt`, `DeletedBy`).
    *   **Soft Deletes:** Deleting records safely sets an `IsDeleted` flag maintaining historical data without permanently destroying crucial database records.
*   **Structured Logging:** Deeply integrated with **Serilog**; automatically rotates and aggregates log files directly into the host machine's `Documents/inventory enterprise log/` directory partitioned by date.

---

## 🛠️ Technology Stack

*   **Framework:** .NET 8, ASP.NET Core MVC
*   **Database:** PostgreSQL 
*   **ORM:** Entity Framework Core
*   **Styling:** Tailwind CSS (via CDN)
*   **Authentication:** .NET Cookie Authentication Scheme (`Microsoft.AspNetCore.Authentication.Cookies`)
*   **Logging:** Serilog (`Serilog.AspNetCore`, Console & File sinks)

---

## 📂 Project Architecture (N-Tiered Approach)

The application has been modularized by concern:
1.  **Core (`/Core`):** Holds pure Domain Entities (e.g., `InventoryItem`, `User`) and standard Interfaces (e.g., `IInventoryRepository`, `IInventoryService`).
2.  **Infrastructure (`/Infrastructure`):** The data-access layer containing `AppDbContext` and concrete Repository implementations, wiring closely to Entity Framework Core.
3.  **Services (`/Services`):** Business logic layer; connects controllers to repositories and manipulates domain data logic (e.g., password hashing flows during active Authentication).
4.  **Presentation (Web):** The Controllers and Razor Views (`.cshtml`) orchestrating how the user acts seamlessly with the application through MVC patterns.

---

## ⚙️ Getting Started

### Prerequisites
*   [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
*   [PostgreSQL](https://www.postgresql.org/download/) installed and actively running.

### 1. Configure the Database
Open `appsettings.json` and adjust the PostgreSQL connection string pointing to your active PostgreSQL credentials and server:
```json
"ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=InventoryEnterpriseDb;Username=postgres;Password=YOUR_PASSWORD"
}
```

### 2. Run Migrations
Run the initial EF Core migrations to scaffold out the required PostgreSQL schema. Open your terminal at the root directory and execute:
```bash
dotnet ef database update
```

### 3. Run the Application
Finally, restore dependencies, build and run the application:
```bash
dotnet run
```
Navigate to the specified `localhost` port printed in the terminal to view the application. Navigate directly to `/Account/Register` to set up your primary credentials, then explore the `StockMaster` dashboard!

# Inventory Enterprise Task

![Stars](https://img.shields.io/github/stars/sukumar335/Inventory-Enterprise-Task)
![Forks](https://img.shields.io/github/forks/sukumar335/Inventory-Enterprise-Task)
![Watchers](https://img.shields.io/github/watchers/sukumar335/Inventory-Enterprise-Task)
