using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoK;

/// <summary>Registro K356 - mapeamento referencial do saldo de resultado.</summary>
[RegistroSped(Codigo = "K356", Nivel = 4, Bloco = "K")]
public sealed partial class RegistroK356 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K356";

    /// <summary>Código da conta no plano referencial.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true, Nome = "COD_CTA_REF")]
    public string? CodCtaRef { get; set; }

    /// <summary>Saldo final antes do encerramento.</summary>
    [CampoSped(Ordem = 3, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "VL_SLD_FIN")]
    public decimal VlSldFin { get; set; }

    /// <summary>Natureza devedora ou credora do saldo final.</summary>
    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true, Nome = "IND_VL_SLD_FIN")]
    public IndicadorDebitoCredito IndVlSldFin { get; set; }
}
