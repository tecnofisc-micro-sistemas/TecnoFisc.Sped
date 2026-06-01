using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 10.007 — exercita a forma do <see cref="Registro0180"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 86-87): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class Registro0180Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0000).Assembly);

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.ReadStreamingAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.WriteAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }

    [Fact]
    public void Atributo_Declara0180_Nivel3_Bloco0()
    {
        var atributo = typeof(Registro0180).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0180");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0180ComTresCamposNaOrdem()
    {
        _catalogo.TentarObter("0180".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0180");
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodRel", "DtIniRel", "DtFinRel"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0180".AsSpan(), out var meta);
        var registro = (Registro0180)meta!.Fabrica();

        // exemplo do manual (p. 87): COD_REL=03 (Coligada), DT_INI_REL=23/03/2019, DT_FIN_REL adicional
        meta.Campos[0].Definidor(registro, "03".AsSpan());       // CodRel
        meta.Campos[1].Definidor(registro, "23032019".AsSpan()); // DtIniRel
        meta.Campos[2].Definidor(registro, "31122025".AsSpan()); // DtFinRel

        registro.CodRel.Should().Be(CodigoRelacionamentoParticipante.Coligada);
        registro.DtIniRel.Should().Be(new DateOnly(2019, 3, 23));
        registro.DtFinRel.Should().Be(new DateOnly(2025, 12, 31));
    }

    [Fact]
    public void Definidor_DataFimVazia_DevolveNulo()
    {
        _catalogo.TentarObter("0180".AsSpan(), out var meta);
        var registro = (Registro0180)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // DtFinRel

        registro.DtFinRel.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // exemplo do manual (p. 87) com data de término adicionada
        const string sped = "|0180|03|23032019|31122025|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemDataFim_PreservaTextoCanonico()
    {
        // exemplo exato do manual (p. 87): relacionamento sem data de término
        const string sped = "|0180|03|23032019||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
