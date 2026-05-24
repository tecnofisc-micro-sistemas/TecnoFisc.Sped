using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Versionamento;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco0;

/// <summary>
/// Registro 0210 — Consumo Específico Padronizado. Nível hierárquico 3, ocorrência 1:N por
/// Registro0200. Informa consumo esperado de insumo/componente para produzir uma unidade do
/// item composto/resultante. Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 37.
/// </summary>
/// <remarks>
/// <b>Descontinuado a partir de V016</b> (Guia Prático 3.0.7, item 22). Anotação
/// <see cref="DescontinuadoAttribute"/> é informacional no read path (ARCHITECTURE §4.7
/// read-only) — o parser continua reconhecendo o registro para que arquivos históricos
/// das versões anteriores sejam lidos sem erro de leiaute.
/// </remarks>
[RegistroSped(Codigo = "0210", Nivel = 3, Bloco = "0")]
[Descontinuado(EmVersao = (int)LayoutEfdIcmsIpi.V016)]
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
