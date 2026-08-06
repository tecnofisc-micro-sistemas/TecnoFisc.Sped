using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoE;

/// <summary>Registro E355 - saldos das contas de resultado antes do encerramento.</summary>
[RegistroSped(Codigo = "E355", Nivel = 3, Bloco = "E")]
public sealed partial class RegistroE355 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E355";

    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? CodCta { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? CodCcus { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlSldFin { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 1, Obrigatorio = true)]
    public IndicadorDebitoCredito IndVlSldFin { get; set; }
}
