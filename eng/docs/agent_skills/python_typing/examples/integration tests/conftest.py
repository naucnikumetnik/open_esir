from __future__ import annotations


def pytest_configure(config) -> None:
    markers = [
        "unt_int: unit-level integration test",
        "cmp_int: component-level integration test",
        "sys_int: system-level integration test",
        "sos_int: system-of-systems integration test",
        "connector_interaction: connector interaction test family",
        "protocol_path: protocol/path test family",
        "failure_propagation: failure propagation test family",
        "guard_behavior: guard behavior test family",
        "adapter_behavior: adapter behavior test family",
        "assembly_wiring_smoke: assembly/wiring smoke test family",
        "integrated_requirement_oriented: integrated requirement-oriented test family",
        "smoke: smoke-style execution marker",
    ]
    for marker in markers:
        config.addinivalue_line("markers", marker)
