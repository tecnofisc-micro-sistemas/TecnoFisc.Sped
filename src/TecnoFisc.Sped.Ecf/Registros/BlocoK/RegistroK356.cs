using TecnoFisc.Sped.Ecf.Enums;
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
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? CodCtaRef { get; set; }

    /// <summary>Saldo final antes do encerramento.</summary>
    [CampoSped(Ordem = 3, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlSldFin { get; set; }

    /// <summary>Natureza devedora ou credora do saldo final.</summary>
    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true)]
    public IndicadorDebitoCredito IndVlSldFin { get; set; }
}
