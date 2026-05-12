using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoB;

/// <summary>
/// Registro B500 — Apuração do ISS Sociedade Uniprofissional.
/// Nível hierárquico 2, ocorrência um (por arquivo).
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 56.
/// </summary>
[RegistroSped(Codigo = "B500", Nivel = 2, Bloco = "B")]
public sealed partial class RegistroB500 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "B500";

    /// <summary>Valor mensal das receitas auferidas pela sociedade uniprofissional.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlRec { get; set; }

    /// <summary>Quantidade de profissionais habilitados.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public int QtdProf { get; set; }

    /// <summary>Valor do ISS devido.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlOr { get; set; }
}
