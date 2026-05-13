using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

/// <summary>
/// Registro K215 — Desmontagem de Mercadorias — Itens de Destino.
/// Nível hierárquico 4, ocorrência vários por registro K210. Conforme Guia Prático
/// EFD-ICMS/IPI V3.0.6, p. 252-253.
/// </summary>
[RegistroSped(Codigo = "K215", Nivel = 4, Bloco = "K")]
public sealed partial class RegistroK215 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K215";

    /// <summary>Código do item de destino (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? CodItemDes { get; set; }

    /// <summary>Quantidade de destino — entrada em estoque.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 6, Obrigatorio = true)]
    public decimal QtdDes { get; set; }
}
