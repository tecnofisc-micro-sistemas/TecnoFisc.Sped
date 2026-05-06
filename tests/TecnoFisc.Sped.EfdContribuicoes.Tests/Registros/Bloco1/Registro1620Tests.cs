using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco1;

public sealed class Registro1620Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1620).Assembly);

    [Fact]
    public void Atributo_Declara1620_Nivel3_Bloco1()
    {
        var atributo = typeof(Registro1620).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1620");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1620Com4CamposNaOrdem()
    {
        _catalogo.TentarObter("1620".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1620");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5]);
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "PerApuCred", "OrigCred", "CodCred", "VlCred",
        ]);
        meta.Campos[0].Tamanho.Should().Be(6);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // PerApuCred
        meta.Campos[1].Tamanho.Should().Be(2);
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // OrigCred
        meta.Campos[2].Tamanho.Should().Be(3);
        meta.Campos[2].Obrigatorio.Should().BeTrue();   // CodCred
        meta.Campos[3].Obrigatorio.Should().BeTrue();   // VlCred
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1620".AsSpan(), out var meta);
        var registro = (Registro1620)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "012021".AsSpan());  // PerApuCred
        meta.Campos[1].Definidor(registro, "01".AsSpan());      // OrigCred
        meta.Campos[2].Definidor(registro, "101".AsSpan());     // CodCred
        meta.Campos[3].Definidor(registro, "1500,00".AsSpan()); // VlCred

        registro.PerApuCred.Should().Be("012021");
        registro.OrigCred.Should().Be(IndicadorOrigemCreditoExtemporaneo.OperacoesProprias);
        registro.CodCred.Should().Be(CodigoTipoCredito.MercadoInternoTributadoAliquotaBasica);
        registro.VlCred.Should().Be(1500.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1620".AsSpan(), out var meta);
        var registro = (Registro1620)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty);  // PerApuCred

        registro.PerApuCred.Should().BeNull();
    }

    [Theory]
    [InlineData("01", IndicadorOrigemCreditoExtemporaneo.OperacoesProprias)]
    [InlineData("02", IndicadorOrigemCreditoExtemporaneo.TransferidoPorSucedida)]
    public void Definidor_OrigCred_MapeiaValoresValidos(string sped, IndicadorOrigemCreditoExtemporaneo esperado)
    {
        _catalogo.TentarObter("1620".AsSpan(), out var meta);
        var registro = (Registro1620)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, sped.AsSpan());

        registro.OrigCred.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1620|012021|01|101|1500,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CreditoTransferido_PreservaTextoCanonico()
    {
        const string sped = "|1620|032022|02|301|3250,75|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.LerStreamingAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.EscreverAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }
}
