using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

/// <summary>
/// Registro K210 — Desmontagem de Mercadorias — Item de Origem.
/// Nível hierárquico 3, ocorrência vários por registro K100. Conforme Guia Prático
/// EFD-ICMS/IPI V3.0.6, p. 251-252.
/// </summary>
[RegistroSped(Codigo = "K210", Nivel = 3, Bloco = "K")]
public sealed partial class RegistroK210 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K210";

    /// <summary>Data de início da ordem de serviço (ddMMyyyy).</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtIniOs { get; set; }

    /// <summary>Data de conclusão da ordem de serviço (ddMMyyyy).</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtFinOs { get; set; }

    /// <summary>Código de identificação da ordem de serviço.</summary>
    [CampoSped(Ordem = 4, Tamanho = 30)]
    public string? CodDocOs { get; set; }

    /// <summary>Código do item de origem (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 5, Tamanho = 60, Obrigatorio = true)]
    public string? CodItemOri { get; set; }

    /// <summary>Quantidade de origem — saída do estoque.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 6, Obrigatorio = true)]
    public decimal QtdOri { get; set; }
}
