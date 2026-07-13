# MultiVendor

A multi-vendor e-commerce web application built with **ASP.NET Core 8 (MVC)**. It lets multiple sellers run their own storefronts ("shops") within a single marketplace, while buyers browse products, manage a cart, place orders, and interact with sellers via comments, wishlists, and follows.

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
- **Authentication & authorization** — ASP.NET Core Identity with roles (`Admin`, seller, customer), cookie-based auth, and a custom claims principal that adds the user's full name to the identity.

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
Shop/
├── Controllers/      # MVC controllers (Account, Admin, Cart, Categories, Comments,
│                     #   Home, Orders, Products, Profile, Shops, ShopFollow, Wishlist)
├── Models/           # Entity models (Product, ShopItem, Order, OrderItem,
│                     #   WishlistItem, ProductComment, ApplicationUser, etc.)
├── Data/             # ApplicationDbContext and DbInitializer (seeding)
├── Views/            # Razor views per controller + Shared layouts
├── Components/       # Razor components (BentoGrid)
├── wwwroot/          # Static assets (CSS, JS, images)
├── Program.cs        # App startup: services, Identity, session, routing
├── Shop.csproj       # Project file (targets net8.0)
└── appsettings.json  # Connection string + configuration
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

# Apply database migrations (creates the ShopDbNew database)
dotnet ef database update

# Run the app
dotnet run
```

The app seeds initial data (roles/users) via `DbInitializer` on startup. The default connection string is configured in `appsettings.json`:

```
Server=(localdb)\mssqllocaldb;Database=ShopDbNew;Trusted_Connection=true
```

## Configuration notes

- Authentication cookie paths: login `/Account/Login`, logout `/Account/Logout`, access denied `/Account/AccessDenied`.
- Session idle timeout: 1 hour; cookie is `HttpOnly` and marked essential.
- Sensitive actions are protected with `[Authorize]`, role checks (`Admin`), and ownership checks (`OwnerId` vs current user).
