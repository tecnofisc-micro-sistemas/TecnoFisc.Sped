using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco1;

public sealed class Registro1610Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1610).Assembly);

    [Fact]
    public void Atributo_Declara1610_Nivel3_Bloco1()
    {
        var atributo = typeof(Registro1610).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1610");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1610Com10CamposNaOrdem()
    {
        _catalogo.TentarObter("1610".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1610");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11]);
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "Cnpj", "CstCofins", "CodPart", "DtOper",
            "VlOper", "VlBcCofins", "AliqCofins", "VlCofins",
            "CodCta", "DescCompl",
        ]);
        meta.Campos[0].Tamanho.Should().Be(14);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // Cnpj
        meta.Campos[1].Tamanho.Should().Be(2);
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // CstCofins
        meta.Campos[2].Obrigatorio.Should().BeFalse();  // CodPart
        meta.Campos[3].Tamanho.Should().Be(8);
        meta.Campos[3].Obrigatorio.Should().BeTrue();   // DtOper
        meta.Campos[4].Obrigatorio.Should().BeTrue();   // VlOper
        meta.Campos[5].Obrigatorio.Should().BeTrue();   // VlBcCofins
        meta.Campos[6].Obrigatorio.Should().BeTrue();   // AliqCofins
        meta.Campos[7].Obrigatorio.Should().BeTrue();   // VlCofins
        meta.Campos[8].Obrigatorio.Should().BeFalse();  // CodCta
        meta.Campos[9].Obrigatorio.Should().BeFalse();  // DescCompl
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1610".AsSpan(), out var meta);
        var registro = (Registro1610)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "11222333000181".AsSpan());   // Cnpj
        meta.Campos[1].Definidor(registro, "73".AsSpan());               // CstCofins
        meta.Campos[2].Definidor(registro, "PART001".AsSpan());          // CodPart
        meta.Campos[3].Definidor(registro, "01012021".AsSpan());         // DtOper
        meta.Campos[4].Definidor(registro, "10000,00".AsSpan());         // VlOper
        meta.Campos[5].Definidor(registro, "10000,000".AsSpan());        // VlBcCofins
        meta.Campos[6].Definidor(registro, "0,0300".AsSpan());           // AliqCofins
        meta.Campos[7].Definidor(registro, "300,00".AsSpan());           // VlCofins
        meta.Campos[8].Definidor(registro, "1.2.3.01".AsSpan());         // CodCta
        meta.Campos[9].Definidor(registro, "Servico prestado".AsSpan()); // DescCompl

        registro.Cnpj.ToString().Should().Be("11222333000181");
        registro.CstCofins.Should().Be(73);
        registro.CodPart.Should().Be("PART001");
        registro.DtOper.Should().Be(new DateOnly(2021, 1, 1));
        registro.VlOper.Should().Be(10000.00m);
        registro.VlBcCofins.Should().Be(10000.000m);
        registro.AliqCofins.Should().Be(0.0300m);
        registro.VlCofins.Should().Be(300.00m);
        registro.CodCta.Should().Be("1.2.3.01");
        registro.DescCompl.Should().Be("Servico prestado");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1610".AsSpan(), out var meta);
        var registro = (Registro1610)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodPart
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodCta
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);  // DescCompl

        registro.CodPart.Should().BeNull();
        registro.CodCta.Should().BeNull();
        registro.DescCompl.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1610|11222333000181|73|PART001|01012021|10000,00|10000,000|0,0300|300,00|1.2.3.01|Servico prestado|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|1610|11222333000181|73||01012021|10000,00|10000,000|0,0300|300,00|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_AliquotaEmReais_PreservaTextoCanonico()
    {
        const string sped = "|1610|11222333000181|73||15032021|5000,00|1000,000|2,1000|2100,00|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.LerAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.EscreverAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }
}
