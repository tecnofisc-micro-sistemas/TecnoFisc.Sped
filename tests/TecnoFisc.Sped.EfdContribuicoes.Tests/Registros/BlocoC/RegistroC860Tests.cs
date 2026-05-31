using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC860Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC860).Assembly);

    [Fact]
    public void Atributo_DeclaraC860_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC860).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C860");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC860ComCincoCamposNaOrdem()
    {
        _catalogo.TentarObter("C860".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C860");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodMod", "NrSat", "DtDoc", "DocIni", "DocFim",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // CodMod
        meta.Campos[1].Tamanho.Should().Be(9);
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // NrSat
        meta.Campos[2].Tamanho.Should().Be(8);
        meta.Campos[2].Obrigatorio.Should().BeFalse();  // DtDoc
        meta.Campos[3].Tamanho.Should().Be(9);
        meta.Campos[3].Obrigatorio.Should().BeFalse();  // DocIni
        meta.Campos[4].Tamanho.Should().Be(9);
        meta.Campos[4].Obrigatorio.Should().BeFalse();  // DocFim
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C860".AsSpan(), out var meta);
        var registro = (RegistroC860)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "59".AsSpan());          // CodMod
        meta.Campos[1].Definidor(registro, "900001234".AsSpan());   // NrSat
        meta.Campos[2].Definidor(registro, "15012024".AsSpan());    // DtDoc
        meta.Campos[3].Definidor(registro, "1".AsSpan());           // DocIni
        meta.Campos[4].Definidor(registro, "100".AsSpan());         // DocFim

        registro.CodMod.Should().Be("59");
        registro.NrSat.Should().Be(900001234);
        registro.DtDoc.Should().Be(new DateOnly(2024, 1, 15));
        registro.DocIni.Should().Be(1);
        registro.DocFim.Should().Be(100);
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("C860".AsSpan(), out var meta);
        var registro = (RegistroC860)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);  // DtDoc
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);  // DocIni
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);  // DocFim

        registro.DtDoc.Should().BeNull();
        registro.DocIni.Should().BeNull();
        registro.DocFim.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|C860|59|900001234|15012024|1|100|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposObrigatoriosApenas_PreservaTextoCanonico()
    {
        const string sped = "|C860|59|900001234||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_OutroEquipamento_PreservaTextoCanonico()
    {
        const string sped = "|C860|59|123456789|01022024|501|750|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

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
}
