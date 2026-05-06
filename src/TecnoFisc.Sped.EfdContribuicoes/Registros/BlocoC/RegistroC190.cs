using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

/// <summary>
/// Registro C190 — Consolidação de NF-e (Código 55) – Operações de Aquisição com Direito a Crédito,
/// e Operações de Devolução de Compras e Vendas.
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático v1.35, p. 135.
/// </summary>
[RegistroSped(Codigo = "C190", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC190 : RegistroSped
{
    public override string Codigo => "C190";

    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? CodMod { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtRefIni { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtRefFin { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 60, Obrigatorio = true)]
    public string? CodItem { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 8)]
    public Ncm? CodNcm { get; set; }

    [CampoSped(Ordem = 7, Tamanho = 3)]
    public string? ExIpi { get; set; }

    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlTotItem { get; set; }
}
