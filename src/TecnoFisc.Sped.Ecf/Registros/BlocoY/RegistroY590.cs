using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoY;

/// <summary>Registro Y590 - ativos no exterior.</summary>
[RegistroSped(Codigo = "Y590", Nivel = 2, Bloco = "Y")]
public sealed partial class RegistroY590 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "Y590";

    /// <summary>Tipo do ativo conforme tabela dinâmica do Sped.</summary>
    [CampoSped(Ordem = 2, Tamanho = 14, Obrigatorio = true)]
    public string? TipAtivo { get; set; }

    /// <summary>Código de país preservado lexicalmente.</summary>
    [CampoSped(Ordem = 3, Tamanho = 3, Obrigatorio = true)]
    public string? Pais { get; set; }

    [CampoSped(Ordem = 4, Obrigatorio = true)]
    public string? Discriminacao { get; set; }

    [CampoSped(Ordem = 5, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlAnt { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlAtual { get; set; }
}
