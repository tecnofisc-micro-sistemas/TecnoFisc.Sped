using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoM;

/// <summary>Registro M030 - período de apuração do IRPJ e da CSLL.</summary>
[RegistroSped(Codigo = "M030", Nivel = 2, Bloco = "M")]
public sealed partial class RegistroM030 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "M030";

    /// <summary>Data do Início do Período</summary>
    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtIni { get; set; }

    /// <summary>Data do Fim doperíodo</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtFin { get; set; }

    /// <summary>Período de apuração [para 0010.FORMA_APUR = “A”]: A00 – Receita Bruta/ Balanço de Suspensão e Redução Anual A01 – Balanço de Suspensão e Redução até Janeiro A02 – Balanço de Suspensão e Redução até Fevereiro A03 – Balanço de Suspensão e Redução até Março A04 – Balanço de Suspensão e Redução até Abril A05 – Balanço de Suspensão e Redução até Maio A06 – Balanço de Suspensão e Redução até Junho A07 – Balanço de Suspensão e Redução até Julho A08 – Balanço de Suspensão e Redução até Agosto A09 – Balanço de Suspensão e Redução até Setembro A10 – Balanço de Suspensão e Redução até Outubro A11 – Balanço de Suspensão e Redução até Novembro A12 – Balanço de Suspensão e Redução até Dezembro Indicador do período de referência [para 0010.FORMA_APUR = “T” OU (0010.FORMA_APUR = “A” E 0010.FORMA_TRIB = “2”)]: T01 – 1º Trimestre T02 – 2º Trimestre T03 – 3º Trimestre T04 – 4º Trimestre Regra: O período deve estar compreendido entre a data início e data fim da escrituração. Regra: SE 0010.FORMA_APUR = “A” - Deve existir um registro A00. - Deve existir um registro [A01..A012] para cada mês marcado no 0010.MES_BAL_RED [1..12] como “B” SE 0010.FORMA_APUR = “T” - Deve existir um registro [T01..T04] para cada trimestre marcado no 0010.FORMA_TRIB_PER[1..4] como “R”</summary>
    [CampoSped(Ordem = 4, Tamanho = 3, Obrigatorio = true)]
    public string? PerApur { get; set; }
}
