# შექმენი README.md ფაილი
cat > README.md << 'EOF'
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
- .NET 10 SDK
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
dotnet ef database update
```

### 5. Build & Run
```bash
dotnet build
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
│   ├── UserController.cs
│   └── LoanController.cs
├── Services/                 # Business Logic
│   ├── UserService.cs
│   ├── LoanService.cs
│   └── JwtService.cs
├── Interfaces/               # Abstractions
│   ├── IUserService.cs
│   ├── ILoanService.cs
│   └── IJwtService.cs
├── Models/                   # Domain Models
│   ├── User.cs
│   ├── Loan.cs
│   └── DTOs/
├── Data/                     # Database
│   ├── LoanDbContext.cs
│   └── Migrations/
├── Validators/               # FluentValidation
├── Middleware/               # Exception Handler
├── Enums/                    # Enumerations
└── Program.cs

LoanApi.Tests/                # Unit Tests
├── Services/
│   ├── UserServiceTests.cs
│   └── LoanServiceTests.cs
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

**PATCH /api/user/{id}/block** - მომხმარებლის დაბლოკვა
- Authorization: Bearer Token (Accountant)
- ბლოკავს/განბლოკავს მომხმარებელს

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
  "password": "securepass123"
}
```

**Response (200 OK):**
```json
{
  "message": "User registered successfully"
}
```

**Validation Rules:**
- firstName: სავალდებულო, max 50 სიმბოლო
- lastName: სავალდებულო, max 50 სიმბოლო
- userName: სავალდებულო, max 50 სიმბოლო, უნიკალური
- email: ვალიდური ელფოსტა, უნიკალური
- age: მინიმუმ 18 წელი
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
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**JWT Token შემადგენლობა:**
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
GET /api/user/1
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
  "isAccountant": false
}
```

**Possible Errors:**
- 401: Unauthorized
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
- 400: "User is blocked and cannot request loans"
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
  }
]
```

---

### 6. სესხის განახლება

**Request:**
```http
PUT /api/loan/1
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
- 403: "Access denied"
- 400: "Only loans in processing status can be updated"

---

### 7. სესხის წაშლა

**Request:**
```http
DELETE /api/loan/1
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

### 9. მომხმარებლის დაბლოკვა (Accountant)

**Request:**
```http
PATCH /api/user/1/block
Authorization: Bearer {accountant-token}
Content-Type: application/json

{
  "isBlocked": true
}
```

**Response (200 OK):**
```json
{
  "message": "User blocked successfully"
}
```

---

## ბიზნეს წესები

### User შეზღუდვები

- შეუძლია მხოლოდ საკუთარი სესხების ნახვა/მართვა
- არ შეუძლია სტატუსის შეცვლა
- არ შეუძლია სხვა მომხმარებლის სესხების ნახვა
- დაბლოკილ მომხმარებელს არ შეუძლია სესხის მოთხოვნა
- განახლება/წაშლა მხოლოდ Processing სტატუსით

### Accountant უფლებები

- ყველა სესხის ნახვა
- ნებისმიერი სესხის სტატუსის შეცვლა
- ნებისმიერი სესხის სრული განახლება
- ნებისმიერი სესხის წაშლა
- მომხმარებლების დაბლოკვა/განბლოკვა

### სესხის სტატუსები
```
LoanStatus:
  0 = Processing   (დამუშავების პროცესში)
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
cd LoanApi.Tests
dotnet test
```

### ტესტების სტრუქტურა
```
LoanApi.Tests/
├── Services/
│   ├── UserServiceTests.cs      (9 tests)
│   └── LoanServiceTests.cs      (11 tests)
```

### ტესტების მაგალითები

**UserServiceTests:**
- RegisterAsync_ValidUser_CreatesUser
- RegisterAsync_DuplicateUsername_ThrowsException
- LoginAsync_ValidCredentials_ReturnsToken
- LoginAsync_WrongPassword_ThrowsException
- LoginAsync_BlockedUser_ThrowsException
- GetByIdAsync_ExistingUser_ReturnsUser
- BlockUserAsync_UpdatesIsBlockedStatus
- BlockUserAsync_UserNotFound_ThrowsException

**LoanServiceTests:**
- CreateLoanAsync_ValidUser_CreatesLoan
- CreateLoanAsync_UserNotFound_ThrowsException
- CreateLoanAsync_BlockedUser_ThrowsException
- GetUserLoansAsync_ReturnsOnlyUserLoans
- UpdateLoanAsync_ValidLoan_UpdatesLoan
- UpdateLoanAsync_WrongUser_ThrowsException
- UpdateLoanAsync_WrongStatus_ThrowsException
- DeleteLoanAsync_ValidLoan_DeletesLoan
- DeleteLoanAsync_WrongStatus_ThrowsException
- GetAllLoansAsync_ReturnsAllLoans

---

## Exception Handling

### Global Exception Middleware

ყველა exception იჭერს ExceptionMiddleware და აბრუნებს user-friendly შეტყობინებას:
```csharp
app.UseMiddleware<ExceptionMiddleware>();
```

**რა ხდება Exception-ის შემთხვევაში:**
1. Exception იჭერს Middleware
2. აბრუნებს 400 Bad Request
3. Response: `{ "error": "error message" }`
4. ლოგავს Serilog-ით

---

## Validation Rules

FluentValidation ავტომატურად ამოწმებს Request Body-ს.

### UserRegisterDto

- firstName: სავალდებულო, max 50 სიმბოლო
- lastName: სავალდებულო, max 50 სიმბოლო
- userName: სავალდებულო, max 50 სიმბოლო
- email: ვალიდური format
- age: >= 18
- monthlyIncome: > 0
- password: >= 6 სიმბოლო

### LoanCreateDto

- amount: > 0
- currency: სავალდებულო, max 10 სიმბოლო
- loanPeriod: > 0
- loanType: IsInEnum

### Validation Error Response
```json
{
  "errors": {
    "UserName": ["Username is required"],
    "Email": ["Invalid email format"]
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
- **File Sink**: `Logs/log-YYYYMMDD.txt` ფაილებში
- **Rolling Interval**: Daily

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
| 400 | Bad Request | ვალიდაციის შეცდომა / Business error |
| 401 | Unauthorized | Token არ არის / არასწორია |
| 403 | Forbidden | Token OK-ია, მაგრამ Role არ უშვებს |
| 404 | Not Found | რესურსი ვერ მოიძებნა |

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
3. დააჭირე Authorize ღილაკს
4. ჩაწერე: `Bearer {token}`
5. Authorize

### 4. ტესტირება

- Try it out
- Request Body-ს შეავსე
- Execute
- ნახე Response

---

## Libraries განმარტება

### BCrypt

- MD5/SHA256 unsafe-ია brute force-ისთვის
- BCrypt იყენებს salt-ს
- ნელი by design
- ინდუსტრიის სტანდარტი

### Serilog

- Structured Logging
- Multiple Sinks (Console + File)
- Filtering (Info, Warning, Error)
- Production-ready

### FluentValidation

- უფრო მოქნილია [Required] Attributes-ზე
- რთული ვალიდაციის დაწერა შესაძლებელია
- Testable
- Separation of Concerns

---

## Author

გიორგი კუთუბიძე
Free University of Tbilisi
Introduction to Programming - Final Project
December 2024

---

## License

Academic Project - Educational Use Only
EOF

echo "README.md ფაილი შეიქმნა!"