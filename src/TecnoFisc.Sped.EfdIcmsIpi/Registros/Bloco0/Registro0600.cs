using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco0;

/// <summary>
/// Registro 0600 — Centro de Custos. Nível hierárquico 2, ocorrência vários por arquivo.
/// Identifica os centros de custos referenciados no Registro 0305. Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 43.
/// </summary>
[RegistroSped(Codigo = "0600", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0600 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0600";

    /// <summary>Data da inclusão/alteração.</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtAlt { get; set; }

    /// <summary>Código do centro de custos.</summary>
    [CampoSped(Ordem = 3, Tamanho = 60)]
    public string? CodCcus { get; set; }

    /// <summary>Nome do centro de custos.</summary>
    [CampoSped(Ordem = 4, Tamanho = 60)]
    public string? Ccus { get; set; }
}
