namespace TecnoFisc.Sped.Core.Enums;

/// <summary>Origem da mercadoria — campo orig do grupo ICMS (item).</summary>
public enum OrigemMercadoria
{
    /// <summary>0 — Nacional, exceto as dos códigos 3 a 5.</summary>
    Nacional = 0,

    /// <summary>1 — Estrangeira, importação direta, exceto código 6.</summary>
    EstrangeiraImportacaoDireta = 1,

    /// <summary>2 — Estrangeira, adquirida no mercado interno, exceto código 7.</summary>
    EstrangeiraMercadoInterno = 2,

    /// <summary>3 — Nacional, conteúdo de importação entre 40% e 70%.</summary>
    NacionalConteudoImportacaoAte70 = 3,

    /// <summary>4 — Nacional, produção conforme processos produtivos básicos.</summary>
    NacionalProcessosProdutivosBasicos = 4,

    /// <summary>5 — Nacional, conteúdo de importação inferior a 40%.</summary>
    NacionalConteudoImportacaoAte40 = 5,

    /// <summary>6 — Estrangeira, importação direta, sem similar nacional (lista CAMEX/gás).</summary>
    EstrangeiraImportacaoDiretaSemSimilar = 6,

    /// <summary>7 — Estrangeira, mercado interno, sem similar nacional (lista CAMEX/gás).</summary>
    EstrangeiraMercadoInternoSemSimilar = 7,

    /// <summary>8 — Nacional, conteúdo de importação superior a 70%.</summary>
    NacionalConteudoImportacaoSuperior70 = 8,
}
