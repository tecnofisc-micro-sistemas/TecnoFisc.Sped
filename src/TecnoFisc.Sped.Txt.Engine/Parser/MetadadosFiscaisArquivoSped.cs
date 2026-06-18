using System.Text;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.Txt.Engine.Parser;

/// <summary>
/// Resultado do sniff fiscal TXT. Inclui a identificacao leve produzida por
/// <see cref="SnifferSped"/> e, quando disponiveis no Registro 0000, CNPJ e periodo.
/// </summary>
public sealed record MetadadosFiscaisArquivoSped(
    MetadadosArquivoSped Identificacao,
    Cnpj? Cnpj,
    DateOnly? DataInicial,
    DateOnly? DataFinal)
{
    public ProjetoSped Projeto => Identificacao.Projeto;
    public int VersaoLeiaute => Identificacao.VersaoLeiaute;
    public Encoding EncodingDetectado => Identificacao.EncodingDetectado;
    public string PrimeiraLinha => Identificacao.PrimeiraLinha;
    public string? CodigoVersaoDeclarado => Identificacao.CodigoVersaoDeclarado;
}
