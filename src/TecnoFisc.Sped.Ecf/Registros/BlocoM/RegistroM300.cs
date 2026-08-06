using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoM;

/// <summary>Registro M300 - lançamentos da Parte A do e-Lalur.</summary>
[RegistroSped(Codigo = "M300", Nivel = 3, Bloco = "M")]
public sealed partial class RegistroM300 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "M300";

    /// <summary>Código do Lançamento no e-Lalur, conforme tabela dinâmica do Sped (Disponibilizada no item III deste registro e no programa da ECF no diretório Arquivos de Programas/Programas Sped/ECf/SpedEcf/Recursos/Tabelas).</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true, Nome = "CODIGO")]
    public string? CampoCodigo { get; set; }

    /// <summary>Descrição do Tipo de Lançamento no e-Lalur, conforme tabela dinâmica do Sped (Disponibilizada no item III deste registro e no programa da ECF no diretório Arquivos de Programas/Programas Sped/ECf/SpedEcf/Recursos/Tabelas).</summary>
    [CampoSped(Ordem = 3)]
    public string? Descricao { get; set; }

    /// <summary>Indicador do Tipo de Lançamento: A- Adição E - Exclusão. P - Compensação de Prejuízo L - Lucro Observação:O tipo “R” (Rótulo) é para linhas de títulos, que aparecem na interface do programa. Portanto, não deve ser utilizado neste campo.</summary>
    [CampoSped(Ordem = 4, Tamanho = 1)]
    public TipoLancamentoParteA? TipoLancamento { get; set; }

    /// <summary>Indicador de Relacionamento do Lançamento da Parte A: 1 - Com Conta da Parte B 2 - Com Conta Contábil 3 – Com Conta da parte B e Conta Contábil 4 - Sem Relacionamento Observação:O valor do lançamento do tipo 3 pode considerar o saldo contas da parte B ou somatório dos saldos das contas da parte B com os saldos das contas contábeis. Para isso, o valor do lançamento correto na parte A deve ser preenchido pela empresa.</summary>
    [CampoSped(Ordem = 5, Tamanho = 1)]
    public IndicadorRelacionamentoParteA? IndRelacao { get; set; }

    /// <summary>Valor do Lançamento no e-Lalur</summary>
    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 2)]
    public decimal? Valor { get; set; }

    /// <summary>Histórico do Lançamento no e-Lalur</summary>
    [CampoSped(Ordem = 7, Tamanho = 500)]
    public string? HistLanLal { get; set; }
}
