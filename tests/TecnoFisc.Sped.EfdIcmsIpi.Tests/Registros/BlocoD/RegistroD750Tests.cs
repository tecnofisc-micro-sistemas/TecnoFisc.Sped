using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.017.012 — exercita a forma do <see cref="RegistroD750"/> (escrituração consolidada
/// NFCom, código 62) contra o Guia Prático EFD-ICMS/IPI V3.2.2 (p. 219, Subseção 12): metadados
/// de catálogo, mapeamento de campos, decisões de tipo (COD_MOD e SER lazy, DED V019) e invariante
/// de round-trip parse → texto idêntico.
/// </summary>
public sealed class RegistroD750Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD750).Assembly);

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

    [Fact]
    public void Atributo_DeclaraD750_Nivel2_BlocoD_IntroduzidoEmV017()
    {
        var atributo = typeof(RegistroD750).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D750");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("D");
        atributo.IntroduzidoEm.Should().Be((int)LayoutEfdIcmsIpi.V017);
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD750Com16CamposNaOrdem()
    {
        _catalogo.TentarObter("D750".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D750");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodMod", "Ser", "DtDoc", "QtdCons", "IndPrepago",
            "VlDoc", "VlServ", "VlServNt", "VlTerc", "VlDesc",
            "VlDa", "VlBcIcms", "VlIcms", "VlPis", "VlCofins",
            "Ded",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 16));
    }

    [Fact]
    public void Definidor_AtribuiCamposObrigatorios()
    {
        _catalogo.TentarObter("D750".AsSpan(), out var meta);
        var registro = (RegistroD750)meta!.Fabrica();

        // Campos obrigatórios (saída): 2,3,4,5,6,7,8,9,10,11,12,13,14.
        meta.Campos[0].Definidor(registro, "62".AsSpan());           // CodMod
        meta.Campos[1].Definidor(registro, "001".AsSpan());          // Ser
        meta.Campos[2].Definidor(registro, "31012025".AsSpan());     // DtDoc
        meta.Campos[3].Definidor(registro, "150".AsSpan());          // QtdCons
        meta.Campos[4].Definidor(registro, "1".AsSpan());            // IndPrepago (Pós-pago)
        meta.Campos[5].Definidor(registro, "15000,00".AsSpan());     // VlDoc
        meta.Campos[6].Definidor(registro, "14500,00".AsSpan());     // VlServ
        meta.Campos[7].Definidor(registro, "0,00".AsSpan());         // VlServNt
        meta.Campos[8].Definidor(registro, "0,00".AsSpan());         // VlTerc
        meta.Campos[9].Definidor(registro, "500,00".AsSpan());       // VlDesc
        meta.Campos[10].Definidor(registro, "0,00".AsSpan());        // VlDa
        meta.Campos[11].Definidor(registro, "14500,00".AsSpan());    // VlBcIcms
        meta.Campos[12].Definidor(registro, "2610,00".AsSpan());     // VlIcms

        registro.CodMod.Should().Be("62");
        registro.Ser.Should().Be("001");
        registro.DtDoc.Should().Be(new DateOnly(2025, 1, 31));
        registro.QtdCons.Should().Be(150);
        registro.IndPrepago.Should().Be(FormaPagamentoConsolidadaNFCom.PosPago);
        registro.VlDoc.Should().Be(15000.00m);
        registro.VlServ.Should().Be(14500.00m);
        registro.VlServNt.Should().Be(0m);
        registro.VlTerc.Should().Be(0m);
        registro.VlDesc.Should().Be(500.00m);
        registro.VlDa.Should().Be(0m);
        registro.VlBcIcms.Should().Be(14500.00m);
        registro.VlIcms.Should().Be(2610.00m);
    }

    [Fact]
    public void Definidor_CodModAceitaTextoLazy()
    {
        // Justificativa: campo 02 COD_MOD do D750 é declarado como string? (lazy) no modelo único
        // para tolerar mudanças de tipo entre versões do leiaute (tracker indica V018 muda C→N;
        // PDF v3.2.2 lista C). Texto canônico "62" deve ser preservado sem conversão.
        _catalogo.TentarObter("D750".AsSpan(), out var meta);
        var registro = (RegistroD750)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "62".AsSpan());

        registro.CodMod.Should().Be("62");
        typeof(RegistroD750).GetProperty("CodMod")!.PropertyType.Should().Be<string>();
    }

    [Fact]
    public void Definidor_SerAceitaTextoLazy()
    {
        // Mesma justificativa do CodMod: campo 03 SER declarado como string? (lazy) para tolerar
        // mudanças de tipo entre versões. Texto não-numérico deve ser preservado sem conversão.
        _catalogo.TentarObter("D750".AsSpan(), out var meta);
        var registro = (RegistroD750)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, "1".AsSpan());

        registro.Ser.Should().Be("1");
        typeof(RegistroD750).GetProperty("Ser")!.PropertyType.Should().Be<string>();
    }

    [Theory]
    [InlineData(FormaPagamentoConsolidadaNFCom.PrePago, "0")]
    [InlineData(FormaPagamentoConsolidadaNFCom.PosPago, "1")]
    public void Definidor_IndPrepago_MapeiaDoisValores(FormaPagamentoConsolidadaNFCom esperado, string entrada)
    {
        _catalogo.TentarObter("D750".AsSpan(), out var meta);
        var registro = (RegistroD750)meta!.Fabrica();

        meta.Campos[4].Definidor(registro, entrada.AsSpan());

        registro.IndPrepago.Should().Be(esperado);
    }

    [Theory]
    [InlineData(FormaPagamentoConsolidadaNFCom.PrePago, "0")]
    [InlineData(FormaPagamentoConsolidadaNFCom.PosPago, "1")]
    public void Serializar_IndPrepago_RetornaCodigo(FormaPagamentoConsolidadaNFCom valor, string esperado)
    {
        _catalogo.TentarObter("D750".AsSpan(), out var meta);
        var registro = (RegistroD750)meta!.Fabrica();
        registro.IndPrepago = valor;

        var texto = meta!.Campos[4].Serializar(registro);

        texto.Should().Be(esperado);
    }

    [Fact]
    public void Definidor_Ded_DeclaradoComoDesdeVersaoV019()
    {
        _catalogo.TentarObter("D750".AsSpan(), out var meta);

        var dedCampo = meta!.Campos.Single(c => c.Nome == "Ded");
        dedCampo.DesdeVersao.Should().Be((int)LayoutEfdIcmsIpi.V019);
        dedCampo.Ordem.Should().Be(17);
    }

    [Fact]
    public async Task RoundTrip_NFComConsolidadaCompleta_PreservaTextoCanonico()
    {
        // NFCom consolidada de saída, pós-pago, todos os campos populados incluindo VL_PIS,
        // VL_COFINS e DED (V019). 150 documentos consolidados emitidos em 31/01/2025.
        var sped =
            "|D750|62|001|31012025|150|1|15000,00|14500,00|0,00|0,00|500,00|0,00|14500,00|2610,00|145,00|667,00|10,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // NFCom consolidada pré-pago, somente campos obrigatórios — VL_PIS, VL_COFINS e DED vazios.
        var sped =
            "|D750|62|001|31012025|50|0|5000,00|4800,00|0,00|0,00|200,00|0,00|4800,00|864,00||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
