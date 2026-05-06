using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC490Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC490).Assembly);

    [Fact]
    public void Atributo_DeclaraC490_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC490).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C490");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC490ComTresCamposNaOrdem()
    {
        _catalogo.TentarObter("C490".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C490");
        meta.Campos.Select(c => c.Nome).Should().Equal(["DtDocIni", "DtDocFin", "CodMod"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
        meta.Campos[0].Tamanho.Should().Be(8);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // DtDocIni
        meta.Campos[1].Tamanho.Should().Be(8);
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // DtDocFin
        meta.Campos[2].Tamanho.Should().Be(2);
        meta.Campos[2].Obrigatorio.Should().BeTrue();   // CodMod
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C490".AsSpan(), out var meta);
        var registro = (RegistroC490)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01012024".AsSpan()); // DtDocIni
        meta.Campos[1].Definidor(registro, "31012024".AsSpan()); // DtDocFin
        meta.Campos[2].Definidor(registro, "02".AsSpan());       // CodMod

        registro.DtDocIni.Should().Be(new DateOnly(2024, 1, 1));
        registro.DtDocFin.Should().Be(new DateOnly(2024, 1, 31));
        registro.CodMod.Should().Be("02");
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|C490|01012024|31012024|02|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComModeloCupomEletronico_PreservaTextoCanonico()
    {
        const string sped = "|C490|01032024|31032024|59|\r\n";

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
