"""Content-addressed storage for converted manual pages."""

from __future__ import annotations

from dataclasses import dataclass
from hashlib import sha256
from pathlib import Path


_CONVERSION_SOURCES = ("converter.py", "fixups.py")


@dataclass(frozen=True)
class CacheKey:
    pdf_sha256: str
    converter_sha256: str
    page: int

    def output_path(self, cache_dir: Path) -> Path:
        return cache_dir / self.pdf_sha256 / self.converter_sha256 / f"page-{self.page:04d}.md"


def sha256_file(path: Path) -> str:
    return sha256(path.read_bytes()).hexdigest()


def converter_fingerprint(package_dir: Path | None = None) -> str:
    """Hash only the source files that affect converted page contents."""
    source_dir = package_dir or Path(__file__).parent
    digest = sha256()
    for relative_name in _CONVERSION_SOURCES:
        source = source_dir / relative_name
        relative_path = relative_name.encode("utf-8")
        source_bytes = source.read_bytes()
        digest.update(len(relative_path).to_bytes(4, "big"))
        digest.update(relative_path)
        digest.update(len(source_bytes).to_bytes(8, "big"))
        digest.update(source_bytes)
    return digest.hexdigest()
