# Contributing to Cortiqa .NET SDK

We welcome contributions to `Cortiqa.Sdk`!

---

## Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)

---

## Build & Test

1. **Clone the repository:**
   ```bash
   git clone https://github.com/cortiqa-ai/cortiqa-sdk-csharp.git
   cd cortiqa-sdk-csharp
   ```

2. **Build the solution:**
   ```bash
   dotnet build
   ```

3. **Run unit tests:**
   ```bash
   dotnet test
   ```

---

## Coding Standards
- C# 12 / 10 modern idioms.
- Keep external package dependencies strictly at zero for runtime targets (only `System.Text.Json`).
- Provide XML doc comments on all public API methods and types.
