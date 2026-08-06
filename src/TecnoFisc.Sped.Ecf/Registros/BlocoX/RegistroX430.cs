using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X430 - rendimentos de serviços, juros e dividendos.</summary>
[RegistroSped(Codigo = "X430", Nivel = 2, Bloco = "X")]
public sealed partial class RegistroX430 : RegistroSped
{
    public override string Codigo => "X430";

    /// <summary>País conforme tabela dinâmica, preservado como código lexical.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true)]
    public string? Pais { get; set; }

    [CampoSped(Ordem = 3, Decimais = 2)]
    public decimal? VlServAssist { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2)]
    public decimal? VlServSemAssist { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 19, Decimais = 2)]
    public decimal? VlServSemAssistExt { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 2)]
    public decimal? VlJuro { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 2)]
    public decimal? VlDemaisJuros { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 19, Decimais = 2)]
    public decimal? VlDivid { get; set; }
}
