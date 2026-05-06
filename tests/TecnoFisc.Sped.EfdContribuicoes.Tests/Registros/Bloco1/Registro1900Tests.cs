using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco1;

public sealed class Registro1900Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1900).Assembly);

    [Fact]
    public void Atributo_Declara1900_Nivel2_Bloco1()
    {
        var atributo = typeof(Registro1900).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1900");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1900Com12CamposNaOrdem()
    {
        _catalogo.TentarObter("1900".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1900");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13]);
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "Cnpj", "CodMod", "Ser", "SubSer", "CodSit",
            "VlTotRec", "QuantDoc", "CstPis", "CstCofins", "Cfop",
            "InfCompl", "CodCta",
        ]);
        meta.Campos[0].Tamanho.Should().Be(14);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // Cnpj
        meta.Campos[1].Tamanho.Should().Be(2);
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // CodMod
        meta.Campos[2].Obrigatorio.Should().BeFalse();  // Ser
        meta.Campos[3].Obrigatorio.Should().BeFalse();  // SubSer
        meta.Campos[4].Obrigatorio.Should().BeFalse();  // CodSit
        meta.Campos[5].Decimais.Should().Be(2);
        meta.Campos[5].Obrigatorio.Should().BeTrue();   // VlTotRec
        meta.Campos[6].Obrigatorio.Should().BeFalse();  // QuantDoc
        meta.Campos[7].Tamanho.Should().Be(2);
        meta.Campos[7].Obrigatorio.Should().BeFalse();  // CstPis
        meta.Campos[8].Tamanho.Should().Be(2);
        meta.Campos[8].Obrigatorio.Should().BeFalse();  // CstCofins
        meta.Campos[9].Tamanho.Should().Be(4);
        meta.Campos[9].Obrigatorio.Should().BeFalse();  // Cfop
        meta.Campos[10].Obrigatorio.Should().BeFalse(); // InfCompl
        meta.Campos[11].Tamanho.Should().Be(255);
        meta.Campos[11].Obrigatorio.Should().BeFalse(); // CodCta
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1900".AsSpan(), out var meta);
        var registro = (Registro1900)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "12345678000195".AsSpan());  // Cnpj
        meta.Campos[1].Definidor(registro, "55".AsSpan());              // CodMod
        meta.Campos[2].Definidor(registro, "A".AsSpan());               // Ser
        meta.Campos[3].Definidor(registro, "1".AsSpan());               // SubSer
        meta.Campos[4].Definidor(registro, "00".AsSpan());              // CodSit
        meta.Campos[5].Definidor(registro, "150000,00".AsSpan());       // VlTotRec
        meta.Campos[6].Definidor(registro, "200".AsSpan());             // QuantDoc
        meta.Campos[7].Definidor(registro, "01".AsSpan());              // CstPis
        meta.Campos[8].Definidor(registro, "01".AsSpan());              // CstCofins
        meta.Campos[9].Definidor(registro, "5102".AsSpan());            // Cfop
        meta.Campos[10].Definidor(registro, "Vendas NF-e".AsSpan());    // InfCompl
        meta.Campos[11].Definidor(registro, "3.1.1.01".AsSpan());       // CodCta

        registro.Cnpj.Should().Be(Cnpj.Criar("12345678000195"));
        registro.CodMod.Should().Be("55");
        registro.Ser.Should().Be("A");
        registro.SubSer.Should().Be(1);
        registro.CodSit.Should().Be("00");
        registro.VlTotRec.Should().Be(150000.00m);
        registro.QuantDoc.Should().Be(200L);
        registro.CstPis.Should().Be("01");
        registro.CstCofins.Should().Be("01");
        registro.Cfop.Should().Be(Cfop.Criar("5102"));
        registro.InfCompl.Should().Be("Vendas NF-e");
        registro.CodCta.Should().Be("3.1.1.01");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1900".AsSpan(), out var meta);
        var registro = (Registro1900)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);   // Ser
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);   // SubSer
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);   // CodSit
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);   // QuantDoc
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);   // CstPis
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);   // CstCofins
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);   // Cfop
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty);  // InfCompl
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodCta

        registro.Ser.Should().BeNull();
        registro.SubSer.Should().BeNull();
        registro.CodSit.Should().BeNull();
        registro.QuantDoc.Should().BeNull();
        registro.CstPis.Should().BeNull();
        registro.CstCofins.Should().BeNull();
        registro.Cfop.Should().BeNull();
        registro.InfCompl.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1900|12345678000195|55|A|1|00|150000,00|200|01|01|5102|Vendas NF-e|3.1.1.01|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // SER, SUB_SER, COD_SIT vazios = 3 campos ausentes → 4 pipes entre COD_MOD e VL_TOT_REC
        const string sped = "|1900|12345678000195|99||||0,00|||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CodMod98_ISSQN_PreservaTextoCanonico()
    {
        // CFOP, INF_COMPL, COD_CTA vazios → 4 pipes no final (3 campos + fechamento)
        const string sped = "|1900|12345678000195|98|B||00|75000,50|50|07|07||||\r\n";

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
