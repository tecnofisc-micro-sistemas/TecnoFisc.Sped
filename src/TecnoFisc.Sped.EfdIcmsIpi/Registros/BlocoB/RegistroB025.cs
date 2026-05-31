using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoB;

/// <summary>
/// Registro B025 — Detalhamento por Combinação de Alíquota e Item da Lista de Serviços (LC 116/2003).
/// Filho do B020. Nível hierárquico 3, ocorrência 1:N por B020 pai.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 47.
/// </summary>
[RegistroSped(Codigo = "B025", Nivel = 3, Bloco = "B")]
public sealed partial class RegistroB025 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "B025";

    /// <summary>Parcela do valor contábil referente à combinação de alíquota e item da lista.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlContP { get; set; }

    /// <summary>Parcela da base de cálculo do ISS referente à combinação de alíquota e item da lista.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcIssP { get; set; }

    /// <summary>Alíquota do ISS (máximo 5%).</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal AliqIss { get; set; }

    /// <summary>Parcela do valor do ISS referente à combinação de alíquota e item da lista.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIssP { get; set; }

    /// <summary>Parcela do valor das operações isentas ou não-tributadas pelo ISS referente à combinação.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIsntIssP { get; set; }

    /// <summary>Item da lista de serviços conforme Tabela 4.6.3 (LC 116/2003). Tamanho fixo 4 caracteres.</summary>
    [CampoSped(Ordem = 7, Tamanho = 4, Obrigatorio = true)]
    public string? CodServ { get; set; }
}
