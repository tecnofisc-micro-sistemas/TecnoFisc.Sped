using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoK;

/// <summary>Registro K355 - saldos finais das contas de resultado.</summary>
[RegistroSped(Codigo = "K355", Nivel = 3, Bloco = "K")]
public sealed partial class RegistroK355 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K355";

    /// <summary>Código da conta analítica de resultado.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true, Nome = "COD_CTA")]
    public string? CodCta { get; set; }

    /// <summary>Código do centro de custos.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Nome = "COD_CCUS")]
    public string? CodCcus { get; set; }

    /// <summary>Saldo final antes do encerramento.</summary>
    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VL_SLD_FIN")]
    public decimal VlSldFin { get; set; }

    /// <summary>Natureza devedora ou credora do saldo final.</summary>
    [CampoSped(Ordem = 5, Tamanho = 1, Obrigatorio = true, Nome = "IND_VL_SLD_FIN")]
    public IndicadorDebitoCredito IndVlSldFin { get; set; }
}
