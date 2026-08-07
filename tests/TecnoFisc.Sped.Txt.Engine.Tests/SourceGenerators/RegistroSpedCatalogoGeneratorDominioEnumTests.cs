using static TecnoFisc.Sped.Txt.Engine.Tests.SourceGenerators.GeradorHarness;

namespace TecnoFisc.Sped.Txt.Engine.Tests.SourceGenerators;

public sealed class RegistroSpedCatalogoGeneratorDominioEnumTests
{
    private const string Fonte = """
        using TecnoFisc.Sped.Txt.Engine.Abstracoes;
        using TecnoFisc.Sped.Txt.Engine.Atributos;

        namespace Exemplo;

        public enum TipoItemGerado { Mercadoria = 0, Servico = 1 }

        [RegistroSped(Codigo = "B100", Nivel = 2, Bloco = "B")]
        public sealed partial class RegistroB100 : RegistroSped
        {
            public override string Codigo => "B100";

            [CampoSped(Ordem = 2, Tamanho = 2, Nome = "TIPO_ITEM")]
            public TipoItemGerado TipoItem { get; set; }
        }
        """;

    // NOTA: o gerador nomeia os helpers a partir de InfoRegistro.Codigo (o código SPED do
    // registro, ex. "B100"), não do nome da classe CLR ("RegistroB100") — ver reg.Codigo em
    // EmitirHelperSet/EmitirCampo. "Set_B100_TipoItem*" é o nome real emitido para a fonte acima.

    [Fact]
    public void EnumFechado_GeraSetterPermissivoESetterEstrito()
    {
        string gerado = ExecutarGerador(Fonte);

        gerado.Should().Contain("Set_B100_TipoItem_Estrito");
        gerado.Should().Contain("Enum.IsDefined(convertido)");
    }

    [Fact]
    public void SetterPermissivo_NaoValidaODominio()
    {
        string gerado = ExecutarGerador(Fonte);

        int inicio = gerado.IndexOf("Set_B100_TipoItem(", StringComparison.Ordinal);
        int fim = gerado.IndexOf("Set_B100_TipoItem_Estrito(", StringComparison.Ordinal);
        inicio.Should().BeGreaterThan(-1);
        fim.Should().BeGreaterThan(inicio);
        gerado[inicio..fim].Should().NotContain("Enum.IsDefined");
    }

    [Fact]
    public void MetadadosCampo_RecebeODefinidorEstritoComoUltimoArgumento()
    {
        string gerado = ExecutarGerador(Fonte);

        gerado.Should().Contain("Set_B100_TipoItem_Estrito)");
    }
}
