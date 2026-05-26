using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoK;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoK;

/// <summary>
/// Sub-stage 10.060 — exercita a forma do <see cref="RegistroK100"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 212–215): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroK100Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0000).Assembly);

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
    public void Atributo_DeclaraK100_Nivel3_BlocoK()
    {
        var atributo = typeof(RegistroK100).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("K100");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("K");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroK100Com9CamposNaOrdem()
    {
        _catalogo.TentarObter("K100".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("K100");
        meta.Campos.Select(c => c.Nome).Should().Equal(
            ["CodPais", "EmpCod", "Cnpj", "Nome", "PerPart", "Evento", "PerCons", "DataIniEmp", "DataFinEmp"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("K100".AsSpan(), out var meta);
        var registro = (RegistroK100)meta!.Fabrica();

        // Exemplo do manual (p. 215)
        meta.Campos[0].Definidor(registro, "105".AsSpan());
        meta.Campos[1].Definidor(registro, "1234".AsSpan());
        meta.Campos[2].Definidor(registro, "11111111".AsSpan());
        meta.Campos[3].Definidor(registro, "EMPRESA PARTICIPANTE Z".AsSpan());
        meta.Campos[4].Definidor(registro, "30,0000".AsSpan());
        meta.Campos[5].Definidor(registro, "S".AsSpan());
        meta.Campos[6].Definidor(registro, "100,0000".AsSpan());
        meta.Campos[7].Definidor(registro, "01012023".AsSpan());
        meta.Campos[8].Definidor(registro, "31122023".AsSpan());

        registro.CodPais.Should().Be(105);
        registro.EmpCod.Should().Be(1234);
        registro.Cnpj.Should().Be("11111111");
        registro.Nome.Should().Be("EMPRESA PARTICIPANTE Z");
        registro.PerPart.Should().Be(30.0000m);
        registro.Evento.Should().Be(IndicadorSimNao.Sim);
        registro.PerCons.Should().Be(100.0000m);
        registro.DataIniEmp.Should().Be(new DateOnly(2023, 1, 1));
        registro.DataFinEmp.Should().Be(new DateOnly(2023, 12, 31));
    }

    [Fact]
    public void Definidor_CnpjVazio_DevolveNulo()
    {
        _catalogo.TentarObter("K100".AsSpan(), out var meta);
        var registro = (RegistroK100)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.Cnpj.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Exemplo do manual (p. 215) com 4 casas decimais conforme Decimais=4
        const string sped = "|K100|105|1234|11111111|EMPRESA PARTICIPANTE Z|30,0000|S|100,0000|01012023|31122023|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCnpj_PreservaTextoCanonico()
    {
        // Empresa estrangeira — CNPJ ausente (campo opcional); COD_PAIS sem zero à esquerda
        const string sped = "|K100|76|5678||EMPRESA EXTERIOR Y|45,5000|N|75,2500|01012023|31122023|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
