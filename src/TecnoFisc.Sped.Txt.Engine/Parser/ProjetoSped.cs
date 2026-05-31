namespace TecnoFisc.Sped.Txt.Engine.Parser;

/// <summary>Projeto SPED textual identificado a partir do inicio do arquivo.</summary>
public enum ProjetoSped
{
    /// <summary>Nao foi possivel identificar o projeto.</summary>
    Desconhecido = 0,

    /// <summary>EFD Contribuicoes (PIS/COFINS).</summary>
    EfdContribuicoes,

    /// <summary>EFD ICMS-IPI.</summary>
    EfdIcmsIpi,

    /// <summary>ECD - Escrituracao Contabil Digital.</summary>
    Ecd,

    /// <summary>ECF - Escrituracao Contabil Fiscal; reservado para Stage 17.</summary>
    Ecf,
}
