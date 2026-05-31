using System.Text;

namespace TecnoFisc.Sped.Txt.Engine.Parser;

/// <summary>
/// Resultado do sniffer TXT: identifica o projeto e a versao do leiaute declarada no inicio
/// do arquivo sem materializar registros.
/// </summary>
public sealed record MetadadosArquivoSped(
    ProjetoSped Projeto,
    int VersaoLeiaute,
    Encoding EncodingDetectado,
    string PrimeiraLinha,
    string? CodigoVersaoDeclarado);
