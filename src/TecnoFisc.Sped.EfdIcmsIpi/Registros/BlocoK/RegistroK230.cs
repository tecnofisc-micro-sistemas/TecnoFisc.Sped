using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

/// <summary>
/// Registro K230 — Itens Produzidos.
/// Nível hierárquico 3, ocorrência vários por registro K100. Conforme Guia Prático
/// EFD-ICMS/IPI V3.0.6, p. 254-255.
/// </summary>
[RegistroSped(Codigo = "K230", Nivel = 3, Bloco = "K")]
public sealed partial class RegistroK230 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K230";

    /// <summary>Data de início da ordem de produção (ddMMyyyy).</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtIniOp { get; set; }

    /// <summary>Data de conclusão da ordem de produção (ddMMyyyy).</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtFinOp { get; set; }

    /// <summary>Código de identificação da ordem de produção.</summary>
    [CampoSped(Ordem = 4, Tamanho = 30)]
    public string? CodDocOp { get; set; }

    /// <summary>Código do item produzido (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 5, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Quantidade de produção acabada.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 6, Obrigatorio = true)]
    public decimal QtdEnc { get; set; }
}
