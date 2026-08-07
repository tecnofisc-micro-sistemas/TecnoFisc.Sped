using static TecnoFisc.Sped.Txt.Engine.Tests.SourceGenerators.GeneratorHarness;

namespace TecnoFisc.Sped.Txt.Engine.Tests.SourceGenerators;

public sealed class RegistroSpedCatalogoGeneratorDominioEnumTests
{
    private const string Fonte = """
        using TecnoFisc.Sped.Txt.Engine.Abstracoes;
        using TecnoFisc.Sped.Txt.Engine.Atributos;

        namespace Exemplo;

        public enum TipoItemGerado { Mercadoria = 0, Servico = 1 }

        [System.Flags]
        public enum MarcadoresGerados { Nenhum = 0, Primeiro = 1, Segundo = 2 }

        public enum SituacaoGerada
        {
            [SpedValor("N")]
            Nao,

            [SpedValor("S")]
            Sim,
        }

        [RegistroSped(Codigo = "B100", Nivel = 2, Bloco = "B")]
        public sealed partial class RegistroB100 : RegistroSped
        {
            public override string Codigo => "B100";

            [CampoSped(Ordem = 2, Tamanho = 2, Nome = "TIPO_ITEM")]
            public TipoItemGerado TipoItem { get; set; }

            [CampoSped(Ordem = 3, Tamanho = 3, Nome = "MARCADORES")]
            public MarcadoresGerados Marcadores { get; set; }

            [CampoSped(Ordem = 4, Tamanho = 1, Nome = "SITUACAO")]
            public SituacaoGerada Situacao { get; set; }
        }
        """;

    // NOTA: o gerador nomeia os helpers a partir de InfoRegistro.Codigo (o código SPED do
    // registro, ex. "B100"), não do nome da classe CLR ("RegistroB100") — ver reg.Codigo em
    // EmitirHelperSet/EmitirCampo. "Set_B100_TipoItem*" é o nome real emitido para a fonte acima.

    [Fact]
    public void EnumFechado_GeraSetterPermissivoESetterEstrito()
    {
        string gerado = RunGenerator(Fonte);

        gerado.Should().Contain("Set_B100_TipoItem_Estrito");
        gerado.Should().Contain("Enum.IsDefined(convertido)");
    }

    [Fact]
    public void SetterPermissivo_NaoValidaODominio()
    {
        string gerado = RunGenerator(Fonte);

        int inicio = gerado.IndexOf("Set_B100_TipoItem(", StringComparison.Ordinal);
        int fim = gerado.IndexOf("Set_B100_TipoItem_Estrito(", StringComparison.Ordinal);
        inicio.Should().BeGreaterThan(-1);
        fim.Should().BeGreaterThan(inicio);
        gerado[inicio..fim].Should().NotContain("Enum.IsDefined");
    }

    [Fact]
    public void MetadadosCampo_RecebeODefinidorEstritoComoUltimoArgumento()
    {
        string gerado = RunGenerator(Fonte);

        gerado.Should().Contain("Set_B100_TipoItem_Estrito)");
    }

    /// <summary>
    /// Ponto exato do escopo extra desta task: <c>RequiresStrictSetter</c> exclui <c>[Flags]</c>
    /// porque a combinação de bits não tem domínio fechado a validar, e o caminho estrito, ao
    /// pular <c>Enum.Parse</c>, perderia o parsing por nome/forma separada por vírgula do
    /// permissivo. Sem este teste, remover o <c>!c.EnumFlags</c> do predicado compila e não
    /// quebra nenhuma suíte — só diverge do lado reflexivo (<c>CatalogoBuilder.RequiresStrictSetter</c>)
    /// em tempo de leitura real.
    /// </summary>
    [Fact]
    public void EnumFlags_NaoGeraSetterEstrito()
    {
        string gerado = RunGenerator(Fonte);

        gerado.Should().NotContain("Set_B100_Marcadores_Estrito");
    }

    /// <summary>
    /// Enum com <c>[SpedValor]</c> textual: o parsing já é por switch de token conhecido — token
    /// desconhecido já lança <c>FormatException</c> no próprio setter permissivo textual, então
    /// não há setter estrito irmão a construir nem a referenciar em <c>MetadadosCampo</c>.
    /// </summary>
    [Fact]
    public void EnumComSpedValor_NaoGeraSetterEstrito()
    {
        string gerado = RunGenerator(Fonte);

        gerado.Should().NotContain("Set_B100_Situacao_Estrito");
    }
}
