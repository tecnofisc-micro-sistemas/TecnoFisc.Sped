using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1921 - Ajuste/beneficio/incentivo da sub-apuracao do ICMS.
/// Nivel hierarquico 5, ocorrencia 1:N. Conforme Guia Pratico EFD-ICMS/IPI
/// V3.0.6, p. 293.
/// </summary>
[RegistroSped(Codigo = "1921", Nivel = 5, Bloco = "1")]
public sealed partial class Registro1921 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1921";

    /// <summary>Codigo do ajuste da sub-apuracao e deducao, conforme tabela indicada no item 5.1.1.</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Obrigatorio = true)]
    public string? CodAjApur { get; set; }

    /// <summary>Descricao complementar do ajuste da apuracao.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? DescrComplAj { get; set; }

    /// <summary>Valor do ajuste da apuracao.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlAjApur { get; set; }
}
