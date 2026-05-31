using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C191 — Informações do Fundo de Combate à Pobreza (FCP) na NF-e (cód. 55) e NFC-e (cód. 65).
/// Filho do C190; valores meramente informativos, não contabilizados na apuração do Bloco E.
/// Nível hierárquico 4, ocorrência 1:1.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 104.
/// </summary>
[RegistroSped(Codigo = "C191", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC191 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C191";

    /// <summary>Valor do FCP vinculado à operação própria, na combinação de CST_ICMS, CFOP e alíquota do ICMS.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2)]
    public decimal? VlFcpOp { get; set; }

    /// <summary>Valor do FCP vinculado à operação de substituição tributária, na combinação de CST_ICMS, CFOP e alíquota do ICMS.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2)]
    public decimal? VlFcpSt { get; set; }

    /// <summary>Valor do FCP retido anteriormente nas operações com ST, na combinação de CST_ICMS, CFOP e alíquota do ICMS.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2)]
    public decimal? VlFcpRet { get; set; }
}
