# EGAIS Inspector

Премиальное WPF-приложение для анализа ЕГАИС: УТМ, марки, справки А/Б, ТТН, остатки, движение, сертификаты и отчёты.

## Стек

- C# 12 / .NET 8 / WPF
- MVVM / CommunityToolkit.Mvvm
- WPF UI 4.3 Fluent Design
- EF Core 8 + SQLite
- ZXing.Net
- ClosedXML / QuestPDF
- Octokit.NET / Velopack
- Serilog

## Сборка

```powershell
dotnet restore .\EGAISInspector.sln
dotnet build .\EGAISInspector.sln -c Release
dotnet test .\EGAISInspector.sln -c Release
```

Приложение рассчитано на Windows 10/11. УТМ указывается вручную, ключи КЭП и приватные ключи в БД не сохраняются.

## Безопасность

GitHub-токены должны храниться через Windows DPAPI. Секреты, приватные ключи и содержимое КЭП запрещено коммитить в репозиторий.

## Статус

Начальный вертикальный срез создан: Core, HTTP-клиент УТМ, парсер марок, SQLite/EF Core, сертификаты Windows, XML-запросы ЕГАИС, Fluent UI shell и CI. Следующий этап — полноценные ViewModels/страницы, парсинг ReplyFormA/ReplyFormB/WayBill и поток данных УТМ → БД → аналитика.
