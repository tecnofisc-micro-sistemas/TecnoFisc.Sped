using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Versionamento;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D761 — Informações do Fundo de Combate à Pobreza (FCP) na escrituração consolidada
/// da NFCom (código 62). Filho do <see cref="RegistroD760"/>; valores meramente informativos.
/// Nível hierárquico 4, ocorrência 1:1.
/// Conforme Guia Prático EFD-ICMS/IPI V3.2.2, p. 221 (Subseção 12).
/// </summary>
/// <remarks>
/// <b>V017 (Guide 3.1.0):</b> registro introduzido com 1 campo.
/// <para>
/// Pacote read-only — modelo único do leiaute mais recente; atributos <see cref="CampoSpedAttribute.DesdeVersao"/>
/// e <see cref="RegistroSpedAttribute.IntroduzidoEm"/> são informacionais (ver ARCHITECTURE §2.5 / §4.7).
/// </para>
/// </remarks>
[RegistroSped(Codigo = "D761", Nivel = 4, Bloco = "D", IntroduzidoEm = (int)LayoutEfdIcmsIpi.V017)]
public sealed partial class RegistroD761 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D761";

    /// <summary>Valor do FCP vinculado à operação própria, na combinação de CST_ICMS, CFOP e alíquota do ICMS do D760.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2, Obrigatorio = true, DesdeVersao = (int)LayoutEfdIcmsIpi.V017)]
    public decimal? VlFcpOp { get; set; }
}
