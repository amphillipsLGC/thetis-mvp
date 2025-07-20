# Thetis Solution

Thetis is a multi-project solution built with C# and ASP\.NET Core, integrating an Angular frontend. The solution is organized into several projects for modularity and maintainability.

## Projects

- `Thetis.Web`: ASP\.NET Core web application serving the API and static frontend.
- `Thetis.Users`: User module.
- `Thetis.Profiles`: Profile module.
- `Thetis.Common`: Shared code and utilities.
- `Thetis.AspireAppHost`: Application hosting and orchestration.

## Serving Angular Static Files

The Angular frontend is built using the new builder `@angular-devkit/build-angular:application`, which outputs the compiled app to `wwwroot/browser` by default.

### Static File Middleware

The backend uses a custom extension method to serve static files from the `browser` folder:

```csharp
app.UseBrowserStaticFiles(builder.Environment.ContentRootPath);
```

This encapsulates the static file and default file configuration, keeping `Program.cs` clean.

### How it works

- Serves static and default files (e\.g\. `index.html`) from `wwwroot/browser`.
- Ensures requests for frontend assets are handled correctly.

## Angular Build Integration

The `.csproj` for `Thetis.Web` includes a custom MSBuild target to build the Angular app before the backend:

```xml
<Target Name="BuildAngular" BeforeTargets="Build">
    <Exec Command="npm install" WorkingDirectory="../Thetis.UI" />
    <Exec Command="npm run build -- --configuration $(AngularBuildConfiguration)" WorkingDirectory="../Thetis.UI" />
</Target>
```

- **`AngularBuildConfiguration`**: Switches between Angular build modes based on the .NET configuration.
- **Output**: Places the Angular build in `wwwroot/browser` for the backend to serve.

---

This setup keeps frontend and backend builds integrated and ensures static files are served correctly from the location where Angular outputs them.