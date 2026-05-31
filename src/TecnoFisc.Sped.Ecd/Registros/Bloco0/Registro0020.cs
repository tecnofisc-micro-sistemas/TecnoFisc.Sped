using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.Bloco0;

/// <summary>
/// Registro 0020 — Escrituração Contábil Descentralizada. Nível hierárquico 2, ocorrência 0:N.
/// Preenchido somente quando a pessoa jurídica utiliza escrituração descentralizada
/// (<c>0000.IND_CENTRALIZADA = "1"</c>). Conforme Manual de Orientação do Leiaute 9 da ECD, p. 78.
/// </summary>
/// <remarks>
/// <para>
/// Quando o arquivo se refere à escrituração da <strong>matriz</strong>
/// (<see cref="IndDec"/> = <see cref="IndicadorDescentralizacao.EscrituradorMatriz"/>),
/// os campos 03 a 08 contêm os dados de cada filial — haverá um registro 0020 por filial.
/// Quando se refere à escrituração da <strong>filial</strong>
/// (<see cref="IndDec"/> = <see cref="IndicadorDescentralizacao.EscrituradorFilial"/>),
/// deve existir um único registro 0020 com os dados da matriz.
/// </para>
/// <para>
/// Regras de validação (<c>REGRA_OCORRENCIA_0020_ARQ</c>, <c>REGRA_CONGLOMERADO_NA_MATRIZ</c>,
/// <c>REGRA_VALIDA_CNPJ</c>, <c>REGRA_VERIFICA_CNPJ_REG_0000_REG_0020</c>,
/// <c>REGRA_REGISTRO_DUPLICADO</c>, <c>REGRA_DUPLICIDADE_CNPJ_REG_0000_REG_0020</c>,
/// <c>REGRA_TABELA_UF</c>, <c>REGRA_CAMPO_CARACTERE_INVALIDO</c>,
/// <c>REGRA_TABELA_MUNICIPIO</c>, <c>REGRA_VALIDA_NIRE</c>) —
/// verificação de responsabilidade do consumidor (ARCHITECTURE §2.3).
/// </para>
/// </remarks>
[RegistroSped(Codigo = "0020", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0020 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0020";

    /// <summary>
    /// Indicador de descentralização: 0 = escrituração da matriz; 1 = escrituração da filial.
    /// Campo <c>IND_DEC</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorDescentralizacao IndDec { get; set; }

    /// <summary>
    /// CNPJ da filial (quando <see cref="IndDec"/> = 0) ou da matriz (quando
    /// <see cref="IndDec"/> = 1). Campo <c>CNPJ</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 14, Obrigatorio = true)]
    public Cnpj? Cnpj { get; set; }

    /// <summary>Sigla da unidade da federação da matriz ou da filial.</summary>
    [CampoSped(Ordem = 4, Tamanho = 2, Obrigatorio = true)]
    public string? Uf { get; set; }

    /// <summary>Inscrição Estadual da matriz ou da filial — preenchida quando houver.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0)]
    public string? Ie { get; set; }

    /// <summary>
    /// Código do município do domicílio da matriz ou da filial conforme tabela IBGE (7 dígitos).
    /// Campo <c>COD_MUN</c>.
    /// </summary>
    [CampoSped(Ordem = 6, Tamanho = 7)]
    public string? CodMun { get; set; }

    /// <summary>Inscrição Municipal da matriz ou da filial — preenchida quando houver.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0)]
    public string? Im { get; set; }

    /// <summary>
    /// Número de Identificação do Registro de Empresas da matriz ou da filial na Junta Comercial
    /// (até 11 dígitos). Campo <c>NIRE</c>.
    /// </summary>
    [CampoSped(Ordem = 8, Tamanho = 11)]
    public long? Nire { get; set; }
}
