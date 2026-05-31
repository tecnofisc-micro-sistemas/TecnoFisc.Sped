using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco0;

/// <summary>
/// Registro 0150 — Tabela de Cadastro do Participante. Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático v1.35, p. 81-82.
/// </summary>
[RegistroSped(Codigo = "0150", Nivel = 3, Bloco = "0")]
public sealed partial class Registro0150 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0150";

    /// <summary>Código de identificação do participante no arquivo (livre atribuição).</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? CodPart { get; set; }

    /// <summary>Nome pessoal ou empresarial do participante.</summary>
    [CampoSped(Ordem = 3, Tamanho = 100, Obrigatorio = true)]
    public string? Nome { get; set; }

    /// <summary>Código do país, conforme tabela item 3.2.1 (ex.: 01058 ou 1058 para Brasil).</summary>
    [CampoSped(Ordem = 4, Tamanho = 5, Obrigatorio = true)]
    public string? CodPais { get; set; }

    /// <summary>CNPJ do participante (14 dígitos, sem máscara). Validação: DV conferido.</summary>
    [CampoSped(Ordem = 5, Tamanho = 14)]
    public Cnpj? Cnpj { get; set; }

    /// <summary>CPF do participante (11 dígitos, sem máscara). Validação: DV conferido.</summary>
    [CampoSped(Ordem = 6, Tamanho = 11)]
    public Cpf? Cpf { get; set; }

    /// <summary>Inscrição Estadual do participante (até 14 caracteres).</summary>
    [CampoSped(Ordem = 7, Tamanho = 14)]
    public string? Ie { get; set; }

    /// <summary>Código do município, conforme tabela IBGE (7 dígitos). Obrigatório quando COD_PAIS = Brasil.</summary>
    [CampoSped(Ordem = 8, Tamanho = 7)]
    public string? CodMun { get; set; }

    /// <summary>Número de inscrição do participante na Suframa (9 dígitos). Validação: DV conferido.</summary>
    [CampoSped(Ordem = 9, Tamanho = 9)]
    public string? Suframa { get; set; }

    /// <summary>Logradouro e endereço do imóvel (até 60 caracteres).</summary>
    [CampoSped(Ordem = 10, Tamanho = 60)]
    public string? End { get; set; }

    /// <summary>Número do imóvel.</summary>
    [CampoSped(Ordem = 11)]
    public string? Num { get; set; }

    /// <summary>Dados complementares do endereço (até 60 caracteres).</summary>
    [CampoSped(Ordem = 12, Tamanho = 60)]
    public string? Compl { get; set; }

    /// <summary>Bairro em que o imóvel está situado (até 60 caracteres).</summary>
    [CampoSped(Ordem = 13, Tamanho = 60)]
    public string? Bairro { get; set; }
}
