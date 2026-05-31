using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

/// <summary>
/// Registro K292 — Produção Conjunta — Insumos Consumidos.
/// Nível hierárquico 4, ocorrência vários por registro K290. Conforme Guia Prático
/// EFD-ICMS/IPI V3.0.6, p. 265.
/// </summary>
/// <remarks>
/// <b>V016 (Guide 3.0.9 item 4):</b> regra de validação revisada.
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// </remarks>
[RegistroSped(Codigo = "K292", Nivel = 4, Bloco = "K")]
public sealed partial class RegistroK292 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K292";

    /// <summary>Código do insumo/componente consumido (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Quantidade consumida.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 6, Obrigatorio = true)]
    public decimal Qtd { get; set; }
}
