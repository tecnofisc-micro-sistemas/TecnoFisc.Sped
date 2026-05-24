using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoG;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoG;

/// <summary>
/// Sub-stage 8.182 — exercita a forma do <see cref="RegistroG125"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 238-240): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroG125Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroG125).Assembly);

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
    public void Atributo_DeclaraG125_Nivel3_BlocoG()
    {
        var atributo = typeof(RegistroG125).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("G125");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("G");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroG125ComNoveCamposNaOrdem()
    {
        _catalogo.TentarObter("G125".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("G125");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodIndBem",
            "DtMov",
            "TipoMov",
            "VlImobIcmsOp",
            "VlImobIcmsSt",
            "VlImobIcmsFrt",
            "VlImobIcmsDif",
            "NumParc",
            "VlParcPass",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("G125".AsSpan(), out var meta);
        var registro = (RegistroG125)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "BEM-CIAP-001".AsSpan());
        meta.Campos[1].Definidor(registro, "15012025".AsSpan());
        meta.Campos[2].Definidor(registro, "IM".AsSpan());
        meta.Campos[3].Definidor(registro, "1200,50".AsSpan());
        meta.Campos[4].Definidor(registro, "100,25".AsSpan());
        meta.Campos[5].Definidor(registro, "35,75".AsSpan());
        meta.Campos[6].Definidor(registro, "10,00".AsSpan());
        meta.Campos[7].Definidor(registro, "001".AsSpan());
        meta.Campos[8].Definidor(registro, "28,05".AsSpan());

        registro.CodIndBem.Should().Be("BEM-CIAP-001");
        registro.DtMov.Should().Be(new DateOnly(2025, 1, 15));
        registro.TipoMov.Should().Be(TipoMovimentacaoBemCiAp.ImobilizacaoBemIndividual);
        registro.VlImobIcmsOp.Should().Be(1200.50m);
        registro.VlImobIcmsSt.Should().Be(100.25m);
        registro.VlImobIcmsFrt.Should().Be(35.75m);
        registro.VlImobIcmsDif.Should().Be(10.00m);
        registro.NumParc.Should().Be(1);
        registro.VlParcPass.Should().Be(28.05m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("G125".AsSpan(), out var meta);
        var registro = (RegistroG125)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.VlImobIcmsOp.Should().BeNull();
        registro.VlImobIcmsSt.Should().BeNull();
        registro.VlImobIcmsFrt.Should().BeNull();
        registro.VlImobIcmsDif.Should().BeNull();
        registro.NumParc.Should().BeNull();
        registro.VlParcPass.Should().BeNull();
    }

    [Theory]
    [InlineData("SI", TipoMovimentacaoBemCiAp.SaldoInicial)]
    [InlineData("IM", TipoMovimentacaoBemCiAp.ImobilizacaoBemIndividual)]
    [InlineData("IA", TipoMovimentacaoBemCiAp.ImobilizacaoEmAndamentoComponente)]
    [InlineData("CI", TipoMovimentacaoBemCiAp.ConclusaoImobilizacaoEmAndamento)]
    [InlineData("MC", TipoMovimentacaoBemCiAp.ImobilizacaoAtivoCirculante)]
    [InlineData("BA", TipoMovimentacaoBemCiAp.BaixaFimApropriacao)]
    [InlineData("AT", TipoMovimentacaoBemCiAp.AlienacaoOuTransferencia)]
    [InlineData("PE", TipoMovimentacaoBemCiAp.PerecimentoExtravioDeterioracao)]
    [InlineData("OT", TipoMovimentacaoBemCiAp.OutrasSaidasImobilizado)]
    public void Definidor_TipoMov_MapeiaCodigos(string input, TipoMovimentacaoBemCiAp esperado)
    {
        _catalogo.TentarObter("G125".AsSpan(), out var meta);
        var registro = (RegistroG125)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, input.AsSpan());

        registro.TipoMov.Should().Be(esperado);
    }

    [Theory]
    [InlineData(TipoMovimentacaoBemCiAp.SaldoInicial, "SI")]
    [InlineData(TipoMovimentacaoBemCiAp.ImobilizacaoBemIndividual, "IM")]
    [InlineData(TipoMovimentacaoBemCiAp.ImobilizacaoEmAndamentoComponente, "IA")]
    [InlineData(TipoMovimentacaoBemCiAp.ConclusaoImobilizacaoEmAndamento, "CI")]
    [InlineData(TipoMovimentacaoBemCiAp.ImobilizacaoAtivoCirculante, "MC")]
    [InlineData(TipoMovimentacaoBemCiAp.BaixaFimApropriacao, "BA")]
    [InlineData(TipoMovimentacaoBemCiAp.AlienacaoOuTransferencia, "AT")]
    [InlineData(TipoMovimentacaoBemCiAp.PerecimentoExtravioDeterioracao, "PE")]
    [InlineData(TipoMovimentacaoBemCiAp.OutrasSaidasImobilizado, "OT")]
    public void Serializar_TipoMov_RetornaCodigo(TipoMovimentacaoBemCiAp tipoMov, string esperado)
    {
        _catalogo.TentarObter("G125".AsSpan(), out var meta);
        var registro = (RegistroG125)meta!.Fabrica();
        registro.TipoMov = tipoMov;

        meta.Campos[2].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|G125|BEM-CIAP-001|15012025|IM|1200,50|100,25|35,75|10,00|1|28,05|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComBaixaSemCamposCondicionais_PreservaTextoCanonico()
    {
        const string sped = "|G125|BEM-CIAP-001|31012025|BA|||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
