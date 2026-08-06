using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>Registro X420 - royalties recebidos ou pagos.</summary>
[RegistroSped(Codigo = "X420", Nivel = 2, Bloco = "X")]
public sealed partial class RegistroX420 : RegistroSped
{
    public override string Codigo => "X420";

    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public TipoRoyalty TipRoy { get; set; }

    /// <summary>País conforme tabela dinâmica, preservado como código lexical.</summary>
    [CampoSped(Ordem = 3, Tamanho = 3, Obrigatorio = true)]
    public string? Pais { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2)]
    public decimal? VlExplDirSw { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 19, Decimais = 2)]
    public decimal? VlExplDirAut { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 2)]
    public decimal? VlExplMarca { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 2)]
    public decimal? VlExplPat { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 19, Decimais = 2)]
    public decimal? VlExplKnow { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 19, Decimais = 2)]
    public decimal? VlExplFranq { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 19, Decimais = 2)]
    public decimal? VlExplInt { get; set; }
}
