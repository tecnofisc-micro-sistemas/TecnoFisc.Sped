using TecnoFisc.Sped.Ecf.Generated;

namespace TecnoFisc.Sped.Ecf.Tests.Catalogo;

public sealed class OrdemCatalogoEcfTests
{
    private static int CategoriaOrdemBloco(string bloco) => bloco switch
    {
        "0" => 0,
        "9" => 3,
        _ when bloco.Length == 1 && bloco[0] >= 'A' && bloco[0] <= 'Z' => 1,
        _ when bloco.Length == 1 && bloco[0] >= '1' && bloco[0] <= '8' => 2,
        _ => 4,
    };

    [Fact]
    public void EnumerarRegistros_SegueAOrdemCanonicaDeBloco()
    {
        var registros = new CatalogoSpedGerado().EnumerarRegistros().ToList();

        var esperado = registros
            .OrderBy(r => CategoriaOrdemBloco(r.Bloco))
            .ThenBy(r => r.Bloco, StringComparer.Ordinal)
            .ThenBy(r => r.Codigo, StringComparer.Ordinal)
            .Select(r => r.Codigo)
            .ToList();

        registros.Select(r => r.Codigo).Should().Equal(esperado);
    }

    [Fact]
    public void EnumerarRegistros_ComecaNoBloco0ETerminaNoBloco9()
    {
        var registros = new CatalogoSpedGerado().EnumerarRegistros().ToList();

        registros[0].Bloco.Should().Be("0");
        registros[^1].Bloco.Should().Be("9");
    }

    [Fact]
    public void EnumerarRegistros_BlocosNaSequenciaCanonica()
    {
        // Oráculo literal: sequência de blocos conforme ArquivoEcf._ordemBlocos,
        // sem depender de CategoriaOrdemBloco. Detecta regressão em CategoriaOrdemBloco
        // quando alterada simultaneamente no gerador e em todos os testes.
        var seqEsperada = new[] { "0", "C", "E", "J", "K", "L", "M", "N", "P", "Q", "T", "U", "V", "W", "X", "Y", "9" };

        var registros = new CatalogoSpedGerado().EnumerarRegistros().ToList();
        var seqObservada = registros.Select(r => r.Bloco).Distinct().ToList();

        seqObservada.Should().Equal(seqEsperada);
    }
}
