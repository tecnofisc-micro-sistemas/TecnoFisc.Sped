using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC395Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC395).Assembly);

    [Fact]
    public void Atributo_DeclaraC395_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC395).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C395");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC395ComSeteCamposNaOrdem()
    {
        _catalogo.TentarObter("C395".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C395");
        meta.Campos.Select(c => c.Nome).Should().Equal(
            ["CodMod", "CodPart", "Ser", "SubSer", "NumDoc", "DtDoc", "VlDoc"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();    // CodMod
        meta.Campos[1].Tamanho.Should().Be(60);
        meta.Campos[1].Obrigatorio.Should().BeFalse();   // CodPart
        meta.Campos[2].Tamanho.Should().Be(3);
        meta.Campos[2].Obrigatorio.Should().BeTrue();    // Ser
        meta.Campos[4].Tamanho.Should().Be(6);
        meta.Campos[4].Obrigatorio.Should().BeTrue();    // NumDoc
        meta.Campos[5].Tamanho.Should().Be(8);
        meta.Campos[5].Obrigatorio.Should().BeTrue();    // DtDoc
        meta.Campos[6].Obrigatorio.Should().BeTrue();    // VlDoc
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C395".AsSpan(), out var meta);
        var registro = (RegistroC395)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "02".AsSpan());         // CodMod
        meta.Campos[1].Definidor(registro, "FORN001".AsSpan());    // CodPart
        meta.Campos[2].Definidor(registro, "001".AsSpan());        // Ser
        meta.Campos[3].Definidor(registro, "A".AsSpan());          // SubSer
        meta.Campos[4].Definidor(registro, "000123".AsSpan());     // NumDoc
        meta.Campos[5].Definidor(registro, "15032024".AsSpan());   // DtDoc
        meta.Campos[6].Definidor(registro, "1500,00".AsSpan());    // VlDoc

        registro.CodMod.Should().Be("02");
        registro.CodPart.Should().Be("FORN001");
        registro.Ser.Should().Be("001");
        registro.SubSer.Should().Be("A");
        registro.NumDoc.Should().Be("000123");
        registro.DtDoc.Should().Be(new DateOnly(2024, 3, 15));
        registro.VlDoc.Should().Be(1500m);
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("C395".AsSpan(), out var meta);
        var registro = (RegistroC395)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty); // CodPart
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // SubSer

        registro.CodPart.Should().BeNull();
        registro.SubSer.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|C395|02|FORN001|001|A|000123|15032024|1500,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|C395|59||000||000456|20042024|850,50|\r\n";

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
