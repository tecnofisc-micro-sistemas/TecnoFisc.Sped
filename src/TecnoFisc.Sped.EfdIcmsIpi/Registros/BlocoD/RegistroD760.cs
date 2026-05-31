using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdIcmsIpi.Versionamento;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D760 — Registro analítico da escrituração consolidada da Nota Fiscal Fatura Eletrônica
/// de Serviços de Comunicação (NFCom, código 62). Totaliza por combinação de CST_ICMS, CFOP e
/// alíquota do ICMS as informações da escrituração consolidada do <see cref="RegistroD750"/>.
/// Nível hierárquico 3, ocorrência 1:N (filho de D750).
/// Conforme Guia Prático EFD-ICMS/IPI V3.2.2, p. 220-221 (Subseção 12).
/// </summary>
/// <remarks>
/// <b>V017 (Guide 3.1.0):</b> registro introduzido com 8 campos.
/// <para>
/// <b>Validação do leiaute (não aplicada aqui):</b> não podem ser informados dois ou mais registros
/// D760 com a mesma combinação de <c>CST_ICMS</c>, <c>CFOP</c> e <c>ALIQ_ICMS</c> para o mesmo
/// documento. Como o pacote é read-only, esta regra fica a cargo do consumidor — o leitor preserva
/// todos os registros como vieram no arquivo.
/// </para>
/// <para>
/// <b>V018 (Guide 3.1.5 item 11 + 3.1.6 itens 7-8):</b> nova orientação geral do registro; descrição
/// e orientação de preenchimento do campo 05 <c>VL_OPR</c> revisadas.
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// </para>
/// <para>
/// Pacote read-only — modelo único do leiaute mais recente; atributos <see cref="CampoSpedAttribute.DesdeVersao"/>
/// e <see cref="RegistroSpedAttribute.IntroduzidoEm"/> são informacionais (ver ARCHITECTURE §2.5 / §4.7).
/// </para>
/// </remarks>
[RegistroSped(Codigo = "D760", Nivel = 3, Bloco = "D", IntroduzidoEm = (int)LayoutEfdIcmsIpi.V017)]
public sealed partial class RegistroD760 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D760";

    /// <summary>Código da Situação Tributária referente ao ICMS, conforme tabela indicada no item 4.3.1.</summary>
    [CampoSped(Ordem = 2, Tamanho = 3, Obrigatorio = true, DesdeVersao = (int)LayoutEfdIcmsIpi.V017)]
    public int? CstIcms { get; set; }

    /// <summary>Código Fiscal de Operação e Prestação, conforme tabela indicada no item 4.2.2.</summary>
    [CampoSped(Ordem = 3, Tamanho = 4, Obrigatorio = true, DesdeVersao = (int)LayoutEfdIcmsIpi.V017)]
    public Cfop? Cfop { get; set; }

    /// <summary>Alíquota do ICMS.</summary>
    [CampoSped(Ordem = 4, Tamanho = 6, Decimais = 2, DesdeVersao = (int)LayoutEfdIcmsIpi.V017)]
    public decimal? AliqIcms { get; set; }

    /// <summary>Valor da operação correspondente à combinação de CST_ICMS, CFOP e alíquota do ICMS.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true, DesdeVersao = (int)LayoutEfdIcmsIpi.V017)]
    public decimal? VlOpr { get; set; }

    /// <summary>Parcela correspondente ao valor da base de cálculo do ICMS referente à combinação CST_ICMS, CFOP e alíquota do ICMS.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true, DesdeVersao = (int)LayoutEfdIcmsIpi.V017)]
    public decimal? VlBcIcms { get; set; }

    /// <summary>Parcela correspondente ao valor do ICMS referente à combinação CST_ICMS, CFOP e alíquota do ICMS.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Decimais = 2, Obrigatorio = true, DesdeVersao = (int)LayoutEfdIcmsIpi.V017)]
    public decimal? VlIcms { get; set; }

    /// <summary>Valor não tributado em função da redução da base de cálculo do ICMS, referente à combinação CST_ICMS, CFOP e alíquota do ICMS.</summary>
    [CampoSped(Ordem = 8, Tamanho = 0, Decimais = 2, Obrigatorio = true, DesdeVersao = (int)LayoutEfdIcmsIpi.V017)]
    public decimal? VlRedBc { get; set; }

    /// <summary>Código da observação do lançamento fiscal (campo 02 do Registro 0460).</summary>
    [CampoSped(Ordem = 9, Tamanho = 6, DesdeVersao = (int)LayoutEfdIcmsIpi.V017)]
    public string? CodObs { get; set; }
}
