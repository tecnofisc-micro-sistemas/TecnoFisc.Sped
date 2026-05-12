using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoB;

/// <summary>
/// Registro B420 — Totalização dos Valores de Serviços Prestados por Combinação de Alíquota
/// e Item da Lista de Serviços da LC 116/2003.
/// Nível hierárquico 2, ocorrência vários (por arquivo).
/// Conforme Guia Prático EFD-ICMS/IPI V3.2.2, p. 55-56.
/// </summary>
[RegistroSped(Codigo = "B420", Nivel = 2, Bloco = "B")]
public sealed partial class RegistroB420 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "B420";

    /// <summary>Totalização do Valor Contábil das prestações do declarante referente à combinação da alíquota e item da lista.</summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCont { get; set; }

    /// <summary>Totalização do Valor da base de cálculo do ISS das prestações do declarante referente à combinação da alíquota e item da lista.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlBcIss { get; set; }

    /// <summary>Alíquota do ISS. Máximo 5%.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal AliqIss { get; set; }

    /// <summary>Totalização do valor das operações isentas ou não-tributadas pelo ISS referente à combinação da alíquota e item da lista.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIsntIss { get; set; }

    /// <summary>Totalização, por combinação da alíquota e item da lista, do Valor do ISS.</summary>
    [CampoSped(Ordem = 6, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlIss { get; set; }

    /// <summary>Item da lista de serviços, conforme Tabela 4.6.3.</summary>
    [CampoSped(Ordem = 7, Tamanho = 0, Obrigatorio = true)]
    public string? CodServ { get; set; }
}
