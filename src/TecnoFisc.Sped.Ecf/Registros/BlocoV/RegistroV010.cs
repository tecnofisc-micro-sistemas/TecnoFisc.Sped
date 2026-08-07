using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoV;

/// <summary>Registro V010 - instituição financeira da declaração DEREX.</summary>
[RegistroSped(Codigo = "V010", Nivel = 2, Bloco = "V")]
public sealed partial class RegistroV010 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "V010";

    /// <summary>Nome da instituição.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true, Nome = "NOME_INSTITUICAO")]
    public string? NomeInstituicao { get; set; }

    /// <summary>Código do país conforme a tabela dinâmica aplicável.</summary>
    [CampoSped(Ordem = 3, Tamanho = 2, Obrigatorio = true, Nome = "PAIS")]
    public string? Pais { get; set; }

    /// <summary>Código da moeda conforme a tabela dinâmica aplicável.</summary>
    [CampoSped(Ordem = 4, Obrigatorio = true, Nome = "TIP_MOEDA")]
    public string? TipMoeda { get; set; }
}
