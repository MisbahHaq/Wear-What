# MultiVendor

A multi-vendor e-commerce marketplace split into three separate deployable web apps — **Customer**, **Vendor**, and **Admin** — sharing one database and one ASP.NET Core Identity user store.

## What this project does

- **Multi-vendor marketplace** — each registered user can own one or more shops and list products under them.
- **Product management** — create, edit, and delete products with multiple images, categories, dynamic specifications, and color options. Owners can only manage their own products.
- **Categories** — products are organized by shop categories for browsing and filtering.
- **Shopping cart** — session-based cart (add, update, remove) that persists across requests without requiring a database table.
- **Checkout & orders** — buy a single product or the whole cart; orders support Standard/Express delivery, Cash-on-Delivery or Card payment, and an order confirmation page. Cart checkout splits the order per shop (one order per vendor).
- **Seller order dashboard** — shop owners see incoming orders for their products, view total sales, and update order status.
- **Wishlist** — users can save products they like (stored per user/product).
- **Product comments** — buyers can leave comments/reviews on product pages.
- **Shop following** — users can follow shops to keep track of sellers.
- **User profiles** — extended identity profile with full name, address, CNIC, and contact number.
- **Admin dashboard** — role-restricted area showing counts of products, shops, categories, and users.
- **Authentication & authorization** — ASP.NET Core Identity with roles (`Admin`, `Vendor`, `Customer`), cookie-based auth, and a custom claims principal that adds the user's full name to the identity.

## Tech stack

| Area | Technology |
|------|------------|
| Framework | .NET 8 / ASP.NET Core 8 |
| UI pattern | MVC (server-rendered Razor views) |
| Real-time UI components | Razor Components (`BentoGrid.razor`) |
| Data access | Entity Framework Core 8 |
| Database | SQL Server (LocalDB via `localdb\mssqllocaldb`) |
| Authentication | ASP.NET Core Identity (with `IdentityRole`, `IdentityUI`) |
| Session state | In-memory distributed session (`AddDistributedMemoryCache` + `AddSession`) |
| Languages | C#, HTML, CSS, Razor (`.cshtml`) |
| Tooling | EF Core migrations (`Microsoft.EntityFrameworkCore.Tools`) |

## Project structure

```
MultiVendor/
├── MultiVendor.slnx                    # Solution file
├── MultiVendor.Core/                   # Class Library — shared across all apps
│   ├── Models/                         # Entities (Product, ShopItem, Order, etc.) + shared ViewModels
│   ├── Data/                           # ApplicationDbContext, DbInitializer
│   ├── Identity/                       # CustomUserClaimsPrincipalFactory, AddMultiVendorIdentity extension
│   └── Migrations/                     # ALL EF Core migrations (shared database schema)
│
├── MultiVendor.Customer/               # ASP.NET Core MVC Web App — port 5276
│   ├── Controllers/                    # Buyer-facing: Home, Products[browse], Cart, Orders[checkout],
│   │                                 #   Shops[browse], Wishlist, Comments, Profile, Addresses,
│   │                                 #   Notifications, Coupons, Ratings, ShopFollow, Account
│   ├── Views/                          # Razor views for buyer flows
│   ├── Models/AccountViewModels.cs     # App-specific login/register/profile view models
│   ├── Program.cs                      # No global auth filter — anonymous browsing allowed
│   └── appsettings.json                # Connection string
│
├── MultiVendor.Vendor/                 # ASP.NET Core MVC Web App — port 5001
│   ├── Controllers/                    # Seller-facing: Products[CRUD], Orders[Dashboard/Analytics],
│   │                                 #   Shops[CRUD], Account
│   ├── Views/                          # Razor views for seller flows
│   └── Program.cs                      # Global [Authorize(Roles="Vendor")] filter
│
└── MultiVendor.Admin/                  # ASP.NET Core MVC Web App — port 5002
    ├── Controllers/                    # Admin-facing: Admin, Categories, Account
    ├── Views/                          # Razor views for admin flows
    └── Program.cs                      # Global [Authorize(Roles="Admin")] filter
```

## Key entities

- **ApplicationUser** (extends `IdentityUser`): `FullName`, `Address`, `CNIC`, `ContactNumber`, `ShopName`.
- **ShopItem**: a vendor storefront owned by a user (`OwnerId`).
- **Product**: belongs to a shop and a category; has price, images, specifications, colors, and comments.
- **Order / OrderItem**: placed by a customer; supports delivery method, payment method, delivery fee, and status.
- **WishlistItem / ShopFollow / ProductComment**: user interactions.

## Getting started

Prerequisites: [.NET 8 SDK](https://dotnet.microsoft.com/download) and SQL Server LocalDB (included with Visual Studio).

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build MultiVendor.slnx

# Run the Customer app (port 5276)
dotnet run --project MultiVendor.Customer/MultiVendor.Customer.csproj

# Run the Vendor app (port 5001) — in another terminal
dotnet run --project MultiVendor.Vendor/MultiVendor.Vendor.csproj

# Run the Admin app (port 5002) — in another terminal
dotnet run --project MultiVendor.Admin/MultiVendor.Admin.csproj
```

The app seeds initial data (roles/users) via `DbInitializer` on startup. The default connection string is configured in each app's `appsettings.json`:

```
Server=(localdb)\mssqllocaldb;Database=ShopDbNew;Trusted_Connection=true
```

> **Note:** All three apps share the same database and Identity tables. A user registered in one app can log into the others with the same credentials. Each app has its own cookie auth session for now — cross-app SSO is not implemented yet.

### Seeded accounts

| Email | Password | Role | App |
|-------|----------|------|-----|
| `admin@shop.com` | `Admin@123` | Admin | Admin |
| `vendor@shop.com` | `Vendor@123` | Vendor | Vendor |
| `customer@shop.com` | `Customer@123` | Customer | Customer |

## Architecture notes

- **Shared Identity registration**: `MultiVendor.Core/Identity/MultiVendorIdentityServiceCollectionExtensions.cs` contains a single `AddMultiVendorIdentity()` extension method. Each app's `Program.cs` calls this to register the shared `ApplicationDbContext` and Identity services. This makes adding shared-cookie SSO later a configuration change rather than a redesign.
- **Migrations**: EF Core migrations live in `MultiVendor.Core/Migrations/` only. Web apps do not generate their own migrations.
- **Role-gated access**: Each app enforces role access via a global `[Authorize(Roles = "...")]` filter in `Program.cs`, not per-controller.
- **Shared services**: Business logic used by multiple apps should live in `MultiVendor.Core/Services/` as injectable services.

## Configuration notes

- Authentication cookie paths per app: login `/Account/Login`, logout `/Account/Logout`, access denied `/Account/AccessDenied`.
- Session idle timeout: 1 hour; cookie is `HttpOnly` and marked essential.
- Sensitive actions are protected with `[Authorize]`, role checks (`Admin`/`Vendor`), and ownership checks (`OwnerId` vs current user).
