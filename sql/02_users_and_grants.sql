-- MySQL 8+
-- DB users and privileges (as required by the assignment)
-- Notes:
-- 1) Replace passwords before running.
-- 2) Host is set to 'localhost'. If you use Docker/remote client, change to '%'.

USE gymdb;

CREATE USER IF NOT EXISTS 'gym_admin_principal'@'localhost' IDENTIFIED BY 'ChangeMe_AdminPrincipal!';
CREATE USER IF NOT EXISTS 'gym_admin_secondaire'@'localhost' IDENTIFIED BY 'ChangeMe_AdminSecondaire!';
CREATE USER IF NOT EXISTS 'gym_app_membre'@'localhost' IDENTIFIED BY 'ChangeMe_Membre!';
CREATE USER IF NOT EXISTS 'gym_reporting'@'localhost' IDENTIFIED BY 'ChangeMe_Reporting!';

-- Admin principal: full rights on the schema (level 1 admin)
GRANT ALL PRIVILEGES ON gymdb.* TO 'gym_admin_principal'@'localhost';

-- Admin secondaire: CRUD on tables, no schema change (level 2 admin)
GRANT SELECT, INSERT, UPDATE, DELETE ON gymdb.* TO 'gym_admin_secondaire'@'localhost';

-- Member app account: limited write access (row-level restrictions are enforced in C#)
GRANT SELECT ON gymdb.SALLE TO 'gym_app_membre'@'localhost';
GRANT SELECT ON gymdb.TYPE_COURS TO 'gym_app_membre'@'localhost';
GRANT SELECT ON gymdb.COACH TO 'gym_app_membre'@'localhost';
GRANT SELECT ON gymdb.SEANCE TO 'gym_app_membre'@'localhost';
GRANT SELECT ON gymdb.RESERVATION TO 'gym_app_membre'@'localhost';
GRANT SELECT ON gymdb.ADHESION TO 'gym_app_membre'@'localhost';

GRANT INSERT ON gymdb.MEMBRE TO 'gym_app_membre'@'localhost';
GRANT INSERT ON gymdb.ADHESION TO 'gym_app_membre'@'localhost';
GRANT INSERT, UPDATE ON gymdb.RESERVATION TO 'gym_app_membre'@'localhost';
GRANT UPDATE ON gymdb.MEMBRE TO 'gym_app_membre'@'localhost';

-- Reporting account: read-only access (evaluation/statistics)
GRANT SELECT ON gymdb.* TO 'gym_reporting'@'localhost';

FLUSH PRIVILEGES;
