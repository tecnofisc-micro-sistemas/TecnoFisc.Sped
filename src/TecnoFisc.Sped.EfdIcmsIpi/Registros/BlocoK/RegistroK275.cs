using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

/// <summary>
/// Registro K275 — Correção de Apontamento e Retorno de Insumos dos Registros
/// K215, K220, K235, K255 e K265. Nível hierárquico 4, ocorrência vários por
/// registro K270. Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 261-262.
/// </summary>
[RegistroSped(Codigo = "K275", Nivel = 4, Bloco = "K")]
public sealed partial class RegistroK275 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K275";

    /// <summary>Código da mercadoria (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Quantidade de correção positiva de apontamento ocorrido em período anterior.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 6)]
    public decimal? QtdCorPos { get; set; }

    /// <summary>Quantidade de correção negativa de apontamento ocorrido em período anterior.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 6)]
    public decimal? QtdCorNeg { get; set; }

    /// <summary>Código do insumo que foi substituído, caso ocorra substituição.</summary>
    [CampoSped(Ordem = 5, Tamanho = 60)]
    public string? CodInsSubst { get; set; }
}
