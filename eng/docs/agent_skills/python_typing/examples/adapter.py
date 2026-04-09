from __future__ import annotations

from dataclasses import dataclass
from typing import Any

from ...dictionary.implementation.TL_CORE import (
    ApiError,
    CallContext,
    ErrorCategory,
    Result,
)
from ...dictionary.implementation.TL_TOOLS_ADAPTER import FSReadOptions, ReadTextResult
from ...interfaces.IF_SYS_TOOL_BUS import FsPort


class McpClient:
    def invoke(self, *, server: str, tool: str, arguments: dict[str, Any]) -> dict[str, Any]:
        raise NotImplementedError


@dataclass(frozen=True, slots=True)
class FsReadAdapterConfig:
    default_server: str
    request_timeout_ms: int = 30_000


class McpFsReadTextAdapter(FsPort):
    """
    Terminal outbound adapter that realizes part of the canonical FsPort
    contract through an MCP transport client.

    This adapter owns request mapping, transport invocation, response mapping,
    and error translation. If the canonical FsPort contract also declares
    boundary policy, wiring may wrap this terminal adapter in a guard:

        caller -> guard -> adapter
    """

    def __init__(self, *, client: McpClient, cfg: FsReadAdapterConfig) -> None:
        self._client = client
        self._cfg = cfg

    def _error(
        self,
        *,
        category: ErrorCategory,
        code: str,
        retryable: bool,
        message: str,
    ) -> Result[ReadTextResult]:
        return Result(
            outcome="err",
            err=ApiError(
                category=category,
                code=code,
                retryable=retryable,
                message=message,
            ),
        )

    def read_text(
        self,
        ctx: CallContext,
        path: str,
        opts: FSReadOptions | None = None,
    ) -> Result[ReadTextResult]:
        request = {
            "path": path,
            "encoding": (opts or FSReadOptions()).encoding,
            "correlation_ref": ctx.correlation_ref,
            "timeout_ms": self._cfg.request_timeout_ms,
        }

        try:
            response = self._client.invoke(
                server=self._cfg.default_server,
                tool="fs.read_text",
                arguments=request,
            )
        except TimeoutError:
            return self._error(
                category=ErrorCategory.DEPENDENCY,
                code="MCP_INVOKE_FAILED",
                retryable=True,
                message="Timed out while invoking fs.read_text over MCP.",
            )

        if not response.get("ok", False):
            return self._error(
                category=ErrorCategory.DEPENDENCY,
                code="FS_READ_FAILED",
                retryable=False,
                message=str(response.get("error", "fs.read_text failed")),
            )

        mapped = ReadTextResult(
            text=str(response["text"]),
            etag=response.get("etag"),
            content_type=response.get("content_type"),
        )
        return Result(outcome="ok", ok=mapped)
