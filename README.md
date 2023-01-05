# About

This repo is ASP.NET templates by Clean Architecture And CQRS Pattern (The Command and Query Responsibility Segregation) for Visual Studio and Rider. Templates configured to run on .net7.

# Nuget packages
- MediatR.Extensions.Microsoft.DependencyInjection
- FluentValidation
- FluentValidation.DependencyInjectionExtensions
- Serilog.AspNetCore
- Serilog.Sinks.MSSqlServer
- Microsoft.AspNetCore.Http.Features
- Microsoft.Data.SqlClient
- Microsoft.EntityFrameworkCore
- EFCore.BulkExtensions
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Tools
- Microsoft.EntityFrameworkCore.Design
- Swashbuckle.AspNetCore
- NewId
- Mapster

# INSTALL

#### Visual Studio
Copy zip from "VisualStudioTemplate" to ..\%UserProfileFolder%\Documents\Visual Studio {version}\Templates\ProjectTemplates.
If Visual Studio is open, you shoud reopen it.


#### Rider
When you create new solution select "More Template", then "Install template", then choose folder "RiderTemplate" from this repo. And then press "Reload" button.
![This is an image](https://i.ibb.co/dGdH2Xk/Screenshot-3.png)

# Notes

Logger is commented by default in Program.cs:
//CreateLogger(builder.Logging);
