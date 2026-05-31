using TecnoFisc.Sped.Txt.Engine.Catalogo;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Catalogo;

/// <summary>
/// Confirma que o catálogo emitido pelo source generator (Stage 6) descreve exatamente os
/// mesmos registros que o catálogo reflexivo (<see cref="CatalogoBuilder.BuildFromAssembly"/>).
/// Qualquer divergência aqui indica que o gerador deixou de detectar uma classe decorada
/// com <c>[RegistroSped]</c> ou diverge nos metadados (Codigo/Nivel/Bloco/Tipo).
/// </summary>
public sealed class CatalogoSpedGeradoTests
{
    [Fact]
    public void CatalogoGerado_TemMesmosCodigosQueReflexivo()
    {
        var gerado = new CatalogoSpedGerado();
        var reflexivo = CatalogoBuilder.BuildFromAssembly(typeof(CatalogoSpedGerado).Assembly);

        var codigosGerados = gerado.EnumerarRegistros()
            .Select(static r => r.Codigo)
            .OrderBy(static c => c, StringComparer.Ordinal)
            .ToList();
        var codigosReflexivos = reflexivo.EnumerarRegistros()
            .Select(static r => r.Codigo)
            .OrderBy(static c => c, StringComparer.Ordinal)
            .ToList();

        codigosGerados.Should().Equal(codigosReflexivos);
    }

    [Fact]
    public void CatalogoGerado_PreservaCodigoNivelBlocoETipoCSharp()
    {
        var gerado = new CatalogoSpedGerado();
        var reflexivo = CatalogoBuilder.BuildFromAssembly(typeof(CatalogoSpedGerado).Assembly);

        foreach (var meta in gerado.EnumerarRegistros())
        {
            reflexivo.TentarObter(meta.Codigo, out var espelho).Should().BeTrue(
                $"código '{meta.Codigo}' precisa existir no catálogo reflexivo");
            espelho!.Nivel.Should().Be(meta.Nivel, $"Nivel divergente em '{meta.Codigo}'");
            espelho.Bloco.Should().Be(meta.Bloco, $"Bloco divergente em '{meta.Codigo}'");
            espelho.TipoCSharp.Should().Be(meta.TipoCSharp, $"TipoCSharp divergente em '{meta.Codigo}'");
            espelho.Campos.Should().HaveCount(meta.Campos.Count, $"Quantidade de campos divergente em '{meta.Codigo}'");
        }
    }

    [Fact]
    public void CatalogoGerado_FabricaProduzInstanciaDoTipoEsperado()
    {
        var gerado = new CatalogoSpedGerado();

        foreach (var meta in gerado.EnumerarRegistros())
        {
            var instancia = meta.Fabrica();
            instancia.Should().BeOfType(meta.TipoCSharp,
                $"fábrica de '{meta.Codigo}' deve retornar exatamente {meta.TipoCSharp.Name}");
            instancia.Codigo.Should().Be(meta.Codigo,
                $"código declarado pelo registro de '{meta.Codigo}' precisa bater com o catálogo");
        }
    }
}
