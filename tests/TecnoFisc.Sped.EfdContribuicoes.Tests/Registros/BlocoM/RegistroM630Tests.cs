using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoM;

public sealed class RegistroM630Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroM630).Assembly);

    [Fact]
    public void Atributo_DeclaraM630_Nivel4_BlocoM()
    {
        var atributo = typeof(RegistroM630).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("M630");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("M");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroM630Com6CamposNaOrdem()
    {
        _catalogo.TentarObter("M630".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("M630");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CnpjPj", "VlVend", "VlNaoReceb", "VlContDif", "VlCredDif", "CodCred",
        ]);
        meta.Campos[0].Tamanho.Should().Be(14);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // CnpjPj
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // VlVend
        meta.Campos[2].Obrigatorio.Should().BeTrue();   // VlNaoReceb
        meta.Campos[3].Obrigatorio.Should().BeTrue();   // VlContDif
        meta.Campos[4].Obrigatorio.Should().BeFalse();  // VlCredDif
        meta.Campos[5].Tamanho.Should().Be(3);
        meta.Campos[5].Obrigatorio.Should().BeFalse();  // CodCred
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("M630".AsSpan(), out var meta);
        var registro = (RegistroM630)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "11222333000181".AsSpan()); // CnpjPj
        meta.Campos[1].Definidor(registro, "10000,00".AsSpan());       // VlVend
        meta.Campos[2].Definidor(registro, "3000,00".AsSpan());        // VlNaoReceb
        meta.Campos[3].Definidor(registro, "1500,00".AsSpan());        // VlContDif
        meta.Campos[4].Definidor(registro, "500,00".AsSpan());         // VlCredDif
        meta.Campos[5].Definidor(registro, "101".AsSpan());            // CodCred

        registro.CnpjPj.Should().Be(Cnpj.Create("11222333000181"));
        registro.VlVend.Should().Be(10000m);
        registro.VlNaoReceb.Should().Be(3000m);
        registro.VlContDif.Should().Be(1500m);
        registro.VlCredDif.Should().Be(500m);
        registro.CodCred.Should().Be(CodigoTipoCredito.MercadoInternoTributadoAliquotaBasica);
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("M630".AsSpan(), out var meta);
        var registro = (RegistroM630)meta!.Fabrica();

        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCredDif
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCred

        registro.VlCredDif.Should().BeNull();
        registro.CodCred.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|M630|11222333000181|10000,00|3000,00|1500,00|500,00|101|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|M630|11222333000181|10000,00|3000,00|1500,00|||\r\n";

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
