using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoK;

/// <summary>
/// Registro K100 — Relação das Empresas Consolidadas. Nível hierárquico 3, ocorrência 0:N
/// por arquivo. Identifica as empresas que fazem parte da escrituração contábil consolidada.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 212–215.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_REGISTRO_OBRIGATORIO_K110</c>: quando <see cref="Evento"/> for
///   <see cref="IndicadorSimNao.Sim"/>, deve existir pelo menos um registro K110 filho.</item>
///   <item><c>REGRA_OBRIGATORIO_K100_CNPJ_0000</c>: deve existir um K100 cujo <see cref="Cnpj"/>
///   (Campo 04) seja igual ao CNPJ (Campo 06) do Registro 0000.</item>
///   <item><c>REGRA_TABELA_PAISES</c>: <see cref="CodPais"/> deve constar na tabela PAIS_SISCOMEX
///   do Banco Central do Brasil.</item>
///   <item><c>REGRA_OBRIGATORIO_CNPJ_BRASIL</c>: quando <see cref="Cnpj"/> estiver preenchido,
///   <see cref="CodPais"/> deve corresponder ao código do Brasil.</item>
///   <item><c>REGRA_PERC_MENOR_IGUAL_100</c>: <see cref="PerPart"/> e <see cref="PerCons"/>
///   devem ser menores ou iguais a 100.</item>
///   <item><c>REGRA_MAIOR_IGUAL_ZERO</c>: <see cref="PerPart"/> e <see cref="PerCons"/>
///   devem ser maiores ou iguais a zero.</item>
///   <item><c>REGRA_CONSOLIDADA_INICIO_DIFERENTE</c>: <see cref="DataIniEmp"/> deve ser
///   igual à data inicial do período consolidado (DT_INI do K030).</item>
///   <item><c>REGRA_ANO_IGUAL_ANTERIOR_K030</c>: o ano de <see cref="DataIniEmp"/> deve ser
///   igual ao ano de DT_INI do K030 ou ao ano anterior; idem para <see cref="DataFinEmp"/>
///   em relação a DT_FIN do K030.</item>
///   <item><c>REGRA_DATA_FIN_MAIOR_IGUAL</c>: <see cref="DataFinEmp"/> deve ser maior ou
///   igual a <see cref="DataIniEmp"/>.</item>
///   <item><c>REGRA_CONSOLIDADA_FINAL_DIFERENTE</c>: <see cref="DataFinEmp"/> deve ser igual
///   à data final do período consolidado (DT_FIN do K030).</item>
///   <item><c>REGRA_PERIODO_CONS</c>: a diferença entre <see cref="DataFinEmp"/> e
///   <see cref="DataIniEmp"/> deve ser menor ou igual a 1 (um) ano.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "K100", Nivel = 3, Bloco = "K")]
public sealed partial class RegistroK100 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K100";

    /// <summary>
    /// Código do país da empresa conforme tabela PAIS_SISCOMEX do Banco Central do Brasil.
    /// Campo <c>COD_PAIS</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 5, Obrigatorio = true)]
    public int CodPais { get; set; }

    /// <summary>
    /// Código de identificação da empresa participante.
    /// Campo <c>EMP_COD</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 4, Obrigatorio = true)]
    public int EmpCod { get; set; }

    /// <summary>
    /// CNPJ da empresa — somente os 8 primeiros dígitos (base do CNPJ, sem filial e dígitos
    /// verificadores). Obrigatório apenas quando <c>COD_PAIS</c> corresponder ao Brasil.
    /// Campo <c>CNPJ</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 8)]
    public string? Cnpj { get; set; }

    /// <summary>
    /// Nome empresarial.
    /// Campo <c>NOME</c>.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Obrigatorio = true)]
    public string? Nome { get; set; }

    /// <summary>
    /// Percentual de participação total do conglomerado na empresa no final do período
    /// consolidado. Deve ser preenchido com o percentual de participação acionária da empresa
    /// titular da ECD.
    /// Campo <c>PER_PART</c>.
    /// </summary>
    [CampoSped(Ordem = 6, Tamanho = 8, Decimais = 4, Obrigatorio = true)]
    public decimal PerPart { get; set; }

    /// <summary>
    /// Evento societário ocorrido no período: S = Sim; N = Não.
    /// Campo <c>EVENTO</c>.
    /// </summary>
    [CampoSped(Ordem = 7, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao Evento { get; set; }

    /// <summary>
    /// Percentual de consolidação da empresa no final do período consolidado. Informar o
    /// percentual do resultado da empresa que foi para a consolidação.
    /// Campo <c>PER_CONS</c>.
    /// </summary>
    [CampoSped(Ordem = 8, Tamanho = 8, Decimais = 4, Obrigatorio = true)]
    public decimal PerCons { get; set; }

    /// <summary>
    /// Data inicial do período da escrituração contábil da empresa que foi consolidada (ddMMyyyy).
    /// Campo <c>DATA_INI_EMP</c>.
    /// </summary>
    [CampoSped(Ordem = 9, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DataIniEmp { get; set; }

    /// <summary>
    /// Data final do período da escrituração contábil da empresa que foi consolidada (ddMMyyyy).
    /// Campo <c>DATA_FIN_EMP</c>.
    /// </summary>
    [CampoSped(Ordem = 10, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DataFinEmp { get; set; }
}
