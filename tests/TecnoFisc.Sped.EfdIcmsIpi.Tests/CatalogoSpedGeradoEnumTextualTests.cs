using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Generated;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests;

/// <summary>
/// Garante que o <see cref="CatalogoSpedGerado"/> (source generator, Stage 6) honra o atributo
/// <c>[SpedValor("S")]</c>/<c>[SpedValor("N")]</c> nos membros de enum. Sem esse suporte, o
/// parser real explode em campos textuais como <c>Registro1010.IndExp</c>, que aparecem em
/// qualquer arquivo EFD ICMS-IPI emitido pelo PVA da Receita.
/// </summary>
public sealed class CatalogoSpedGeradoEnumTextualTests
{
    private static readonly IRegistroSpedCatalogo _catalogo = new CatalogoSpedGerado();

    [Fact]
    public async Task LeitorSpedTxt_ParseiaIndicadorSimNao_DoLeiauteEfdIcmsIpi()
    {
        var leitor = new LeitorSpedTxt(_catalogo);

        const string sped = "|1010|S|N|S|N|S|N|S|N|S|N|S|N|S|\r\n";
        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));

        Registro1010? lido = null;
        await foreach (var registro in leitor.ReadStreamingAsync(entrada, TestContext.Current.CancellationToken))
            lido = (Registro1010)registro;

        lido.Should().NotBeNull();
        lido!.IndExp.Should().Be(IndicadorSimNao.Sim);
        lido.IndCcrf.Should().Be(IndicadorSimNao.Nao);
        lido.IndComb.Should().Be(IndicadorSimNao.Sim);
        lido.IndUsina.Should().Be(IndicadorSimNao.Nao);
        lido.IndVa.Should().Be(IndicadorSimNao.Sim);
        lido.IndEe.Should().Be(IndicadorSimNao.Nao);
        lido.IndCart.Should().Be(IndicadorSimNao.Sim);
        lido.IndForm.Should().Be(IndicadorSimNao.Nao);
        lido.IndAer.Should().Be(IndicadorSimNao.Sim);
        lido.IndGiaf1.Should().Be(IndicadorSimNao.Nao);
        lido.IndGiaf3.Should().Be(IndicadorSimNao.Sim);
        lido.IndGiaf4.Should().Be(IndicadorSimNao.Nao);
        lido.IndRestRessarcComplIcms.Should().Be(IndicadorSimNao.Sim);
    }

    [Fact]
    public async Task EscritorSpedTxt_SerializaIndicadorSimNao_ComoTexto()
    {
        var escritor = new EscritorSpedTxt(_catalogo);
        var registro = new Registro1010
        {
            IndExp = IndicadorSimNao.Sim,
            IndCcrf = IndicadorSimNao.Nao,
            IndComb = IndicadorSimNao.Sim,
            IndUsina = IndicadorSimNao.Nao,
            IndVa = IndicadorSimNao.Sim,
            IndEe = IndicadorSimNao.Nao,
            IndCart = IndicadorSimNao.Sim,
            IndForm = IndicadorSimNao.Nao,
            IndAer = IndicadorSimNao.Sim,
            IndGiaf1 = IndicadorSimNao.Nao,
            IndGiaf3 = IndicadorSimNao.Sim,
            IndGiaf4 = IndicadorSimNao.Nao,
            IndRestRessarcComplIcms = IndicadorSimNao.Sim,
        };

        using var saida = new MemoryStream();
        await escritor.WriteAsync(saida, new RegistroSped[] { registro }, TestContext.Current.CancellationToken);

        EncodingSped.Latin1.GetString(saida.ToArray())
            .Should().Be("|1010|S|N|S|N|S|N|S|N|S|N|S|N|S|\r\n");
    }

    [Fact]
    public async Task LeitorSpedTxt_ValorDesconhecidoEmEnumTextual_LancaErroFormato()
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        const string sped = "|1010|X|N|S|N|S|N|S|N|S|N|S|N|S|\r\n";
        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));

        var act = async () =>
        {
            await foreach (var _ in leitor.ReadStreamingAsync(entrada, TestContext.Current.CancellationToken)) { }
        };

        await act.Should().ThrowAsync<TecnoFisc.Sped.Core.Erros.ErroFormatoSpedException>();
    }
}
