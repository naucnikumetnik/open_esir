"""
bootstrap_example.py

Purpose:
    Thin process bootstrap for a Python service or CLI.

Responsibilities:
    - parse CLI args
    - load validated settings
    - configure logging
    - build runtime via wiring
    - install signal handlers
    - execute top-level command
    - close owned resources
    - return an exit code

Not responsible for:
    - business logic
    - dependency graph details
    - domain orchestration
"""

from __future__ import annotations

import argparse
import logging
import logging.config
import signal
from contextlib import ExitStack
from typing import Protocol, Sequence

from pydantic import Field
from pydantic_settings import BaseSettings, SettingsConfigDict


# ============================================================================
# SETTINGS
# ============================================================================

class AppSettings(BaseSettings):
    """
    Process-level configuration loaded from environment variables.

    Example env vars:
        AIOS_APP_NAME=aios
        AIOS_LOG_LEVEL=DEBUG
        AIOS_MODE=cli
    """

    model_config = SettingsConfigDict(
        env_prefix="AIOS_",
        extra="forbid",
    )

    app_name: str = "aios"
    log_level: str = "INFO"
    mode: str = Field(default="cli")
    graceful_shutdown_timeout_s: int = Field(default=10, ge=1, le=300)


# ============================================================================
# RUNTIME CONTRACT
# ============================================================================

class AppRuntime(Protocol):
    """
    Minimal runtime contract expected by bootstrap.
    Real implementations come from wiring.
    """

    def run(self) -> int:
        """Run the top-level command and return a process exit code."""
        ...

    def request_shutdown(self, *, reason: str) -> None:
        """Ask the runtime to stop gracefully."""
        ...

    def close(self) -> None:
        """Release owned resources."""
        ...


# ============================================================================
# DEMO RUNTIME
# Replace this with real wiring output in production.
# ============================================================================

class DemoRuntime:
    def __init__(self, *, settings: AppSettings, command: str) -> None:
        self._settings = settings
        self._command = command
        self._shutdown_requested = False
        self._logger = logging.getLogger("aios.runtime")

    def run(self) -> int:
        self._logger.info("runtime_start", extra={"command": self._command})

        if self._shutdown_requested:
            self._logger.warning("runtime_not_started_shutdown_already_requested")
            return 130

        if self._command == "healthcheck":
            self._logger.info("healthcheck_ok")
            return 0

        self._logger.info(
            "runtime_execute",
            extra={"mode": self._settings.mode, "app_name": self._settings.app_name},
        )

        # Real top-level execution would go here.
        return 0

    def request_shutdown(self, *, reason: str) -> None:
        self._shutdown_requested = True
        self._logger.warning("shutdown_requested", extra={"reason": reason})

    def close(self) -> None:
        self._logger.info("runtime_close")


def build_system_runtime(*, settings: AppSettings, command: str) -> AppRuntime:
    """
    Bootstrap must call wiring, not assemble the whole graph inline.

    Replace body with something like:
        return build_cli_runtime(settings=settings, command=command)
    """
    return DemoRuntime(settings=settings, command=command)


# ============================================================================
# CLI
# ============================================================================

def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(prog="aios")
    parser.add_argument(
        "command",
        nargs="?",
        default="run",
        choices=["run", "healthcheck"],
        help="Top-level command to execute.",
    )
    parser.add_argument(
        "--mode",
        default=None,
        choices=["cli", "service"],
        help="Override runtime mode.",
    )
    return parser.parse_args(argv)


# ============================================================================
# LOGGING
# ============================================================================

def configure_logging(settings: AppSettings) -> None:
    logging.config.dictConfig(
        {
            "version": 1,
            "disable_existing_loggers": False,
            "formatters": {
                "standard": {
                    "format": "%(asctime)s %(levelname)s %(name)s %(message)s",
                },
            },
            "handlers": {
                "console": {
                    "class": "logging.StreamHandler",
                    "formatter": "standard",
                },
            },
            "root": {
                "level": settings.log_level,
                "handlers": ["console"],
            },
        }
    )


# ============================================================================
# SIGNALS
# ============================================================================

def install_signal_handlers(runtime: AppRuntime) -> None:
    def _handle_shutdown(signum: int, _frame) -> None:
        runtime.request_shutdown(reason=f"signal:{signum}")

    signal.signal(signal.SIGINT, _handle_shutdown)
    signal.signal(signal.SIGTERM, _handle_shutdown)


# ============================================================================
# MAIN
# ============================================================================

def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    settings = AppSettings()

    if args.mode is not None:
        settings = settings.model_copy(update={"mode": args.mode})

    configure_logging(settings)
    logger = logging.getLogger("aios.bootstrap")

    try:
        with ExitStack() as stack:
            runtime = build_system_runtime(
                settings=settings,
                command=args.command,
            )
            stack.callback(runtime.close)

            install_signal_handlers(runtime)

            logger.info(
                "startup_complete",
                extra={
                    "app_name": settings.app_name,
                    "mode": settings.mode,
                    "command": args.command,
                },
            )

            exit_code = runtime.run()

            logger.info("shutdown_complete", extra={"exit_code": exit_code})
            return exit_code

    except KeyboardInterrupt:
        logger.warning("interrupted")
        return 130
    except Exception:
        logger.exception("fatal_bootstrap_error")
        return 1


if __name__ == "__main__":
    raise SystemExit(main())