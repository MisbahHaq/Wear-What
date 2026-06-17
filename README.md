# Multi-Vendor E-Commerce Platform — ASP.NET Core 9

## Architecture Decision: Web API + Razor Pages Hybrid

This implementation uses a **hybrid approach**:
- **ASP.NET Core Web API** (`[ApiController]`) for all JSON endpoints (Shop API, Products API, Orders API, Follow API). This cleanly separates the data contract and allows a future SPA (React/Vue/Angular) or mobile app.
- **Razor Pages / MVC Views** in the `Vendor` Area for the vendor dashboard with server-rendered HTML.
- This is strategically cleaner than pure MVC because the customer-facing frontend can be decoupled into its own project without rewriting vendor logic.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 9 |
| ORM | Entity Framework Core 9 |
| Database | SQL Server (LocalDB default; easily switchable to PostgreSQL) |
| Auth | ASP.NET Core Identity + Cookie Auth |
| RBAC | Roles: Admin, Vendor, Customer |
| Serialization | System.Text.Json (default in .NET 9) |

---

## Domain Model Summary

```
ApplicationUser (IdentityUser)
├── FirstName, LastName, IsVendor flag
├── 1:N → Shop (owner)
├── 1:N → Follow (follower)
└── 1:N → Order (customer)

Shop
├── OwnerId → ApplicationUser
├── 1:N → Product
├── 1:N → Follow (followed by users)
└── 1:N → ShopOrder

Product
├── ShopId → Shop
├── Sku (unique)
└── 1:N → OrderItem

Follow (junction table: User ↔ Shop)
├── FollowerId → ApplicationUser (unique per Shop)
├── ShopId → Shop
└── Composite Unique Index: (FollowerId, ShopId)

Order (master customer order)
├── CustomerId → ApplicationUser
├── OrderNumber (unique, indexed)
├── Status enum: Pending/Confirmed/Processing/Shipped/Delivered/Cancelled
├── Financials: SubTotal, Tax, Shipping, Discount, GrandTotal
└── 1:N → ShopOrder

ShopOrder (per-vendor sub-order within a master Order)
├── OrderId → Order
├── ShopId → Shop
├── VendorOrderNumber (unique, indexed)
├── Status enum: Pending/Accepted/Processing/Shipped/Delivered/Cancelled
└── 1:N → OrderItem

OrderItem
├── OrderId → Order
├── ShopOrderId → ShopOrder
├── ProductId → Product (snapshot: ProductNameSnapshot, ProductImageSnapshot)
├── Quantity, UnitPrice, LineTotal
```

---

## Multi-Vendor Checkout Flow (Single Transaction)

```
Cart (items from multiple shops)
    │
    ▼
Validate stock for all items
    │
    ▼
Group items by ShopId
    │
    ▼
BEGIN TRANSACTION
    │
    ├──► Create Order (master)
    │
    ├──► For each Shop group:
    │       ├──► Create ShopOrder (with VendorOrderNumber)
    │       ├──► Create OrderItems
    │       └──► DECREMENT Product.Stock
    │
    ├──► Calculate GrandTotal (+ shipping if multi-shop)
    │
    ▼
COMMIT TRANSACTION
```

---

## Role-Based Access Summary

| Role | Permissions |
|---|---|
| **Admin** | All operations |
| **Vendor** | Create own Shop; CRUD own Products; View own ShopOrders; Accept/Ship vendor orders |
| **Customer (any user)** | Browse shops/products; Follow/unfollow shops; Checkout multi-vendor orders; View own order history |

A single user can hold both `Vendor` and `Customer` roles simultaneously.

---

## Key Design Decisions

1. **No Cart table** — Cart is managed client-side (or optionally via session) and resolved at checkout. Simplifies data model; items are validated + grouped on checkout.
2. **Snapshot fields in OrderItem** — `ProductNameSnapshot` and `ProductImageSnapshot` are stored so order history remains accurate even if the vendor later edits or deletes the product.
3. **Soft-delete for Products** via `IsActive` flag. Preserves order history integrity.
4. **Followable relationship** uses a dedicated `Follow` entity (not many-to-many without payload) to allow future extensions (e.g., `NotifyOnNewProduct` flag).
5. **VendorOrderNumber** independently auto-generated per `ShopOrder` for vendor-facing reconciliation.

---

## Running the Project

```bash
cd src/ECommercePlatform
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

The API will be available at `https://localhost:5001/api/shops`, `/api/products`, `/api/orders`.
The Vendor area at `https://localhost:5001/Vendor/Dashboard`.

Seeded admin account: `admin@ecommerce.local` / `Admin@123!`
