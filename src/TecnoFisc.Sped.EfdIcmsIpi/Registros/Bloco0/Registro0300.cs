using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco0;

/// <summary>
/// Registro 0300 — Cadastro de Bens ou Componentes do Ativo Imobilizado. Nível hierárquico 2,
/// ocorrência vários por arquivo. Identifica e caracteriza todos os bens ou componentes
/// arrolados no Registro G125 do Bloco G e os bens em construção. Conforme Guia Prático
/// EFD-ICMS/IPI V3.0.6, p. 39.
/// </summary>
[RegistroSped(Codigo = "0300", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0300 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0300";

    /// <summary>Código individualizado do bem ou componente adotado no controle patrimonial do estabelecimento informante.</summary>
    [CampoSped(Ordem = 2, Tamanho = 60, Obrigatorio = true)]
    public string? CodIndBem { get; set; }

    /// <summary>Identificação do tipo de mercadoria: 1 = bem; 2 = componente.</summary>
    [CampoSped(Ordem = 3, Tamanho = 1, Obrigatorio = true)]
    public IdentificadorMercadoriaAtivo IdentMerc { get; set; }

    /// <summary>Descrição do bem ou componente (modelo, marca e outras características necessárias à sua individualização).</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Obrigatorio = true)]
    public string? DescrItem { get; set; }

    /// <summary>Código de cadastro do bem principal nos casos em que o bem ou componente esteja vinculado a um bem principal.</summary>
    [CampoSped(Ordem = 5, Tamanho = 60)]
    public string? CodPrnc { get; set; }

    /// <summary>Código da conta analítica de contabilização do bem ou componente (campo 06 do Registro 0500).</summary>
    [CampoSped(Ordem = 6, Tamanho = 60, Obrigatorio = true)]
    public string? CodCta { get; set; }

    /// <summary>Número total de parcelas a serem apropriadas segundo a legislação de cada unidade federada.</summary>
    [CampoSped(Ordem = 7, Tamanho = 3)]
    public int? NrParc { get; set; }
}
