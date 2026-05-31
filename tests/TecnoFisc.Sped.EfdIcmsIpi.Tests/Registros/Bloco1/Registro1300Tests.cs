using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.224 - exercita a forma do <see cref="Registro1300"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 275-276): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1300Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1300).Assembly);

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
    public void Atributo_Declara1300_Nivel2_Bloco1()
    {
        var atributo = typeof(Registro1300).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1300");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1300ComDezCamposNaOrdem()
    {
        _catalogo.TentarObter("1300".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1300");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodItem",
            "DtFech",
            "EstqAbert",
            "VolEntr",
            "VolDisp",
            "VolSaidas",
            "EstqEscr",
            "ValAjPerda",
            "ValAjGanho",
            "FechFisico",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 10));
        meta.Campos.Should().OnlyContain(c => c.Obrigatorio);
        meta.Campos[0].Tamanho.Should().Be(60);
        meta.Campos[1].Tamanho.Should().Be(8);
        meta.Campos[1].Formato.Should().Be("ddMMyyyy");
        meta.Campos.Skip(2).Should().OnlyContain(c => c.Tamanho == 0);
        meta.Campos.Skip(2).Should().OnlyContain(c => c.Decimais == 3);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1300".AsSpan(), out var meta);
        var registro = (Registro1300)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "DIESEL-S10".AsSpan());
        meta.Campos[1].Definidor(registro, "15032024".AsSpan());
        meta.Campos[2].Definidor(registro, "1000,125".AsSpan());
        meta.Campos[3].Definidor(registro, "500,250".AsSpan());
        meta.Campos[4].Definidor(registro, "1500,375".AsSpan());
        meta.Campos[5].Definidor(registro, "800,100".AsSpan());
        meta.Campos[6].Definidor(registro, "700,275".AsSpan());
        meta.Campos[7].Definidor(registro, "1,500".AsSpan());
        meta.Campos[8].Definidor(registro, "0,750".AsSpan());
        meta.Campos[9].Definidor(registro, "699,525".AsSpan());

        registro.CodItem.Should().Be("DIESEL-S10");
        registro.DtFech.Should().Be(new DateOnly(2024, 3, 15));
        registro.EstqAbert.Should().Be(1000.125m);
        registro.VolEntr.Should().Be(500.250m);
        registro.VolDisp.Should().Be(1500.375m);
        registro.VolSaidas.Should().Be(800.100m);
        registro.EstqEscr.Should().Be(700.275m);
        registro.ValAjPerda.Should().Be(1.500m);
        registro.ValAjGanho.Should().Be(0.750m);
        registro.FechFisico.Should().Be(699.525m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNuloOuZero()
    {
        _catalogo.TentarObter("1300".AsSpan(), out var meta);
        var registro = (Registro1300)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, Span<char>.Empty);

        registro.CodItem.Should().BeNull();
        registro.DtFech.Should().Be(default);
        registro.EstqAbert.Should().Be(0m);
        registro.VolEntr.Should().Be(0m);
        registro.VolDisp.Should().Be(0m);
        registro.VolSaidas.Should().Be(0m);
        registro.EstqEscr.Should().Be(0m);
        registro.ValAjPerda.Should().Be(0m);
        registro.ValAjGanho.Should().Be(0m);
        registro.FechFisico.Should().Be(0m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1300|DIESEL-S10|15032024|1000,125|500,250|1500,375|800,100|700,275|1,500|0,750|699,525|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_MovimentacaoSemAjustes_PreservaTextoCanonico()
    {
        const string sped = "|1300|GASOLINA-C|20042024|250,000|100,000|350,000|75,500|274,500|0,000|0,000|274,500|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
