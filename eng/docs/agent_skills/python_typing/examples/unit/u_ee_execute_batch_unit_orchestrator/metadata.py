UNIT_SPEC = {
    "unit_id": "U_EE_EXECUTE_BATCH_UNIT_ORCHESTRATOR",
    "component_ref": "ExecutionEngine",
    "provides": [
        "IF_CMP_AGENT_RUNTIME_EXECUTION_ENGINE",
    ],
    "consumes": [
        "IF_U_EE_RUNTIME_STORE_CLIENT",
        "IF_U_EE_ARTIFACT_CLIENT",
        "IF_U_EE_CORE_CLIENT",
        "IF_U_EE_AGENT_EXECUTOR",
        "IF_U_EE_PATCH_PIPELINE",
        "IF_U_EE_EVIDENCE_CLIENT",
        "IF_U_EE_OBS_CLIENT",
    ],
    "scenario_refs": [
        "S_SYS_EXECUTE_BATCH_EXECUTION_UNIT",
        "S_CMP_AGENT_RUNTIME_RUN_TASK",
        "S_U_EE_RUN_TASK",
    ],
    "activity_refs": [
        "A_U_EE_EXECUTE_BATCH_UNIT_FLOW",
    ],
    "cm_binding_ref": "CM_U_EE_EXECUTE_BATCH_UNIT_ORCHESTRATOR",
}
