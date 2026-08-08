using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Tests._Sintetico;

/// <summary>
/// Registro sintético usado só para provar, via <see cref="TecnoFisc.Sped.Ecf.Parser.ParserEcf"/>,
/// que a política de domínio de enum é aplicada de ponta a ponta ao ler uma linha real.
/// </summary>
/// <remarks>
/// Nenhum registro real do leiaute ECF hoje tem campo de enum numérico fechado sem
/// <c>[SpedValor]</c> e sem <c>[Flags]</c> — todos os enums de <c>TecnoFisc.Sped.Ecf.Enums</c> são
/// indicadores textuais com <c>[SpedValor]</c> (ex.: <c>IndicadorTributoContaParteB</c>,
/// <c>IndicadorDebitoCredito</c> do próprio <c>RegistroM500</c>), então nenhum campo do catálogo
/// gerado da ECF passa pelo definidor estrito (ver <c>CatalogoBuilder.RequiresStrictSetter</c>).
/// Por isso o teste monta seu próprio catálogo reflexivo a partir deste registro fictício —
/// via <see cref="TecnoFisc.Sped.Txt.Engine.Catalogo.CatalogoBuilder.BuildFromAssembly"/> e o
/// injeta em <see cref="TecnoFisc.Sped.Ecf.Parser.ParserEcf(IRegistroSpedCatalogo)"/> — em vez de
/// usar o catálogo gerado padrão da ECF. O que está sob teste é a resolução de opções e a
/// escolha de definidor do <c>ParserEcf</c>/<c>LeitorSpedTxt</c>, não um registro específico do
/// leiaute.
/// </remarks>
[RegistroSped(Codigo = "A200", Nivel = 1, Bloco = "A")]
public sealed class RegistroEnumDominioSinteticoEcf : RegistroSped
{
    public override string Codigo => "A200";

    [CampoSped(Ordem = 2, Tamanho = 2, Nome = "TIPO_ITEM")]
    public TipoItemSinteticoEcf TipoItem { get; set; }
}

/// <summary>Enum numérico fechado, sem <c>[SpedValor]</c> — não existe hoje em nenhum campo real da ECF.</summary>
public enum TipoItemSinteticoEcf
{
    Mercadoria = 0,
    Servico = 1,
}
