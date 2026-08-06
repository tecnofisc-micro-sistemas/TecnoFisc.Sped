using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoE;

/// <summary>Registro E020 - saldos finais das contas da Parte B da ECF anterior.</summary>
[RegistroSped(Codigo = "E020", Nivel = 2, Bloco = "E")]
public sealed partial class RegistroE020 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "E020";

    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? CodCtaB { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? DescCtaLal { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtApLal { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtLimLal { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 1)]
    public IndicadorTributoParteB? Tributo { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 2)]
    public decimal? VlSaldoFin { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 1)]
    public IndicadorDebitoCredito? IndVlSaldoFin { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 6, Obrigatorio = true)]
    public string? CodPbRfb { get; set; }
}
