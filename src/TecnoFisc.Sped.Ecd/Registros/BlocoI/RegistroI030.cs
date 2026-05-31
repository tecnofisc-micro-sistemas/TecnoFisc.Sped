using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoI;

/// <summary>
/// Registro I030 — Termo de Abertura do Livro. Nível hierárquico 3, ocorrência 1:1 por arquivo.
/// Identifica os dados do termo de abertura do livro correspondente ao arquivo.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 113–117.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_OCORRENCIA_UNITARIA_ARQ</c>: registro ocorre exatamente uma vez por arquivo.</item>
///   <item><c>REGRA_MAIOR_QUE_ZERO</c>: <see cref="NumOrd"/> deve ser maior que zero.</item>
///   <item><c>REGRA_IGUAL_QTD_LIN_REG9999</c>: <see cref="QtdLin"/> deve igualar o campo homônimo do Registro 9999.</item>
///   <item><c>REGRA_VALIDA_IDENT_MF_LIVRO_RAS</c> e <c>REGRA_VALIDA_CONTEUDO_NAT_LIVR</c>: validações de <see cref="NatLivr"/>.</item>
///   <item><c>REGRA_IGUAL_NOME_REG0000</c>: <see cref="Nome"/> deve igualar o campo <c>NOME</c> do Registro 0000.</item>
///   <item><c>REGRA_VALIDA_NIRE</c>, <c>REGRA_NIRE_UF</c>, <c>REGRA_CAMPO_OBRIGATORIO_NIRE</c>: validações do campo <see cref="Nire"/>.</item>
///   <item><c>REGRA_IGUAL_CNPJ_REG0000</c>: <see cref="Cnpj"/> deve igualar o CNPJ do Registro 0000.</item>
///   <item><c>REGRA_DATA_INI_MAIOR_ADV</c>: <see cref="DtArq"/> deve ser ≤ DT_FIN do Registro 0000.</item>
///   <item><c>REGRA_DATA_INI_MAIOR</c> e <c>REGRA_PREENCHE_DATA_I030</c>: validações cruzadas de <see cref="DtArqConv"/>.</item>
///   <item><c>REGRA_OBRIGATORIA_DT_EX_SOCIAL</c>: <see cref="DtExSocial"/> é obrigatória.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "I030", Nivel = 3, Bloco = "I")]
public sealed partial class RegistroI030 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "I030";

    /// <summary>
    /// Texto fixo contendo <c>"TERMO DE ABERTURA"</c> (17 caracteres).
    /// Campo <c>DNRC_ABERT</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 17, Obrigatorio = true)]
    public string? DnrcAbert { get; set; } = "TERMO DE ABERTURA";

    /// <summary>
    /// Número de ordem do instrumento de escrituração. Sequencial por tipo de livro,
    /// independentemente de sua forma (papel, fichas ou digital).
    /// Campo <c>NUM_ORD</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public int NumOrd { get; set; }

    /// <summary>
    /// Natureza do livro; finalidade a que se destina o instrumento (até 80 caracteres).
    /// Normalmente os livros G e R recebem o mesmo nome, sendo os mais comuns "Diário" e "Diário Geral".
    /// Campo <c>NAT_LIVR</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 80, Obrigatorio = true)]
    public string? NatLivr { get; set; }

    /// <summary>
    /// Quantidade total de linhas do arquivo digital.
    /// Campo <c>QTD_LIN</c>.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Obrigatorio = true)]
    public int QtdLin { get; set; }

    /// <summary>
    /// Nome empresarial, exatamente como nos atos constitutivos da empresa.
    /// Campo <c>NOME</c>.
    /// </summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Obrigatorio = true)]
    public string? Nome { get; set; }

    /// <summary>
    /// Número de Identificação do Registro de Empresas da Junta Comercial — NIRE (11 dígitos).
    /// Não preenchido quando a empresa não possui registro na Junta Comercial.
    /// Campo <c>NIRE</c>.
    /// </summary>
    [CampoSped(Ordem = 7, Tamanho = 11)]
    public string? Nire { get; set; }

    /// <summary>
    /// Número de inscrição no CNPJ (14 dígitos).
    /// Campo <c>CNPJ</c>.
    /// </summary>
    [CampoSped(Ordem = 8, Tamanho = 14, Obrigatorio = true)]
    public Cnpj? Cnpj { get; set; }

    /// <summary>
    /// Data do arquivamento dos atos constitutivos (ddMMyyyy). Para empresas sem NIRE,
    /// colocar a data de abertura da empresa.
    /// Campo <c>DT_ARQ</c>.
    /// </summary>
    [CampoSped(Ordem = 9, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtArq { get; set; }

    /// <summary>
    /// Data de arquivamento do ato de conversão de sociedade simples em sociedade empresária
    /// (ddMMyyyy). Corresponde à data do NIRE.
    /// Campo <c>DT_ARQ_CONV</c>.
    /// </summary>
    [CampoSped(Ordem = 10, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtArqConv { get; set; }

    /// <summary>
    /// Município do domicílio da empresa.
    /// Campo <c>DESC_MUN</c>.
    /// </summary>
    [CampoSped(Ordem = 11, Tamanho = 0)]
    public string? DescMun { get; set; }

    /// <summary>
    /// Data de encerramento do exercício social (ddMMyyyy).
    /// Campo <c>DT_EX_SOCIAL</c>.
    /// </summary>
    [CampoSped(Ordem = 12, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtExSocial { get; set; }
}
