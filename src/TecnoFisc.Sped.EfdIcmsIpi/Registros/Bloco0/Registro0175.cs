using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco0;

/// <summary>
/// Registro 0175 — Alteração da Tabela de Cadastro de Participante. Nível hierárquico 3,
/// ocorrência 1:N por Registro0150. Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 32.
/// </summary>
[RegistroSped(Codigo = "0175", Nivel = 3, Bloco = "0")]
public sealed partial class Registro0175 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0175";

    /// <summary>Data de alteração do cadastro (ddMMyyyy). Deve estar entre DT_INI e DT_FIN do Registro0000.</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtAlt { get; set; }

    /// <summary>Número do campo alterado relativo ao Registro0150 (campos 03 a 13, exceto 07). Valores válidos: 03, 04, 05, 06, 08, 09, 10, 11, 12, 13.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true)]
    public string? NrCampo { get; set; }

    /// <summary>Conteúdo anterior do campo (até 100 caracteres). Válido até às 24:00 horas do dia anterior à data de alteração.</summary>
    [CampoSped(Ordem = 4, Tamanho = 100, Obrigatorio = true)]
    public string? ContAnt { get; set; }
}
