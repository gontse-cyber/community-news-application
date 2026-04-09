# Community News Application

A C# console application for managing, moderating, and viewing community news submissions. Users can register, submit news articles for moderation, and browse approved community news across multiple categories.

**Course:** PRG281 - Second Year Group Project (Programming)  
**Language:** C# (.NET Framework)  
**Type:** Console Application  
**My Role:** Backend Developer (Content Moderation System, News Management)

---

## 📋 Overview

The Community News Application is a console-based system that manages community news submissions with content moderation, user authentication, and category-based news browsing. The application implements object-oriented design patterns including inheritance, interfaces, custom exceptions, and event handling.

### Key Features

- **User Registration & Authentication** - Register users with password hashing
- **News Submission** - Submit news articles with titles, content, categories
- **Content Moderation** - Automated moderation with hate speech detection
- **Content Validation** - Input validation and security checks
- **News Management** - Store, organize, and retrieve news by category
- **News Browsing** - View approved news with full article display
- **Error Handling** - Custom exceptions and error messages
- **Event System** - Event-driven architecture for content flagging and user actions

---

## 🏗️ Architecture & Design Patterns

### Technology Stack
- **Language:** C# 
- **.NET Framework:** Console Application
- **Design Patterns:** 
  - Abstract classes and inheritance
  - Interfaces (INewsItem, IModeratable)
  - Custom exceptions
  - Event delegates and EventArgs
  - Reflection (for data initialization)

### Object-Oriented Design

The application demonstrates solid OOP principles:

```
INewsItem (Interface)
    ↓
NewsItemBase (Abstract Class)
    ↓
CommunityNewsItem (Concrete Class)
```

---

## 📁 Project Structure

```
CommunityNewsApplication/
├── Program.cs                           # All application logic (562 lines)
├── CommunityNewsApplication.csproj      # Project configuration
└── bin/, obj/                          # Build files (ignored by Git)
```

---

## 🔧 My Backend Contributions

### 1. **Content Validation System**

Implemented comprehensive validation for all news submissions:

**Validation Logic:**
- Empty field checking for title, content, location
- Hate speech detection using prohibited terms list
- Custom exception throwing for invalid content
- Inheritance of validation in derived classes

### 2. **Hate Speech Detection Algorithm**

Built a content filtering system that detects prohibited terms:

**Features:**
- Case-insensitive matching
- Uses `.Any()` LINQ method for efficient checking
- Flagged content triggers event notifications
- Prevents publication of harmful content

### 3. **Content Moderation Engine**

Designed the `ContentModerator` class that reviews submissions:

**Responsibilities:**
- Invokes validation on news items
- Manages approval status
- Can be extended with additional review criteria
- Works with interface contract (INewsItem)

### 4. **News Management System**

Built the `NewsManager` class that handles all news operations:

**Key Methods:**
- `AddNewsItem()` - Add new submissions to collection
- `GetApprovedNews()` - Retrieve approved news only
- `GetNewsByCategory()` - Filter news by category
- `GetNewsItemById()` - Retrieve specific articles

**Features:**
- LINQ queries for filtering (`.Where()`, `.FirstOrDefault()`)
- Maintains private `_newsItems` collection
- Returns only approved news to users
- Manages news lifecycle

### 5. **User Authentication System**

Implemented registration and user verification:

**Security Features:**
- Password hashing with `SecurityHelper.HashPassword()`
- Duplicate username prevention
- User existence verification before news submission
- Unregistered user event triggering

### 6. **Security Helper Utilities**

Created the `SecurityHelper` static class:

**Methods:**
- `HashPassword(string password)` - Cryptographic password hashing
- `SanitizeInput(string input)` - Input sanitization for XSS prevention

**Purpose:**
- Applied to all user inputs (title, content, author)
- Uses `System.Security.Cryptography` namespace
- Prevents injection attacks

### 7. **Custom Exception Hierarchy**

Designed exception system for specific error handling:

**Benefits:**
- Specific exception types for different errors
- Inheritance chain for selective catching
- Descriptive error messages
- Better error tracking and logging

### 8. **Event-Driven Architecture**

Implemented custom events for content flagging:

**Events Used:**
- `NewsPublishedEventHandler` - When news is published
- `ContentFlaggedEventHandler` - When hate speech detected
- `NewsSelectedEventHandler` - When user selects news
- `UnregisteredUserAttempt` - When unregistered user tries to submit

### 9. **Error Handling with Enums**

Built enum validation system:

**Features:**
- Validates user-selected categories
- Uses `Enum.IsDefined()` for safe casting
- Provides clear error messages
- Prevents invalid category assignments

---

## 🎯 Data Flow

### News Submission Flow

```
User Input (Console)
    ↓
Input Validation (null/empty checks)
    ↓
SecurityHelper.SanitizeInput()
    ↓
Create CommunityNewsItem
    ↓
ValidateContent() - Check for hate speech
    ↓
ContentModerator.Review()
    ↓
NewsManager.AddNewsItem()
    ↓
User/Admin Notification (Event triggered)
    ↓
News stored in _newsItems collection
```

### News Browsing Flow

```
User selects category
    ↓
NewsManager.GetNewsByCategory()
    ↓
Filter approved news only
    ↓
Display list to user
    ↓
User selects specific news
    ↓
DisplayNewsContent()
    ↓
Show full article with metadata
```

---

## 🛠️ Technical Implementation

### Key Language Features Used

**Object-Oriented Programming:**
- Abstract classes and inheritance
- Interfaces and contracts
- Polymorphism (virtual methods)
- Encapsulation (public/private members)

**Data Structures:**
- `List<T>` for news collections
- `Dictionary<string, string>` for user database
- Collections in prohibited terms list

**Advanced C# Features:**
- Custom exceptions hierarchy
- Event delegates and EventArgs
- LINQ queries (`.Where()`, `.Any()`, `.FirstOrDefault()`)
- Reflection for field access
- Nullable reference types (`?.`)
- String interpolation

**Security Practices:**
- Password hashing (not plaintext)
- Input sanitization
- Hate speech filtering
- User authentication requirements

---

## 🚀 How to Use

### Running the Application

1. Clone from GitHub (or run locally)
2. Open in Visual Studio or build from command line
3. Run the executable
4. Follow the console menu

### Sample Data

The application includes 4 sample news items:
1. **Community Women's Month Event** - Community category
2. **Heat Front Weather Alert** - Weather category
3. **Arsenal vs Manchester United** - Sports category
4. **Tech Fair 2025** - Technology category

### Test Credentials
- **Username:** admin
- **Password:** password

### User Actions

1. **Register:** Create new account with username/password
2. **Submit News:** Enter title, content, category (requires registration)
3. **Browse News:** View all approved news by category
4. **Read Article:** Select specific news to view full content

---

## 🎓 Learning Outcomes

This project demonstrates:

1. **Object-Oriented Design**
   - Inheritance and polymorphism
   - Interface contracts
   - Abstract base classes

2. **Data Management**
   - Collection management with LINQ
   - Dictionary for key-value storage
   - Entity relationships

3. **Security Concepts**
   - Password hashing
   - Input validation
   - Content filtering
   - Authorization checks

4. **C# Advanced Features**
   - Custom exceptions
   - Event system
   - Reflection
   - Delegates and EventArgs
   - LINQ queries

5. **Design Patterns**
   - Repository pattern
   - Observer pattern
   - Strategy pattern

6. **Error Handling**
   - Try-catch-finally
   - Custom exception hierarchies
   - Graceful error messaging

---

## 👥 Group Members

This was a combined group effort for a second-year C# programming course. My contribution focused on the backend systems including content moderation, validation, security, news management, and the overall application architecture.


**Status:** Educational Project - Complete  
**Last Updated:** April 2026  
**Version:** 1.0.0
