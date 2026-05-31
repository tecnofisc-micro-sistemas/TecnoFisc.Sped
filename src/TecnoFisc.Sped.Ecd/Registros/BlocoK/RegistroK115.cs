using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Ecd.Enums;

namespace TecnoFisc.Sped.Ecd.Registros.BlocoK;

/// <summary>
/// Registro K115 — Empresas Participantes do Evento Societário. Nível hierárquico 5, ocorrência 0:N
/// por arquivo. Demonstra a relação das empresas participantes dos eventos societários informados
/// no registro K110.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 218–219.
/// </summary>
/// <remarks>
/// Regras de validação são responsabilidade do consumidor (ARCHITECTURE §2.3):
/// <list type="bullet">
///   <item><c>REGRA_EXISTE_EMP_COD_K100</c>: <see cref="EmpCodPart"/> deve corresponder ao
///   campo <c>EMP_COD</c> de algum registro K100 informado no arquivo.</item>
///   <item><c>REGRA_CONDICAO_COMPATIVEL</c>: a condição da empresa (<see cref="CondPart"/>) deve
///   ser compatível com o tipo de evento registrado em K110 (<c>EVENTO</c>):
///   <list type="bullet">
///     <item><see cref="CondicaoParticipanteEvento.Sucessora"/> (1) → K110 deve ser Fusão (3),
///     Cisão Parcial (4), Cisão Total (5) ou Incorporação (6).</item>
///     <item><see cref="CondicaoParticipanteEvento.Adquirente"/> (2) → K110 deve ser Aquisição (1).</item>
///     <item><see cref="CondicaoParticipanteEvento.Alienante"/> (3) → K110 deve ser Alienação (2).</item>
///   </list></item>
///   <item><c>REGRA_PERC_MENOR_IGUAL_100</c>: o somatório de <see cref="PerEvt"/> de todos os K115
///   filhos de um mesmo K110 deve ser menor ou igual a 100.</item>
/// </list>
/// </remarks>
[RegistroSped(Codigo = "K115", Nivel = 5, Bloco = "K")]
public sealed partial class RegistroK115 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "K115";

    /// <summary>
    /// Código da empresa envolvida na operação, conforme informado em <c>EMP_COD</c> do
    /// registro K100.
    /// Campo <c>EMP_COD_PART</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 4, Obrigatorio = true)]
    public int EmpCodPart { get; set; }

    /// <summary>
    /// Condição da empresa relacionada à operação societária.
    /// Campo <c>COND_PART</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public CondicaoParticipanteEvento CondPart { get; set; }

    /// <summary>
    /// Percentual da empresa participante envolvida na operação.
    /// Campo <c>PER_EVT</c>.
    /// </summary>
    [CampoSped(Ordem = 4, Tamanho = 8, Decimais = 4, Obrigatorio = true)]
    public decimal PerEvt { get; set; }
}
