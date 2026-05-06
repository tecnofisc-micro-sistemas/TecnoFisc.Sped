using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoM;

public sealed class RegistroM105Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroM105).Assembly);

    [Fact]
    public void Atributo_DeclaraM105_Nivel3_BlocoM()
    {
        var atributo = typeof(RegistroM105).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("M105");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("M");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroM105Com9CamposNaOrdem()
    {
        _catalogo.TentarObter("M105".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("M105");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "NatBcCred", "CstPis", "VlBcPisTot", "VlBcPisCum", "VlBcPisNc",
            "VlBcPis", "QuantBcPisTot", "QuantBcPis", "DescCred",
        ]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // NatBcCred
        meta.Campos[1].Tamanho.Should().Be(2);
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // CstPis
        meta.Campos[8].Tamanho.Should().Be(60);
        meta.Campos[8].Obrigatorio.Should().BeFalse();  // DescCred
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("M105".AsSpan(), out var meta);
        var registro = (RegistroM105)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "02".AsSpan());           // NatBcCred
        meta.Campos[1].Definidor(registro, "56".AsSpan());           // CstPis
        meta.Campos[2].Definidor(registro, "1000000,00".AsSpan());   // VlBcPisTot
        meta.Campos[3].Definidor(registro, "200000,00".AsSpan());    // VlBcPisCum
        meta.Campos[4].Definidor(registro, "800000,00".AsSpan());    // VlBcPisNc
        meta.Campos[5].Definidor(registro, "500000,00".AsSpan());    // VlBcPis
        meta.Campos[6].Definidor(registro, "".AsSpan());             // QuantBcPisTot
        meta.Campos[7].Definidor(registro, "".AsSpan());             // QuantBcPis
        meta.Campos[8].Definidor(registro, "Aquisição de insumo".AsSpan()); // DescCred

        registro.NatBcCred.Should().Be(CodigoBaseCalculoCredito.AquisicaoBensInsumo);
        registro.CstPis.Should().Be(56);
        registro.VlBcPisTot.Should().Be(1000000m);
        registro.VlBcPisCum.Should().Be(200000m);
        registro.VlBcPisNc.Should().Be(800000m);
        registro.VlBcPis.Should().Be(500000m);
        registro.QuantBcPisTot.Should().BeNull();
        registro.QuantBcPis.Should().BeNull();
        registro.DescCred.Should().Be("Aquisição de insumo");
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("M105".AsSpan(), out var meta);
        var registro = (RegistroM105)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcPisTot
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcPisCum
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcPisNc
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcPis
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty); // QuantBcPisTot
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty); // QuantBcPis
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty); // DescCred

        registro.VlBcPisTot.Should().BeNull();
        registro.VlBcPisCum.Should().BeNull();
        registro.VlBcPisNc.Should().BeNull();
        registro.VlBcPis.Should().BeNull();
        registro.QuantBcPisTot.Should().BeNull();
        registro.QuantBcPis.Should().BeNull();
        registro.DescCred.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Exemplo do PDF Guia v1.35 p. 302 — Registro M105 filho de M100 COD_CRED=101
        const string sped =
            "|M105|02|56|1000000,00|200000,00|800000,00|500000,00||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComDescricaoCreditoERegimeNaoCumulativo_PreservaTextoCanonico()
    {
        // NAT_BC_CRED=13 (Outras Operações) — DescCred obrigatório neste caso (validação semântica)
        const string sped =
            "|M105|13|50|||800000,00|800000,00|||Outras operações com direito a crédito|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComQuantidadeBaseDaCalculoSemValor_PreservaTextoCanonico()
    {
        // Crédito por unidade de medida (combustíveis/bebidas frias) — campos QUANT preenchidos, VL vazios
        const string sped =
            "|M105|09|50|||||350000,000|350000,000||\r\n";

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
