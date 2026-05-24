using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoK;

/// <summary>
/// Sub-stage 8.207 — exercita a forma do <see cref="RegistroK280"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 262-263): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroK280Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroK280).Assembly);

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
    public void Atributo_DeclaraK280_Nivel3_BlocoK()
    {
        var atributo = typeof(RegistroK280).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("K280");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("K");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroK280ComSeisCamposNaOrdem()
    {
        _catalogo.TentarObter("K280".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("K280");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "DtEst",
            "CodItem",
            "QtdCorPos",
            "QtdCorNeg",
            "IndEst",
            "CodPart",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("K280".AsSpan(), out var meta);
        var registro = (RegistroK280)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "31122024".AsSpan());
        meta.Campos[1].Definidor(registro, "PROD-CORRIGIDO".AsSpan());
        meta.Campos[2].Definidor(registro, "3,125".AsSpan());
        meta.Campos[3].Definidor(registro, "1,250".AsSpan());
        meta.Campos[4].Definidor(registro, "2".AsSpan());
        meta.Campos[5].Definidor(registro, "PART001".AsSpan());

        registro.DtEst.Should().Be(new DateOnly(2024, 12, 31));
        registro.CodItem.Should().Be("PROD-CORRIGIDO");
        registro.QtdCorPos.Should().Be(3.125m);
        registro.QtdCorNeg.Should().Be(1.250m);
        registro.IndEst.Should().Be(IndicadorPropriedadeItem.PropriedadeTerceirosPosseInformante);
        registro.CodPart.Should().Be("PART001");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("K280".AsSpan(), out var meta);
        var registro = (RegistroK280)meta!.Fabrica();

        meta!.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.QtdCorPos.Should().BeNull();
        registro.QtdCorNeg.Should().BeNull();
        registro.CodPart.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|K280|31122024|PROD-CORRIGIDO|3,125|1,250|2|PART001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComEstoqueProprioSemParticipante_PreservaTextoCanonico()
    {
        const string sped = "|K280|31122024|PROD-CORRIGIDO|3,125||0||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("0", IndicadorPropriedadeItem.PropriedadeInformanteEmSeuPoder)]
    [InlineData("1", IndicadorPropriedadeItem.PropriedadeInformantePosseTerceiros)]
    [InlineData("2", IndicadorPropriedadeItem.PropriedadeTerceirosPosseInformante)]
    public void Definidor_IndEst_MapeiaCodigos(string input, IndicadorPropriedadeItem esperado)
    {
        _catalogo.TentarObter("K280".AsSpan(), out var meta);
        var registro = (RegistroK280)meta!.Fabrica();

        meta.Campos[4].Definidor(registro, input.AsSpan());

        registro.IndEst.Should().Be(esperado);
    }
}
