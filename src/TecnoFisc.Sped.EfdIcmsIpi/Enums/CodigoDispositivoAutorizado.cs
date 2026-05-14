using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Codigo do dispositivo autorizado para emissao de documentos fiscais, campo COD_DISP do Registro 1700.
/// </summary>
public enum CodigoDispositivoAutorizado
{
    /// <summary>00 - Formulario de Seguranca - impressor autonomo.</summary>
    [SpedValor("00")]
    FormularioSegurancaImpressorAutonomo = 0,

    /// <summary>01 - FS-DA - Formulario de Seguranca para Impressao de DANFE.</summary>
    [SpedValor("01")]
    FsDa = 1,

    /// <summary>02 - Formulario de seguranca - NF-e.</summary>
    [SpedValor("02")]
    FormularioSegurancaNfe = 2,

    /// <summary>03 - Formulario continuo.</summary>
    [SpedValor("03")]
    FormularioContinuo = 3,

    /// <summary>04 - Blocos.</summary>
    [SpedValor("04")]
    Blocos = 4,

    /// <summary>05 - Jogos soltos.</summary>
    [SpedValor("05")]
    JogosSoltos = 5,
}
