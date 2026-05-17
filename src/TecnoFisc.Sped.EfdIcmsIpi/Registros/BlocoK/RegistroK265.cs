using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

/// <summary>
/// Registro K265 — Reprocessamento/Reparo — Mercadorias Consumidas e/ou Retornadas.
/// Nível hierárquico 4, ocorrência vários por registro K260. Conforme Guia Prático
/// EFD-ICMS/IPI V3.0.6, p. 259.
/// </summary>
[RegistroSped(Codigo = "K265", Nivel = 4, Bloco = "K")]
public sealed partial class RegistroK265 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K265";

    /// <summary>Código da mercadoria (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Quantidade consumida, saída do estoque.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 6)]
    public decimal? QtdCons { get; set; }

    /// <summary>Quantidade retornada, entrada em estoque.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 6)]
    public decimal? QtdRet { get; set; }
}
