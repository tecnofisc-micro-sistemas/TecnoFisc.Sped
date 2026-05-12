using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.EfdIcmsIpi.Enums;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoC;

/// <summary>
/// Registro C115 — Local de Coleta e/ou Entrega (código 01, 1B e 04).
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 70.
/// </summary>
[RegistroSped(Codigo = "C115", Nivel = 4, Bloco = "C")]
public sealed partial class RegistroC115 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C115";

    /// <summary>Indicador do tipo de transporte (modal de carga).</summary>
    [CampoSped(Ordem = 2, Tamanho = 1)]
    public IndicadorTipoCarga? IndCarga { get; set; }

    /// <summary>CNPJ do contribuinte do local de coleta (14 dígitos, sem máscara).</summary>
    [CampoSped(Ordem = 3, Tamanho = 14)]
    public Cnpj? CnpjCol { get; set; }

    /// <summary>Inscrição Estadual do contribuinte do local de coleta (até 14 caracteres).</summary>
    [CampoSped(Ordem = 4, Tamanho = 14)]
    public InscricaoEstadual? IeCol { get; set; }

    /// <summary>CPF do contribuinte do local de coleta (11 dígitos, sem máscara).</summary>
    [CampoSped(Ordem = 5, Tamanho = 11)]
    public Cpf? CpfCol { get; set; }

    /// <summary>Código do município do local de coleta, conforme tabela IBGE (7 dígitos).</summary>
    [CampoSped(Ordem = 6, Tamanho = 7)]
    public string? CodMunCol { get; set; }

    /// <summary>CNPJ do contribuinte do local de entrega (14 dígitos, sem máscara).</summary>
    [CampoSped(Ordem = 7, Tamanho = 14)]
    public Cnpj? CnpjEntg { get; set; }

    /// <summary>Inscrição Estadual do contribuinte do local de entrega (até 14 caracteres).</summary>
    [CampoSped(Ordem = 8, Tamanho = 14)]
    public InscricaoEstadual? IeEntg { get; set; }

    /// <summary>CPF do contribuinte do local de entrega (11 dígitos, sem máscara).</summary>
    [CampoSped(Ordem = 9, Tamanho = 11)]
    public Cpf? CpfEntg { get; set; }

    /// <summary>Código do município do local de entrega, conforme tabela IBGE (7 dígitos).</summary>
    [CampoSped(Ordem = 10, Tamanho = 7)]
    public string? CodMunEntg { get; set; }
}
