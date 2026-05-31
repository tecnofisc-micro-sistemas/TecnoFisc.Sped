using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoJ;

/// <summary>
/// Registro J005 — Demonstrações Contábeis. Nível hierárquico 2, ocorrência 0:N.
/// Identifica o período e a natureza de cada conjunto de demonstrações contábeis
/// (Balanço Patrimonial — J100; DRE — J150; DLPA/DMPL — J210) informadas na ECD.
/// Campo(s) chave: [DT_INI]+[DT_FIN]+[ID_DEM].
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 172–174.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_OBRIGATORIA_DEMONSTRACAO</c>: se J005 existe, devem existir J100 e J150 filhos.</item>
///   <item><c>REGRA_REGISTRO_OBRIGATORIO_J005_FIM_EXERCICIO</c>: para IND_ESC G, R ou B, deve haver
///   um J005 cuja DT_FIN coincida com a data de encerramento do exercício (DT-EX-SOCIAL do I030),
///   acompanhado de J100 e J150.</item>
///   <item><c>REGRA_PERIODO_SUP_UM_ANO</c>: período DT_INI–DT_FIN superior a um ano gera aviso.</item>
///   <item><c>REGRA_ENC_OBRIGATORIO</c>: para ID_DEM = 1 e J005 com filhos J100/J210, deve existir
///   I350 com DT_RES igual a DT_FIN (Campo 03).</item>
///   <item><c>REGRA_CAB_DEM_OBRIGATORIO</c>: CAB_DEM obrigatório quando ID_DEM = 2.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "J005", Nivel = 2, Bloco = "J")]
public sealed partial class RegistroJ005 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "J005";

    /// <summary>Data inicial das demonstrações contábeis (ddMMyyyy).</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    /// <summary>Data final das demonstrações contábeis (ddMMyyyy).</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFin { get; set; }

    /// <summary>Identificação das demonstrações: 1 = empresário/sociedade; 2 = consolidadas/outras PJ.</summary>
    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true)]
    public IdentificacaoDemonstracao IdDem { get; set; }

    /// <summary>
    /// Cabeçalho das demonstrações (texto livre, máx. 65535 caracteres).
    /// Obrigatório quando <see cref="IdDem"/> = <see cref="IdentificacaoDemonstracao.Consolidado"/>.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 65535)]
    public string? CabDem { get; set; }
}
