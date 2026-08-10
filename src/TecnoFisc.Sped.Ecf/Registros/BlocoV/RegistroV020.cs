using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoV;

/// <summary>Registro V020 - responsável pela movimentação.</summary>
[RegistroSped(Codigo = "V020", Nivel = 3, Bloco = "V")]
public sealed partial class RegistroV020 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "V020";

    /// <summary>Nome do responsável.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true, Nome = "NOME")]
    public string? Nome { get; set; }

    /// <summary>Endereço do responsável.</summary>
    [CampoSped(Ordem = 3, Obrigatorio = true, Nome = "ENDERECO")]
    public string? Endereco { get; set; }

    /// <summary>Tipo de documento conforme a tabela dinâmica aplicável.</summary>
    [CampoSped(Ordem = 4, Obrigatorio = true, Nome = "TIPO_DO_C")]
    public string? TipoDoC { get; set; }

    /// <summary>Número de identificação do responsável, preservado sem inferir seu tipo.</summary>
    [CampoSped(Ordem = 5, Obrigatorio = true, Nome = "NI")]
    public string? Ni { get; set; }

    /// <summary>Identificação das contas movimentadas.</summary>
    [CampoSped(Ordem = 6, Obrigatorio = true, Nome = "IDENT_CONTA")]
    public string? IdentConta { get; set; }
}
