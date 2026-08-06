using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoY;

/// <summary>Registro Y672 - outras informações do lucro presumido ou arbitrado.</summary>
[RegistroSped(Codigo = "Y672", Nivel = 2, Bloco = "Y")]
public sealed partial class RegistroY672 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "Y672";

    [CampoSped(Ordem = 2, Tamanho = 19, Decimais = 2)]
    public decimal? VlCapitalAnt { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 19, Decimais = 2)]
    public decimal? VlCapital { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2)]
    public decimal? VlEstoqueAnt { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 19, Decimais = 2)]
    public decimal? VlEstoques { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 2)]
    public decimal? VlCaixaAnt { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 19, Decimais = 2)]
    public decimal? VlCaixa { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 19, Decimais = 2)]
    public decimal? VlAplicFinAnt { get; set; }

    [CampoSped(Ordem = 9, Tamanho = 19, Decimais = 2)]
    public decimal? VlAplicFin { get; set; }

    [CampoSped(Ordem = 10, Tamanho = 19, Decimais = 2)]
    public decimal? VlCtaRecAnt { get; set; }

    [CampoSped(Ordem = 11, Tamanho = 19, Decimais = 2)]
    public decimal? VlCtaRec { get; set; }

    [CampoSped(Ordem = 12, Tamanho = 19, Decimais = 2)]
    public decimal? VlCtaPagAnt { get; set; }

    [CampoSped(Ordem = 13, Tamanho = 19, Decimais = 2)]
    public decimal? VlCtaPag { get; set; }

    [CampoSped(Ordem = 14, Tamanho = 19, Decimais = 2)]
    public decimal? VlCompraMerc { get; set; }

    [CampoSped(Ordem = 15, Tamanho = 19, Decimais = 2)]
    public decimal? VlCompraAtivo { get; set; }

    [CampoSped(Ordem = 16, Tamanho = 19, Decimais = 2)]
    public decimal? VlReceitas { get; set; }

    [CampoSped(Ordem = 17, Tamanho = 19, Decimais = 2)]
    public decimal? TotAtivo { get; set; }

    [CampoSped(Ordem = 18, Tamanho = 1)]
    public MetodoAvaliacaoEstoque? IndAvalEstoq { get; set; }
}
