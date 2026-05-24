using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

/// <summary>
/// Registro K235 — Insumos Consumidos.
/// Nível hierárquico 4, ocorrência vários por registro K230. Conforme Guia Prático
/// EFD-ICMS/IPI V3.0.6, p. 255-256.
/// </summary>
/// <remarks>
/// <b>V016 (Guide 3.0.9 item 4):</b> regra de validação revisada.
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// </remarks>
[RegistroSped(Codigo = "K235", Nivel = 4, Bloco = "K")]
public sealed partial class RegistroK235 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K235";

    /// <summary>Data de saída do estoque para alocação ao produto (ddMMyyyy).</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtSaida { get; set; }

    /// <summary>Código do item componente/insumo (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 3, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Quantidade consumida do item.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 6, Obrigatorio = true)]
    public decimal Qtd { get; set; }

    /// <summary>Código do insumo substituído, caso ocorra substituição (campo 02 do Registro 0210).</summary>
    [CampoSped(Ordem = 5, Tamanho = 60)]
    public string? CodInsSubst { get; set; }
}
