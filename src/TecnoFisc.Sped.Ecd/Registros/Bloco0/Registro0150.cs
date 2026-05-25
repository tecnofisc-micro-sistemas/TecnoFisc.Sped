using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.Ecd.Registros.Bloco0;

/// <summary>
/// Registro 0150 — Tabela de Cadastro do Participante. Nível hierárquico 2, ocorrência 0:N.
/// Identifica pessoas físicas e jurídicas com as quais a empresa tem relacionamento específico
/// previsto na tabela do Sped. Campo chave: <c>COD_PART</c>.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 83.
/// </summary>
/// <remarks>
/// <para>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_REGISTRO_DUPLICADO</c>: COD_PART (campo 02) não deve estar duplicado no arquivo.</item>
///   <item><c>REGRA_TABELA_PAIS</c>: COD_PAIS (campo 04) deve existir na tabela do Banco Central do Brasil.</item>
///   <item><c>REGRA_VALIDA_CNPJ</c>: CNPJ (campo 05), quando preenchido, deve ter dígitos verificadores válidos.</item>
///   <item><c>REGRA_VALIDA_CPF</c>: CPF (campo 06), quando preenchido, deve ter dígitos verificadores válidos.</item>
///   <item><c>REGRA_TABELA_UF</c>: UF (campo 08), quando preenchido, deve existir na tabela de Unidades da Federação.</item>
///   <item><c>REGRA_CAMPO_NAO_OBRIGATORIO_PAIS_BRASIL</c>: UF (campo 08) e COD_MUN (campo 11) só devem ser preenchidos quando COD_PAIS corresponder ao Brasil.</item>
///   <item><c>REGRA_TABELA_MUNICIPIO</c>: COD_MUN (campo 11), quando preenchido, deve existir na tabela IBGE de municípios.</item>
///   <item><c>REGRA_CAMPO_CARACTERE_INVALIDO</c>: IE (campo 09), IE_ST (campo 10), IM (campo 12) e SUFRAMA (campo 13) devem conter apenas letras e dígitos.</item>
/// </list>
/// </para>
/// </remarks>
[RegistroSped(Codigo = "0150", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0150 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0150";

    /// <summary>
    /// Código de identificação do participante no arquivo, criado pela própria pessoa jurídica.
    /// Campo <c>COD_PART</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? CodPart { get; set; }

    /// <summary>Nome pessoal ou empresarial do participante. Campo <c>NOME</c>.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? Nome { get; set; }

    /// <summary>
    /// Código do país do participante, conforme tabela do Banco Central do Brasil (5 dígitos,
    /// ex.: "01058" para Brasil). Campo <c>COD_PAIS</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 5, Obrigatorio = true)]
    public string? CodPais { get; set; }

    /// <summary>CNPJ do participante (14 dígitos, sem máscara). Campo <c>CNPJ</c>.</summary>
    [CampoSped(Ordem = 5, Tamanho = 14)]
    public Cnpj? Cnpj { get; set; }

    /// <summary>CPF do participante (11 dígitos, sem máscara). Campo <c>CPF</c>.</summary>
    [CampoSped(Ordem = 6, Tamanho = 11)]
    public Cpf? Cpf { get; set; }

    /// <summary>
    /// Número de Identificação do Trabalhador — PIS, Pasep ou SUS (11 dígitos). Campo <c>NIT</c>.
    /// </summary>
    [CampoSped(Ordem = 7, Tamanho = 11)]
    public string? Nit { get; set; }

    /// <summary>
    /// Código numérico IBGE da unidade da federação do participante (2 dígitos, ex.: "35" para SP).
    /// Deve ser preenchido apenas quando COD_PAIS corresponder ao Brasil. Campo <c>UF</c>.
    /// </summary>
    [CampoSped(Ordem = 8, Tamanho = 2)]
    public string? Uf { get; set; }

    /// <summary>Inscrição Estadual do participante. Campo <c>IE</c>.</summary>
    [CampoSped(Ordem = 9, Tamanho = 0)]
    public string? Ie { get; set; }

    /// <summary>
    /// Inscrição Estadual do participante na unidade da federação do destinatário, na condição
    /// de contribuinte substituto. Campo <c>IE_ST</c>.
    /// </summary>
    [CampoSped(Ordem = 10, Tamanho = 0)]
    public string? IeSt { get; set; }

    /// <summary>
    /// Código do município, conforme tabela IBGE (7 dígitos). Deve ser preenchido apenas quando
    /// COD_PAIS corresponder ao Brasil. Campo <c>COD_MUN</c>.
    /// </summary>
    [CampoSped(Ordem = 11, Tamanho = 7)]
    public string? CodMun { get; set; }

    /// <summary>Inscrição Municipal do participante. Campo <c>IM</c>.</summary>
    [CampoSped(Ordem = 12, Tamanho = 0)]
    public string? Im { get; set; }

    /// <summary>Número de inscrição do participante na Suframa (9 dígitos). Campo <c>SUFRAMA</c>.</summary>
    [CampoSped(Ordem = 13, Tamanho = 9)]
    public string? Suframa { get; set; }
}
