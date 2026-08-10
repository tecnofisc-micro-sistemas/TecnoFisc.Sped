using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

/// <summary>Enum numérico fechado, sem [SpedValor] — o caso do achado 1.</summary>
public enum TipoItemSintetico
{
    Mercadoria = 0,
    Servico = 1,
}

/// <summary>
/// Enum com nomes de membro, para provar o parsing por nome do caminho permissivo.
/// Nome não é <c>SituacaoSintetica</c> para não colidir com o enum homônimo já existente
/// em <c>RegistrosSinteticos.cs</c> (valores diferentes: Ativa/Inativa).
/// </summary>
public enum SituacaoItemSintetica
{
    S = 0,
    N = 1,
}

[Flags]
public enum MarcadoresSinteticos
{
    Nenhum = 0,
    Primeiro = 1,
    Segundo = 2,
}

[RegistroSped(Codigo = "A200", Nivel = 2, Bloco = "A")]
public sealed partial class RegistroEnumDominioSintetico : RegistroSped
{
    public override string Codigo => "A200";

    [CampoSped(Ordem = 2, Tamanho = 2, Nome = "TIPO_ITEM")]
    public TipoItemSintetico TipoItem { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 1, Nome = "SITUACAO")]
    public SituacaoItemSintetica Situacao { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 3, Nome = "MARCADORES")]
    public MarcadoresSinteticos Marcadores { get; set; }
}
