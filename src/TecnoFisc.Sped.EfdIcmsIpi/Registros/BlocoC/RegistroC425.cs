using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C425 — Resumo de Itens do Movimento Diário (código 02 e 2D).
/// Identifica os produtos comercializados na data da movimentação relativa à Redução Z informada.
/// Nível hierárquico 5, ocorrência 1:N por C420.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 121.
/// </summary>
/// <remarks>
/// <b>V016 (Guide 3.0.7 item 9):</b> validação adicional no campo 04 <c>UNID</c>.
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// </remarks>
[RegistroSped(Codigo = "C425", Nivel = 5, Bloco = "C")]
public sealed partial class RegistroC425 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C425";

    /// <summary>Código do item (campo 02 do Registro 0200).</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    /// <summary>Quantidade acumulada do item.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 3, Obrigatorio = true)]
    public decimal Qtd { get; set; }

    /// <summary>Unidade do item (campo 02 do Registro 0190).</summary>
    [CampoSped(Ordem = 4, Tamanho = 6, Obrigatorio = true)]
    public string? Unid { get; set; }

    /// <summary>Valor acumulado do item.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlItem { get; set; }

    /// <summary>Valor do PIS.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2)]
    public decimal? VlPis { get; set; }

    /// <summary>Valor da COFINS.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofins { get; set; }
}
