using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoM;

public sealed class RegistroM300Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroM300).Assembly);

    [Fact]
    public void Atributo_DeclaraM300_Nivel2_BlocoM()
    {
        var atributo = typeof(RegistroM300).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("M300");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("M");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroM300Com7CamposNaOrdem()
    {
        _catalogo.TentarObter("M300".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("M300");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodCont", "VlContApurDifer", "NatCredDesc", "VlCredDescDifer",
            "VlContDiferAnt", "PerApur", "DtReceb",
        ]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // CodCont
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // VlContApurDifer
        meta.Campos[2].Obrigatorio.Should().BeFalse();  // NatCredDesc
        meta.Campos[3].Obrigatorio.Should().BeFalse();  // VlCredDescDifer
        meta.Campos[4].Obrigatorio.Should().BeTrue();   // VlContDiferAnt
        meta.Campos[5].Tamanho.Should().Be(6);
        meta.Campos[5].Obrigatorio.Should().BeTrue();   // PerApur
        meta.Campos[6].Tamanho.Should().Be(8);
        meta.Campos[6].Obrigatorio.Should().BeFalse();  // DtReceb
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("M300".AsSpan(), out var meta);
        var registro = (RegistroM300)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "51".AsSpan());        // CodCont
        meta.Campos[1].Definidor(registro, "5000,00".AsSpan());   // VlContApurDifer
        meta.Campos[2].Definidor(registro, "01".AsSpan());        // NatCredDesc
        meta.Campos[3].Definidor(registro, "2000,00".AsSpan());   // VlCredDescDifer
        meta.Campos[4].Definidor(registro, "3000,00".AsSpan());   // VlContDiferAnt
        meta.Campos[5].Definidor(registro, "012024".AsSpan());    // PerApur
        meta.Campos[6].Definidor(registro, "15012024".AsSpan());  // DtReceb

        registro.CodCont.Should().Be(CodigoContribuicaoSocialApurada.CumulativaAliquotaBasica);
        registro.VlContApurDifer.Should().Be(5000m);
        registro.NatCredDesc.Should().Be(NaturezaCreditoDiferido.CreditoAliquotaBasica);  // 01 → valor 1
        registro.VlCredDescDifer.Should().Be(2000m);
        registro.VlContDiferAnt.Should().Be(3000m);
        registro.PerApur.Should().Be("012024");
        registro.DtReceb.Should().Be(new DateOnly(2024, 1, 15));
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("M300".AsSpan(), out var meta);
        var registro = (RegistroM300)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // NatCredDesc
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCredDescDifer
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty); // DtReceb

        registro.NatCredDesc.Should().BeNull();
        registro.VlCredDescDifer.Should().BeNull();
        registro.DtReceb.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|M300|51|5000,00|01|2000,00|3000,00|012024|15012024|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|M300|51|3000,00|||3000,00|012024||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("01", NaturezaCreditoDiferido.CreditoAliquotaBasica)]
    [InlineData("02", NaturezaCreditoDiferido.CreditoAliquotaDiferenciada)]
    [InlineData("03", NaturezaCreditoDiferido.CreditoAliquotaUnidadeProduto)]
    [InlineData("04", NaturezaCreditoDiferido.CreditoPresumidoAgroindustria)]
    public void NaturezaCreditoDiferido_Roundtrip_MapeiaCodigo(string codigo, NaturezaCreditoDiferido esperado)
    {
        _catalogo.TentarObter("M300".AsSpan(), out var meta);
        var registro = (RegistroM300)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, codigo.AsSpan());

        registro.NatCredDesc.Should().Be(esperado);
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
