using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco0;

/// <summary>
/// Registro 0140 — Tabela de Cadastro de Estabelecimentos. Nível hierárquico 2, ocorrência vários (por arquivo).
/// Conforme Guia Prático v1.35, p. 78.
/// </summary>
[RegistroSped(Codigo = "0140", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0140 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0140";

    /// <summary>Código de identificação do estabelecimento (livre atribuição da empresa).</summary>
    [CampoSped(Ordem = 2, Tamanho = 60)]
    public string? CodEst { get; set; }

    /// <summary>Nome empresarial do estabelecimento.</summary>
    [CampoSped(Ordem = 3, Tamanho = 100, Obrigatorio = true)]
    public string? Nome { get; set; }

    /// <summary>CNPJ do estabelecimento (14 dígitos, sem máscara). Validação: DV conferido.</summary>
    [CampoSped(Ordem = 4, Tamanho = 14, Obrigatorio = true)]
    public Cnpj Cnpj { get; set; }

    /// <summary>Sigla da unidade da federação do estabelecimento (2 letras).</summary>
    [CampoSped(Ordem = 5, Tamanho = 2, Obrigatorio = true)]
    public string? Uf { get; set; }

    /// <summary>Inscrição Estadual do estabelecimento, se contribuinte de ICMS (até 14 caracteres).</summary>
    [CampoSped(Ordem = 6, Tamanho = 14)]
    public string? Ie { get; set; }

    /// <summary>Código do município do domicílio fiscal, conforme tabela IBGE (7 dígitos).</summary>
    [CampoSped(Ordem = 7, Tamanho = 7, Obrigatorio = true)]
    public string? CodMun { get; set; }

    /// <summary>Inscrição Municipal do estabelecimento, se contribuinte do ISS.</summary>
    [CampoSped(Ordem = 8)]
    public string? Im { get; set; }

    /// <summary>Inscrição do estabelecimento na Suframa (9 dígitos). Validação: DV conferido.</summary>
    [CampoSped(Ordem = 9, Tamanho = 9)]
    public string? Suframa { get; set; }
}
