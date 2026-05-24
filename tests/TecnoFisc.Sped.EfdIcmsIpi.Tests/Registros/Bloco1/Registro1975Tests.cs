using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.249 - exercita a forma do <see cref="Registro1975"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 299-300): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1975Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1975).Assembly);

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
    public void Atributo_Declara1975_Nivel3_Bloco1()
    {
        var atributo = typeof(Registro1975).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1975");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1975Com4CamposNaOrdem()
    {
        _catalogo.TentarObter("1975".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1975");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "AliqImpBase",
            "G310",
            "G311",
            "G312",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 4));
        meta.Campos.Select(c => c.Tamanho).Should().Equal([0, 0, 0, 0]);
        meta.Campos.Select(c => c.Decimais).Should().Equal([2, 2, 2, 2]);
        meta.Campos.Where(c => c.Obrigatorio).Select(c => c.Nome).Should().Equal([
            "AliqImpBase",
            "G310",
            "G311",
            "G312",
        ]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1975".AsSpan(), out var meta);
        var registro = (Registro1975)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "3,50".AsSpan());
        meta.Campos[1].Definidor(registro, "1000,00".AsSpan());
        meta.Campos[2].Definidor(registro, "800,00".AsSpan());
        meta.Campos[3].Definidor(registro, "28,00".AsSpan());

        registro.AliqImpBase.Should().Be(3.50m);
        registro.G310.Should().Be(1000.00m);
        registro.G311.Should().Be(800.00m);
        registro.G312.Should().Be(28.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveZeroParaCamposObrigatorios()
    {
        _catalogo.TentarObter("1975".AsSpan(), out var meta);
        var registro = (Registro1975)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, Span<char>.Empty);

        registro.AliqImpBase.Should().Be(0m);
        registro.G310.Should().Be(0m);
        registro.G311.Should().Be(0m);
        registro.G312.Should().Be(0m);
    }

    [Theory]
    [InlineData("3,50")]
    [InlineData("6,00")]
    [InlineData("8,00")]
    [InlineData("10,00")]
    public void AliqImpBase_Definidor_AtribuiValorValido(string valor)
    {
        _catalogo.TentarObter("1975".AsSpan(), out var meta);
        var registro = (Registro1975)meta!.Fabrica();
        var esperado = valor switch
        {
            "3,50" => 3.50m,
            "6,00" => 6.00m,
            "8,00" => 8.00m,
            "10,00" => 10.00m,
            _ => throw new ArgumentOutOfRangeException(nameof(valor)),
        };

        meta.Campos[0].Definidor(registro, valor.AsSpan());

        registro.AliqImpBase.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1975|3,50|1000,00|800,00|28,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComAliquota10EValoresZerados_PreservaTextoCanonico()
    {
        const string sped = "|1975|10,00|0,00|0,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
