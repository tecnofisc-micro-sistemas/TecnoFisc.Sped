using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.231 - exercita a forma do <see cref="Registro1391"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 280-281): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico. Sub-stage 8.018.001 estende
/// para os campos 21-23 (QTD_RESIDUO_DDG/WDG/CANA) introduzidos em V018 (Guia Prático
/// 3.1.4 item 9).
/// </summary>
public sealed class Registro1391Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1391).Assembly);

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
    public void Atributo_Declara1391_Nivel3_Bloco1()
    {
        var atributo = (RegistroSpedAttribute?)Attribute.GetCustomAttribute(
            typeof(Registro1391),
            typeof(RegistroSpedAttribute));

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1391");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1391ComVinteEDoisCamposNaOrdem()
    {
        _catalogo.TentarObter("1391".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1391");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "DtRegistro",
            "QtdMoid",
            "EstqIni",
            "QtdProduz",
            "EntAnidHid",
            "OutrEntr",
            "Perda",
            "Cons",
            "SaiAniHid",
            "Saidas",
            "EstqFin",
            "EstqIniMel",
            "ProdDiaMel",
            "UtilMel",
            "ProdAlcMel",
            "Obs",
            "CodItem",
            "TpResiduo",
            "QtdResiduo",
            "QtdResiduoDdg",
            "QtdResiduoWdg",
            "QtdResiduoCana",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 22));
        meta.Campos.Where(c => c.Obrigatorio).Select(c => c.Nome).Should().Equal([
            "DtRegistro",
            "EstqIni",
            "EstqFin",
            "CodItem",
            "TpResiduo",
            "QtdResiduo",
        ]);
        meta.Campos[0].Tamanho.Should().Be(8);
        meta.Campos[0].Formato.Should().Be("ddMMyyyy");
        meta.Campos[16].Tamanho.Should().Be(60);
        meta.Campos[17].Tamanho.Should().Be(2);
        meta.Campos.Where(c => c.Nome is not "DtRegistro" and not "Obs" and not "CodItem" and not "TpResiduo"
                and not "QtdResiduoDdg" and not "QtdResiduoWdg" and not "QtdResiduoCana")
            .Should().OnlyContain(c => c.Decimais == 2);
        meta.Campos.Where(c => c.Nome is "QtdResiduoDdg" or "QtdResiduoWdg" or "QtdResiduoCana")
            .Should().OnlyContain(c => c.Decimais == 3);
    }

    [Fact]
    public void Catalogo_NovosCamposV018_DeclaramDesdeVersao()
    {
        _catalogo.TentarObter("1391".AsSpan(), out var meta).Should().BeTrue();

        foreach (var nome in new[] { "QtdResiduoDdg", "QtdResiduoWdg", "QtdResiduoCana" })
        {
            var prop = typeof(Registro1391).GetProperty(nome);
            prop.Should().NotBeNull($"campo {nome} deve existir como propriedade");
            var atributo = (CampoSpedAttribute?)Attribute.GetCustomAttribute(prop!, typeof(CampoSpedAttribute));
            atributo.Should().NotBeNull();
            atributo!.DesdeVersao.Should().Be((int)LayoutEfdIcmsIpi.V018, $"campo {nome} foi introduzido em V018");
        }
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1391".AsSpan(), out var meta);
        var registro = (Registro1391)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "15032024".AsSpan());
        meta.Campos[1].Definidor(registro, "1000,50".AsSpan());
        meta.Campos[2].Definidor(registro, "200,00".AsSpan());
        meta.Campos[3].Definidor(registro, "850,25".AsSpan());
        meta.Campos[4].Definidor(registro, "10,00".AsSpan());
        meta.Campos[5].Definidor(registro, "20,00".AsSpan());
        meta.Campos[6].Definidor(registro, "5,25".AsSpan());
        meta.Campos[7].Definidor(registro, "12,00".AsSpan());
        meta.Campos[8].Definidor(registro, "8,00".AsSpan());
        meta.Campos[9].Definidor(registro, "750,00".AsSpan());
        meta.Campos[10].Definidor(registro, "305,50".AsSpan());
        meta.Campos[11].Definidor(registro, "30,00".AsSpan());
        meta.Campos[12].Definidor(registro, "45,00".AsSpan());
        meta.Campos[13].Definidor(registro, "10,00".AsSpan());
        meta.Campos[14].Definidor(registro, "15,00".AsSpan());
        meta.Campos[15].Definidor(registro, "PRODUCAO DIARIA".AsSpan());
        meta.Campos[16].Definidor(registro, "INSUMO-CANA".AsSpan());
        meta.Campos[17].Definidor(registro, "01".AsSpan());
        meta.Campos[18].Definidor(registro, "120,75".AsSpan());
        meta.Campos[19].Definidor(registro, "1,234".AsSpan());
        meta.Campos[20].Definidor(registro, "2,345".AsSpan());
        meta.Campos[21].Definidor(registro, "3,456".AsSpan());

        registro.DtRegistro.Should().Be(new DateOnly(2024, 3, 15));
        registro.QtdMoid.Should().Be(1000.50m);
        registro.EstqIni.Should().Be(200.00m);
        registro.QtdProduz.Should().Be(850.25m);
        registro.EntAnidHid.Should().Be(10.00m);
        registro.OutrEntr.Should().Be(20.00m);
        registro.Perda.Should().Be(5.25m);
        registro.Cons.Should().Be(12.00m);
        registro.SaiAniHid.Should().Be(8.00m);
        registro.Saidas.Should().Be(750.00m);
        registro.EstqFin.Should().Be(305.50m);
        registro.EstqIniMel.Should().Be(30.00m);
        registro.ProdDiaMel.Should().Be(45.00m);
        registro.UtilMel.Should().Be(10.00m);
        registro.ProdAlcMel.Should().Be(15.00m);
        registro.Obs.Should().Be("PRODUCAO DIARIA");
        registro.CodItem.Should().Be("INSUMO-CANA");
        registro.TpResiduo.Should().Be(TipoResiduoProducaoUsina.BagacoCana);
        registro.QtdResiduo.Should().Be(120.75m);
        registro.QtdResiduoDdg.Should().Be(1.234m);
        registro.QtdResiduoWdg.Should().Be(2.345m);
        registro.QtdResiduoCana.Should().Be(3.456m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNuloOuDefault()
    {
        _catalogo.TentarObter("1391".AsSpan(), out var meta);
        var registro = (Registro1391)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, Span<char>.Empty);

        registro.DtRegistro.Should().Be(default(DateOnly));
        registro.QtdMoid.Should().BeNull();
        registro.EstqIni.Should().Be(0m);
        registro.QtdProduz.Should().BeNull();
        registro.EntAnidHid.Should().BeNull();
        registro.OutrEntr.Should().BeNull();
        registro.Perda.Should().BeNull();
        registro.Cons.Should().BeNull();
        registro.SaiAniHid.Should().BeNull();
        registro.Saidas.Should().BeNull();
        registro.EstqFin.Should().Be(0m);
        registro.EstqIniMel.Should().BeNull();
        registro.ProdDiaMel.Should().BeNull();
        registro.UtilMel.Should().BeNull();
        registro.ProdAlcMel.Should().BeNull();
        registro.Obs.Should().BeNull();
        registro.CodItem.Should().BeNull();
        registro.TpResiduo.Should().Be(default(TipoResiduoProducaoUsina));
        registro.QtdResiduo.Should().Be(0m);
        registro.QtdResiduoDdg.Should().BeNull();
        registro.QtdResiduoWdg.Should().BeNull();
        registro.QtdResiduoCana.Should().BeNull();
    }

    [Theory]
    [InlineData("01", TipoResiduoProducaoUsina.BagacoCana)]
    [InlineData("02", TipoResiduoProducaoUsina.Ddg)]
    [InlineData("03", TipoResiduoProducaoUsina.Wdg)]
    public void Definidor_TipoResiduo_AtribuiValorPorCodigo(string valor, TipoResiduoProducaoUsina esperado)
    {
        _catalogo.TentarObter("1391".AsSpan(), out var meta);
        var registro = (Registro1391)meta!.Fabrica();

        meta.Campos[17].Definidor(registro, valor.AsSpan());

        registro.TpResiduo.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1391|15032024|1000,50|200,00|850,25|10,00|20,00|5,25|12,00|8,00|750,00|305,50|30,00|45,00|10,00|15,00|PRODUCAO DIARIA|INSUMO-CANA|01|120,75|1,234|2,345|3,456|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposCondicionaisVazios_PreservaTextoCanonico()
    {
        const string sped = "|1391|16032024||200,00||||||||305,50||||||INSUMO-MILHO|03|18,25||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
