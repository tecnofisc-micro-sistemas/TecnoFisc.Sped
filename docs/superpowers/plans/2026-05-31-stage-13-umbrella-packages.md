# Stage 13 Umbrella Packages Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Criar os pacotes guarda-chuva `TecnoFisc.Sped.Txt` e `TecnoFisc.Sped`, sem codigo proprio, para agregar os leiautes textuais ja publicados.

**Architecture:** Stage 13 e o passo 4 da Stage 18 adicionam apenas projetos de empacotamento na camada 4. `TecnoFisc.Sped.Txt` referencia os pacotes TXT existentes (`EfdContribuicoes`, `EfdIcmsIpi`, `Ecd`); `TecnoFisc.Sped` referencia `TecnoFisc.Sped.Txt` agora e passa a referenciar `TecnoFisc.Sped.Xml` somente depois do CT-e/Stage 16. Nao criar `TecnoFisc.Sped.Xml` nesta slice e nao referenciar `NFeNFCe` direto pelo pacote geral.

**Tech Stack:** .NET 10 SDK, MSBuild SDK-style projects, NuGet pack metadata via `Directory.Build.props`, xUnit v3 + FluentAssertions para invariantes de empacotamento.

---

## Scope

Implementar agora:

- `src/TecnoFisc.Sped.Txt/TecnoFisc.Sped.Txt.csproj`
- `src/TecnoFisc.Sped/TecnoFisc.Sped.csproj`
- entrada dos dois projetos em `TecnoFisc.Sped.slnx`
- teste de invariantes para garantir que os guarda-chuvas continuam sem codigo e com referencias corretas
- atualizacao de `README.md`, `ARCHITECTURE.md` e `sped/STAGE_18_REORG.md`

Fora do escopo:

- `src/TecnoFisc.Sped.Xml/` antes do CT-e
- qualquer classe, namespace, facade, factory ou API nova nos guarda-chuvas
- mudanca de parser, sniffer, source generator ou leiautes

## File Structure

- Create: `src/TecnoFisc.Sped.Txt/TecnoFisc.Sped.Txt.csproj`
  - Projeto packable sem arquivos `.cs`.
  - Agrega os projetos TXT existentes via `ProjectReference`; `dotnet pack` transforma essas referencias em dependencias NuGet.
- Create: `src/TecnoFisc.Sped/TecnoFisc.Sped.csproj`
  - Projeto packable sem arquivos `.cs`.
  - Agrega `TecnoFisc.Sped.Txt` por `ProjectReference`.
  - Nao referencia `NFeNFCe` direto; o caminho XML entra depois via `TecnoFisc.Sped.Xml`.
- Modify: `TecnoFisc.Sped.slnx`
  - Adicionar os dois projetos na pasta `/src/`, antes de `Core`, para refletir a camada 4 ja documentada.
- Create: `tests/TecnoFisc.Sped.Core.Tests/Packaging/UmbrellaPackageTests.cs`
  - Testa os `.csproj` como XML e o sistema de arquivos.
  - Garante referencias esperadas, ausencia de `TecnoFisc.Sped.Xml` e ausencia de `.cs` nos dois projetos.
- Modify: `README.md`
  - Atualizar tabela de pacotes e arvore de projetos.
  - Documentar quando usar pacote especifico, `TecnoFisc.Sped.Txt` ou `TecnoFisc.Sped`.
- Modify: `ARCHITECTURE.md`
  - Marcar o recorte atual da Stage 13: TXT + geral agora; XML depois do CT-e.
  - Ajustar o texto que ainda fala como se `TecnoFisc.Sped.Xml` ja existisse.
- Modify: `sped/STAGE_18_REORG.md`
  - Marcar o passo 4 como concluido para `Txt` + `Sped`, deixando a pendencia explicita para `Xml`.

## Task 1: Add Packaging Invariant Tests

**Files:**
- Create: `tests/TecnoFisc.Sped.Core.Tests/Packaging/UmbrellaPackageTests.cs`

- [ ] **Step 1: Write the failing tests**

Create `tests/TecnoFisc.Sped.Core.Tests/Packaging/UmbrellaPackageTests.cs` with:

```csharp
using System.Xml.Linq;

namespace TecnoFisc.Sped.Core.Tests.Packaging;

public sealed class UmbrellaPackageTests
{
    [Fact]
    public void TxtUmbrella_ReferenciaSomenteLeiautesTxtExistentes()
    {
        var project = LoadProject("src", "TecnoFisc.Sped.Txt", "TecnoFisc.Sped.Txt.csproj");

        ProjectReferences(project).Should().BeEquivalentTo(
            [
                @"..\TecnoFisc.Sped.EfdContribuicoes\TecnoFisc.Sped.EfdContribuicoes.csproj",
                @"..\TecnoFisc.Sped.EfdIcmsIpi\TecnoFisc.Sped.EfdIcmsIpi.csproj",
                @"..\TecnoFisc.Sped.Ecd\TecnoFisc.Sped.Ecd.csproj",
            ]);

        PackageReferences(project).Should().BeEmpty("guarda-chuva local deve agregar projetos do repositorio");
        Directory.GetFiles(ProjectDirectory("src", "TecnoFisc.Sped.Txt"), "*.cs", SearchOption.AllDirectories)
            .Should().BeEmpty("guarda-chuvas nao carregam codigo proprio");
    }

    [Fact]
    public void RootUmbrella_ReferenciaSomenteTxtAteXmlUmbrellaExistir()
    {
        var project = LoadProject("src", "TecnoFisc.Sped", "TecnoFisc.Sped.csproj");

        ProjectReferences(project).Should().BeEquivalentTo(
            [
                @"..\TecnoFisc.Sped.Txt\TecnoFisc.Sped.Txt.csproj",
            ]);

        ProjectReferences(project).Should().NotContain(reference => reference.Contains("NFeNFCe", StringComparison.Ordinal));
        ProjectReferences(project).Should().NotContain(reference => reference.Contains("Xml", StringComparison.Ordinal));
        PackageReferences(project).Should().BeEmpty("o pacote geral tambem deve agregar por ProjectReference local");
        Directory.GetFiles(ProjectDirectory("src", "TecnoFisc.Sped"), "*.cs", SearchOption.AllDirectories)
            .Should().BeEmpty("guarda-chuvas nao carregam codigo proprio");
    }

    [Fact]
    public void XmlUmbrella_AindaNaoExisteAntesDoCte()
    {
        Directory.Exists(ProjectDirectory("src", "TecnoFisc.Sped.Xml"))
            .Should().BeFalse("Stage 13 adia TecnoFisc.Sped.Xml ate haver NFeNFCe + CTe");
    }

    private static XDocument LoadProject(params string[] pathSegments)
        => XDocument.Load(Path.Combine(RepositoryRoot(), Path.Combine(pathSegments)));

    private static string ProjectDirectory(params string[] pathSegments)
        => Path.Combine(RepositoryRoot(), Path.Combine(pathSegments));

    private static string[] ProjectReferences(XDocument project)
        => project.Descendants("ProjectReference")
            .Select(element => element.Attribute("Include")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static string[] PackageReferences(XDocument project)
        => project.Descendants("PackageReference")
            .Select(element => element.Attribute("Include")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "TecnoFisc.Sped.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Repository root not found from test output directory.");
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run:

```powershell
dotnet test tests/TecnoFisc.Sped.Core.Tests/TecnoFisc.Sped.Core.Tests.csproj --filter UmbrellaPackageTests
```

Expected: FAIL because `src/TecnoFisc.Sped.Txt/TecnoFisc.Sped.Txt.csproj` and `src/TecnoFisc.Sped/TecnoFisc.Sped.csproj` do not exist yet.

- [ ] **Step 3: Commit failing tests**

```powershell
git add tests/TecnoFisc.Sped.Core.Tests/Packaging/UmbrellaPackageTests.cs
git commit -m "test: cobrir invariantes dos guarda-chuvas Stage 13"
```

## Task 2: Create Umbrella Projects

**Files:**
- Create: `src/TecnoFisc.Sped.Txt/TecnoFisc.Sped.Txt.csproj`
- Create: `src/TecnoFisc.Sped/TecnoFisc.Sped.csproj`
- Modify: `TecnoFisc.Sped.slnx`

- [ ] **Step 1: Create `TecnoFisc.Sped.Txt` project**

Create `src/TecnoFisc.Sped.Txt/TecnoFisc.Sped.Txt.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <Description>Pacote guarda-chuva dos leiautes SPED textuais da TecnoFisc: EFD Contribuições, EFD ICMS-IPI e ECD em uma única dependência NuGet.</Description>
    <PackageId>TecnoFisc.Sped.Txt</PackageId>
    <PackageTags>sped;txt;efd-contribuicoes;efd-icms-ipi;ecd;fiscal;brasil;tecnofisc</PackageTags>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\TecnoFisc.Sped.EfdContribuicoes\TecnoFisc.Sped.EfdContribuicoes.csproj" />
    <ProjectReference Include="..\TecnoFisc.Sped.EfdIcmsIpi\TecnoFisc.Sped.EfdIcmsIpi.csproj" />
    <ProjectReference Include="..\TecnoFisc.Sped.Ecd\TecnoFisc.Sped.Ecd.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Create root `TecnoFisc.Sped` project**

Create `src/TecnoFisc.Sped/TecnoFisc.Sped.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <Description>Pacote guarda-chuva da família TecnoFisc.Sped. Nesta etapa agrega os leiautes SPED textuais via TecnoFisc.Sped.Txt; o guarda-chuva XML será incorporado após o CT-e.</Description>
    <PackageId>TecnoFisc.Sped</PackageId>
    <PackageTags>sped;fiscal;brasil;tecnofisc;efd-contribuicoes;efd-icms-ipi;ecd</PackageTags>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\TecnoFisc.Sped.Txt\TecnoFisc.Sped.Txt.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 3: Add projects to solution**

Modify the `/src/` folder in `TecnoFisc.Sped.slnx` so its first entries are:

```xml
  <Folder Name="/src/">
    <Project Path="src/TecnoFisc.Sped/TecnoFisc.Sped.csproj" />
    <Project Path="src/TecnoFisc.Sped.Txt/TecnoFisc.Sped.Txt.csproj" />
    <Project Path="src/TecnoFisc.Sped.Core/TecnoFisc.Sped.Core.csproj" />
    <Project Path="src/TecnoFisc.Sped.Txt.Engine/TecnoFisc.Sped.Txt.Engine.csproj" />
    <Project Path="src/TecnoFisc.Sped.Txt.Engine.SourceGenerators/TecnoFisc.Sped.Txt.Engine.SourceGenerators.csproj" />
    <Project Path="src/TecnoFisc.Sped.Xml.Engine/TecnoFisc.Sped.Xml.Engine.csproj" />
    <Project Path="src/TecnoFisc.Sped.EfdContribuicoes/TecnoFisc.Sped.EfdContribuicoes.csproj" />
    <Project Path="src/TecnoFisc.Sped.EfdIcmsIpi/TecnoFisc.Sped.EfdIcmsIpi.csproj" />
    <Project Path="src/TecnoFisc.Sped.Ecd/TecnoFisc.Sped.Ecd.csproj" />
    <Project Path="src/TecnoFisc.Sped.NFeNFCe/TecnoFisc.Sped.NFeNFCe.csproj" />
  </Folder>
```

- [ ] **Step 4: Run targeted tests**

Run:

```powershell
dotnet test tests/TecnoFisc.Sped.Core.Tests/TecnoFisc.Sped.Core.Tests.csproj --filter UmbrellaPackageTests
```

Expected: PASS.

- [ ] **Step 5: Build solution**

Run:

```powershell
dotnet build TecnoFisc.Sped.slnx
```

Expected: PASS with 0 warnings and 0 errors.

- [ ] **Step 6: Commit project creation**

```powershell
git add src/TecnoFisc.Sped.Txt/TecnoFisc.Sped.Txt.csproj src/TecnoFisc.Sped/TecnoFisc.Sped.csproj TecnoFisc.Sped.slnx
git commit -m "feat: criar guarda-chuvas Txt e Sped"
```

## Task 3: Verify NuGet Pack Dependencies

**Files:**
- No source edits expected.

- [ ] **Step 1: Pack `TecnoFisc.Sped.Txt`**

Run:

```powershell
dotnet pack src/TecnoFisc.Sped.Txt/TecnoFisc.Sped.Txt.csproj --no-build -o artifacts/packages-stage13
```

Expected: creates `artifacts/packages-stage13/TecnoFisc.Sped.Txt.0.6.0.nupkg` and `.snupkg`.

- [ ] **Step 2: Pack root `TecnoFisc.Sped`**

Run:

```powershell
dotnet pack src/TecnoFisc.Sped/TecnoFisc.Sped.csproj --no-build -o artifacts/packages-stage13
```

Expected: creates `artifacts/packages-stage13/TecnoFisc.Sped.0.6.0.nupkg` and `.snupkg`.

- [ ] **Step 3: Inspect package dependency groups**

Run:

```powershell
Add-Type -AssemblyName System.IO.Compression.FileSystem
$txtNuspec = [IO.Compression.ZipFile]::OpenRead('artifacts/packages-stage13/TecnoFisc.Sped.Txt.0.6.0.nupkg').Entries | Where-Object FullName -like '*.nuspec'
$rootNuspec = [IO.Compression.ZipFile]::OpenRead('artifacts/packages-stage13/TecnoFisc.Sped.0.6.0.nupkg').Entries | Where-Object FullName -like '*.nuspec'
$txtNuspec.FullName
$rootNuspec.FullName
```

Expected: prints `TecnoFisc.Sped.Txt.nuspec` and `TecnoFisc.Sped.nuspec`. Then inspect contents with any zip viewer or a temporary extraction and confirm:

```xml
<dependency id="TecnoFisc.Sped.EfdContribuicoes" version="0.6.0" exclude="Build,Analyzers" />
<dependency id="TecnoFisc.Sped.EfdIcmsIpi" version="0.6.0" exclude="Build,Analyzers" />
<dependency id="TecnoFisc.Sped.Ecd" version="0.6.0" exclude="Build,Analyzers" />
```

for `TecnoFisc.Sped.Txt`, and:

```xml
<dependency id="TecnoFisc.Sped.Txt" version="0.6.0" exclude="Build,Analyzers" />
```

for `TecnoFisc.Sped`.

- [ ] **Step 4: If dependencies are missing, fix ProjectReference metadata**

If `dotnet pack` does not emit dependency entries, update each umbrella `ProjectReference` to include `PrivateAssets="none"` and rerun Steps 1-3. The final `TecnoFisc.Sped.Txt.csproj` references would be:

```xml
    <ProjectReference Include="..\TecnoFisc.Sped.EfdContribuicoes\TecnoFisc.Sped.EfdContribuicoes.csproj" PrivateAssets="none" />
    <ProjectReference Include="..\TecnoFisc.Sped.EfdIcmsIpi\TecnoFisc.Sped.EfdIcmsIpi.csproj" PrivateAssets="none" />
    <ProjectReference Include="..\TecnoFisc.Sped.Ecd\TecnoFisc.Sped.Ecd.csproj" PrivateAssets="none" />
```

and the final `TecnoFisc.Sped.csproj` reference would be:

```xml
    <ProjectReference Include="..\TecnoFisc.Sped.Txt\TecnoFisc.Sped.Txt.csproj" PrivateAssets="none" />
```

Run:

```powershell
dotnet test tests/TecnoFisc.Sped.Core.Tests/TecnoFisc.Sped.Core.Tests.csproj --filter UmbrellaPackageTests
dotnet build TecnoFisc.Sped.slnx
dotnet pack src/TecnoFisc.Sped.Txt/TecnoFisc.Sped.Txt.csproj --no-build -o artifacts/packages-stage13
dotnet pack src/TecnoFisc.Sped/TecnoFisc.Sped.csproj --no-build -o artifacts/packages-stage13
```

Expected: all commands pass and package dependency entries are present.

- [ ] **Step 5: Commit pack metadata fix if needed**

Only run this commit if Step 4 changed `.csproj` files:

```powershell
git add src/TecnoFisc.Sped.Txt/TecnoFisc.Sped.Txt.csproj src/TecnoFisc.Sped/TecnoFisc.Sped.csproj
git commit -m "fix: preservar dependencias NuGet dos guarda-chuvas"
```

## Task 4: Update Documentation

**Files:**
- Modify: `README.md`
- Modify: `ARCHITECTURE.md`
- Modify: `sped/STAGE_18_REORG.md`

- [ ] **Step 1: Update README package table**

In `README.md`, replace the current metapackage row:

```markdown
| Metapacote agregador | `TecnoFisc.Sped` | planejado — referencia todos os leiautes acima em uma única dependência |
```

with:

```markdown
| Guarda-chuva TXT | `TecnoFisc.Sped.Txt` | disponível — agrega EFD Contribuições, EFD ICMS-IPI e ECD |
| Guarda-chuva geral | `TecnoFisc.Sped` | disponível — agrega `TecnoFisc.Sped.Txt`; passará a agregar XML após CT-e |
| Guarda-chuva XML | `TecnoFisc.Sped.Xml` | planejado após CT-e — agregará NFe/NFC-e e CT-e |
```

- [ ] **Step 2: Update README infrastructure paragraph**

Replace the paragraph beginning with:

```markdown
`TecnoFisc.Sped.Core` é a infraestrutura compartilhada
```

with:

```markdown
`TecnoFisc.Sped.Core` contém os primitivos fiscais universais. A maquinaria textual vive em
`TecnoFisc.Sped.Txt.Engine`; a maquinaria XML vive em `TecnoFisc.Sped.Xml.Engine`. Para
consumidores que querem todos os leiautes textuais em uma única dependência, use
`TecnoFisc.Sped.Txt`. O pacote `TecnoFisc.Sped` agrega os guarda-chuvas disponíveis; hoje ele
puxa o mundo TXT e será ampliado com `TecnoFisc.Sped.Xml` depois do CT-e.
```

- [ ] **Step 3: Update README repository tree**

Replace the `src/` block in `README.md` with:

```text
├── src/
│   ├── TecnoFisc.Sped/                       # Guarda-chuva geral (Txt agora; Xml após CT-e)
│   ├── TecnoFisc.Sped.Txt/                   # Guarda-chuva textual (EFD Contribuições, EFD ICMS-IPI, ECD)
│   ├── TecnoFisc.Sped.Core/                  # Value objects fiscais universais
│   ├── TecnoFisc.Sped.Txt.Engine/            # Motor .txt + catálogo + parser/gerador + sniffer TXT
│   ├── TecnoFisc.Sped.Txt.Engine.SourceGenerators/ # Source generator do catálogo TXT (analyzer)
│   ├── TecnoFisc.Sped.Xml.Engine/            # Motor XML + IDocumentoFiscalXml + sniffer XML
│   ├── TecnoFisc.Sped.EfdContribuicoes/      # Leiaute EFD Contribuições V006
│   ├── TecnoFisc.Sped.EfdIcmsIpi/            # Leiaute EFD ICMS-IPI baseline V015 + V016-V020 (read-only)
│   ├── TecnoFisc.Sped.Ecd/                   # Leiaute ECD 9 (Sped Contábil, read-only)
│   └── TecnoFisc.Sped.NFeNFCe/               # NF-e/NFC-e 4.00 (XML, read-only)
│   # Stages futuros (planejados): CTe, Ecf, TecnoFisc.Sped.Xml
```

- [ ] **Step 4: Update `ARCHITECTURE.md` Stage 13 sequencing**

In `ARCHITECTURE.md` under `### Stage 13 — Guarda-chuvas TecnoFisc.Sped (Txt / Xml / tudo)`, replace:

```markdown
- `TecnoFisc.Sped.Txt` → `EfdContribuicoes`, `EfdIcmsIpi`, `Ecd`, `Ecf`.
- `TecnoFisc.Sped.Xml` → `NFeNFCe`, `CTe`.
- `TecnoFisc.Sped` → `Txt` + `Xml` (tudo).
```

with:

```markdown
- `TecnoFisc.Sped.Txt` → `EfdContribuicoes`, `EfdIcmsIpi`, `Ecd` agora; `Ecf` entra na Stage 17.
- `TecnoFisc.Sped.Xml` → `NFeNFCe`, `CTe` depois da Stage 16 (adiado enquanto só existe `NFeNFCe`).
- `TecnoFisc.Sped` → `Txt` agora; `Txt` + `Xml` quando o guarda-chuva XML existir.
```

Then replace:

```markdown
Publica a primeira vez que todos os leiautes textuais estiverem em uso (EFD Contribuições + EFD ICMS-IPI + ECD; ECF pode ser placeholder até Stage 17).
```

with:

```markdown
Publica agora porque ja ha tres leiautes textuais em uso (EFD Contribuições + EFD ICMS-IPI + ECD). `Ecf` sera adicionado ao `TecnoFisc.Sped.Txt` na Stage 17; `TecnoFisc.Sped.Xml` e a perna XML do pacote geral entram depois do CT-e.
```

- [ ] **Step 5: Update Stage 18 tracking**

In `sped/STAGE_18_REORG.md`, replace:

```markdown
- [ ] Passo 4 — guarda-chuvas (Stage 13): `Sped.Txt` + `Sped` agora; `Sped.Xml` após CT-e.
```

with:

```markdown
- [x] Passo 4 — guarda-chuvas (Stage 13): `TecnoFisc.Sped.Txt` + `TecnoFisc.Sped` criados; `TecnoFisc.Sped.Xml` permanece adiado para depois do CT-e.
```

- [ ] **Step 6: Run docs consistency search**

Run:

```powershell
rg -n "TecnoFisc\.Sped\.Xml|Metapacote agregador|Core\.SourceGenerators|sniffer" README.md ARCHITECTURE.md sped/STAGE_18_REORG.md
```

Expected:

- `TecnoFisc.Sped.Xml` appears only as planned/after CT-e in README/Architecture/tracking.
- No README reference to `TecnoFisc.Sped.Core.SourceGenerators`.
- No README claim that `Core` contains parser/gerador genericos or sniffer.

- [ ] **Step 7: Commit documentation**

```powershell
git add README.md ARCHITECTURE.md sped/STAGE_18_REORG.md
git commit -m "docs: documentar guarda-chuvas do Stage 13"
```

## Task 5: Full Verification

**Files:**
- No source edits expected.

- [ ] **Step 1: Run targeted packaging tests**

```powershell
dotnet test tests/TecnoFisc.Sped.Core.Tests/TecnoFisc.Sped.Core.Tests.csproj --filter UmbrellaPackageTests
```

Expected: PASS.

- [ ] **Step 2: Run full test suite**

```powershell
dotnet test TecnoFisc.Sped.slnx
```

Expected: PASS, preserving the current suite count unless unrelated tests changed in the branch.

- [ ] **Step 3: Build release packages for both new umbrellas**

```powershell
dotnet pack src/TecnoFisc.Sped.Txt/TecnoFisc.Sped.Txt.csproj -c Release -o artifacts/packages-stage13
dotnet pack src/TecnoFisc.Sped/TecnoFisc.Sped.csproj -c Release -o artifacts/packages-stage13
```

Expected: both packages and symbol packages are produced with version `0.6.0`, README embedded from the repository root, SourceLink metadata included, and dependency groups matching Task 3.

- [ ] **Step 4: Check no accidental code was added to umbrella projects**

```powershell
Get-ChildItem src/TecnoFisc.Sped,src/TecnoFisc.Sped.Txt -Recurse -Filter *.cs
```

Expected: no output.

- [ ] **Step 5: Check git diff**

```powershell
git diff --stat
git diff -- src/TecnoFisc.Sped src/TecnoFisc.Sped.Txt TecnoFisc.Sped.slnx tests/TecnoFisc.Sped.Core.Tests/Packaging/UmbrellaPackageTests.cs README.md ARCHITECTURE.md sped/STAGE_18_REORG.md
```

Expected: only the planned files changed. No generated `bin/`, `obj/`, `artifacts/packages-stage13/`, `.nupkg` or `.snupkg` files staged.

- [ ] **Step 6: Commit any final fix**

Only if verification required a fix:

```powershell
git add src/TecnoFisc.Sped src/TecnoFisc.Sped.Txt TecnoFisc.Sped.slnx tests/TecnoFisc.Sped.Core.Tests/Packaging/UmbrellaPackageTests.cs README.md ARCHITECTURE.md sped/STAGE_18_REORG.md
git commit -m "chore: finalizar verificacao dos guarda-chuvas Stage 13"
```

## Self-Review

- Spec coverage: cobre Stage 13 (`Txt`, `Sped`, sem codigo, sem `Xml` antes do CT-e), Stage 18 passo 4, README, solution e pack NuGet.
- Red-flag scan: o plano nao usa marcadores de trabalho pendente nem passos genericos sem conteudo; os comandos e trechos de codigo/XML estao completos.
- Type/signature consistency: testes usam `UmbrellaPackageTests`, caminhos reais do repositorio e APIs BCL (`XDocument`, `Directory`, `Path`) sem depender dos novos projetos em compile-time.
