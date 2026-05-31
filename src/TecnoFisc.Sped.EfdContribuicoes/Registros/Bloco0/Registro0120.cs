using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco0;

/// <summary>
/// Registro 0120 — Identificação de EFD-Contribuições Sem Dados a Escriturar. Nível
/// hierárquico 2, ocorrência Vários. Obrigatório em dezembro e no mês de encerramento
/// de atividades; também obrigatório a partir de 01/08/2017 quando a escrituração não
/// contiver dados de receitas ou créditos nos Blocos A, C, D, F ou I.
/// Conforme Guia Prático v1.35, p. 75.
/// </summary>
[RegistroSped(Codigo = "0120", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0120 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0120";

    /// <summary>
    /// Mês de referência do ano-calendário da escrituração sem dados, no formato "mmaaaa"
    /// (ex.: "012025" para janeiro de 2025).
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true)]
    public string? MesRefer { get; set; }

    /// <summary>
    /// Informação complementar. Indicador de 2 caracteres com o real motivo da escrituração
    /// sem dados: 01=PJ imune/isenta do IRPJ, 02=Órgãos públicos/autarquias/fundações,
    /// 03=PJ inativa, 04=PJ sem operações geradoras de receitas/créditos, 05=SCP sem
    /// operações, 06=Soc. Cooperativa sem operações, 07=Escrituração por
    /// incorporação/fusão/cisão sem operações, 99=Demais hipóteses (IN RFB nº 1.252/2012).
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 90, Obrigatorio = true)]
    public string? InfComp { get; set; }
}
