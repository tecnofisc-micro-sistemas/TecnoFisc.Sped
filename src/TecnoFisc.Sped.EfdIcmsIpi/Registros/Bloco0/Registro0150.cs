using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco0;

/// <summary>
/// Registro 0150 — Tabela de Cadastro do Participante. Nível hierárquico 2, ocorrência vários por arquivo.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 30-31.
/// </summary>
/// <remarks>
/// <b>V019 (Guide 3.1.9 item 5):</b> nova orientação adicionada sobre o cadastro de participantes
/// no contexto do DIFAL (EC 87/2015) — hipótese prevista no § 30 do art. 19 do Convênio SN/1970.
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// </remarks>
[RegistroSped(Codigo = "0150", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0150 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0150";

    /// <summary>Código de identificação do participante no arquivo (livre atribuição do contribuinte, único por período).</summary>
    [CampoSped(Ordem = 2, Tamanho = 60)]
    public string? CodPart { get; set; }

    /// <summary>Nome pessoal ou empresarial do participante.</summary>
    [CampoSped(Ordem = 3, Tamanho = 100, Obrigatorio = true)]
    public string? Nome { get; set; }

    /// <summary>Código do país do participante, conforme tabela do item 3.2.1 da Nota Técnica (Ato COTEPE/ICMS nº 44/2018). Pode ter 5 ou 4 caracteres desprezando zero à esquerda.</summary>
    [CampoSped(Ordem = 4, Tamanho = 5, Obrigatorio = true)]
    public string? CodPais { get; set; }

    /// <summary>
    /// CNPJ do participante (14 dígitos, sem máscara). Obrigatório quando COD_PAIS = "01058" ou "1058" e CPF não informado.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 14)]
    public Cnpj? Cnpj { get; set; }

    /// <summary>
    /// CPF do participante (11 dígitos, sem máscara). Obrigatório quando COD_PAIS = "01058" ou "1058" e CNPJ não informado.
    /// </summary>
    [CampoSped(Ordem = 6, Tamanho = 11)]
    public Cpf? Cpf { get; set; }

    /// <summary>Inscrição Estadual do participante (até 14 caracteres).</summary>
    [CampoSped(Ordem = 7, Tamanho = 14)]
    public InscricaoEstadual? Ie { get; set; }

    /// <summary>Código do município, conforme tabela IBGE (7 dígitos). Obrigatório quando COD_PAIS = "01058" ou "1058"; para exterior, informar vazio ou "9999999".</summary>
    [CampoSped(Ordem = 8, Tamanho = 7)]
    public string? CodMun { get; set; }

    /// <summary>Número de inscrição do participante na SUFRAMA (9 dígitos).</summary>
    [CampoSped(Ordem = 9, Tamanho = 9)]
    public string? Suframa { get; set; }

    /// <summary>Logradouro e endereço do imóvel (até 60 caracteres). Para participante do exterior, incluir cidade e país.</summary>
    [CampoSped(Ordem = 10, Tamanho = 60, Obrigatorio = true)]
    public string? End { get; set; }

    /// <summary>Número do imóvel (até 10 caracteres).</summary>
    [CampoSped(Ordem = 11, Tamanho = 10)]
    public string? Num { get; set; }

    /// <summary>Dados complementares do endereço (até 60 caracteres).</summary>
    [CampoSped(Ordem = 12, Tamanho = 60)]
    public string? Compl { get; set; }

    /// <summary>Bairro em que o imóvel está situado (até 60 caracteres).</summary>
    [CampoSped(Ordem = 13, Tamanho = 60)]
    public string? Bairro { get; set; }
}
