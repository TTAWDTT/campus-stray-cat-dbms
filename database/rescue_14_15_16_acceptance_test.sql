SET DEFINE OFF;
SET SERVEROUTPUT ON;

PROMPT ===== Rescue 14/15/16 acceptance test =====

@database/rescue_14_15_16/10_demo_data.sql
@database/rescue_14_15_16/11_test_reminders.sql
@database/rescue_14_15_16/12_test_emergency_reports.sql
@database/rescue_14_15_16/13_test_missing_alerts.sql

PROMPT ===== Acceptance test complete =====
