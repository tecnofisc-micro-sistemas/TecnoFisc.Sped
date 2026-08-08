using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoY;

/// <summary>Registro Y630 - fundos e clubes de investimento.</summary>
[RegistroSped(Codigo = "Y630", Nivel = 2, Bloco = "Y")]
public sealed partial class RegistroY630 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "Y630";

    [CampoSped(Ordem = 2, Tamanho = 14, Obrigatorio = true, Nome = "CNPJ")]
    public Cnpj Cnpj { get; set; }

    [CampoSped(Ordem = 3, Obrigatorio = true, Nome = "QTE_QUOT")]
    public int QteQuot { get; set; }

    [CampoSped(Ordem = 4, Obrigatorio = true, Nome = "QTE_QUOTA")]
    public int QteQuota { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 19, Decimais = 2, Obrigatorio = true, Nome = "PATR_FIN_PER")]
    public decimal PatrFinPer { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true, Nome = "DAT_ABERT")]
    public DateOnly DatAbert { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 8, Formato = "ddMMyyyy", Nome = "DAT_ENCER")]
    public DateOnly? DatEncer { get; set; }
}
