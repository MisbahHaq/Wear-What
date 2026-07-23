# Nexora

Nexora is an ASP.NET Core MVC 8.0 e-commerce application being incrementally converted into a Web API backend for future consumption by a React frontend. The existing MVC views and session-based auth remain functional during the migration.

## What the Project Does

The application provides a complete shopping workflow:

- **Product Catalog**
  - Browse men's, women's, and vault collections.
  - View product details, images, pricing, stock, tags, sizes, and colors.
  - Search products through autocomplete suggestions.

- **Shopping Cart and Checkout**
  - Add products to cart with size and color options.
  - Store cart items in session for guests and in the database for logged-in users.
  - Checkout with delivery and payment method selection.
  - Required checkout fields for phone number, country/region, city, and address.
  - Editable checkout address that is saved back to the user profile/session.
  - Create orders with order items and show an order confirmation page.

- **Customer Accounts**
  - Login, signup, profile management, username updates, and address updates.
  - Session-based authentication using `UserEmail`, `UserName`, `UserAddress`, and `IsAdmin`.
  - Customer order history from the profile page.

- **Bookmarks**
  - Logged-in users can bookmark products.
  - Bookmarks are stored in the database and linked to the user email.

- **Chatbot and Customer Support**
  - In-site chatbot for order status, cancellation requests, and admin chat.
  - Cancellation requests and admin chat messages are saved as `SupportRequest` records.
  - Cancel Order and Chat With Admin actions require the customer to be logged in.
  - Logged-in users receive admin replies in the chatbot through a reply polling endpoint.
  - Admin can mark support requests as read, reply to requests, resolve them, or cancel related orders.

- **Admin Dashboard**
  - Admin login and dashboard overview.
  - Product management: create, edit, delete, and view products.
  - Order management: view orders, update order status, and inspect order details.
  - Support management: view all chatbot requests, unread requests, cancellation requests, and respond to customers.

## Technology Stack

- **Backend**: ASP.NET Core MVC 8.0 (being migrated to Web API)
- **Language**: C#
- **Frontend**: Razor Views, HTML, CSS, Bootstrap, JavaScript (existing); React (planned)
- **Database**: PostgreSQL (Npgsql)
- **ORM**: Entity Framework Core 8.0
- **Build Tool**: .NET CLI / MSBuild
- **API Docs**: Swagger / Swashbuckle

## API Conversion Status

Controllers are being converted to API controllers in isolated steps. Session-based auth is preserved during migration.

| Controller | Status | Notes |
|---|---|---|
| `ProductsController` | **API + Views** | `api/Products` for API; MVC views for product listing/details/admin |
| `OrdersController` | **API** | `api/Orders` — checkout, order list/detail, status updates |
| `AccountController` | **API + Views** | `api/Account` for API; MVC views for login/register/profile |
| `CartController` | **API + Views** | `api/Cart` for API; MVC views for cart/checkout |
| `AdminController` | MVC | Admin views; order/product endpoints will move to API |
| `BookmarkController` | MVC | To be converted |
| `ChatbotController` | MVC / JSON | Already returns JSON; to be converted |
| `HomeController` | MVC | To be removed or replaced after React migration |

## Main Project Structure

```text
Nexora/
  Controllers/       MVC and API controllers
  Data/              EF Core context, initializer, and migrations
  Models/
    Dtos.cs          API DTOs for Products and Orders
    *.cs             Domain models and view models
  Views/             Razor views (being phased out)
  wwwroot/           Static assets
  appsettings.json   Application configuration and database connection string
```

## API Endpoints (Converted So Far)

### Products (`api/Products`)

| Method | Route | Description |
|---|---|---|
| GET | `/api/Products` | List all products, optional `?gender=` filter |
| GET | `/api/Products/{id}` | Get single product |
| GET | `/api/Products/{id}/related` | Get related products by tag |
| POST | `/api/Products` | Create product (admin) |
| PUT | `/api/Products/{id}` | Update product (admin) |
| DELETE | `/api/Products/{id}` | Delete product (admin) |

### Orders (`api/Orders`)

| Method | Route | Description |
|---|---|---|
| GET | `/api/Orders` | List orders (admin: all; customer: own) |
| GET | `/api/Orders/{id}` | Get single order |
| POST | `/api/Orders/checkout` | Create order from cart |
| POST | `/api/Orders/{id}/status` | Update order status (admin) |

### Account (`api/Account`)

| Method | Route | Description |
|---|---|---|
| POST | `/api/Account/login` | Login, sets session values |
| POST | `/api/Account/register` | Register new user, sets session values |
| POST | `/api/Account/logout` | Clears session |
| GET | `/api/Account/profile` | Get current user profile (requires login) |
| PUT | `/api/Account/profile` | Update username/address (requires login) |
| GET | `/api/Account/check-auth` | Check if user is authenticated |

### Cart (`api/Cart`)

| Method | Route | Description |
|---|---|---|
| GET | `/api/Cart` | Get current cart (guest session or logged-in DB) |
| POST | `/api/Cart/add` | Add product to cart |
| PUT | `/api/Cart/update` | Update cart item quantity |
| DELETE | `/api/Cart/{cartItemId}` | Remove cart item |
| DELETE | `/api/Cart/clear` | Clear entire cart |
| POST | `/api/Cart/merge` | Merge guest session cart into logged-in user's DB cart |

## UI Pages (MVC Views During Migration)

The application currently serves both API endpoints and MVC views. The views act as a temporary frontend until the React app is ready.

| Page | Route | Notes |
|---|---|---|
| Home | `/` | Landing page with best sellers |
| Men | `/Home/Men` | Men's product catalog |
| Women | `/Home/Women` | Women's product catalog |
| Vault | `/Home/Vault` | Vault collection |
| Search | `/Home/Search?q=` | Product search |
| Product Details | `/Products/Details/{id}` | Single product page |
| Login | `/Account/Login` | Session-based login |
| Sign Up | `/Account/SignUp` | Create new account |
| Profile | `/Account/Profile` | View/edit profile (requires login) |
| Cart | `/Cart/Index` | Shopping cart |
| Checkout | `/Cart/Checkout` | Checkout form (requires login) |
| Order Confirmation | `/Cart/OrderConfirmation/{id}` | After successful checkout |
| Bookmarks | `/Bookmark/Index` | Saved products (requires login) |

**Swagger UI**: `https://localhost:<port>/swagger`

> Note: Session-based cookies work automatically in Swagger UI because it shares the same origin as the API. For a future React client on a different origin, cross-origin credential handling may require additional cookie configuration (`SameSite=None; Secure`) when browser flow demands it.

## Database Models

The application uses these main entities:

- `Product`
- `Order`
- `OrderItem`
- `User`
- `ShoppingCart`
- `Bookmark`
- `SupportRequest`

EF Core `DbSet`s are configured in `Nexora/Data/ApplicationDbContext.cs`.

## Authentication and Authorization

The app uses ASP.NET Core session state for authentication instead of ASP.NET Identity. This is preserved during the API migration.

Important session values include:

- `UserEmail`
- `UserName`
- `UserAddress`
- `AuthToken`
- `IsAdmin`

Customer-only actions check for `UserEmail`. Admin-only actions check for `IsAdmin == "true"`.

CORS is configured to allow a future React client running on `localhost:5173` or `localhost:3000`.

## Chatbot Support Flow

The chatbot supports three main actions:

1. **Order Status**
   - Customer enters an order number.
   - The app returns the current order status.

2. **Cancel Order**
   - Customer must be logged in.
   - Customer enters an order number and cancellation reason.
   - The app creates a `SupportRequest` with type `Cancellation`.
   - Admin can review and cancel the order from the admin panel.

3. **Chat With Admin**
   - Customer must be logged in.
   - Customer sends a message, optionally with an order number.
   - The app creates a `SupportRequest` with type `AdminChat`.
   - Admin can reply from the admin panel.
   - Customer receives admin replies in the chatbot.

## Admin Features

Admin users can access:

- Dashboard statistics
- Product list
- Product create/edit/delete pages
- Order list and order details
- Order status updates
- Support request lists
- Unread support requests
- Cancellation requests
- Reply to customer messages
- Resolve support requests
- Cancel orders from chatbot cancellation requests

## Getting Started

### Prerequisites

- .NET 8 SDK
- PostgreSQL running locally or accessible via connection string
- Visual Studio, VS Code, or another C# editor

### Restore Packages

```bash
dotnet restore Nexora/Nexora.csproj
```

### Configure Database

The project uses PostgreSQL via Npgsql. Edit the connection string in `Nexora/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=NexoraDb;Username=postgres;Password=yourpassword"
}
```

### Apply Migrations

```bash
dotnet ef database update --project Nexora/Nexora.csproj
```

If EF tools are not installed globally:

```bash
dotnet tool install --global dotnet-ef
```

### Run the Application

```bash
dotnet run --project Nexora/Nexora.csproj
```

Then open:
- `https://localhost:<port>/swagger` — API documentation and testing
- `https://localhost:<port>` — existing MVC frontend (if still present)

## Default Admin Login

The admin account is handled as a special session-based login case:

```text
Email: admin@represent.com
Password: Qwerty123
```

## Common Commands

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build "Nexora.sln"

# Run the app
dotnet run --project Nexora/Nexora.csproj

# Add a migration
dotnet ef migrations add MigrationName --project Nexora/Nexora.csproj

# Update the database
dotnet ef database update --project Nexora/Nexora.csproj
```

## Notes

- Build output folders such as `bin/` and `obj/` are generated by the .NET SDK.
- Existing nullable reference warnings are from older project code and do not currently block builds.
- Product images and store content are managed through the admin product pages.
- The MVC migration is being done incrementally. Existing controllers that have not yet been converted to API remain fully functional.
