# Kaopiz-Lab

Kaopiz-Lab is a RESTful backend built with **.Net**, designed for a blogging platform. It handles user authentication.

> Technologies: NET · Csharp · EFcore · JWT · MySql

---

## 🚀 Features

- User authentication with JWT via access and refresh token (login/signup)
- Secure access to protected routes
---

## 🛠️ Tech Stack

| Tech             | Purpose                         |
|------------------|----------------------------------|
| ASP .NET      | Backend framework          |
| Identity  | Authentication & authorization  |
| JWT              | Token-based user auth           |
| EF Core  | ORM for data persistence        |
| MySQL      |  DB        |
| Redis      |  For BlacklistToken, RefreshToken (Future)        |

---

## 📦 Getting Started

### 0. Demo Swagger (Recommend But Slowly)

https://kaopiz-lab.onrender.com/swagger/index.html
```bash
https://kaopiz-lab.onrender.com/swagger/index.html
```


### 1. Clone the Repository

```bash
git clone https://github.com/kh0adev/kaopiz-lab.git
cd kblog-api
```

### 2. Run the Application

Make sure you have .NET 9

```bash
dotnet run
```
## ⚙️ Configuration

You can customize environment variables inside application.properties:

```properties
"ConnectionStrings": {
    "DefaultConnection": "Server=yourserver;Database=yourdatabase;Uid=yourid;Pwd=yourpassword"
  },
```

## 🔐 Authentication

### ✅ Signup

**POST** `/api/register`

**Request Body:**
```json
{
  "userName": "john",
  "password": "password123",
  "email": "John@abc"
}
```
### 🔑 Login

 
**POST** `/api/login`

**Response:**
```json
{
  "token": "JWT_TOKEN_HERE"
}
```

## 📘 API Endpoints

### 🧑 Auth

| Method | Endpoint             | Description           |
|--------|----------------------|-----------------------|
| POST   | /api/register   | Register new user     |
| POST   | /api/login      | Login and get JWT     |
| DELETE   | /api/logout      | Logout     |
| POST   | /api/refresh      | Login via refreshToken     |
| GET   | /api/me      | Get UserName     |

## 🧑‍💻 Contributing

Contributions, issues, and feature requests are welcome!  
Feel free to open a pull request or submit an issue.


## 📬 Contact

Created by @kh0adev

Feel free to reach out!
