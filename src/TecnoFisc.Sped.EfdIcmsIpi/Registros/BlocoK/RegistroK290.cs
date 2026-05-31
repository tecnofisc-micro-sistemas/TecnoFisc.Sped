using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

/// <summary>
/// Registro K290 — Produção Conjunta — Ordem de Produção.
/// Nível hierárquico 3, ocorrência vários por registro K100. Conforme Guia Prático
/// EFD-ICMS/IPI V3.0.6, p. 264.
/// </summary>
[RegistroSped(Codigo = "K290", Nivel = 3, Bloco = "K")]
public sealed partial class RegistroK290 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K290";

    /// <summary>Data de início da ordem de produção (ddMMyyyy).</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtIniOp { get; set; }

    /// <summary>Data de conclusão da ordem de produção (ddMMyyyy).</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtFinOp { get; set; }

    /// <summary>Código de identificação da ordem de produção.</summary>
    [CampoSped(Ordem = 4, Tamanho = 30)]
    public string? CodDocOp { get; set; }
}
