using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Catalogo;

public sealed class ConversorEnumDominioTests
{
    private static MetadadosCampo Campo(string nome)
    {
        var catalogo = CatalogoBuilder.BuildFromAssembly(typeof(RegistroEnumDominioSintetico).Assembly);
        catalogo.TentarObter("A200", out var metadados).Should().BeTrue();
        return metadados!.Campos.Single(c => c.Nome == nome);
    }

    [Fact]
    public void SemValidacao_CodigoForaDoDominioEhAceitoComoCast()
    {
        var campo = Campo("TIPO_ITEM");
        var registro = new RegistroEnumDominioSintetico();

        campo.Definidor(registro, "12", validarDominio: false);

        ((int)registro.TipoItem).Should().Be(12);
    }

    [Fact]
    public void ComValidacao_CodigoForaDoDominioLancaFormatException()
    {
        var campo = Campo("TIPO_ITEM");
        var registro = new RegistroEnumDominioSintetico();

        var acao = () => campo.Definidor(registro, "12", validarDominio: true);

        acao.Should().Throw<FormatException>();
    }

    [Fact]
    public void ComValidacao_CodigoDentroDoDominioEhAceito()
    {
        var campo = Campo("TIPO_ITEM");
        var registro = new RegistroEnumDominioSintetico();

        campo.Definidor(registro, "1", validarDominio: true);

        registro.TipoItem.Should().Be(TipoItemSintetico.Servico);
    }

    [Fact]
    public void SemValidacao_NomeDeMembroContinuaSendoAceito()
    {
        var campo = Campo("SITUACAO");
        var registro = new RegistroEnumDominioSintetico();

        campo.Definidor(registro, "N", validarDominio: false);

        registro.Situacao.Should().Be(SituacaoItemSintetica.N);
    }

    [Fact]
    public void EnumFlags_NaoEhValidadoNemComValidacaoLigada()
    {
        var campo = Campo("MARCADORES");
        var registro = new RegistroEnumDominioSintetico();

        campo.Definidor(registro, "3", validarDominio: true);

        registro.Marcadores.Should().Be(MarcadoresSinteticos.Primeiro | MarcadoresSinteticos.Segundo);
    }

    [Fact]
    public void SobrecargaDeDoisArgumentos_EquivaleAoCaminhoPermissivo()
    {
        var campo = Campo("TIPO_ITEM");
        var registro = new RegistroEnumDominioSintetico();

        campo.Definidor(registro, "12");

        ((int)registro.TipoItem).Should().Be(12);
    }
}
