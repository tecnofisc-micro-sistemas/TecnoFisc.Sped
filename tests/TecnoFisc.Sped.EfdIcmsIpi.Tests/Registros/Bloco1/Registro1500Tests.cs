using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.233 - exercita a forma do <see cref="Registro1500"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (pp. 282-284): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1500Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1500).Assembly);

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
    public void Atributo_Declara1500_Nivel2_Bloco1()
    {
        var atributo = typeof(Registro1500).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1500");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1500Com26CamposNaOrdem()
    {
        _catalogo.TentarObter("1500".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1500");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IndOper", "IndEmit", "CodPart", "CodMod", "CodSit",
            "Ser", "Sub", "CodCons", "NumDoc", "DtDoc", "DtES",
            "VlDoc", "VlDesc", "VlForn", "VlServNt", "VlTerc", "VlDa",
            "VlBcIcms", "VlIcms", "VlBcIcmsSt", "VlIcmsSt", "CodInf",
            "VlPis", "VlCofins", "TpLigacao", "CodGrupoTensao"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 26));
        meta.Campos.Where(c => c.Obrigatorio).Select(c => c.Nome).Should().Equal([
            "IndOper", "IndEmit", "CodPart", "CodMod", "CodSit",
            "CodCons", "NumDoc", "DtDoc", "DtES", "VlDoc", "VlForn"
        ]);
        meta.Campos[8].Tamanho.Should().Be(9);
        meta.Campos[9].Formato.Should().Be("ddMMyyyy");
        meta.Campos[11].Decimais.Should().Be(2);
        meta.Campos[25].Tamanho.Should().Be(2);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1500".AsSpan(), out var meta);
        var registro = (Registro1500)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());
        meta.Campos[1].Definidor(registro, "0".AsSpan());
        meta.Campos[2].Definidor(registro, "ADQ001".AsSpan());
        meta.Campos[3].Definidor(registro, "06".AsSpan());
        meta.Campos[4].Definidor(registro, "00".AsSpan());
        meta.Campos[5].Definidor(registro, "A".AsSpan());
        meta.Campos[6].Definidor(registro, "1".AsSpan());
        meta.Campos[7].Definidor(registro, "01".AsSpan());
        meta.Campos[8].Definidor(registro, "123456789".AsSpan());
        meta.Campos[9].Definidor(registro, "01012021".AsSpan());
        meta.Campos[10].Definidor(registro, "02012021".AsSpan());
        meta.Campos[11].Definidor(registro, "1500,00".AsSpan());
        meta.Campos[12].Definidor(registro, "50,00".AsSpan());
        meta.Campos[13].Definidor(registro, "1200,00".AsSpan());
        meta.Campos[14].Definidor(registro, "100,00".AsSpan());
        meta.Campos[15].Definidor(registro, "30,00".AsSpan());
        meta.Campos[16].Definidor(registro, "20,00".AsSpan());
        meta.Campos[17].Definidor(registro, "800,00".AsSpan());
        meta.Campos[18].Definidor(registro, "96,00".AsSpan());
        meta.Campos[19].Definidor(registro, "200,00".AsSpan());
        meta.Campos[20].Definidor(registro, "24,00".AsSpan());
        meta.Campos[21].Definidor(registro, "INF001".AsSpan());
        meta.Campos[22].Definidor(registro, "5,00".AsSpan());
        meta.Campos[23].Definidor(registro, "10,00".AsSpan());
        meta.Campos[24].Definidor(registro, "1".AsSpan());
        meta.Campos[25].Definidor(registro, "07".AsSpan());

        registro.IndOper.Should().Be(IndicadorOperacao.Saida);
        registro.IndEmit.Should().Be(IndicadorEmissorDocumento.EmissaoPropria);
        registro.CodPart.Should().Be("ADQ001");
        registro.CodMod.Should().Be("06");
        registro.CodSit.Should().Be(CodigoSituacaoDocumentoFiscal.DocumentoRegular);
        registro.Ser.Should().Be("A");
        registro.Sub.Should().Be(1);
        registro.CodCons.Should().Be("01");
        registro.NumDoc.Should().Be(123456789L);
        registro.DtDoc.Should().Be(new DateOnly(2021, 1, 1));
        registro.DtES.Should().Be(new DateOnly(2021, 1, 2));
        registro.VlDoc.Should().Be(1500.00m);
        registro.VlDesc.Should().Be(50.00m);
        registro.VlForn.Should().Be(1200.00m);
        registro.VlServNt.Should().Be(100.00m);
        registro.VlTerc.Should().Be(30.00m);
        registro.VlDa.Should().Be(20.00m);
        registro.VlBcIcms.Should().Be(800.00m);
        registro.VlIcms.Should().Be(96.00m);
        registro.VlBcIcmsSt.Should().Be(200.00m);
        registro.VlIcmsSt.Should().Be(24.00m);
        registro.CodInf.Should().Be("INF001");
        registro.VlPis.Should().Be(5.00m);
        registro.VlCofins.Should().Be(10.00m);
        registro.TpLigacao.Should().Be(TipoLigacaoEletrica.Monofasico);
        registro.CodGrupoTensao.Should().Be("07");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1500".AsSpan(), out var meta);
        var registro = (Registro1500)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.IndOper.Should().BeNull();
        registro.IndEmit.Should().BeNull();
        registro.CodPart.Should().BeNull();
        registro.CodMod.Should().BeNull();
        registro.CodSit.Should().BeNull();
        registro.Ser.Should().BeNull();
        registro.Sub.Should().BeNull();
        registro.CodCons.Should().BeNull();
        registro.NumDoc.Should().BeNull();
        registro.DtDoc.Should().BeNull();
        registro.DtES.Should().BeNull();
        registro.VlDoc.Should().BeNull();
        registro.VlDesc.Should().BeNull();
        registro.VlForn.Should().BeNull();
        registro.VlServNt.Should().BeNull();
        registro.VlTerc.Should().BeNull();
        registro.VlDa.Should().BeNull();
        registro.VlBcIcms.Should().BeNull();
        registro.VlIcms.Should().BeNull();
        registro.VlBcIcmsSt.Should().BeNull();
        registro.VlIcmsSt.Should().BeNull();
        registro.CodInf.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.TpLigacao.Should().BeNull();
        registro.CodGrupoTensao.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|1500|1|0|ADQ001|06|00|A|1|01|123456789|01012021|02012021|1500,00|50,00|1200,00|100,00|30,00|20,00|800,00|96,00|200,00|24,00|INF001|5,00|10,00|1|07|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposFacultativosVazios_PreservaTextoCanonico()
    {
        const string sped =
            "|1500|1|0|ADQ001|06|06|||02|987654321|15032021||2500,00||2500,00|||||||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("1", TipoLigacaoEletrica.Monofasico)]
    [InlineData("2", TipoLigacaoEletrica.Bifasico)]
    [InlineData("3", TipoLigacaoEletrica.Trifasico)]
    public void TpLigacao_Definidor_AtribuiValorCorreto(string valor, TipoLigacaoEletrica esperado)
    {
        _catalogo.TentarObter("1500".AsSpan(), out var meta);
        var registro = (Registro1500)meta!.Fabrica();
        var campo = meta.Campos.First(c => c.Nome == "TpLigacao");

        campo.Definidor(registro, valor.AsSpan());

        registro.TpLigacao.Should().Be(esperado);
    }
}
