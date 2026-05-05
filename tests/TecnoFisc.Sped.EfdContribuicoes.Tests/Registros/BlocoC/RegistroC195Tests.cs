using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC195Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC195).Assembly);

    [Fact]
    public void Atributo_DeclaraC195_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC195).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C195");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC195Com11CamposNaOrdem()
    {
        _catalogo.TentarObter("C195".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C195");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CnpjCpfPart", "CstCofins", "Cfop", "VlItem", "VlDesc",
            "VlBcCofins", "AliqCofins", "QuantBcCofins", "AliqCofinsQuant", "VlCofins", "CodCta",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]);
        meta.Campos[0].Tamanho.Should().Be(14);
        meta.Campos[0].Obrigatorio.Should().BeFalse(); // CnpjCpfPart
        meta.Campos[1].Tamanho.Should().Be(2);
        meta.Campos[1].Obrigatorio.Should().BeTrue();  // CstCofins
        meta.Campos[2].Tamanho.Should().Be(4);
        meta.Campos[2].Obrigatorio.Should().BeTrue();  // Cfop
        meta.Campos[3].Obrigatorio.Should().BeTrue();  // VlItem
        meta.Campos[6].Tamanho.Should().Be(8);         // AliqCofins tamanho fixo
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C195".AsSpan(), out var meta);
        var registro = (RegistroC195)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "12345678000195".AsSpan()); // CnpjCpfPart
        meta.Campos[1].Definidor(registro, "50".AsSpan());             // CstCofins
        meta.Campos[2].Definidor(registro, "1101".AsSpan());           // Cfop
        meta.Campos[3].Definidor(registro, "2000,00".AsSpan());        // VlItem
        meta.Campos[4].Definidor(registro, "100,00".AsSpan());         // VlDesc
        meta.Campos[5].Definidor(registro, "1900,00".AsSpan());        // VlBcCofins
        meta.Campos[6].Definidor(registro, "7,6000".AsSpan());         // AliqCofins
        meta.Campos[7].Definidor(registro, "20,000".AsSpan());         // QuantBcCofins
        meta.Campos[8].Definidor(registro, "0,0460".AsSpan());         // AliqCofinsQuant
        meta.Campos[9].Definidor(registro, "144,40".AsSpan());         // VlCofins
        meta.Campos[10].Definidor(registro, "1.2.01.001".AsSpan());    // CodCta

        registro.CnpjCpfPart.Should().Be("12345678000195");
        registro.CstCofins.Should().Be(50);
        registro.Cfop.Should().Be(Cfop.Criar("1101"));
        registro.VlItem.Should().Be(2000m);
        registro.VlDesc.Should().Be(100m);
        registro.VlBcCofins.Should().Be(1900m);
        registro.AliqCofins.Should().Be(7.6m);
        registro.QuantBcCofins.Should().Be(20m);
        registro.AliqCofinsQuant.Should().Be(0.046m);
        registro.VlCofins.Should().Be(144.40m);
        registro.CodCta.Should().Be("1.2.01.001");
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("C195".AsSpan(), out var meta);
        var registro = (RegistroC195)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty);  // CnpjCpfPart
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlDesc
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlBcCofins
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);  // AliqCofins
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);  // QuantBcCofins
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);  // AliqCofinsQuant
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlCofins
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta

        registro.CnpjCpfPart.Should().BeNull();
        registro.VlDesc.Should().BeNull();
        registro.VlBcCofins.Should().BeNull();
        registro.AliqCofins.Should().BeNull();
        registro.QuantBcCofins.Should().BeNull();
        registro.AliqCofinsQuant.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C195|12345678000195|50|1101|2000,00|100,00|1900,00|7,6000|20,000|0,0460|144,40|1.2.01.001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemParticipanteECamposOpcionais_PreservaTextoCanonico()
    {
        const string sped =
            "|C195||70|1556|5000,00||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.LerAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.EscreverAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }
}
