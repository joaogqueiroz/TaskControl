# TaskControl

A full-stack task manager built with ASP.NET Core MVC. People create an account, register and edit their tasks with a date, time and priority, see their tasks in charts, and export reports to PDF.

## Features

- **Accounts:** registration, login with cookie authentication, password change and password recovery by email. Passwords are hashed with ASP.NET Core Identity's `PasswordHasher` (PBKDF2 with a per-user salt).
- **Tasks:** create, list, edit and delete tasks with a name, description, date, time and priority.
- **Dashboard:** Highcharts charts of the user's tasks.
- **Reports:** PDF reports of tasks in a date range, generated with iText 7.

## Architecture

```
AspNetMVCproject03           ASP.NET Core MVC app: controllers, view models, Razor views
AspNetMVCproject03.Data      Entities and repositories with Dapper over SQL Server
AspNetMVCproject03.Reports   PDF report generation (iText 7)
AspNetMVCproject03.Messeges  Email service used for password recovery (SMTP)
```

## Tech stack

C# · .NET 8 · ASP.NET Core MVC · Razor · Dapper · SQL Server · iText 7 · Highcharts · Bootstrap 5 · jQuery

## Running locally

Requirements: .NET 8 SDK and SQL Server (LocalDB works).

1. Create the database and run `SQLQuerys.sql` to create the `USER_TB` and `TASK_TB` tables.
2. Set `ConnectionStrings:DB_context` in `AspNetMVCproject03/appsettings.json` to your database.
3. To enable password recovery, fill in the SMTP settings in `AspNetMVCproject03.Messeges/EmailServiceMessage.cs`.
4. Run it:

```sh
dotnet run --project AspNetMVCproject03
```
