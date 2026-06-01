using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Versionamento;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C895 — Observações do lançamento fiscal do Resumo Diário do CF-e-SAT (código 59)
/// por equipamento SAT-CF-e. Filho do <see cref="RegistroC890"/>; informado quando, em
/// decorrência da legislação estadual, houver ajustes nos documentos fiscais, informações sobre
/// diferencial de alíquota, antecipação de imposto e outras situações equivalentes às observações
/// lançadas na coluna "Observações" dos Livros Fiscais (Convênio SN/70 — SINIEF, art. 63, I a IV).
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.2.2, p. 169 (Subseção 12).
/// </summary>
/// <remarks>
/// <b>V017 (Guide 3.1.0):</b> registro introduzido com 2 campos. Estrutura idêntica ao
/// <see cref="RegistroC855"/>, diferindo apenas pelo pai (C890 consolidado vs. C850 analítico).
/// <para>
/// Pacote read-only — modelo único do leiaute mais recente; atributos <see cref="CampoSpedAttribute.DesdeVersao"/>
/// e <see cref="RegistroSpedAttribute.IntroduzidoEm"/> são informacionais (ver ARCHITECTURE §2.5 / §4.7).
/// </para>
/// </remarks>
[RegistroSped(Codigo = "C895", Nivel = 3, Bloco = "C", IntroduzidoEm = (int)LayoutEfdIcmsIpi.V017)]
public sealed partial class RegistroC895 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C895";

    /// <summary>Código da observação do lançamento fiscal (campo 02 do <see cref="Bloco0.Registro0460"/>).</summary>
    [CampoSped(Ordem = 2, Tamanho = 6, Obrigatorio = true, DesdeVersao = (int)LayoutEfdIcmsIpi.V017)]
    public string? CodObs { get; set; }

    /// <summary>Descrição complementar do código de observação.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, DesdeVersao = (int)LayoutEfdIcmsIpi.V017)]
    public string? TxtCompl { get; set; }
}
