using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.234 - exercita a forma do <see cref="Registro1510"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (pp. 285-286): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1510Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1510).Assembly);

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
    public void Atributo_Declara1510_Nivel3_Bloco1()
    {
        var atributo = typeof(Registro1510).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1510");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1510Com20CamposNaOrdem()
    {
        _catalogo.TentarObter("1510".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1510");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "NumItem", "CodItem", "CodClass", "Qtd", "Unid",
            "VlItem", "VlDesc", "CstIcms", "Cfop",
            "VlBcIcms", "AliqIcms", "VlIcms",
            "VlBcIcmsSt", "AliqSt", "VlIcmsSt",
            "IndRec", "CodPart", "VlPis", "VlCofins", "CodCta"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 20));
        meta.Campos.Where(c => c.Obrigatorio).Select(c => c.Nome).Should().Equal([
            "NumItem", "CodItem", "CodClass", "VlItem", "CstIcms", "Cfop", "IndRec"
        ]);
        meta.Campos[2].Tamanho.Should().Be(4);
        meta.Campos[3].Decimais.Should().Be(3);
        meta.Campos[10].Tamanho.Should().Be(6);
        meta.Campos[19].Tamanho.Should().Be(0);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1510".AsSpan(), out var meta);
        var registro = (Registro1510)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());
        meta.Campos[1].Definidor(registro, "ENERGIA001".AsSpan());
        meta.Campos[2].Definidor(registro, "1001".AsSpan());
        meta.Campos[3].Definidor(registro, "250,000".AsSpan());
        meta.Campos[4].Definidor(registro, "kWh".AsSpan());
        meta.Campos[5].Definidor(registro, "500,00".AsSpan());
        meta.Campos[6].Definidor(registro, "10,00".AsSpan());
        meta.Campos[7].Definidor(registro, "060".AsSpan());
        meta.Campos[8].Definidor(registro, "6102".AsSpan());
        meta.Campos[9].Definidor(registro, "250,00".AsSpan());
        meta.Campos[10].Definidor(registro, "12,00".AsSpan());
        meta.Campos[11].Definidor(registro, "30,00".AsSpan());
        meta.Campos[12].Definidor(registro, "100,00".AsSpan());
        meta.Campos[13].Definidor(registro, "12,00".AsSpan());
        meta.Campos[14].Definidor(registro, "12,00".AsSpan());
        meta.Campos[15].Definidor(registro, "0".AsSpan());
        meta.Campos[16].Definidor(registro, "PART001".AsSpan());
        meta.Campos[17].Definidor(registro, "5,00".AsSpan());
        meta.Campos[18].Definidor(registro, "10,00".AsSpan());
        meta.Campos[19].Definidor(registro, "CONTA001".AsSpan());

        registro.NumItem.Should().Be(1);
        registro.CodItem.Should().Be("ENERGIA001");
        registro.CodClass.Should().Be(1001);
        registro.Qtd.Should().Be(250.000m);
        registro.Unid.Should().Be("kWh");
        registro.VlItem.Should().Be(500.00m);
        registro.VlDesc.Should().Be(10.00m);
        registro.CstIcms.Should().Be(60);
        registro.Cfop.Should().Be(Cfop.Create("6102"));
        registro.VlBcIcms.Should().Be(250.00m);
        registro.AliqIcms.Should().Be(12.00m);
        registro.VlIcms.Should().Be(30.00m);
        registro.VlBcIcmsSt.Should().Be(100.00m);
        registro.AliqSt.Should().Be(12.00m);
        registro.VlIcmsSt.Should().Be(12.00m);
        registro.IndRec.Should().Be(IndicadorTipoReceita.ReceitaPropria);
        registro.CodPart.Should().Be("PART001");
        registro.VlPis.Should().Be(5.00m);
        registro.VlCofins.Should().Be(10.00m);
        registro.CodCta.Should().Be("CONTA001");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1510".AsSpan(), out var meta);
        var registro = (Registro1510)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.NumItem.Should().Be(0);
        registro.CodItem.Should().BeNull();
        registro.CodClass.Should().BeNull();
        registro.Qtd.Should().BeNull();
        registro.Unid.Should().BeNull();
        registro.VlItem.Should().Be(0m);
        registro.VlDesc.Should().BeNull();
        registro.CstIcms.Should().BeNull();
        registro.VlBcIcms.Should().BeNull();
        registro.AliqIcms.Should().BeNull();
        registro.VlIcms.Should().BeNull();
        registro.VlBcIcmsSt.Should().BeNull();
        registro.AliqSt.Should().BeNull();
        registro.VlIcmsSt.Should().BeNull();
        registro.IndRec.Should().BeNull();
        registro.CodPart.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|1510|1|ENERGIA001|1001|250,000|kWh|500,00|10,00|60|6102|250,00|12,00|30,00|100,00|12,00|12,00|0|PART001|5,00|10,00|CONTA001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposOpcionaisVazios_PreservaTextoCanonico()
    {
        const string sped =
            "|1510|1|ENERGIA001|1001|||500,00||60|6102|||||||0|||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComReceitaTerceiros_PreservaTextoCanonico()
    {
        const string sped =
            "|1510|2|ENERGIA002|2002|||320,00||40|6556|||||||1|DIST001||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("0", IndicadorTipoReceita.ReceitaPropria)]
    [InlineData("1", IndicadorTipoReceita.ReceitaTerceiros)]
    public void IndRec_Definidor_AtribuiValorCorreto(string valor, IndicadorTipoReceita esperado)
    {
        _catalogo.TentarObter("1510".AsSpan(), out var meta);
        var registro = (Registro1510)meta!.Fabrica();
        var campo = meta.Campos.First(c => c.Nome == "IndRec");

        campo.Definidor(registro, valor.AsSpan());

        registro.IndRec.Should().Be(esperado);
    }
}
