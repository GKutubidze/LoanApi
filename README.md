# LoanApi დოკუმენტაცია

## შინაარსი

1. [პროექტის აღწერა](#პროექტის-აღწერა)
2. [ტექნოლოგიები](#ტექნოლოგიები)
3. [Setup ინსტრუქციები](#setup-ინსტრუქციები)
4. [არქიტექტურა](#არქიტექტურა)
5. [API Endpoints](#api-endpoints)
6. [Request/Response მაგალითები](#requestresponse-მაგალითები)
7. [მართვის წესები](#მართვის-წესები)
8. [უსაფრთხოება](#უსაფრთხოება)
9. [ტესტები](#ტესტები)

---

## პროექტის აღწერა

LoanApi არის სესხების მართვის სისტემა .NET 10.0-ზე. სისტემა აქვს ორი ძირითადი მომხმარებელი:

მომხმარებლები - მათ შეუძლიათ რეგისტრაცია, ავტორიზაცია და საკუთარი სესხების მართვა.

ბუღალტრები - მათ შეუძლიათ ყველა სესხის ნახვა, მართვა და მომხმარებლების დაბლოკვა.

ძირითადი ფუნქციები:

- JWT-based ავტენტიკაცია
- Role-based კონტროლი (User, Accountant)
- სესხის სრული CRUD ოპერაციები
- მომხმარებლების დაბლოკვა/განბლოკვა
- ყველა შეყვანილი მონაცემის ვალიდაცია
- სტრუქტურირებული ლოგირება
- აპლიკაციის უნივერსალური შეცდომების დამუშავება
- ვიზუალური API დოკუმენტაცია Swagger-ის საშუალებით

---

## ტექნოლოგიები

| ტექნოლოგია | ვერსია | გამოყენება |
|---|---|---|
| .NET | 10.0 | ძირითადი ფრეიმვორკი |
| Entity Framework Core | 9.0 | მონაცემთა წვდომა |
| SQL Server | 2019+ | მონაცემთა საცავი |
| JWT Bearer | 9.0 | ავტენტიკაცია |
| BCrypt.Net | 4.0.3 | პაროლის დაშიფვრა |
| Serilog | 8.0.3 | ლოგირება |
| FluentValidation | 11.11.0 | მონაცემთა შემოწმება |
| xUnit | 2.9.3 | ტესტირება |
| Swagger | 7.2.0 | დოკუმენტაცია |

---

## Setup ინსტრუქციები

### მოთხოვნილი პროგრამები

დაიყენეთ შემდეგი:
- .NET 10 SDK
- SQL Server 2019 ან უფრო ახალი
- Git

### რეპოზიტორიის დაკლონვა

```bash
git clone git@github.com:GKutubidze/LoanApi.git
cd LoanApi
```

### მონაცემთა ბაზის კონფიგურაცია

შექმენით ან განაახლეთ ფაილი `LoanApi/appsettings.json`:

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

მნიშვნელოვანი ინფორმაცია:

`Server=.` აღნიშნავს ადგილობრივ SQL Server-ს. თუ სხვა სერვერი გაქვთ, შეცვალეთ connection string. `JwtSettings:Key` უნდა იყოს მინიმუმ 32 სიმბოლო.

### მიგრაცია

```bash
cd LoanApi
dotnet ef database update
```

### აპლიკაციის გაშვება

```bash
dotnet build
dotnet run
```

აპლიკაციის მისამართები:
- API: http://localhost:5090
- Swagger: http://localhost:5090/swagger

---

## არქიტექტურა

### ფაილების სტრუქტურა

```
LoanApi/
├── Controllers/              
│   ├── UserController.cs
│   └── LoanController.cs
├── Services/                 
│   ├── UserService.cs
│   ├── LoanService.cs
│   └── JwtService.cs
├── Interfaces/               
│   ├── IUserService.cs
│   ├── ILoanService.cs
│   └── IJwtService.cs
├── Models/                   
│   ├── User.cs
│   ├── Loan.cs
│   └── DTOs/
├── Data/                     
│   ├── LoanDbContext.cs
│   └── Migrations/
├── Validators/               
├── Middleware/               
├── Enums/                    
└── Program.cs

LoanApi.Tests/                
├── Services/
│   ├── UserServiceTests.cs
│   └── LoanServiceTests.cs
```

### დიზაინ პრინციპები

SOLID პრინციპები გამოყენებულია აბსტრაქციის რწყობილობისთვის:

Single Responsibility - ყველა კლასი ერთი რამის ზე პასუხისმგებელია.

Open/Closed - კლასები ღიაა გაფართოებისთვის, დახურულია შეცვლისთვის.

Liskov Substitution - ინტერფეისის განხორციელება შესაბამისი და ჩანაცვლებადია.

Interface Segregation - ინტერფეისები პატარა და კონკრეტული.

Dependency Inversion - კლასები დამოკიდებულია აბსტრაქციაზე, არა კონკრეტებზე.

---

## API Endpoints

### ავტენტიკაცია

POST /api/user/register

მომხმარებლის რეგისტრაცია. ავტორიზაცია არ სჭირდება. იქმნება ახალი მომხმარებელი, პაროლი დაშიფრული ხდება BCrypt-ით. ნაგულისხმები როლი არის User.

POST /api/user/login

ავტორიზაცია. ავტორიზაცია არ სჭირდება. მოწმდება ხელმოწერა და დაბრუნდება JWT Token. Token-ის ვადა 3 საათი.

### მომხმარებელი

GET /api/user/{id}

მომხმარებლის ინფორმაციის მიღება. საჭირო Bearer Token (User ან Accountant). დაბრუნდება მომხმარებლის დეტალები ID-ის მიხედვით.

PATCH /api/user/accountant/{id}/block

მომხმარებლის დაბლოკვა ან განბლოკვა. საჭირო Bearer Token, მხოლოდ Accountant. ბლოკავს ან განბლოკავს მომხმარებელს.

### სესხი - მომხმარებელი

POST /api/loan/user

სესხის მოთხოვნა. საჭირო Bearer Token, მხოლოდ User. იქმნება ახალი სესხი. ნაგულისხმები სტატუსი Processing. შემოწმდება თუ მომხმარებელი დაბლოკილია.

GET /api/loan/user/my

საკუთარი სესხების ნახვა. საჭირო Bearer Token, მხოლოდ User. დაბრუნდება მხოლოდ ამ მომხმარებლის სესხები.

PUT /api/loan/user/{id}

სესხის განახლება. საჭირო Bearer Token, მხოლოდ User. განახლდება საკუთარი სესხი. შესაძლებელი მხოლოდ Processing სტატუსით.

DELETE /api/loan/user/{id}

სესხის წაშლა. საჭირო Bearer Token, მხოლოდ User. წაიშლება საკუთარი სესხი. შესაძლებელი მხოლოდ Processing სტატუსით.

### სესხი - ბუღალტერი

GET /api/loan/accountant/all

ყველა სესხის ნახვა. საჭირო Bearer Token, მხოლოდ Accountant. დაბრუნდება ყველა სესხი ყველა მომხმარებლისგან.

PUT /api/loan/accountant/{id}

ნებისმიერი სესხის განახლება. საჭირო Bearer Token, მხოლოდ Accountant. განახლდება ნებისმიერი სესხი, ნებისმიერი სტატუსით.

DELETE /api/loan/accountant/{id}

ნებისმიერი სესხის წაშლა. საჭირო Bearer Token, მხოლოდ Accountant. წაიშლება ნებისმიერი სესხი.

---

## Request/Response მაგალითები

### რეგისტრაცია

Request:
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

Response (200 OK):
```json
{
  "message": "User registered successfully"
}
```

ვალიდაციის წესები:

firstName - სავალდებულო, მაქსიმუმ 50 სიმბოლო.
lastName - სავალდებულო, მაქსიმუმ 50 სიმბოლო.
userName - სავალდებულო, მაქსიმუმ 50 სიმბოლო, უნიკალური.
email - სწორი ელფოსტის ფორმატი, უნიკალური.
age - მინიმუმ 18 წელი.
monthlyIncome - მეტი ვიდრე 0.
password - მინიმუმ 6 სიმბოლო.

შესაძლო შეცდომები:

400 - Username უკვე არსებობს.
400 - Email უკვე არსებობს.
400 - ვალიდაციის შეცდომები.

### ავტორიზაცია

Request:
```http
POST /api/user/login
Content-Type: application/json

{
  "userName": "gkutubidze",
  "password": "securepass123"
}
```

Response (200 OK):
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

JWT Token შემადგენლობა:

NameIdentifier - მომხმარებლის ID.
Name - მომხმარებლის სახელი.
Role - User ან Accountant.

Token გამოყენება:
```http
Authorization: Bearer {token}
```

შესაძლო შეცდომები:

400 - არასწორი username ან პაროლი.
400 - ანგარიში დაბლოკილია.

### სესხის მოთხოვნა

Request:
```http
POST /api/loan/user
Authorization: Bearer {token}
Content-Type: application/json

{
  "loanType": 0,
  "amount": 5000.00,
  "currency": "GEL",
  "loanPeriod": 12
}
```

LoanType მნიშვნელობები:

0 - FastLoan (სწრაფი სესხი).
1 - AutoLoan (ავტო სესხი).
2 - Installment (განვადება).

Response (200 OK):
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

შესაძლო შეცდომები:

401 - Unauthorized.
400 - მომხმარებელი დაბლოკილია.
400 - ვალიდაციის შეცდომები.

### ჩემი სესხები

Request:
```http
GET /api/loan/user/my
Authorization: Bearer {token}
```

Response (200 OK):
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

### სესხის განახლება (მომხმარებელი)

Request:
```http
PUT /api/loan/user/1
Authorization: Bearer {token}
Content-Type: application/json

{
  "loanType": 0,
  "amount": 7000.00,
  "currency": "USD",
  "loanPeriod": 24
}
```

Response (200 OK):
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

შესაძლო შეცდომები:

403 - წვდომა უარყოფილია.
400 - მხოლოდ Processing სტატუსის სესხი შეიძლება განახლდეს.

### სესხის წაშლა (მომხმარებელი)

Request:
```http
DELETE /api/loan/user/1
Authorization: Bearer {token}
```

Response (200 OK):
```json
"Loan deleted"
```

შესაძლო შეცდომები:

403 - წვდომა უარყოფილია.
400 - მხოლოდ Processing სტატუსის სესხი შეიძლება წაიშალოს.
404 - სესხი ვერ მოიძებნა.

### ყველა სესხი (Accountant)

Request:
```http
GET /api/loan/accountant/all
Authorization: Bearer {accountant-token}
```

Response (200 OK):
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

### სესხის განახლება (Accountant)

Request:
```http
PUT /api/loan/accountant/1
Authorization: Bearer {accountant-token}
Content-Type: application/json

{
  "loanType": 0,
  "amount": 8000.00,
  "currency": "EUR",
  "loanPeriod": 36
}
```

Response (200 OK):
```json
{
  "id": 1,
  "userId": 1,
  "loanType": 0,
  "amount": 8000.00,
  "currency": "EUR",
  "loanPeriod": 36,
  "status": 0
}
```

### მომხმარებლის დაბლოკვა (Accountant)

Request:
```http
PATCH /api/user/accountant/1/block
Authorization: Bearer {accountant-token}
Content-Type: application/json

{
  "isBlocked": true
}
```

Response (200 OK):
```json
{
  "message": "User blocked successfully"
}
```

---

## მართვის წესები

### მომხმარებელი

მომხმარებელს შეუძლია:

მხოლოდ საკუთარი სესხების ნახვა და მართვა.
Processing სტატუსის სესხის განახლება.
Processing სტატუსის სესხის წაშლა.
თავის მონაცემების ნახვა.

მომხმარებელს არ შეუძლია:

სესხის სტატუსის შეცვლა.
სხვა მომხმარებლის სესხების ნახვა.
სხვა მომხმარებლის მონაცემების შეცვლა.
მომხმარებელი რომელიც დაბლოკილია, სესხის მოთხოვნა ვერ გააკეთებს.

### ბუღალტერი

ბუღალტერს შეუძლია:

ყველა სესხის ნახვა.
ნებისმიერი სესხის განახლება ნებისმიერი სტატუსით.
ნებისმიერი სესხის წაშლა.
ნებისმიერი სესხის სტატუსის შეცვლა.
მომხმარებელის დაბლოკვა ან განბლოკვა.

### სესხის სტატუსები

Processing (0) - სესხი დამუშავების პროცესში.
Approved (1) - სესხი დამტკიცებული.
Rejected (2) - სესხი უარყოფილი.

---

## უსაფრთხოება

### პაროლის დაშიფვრა

პაროლები ინახება BCrypt hash-ის ფორმატში:

რეგისტრაციის დროს:
```
user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
```

ლოგინის დროს:
```
bool valid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)
```

BCrypt-ის უპირატესობები:

MD5 და SHA256 უსაფრთხო არ არის brute-force თავდასხმის წინააღმდეგ.
BCrypt იყენებს salt-ს და hashing-ს.
BCrypt განზომილებით ნელი არის, რაც brute-force-ს გრძელვადიან ხდის.
ეს ინდუსტრიის სტანდარტი.

### JWT ავტენტიკაცია

Token შედგება:

NameIdentifier - მომხმარებლის ID.
Name - მომხმარებლის სახელი.
Role - User ან Accountant.
Expiry - 3 საათი.

### როლი-ორიენტირებული წვდომა

```csharp
[Authorize(Roles = "User")]        // მხოლოდ User-ებისთვის
[Authorize(Roles = "Accountant")]  // მხოლოდ Accountant-ებისთვის
[Authorize]                        // ნებისმიერი ავტორიზებული მომხმარებელი
```

---

## ტესტები

### ტესტების გაშვება

```bash
cd LoanApi.Tests
dotnet test
```

### ტესტების ორგანიზაცია

LoanApi.Tests/Services/UserServiceTests.cs - 8 ტესტი.
LoanApi.Tests/Services/LoanServiceTests.cs - 10 ტესტი.

### UserServiceTests

RegisterAsync_ValidUser_CreatesUser - სწორი მონაცემით რეგისტრაცია.
RegisterAsync_DuplicateUsername_ThrowsException - დუბლიკატი სახელით შეცდომა.
LoginAsync_ValidCredentials_ReturnsToken - სწორი ხელმოწერით token.
LoginAsync_WrongPassword_ThrowsException - არასწორი პაროლით შეცდომა.
LoginAsync_BlockedUser_ThrowsException - დაბლოკილი მომხმარებელი შეცდომა.
GetByIdAsync_ExistingUser_ReturnsUser - მომხმარებელის ნახვა.
BlockUserAsync_UpdatesIsBlockedStatus - მომხმარებელი დაბლოკა.
BlockUserAsync_UserNotFound_ThrowsException - ვერ ნაპოვნი მომხმარებელი შეცდომა.

### LoanServiceTests

CreateLoanAsync_ValidUser_CreatesLoan - სესხის შექმნა.
CreateLoanAsync_UserNotFound_ThrowsException - ვერ ნაპოვნი მომხმარებელი.
CreateLoanAsync_BlockedUser_ThrowsException - დაბლოკილი მომხმარებელი.
GetUserLoansAsync_ReturnsOnlyUserLoans - მხოლოდ საკუთარი სესხები.
UpdateLoanAsync_ValidLoan_UpdatesLoan - სესხის განახლება.
UpdateLoanAsync_WrongUser_ThrowsException - არასწორი მომხმარებელი.
UpdateLoanAsync_WrongStatus_ThrowsException - არასწორი სტატუსი.
DeleteLoanAsync_ValidLoan_DeletesLoan - სესხის წაშლა.
DeleteLoanAsync_WrongStatus_ThrowsException - არასწორი სტატუსი წაშლისას.
GetAllLoansAsync_ReturnsAllLoans - ყველა სესხი.

---

## მონაცემთა ბაზის სქემა

### Users ცხრილი

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

### Loans ცხრილი

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

### ურთიერთობა

User-სა და Loan-ს შორის არის One-to-Many ურთიერთობა. User-ის წაშლისას წაიშლება მისი ყველა სესხი (Cascade Delete).

---

## შეყვანილი მონაცემის შემოწმება

### UserRegisterDto

firstName - სავალდებულო, მაქსიმუმ 50 სიმბოლო.
lastName - სავალდებულო, მაქსიმუმ 50 სიმბოლო.
userName - სავალდებულო, მაქსიმუმ 50 სიმბოლო.
email - სწორი ელფოსტის ფორმატი.
age - მინიმუმ 18.
monthlyIncome - მეტი ვიდრე 0.
password - მინიმუმ 6 სიმბოლო.

### LoanCreateDto

amount - მეტი ვიდრე 0.
currency - სავალდებულო, მაქსიმუმ 10 სიმბოლო.
loanPeriod - მეტი ვიდრე 0.
loanType - სწორი enum მნიშვნელობა.

### შეცდომის პასუხი

```json
{
  "errors": {
    "UserName": ["Username is required"],
    "Email": ["Invalid email format"]
  }
}
```

---

## HTTP სტატუსის კოდები

200 - OK - წარმატებული მოთხოვნა.
400 - Bad Request - შეყვანილი მონაცემის შეცდომა ან სიტუაციის პრობლემა.
401 - Unauthorized - Token აკლია ან არასწორია.
403 - Forbidden - Token OK-ია, მაგრამ წვდომა აკრძალული.
404 - Not Found - რესურსი ვერ მოიძებნა.

---

## Swagger გამოყენება

### ნაბიჯი 1: აპლიკაციის გაშვება

```bash
cd LoanApi
dotnet run
```

### ნაბიჯი 2: Swagger-ის გახსნა

მოიხსენით ბრაუზერი და წადით http://localhost:5090/swagger

### ნაბიჯი 3: ავტორიზაცია

დააჭირეთ Authorize ღილაკს (ზე მარჯვნივ).

### ნაბიჯი 4: Token-ის მიღება

აირჩიეთ POST /api/user/login.
დააჭირეთ "Try it out".
შეიყვანეთ username და password.
დააჭირეთ Execute.
დააკოპირეთ მიღებული token.

### ნაბიჯი 5: Token-ის გამოყენება

დააჭირეთ Authorize ღილაკს ისევ.
Authorize ველში შეიყვანეთ: Bearer {token}
დააჭირეთ Authorize.

### ნაბიჯი 6: ენდპოინტების ტესტირება

ახლა შეგიძლიათ სხვა ენდპოინტებს სცადოთ.

---

## ლოგირება

### Serilog კონფიგურაცია

Console Sink - ლოგები Console-ში.
File Sink - ლოგები Logs/log-YYYYMMDD.txt ფაილებში.
Rolling Interval - ყოველ დღეს ახალი ფაილი.

### Log დონეები

```
Information - რეგულარული ინფორმაცია
Warning - გაფრთხოების შეტყობინებები
Error - შეცდომები
```

### ლოგის მაგალითი

```
2025-12-10 16:53:14.148 [INF] Application started
2025-12-10 16:53:16.137 [INF] Request finished HTTP/1.1 GET
2025-12-10 16:54:26.656 [INF] Now listening on: http://localhost:5090
```

### ლოგის ფაილის ნახვა

```bash
cat Logs/log-20251210.txt
```

---

## ზოგადი ინფორმაცია

GitHub Repository: https://github.com/GKutubidze/LoanApi

მომხმარებელი: GKutubidze

ლიცენზია: MIT