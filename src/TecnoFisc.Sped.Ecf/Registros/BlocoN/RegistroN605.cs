using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Ecf.Versionamento;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoN;

/// <summary>Registro N605 - contas contábeis usadas na apuração do lucro da exploração.</summary>
[RegistroSped(Codigo = "N605", Nivel = 3, Bloco = "N", IntroduzidoEm = (int)LayoutEcf.V010)]
public sealed partial class RegistroN605 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "N605";

    /// <summary>Código da conta contábil.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true, Nome = "COD_CTA")]
    public string? CodCta { get; set; }

    /// <summary>Código opcional do centro de custos.</summary>
    [CampoSped(Ordem = 3, Nome = "COD_CCUS")]
    public string? CodCcus { get; set; }

    /// <summary>Saldo utilizado.</summary>
    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VALOR")]
    public decimal Valor { get; set; }

    /// <summary>Indicador do saldo utilizado.</summary>
    [CampoSped(Ordem = 5, Obrigatorio = true, Nome = "IND_VALOR")]
    public IndicadorDebitoCredito IndValor { get; set; }
}
