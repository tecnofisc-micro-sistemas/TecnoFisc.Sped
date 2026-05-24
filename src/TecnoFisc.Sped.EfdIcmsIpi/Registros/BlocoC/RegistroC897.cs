using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Versionamento;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C897 — Outras obrigações tributárias, ajustes e informações de valores provenientes
/// do documento fiscal — Resumo Diário do CF-e-SAT (código 59) por equipamento SAT-CF-e. Filho
/// do <see cref="RegistroC895"/>; detalha obrigações tributárias, ajustes e informações de valores
/// que podem ou não alterar o cálculo do imposto. Deve ser informado somente pelas UF que
/// publicarem a tabela constante no item 5.3 da Nota Técnica (Ato COTEPE/ICMS nº 44/2018 e alterações).
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.2.2, p. 169-170 (Subseção 12).
/// </summary>
/// <remarks>
/// <b>V017 (Guide 3.1.0):</b> registro introduzido com 7 campos. Estrutura idêntica ao
/// <see cref="RegistroC857"/>, diferindo apenas pela árvore consolidada (C890 → C895 → C897 vs.
/// C850 → C855 → C857 analítica).
/// <para>
/// Pacote read-only — modelo único do leiaute mais recente; atributos <see cref="CampoSpedAttribute.DesdeVersao"/>
/// e <see cref="RegistroSpedAttribute.IntroduzidoEm"/> são informacionais (ver ARCHITECTURE §2.5 / §4.7).
/// </para>
/// </remarks>
[RegistroSped(Codigo = "C897", Nivel = 4, Bloco = "C", IntroduzidoEm = (int)LayoutEfdIcmsIpi.V017)]
public sealed partial class RegistroC897 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C897";

    /// <summary>Código do ajuste/benefício/incentivo, conforme tabela indicada no item 5.3.</summary>
    [CampoSped(Ordem = 2, Tamanho = 10, Obrigatorio = true, DesdeVersao = (int)LayoutEfdIcmsIpi.V017)]
    public string? CodAj { get; set; }

    /// <summary>Descrição complementar do ajuste do documento fiscal.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, DesdeVersao = (int)LayoutEfdIcmsIpi.V017)]
    public string? DescrComplAj { get; set; }

    /// <summary>Código do item (campo 02 do <see cref="Bloco0.Registro0200"/>).</summary>
    [CampoSped(Ordem = 4, Tamanho = 60, DesdeVersao = (int)LayoutEfdIcmsIpi.V017)]
    public string? CodItem { get; set; }

    /// <summary>Base de cálculo do ICMS ou do ICMS ST.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, DesdeVersao = (int)LayoutEfdIcmsIpi.V017)]
    public decimal? VlBcIcms { get; set; }

    /// <summary>Alíquota do ICMS.</summary>
    [CampoSped(Ordem = 6, Tamanho = 6, Decimais = 2, DesdeVersao = (int)LayoutEfdIcmsIpi.V017)]
    public decimal? AliqIcms { get; set; }

    /// <summary>Valor do ICMS ou do ICMS ST.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, DesdeVersao = (int)LayoutEfdIcmsIpi.V017)]
    public decimal? VlIcms { get; set; }

    /// <summary>Outros valores conforme Tabela 5.3.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, DesdeVersao = (int)LayoutEfdIcmsIpi.V017)]
    public decimal? VlOutros { get; set; }
}
