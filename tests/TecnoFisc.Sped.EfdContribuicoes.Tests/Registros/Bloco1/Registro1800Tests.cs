using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco1;

public sealed class Registro1800Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1800).Assembly);

    [Fact]
    public void Atributo_Declara1800_Nivel2_Bloco1()
    {
        var atributo = typeof(Registro1800).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1800");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1800Com8CamposNaOrdem()
    {
        _catalogo.TentarObter("1800".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1800");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9]);
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IncImob", "RecRecebRet", "RecFinRet", "BcRet", "AliqRet", "VlRecUni", "DtRecUni", "CodRec",
        ]);
        meta.Campos[0].Tamanho.Should().Be(90);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // IncImob
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // RecRecebRet
        meta.Campos[2].Obrigatorio.Should().BeFalse();  // RecFinRet
        meta.Campos[3].Obrigatorio.Should().BeTrue();   // BcRet
        meta.Campos[4].Tamanho.Should().Be(6);
        meta.Campos[4].Obrigatorio.Should().BeTrue();   // AliqRet
        meta.Campos[5].Obrigatorio.Should().BeTrue();   // VlRecUni
        meta.Campos[6].Tamanho.Should().Be(8);
        meta.Campos[6].Obrigatorio.Should().BeFalse();  // DtRecUni
        meta.Campos[7].Tamanho.Should().Be(4);
        meta.Campos[7].Obrigatorio.Should().BeFalse();  // CodRec
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1800".AsSpan(), out var meta);
        var registro = (Registro1800)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "12345678000195".AsSpan());  // IncImob
        meta.Campos[1].Definidor(registro, "100000,00".AsSpan());       // RecRecebRet
        meta.Campos[2].Definidor(registro, "5000,00".AsSpan());         // RecFinRet
        meta.Campos[3].Definidor(registro, "105000,00".AsSpan());       // BcRet
        meta.Campos[4].Definidor(registro, "4,00".AsSpan());            // AliqRet
        meta.Campos[5].Definidor(registro, "4200,00".AsSpan());         // VlRecUni
        meta.Campos[6].Definidor(registro, "10052024".AsSpan());        // DtRecUni
        meta.Campos[7].Definidor(registro, "1767".AsSpan());            // CodRec

        registro.IncImob.Should().Be("12345678000195");
        registro.RecRecebRet.Should().Be(100000.00m);
        registro.RecFinRet.Should().Be(5000.00m);
        registro.BcRet.Should().Be(105000.00m);
        registro.AliqRet.Should().Be(4.00m);
        registro.VlRecUni.Should().Be(4200.00m);
        registro.DtRecUni.Should().Be(new DateOnly(2024, 5, 10));
        registro.CodRec.Should().Be("1767");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1800".AsSpan(), out var meta);
        var registro = (Registro1800)meta!.Fabrica();

        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);  // DtRecUni
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodRec

        registro.DtRecUni.Should().BeNull();
        registro.CodRec.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1800|12345678000195|100000,00|5000,00|105000,00|4,00|4200,00|10052024|1767|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|1800|12345678000195|80000,00||80000,00|1,00|800,00|||\r\n";

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
