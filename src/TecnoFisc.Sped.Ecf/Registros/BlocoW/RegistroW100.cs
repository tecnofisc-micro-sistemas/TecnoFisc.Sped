using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoW;

/// <summary>Registro W100 - grupo multinacional e entidade declarante da DPP.</summary>
[RegistroSped(Codigo = "W100", Nivel = 2, Bloco = "W")]
public sealed partial class RegistroW100 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "W100";

    /// <summary>Nome do grupo multinacional.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public string? NomeMultinacional { get; set; }

    /// <summary>Indica se a entidade é a controladora final.</summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IndicadorSimNao IndControladora { get; set; }

    /// <summary>Nome legal da controladora final.</summary>
    [CampoSped(Ordem = 4, Obrigatorio = true)]
    public string? NomeControladora { get; set; }

    /// <summary>Jurisdição da controladora conforme tabela dinâmica do Sped.</summary>
    [CampoSped(Ordem = 5, Tamanho = 2, Obrigatorio = true)]
    public string? JurisdicaoControladora { get; set; }

    /// <summary>TIN genérico da controladora, preservado sem inferir CNPJ.</summary>
    [CampoSped(Ordem = 6, Obrigatorio = true)]
    public string? TinControladora { get; set; }

    /// <summary>Entidade responsável pela entrega da DPP.</summary>
    [CampoSped(Ordem = 7, Tamanho = 1, Obrigatorio = true)]
    public ResponsavelEntregaDpp IndEntrega { get; set; }

    /// <summary>Modalidade de entrega da DPP.</summary>
    [CampoSped(Ordem = 8, Tamanho = 1, Obrigatorio = true)]
    public ModalidadeEntregaDpp IndModalidade { get; set; }

    /// <summary>Nome da entidade substituta ou local.</summary>
    [CampoSped(Ordem = 9)]
    public string? NomeSubstituta { get; set; }

    /// <summary>Jurisdição da entidade substituta conforme tabela dinâmica do Sped.</summary>
    [CampoSped(Ordem = 10, Tamanho = 2)]
    public string? JurisdicaoSubstituta { get; set; }

    /// <summary>TIN genérico da entidade substituta, preservado sem inferir CNPJ.</summary>
    [CampoSped(Ordem = 11, Tamanho = 14)]
    public string? TinSubstituta { get; set; }

    /// <summary>Data inicial do período societário da DPP.</summary>
    [CampoSped(Ordem = 12, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtIni { get; set; }

    /// <summary>Data final do período societário da DPP.</summary>
    [CampoSped(Ordem = 13, Tamanho = 8, Formato = "ddMMyyyy")]
    public DateOnly? DtFin { get; set; }

    /// <summary>Moeda conforme tabela dinâmica do Sped.</summary>
    [CampoSped(Ordem = 14, Tamanho = 3)]
    public string? TipMoeda { get; set; }

    /// <summary>Idioma das informações adicionais.</summary>
    [CampoSped(Ordem = 15, Tamanho = 2)]
    public IdiomaDpp? IndIdioma { get; set; }
}
