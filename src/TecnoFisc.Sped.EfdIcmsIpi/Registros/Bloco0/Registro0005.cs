using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco0;

/// <summary>
/// Registro 0005 — Dados Complementares da Entidade.
/// Nível hierárquico 2, ocorrência única por arquivo. Conforme Guia Prático EFD-ICMS/IPI
/// V3.0.6, p. 29.
/// </summary>
[RegistroSped(Codigo = "0005", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0005 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0005";

    /// <summary>Nome de fantasia associado ao nome empresarial.</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? Fantasia { get; set; }

    /// <summary>Código de Endereçamento Postal (8 dígitos, sem formatação).</summary>
    [CampoSped(Ordem = 3, Tamanho = 8, Obrigatorio = true)]
    public string? Cep { get; set; }

    /// <summary>Logradouro e endereço do imóvel.</summary>
    [CampoSped(Ordem = 4, Tamanho = 60, Obrigatorio = true)]
    public string? End { get; set; }

    /// <summary>Número do imóvel.</summary>
    [CampoSped(Ordem = 5, Tamanho = 10)]
    public string? Num { get; set; }

    /// <summary>Dados complementares do endereço.</summary>
    [CampoSped(Ordem = 6, Tamanho = 60)]
    public string? Compl { get; set; }

    /// <summary>Bairro em que o imóvel está situado.</summary>
    [CampoSped(Ordem = 7, Tamanho = 60, Obrigatorio = true)]
    public string? Bairro { get; set; }

    /// <summary>Número do telefone (DDD+FONE).</summary>
    [CampoSped(Ordem = 8, Tamanho = 11)]
    public string? Fone { get; set; }

    /// <summary>Número do fax.</summary>
    [CampoSped(Ordem = 9, Tamanho = 11)]
    public string? Fax { get; set; }

    /// <summary>Endereço do correio eletrônico.</summary>
    [CampoSped(Ordem = 10, Tamanho = 0)]
    public string? Email { get; set; }
}
