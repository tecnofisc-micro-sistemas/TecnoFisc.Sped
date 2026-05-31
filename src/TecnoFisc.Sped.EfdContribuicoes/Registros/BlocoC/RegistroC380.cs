using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

/// <summary>
/// Registro C380 — NF de Venda a Consumidor (Código 02) - Consolidação de Documentos Emitidos.
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático v1.35, p. 147.
/// </summary>
[RegistroSped(Codigo = "C380", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC380 : RegistroSped
{
    public override string Codigo => "C380";

    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDocIni { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDocFin { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 6)]
    public int? NumDocIni { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 6)]
    public int? NumDocFin { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDoc { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlDocCanc { get; set; }
}
