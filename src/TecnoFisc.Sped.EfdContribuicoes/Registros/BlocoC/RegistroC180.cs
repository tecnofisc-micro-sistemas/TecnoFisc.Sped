using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

/// <summary>
/// Registro C180 — Consolidação de NF-e Emitidas Pela Pessoa Jurídica (Códigos 55 e 65) – Operações de Vendas.
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático v1.35, p. 126.
/// </summary>
[RegistroSped(Codigo = "C180", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC180 : RegistroSped
{
    public override string Codigo => "C180";

    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDocIni { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtDocFin { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 8)]
    public Ncm? CodNcm { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 3)]
    public string? ExIpi { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotItem { get; set; }
}
