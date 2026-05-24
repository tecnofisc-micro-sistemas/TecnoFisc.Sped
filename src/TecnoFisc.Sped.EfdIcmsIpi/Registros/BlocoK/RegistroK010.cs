using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;
using TecnoFisc.Sped.EfdIcmsIpi.Versionamento;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

/// <summary>
/// Registro K010 — Informação sobre o Tipo de Leiaute (Simplificado / Completo / Restrito).
/// Nível hierárquico 2, ocorrência única por arquivo. Obrigatório quando o campo
/// <c>IND_MOV</c> do <see cref="RegistroK001"/> é "0" (com dados). Conforme Guia Prático
/// EFD-ICMS/IPI V3.2.2, p. 266 (Subseção 12).
/// </summary>
/// <remarks>
/// <b>Introduzido em V016</b> (Guide 3.0.9 item 3, facultativo a partir de 2022). Obrigatoriedade
/// fiscal a partir de janeiro/2023 (V017) — pacote read-only não distingue, atributo
/// <see cref="RegistroSpedAttribute.IntroduzidoEm"/> é informacional.
/// </remarks>
[RegistroSped(Codigo = "K010", Nivel = 2, Bloco = "K", IntroduzidoEm = (int)LayoutEfdIcmsIpi.V016)]
public sealed partial class RegistroK010 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K010";

    /// <summary>
    /// Indicador do tipo de leiaute adotado pelo Bloco K — 0 simplificado, 1 completo,
    /// 2 restrito aos saldos de estoque.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true, DesdeVersao = (int)LayoutEfdIcmsIpi.V016)]
    public TipoLeiauteBlocoK? IndTpLeiaute { get; set; }
}
