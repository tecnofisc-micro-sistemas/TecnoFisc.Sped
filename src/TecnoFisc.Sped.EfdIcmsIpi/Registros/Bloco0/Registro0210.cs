using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco0;

/// <summary>
/// Registro 0210 — Consumo Específico Padronizado. Nível hierárquico 3, ocorrência 1:N por
/// Registro0200. Informa consumo esperado de insumo/componente para produzir uma unidade do
/// item composto/resultante. Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 37.
/// </summary>
[RegistroSped(Codigo = "0210", Nivel = 3, Bloco = "0")]
public sealed partial class Registro0210 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0210";

    /// <summary>Código do item componente/insumo, correspondente ao campo COD_ITEM do Registro 0200.</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? CodItemComp { get; set; }

    /// <summary>Quantidade de insumo por unidade do item resultante. Deve ser maior que zero.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 6, Obrigatorio = true)]
    public decimal QtdComp { get; set; }

    /// <summary>Perda/quebra normal percentual do insumo por unidade produzida do item resultante.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 4, Obrigatorio = true)]
    public decimal Perda { get; set; }
}
