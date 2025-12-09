# Loan API - სესხების მართვის სისტემა

ASP.NET Core Web API პროექტი სესხების მოთხოვნების მართვისთვის JWT ავტორიზაციით და Role-Based Access Control-ით.

---

## შინაარსი

1. [პროექტის აღწერა](#პროექტის-აღწერა)
2. [ტექნოლოგიები](#ტექნოლოგიები)
3. [Setup ინსტრუქციები](#setup-ინსტრუქციები)
4. [არქიტექტურა](#არქიტექტურა)
5. [API Endpoints](#api-endpoints)
6. [Request/Response მაგალითები](#requestresponse-მაგალითები)
7. [ბიზნეს წესები](#ბიზნეს-წესები)
8. [Security](#security)
9. [ტესტების გაშვება](#ტესტების-გაშვება)
10. [Exception Handling](#exception-handling)
11. [Validation Rules](#validation-rules)
12. [Database Schema](#database-schema)
13. [Logging](#logging)

---

## პროექტის აღწერა

სესხების მართვის სისტემა, სადაც:
- მომხმარებლები (Users) რეგისტრირდებიან და სესხის მოთხოვნას აკეთებენ
- ბუღალტრები (Accountants) უმართავენ სესხებს და მომხმარებლებს

### მთავარი ფუნქციები

- მომხმარებლის რეგისტრაცია/ავტორიზაცია (JWT Token)
- Role-Based Authorization (User, Accountant)
- სესხის მოთხოვნა, განახლება, წაშლა
- Accountant-ის მიერ სესხების სტატუსის მართვა
- მომხმარებლების დაბლოკვა/განბლოკვა
- Global Exception Handling
- Request Validation (FluentValidation)
- Structured Logging (Serilog)
- Unit Tests (xUnit + Moq)

---

## ტექნოლოგიები

| ტექნოლოგია | ვერსია | დანიშნულება |
|------------|--------|--------------|
| .NET | 10.0 | Backend Framework |
| Entity Framework Core | 9.0 | ORM (Database Access) |
| SQL Server | 2019+ | Relational Database |
| JWT Bearer | 9.0 | Authentication & Authorization |
| BCrypt.Net | 4.0.3 | Password Hashing |
| Serilog | 8.0.3 | Structured Logging |
| FluentValidation | 11.11.0 | Input Validation |
| xUnit | 2.9.3 | Unit Testing Framework |
| Moq | 4.20.72 | Mocking Library |
| Swagger | 7.2.0 | API Documentation |

---

## Setup ინსტრუქციები

### 1. Prerequisites

დააინსტალირე შემდეგი:
- .NET 10 SDK (https://dotnet.microsoft.com/download)
- SQL Server 2019+ ან SQL Server Express
- (Optional) JetBrains Rider / Visual Studio 2022 / VS Code

### 2. Clone Repository
```bash
git clone <your-repository-url>
cd LoanApi
```

### 3. Database Configuration

შექმენი ან განაახლე `LoanApi/appsettings.json` ფაილი:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=LoanDb;Trusted_Connection=true;TrustServerCertificate=true"
  },
  "JwtSettings": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForSecurity!",
    "Issuer": "LoanApi",
    "Audience": "LoanApiUsers"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

**მნიშვნელოვანი:**
- `Server=.` ნიშნავს local SQL Server-ს
- თუ სხვა server გაქვს, შეცვალე connection string
- `JwtSettings:Key` უნდა იყოს მინიმუმ 32 სიმბოლო

### 4. Database Migration
```bash
cd LoanApi

# Migration-ის გაშვება (ბაზის შექმნა)
dotnet ef database update

# თუ მიგრაცია არ არსებობს, შექმენი:
dotnet ef migrations add InitialMigration
dotnet ef database update
```

### 5. Build & Run
```bash
# Build
dotnet build

# Run
dotnet run
```

**Application URLs:**
- API Base: http://localhost:5090
- Swagger UI: http://localhost:5090/swagger

---

## არქიტექტურა

### ფაილური სტრუქტურა
```
LoanApi/
├── Controllers/              # API Endpoints
│   ├── UserController.cs     # რეგისტრაცია, ლოგინი, მომხმარებლის ინფო
│   └── LoanController.cs     # სესხის CRUD ოპერაციები
│
├── Services/                 # Business Logic Layer
│   ├── UserService.cs        # მომხმარებლის ლოგიკა
│   ├── LoanService.cs        # სესხის ლოგიკა
│   └── JwtService.cs         # Token Generation
│
├── Interfaces/               # Abstractions (SOLID)
│   ├── IUserService.cs
│   ├── ILoanService.cs
│   └── IJwtService.cs
│
├── Models/                   # Domain Models & DTOs
│   ├── User.cs               # Entity Model
│   ├── Loan.cs               # Entity Model
│   └── DTOs/                 # Data Transfer Objects
│       ├── UserRegisterDto.cs
│       ├── UserLoginDto.cs
│       ├── LoanCreateDto.cs
│       └── ...
│
├── Data/                     # Database Context & Migrations
│   ├── LoanDbContext.cs      # EF Core DbContext
│   └── Migrations/           # Database Version Control
│
├── Validators/               # FluentValidation Rules
│   ├── UserRegisterDtoValidator.cs
│   ├── LoanCreateDtoValidator.cs
│   └── LoanUpdateDtoValidator.cs
│
├── Middleware/               # HTTP Pipeline Middleware
│   └── ExceptionMiddleware.cs  # Global Exception Handler
│
├── Enums/                    # Enumerations
│   └── Enum.cs               # LoanType, LoanStatus, UserRole
│
├── Logs/                     # Serilog Log Files
│   └── log-YYYYMMDD.txt
│
└── Program.cs                # Application Entry Point

LoanApi.Tests/                # Unit Tests Project
├── Services/
│   ├── UserServiceTests.cs
│   └── LoanServiceTests.cs
└── Controllers/
    ├── UserControllerTests.cs
    └── LoanControllerTests.cs
```

### SOLID პრინციპები

- **S - Single Responsibility:** თითოეული კლასი პასუხისმგებელია ერთ რამეზე
- **O - Open/Closed:** გახსნილია გაფართოებისთვის, დახურული შეცვლისთვის
- **L - Liskov Substitution:** ინტერფეისების იმპლემენტაციები ჩანაცვლებადია
- **I - Interface Segregation:** პატარა, სპეციფიკური ინტერფეისები
- **D - Dependency Inversion:** კლასები დამოკიდებულნი არიან Abstraction-ზე

---

## API Endpoints

### Authentication

**POST /api/user/register** - მომხმარებლის რეგისტრაცია
- Authorization: არ სჭირდება
- აკეთებს ახალი მომხმარებლის რეგისტრაციას
- პაროლს ჰეშავს BCrypt-ით
- Default როლი: User

**POST /api/user/login** - ავტორიზაცია
- Authorization: არ სჭირდება
- ამოწმებს credentials-ს და აბრუნებს JWT Token-ს
- Token-ის ვადა: 3 საათი

### User Management

**GET /api/user/{id}** - მომხმარებლის ინფორმაცია
- Authorization: Bearer Token (ნებისმიერი როლი)
- აბრუნებს მომხმარებლის დეტალებს ID-ის მიხედვით

### Loan Operations (User Role)

**POST /api/loan** - სესხის მოთხოვნა
- Authorization: Bearer Token (User)
- ქმნის ახალ სესხის მოთხოვნას
- Default სტატუსი: Processing
- ამოწმებს არის თუ არა მომხმარებელი დაბლოკილი

**GET /api/loan/my** - ჩემი სესხების ნახვა
- Authorization: Bearer Token (User)
- აბრუნებს მხოლოდ ავტორიზებული მომხმარებლის სესხებს

**PUT /api/loan/{id}** - სესხის განახლება
- Authorization: Bearer Token (User)
- ანახლებს საკუთარ სესხს
- მხოლოდ Processing სტატუსით
- სტატუსის შეცვლა არ შეუძლია

**DELETE /api/loan/{id}** - სესხის წაშლა
- Authorization: Bearer Token (User)
- შლის საკუთარ სესხს
- მხოლოდ Processing სტატუსით

### Accountant Operations

**GET /api/loan/all** - ყველა სესხის ნახვა
- Authorization: Bearer Token (Accountant)
- აბრუნებს ყველა მომხმარებლის ყველა სესხს

---

## Request/Response მაგალითები

### 1. რეგისტრაცია

**Request:**
```http
POST /api/user/register
Content-Type: application/json

{
  "firstName": "გიორგი",
  "lastName": "კუთუბიძე",
  "userName": "gkutubidze",
  "age": 25,
  "email": "giorgi@example.com",
  "monthlyIncome": 2500.00,
  "password": "securepass123",
  "isAccountant": false
}
```

**Response (200 OK):**
```json
"User registered"
```

**Validation Rules:**
- firstName: 2-50 სიმბოლო, სავალდებულო
- lastName: 2-50 სიმბოლო, სავალდებულო
- userName: 3-30 სიმბოლო, მხოლოდ ასოები/ციფრები/_, უნიკალური
- email: ვალიდური ელფოსტა, უნიკალური
- age: 18-100 წელი
- monthlyIncome: > 0
- password: მინიმუმ 6 სიმბოლო

**Possible Errors:**
- 400: "Username უკვე არსებობს"
- 400: "Email უკვე არსებობს"
- 400: Validation errors

---

### 2. ავტორიზაცია

**Request:**
```http
POST /api/user/login
Content-Type: application/json

{
  "userName": "gkutubidze",
  "password": "securepass123"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxIiwibmFtZSI6Imdrdeh..."
}
```

**JWT Token შემადგენლობა (Claims):**
- NameIdentifier: user.Id
- Name: user.UserName
- Role: "User" ან "Accountant"

**როგორ გამოვიყენო Token:**
```http
Authorization: Bearer {token}
```

**Possible Errors:**
- 400: "არასწორი username ან პაროლი"
- 400: "თქვენი ანგარიში დაბლოკილია"

---

### 3. მომხმარებლის ინფორმაცია

**Request:**
```http
GET /api/user/{id}
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "id": 1,
  "firstName": "გიორგი",
  "lastName": "კუთუბიძე",
  "userName": "gkutubidze",
  "age": 25,
  "email": "giorgi@example.com",
  "monthlyIncome": 2500.00,
  "isBlocked": false,
  "role": 0
}
```

**Possible Errors:**
- 401: Unauthorized (არასწორი token)
- 404: "User not found"

---

### 4. სესხის მოთხოვნა

**Request:**
```http
POST /api/loan
Authorization: Bearer {token}
Content-Type: application/json

{
  "loanType": 0,
  "amount": 5000.00,
  "currency": "GEL",
  "loanPeriod": 12
}
```

**LoanType Values:**
- 0 = FastLoan (სწრაფი სესხი)
- 1 = AutoLoan (ავტო სესხი)
- 2 = Installment (განვადება)

**Validation Rules:**
- amount: 0 < თანხა <= 1,000,000
- currency: "GEL", "USD", "EUR"
- loanPeriod: 1-360 თვე

**Response (200 OK):**
```json
{
  "id": 1,
  "userId": 1,
  "loanType": 0,
  "amount": 5000.00,
  "currency": "GEL",
  "loanPeriod": 12,
  "status": 0
}
```

**Possible Errors:**
- 401: Unauthorized
- 403: "User is blocked and cannot request loans"
- 400: Validation errors

---

### 5. ჩემი სესხების ნახვა

**Request:**
```http
GET /api/loan/my
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "userId": 1,
    "loanType": 0,
    "amount": 5000.00,
    "currency": "GEL",
    "loanPeriod": 12,
    "status": 0
  },
  {
    "id": 2,
    "userId": 1,
    "loanType": 1,
    "amount": 20000.00,
    "currency": "USD",
    "loanPeriod": 60,
    "status": 1
  }
]
```

---

### 6. სესხის განახლება

**Request:**
```http
PUT /api/loan/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "loanType": 0,
  "amount": 7000.00,
  "currency": "USD",
  "loanPeriod": 24
}
```

**Response (200 OK):**
```json
{
  "id": 1,
  "userId": 1,
  "loanType": 0,
  "amount": 7000.00,
  "currency": "USD",
  "loanPeriod": 24,
  "status": 0
}
```

**Possible Errors:**
- 403: "Access denied" (არ არის შენი სესხი)
- 400: "Only loans in processing status can be updated"

---

### 7. სესხის წაშლა

**Request:**
```http
DELETE /api/loan/{id}
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
"Loan deleted"
```

**Possible Errors:**
- 403: "Access denied"
- 400: "Only loans in processing status can be deleted"
- 404: "Loan not found"

---

### 8. ყველა სესხის ნახვა (Accountant)

**Request:**
```http
GET /api/loan/all
Authorization: Bearer {accountant-token}
```

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "userId": 1,
    "user": {
      "id": 1,
      "firstName": "გიორგი",
      "userName": "gkutubidze"
    },
    "loanType": 0,
    "amount": 5000.00,
    "currency": "GEL",
    "loanPeriod": 12,
    "status": 0
  }
]
```

---

## ბიზნეს წესები

### User შეზღუდვები

- შეუძლია მხოლოდ საკუთარი სესხების ნახვა/მართვა
- არ შეუძლია სტატუსის შეცვლა
- არ შეუძლია სხვა მომხმარებლის სესხების ნახვა
- დაბლოკილ მომხმარებელს (IsBlocked=true) არ შეუძლია სესხის მოთხოვნა
- განახლება/წაშლა მხოლოდ Processing სტატუსით

### Accountant უფლებები

- ყველა სესხის ნახვა
- ნებისმიერი სესხის სტატუსის შეცვლა
- ნებისმიერი სესხის სრული განახლება
- ნებისმიერი სესხის წაშლა (სტატუსს არ აქვს მნიშვნელობა)
- მომხმარებლების დაბლოკვა/განბლოკვა

### სესხის სტატუსები
```
LoanStatus:
  0 = Processing   (დამუშავების პროცესში - default)
  1 = Approved     (დამტკიცებული)
  2 = Rejected     (უარყოფილი)
```

---

## Security

### Password Security

პაროლები ინახება BCrypt hash-ის სახით:
```csharp
// რეგისტრაციისას:
user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

// ლოგინისას:
bool valid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
```

**რატომ BCrypt?**
- MD5/SHA256 არის unsafe brute-force attack-ებისთვის
- BCrypt იყენებს salting + hashing-ს
- "slow by design" - რთულია brute force
- ინდუსტრიის სტანდარტი

### JWT Authentication

Token შემადგენლობა:
```json
{
  "nameid": "1",
  "unique_name": "gkutubidze",
  "role": "User",
  "exp": 1234567890
}
```

Token-ის ვადა: 3 საათი

### Role-Based Authorization
```csharp
[Authorize(Roles = "User")]        // მხოლოდ User-ებისთვის
[Authorize(Roles = "Accountant")]  // მხოლოდ Accountant-ებისთვის
[Authorize]                        // ნებისმიერი ავტორიზებული
```

---

## ტესტების გაშვება
```bash
# Navigate to test project
cd LoanApi.Tests

# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity detailed

# Run with code coverage
dotnet test /p:CollectCoverage=true
```

### ტესტების სტრუქტურა
```
LoanApi.Tests/
├── Services/
│   ├── UserServiceTests.cs      (12+ tests)
│   └── LoanServiceTests.cs      (10+ tests)
└── Controllers/
    ├── UserControllerTests.cs
    └── LoanControllerTests.cs
```

### ტესტების მაგალითები

**UserServiceTests:**
- RegisterAsync_ShouldCreateUser_WhenValidData
- RegisterAsync_ShouldThrowException_WhenUsernameExists
- RegisterAsync_ShouldThrowException_WhenEmailExists
- LoginAsync_ShouldReturnToken_WhenValidCredentials
- LoginAsync_ShouldThrowException_WhenUserNotFound
- LoginAsync_ShouldThrowException_WhenUserIsBlocked
- LoginAsync_ShouldThrowException_WhenInvalidPassword
- BlockUserAsync_ShouldBlockUser_WhenCalled
- GetByIdAsync_ShouldReturnUser_WhenExists

**LoanServiceTests:**
- CreateLoanAsync_ShouldCreateLoan_WhenUserNotBlocked
- CreateLoanAsync_ShouldThrowException_WhenUserBlocked
- UpdateLoanAsync_ShouldUpdate_WhenStatusIsProcessing
- UpdateLoanAsync_ShouldThrowException_WhenStatusNotProcessing
- DeleteLoanAsync_ShouldDelete_WhenStatusIsProcessing
- GetUserLoansAsync_ShouldReturnOnlyUserLoans

---

## Exception Handling

### Global Exception Middleware

ყველა exception იჭერს ExceptionMiddleware და აბრუნებს user-friendly შეტყობინებას:
```csharp
// Program.cs
app.UseMiddleware<ExceptionMiddleware>();
```

**რა ხდება Exception-ის შემთხვევაში:**
1. Exception იჭერს Middleware
2. აბრუნებს 400 Bad Request
3. Body: { "error": "error message" }
4. ლოგავს Serilog-ით

**მაგალითი:**
```json
{
  "error": "User is blocked and cannot request loans"
}
```

---

## Validation Rules

FluentValidation ავტომატურად ამოწმებს Request Body-ს.

### UserRegisterDto Validation

- firstName: 2-50 სიმბოლო, სავალდებულო
- lastName: 2-50 სიმბოლო, სავალდებულო
- userName: 3-30 სიმბოლო, ასოები/ციფრები/_, უნიკალური
- email: ვალიდური format, უნიკალური
- age: 18-100 წელი
- monthlyIncome: > 0
- password: >= 6 სიმბოლო

### LoanCreateDto Validation

- amount: 0 < amount <= 1,000,000
- currency: "GEL" | "USD" | "EUR"
- loanPeriod: 1-360 თვე
- loanType: IsInEnum

### Validation Error Response
```json
{
  "errors": {
    "UserName": ["Username უნდა იყოს მინიმუმ 3 სიმბოლო"],
    "Email": ["ელფოსტის ფორმატი არასწორია"]
  }
}
```

---

## Database Schema

### Users Table
```sql
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    FirstName NVARCHAR(MAX) NOT NULL,
    LastName NVARCHAR(MAX) NOT NULL,
    UserName NVARCHAR(MAX) NOT NULL,
    Age INT NOT NULL,
    Email NVARCHAR(MAX) NOT NULL,
    MonthlyIncome DECIMAL(18,2) NOT NULL,
    IsBlocked BIT DEFAULT 0,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Role INT DEFAULT 0
);
```

### Loans Table
```sql
CREATE TABLE Loans (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    LoanType INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Currency NVARCHAR(MAX) NOT NULL,
    LoanPeriod INT NOT NULL,
    Status INT DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
```

### Relationship

- User → Loans (One-to-Many)
- Cascade Delete: User-ის წაშლისას წაიშლება მისი ყველა Loan

---

## Logging

### Serilog Configuration

- **Console Sink**: Console-ში წერს INFO+ logs
- **File Sink**: Logs/log-YYYYMMDD.txt ფაილებში
- **Rolling Interval**: Daily (ყოველდღე ახალი ფაილი)

### Log Levels
```csharp
Log.Information("User registered: {UserName}", userName);
Log.Warning("Login failed: User blocked - {UserName}", userName);
Log.Error(ex, "Error during registration: {UserName}", userName);
```

### Log ფაილის ნახვა
```bash
cat Logs/log-20251209.txt
```

---

## HTTP Status Codes

| Code | Meaning | როდის ამოდის |
|------|---------|-------------|
| 200 | OK | წარმატებული request |
| 400 | Bad Request | ვალიდაციის შეცდომა / Business logic error |
| 401 | Unauthorized | Token არ არის / არასწორია |
| 403 | Forbidden | Token OK-ია, მაგრამ Role არ უშვებს |
| 404 | Not Found | რესურსი ვერ მოიძებნა |
| 500 | Internal Server Error | სერვერის შიდა შეცდომა |

---

## Swagger Usage

### 1. გაუშვი აპლიკაცია
```bash
cd LoanApi
dotnet run
```

### 2. გახსენი Swagger
```
http://localhost:5090/swagger
```

### 3. ავტორიზაცია

1. დააჭირე POST /api/user/login
2. Execute → დააკოპირე token
3. დააჭირე Authorize ღილაკს (ზემოთ მარჯვნივ)
4. ჩაწერე: Bearer {შენი-token}
5. Authorize → ახლა ყველა protected endpoint-ზე გიშვებს

### 4. ტესტირება

- Try it out
- Request Body-ს შეავსე
- Execute
- ნახე Response

---

## Known Issues & Limitations

### არ არის იმპლემენტირებული:

- Rate Limiting (რექვესტების შეზღუდვა)
- Email Verification (ელფოსტის დადასტურება)
- Password Reset (პაროლის აღდგენა)
- Refresh Token (Token-ის განახლება)
- Pagination (სიების pagination)
- Soft Delete (მონაცემების "რბილი" წაშლა)

### შეზღუდვები:

- Logs მხოლოდ ფაილებში (არა DB-ში)
- JWT Secret არ უნდა შეინახოს Git-ში (appsettings.json ignored)
- არ არის CORS Configuration (საჭიროა Frontend-ისთვის)

---

## პროექტის სტრუქტურის განმარტება

### რატომ Interfaces?
```csharp
// SOLID - Dependency Inversion Principle
public class UserController {
    private readonly IUserService _userService;  // Interface, არა კონკრეტული კლასი
}
```

კლასები დამოკიდებულნი არიან Abstraction-ზე (ინტერფეისი), რაც აადვილებს:
- Testing (Mocking)
- Dependency Injection
- Code Maintainability

### რატომ DTOs?
```csharp
// არ ვაბრუნებთ პირდაპირ Entity-ს (Security)
public class UserResponseDto {
    // მხოლოდ საჭირო ველები, არა PasswordHash
}
```

DTOs (Data Transfer Objects) გვიცავს:
- არ ვაჩვენებთ PasswordHash-ს
- არ ვაბრუნებთ არასაჭირო ველებს
- შეგვიძლია სხვადასხვა Response Format

### რატომ Middleware?
```csharp
// Global Exception Handler
app.UseMiddleware<ExceptionMiddleware>();
```

ერთ ადგილას ხდება ყველა Exception-ის დამუშავება, არა თითოეულ Controller-ში try-catch.

---

## Libraries განმარტება

### BCrypt

რატომ არა MD5/SHA256?
- MD5/SHA256: სწრაფი → brute force-ისთვის მარტივი
- BCrypt: ნელი by design → brute force-ისთვის რთული
- BCrypt იყენებს salt-ს → ერთი password, სხვადასხვა hash
- ინდუსტრიის სტანდარტი

### Serilog

რატომ არა Console.WriteLine?
- Structured Logging: ლოგს აქვს სტრუქტურა (JSON format)
- Multiple Sinks: Console + File + DB
