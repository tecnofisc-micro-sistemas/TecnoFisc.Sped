using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Tests._Sintetico;

namespace TecnoFisc.Sped.Core.Tests.Gerador;

public sealed class TotalizadorBlocosTests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0000Sintetico).Assembly);

    private static readonly Func<string, int, RegistroSped> _fabricaX990 = (bloco, qtd) => bloco switch
    {
        "0" => new Registro0990Sintetico { QtdLin0 = qtd },
        "C" => new RegistroC990Sintetico { QtdLinC = qtd },
        _ => throw new InvalidOperationException($"Bloco sem fechador sintético: {bloco}"),
    };

    private static readonly Func<int, RegistroSped> _fabrica9999 =
        total => new Registro9999Sintetico { QtdLin = total };

    private static List<RegistroSped> Materializar(IEnumerable<RegistroSped> entrada)
        => TotalizadorBlocos.ComFechadores(entrada, _catalogo, _fabricaX990, _fabrica9999).ToList();

    [Fact]
    public void ComFechadores_BlocoUnico_EmiteX990ENoveTresVezes()
    {
        var entrada = new RegistroSped[]
        {
            new Registro0000Sintetico
            {
                CodVer = "006",
                DtIni = new DateOnly(2025, 1, 1),
                DtFin = new DateOnly(2025, 1, 31),
                Nome = "EMPRESA",
                Cnpj = Cnpj.Create("11222333000181"),
            },
        };

        var saida = Materializar(entrada);

        saida.Select(r => r.Codigo).Should().Equal(["0000", "0990", "9999"]);

        var f0 = saida.OfType<Registro0990Sintetico>().Single();
        f0.QtdLin0.Should().Be(2); // 0000 + 0990

        var f9 = saida.OfType<Registro9999Sintetico>().Single();
        f9.QtdLin.Should().Be(3); // 0000 + 0990 + 9999
    }

    [Fact]
    public void ComFechadores_DoisBlocos_EmiteFechadorParaCadaTransicao()
    {
        var entrada = new RegistroSped[]
        {
            new Registro0000Sintetico
            {
                CodVer = "006",
                DtIni = new DateOnly(2025, 1, 1),
                DtFin = new DateOnly(2025, 1, 31),
                Nome = "EMPRESA",
                Cnpj = Cnpj.Create("11222333000181"),
            },
            new RegistroC001Sintetico { IndMov = 0 },
            new RegistroC100Sintetico
            {
                IndOper = "0",
                CodPart = 1,
                VlDoc = 100m,
                Cfop = Cfop.Create("5102"),
            },
        };

        var saida = Materializar(entrada);

        saida.Select(r => r.Codigo).Should().Equal(["0000", "0990", "C001", "C100", "C990", "9999"]);

        saida.OfType<Registro0990Sintetico>().Single().QtdLin0.Should().Be(2); // 0000 + 0990
        saida.OfType<RegistroC990Sintetico>().Single().QtdLinC.Should().Be(3); // C001 + C100 + C990
        saida.OfType<Registro9999Sintetico>().Single().QtdLin.Should().Be(6); // total
    }

    [Fact]
    public void ComFechadores_FechadorEmitidoNaPosicaoCorrenteAntesDaTransicao()
    {
        var entrada = new RegistroSped[]
        {
            new Registro0000Sintetico
            {
                CodVer = "006",
                DtIni = new DateOnly(2025, 1, 1),
                DtFin = new DateOnly(2025, 1, 31),
                Nome = "X",
                Cnpj = Cnpj.Create("11222333000181"),
            },
            new RegistroC001Sintetico { IndMov = 0 },
        };

        var saida = Materializar(entrada);

        // 0990 entra entre 0000 e C001, não depois de C001.
        var indices = saida.Select((r, i) => (r.Codigo, i)).ToList();
        indices.Single(t => t.Codigo == "0990").i.Should().Be(1);
        indices.Single(t => t.Codigo == "C001").i.Should().Be(2);
    }

    [Fact]
    public void ComFechadores_EntradaVazia_EmiteApenas9999()
    {
        var saida = Materializar(Array.Empty<RegistroSped>());

        saida.Should().HaveCount(1);
        saida.Single().Codigo.Should().Be("9999");
        ((Registro9999Sintetico)saida.Single()).QtdLin.Should().Be(1);
    }

    [Fact]
    public void ComFechadores_RegistroDesconhecidoNoCatalogo_LancaInvalidOperationException()
    {
        var entrada = new RegistroSped[] { new RegistroNaoCatalogado() };

        var act = () => Materializar(entrada);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*XYZW*");
    }

    [Fact]
    public void ComFechadores_RegistrosNulo_LancaArgumentNullException()
    {
        var act = () => TotalizadorBlocos.ComFechadores(
            registros: null!,
            _catalogo,
            _fabricaX990,
            _fabrica9999).ToList();

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ComFechadores_FabricaX990Nula_LancaArgumentNullException()
    {
        var act = () => TotalizadorBlocos.ComFechadores(
            Array.Empty<RegistroSped>(),
            _catalogo,
            fabricaX990: null!,
            _fabrica9999).ToList();

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ComFechadores_Fabrica9999Nula_LancaArgumentNullException()
    {
        var act = () => TotalizadorBlocos.ComFechadores(
            Array.Empty<RegistroSped>(),
            _catalogo,
            _fabricaX990,
            fabrica9999: null!).ToList();

        act.Should().Throw<ArgumentNullException>();
    }

    private sealed class RegistroNaoCatalogado : RegistroSped
    {
        public override string Codigo => "XYZW";
    }
}
