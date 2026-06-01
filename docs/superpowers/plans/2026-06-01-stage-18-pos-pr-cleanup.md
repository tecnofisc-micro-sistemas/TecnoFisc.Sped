# Stage 18 Pos-PR Cleanup Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Finalizar as pendencias pos-PRs do Stage 18, separando os testes dos engines de `Core.Tests` e documentando as breaking changes de empacotamento no `CHANGELOG.md`.

**Architecture:** O `Core.Tests` deve voltar a testar somente `TecnoFisc.Sped.Core` e invariantes de empacotamento do repositorio. Os testes da maquinaria TXT ficam em `TecnoFisc.Sped.Txt.Engine.Tests`; os testes do sniffer/contratos XML ficam em `TecnoFisc.Sped.Xml.Engine.Tests`; ambos referenciam `Core` apenas quando seus fixtures sinteticos precisam de value objects/atributos universais.

**Tech Stack:** .NET 10 SDK, C#, xUnit v3, FluentAssertions, MSBuild SDK-style projects, `.slnx`.

---

## Contexto Obrigatorio

Fonte de verdade: `sped/STAGE_18_REORG.md`, secao "Pos-PRs (pendencias separadas)".

Pendencias cobertas por este plano:

- `CHANGELOG.md`: adicionar entrada de proximo release por pacote para os novos pacotes `TecnoFisc.Sped.Txt.Engine`, `TecnoFisc.Sped.Xml.Engine`, `TecnoFisc.Sped.Txt.Engine.SourceGenerators`, os guarda-chuvas `TecnoFisc.Sped.Txt`/`TecnoFisc.Sped` e as quebras de namespace do Stage 18.
- Extrair `Txt.Engine.Tests` e `Xml.Engine.Tests` de `tests/TecnoFisc.Sped.Core.Tests`, removendo as referencias diretas de `Core.Tests` para `Txt.Engine` e `Xml.Engine`.
- Atualizar `sped/STAGE_18_REORG.md` marcando o cleanup como concluido apenas depois de build e suite completa verdes.

Baseline esperado apos cada checkpoint:

```powershell
dotnet test TecnoFisc.Sped.slnx
```

Expected: todos os testes passam. A contagem total deve permanecer equivalente a baseline do Stage 18, salvo diferenca explicavel de descoberta por mudanca de projetos; nenhum teste deve ser removido.

## File Structure

Arquivos a criar:

- `tests/TecnoFisc.Sped.Txt.Engine.Tests/TecnoFisc.Sped.Txt.Engine.Tests.csproj` - projeto xUnit dos testes da maquinaria TXT.
- `tests/TecnoFisc.Sped.Txt.Engine.Tests/GlobalUsings.cs` - usings globais necessarios aos testes TXT.
- `tests/TecnoFisc.Sped.Xml.Engine.Tests/TecnoFisc.Sped.Xml.Engine.Tests.csproj` - projeto xUnit dos testes da maquinaria XML.
- `tests/TecnoFisc.Sped.Xml.Engine.Tests/GlobalUsings.cs` - usings globais necessarios aos testes XML.

Arquivos a mover de `Core.Tests` para `Txt.Engine.Tests`:

- `tests/TecnoFisc.Sped.Core.Tests/_Sintetico/RegistrosSinteticos.cs`
- `tests/TecnoFisc.Sped.Core.Tests/Atributos/AtributosTests.cs`
- `tests/TecnoFisc.Sped.Core.Tests/Catalogo/CatalogoBuilderTests.cs`
- `tests/TecnoFisc.Sped.Core.Tests/Catalogo/CatalogoBuilderCampoArquivoTests.cs`
- `tests/TecnoFisc.Sped.Core.Tests/Gerador/EscritorSpedTxtTests.cs`
- `tests/TecnoFisc.Sped.Core.Tests/Gerador/RoundTripTests.cs`
- `tests/TecnoFisc.Sped.Core.Tests/Gerador/SerializadoresPrimitivosTests.cs`
- `tests/TecnoFisc.Sped.Core.Tests/Gerador/TotalizadorBlocosTests.cs`
- `tests/TecnoFisc.Sped.Core.Tests/Parser/EncodingSpedTests.cs`
- `tests/TecnoFisc.Sped.Core.Tests/Parser/LeitorSpedTxtTests.cs`
- `tests/TecnoFisc.Sped.Core.Tests/Parser/ParseadoresPrimitivosTests.cs`
- `tests/TecnoFisc.Sped.Core.Tests/Parser/PilhaHierarquicaTests.cs`
- `tests/TecnoFisc.Sped.Core.Tests/Parser/SnifferSpedTests.cs`
- `tests/TecnoFisc.Sped.Core.Tests/Streaming/StreamingExtensionsTests.cs`
- `tests/TecnoFisc.Sped.Core.Tests/Streaming/WithContextTests.cs`

Arquivos a mover de `Core.Tests` para `Xml.Engine.Tests`:

- `tests/TecnoFisc.Sped.Core.Tests/Xml/IDocumentoFiscalXmlTests.cs`
- `tests/TecnoFisc.Sped.Core.Tests/Xml/IdentificadorXmlFiscalTests.cs`

Arquivos a modificar:

- `tests/TecnoFisc.Sped.Core.Tests/TecnoFisc.Sped.Core.Tests.csproj` - remover `ProjectReference` para `Txt.Engine` e `Xml.Engine`.
- `TecnoFisc.Sped.slnx` - incluir os dois novos projetos de teste na pasta `/tests/`.
- `CHANGELOG.md` - adicionar secoes de `Nao publicado` por pacote.
- `sped/STAGE_18_REORG.md` - marcar as duas pendencias como concluidas.

Arquivos que devem continuar em `Core.Tests`:

- `tests/TecnoFisc.Sped.Core.Tests/Erros/ResultadoParseTests.cs`
- `tests/TecnoFisc.Sped.Core.Tests/Packaging/UmbrellaPackageTests.cs`
- `tests/TecnoFisc.Sped.Core.Tests/ValueObjects/*.cs`

---

### Task 1: Criar projetos de teste dos engines

**Files:**

- Create: `tests/TecnoFisc.Sped.Txt.Engine.Tests/TecnoFisc.Sped.Txt.Engine.Tests.csproj`
- Create: `tests/TecnoFisc.Sped.Txt.Engine.Tests/GlobalUsings.cs`
- Create: `tests/TecnoFisc.Sped.Xml.Engine.Tests/TecnoFisc.Sped.Xml.Engine.Tests.csproj`
- Create: `tests/TecnoFisc.Sped.Xml.Engine.Tests/GlobalUsings.cs`
- Modify: `TecnoFisc.Sped.slnx`

- [ ] **Step 1: Criar os diretorios dos novos projetos**

Run:

```powershell
New-Item -ItemType Directory -Force tests\TecnoFisc.Sped.Txt.Engine.Tests
New-Item -ItemType Directory -Force tests\TecnoFisc.Sped.Xml.Engine.Tests
```

Expected: os dois diretorios existem e nenhum arquivo de teste foi movido ainda.

- [ ] **Step 2: Criar `Txt.Engine.Tests.csproj`**

Create `tests/TecnoFisc.Sped.Txt.Engine.Tests/TecnoFisc.Sped.Txt.Engine.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit.v3" />
    <PackageReference Include="xunit.runner.visualstudio">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="coverlet.collector">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\TecnoFisc.Sped.Core\TecnoFisc.Sped.Core.csproj" />
    <ProjectReference Include="..\..\src\TecnoFisc.Sped.Txt.Engine\TecnoFisc.Sped.Txt.Engine.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 3: Criar `Txt.Engine.Tests/GlobalUsings.cs`**

Create `tests/TecnoFisc.Sped.Txt.Engine.Tests/GlobalUsings.cs`:

```csharp
global using FluentAssertions;
global using Xunit;
```

- [ ] **Step 4: Criar `Xml.Engine.Tests.csproj`**

Create `tests/TecnoFisc.Sped.Xml.Engine.Tests/TecnoFisc.Sped.Xml.Engine.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit.v3" />
    <PackageReference Include="xunit.runner.visualstudio">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="coverlet.collector">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\TecnoFisc.Sped.Core\TecnoFisc.Sped.Core.csproj" />
    <ProjectReference Include="..\..\src\TecnoFisc.Sped.Xml.Engine\TecnoFisc.Sped.Xml.Engine.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 5: Criar `Xml.Engine.Tests/GlobalUsings.cs`**

Create `tests/TecnoFisc.Sped.Xml.Engine.Tests/GlobalUsings.cs`:

```csharp
global using FluentAssertions;
global using Xunit;
```

- [ ] **Step 6: Adicionar os projetos na solution**

Modify `TecnoFisc.Sped.slnx` inside `<Folder Name="/tests/">` to include the new projects immediately after `Core.Tests`:

```xml
  <Folder Name="/tests/">
    <Project Path="tests/TecnoFisc.Sped.Core.Tests/TecnoFisc.Sped.Core.Tests.csproj" />
    <Project Path="tests/TecnoFisc.Sped.Txt.Engine.Tests/TecnoFisc.Sped.Txt.Engine.Tests.csproj" />
    <Project Path="tests/TecnoFisc.Sped.Xml.Engine.Tests/TecnoFisc.Sped.Xml.Engine.Tests.csproj" />
    <Project Path="tests/TecnoFisc.Sped.EfdContribuicoes.Tests/TecnoFisc.Sped.EfdContribuicoes.Tests.csproj" />
    <Project Path="tests/TecnoFisc.Sped.EfdIcmsIpi.Tests/TecnoFisc.Sped.EfdIcmsIpi.Tests.csproj" />
    <Project Path="tests/TecnoFisc.Sped.Ecd.Tests/TecnoFisc.Sped.Ecd.Tests.csproj" />
    <Project Path="tests/TecnoFisc.Sped.NFeNFCe.Tests/TecnoFisc.Sped.NFeNFCe.Tests.csproj" />
  </Folder>
```

- [ ] **Step 7: Verificar que os projetos vazios compilam**

Run:

```powershell
dotnet test tests\TecnoFisc.Sped.Txt.Engine.Tests\TecnoFisc.Sped.Txt.Engine.Tests.csproj
dotnet test tests\TecnoFisc.Sped.Xml.Engine.Tests\TecnoFisc.Sped.Xml.Engine.Tests.csproj
```

Expected: ambos executam com 0 testes descobertos ou sucesso equivalente, sem warnings e sem erros.

- [ ] **Step 8: Commit**

Run:

```powershell
git add TecnoFisc.Sped.slnx tests\TecnoFisc.Sped.Txt.Engine.Tests tests\TecnoFisc.Sped.Xml.Engine.Tests
git commit -m "test: add engine test projects"
```

Expected: commit criado somente com os dois projetos novos e a inclusao na solution.

---

### Task 2: Migrar testes TXT para `Txt.Engine.Tests`

**Files:**

- Move: `tests/TecnoFisc.Sped.Core.Tests/_Sintetico/RegistrosSinteticos.cs`
- Move: `tests/TecnoFisc.Sped.Core.Tests/Atributos/AtributosTests.cs`
- Move: `tests/TecnoFisc.Sped.Core.Tests/Catalogo/*.cs`
- Move: `tests/TecnoFisc.Sped.Core.Tests/Gerador/*.cs`
- Move: `tests/TecnoFisc.Sped.Core.Tests/Parser/*.cs`
- Move: `tests/TecnoFisc.Sped.Core.Tests/Streaming/*.cs`
- Modify moved files: namespaces from `TecnoFisc.Sped.Core.Tests.*` to `TecnoFisc.Sped.Txt.Engine.Tests.*`

- [ ] **Step 1: Mover as pastas e fixture sintetico com historico Git**

Run:

```powershell
New-Item -ItemType Directory -Force tests\TecnoFisc.Sped.Txt.Engine.Tests\_Sintetico
git mv tests\TecnoFisc.Sped.Core.Tests\_Sintetico\RegistrosSinteticos.cs tests\TecnoFisc.Sped.Txt.Engine.Tests\_Sintetico\RegistrosSinteticos.cs
git mv tests\TecnoFisc.Sped.Core.Tests\Atributos tests\TecnoFisc.Sped.Txt.Engine.Tests\Atributos
git mv tests\TecnoFisc.Sped.Core.Tests\Catalogo tests\TecnoFisc.Sped.Txt.Engine.Tests\Catalogo
git mv tests\TecnoFisc.Sped.Core.Tests\Gerador tests\TecnoFisc.Sped.Txt.Engine.Tests\Gerador
git mv tests\TecnoFisc.Sped.Core.Tests\Parser tests\TecnoFisc.Sped.Txt.Engine.Tests\Parser
git mv tests\TecnoFisc.Sped.Core.Tests\Streaming tests\TecnoFisc.Sped.Txt.Engine.Tests\Streaming
```

Expected: as pastas existem sob `Txt.Engine.Tests`; `Core.Tests` nao tem mais `_Sintetico`, `Atributos`, `Catalogo`, `Gerador`, `Parser` ou `Streaming`.

- [ ] **Step 2: Atualizar namespaces dos testes movidos**

Run:

```powershell
Get-ChildItem tests\TecnoFisc.Sped.Txt.Engine.Tests -Recurse -Filter *.cs |
  ForEach-Object {
    $path = $_.FullName
    $text = Get-Content -Raw $path
    $text = $text.Replace('TecnoFisc.Sped.Core.Tests', 'TecnoFisc.Sped.Txt.Engine.Tests')
    Set-Content -NoNewline -Path $path -Value $text
  }
```

Expected: nenhum arquivo em `Txt.Engine.Tests` ainda declara namespace `TecnoFisc.Sped.Core.Tests`.

- [ ] **Step 3: Verificar namespace residual**

Run:

```powershell
rg -n "TecnoFisc\.Sped\.Core\.Tests" tests\TecnoFisc.Sped.Txt.Engine.Tests
```

Expected: nenhum resultado.

- [ ] **Step 4: Rodar os testes TXT movidos**

Run:

```powershell
dotnet test tests\TecnoFisc.Sped.Txt.Engine.Tests\TecnoFisc.Sped.Txt.Engine.Tests.csproj
```

Expected: os testes de `Atributos`, `Catalogo`, `Gerador`, `Parser` e `Streaming` passam. Se houver IDE0005, remover apenas o `using` redundante apontado pelo compilador.

- [ ] **Step 5: Commit**

Run:

```powershell
git add tests\TecnoFisc.Sped.Core.Tests tests\TecnoFisc.Sped.Txt.Engine.Tests
git commit -m "test: move txt engine tests out of core"
```

Expected: commit criado com movimentos e ajustes de namespace dos testes TXT.

---

### Task 3: Migrar testes XML para `Xml.Engine.Tests`

**Files:**

- Move: `tests/TecnoFisc.Sped.Core.Tests/Xml/IDocumentoFiscalXmlTests.cs`
- Move: `tests/TecnoFisc.Sped.Core.Tests/Xml/IdentificadorXmlFiscalTests.cs`
- Modify moved files: namespace from `TecnoFisc.Sped.Core.Tests.Xml` to `TecnoFisc.Sped.Xml.Engine.Tests.Xml`

- [ ] **Step 1: Mover a pasta XML com historico Git**

Run:

```powershell
git mv tests\TecnoFisc.Sped.Core.Tests\Xml tests\TecnoFisc.Sped.Xml.Engine.Tests\Xml
```

Expected: `tests\TecnoFisc.Sped.Xml.Engine.Tests\Xml` contem os dois testes XML; `Core.Tests` nao tem mais pasta `Xml`.

- [ ] **Step 2: Atualizar namespaces dos testes XML movidos**

Run:

```powershell
Get-ChildItem tests\TecnoFisc.Sped.Xml.Engine.Tests -Recurse -Filter *.cs |
  ForEach-Object {
    $path = $_.FullName
    $text = Get-Content -Raw $path
    $text = $text.Replace('TecnoFisc.Sped.Core.Tests.Xml', 'TecnoFisc.Sped.Xml.Engine.Tests.Xml')
    Set-Content -NoNewline -Path $path -Value $text
  }
```

Expected: nenhum arquivo em `Xml.Engine.Tests` ainda declara namespace `TecnoFisc.Sped.Core.Tests`.

- [ ] **Step 3: Verificar namespace residual**

Run:

```powershell
rg -n "TecnoFisc\.Sped\.Core\.Tests" tests\TecnoFisc.Sped.Xml.Engine.Tests
```

Expected: nenhum resultado.

- [ ] **Step 4: Rodar os testes XML movidos**

Run:

```powershell
dotnet test tests\TecnoFisc.Sped.Xml.Engine.Tests\TecnoFisc.Sped.Xml.Engine.Tests.csproj
```

Expected: os testes de `IDocumentoFiscalXml` e `IdentificadorXmlFiscal` passam. Se houver IDE0005, remover apenas o `using` redundante apontado pelo compilador.

- [ ] **Step 5: Commit**

Run:

```powershell
git add tests\TecnoFisc.Sped.Core.Tests tests\TecnoFisc.Sped.Xml.Engine.Tests
git commit -m "test: move xml engine tests out of core"
```

Expected: commit criado com movimentos e ajustes de namespace dos testes XML.

---

### Task 4: Remover dependencias de engine do `Core.Tests`

**Files:**

- Modify: `tests/TecnoFisc.Sped.Core.Tests/TecnoFisc.Sped.Core.Tests.csproj`
- Verify: `tests/TecnoFisc.Sped.Core.Tests/Erros/ResultadoParseTests.cs`
- Verify: `tests/TecnoFisc.Sped.Core.Tests/Packaging/UmbrellaPackageTests.cs`
- Verify: `tests/TecnoFisc.Sped.Core.Tests/ValueObjects/*.cs`

- [ ] **Step 1: Remover `ProjectReference` de engines**

Modify `tests/TecnoFisc.Sped.Core.Tests/TecnoFisc.Sped.Core.Tests.csproj` so the final project references are exactly:

```xml
  <ItemGroup>
    <ProjectReference Include="..\..\src\TecnoFisc.Sped.Core\TecnoFisc.Sped.Core.csproj" />
  </ItemGroup>
```

The full file should be:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit.v3" />
    <PackageReference Include="xunit.runner.visualstudio">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="coverlet.collector">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\TecnoFisc.Sped.Core\TecnoFisc.Sped.Core.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Confirmar que `Core.Tests` nao referencia engines**

Run:

```powershell
rg -n "TecnoFisc\.Sped\.(Txt|Xml)\.Engine|Txt\.Engine|Xml\.Engine" tests\TecnoFisc.Sped.Core.Tests
```

Expected: nenhum resultado.

- [ ] **Step 3: Rodar somente `Core.Tests`**

Run:

```powershell
dotnet test tests\TecnoFisc.Sped.Core.Tests\TecnoFisc.Sped.Core.Tests.csproj
```

Expected: passam apenas os testes de `Erros`, `Packaging` e `ValueObjects`; sem referencias a `Txt.Engine`/`Xml.Engine`.

- [ ] **Step 4: Verificar grafo de referencias dos projetos de teste**

Run:

```powershell
dotnet list tests\TecnoFisc.Sped.Core.Tests\TecnoFisc.Sped.Core.Tests.csproj reference
dotnet list tests\TecnoFisc.Sped.Txt.Engine.Tests\TecnoFisc.Sped.Txt.Engine.Tests.csproj reference
dotnet list tests\TecnoFisc.Sped.Xml.Engine.Tests\TecnoFisc.Sped.Xml.Engine.Tests.csproj reference
```

Expected:

```text
Core.Tests references:
..\..\src\TecnoFisc.Sped.Core\TecnoFisc.Sped.Core.csproj

Txt.Engine.Tests references:
..\..\src\TecnoFisc.Sped.Core\TecnoFisc.Sped.Core.csproj
..\..\src\TecnoFisc.Sped.Txt.Engine\TecnoFisc.Sped.Txt.Engine.csproj

Xml.Engine.Tests references:
..\..\src\TecnoFisc.Sped.Core\TecnoFisc.Sped.Core.csproj
..\..\src\TecnoFisc.Sped.Xml.Engine\TecnoFisc.Sped.Xml.Engine.csproj
```

- [ ] **Step 5: Commit**

Run:

```powershell
git add tests\TecnoFisc.Sped.Core.Tests\TecnoFisc.Sped.Core.Tests.csproj
git commit -m "test: keep core tests independent from engines"
```

Expected: commit criado com a remocao das referencias indevidas.

---

### Task 5: Atualizar changelog do proximo release

**Files:**

- Modify: `CHANGELOG.md`

- [ ] **Step 1: Inserir secoes em `[Nao publicado]`**

Modify `CHANGELOG.md` so the top of the unreleased section becomes:

```markdown
## [Não publicado]

### TecnoFisc.Sped.Core

#### Alterado (breaking)

- Maquinaria especifica de TXT saiu do `Core` para `TecnoFisc.Sped.Txt.Engine`: abstracoes de registros/blocos/arquivos TXT, atributos `[RegistroSped]`/`[CampoSped]`/`[BlocoSped]`/`[SpedValor]`, catalogo, parser, gerador, streaming e enums transversais TXT. Consumidores que referenciavam esses tipos pelo namespace `TecnoFisc.Sped.Core.*` devem trocar para `TecnoFisc.Sped.Txt.Engine.*`.
- Maquinaria especifica de XML saiu do `Core` para `TecnoFisc.Sped.Xml.Engine`: `IdentificadorXmlFiscal`, `IDocumentoFiscalXml` e `TipoDocumentoFiscalXml`. Consumidores devem trocar `TecnoFisc.Sped.Core.Xml` por `TecnoFisc.Sped.Xml.Engine`.
- Enums de leiaute unico sairam do `Core` para seus pacotes donos: enums EFD ICMS-IPI para `TecnoFisc.Sped.EfdIcmsIpi.Enums` e enums NF-e/NFC-e para `TecnoFisc.Sped.NFeNFCe.Enums`.

#### Mantido

- Tipos universais continuam no `Core`: value objects fiscais, `ResultadoParse`/erros, `DescontinuadoAttribute` e enums fiscais transversais como `CodigoSituacaoDocumentoFiscal`, `IndicadorOperacao`, `OrigemMercadoria` e `ModalidadeFrete`.

### TecnoFisc.Sped.Txt.Engine

#### Adicionado

- Novo pacote de maquinaria TXT compartilhada pelos leiautes textuais. Contem contratos base (`RegistroSped`, `IArquivoSped`, `IBlocoSped`, `ILeitorSped`, `IEscritorSped`, `IRegistroSpedCatalogo`), atributos de metadados TXT, catalogo, parser/gerador `.txt`, helpers de streaming, `SnifferSped` e enums transversais TXT.

### TecnoFisc.Sped.Txt.Engine.SourceGenerators

#### Alterado (breaking)

- Pacote de source generators renomeado de `TecnoFisc.Sped.Core.SourceGenerators` para `TecnoFisc.Sped.Txt.Engine.SourceGenerators`. Continua sendo referenciado como analyzer (`OutputItemType="Analyzer"` e `ReferenceOutputAssembly="false"`) pelos pacotes de leiaute TXT.

### TecnoFisc.Sped.Xml.Engine

#### Adicionado

- Novo pacote de maquinaria XML compartilhada pelos leiautes XML. Contem `IdentificadorXmlFiscal`, `IDocumentoFiscalXml` e `TipoDocumentoFiscalXml`, dependendo apenas de `TecnoFisc.Sped.Core`.

### TecnoFisc.Sped.Txt

#### Adicionado

- Novo pacote guarda-chuva TXT, sem codigo proprio, agregando os leiautes textuais existentes (`TecnoFisc.Sped.EfdContribuicoes`, `TecnoFisc.Sped.EfdIcmsIpi` e `TecnoFisc.Sped.Ecd`).

### TecnoFisc.Sped

#### Adicionado

- Novo pacote guarda-chuva raiz, sem codigo proprio, agregando `TecnoFisc.Sped.Txt`. O guarda-chuva XML permanece adiado ate a chegada do CT-e.
```

- [ ] **Step 2: Verificar que nao ha mencao antiga incorreta no changelog**

Run:

```powershell
rg -n "Core\.SourceGenerators|Core\.Xml|Core\.Parser|Core\.Gerador|Core\.Catalogo|Core\.Streaming|Core\.Atributos" CHANGELOG.md
```

Expected: resultados antigos podem existir em releases anteriores e nao devem ser reescritos. Na secao `[Nao publicado]`, as entradas devem explicar a migracao para `Txt.Engine`/`Xml.Engine`.

- [ ] **Step 3: Commit**

Run:

```powershell
git add CHANGELOG.md
git commit -m "docs: document stage 18 package cleanup"
```

Expected: commit criado apenas com o changelog.

---

### Task 6: Atualizar tracking do Stage 18

**Files:**

- Modify: `sped/STAGE_18_REORG.md`

- [ ] **Step 1: Marcar pendencias como concluidas**

Modify the final checklist in `sped/STAGE_18_REORG.md` to:

```markdown
### Pós-PRs (pendências separadas)
- [x] CHANGELOG por pacote (próximo release — novos pacotes `Txt.Engine`/`Xml.Engine`/`Txt.Engine.SourceGenerators`; breaking de namespace).
- [x] Passo 4 — guarda-chuvas (Stage 13): `TecnoFisc.Sped.Txt` + `TecnoFisc.Sped` criados; `TecnoFisc.Sped.Xml` permanece adiado para depois do CT-e.
- [x] Stage 12 — sniffer TXT (`SnifferSped`) implementado em `TecnoFisc.Sped.Txt.Engine`; XML ja existia em `TecnoFisc.Sped.Xml.Engine`.
- [x] Cleanup opcional: `Txt.Engine.Tests` e `Xml.Engine.Tests` extraidos; `Core.Tests` voltou a referenciar somente `Core`.
```

- [ ] **Step 2: Rodar a suite completa**

Run:

```powershell
dotnet test TecnoFisc.Sped.slnx
```

Expected: build sem warnings/erros e todos os testes passam.

- [ ] **Step 3: Verificar que o isolamento arquitetural ficou claro**

Run:

```powershell
rg -n "ProjectReference Include=.*TecnoFisc\.Sped\.(Txt|Xml)\.Engine" tests\TecnoFisc.Sped.Core.Tests src\TecnoFisc.Sped.Core
rg -n "TecnoFisc\.Sped\.(Txt|Xml)\.Engine" src\TecnoFisc.Sped.Core tests\TecnoFisc.Sped.Core.Tests
```

Expected: nenhum resultado. Se o segundo comando retornar mencoes dentro de comentarios de docs embutidas em testes de packaging, avaliar o arquivo; referencias em docs externos nao bloqueiam.

- [ ] **Step 4: Conferir mudancas finais**

Run:

```powershell
git status --short
git diff --stat
```

Expected: somente arquivos deste plano aparecem modificados.

- [ ] **Step 5: Commit**

Run:

```powershell
git add sped\STAGE_18_REORG.md
git commit -m "docs: close stage 18 cleanup tracking"
```

Expected: commit criado com o tracking atualizado.

---

## Final Verification

- [ ] **Step 1: Rodar suite completa uma ultima vez**

Run:

```powershell
dotnet test TecnoFisc.Sped.slnx
```

Expected: todos os testes passam, sem warnings e sem erros.

- [ ] **Step 2: Validar isolamento dos projetos**

Run:

```powershell
dotnet list tests\TecnoFisc.Sped.Core.Tests\TecnoFisc.Sped.Core.Tests.csproj reference
dotnet list tests\TecnoFisc.Sped.Txt.Engine.Tests\TecnoFisc.Sped.Txt.Engine.Tests.csproj reference
dotnet list tests\TecnoFisc.Sped.Xml.Engine.Tests\TecnoFisc.Sped.Xml.Engine.Tests.csproj reference
```

Expected: `Core.Tests` referencia apenas `Core`; `Txt.Engine.Tests` referencia `Core + Txt.Engine`; `Xml.Engine.Tests` referencia `Core + Xml.Engine`.

- [ ] **Step 3: Confirmar que nao sobrou teste de engine no `Core.Tests`**

Run:

```powershell
Test-Path tests\TecnoFisc.Sped.Core.Tests\Atributos
Test-Path tests\TecnoFisc.Sped.Core.Tests\Catalogo
Test-Path tests\TecnoFisc.Sped.Core.Tests\Gerador
Test-Path tests\TecnoFisc.Sped.Core.Tests\Parser
Test-Path tests\TecnoFisc.Sped.Core.Tests\Streaming
Test-Path tests\TecnoFisc.Sped.Core.Tests\Xml
```

Expected: seis linhas `False`.

- [ ] **Step 4: Revisar diff final**

Run:

```powershell
git diff --check
git status --short
```

Expected: `git diff --check` sem whitespace errors; `git status --short` limpo se todos os commits foram feitos.

## Notes

- Use `git mv` para preservar historico dos testes.
- Nao mover `Packaging/UmbrellaPackageTests.cs`; apesar de estar em `Core.Tests`, ele testa invariantes de empacotamento do repositorio e nao referencia os engines.
- Nao adicionar projeto `TecnoFisc.Sped.Txt.Engine.SourceGenerators.Tests` neste cleanup; ele aparece em `ARCHITECTURE.md` como estrutura desejada, mas nao e uma pendencia listada em `sped/STAGE_18_REORG.md`.
- Nao criar `TecnoFisc.Sped.Xml`; o tracking deixa explicito que o guarda-chuva XML fica adiado ate depois do CT-e.
