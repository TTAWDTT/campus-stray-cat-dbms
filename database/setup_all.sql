SET DEFINE OFF;
SET SERVEROUTPUT ON;

PROMPT ===== Rebuilding campus stray cat database =====;
@@drop_tables.sql
@@create_tables.sql
@@queries/cat_photos_oracle_programming.sql
@@queries/a_group_advanced.sql
@@queries/task_17_18_19_oracle_programming.sql
@@queries/rescue_care_oracle_programming.sql
@@queries/a_group_demo_data.sql
@@insert_demo_data.sql

PROMPT ===== Campus stray cat database is ready =====;
