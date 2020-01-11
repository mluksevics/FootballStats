.output wipe.sql
.print BEGIN TRANSACTION;
SELECT 'DELETE FROM ' || name
FROM StatsDB
WHERE type = 'table';
.print COMMIT;
.print VACUUM;
.output
-- .read wipe.sql