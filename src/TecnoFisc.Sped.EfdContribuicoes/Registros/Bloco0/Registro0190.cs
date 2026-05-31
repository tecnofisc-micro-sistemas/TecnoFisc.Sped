using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco0;

/// <summary>
/// Registro 0190 — Identificação das Unidades de Medida. Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático v1.35, p. 83.
/// </summary>
[RegistroSped(Codigo = "0190", Nivel = 3, Bloco = "0")]
public sealed partial class Registro0190 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0190";

    /// <summary>Código da unidade de medida utilizada no arquivo digital.</summary>
    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true)]
    public string? Unid { get; set; }

    /// <summary>Descrição da unidade de medida. Não pode ser igual a UNID.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Obrigatorio = true)]
    public string? Descr { get; set; }
}
