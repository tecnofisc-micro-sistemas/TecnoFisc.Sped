using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

/// <summary>
/// Registro C001 — Abertura do Bloco C (documentos fiscais I — mercadorias / ICMS-IPI).
/// Nível hierárquico 1, ocorrência única por arquivo. Conforme Guia Prático v1.35, p. 104.
/// </summary>
[RegistroSped(Codigo = "C001", Nivel = 1, Bloco = "C")]
public sealed partial class RegistroC001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C001";

    /// <summary>Indicador de movimento do bloco — 0 com dados, 1 sem dados.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorMovimentoBloco IndMov { get; set; }
}
