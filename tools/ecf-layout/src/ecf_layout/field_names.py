"""Aplica o nome normativo do manifesto como alias `Nome` em cada [CampoSped]."""

from __future__ import annotations

import json
import re
from pathlib import Path

_CAMPO = re.compile(
    r"\[CampoSped\((?P<args>[^\]]*?)\)\]\s*\n(?P<indent>\s*)public\s",
    re.MULTILINE,
)
_ORDEM = re.compile(r"\bOrdem\s*=\s*(?P<ordem>\d+)")
_NOME = re.compile(r",?\s*Nome\s*=\s*\"[^\"]*\"")


def nomes_por_ordem(manifesto: Path) -> dict[str, dict[int, str]]:
    """Mapeia código de registro -> {número do campo: nome normativo}."""
    dados = json.loads(manifesto.read_text(encoding="utf-8"))
    return {
        registro["code"]: {
            campo["number"]: campo["name"]
            for campo in registro["fields"]
            if campo["number"] != 1  # REG não recebe atributo
        }
        for registro in dados
    }


def aplicar(fonte: str, nomes: dict[int, str]) -> str:
    """Reescreve os atributos [CampoSped] da fonte com o alias normativo."""

    def substituir(match: re.Match[str]) -> str:
        args = match.group("args")
        ordem_match = _ORDEM.search(args)
        if ordem_match is None:
            return match.group(0)
        ordem = int(ordem_match.group("ordem"))
        nome = nomes.get(ordem)
        if nome is None:
            return match.group(0)
        limpo = _NOME.sub("", args).strip().rstrip(",")
        return f'[CampoSped({limpo}, Nome = "{nome}")]\n{match.group("indent")}public '

    return _CAMPO.sub(substituir, fonte)
