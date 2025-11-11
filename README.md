FileIntake: AI-Powered Document Analysis Platform

🎯 Project Goal

FileIntake is a web application designed to streamline the process of handling and analyzing complex documents. The core function is to allow users to securely upload PDF files, which are then processed and analyzed using advanced Artificial Intelligence (AI) models to extract key insights, summaries, and structured data.

🛠 Technology Stack

This project is built on the modern .NET ecosystem, utilizing robust, scalable, and secure technologies.

Component

Technology

Purpose

Backend

C# / ASP.NET Core 8.0

High-performance API and server-side logic.

Database

Entity Framework Core (EF Core)

Object-Relational Mapper (ORM) for data access.

Authentication

ASP.NET Core Identity

Secure user registration, login, and role management.

UI

HTML, CSS, JavaScript

Frontend presentation (using MVC/Razor Pages).

Future AI

Google Gemini API (Placeholder)

Processing uploaded documents for content analysis.

✨ Features

Current Features (Setup Complete)

Secure Authentication: User accounts are managed via ASP.NET Core Identity, ensuring personalized and protected access.

MVC Structure: The application is configured with a standard Model-View-Controller pattern, ready for development.

Database Integration: Configured to use SQL Server via EF Core for persistent storage.

Planned Features

PDF Ingestion: Secure endpoint for uploading and storing PDF documents.

AI Analysis: Integration with AI services to perform tasks such as summarization, sentiment analysis, and Q&A on ingested files.

User Dashboard: A personal area for users to view their uploaded files and the results of the AI analysis.

🚀 Getting Started

Prerequisites

.NET 8.0 SDK

$$SQL Server LocalDB or full instance$$

A code editor (e.g., Visual Studio or VS Code)

Setup & Run

Clone the Repository:

git clone [https://github.com/JohnnyGitH/FileIntake.git](https://github.com/JohnnyGitH/FileIntake.git)
cd FileIntake


Restore Dependencies:

dotnet restore


Update Database Connection:
Ensure your appsettings.json file contains a valid DefaultConnection string pointing to your SQL Server instance.

Apply Migrations:
Since Identity is enabled, you need to create the database structure.

dotnet ef database update


Run the Application:

dotnet run


The application will typically start on https://localhost:7001 or a similar port.