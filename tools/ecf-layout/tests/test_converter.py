from pathlib import Path
from subprocess import CompletedProcess

import pytest

from ecf_layout.converter import EmptyMarkdownError, convert_page


def test_converter_rejects_empty_markdown(tmp_path: Path) -> None:
    pdf = tmp_path / "manual.pdf"
    pdf.write_bytes(b"manual")

    def run_empty(*_args: object, **_kwargs: object) -> CompletedProcess[bytes]:
        return CompletedProcess([], 0, b"", b"")

    with pytest.raises(EmptyMarkdownError):
        convert_page(pdf, 58, run=run_empty)
