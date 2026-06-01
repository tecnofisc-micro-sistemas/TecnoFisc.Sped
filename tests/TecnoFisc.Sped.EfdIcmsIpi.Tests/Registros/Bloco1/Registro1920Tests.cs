using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco1;

/// <summary>
/// Sub-stage 8.241 - exercita a forma do <see cref="Registro1920"/> contra o Guia Pratico
/// EFD-ICMS/IPI V3.0.6 (p. 290): metadados de catalogo, mapeamento de campos e
/// invariante de round-trip parse -> gerar -> texto identico.
/// </summary>
public sealed class Registro1920Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1920).Assembly);

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
    public void Atributo_Declara1920_Nivel4_Bloco1()
    {
        var atributo = typeof(Registro1920).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1920");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1920Com12CamposNaOrdem()
    {
        _catalogo.TentarObter("1920".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1920");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "VlTotTransfDebitosOa", "VlTotAjDebitosOa", "VlEstornosCredOa",
            "VlTotTransfCreditosOa", "VlTotAjCreditosOa", "VlEstornosDebOa",
            "VlSldCredorAntOa", "VlSldApuradoOa", "VlTotDed", "VlIcmsRecolherOa",
            "VlSldCredorTranspOa", "DebEspOa"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13]);
        meta.Campos.Where(c => c.Obrigatorio).Select(c => c.Nome).Should().Equal([
            "VlTotTransfDebitosOa", "VlTotAjDebitosOa", "VlEstornosCredOa",
            "VlTotTransfCreditosOa", "VlTotAjCreditosOa", "VlEstornosDebOa",
            "VlSldCredorAntOa", "VlSldApuradoOa", "VlTotDed", "VlIcmsRecolherOa",
            "VlSldCredorTranspOa", "DebEspOa"
        ]);
        meta.Campos.Select(c => c.Tamanho).Should().Equal([0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);
        meta.Campos.Select(c => c.Decimais).Should().Equal([2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1920".AsSpan(), out var meta);
        var registro = (Registro1920)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1000,00".AsSpan());
        meta.Campos[1].Definidor(registro, "100,00".AsSpan());
        meta.Campos[2].Definidor(registro, "50,00".AsSpan());
        meta.Campos[3].Definidor(registro, "800,00".AsSpan());
        meta.Campos[4].Definidor(registro, "80,00".AsSpan());
        meta.Campos[5].Definidor(registro, "40,00".AsSpan());
        meta.Campos[6].Definidor(registro, "200,00".AsSpan());
        meta.Campos[7].Definidor(registro, "30,00".AsSpan());
        meta.Campos[8].Definidor(registro, "10,00".AsSpan());
        meta.Campos[9].Definidor(registro, "20,00".AsSpan());
        meta.Campos[10].Definidor(registro, "0,00".AsSpan());
        meta.Campos[11].Definidor(registro, "5,00".AsSpan());

        registro.VlTotTransfDebitosOa.Should().Be(1000.00m);
        registro.VlTotAjDebitosOa.Should().Be(100.00m);
        registro.VlEstornosCredOa.Should().Be(50.00m);
        registro.VlTotTransfCreditosOa.Should().Be(800.00m);
        registro.VlTotAjCreditosOa.Should().Be(80.00m);
        registro.VlEstornosDebOa.Should().Be(40.00m);
        registro.VlSldCredorAntOa.Should().Be(200.00m);
        registro.VlSldApuradoOa.Should().Be(30.00m);
        registro.VlTotDed.Should().Be(10.00m);
        registro.VlIcmsRecolherOa.Should().Be(20.00m);
        registro.VlSldCredorTranspOa.Should().Be(0.00m);
        registro.DebEspOa.Should().Be(5.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1920".AsSpan(), out var meta);
        var registro = (Registro1920)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.VlTotTransfDebitosOa.Should().Be(0m);
        registro.VlTotAjDebitosOa.Should().Be(0m);
        registro.VlEstornosCredOa.Should().Be(0m);
        registro.VlTotTransfCreditosOa.Should().Be(0m);
        registro.VlTotAjCreditosOa.Should().Be(0m);
        registro.VlEstornosDebOa.Should().Be(0m);
        registro.VlSldCredorAntOa.Should().Be(0m);
        registro.VlSldApuradoOa.Should().Be(0m);
        registro.VlTotDed.Should().Be(0m);
        registro.VlIcmsRecolherOa.Should().Be(0m);
        registro.VlSldCredorTranspOa.Should().Be(0m);
        registro.DebEspOa.Should().Be(0m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|1920|1000,00|100,00|50,00|800,00|80,00|40,00|200,00|30,00|10,00|20,00|0,00|5,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComSaldoCredorTransportado_PreservaTextoCanonico()
    {
        const string sped =
            "|1920|100,00|10,00|5,00|200,00|20,00|10,00|50,00|0,00|0,00|0,00|165,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
