"""
production_config_example.py

Single-file example showing a production-config artifact for Python:
- typed deploy-time settings
- nested config by concern
- env-based loading
- optional CLI overrides
- logging dictConfig builder
- fail-fast validation
- narrow derived config slices for wiring

Environment variable style:
- AIOS_APP__NAME=aios
- AIOS_APP__ENV=prod
- AIOS_RUNTIME__BIND_HOST=127.0.0.1
- AIOS_RUNTIME__BIND_PORT=8080
- AIOS_STORAGE__ROOT=/srv/aios
- AIOS_PROVIDERS__LLM__MODE=cloud
- AIOS_PROVIDERS__LLM__BASE_URL=https://api.example.com/v1
- AIOS_PROVIDERS__LLM__API_KEY=secret
- AIOS_LOGGING__LEVEL=INFO
"""

from __future__ import annotations

import argparse
import logging
import logging.config
from dataclasses import dataclass
from enum import StrEnum
from pathlib import Path
from typing import Any

from pydantic import (
    BaseModel,
    ConfigDict,
    Field,
    SecretStr,
    ValidationError,
    model_validator,
)
from pydantic.networks import AnyHttpUrl
from pydantic_settings import BaseSettings, SettingsConfigDict
from typing_extensions import Annotated


# ============================================================================
# ENUMS
# ============================================================================


class AppEnv(StrEnum):
    DEV = "dev"
    TEST = "test"
    PROD = "prod"


class LogLevel(StrEnum):
    DEBUG = "DEBUG"
    INFO = "INFO"
    WARNING = "WARNING"
    ERROR = "ERROR"
    CRITICAL = "CRITICAL"


class LlmMode(StrEnum):
    LOCAL = "local"
    CLOUD = "cloud"


# ============================================================================
# REUSABLE CONSTRAINED TYPES
# ============================================================================

Port = Annotated[int, Field(ge=1, le=65535)]
PositiveFloat = Annotated[float, Field(gt=0.0)]
NonNegativeInt = Annotated[int, Field(ge=0)]


# ============================================================================
# BASE MODELS
# ============================================================================


class StrictConfigModel(BaseModel):
    model_config = ConfigDict(extra="forbid", frozen=True)


# ============================================================================
# NESTED SETTINGS MODELS
# ============================================================================


class AppSettings(StrictConfigModel):
    name: str = "aios"
    env: AppEnv = AppEnv.DEV
    debug: bool = False


class RuntimeSettings(StrictConfigModel):
    bind_host: str = "127.0.0.1"
    bind_port: Port = 8000
    worker_count: Annotated[int, Field(ge=1)] = 1
    request_timeout_s: PositiveFloat = 30.0


class StorageSettings(StrictConfigModel):
    root: Path
    evidence_dir_name: str = "evidence"
    telemetry_dir_name: str = "telemetry"

    @property
    def evidence_root(self) -> Path:
        return self.root / self.evidence_dir_name

    @property
    def telemetry_root(self) -> Path:
        return self.root / self.telemetry_dir_name


class LlmProviderSettings(StrictConfigModel):
    mode: LlmMode = LlmMode.LOCAL
    base_url: AnyHttpUrl | None = None
    api_key: SecretStr | None = None
    model_name: str | None = None
    connect_timeout_s: PositiveFloat = 10.0
    read_timeout_s: PositiveFloat = 120.0
    max_retry_attempts: NonNegativeInt = 2

    @model_validator(mode="after")
    def validate_mode_requirements(self) -> "LlmProviderSettings":
        if self.mode == LlmMode.CLOUD:
            if self.base_url is None:
                raise ValueError("providers.llm.base_url is required in cloud mode")
            if self.api_key is None:
                raise ValueError("providers.llm.api_key is required in cloud mode")
        if self.mode == LlmMode.LOCAL:
            if self.base_url is None:
                raise ValueError("providers.llm.base_url is required in local mode")
        return self


class ProviderSettings(StrictConfigModel):
    llm: LlmProviderSettings


class LoggingSettings(StrictConfigModel):
    level: LogLevel = LogLevel.INFO
    json: bool = False
    logger_name: str = "aios"
    include_timestamps: bool = True


class FeatureFlags(StrictConfigModel):
    enable_progress_streaming: bool = True
    enable_cloud_generation: bool = False


# ============================================================================
# ROOT SETTINGS
# ============================================================================


class Settings(BaseSettings):
    """
    Root deploy-time settings object.

    Values are loaded from environment variables using:
    - prefix: AIOS_
    - nested delimiter: __
    """

    model_config = SettingsConfigDict(
        env_prefix="AIOS_",
        env_nested_delimiter="__",
        extra="forbid",
    )

    app: AppSettings = AppSettings()
    runtime: RuntimeSettings = RuntimeSettings()
    storage: StorageSettings
    providers: ProviderSettings
    logging: LoggingSettings = LoggingSettings()
    features: FeatureFlags = FeatureFlags()

    @model_validator(mode="after")
    def validate_cross_section_rules(self) -> "Settings":
        if self.features.enable_cloud_generation and self.providers.llm.mode != LlmMode.CLOUD:
            raise ValueError(
                "features.enable_cloud_generation=true requires providers.llm.mode=cloud"
            )
        return self


# ============================================================================
# DERIVED NARROW CONFIG SLICES FOR WIRING
# ============================================================================


@dataclass(frozen=True, slots=True)
class HttpServerConfig:
    host: str
    port: int
    worker_count: int


@dataclass(frozen=True, slots=True)
class FileStoreConfig:
    root: Path
    evidence_root: Path
    telemetry_root: Path


@dataclass(frozen=True, slots=True)
class LlmClientConfig:
    mode: LlmMode
    base_url: str
    api_key: str | None
    model_name: str | None
    connect_timeout_s: float
    read_timeout_s: float
    max_retry_attempts: int


def derive_http_server_config(settings: Settings) -> HttpServerConfig:
    return HttpServerConfig(
        host=settings.runtime.bind_host,
        port=settings.runtime.bind_port,
        worker_count=settings.runtime.worker_count,
    )


def derive_file_store_config(settings: Settings) -> FileStoreConfig:
    return FileStoreConfig(
        root=settings.storage.root,
        evidence_root=settings.storage.evidence_root,
        telemetry_root=settings.storage.telemetry_root,
    )


def derive_llm_client_config(settings: Settings) -> LlmClientConfig:
    return LlmClientConfig(
        mode=settings.providers.llm.mode,
        base_url=str(settings.providers.llm.base_url),
        api_key=(
            settings.providers.llm.api_key.get_secret_value()
            if settings.providers.llm.api_key is not None
            else None
        ),
        model_name=settings.providers.llm.model_name,
        connect_timeout_s=settings.providers.llm.connect_timeout_s,
        read_timeout_s=settings.providers.llm.read_timeout_s,
        max_retry_attempts=settings.providers.llm.max_retry_attempts,
    )


# ============================================================================
# LOGGING CONFIG
# ============================================================================


def build_logging_dict(settings: Settings) -> dict[str, Any]:
    format_string = (
        "%(asctime)s %(levelname)s %(name)s %(message)s"
        if settings.logging.include_timestamps
        else "%(levelname)s %(name)s %(message)s"
    )

    if settings.logging.json:
        # Kept simple here. Real project may use a JSON formatter class.
        format_string = '{"ts":"%(asctime)s","lvl":"%(levelname)s","logger":"%(name)s","msg":"%(message)s"}'

    return {
        "version": 1,
        "disable_existing_loggers": False,
        "formatters": {
            "default": {
                "format": format_string,
            }
        },
        "handlers": {
            "console": {
                "class": "logging.StreamHandler",
                "level": settings.logging.level.value,
                "formatter": "default",
            }
        },
        "loggers": {
            settings.logging.logger_name: {
                "handlers": ["console"],
                "level": settings.logging.level.value,
                "propagate": False,
            }
        },
        "root": {
            "handlers": ["console"],
            "level": settings.logging.level.value,
        },
    }


def configure_logging(settings: Settings) -> None:
    logging.config.dictConfig(build_logging_dict(settings))


# ============================================================================
# CLI OVERRIDES
# ============================================================================


class CliOverrides(StrictConfigModel):
    bind_host: str | None = None
    bind_port: Port | None = None
    log_level: LogLevel | None = None
    check_config: bool = False


def parse_cli_args() -> CliOverrides:
    parser = argparse.ArgumentParser(description="AIOS process bootstrap")
    parser.add_argument("--bind-host", type=str, default=None)
    parser.add_argument("--bind-port", type=int, default=None)
    parser.add_argument("--log-level", type=str, choices=[x.value for x in LogLevel], default=None)
    parser.add_argument("--check-config", action="store_true")
    ns = parser.parse_args()

    return CliOverrides(
        bind_host=ns.bind_host,
        bind_port=ns.bind_port,
        log_level=LogLevel(ns.log_level) if ns.log_level else None,
        check_config=ns.check_config,
    )


def apply_cli_overrides(settings: Settings, overrides: CliOverrides) -> Settings:
    update: dict[str, Any] = {}

    if overrides.bind_host is not None or overrides.bind_port is not None:
        update["runtime"] = settings.runtime.model_copy(
            update={
                "bind_host": overrides.bind_host if overrides.bind_host is not None else settings.runtime.bind_host,
                "bind_port": overrides.bind_port if overrides.bind_port is not None else settings.runtime.bind_port,
            }
        )

    if overrides.log_level is not None:
        update["logging"] = settings.logging.model_copy(
            update={"level": overrides.log_level}
        )

    if not update:
        return settings

    return settings.model_copy(update=update)


# ============================================================================
# LOADING ENTRYPOINT
# ============================================================================


def load_settings() -> Settings:
    """
    Load and validate settings from environment.
    Fail fast if invalid.
    """
    return Settings()


# ============================================================================
# DEMO BOOTSTRAP / MAIN
# ============================================================================


def main() -> int:
    try:
        settings = load_settings()
        overrides = parse_cli_args()
        settings = apply_cli_overrides(settings, overrides)
        configure_logging(settings)

        logger = logging.getLogger(settings.logging.logger_name)

        if overrides.check_config:
            logger.info("Configuration valid")
            logger.info("App=%s env=%s", settings.app.name, settings.app.env.value)
            logger.info(
                "Bind=%s:%s",
                settings.runtime.bind_host,
                settings.runtime.bind_port,
            )
            logger.info("Storage root=%s", settings.storage.root)
            logger.info("LLM mode=%s", settings.providers.llm.mode.value)
            return 0

        http_cfg = derive_http_server_config(settings)
        fs_cfg = derive_file_store_config(settings)
        llm_cfg = derive_llm_client_config(settings)

        logger.info(
            "Starting %s in %s on %s:%s",
            settings.app.name,
            settings.app.env.value,
            http_cfg.host,
            http_cfg.port,
        )
        logger.info("Evidence root=%s", fs_cfg.evidence_root)
        logger.info("Telemetry root=%s", fs_cfg.telemetry_root)
        logger.info("LLM mode=%s model=%s", llm_cfg.mode.value, llm_cfg.model_name)

        # Here bootstrap would pass narrow config slices into wiring:
        #
        # container = build_container(
        #     http=http_cfg,
        #     file_store=fs_cfg,
        #     llm=llm_cfg,
        # )
        # app = container.app()
        # app.run()
        #
        # This file intentionally stops at the config boundary.

        return 0

    except ValidationError as exc:
        print("Configuration validation failed:")
        print(exc)
        return 2
    except Exception as exc:
        print(f"Fatal bootstrap error: {exc}")
        return 1


if __name__ == "__main__":
    raise SystemExit(main())