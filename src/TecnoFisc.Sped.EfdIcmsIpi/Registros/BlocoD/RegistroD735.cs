using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Versionamento;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D735 — Observações do lançamento fiscal da Nota Fiscal Fatura Eletrônica de
/// Serviços de Comunicação (NFCom, código 62). Filho do <see cref="RegistroD700"/>; informado
/// quando houver ajustes nos documentos fiscais decorrentes de legislação estadual.
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.2.2, p. 217 (Subseção 12).
/// </summary>
/// <remarks>
/// <b>V017 (Guide 3.1.0):</b> registro introduzido com 2 campos.
/// <para>
/// Pacote read-only — modelo único do leiaute mais recente; atributos <see cref="CampoSpedAttribute.DesdeVersao"/>
/// e <see cref="RegistroSpedAttribute.IntroduzidoEm"/> são informacionais (ver ARCHITECTURE §2.5 / §4.7).
/// </para>
/// </remarks>
[RegistroSped(Codigo = "D735", Nivel = 3, Bloco = "D", IntroduzidoEm = (int)LayoutEfdIcmsIpi.V017)]
public sealed partial class RegistroD735 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D735";

    /// <summary>Código da observação do lançamento fiscal (campo 02 do Registro 0460).</summary>
    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true, DesdeVersao = (int)LayoutEfdIcmsIpi.V017)]
    public string? CodObs { get; set; }

    /// <summary>Descrição complementar do código de observação.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, DesdeVersao = (int)LayoutEfdIcmsIpi.V017)]
    public string? TxtCompl { get; set; }
}
