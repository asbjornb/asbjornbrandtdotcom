# NuGet

## What is a `.nupkg`?

A **NuGet package is just a ZIP archive** with a different extension. To look inside:

```powershell
# Copy or rename first (Expand-Archive only accepts .zip)
Copy-Item .\MyLib.1.2.3.nupkg .\MyLib.zip
Expand-Archive -Path .\MyLib.zip -DestinationPath .\pkg -Force
```

Inside you'll find:
- `/lib/<TFM>/...dll` – the assemblies that get referenced
- `<id>.nuspec` – XML describing ID, version, dependencies, git commit/branch, etc.

## Inspect Cached Packages

Search your local NuGet cache for specific packages:

```powershell
Get-ChildItem -Path $env:USERPROFILE -Recurse -Filter *.nupkg | 
Where-Object { $_.Name -match "ChronoStream\.(Serialization|Dtos).*" } | 
Select-Object FullName
```

## Keep Project References Inside One Package

If you reference another project in the same solution but don't want a separate NuGet dependency, add `PrivateAssets="all"` to the `<ProjectReference>`:

```xml
<ItemGroup>
  <ProjectReference Include="..\ChronoStream.Serialization\ChronoStream.Serialization.csproj" 
                    PrivateAssets="all" />
</ItemGroup>
```

With `<IncludeReferencedProjects>true</IncludeReferencedProjects>` in your project file, the DLL gets embedded in the parent package without creating a dependency in the `.nuspec`.

## Pack Locally

```bash
# Default version comes from the .csproj
dotnet pack ChronoStream.Client/ChronoStream.Client.csproj -c Release -o ./nupkgs

# Override version on command line
dotnet pack ChronoStream.Client/ChronoStream.Client.csproj -c Release -o ./nupkgs -p:Version=3.1.4
```

## Push to GitLab NuGet Feed

*Note: GitLab only lets you push to project-level feeds (group feeds are pull-only)*

```powershell
dotnet nuget push ".\nupkgs\ChronoStream.Client.7.0.3.nupkg" `
  --source "https://gitlab.com/api/v4/projects/<project_id>/packages/nuget/index.json" `
  --api-key "<your_PAT_or_CI_TOKEN>" `
  --skip-duplicate
```

Replace `<project_id>` with the number shown in **Project › Settings › General**.

## Include Source Code

To include source files in your package for debugging:

```bash
dotnet pack -c Release -o ./nupkgs --include-source
```

This creates both the regular package and a symbols package (`.snupkg`) containing source files.