using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Projeto associado ao benefício fiscal informado no X280.</summary>
public enum ProjetoIncentivado
{
    [SpedValor("00")]
    NaoPreenchido = 0,

    [SpedValor("01")]
    NovoEmpreendimento = 1,

    [SpedValor("02")]
    Modernizacao = 2,

    [SpedValor("03")]
    Ampliacao = 3,

    [SpedValor("04")]
    Diversificacao = 4,

    [SpedValor("05")]
    ManutencaoEmpreendimento = 5,

    [SpedValor("06")]
    Prouni = 6,

    [SpedValor("07")]
    Padis = 7,

    [SpedValor("08")]
    EventosFifa = 8,

    [SpedValor("09")]
    ServicosFifa = 9,

    [SpedValor("10")]
    EventosCio = 10,

    [SpedValor("11")]
    ServicosCio = 11,

    [SpedValor("99")]
    Outros = 99,
}
