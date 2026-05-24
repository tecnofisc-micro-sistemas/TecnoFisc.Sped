using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco0;

public sealed class Registro0400Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0400).Assembly);

    [Fact]
    public void Atributo_Declara0400_Nivel3_Bloco0()
    {
        var atributo = typeof(Registro0400).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0400");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0400Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("0400".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0400");
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodNat", "DescrNat"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0400".AsSpan(), out var meta);
        var registro = (Registro0400)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "VENDA".AsSpan());
        meta.Campos[1].Definidor(registro, "Venda de mercadorias".AsSpan());

        registro.CodNat.Should().Be("VENDA");
        registro.DescrNat.Should().Be("Venda de mercadorias");
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|0400|VENDA|Venda de mercadorias|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CodigoNumerico_PreservaTextoCanonico()
    {
        const string sped = "|0400|0001|Prestação de serviços tributados pelo ISS|\r\n";

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
