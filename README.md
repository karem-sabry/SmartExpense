# SmartExpense API

A full-featured expense tracking REST API built with .NET 8 and Clean Architecture. Track your spending, set budgets, manage recurring transactions, and get detailed analytics on your finances.

## Why I Built This

I wanted to build something practical that demonstrates solid backend development skills. Most expense trackers focus on the frontend, but I wanted to show I can design and implement a complete API with proper architecture, testing, and real-world features like auto-generation and analytics.

## What It Does

**Core Features:**
- 🔐 JWT authentication with refresh tokens
- 💸 Full transaction management with filtering and pagination
- 💰 Monthly budgets with tracking and alerts
- 🔄 Recurring transactions (monthly rent, weekly subscriptions, etc.)
- 📊 Financial analytics and spending trends
- 👥 Multi-user support with role-based access

**The Cool Stuff:**
- Automatic transaction generation for recurring expenses
- Budget performance tracking with "on track" indicators
- Category breakdown with percentages for spending analysis
- Month-over-month comparisons to see financial trends
- Comprehensive financial overview dashboard

## Tech Stack

- **.NET 8** - Latest C# features and performance improvements
- **ASP.NET Core Web API** - RESTful API design
- **Entity Framework Core 8** - Code-first database approach
- **SQL Server** - Relational database
- **JWT Authentication** - Secure token-based auth
- **xUnit, Moq, FluentAssertions** - 130 unit tests with 90%+ coverage
- **Swagger/OpenAPI** - API documentation and testing

## Architecture

I used Clean Architecture to keep things organized and maintainable:

```
SmartExpense.Api          → Controllers, Middleware
SmartExpense.Application  → DTOs, Service Interfaces
SmartExpense.Infrastructure → Repositories, Services, Data Access
SmartExpense.Core         → Domain Entities, Business Rules
SmartExpense.Tests        → Unit Tests
```

**Design Patterns:**
- Repository Pattern for data access
- Unit of Work for transaction management
- Dependency Injection throughout
- Custom exception handling with global error handler
- Audit interceptor for tracking created/updated records

## API Endpoints

**45 endpoints across 6 feature areas:**

### Authentication (8 endpoints)
- Register, Login, Logout
- Refresh token
- Password reset
- Email confirmation

### Transactions (7 endpoints)
- CRUD operations
- Advanced filtering (by date, category, amount, type)
- Pagination and sorting
- Recent transactions
- Financial summaries

### Budgets (6 endpoints)
- Monthly budget management
- Budget vs actual tracking
- Budget status (under/approaching/exceeded)
- Monthly summaries

### Recurring Transactions (8 endpoints)
- Set up recurring patterns (daily, weekly, monthly, yearly)
- Automatic generation
- Pause/resume
- Prevents duplicates

### Analytics (6 endpoints)
- Financial overview
- Spending trends (daily/weekly/monthly)
- Category breakdown with percentages
- Month-over-month comparisons
- Budget performance
- Top spending categories

### Admin (5 endpoints)
- User management
- Role assignment

Plus a health check endpoint for monitoring.

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server (or SQL Server LocalDB)
- Your favorite IDE (I use Rider, but VS Code or Visual Studio work great)

### Installation

1. Clone the repo
```bash
git clone https://github.com/karem-sabry/SmartExpense.git
cd SmartExpense
```

2. Set up user secrets (don't put sensitive data in appsettings.json!)
```bash
cd SmartExpense.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_CONNECTION_STRING"
dotnet user-secrets set "JwtOptions:Secret" "your-super-secret-key-at-least-32-characters"
```

3. Run migrations
```bash
cd ../SmartExpense.Infrastructure
dotnet ef database update --startup-project ../SmartExpense.Api
```

4. Run the API
```bash
cd ../SmartExpense.Api
dotnet run
```

5. Open Swagger UI
   Navigate to `https://localhost:7xxx/swagger` (check console for exact port)

## Testing

The project has 130 unit tests covering all major functionality:

```bash
cd SmartExpense.Tests
dotnet test
```

Tests cover:
- Service layer business logic
- Repository queries and filtering
- Controller responses
- Error handling and validation
- Edge cases and boundary conditions

## What I Learned

Building this taught me a lot about:
- Designing a clean API architecture that's easy to maintain
- Writing testable code (dependency injection is your friend)
- Handling authentication properly with JWTs and refresh tokens
- Working with EF Core for complex queries
- Creating algorithms (the recurring transaction auto-generation was fun)
- Writing meaningful tests that actually catch bugs

The hardest part was getting the recurring transaction generation right - figuring out the date calculations and making sure we don't create duplicates took some thinking.

## What I'd Add Next

If I keep working on this (or use it as a base for a real project):
- Export transactions to CSV or Excel
- Import transactions from bank statements
- Shared budgets for families/roommates
- Email notifications for budget alerts
- Mobile app using this as the backend
- Docker containerization
- CI/CD pipeline

## Project Stats

- **130 unit tests** with 90%+ code coverage
- **45 REST endpoints** across 5 feature modules
- **5 entities** with proper relationships
- **Clean Architecture** with clear separation of concerns
- **0 warnings** on build

## Why Clean Architecture?

I chose Clean Architecture because it:
- Makes the code easier to test (no tight coupling)
- Keeps business logic separate from infrastructure
- Makes it easier to swap out parts (like changing from SQL Server to PostgreSQL)
- Is a pattern used in real production codebases

It's a bit more setup than throwing everything in one project, but it pays off when the codebase grows.

## Contact

**Karem Sabry**

- GitHub: [@karem-sabry](https://github.com/karem-sabry)
- LinkedIn: [karem-sabry](https://www.linkedin.com/in/karem-sabry/)
- Email: karemsabry2013@gmail.com

Feel free to reach out if you have questions or want to discuss the project!

## License

This project is open source and available under the MIT License.

---

Built with ☕ and a lot of unit tests.