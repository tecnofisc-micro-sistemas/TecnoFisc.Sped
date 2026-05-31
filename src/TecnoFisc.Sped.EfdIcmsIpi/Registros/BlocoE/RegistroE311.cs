using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

/// <summary>
/// Registro E311 — Ajuste/Benefício/Incentivo da Apuração do Fundo de Combate à Pobreza e do ICMS Diferencial de Alíquota.
/// Nível hierárquico 4, ocorrência 1:N. Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 227-228.
/// </summary>
[RegistroSped(Codigo = "E311", Nivel = 4, Bloco = "E")]
public sealed partial class RegistroE311 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E311";

    /// <summary>Código do ajuste da apuração e dedução, conforme tabela indicada no item 5.1.1.</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Obrigatorio = true)]
    public string? CodAjApur { get; set; }

    /// <summary>Descrição complementar do ajuste da apuração.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? DescrComplAj { get; set; }

    /// <summary>Valor do ajuste da apuração.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjApur { get; set; }
}
