using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C105 — Operações com ICMS ST Recolhido para UF Diversa do Destinatário do
/// Documento Fiscal (Código 55).
/// Nível hierárquico 3, ocorrência 1:1.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 66.
/// </summary>
/// <remarks>
/// <b>V017 (Guide 3.1.4 itens 4-5):</b> instrução do registro reescrita e novo valor válido
/// "2" admitido no campo 02 <c>OPER</c>.
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// </remarks>
[RegistroSped(Codigo = "C105", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC105 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C105";

    /// <summary>Indicador do tipo de operação: 0-Combustíveis e Lubrificantes; 1-Leasing de veículos ou faturamento direto.</summary>
    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public IndicadorOperacaoIcmsSt Oper { get; set; }

    /// <summary>Sigla da UF de destino do ICMS ST.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public string? Uf { get; set; }
}
