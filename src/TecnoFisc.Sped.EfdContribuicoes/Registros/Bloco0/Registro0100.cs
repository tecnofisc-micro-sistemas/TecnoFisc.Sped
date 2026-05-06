using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco0;

/// <summary>
/// Registro 0100 — Dados do Contabilista. Nível hierárquico 2, ocorrência vários (por arquivo).
/// Conforme Guia Prático v1.35, p. 70-71.
/// </summary>
[RegistroSped(Codigo = "0100", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0100 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0100";

    /// <summary>Nome do contabilista responsável pela escrituração.</summary>
    [CampoSped(Ordem = 2, Tamanho = 100, Obrigatorio = true)]
    public string? Nome { get; set; }

    /// <summary>
    /// CPF do contabilista responsável (11 dígitos, sem máscara). Validação: DV conferido.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 11, Obrigatorio = true)]
    public Cpf Cpf { get; set; }

    /// <summary>Inscrição no Conselho Regional de Contabilidade (15 caracteres).</summary>
    [CampoSped(Ordem = 4, Tamanho = 15, Obrigatorio = true)]
    public string? Crc { get; set; }

    /// <summary>
    /// CNPJ do escritório de contabilidade (14 dígitos, sem máscara). Validação: DV conferido.
    /// </summary>
    [CampoSped(Ordem = 5, Tamanho = 14)]
    public Cnpj? Cnpj { get; set; }

    /// <summary>CEP do endereço do contabilista/escritório (8 dígitos).</summary>
    [CampoSped(Ordem = 6, Tamanho = 8)]
    public string? Cep { get; set; }

    /// <summary>Logradouro e endereço do imóvel (até 60 caracteres).</summary>
    [CampoSped(Ordem = 7, Tamanho = 60)]
    public string? End { get; set; }

    /// <summary>Número do imóvel.</summary>
    [CampoSped(Ordem = 8)]
    public string? Num { get; set; }

    /// <summary>Dados complementares do endereço (até 60 caracteres).</summary>
    [CampoSped(Ordem = 9, Tamanho = 60)]
    public string? Compl { get; set; }

    /// <summary>Bairro em que o imóvel está situado (até 60 caracteres).</summary>
    [CampoSped(Ordem = 10, Tamanho = 60)]
    public string? Bairro { get; set; }

    /// <summary>Número do telefone (11 caracteres).</summary>
    [CampoSped(Ordem = 11, Tamanho = 11)]
    public string? Fone { get; set; }

    /// <summary>Número do fax (11 caracteres).</summary>
    [CampoSped(Ordem = 12, Tamanho = 11)]
    public string? Fax { get; set; }

    /// <summary>Endereço de correio eletrônico.</summary>
    [CampoSped(Ordem = 13)]
    public string? Email { get; set; }

    /// <summary>Código do município do domicílio fiscal (7 dígitos, tabela IBGE).</summary>
    [CampoSped(Ordem = 14, Tamanho = 7)]
    public string? CodMun { get; set; }
}
