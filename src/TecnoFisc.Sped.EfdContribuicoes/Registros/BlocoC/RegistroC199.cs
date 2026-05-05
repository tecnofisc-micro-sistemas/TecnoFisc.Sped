using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdContribuicoes.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

/// <summary>
/// Registro C199 — Complemento do Documento - Operações de Importação (Código 55).
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático v1.35, p. 146.
/// </summary>
[RegistroSped(Codigo = "C199", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC199 : RegistroSped
{
    public override string Codigo => "C199";

    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public CodigoDocumentoImportacao CodDocImp { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 15, Obrigatorio = true)]
    public string? NumDocImp { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2)]
    public decimal? VlPisImp { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2)]
    public decimal? VlCofinsImp { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 20)]
    public string? NumAcdraw { get; set; }
}
