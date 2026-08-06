using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoY;

/// <summary>Registro Y520 - pagamentos e recebimentos do exterior ou de não residentes.</summary>
[RegistroSped(Codigo = "Y520", Nivel = 2, Bloco = "Y")]
public sealed partial class RegistroY520 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "Y520";

    [CampoSped(Ordem = 2, Tamanho = 1, Obrigatorio = true)]
    public TipoOperacaoExterior TipExt { get; set; }

    /// <summary>Código de país preservado lexicalmente.</summary>
    [CampoSped(Ordem = 3, Tamanho = 3, Obrigatorio = true)]
    public string? Pais { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 1, Obrigatorio = true)]
    public FormaPagamentoRecebimentoExterior Forma { get; set; }

    /// <summary>Código da natureza da operação conforme tabela dinâmica do Sped.</summary>
    [CampoSped(Ordem = 5, Tamanho = 5, Obrigatorio = true)]
    public string? NatOper { get; set; }

    [CampoSped(Ordem = 6, Tamanho = 19, Decimais = 2, Obrigatorio = true)]
    public decimal VlPeriodo { get; set; }
}
