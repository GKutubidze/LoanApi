# LoanApi - სესხების მართვის სისტემა

## რა არის LoanApi?

LoanApi არის .NET 10.0 ზე აგებული REST API აპლიკაცია სესხების მართვის სისტემა. სისტემაში ორი ტიპის მომხმარებელი არის: ჩვეულებრივი მომხმარებელი (User) რომელსაც შეუძლია საკუთარი სესხების მოთხოვნა და მართვა, და ბუღალტერი (Accountant) რომელსაც შეუძლია ყველა სესხის დენიშვნება, განახლება, წაშლა და მომხმარებლების დაბლოკვა.

---

## მოთხოვნილი ტექნოლოგია

- .NET 10.0 SDK
- SQL Server 2019 ან უფრო ახალი
- Git

---

## ინსტალაცია

### 1. რეპოზიტორიის დაკლონვა

```bash
git clone https://github.com/GKutubidze/LoanApi.git
cd LoanApi
```

### 2. appsettings.json კონფიგურაცია

`LoanApi` ფოლდერში შექმენით ან განაახლეთ `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=LoanDb;Trusted_Connection=true;TrustServerCertificate=true"
  },
  "JwtSettings": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForSecurity!",
    "Issuer": "LoanApi",
    "Audience": "LoanApiUsers"
  }
}
```

**მნიშვნელოვანი**: JwtSettings:Key უნდა იყოს მინიმუმ 32 სიმბოლო.

### 3. მონაცემთა ბაზის შექმნა

```bash
cd LoanApi
dotnet ef database update
```

ეს ბრძანება SQL Server-ზე LoanDb მონაცემთა ბაზას შექმნის და მიგრაციებს გამოიყენებს.

### 4. აპლიკაციის გაშვება

```bash
dotnet run
```

აპლიკაცია ხელმისაწვდომი იქნება შემდეგ მისამართებზე:
- API: `http://localhost:5090`
- Swagger დოკუმენტაცია: `http://localhost:5090/swagger`

---

## პროექტის სტრუქტურა

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
│   ├── UserRegisterDtoValidator.cs
│   ├── LoanCreateDtoValidator.cs
│   └── LoanUpdateDtoValidator.cs
├── Enums/
│   ├── UserRole.cs
│   ├── LoanStatus.cs
│   └── LoanType.cs
├── Middleware/
│   └── ExceptionMiddleware.cs
├── appsettings.json
└── Program.cs

LoanApi.Tests/
├── Services/
│   ├── UserServiceTests.cs
│   └── LoanServiceTests.cs
└── LoanApi.Tests.csproj
```

---

## მოდელები

### User

მომხმარებელი ინფორმაცია:

- **Id**: უნიკალური იდენტიფიკატორი
- **FirstName, LastName**: პირადი ინფორმაცია
- **UserName**: მიმოწოდება (უნიკალური)
- **Email**: ელფოსტა (უნიკალური)
- **Age**: ასაკი
- **MonthlyIncome**: ყოველთვიური შემოსავალი
- **IsBlocked**: დაბლოკილია თუ არა
- **PasswordHash**: დაშიფრული პაროლი
- **Role**: UserRole.User ან UserRole.Accountant

### Loan

სესხი ინფორმაცია:

- **Id**: უნიკალური იდენტიფიკატორი
- **UserId**: რომელი მომხმარებელის სესხი
- **LoanType**: სესხის ტიპი (FastLoan=0, AutoLoan=1, Installment=2)
- **Amount**: რაოდენობა
- **Currency**: валютა (GEL, USD, EUR)
- **LoanPeriod**: თვეების რაოდენობა
- **Status**: LoanStatus.Processing(0), Approved(1), Rejected(2)

---

## API Endpoints

### ავტენტიკაცია

#### რეგისტრაცია

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

**პასუხი**: 200 OK
```json
{
  "message": "User registered successfully"
}
```

**შეცდომები**:
- 400: Username უკვე არსებობს
- 400: Email უკვე არსებობს
- 400: ვალიდაციის შეცდომა

#### ლოგინი

```http
POST /api/user/login
Content-Type: application/json

{
  "userName": "gkutubidze",
  "password": "securepass123"
}
```

**პასუხი**: 200 OK
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**შეცდომები**:
- 400: არასწორი username ან პაროლი
- 400: ანგარიში დაბლოკილია

---

### მომხმარებელი

#### მომხმარებელი ინფორმაცია

```http
GET /api/user/{id}
Authorization: Bearer {token}
```

**პასუხი**: 200 OK
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

#### მომხმარებელი დაბლოკვა (Accountant)

```http
PATCH /api/user/accountant/{id}/block
Authorization: Bearer {accountant-token}
Content-Type: application/json

{
  "isBlocked": true
}
```

**პასუხი**: 200 OK
```json
{
  "message": "User blocked successfully"
}
```

---

### სესხი - მომხმარებელი

#### ახალი სესხის მოთხოვნა

```http
POST /api/loan/user
Authorization: Bearer {user-token}
Content-Type: application/json

{
  "loanType": 0,
  "amount": 5000.00,
  "currency": "GEL",
  "loanPeriod": 12
}
```

**პასუხი**: 200 OK
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

**შეცდომები**:
- 401: Unauthorized (token აკლია)
- 400: მომხმარებელი დაბლოკილია
- 400: ვალიდაციის შეცდომა

#### საკუთარი სესხების ნახვა

```http
GET /api/loan/user/my
Authorization: Bearer {user-token}
```

**პასუხი**: 200 OK
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

#### სესხის განახლება

```http
PUT /api/loan/user/{id}
Authorization: Bearer {user-token}
Content-Type: application/json

{
  "loanType": 0,
  "amount": 7000.00,
  "currency": "USD",
  "loanPeriod": 24
}
```

**პირობები**:
- სესხის Status უნდა იყოს Processing
- სესხი ეკუთვნის ამ მომხმარებელს

#### სესხის წაშლა

```http
DELETE /api/loan/user/{id}
Authorization: Bearer {user-token}
```

**პირობები**:
- სესხის Status უნდა იყოს Processing

---

### სესხი - ბუღალტერი

#### ყველა სესხის ნახვა

```http
GET /api/loan/accountant/all
Authorization: Bearer {accountant-token}
```

**პასუხი**: 200 OK - ყველა სესხი

#### სესხის განახლება (ნებისმიერი სტატუსი)

```http
PUT /api/loan/accountant/{id}
Authorization: Bearer {accountant-token}
Content-Type: application/json

{
  "loanType": 0,
  "amount": 8000.00,
  "currency": "EUR",
  "loanPeriod": 36
}
```

**უპირატესობა**: ნებისმიერი სტატუსის სესხი განახლდება

#### სესხის წაშლა

```http
DELETE /api/loan/accountant/{id}
Authorization: Bearer {accountant-token}
```

---

## ავტენტიკაცია და დავალების მართვა

### JWT Token

Token შედგება:
- **NameIdentifier**: მომხმარებელი ID
- **Name**: მომხმარებელი სახელი
- **Role**: User ან Accountant
- **Expiry**: 3 საათი

Token გამოყენება:
```http
Authorization: Bearer {token}
```

### Role-ორიენტირებული წვდომა

- `[Authorize]` - ნებისმიერი ავტორიზებული
- `[Authorize(Roles = "User")]` - მხოლოდ User
- `[Authorize(Roles = "Accountant")]` - მხოლოდ Accountant

---

## სესხის მართვის წესები

### მომხმარებელი შეუძლია:

1. მხოლოდ საკუთარი სესხების ნახვა
2. Processing სტატუსის სესხის განახლება (Amount, Currency, LoanPeriod)
3. Processing სტატუსის სესხის წაშლა
4. თავის მონაცემების ნახვა

### მომხმარებელი არ შეუძლია:

1. სხვა მომხმარებლის სესხის ნახვა
2. სესხის Status შეცვლა
3. სესხის მოთხოვნა თუ დაბლოკილია

### ბუღალტერი შეუძლია:

1. ყველა სესხის ნახვა
2. ნებისმიერი სესხის განახლება ნებისმიერი სტატუსით
3. ნებისმიერი სესხის წაშლა
4. მომხმარებელი დაბლოკვა/განბლოკვა

---

## შეყვანილი მონაცემის შემოწმება

### რეგისტრაციაზე

- **FirstName**: სავალდებულო, მაქსიმუმ 50 სიმბოლო
- **LastName**: სავალდებულო, მაქსიმუმ 50 სიმბოლო
- **UserName**: სავალდებულო, მაქსიმუმ 50 სიმბოლო, უნიკალური
- **Email**: სწორი ელფოსტის ფორმატი, უნიკალური
- **Age**: მინიმუმ 18 წელი
- **MonthlyIncome**: მეტი ვიდრე 0
- **Password**: მინიმუმ 6 სიმბოლო

### სესხის შექმნაზე

- **Amount**: მეტი ვიდრე 0
- **Currency**: 3 ასოიანი ISO კოდი (GEL, USD, EUR)
- **LoanPeriod**: მეტი ვიდრე 0 (თვეების რაოდენობა)
- **LoanType**: სწორი enum (FastLoan, AutoLoan, Installment)

---

## დაშიფვრა და უსაფრთხოება

### პაროლი

პაროლები დაშიფრულია BCrypt ალგორითმით:

**რეგისტრაცია**: `PasswordHash = BCrypt.HashPassword(password)`

**ლოგინი**: `BCrypt.Verify(password, PasswordHash)`

BCrypt უპირატესობები:
- Salt-სთან ერთად ჰეშირება
- Brute-force თავდასხმის წინააღმდეგ დაცული
- ინდუსტრიის სტანდარტი

---

## ტესტები

### გაშვება

```bash
cd LoanApi.Tests
dotnet test
```

### ტესტის დაფა

**UserServiceTests** (8 ტესტი):
- RegisterAsync სწორი მონაცემით ✓
- RegisterAsync დუბლიკატი username ✓
- LoginAsync სწორი ხელმოწერა ✓
- LoginAsync არასწორი პაროლი ✓
- LoginAsync დაბლოკილი user ✓
- GetByIdAsync user დაბრუნება ✓
- BlockUserAsync დაბლოკვა ✓
- BlockUserAsync user ვერ ნაპოვნი ✓

**LoanServiceTests** (10 ტესტი):
- CreateLoanAsync სესხი შედგება ✓
- CreateLoanAsync user ვერ ნაპოვნი ✓
- CreateLoanAsync დაბლოკილი user ✓
- GetUserLoansAsync user-ის სესხები ✓
- UpdateLoanAsync განახლება ✓
- UpdateLoanAsync არასწორი user ✓
- UpdateLoanAsync არასწორი სტატუსი ✓
- DeleteLoanAsync წაშლება ✓
- DeleteLoanAsync არასწორი სტატუსი ✓
- GetAllLoansAsync ყველა სესხი ✓

---

## მნიშვნელოვანი კლასები და მათი მოვალეობა

### Services

**UserService**: რეგისტრაცია, ლოგინი, მომხმარებელი მოძებნა, დაბლოკვა

**LoanService**: სესხის CRUD ოპერაციები, სტატუსის შემოწმება

**JwtService**: Token გენერირება

### Controllers

**UserController**: /api/user/* ენდპოინტები

**LoanController**: /api/loan/* ენდპოინტები

### Data

**LoanDbContext**: Entity Framework კონტექსტი, Users და Loans ცხრილი

### Validators

**FluentValidation**: შეყვანილი მონაცემის შემოწმება

### Middleware

**ExceptionMiddleware**: გლობალური შეცდომების დამუშავება

---

## HTTP სტატუსის კოდები

- **200 OK**: წარმატებული მოთხოვნა
- **400 Bad Request**: შეყვანილი მონაცემის ან ბიზნეს ლოგიკის შეცდომა
- **401 Unauthorized**: Token აკლია ან არასწორია
- **403 Forbidden**: Token OK, მაგრამ წვდომა აკრძალული
- **404 Not Found**: რესურსი ვერ მოიძებნა

---

## Swagger გამოყენება

1. აპლიკაცია გაშვება: `dotnet run`
2. ბრაუზერი: `http://localhost:5090/swagger`
3. Authorize ღილაკი (ზე მარჯვნივ)
4. /api/user/login რეკვეზიტი, token მიღება
5. Authorize: `Bearer {token}`
6. სხვა ენდპოინტები ტესტირება

---

## ლოგირება

Serilog კამერა აკუმულირებს ლოგებს:

- **Console**: რეალ-დროში ხედვა
- **File**: `Logs/log-YYYYMMDD.txt` (ყოველ დღეს ახალი)

Log დონეები: Information, Warning, Error

---

## რესურსები

- GitHub: https://github.com/GKutubidze/LoanApi
- .NET Documentation: https://docs.microsoft.com/en-us/dotnet/
- Entity Framework: https://docs.microsoft.com/en-us/ef/