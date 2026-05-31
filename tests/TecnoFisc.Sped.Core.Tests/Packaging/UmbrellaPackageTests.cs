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
        SourceFiles("src", "TecnoFisc.Sped.Txt")
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
        SourceFiles("src", "TecnoFisc.Sped")
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

    private static string[] SourceFiles(params string[] pathSegments)
    {
        var projectDirectory = ProjectDirectory(pathSegments);

        return Directory.EnumerateFiles(projectDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(file => !IsBuildOutputFile(projectDirectory, file))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static bool IsBuildOutputFile(string projectDirectory, string file)
    {
        var relativePath = Path.GetRelativePath(projectDirectory, file);

        return relativePath
            .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(segment =>
                string.Equals(segment, "bin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(segment, "obj", StringComparison.OrdinalIgnoreCase));
    }

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
