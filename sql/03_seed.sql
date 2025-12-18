-- MySQL 8+
-- Seed data for gymdb

USE gymdb;

-- Important: this project uses SHA-256 with a fixed salt for demo purposes.
-- The C# app uses the same salt: 'GYM_SALT_V1'
-- password_hash = SHA2(CONCAT('GYM_SALT_V1', <password>), 256)

INSERT INTO ADMINISTRATEUR (nom, prenom, email, password_hash, role)
VALUES
  ('Durand', 'Alice', 'admin.principal@gym.local', SHA2(CONCAT('GYM_SALT_V1','Admin123!'), 256), 'PRINCIPAL'),
  ('Martin', 'Benoit', 'admin.secondaire@gym.local', SHA2(CONCAT('GYM_SALT_V1','Admin123!'), 256), 'SECONDAIRE');

INSERT INTO COACH (nom, prenom, telephone, email, specialite, formations, password_hash)
VALUES
  ('Nguyen', 'Chloe', '0611223344', 'coach.chloe@gym.local', 'HIIT', 'BPJEPS + formations HIIT', SHA2(CONCAT('GYM_SALT_V1','Coach123!'), 256)),
  ('Bernard', 'David', '0622334455', 'coach.david@gym.local', 'Yoga', 'Yoga 200h + Vinyasa', SHA2(CONCAT('GYM_SALT_V1','Coach123!'), 256));

INSERT INTO SALLE (nom_salle, capacite_max_salle)
VALUES
  ('Salle A', 20),
  ('Salle B', 12);

INSERT INTO TYPE_COURS (nom_cours, description, capacite_max_cours, id_admin_createur)
VALUES
  ('HIIT Express', 'Entrainement fractionne haute intensite. Description seule (duree/intensite/niveau inclus ici).', 20, 1),
  ('Yoga Flow', 'Yoga dynamique, respiration, souplesse. Description seule.', 12, 1);

-- Two upcoming sessions
INSERT INTO SEANCE (
  date_heure_debut, date_heure_fin, capacite_max_seance, statut,
  id_coach, id_type_cours, id_salle
)
VALUES
  (DATE_ADD(NOW(), INTERVAL 1 DAY),  DATE_ADD(NOW(), INTERVAL 1 DAY + INTERVAL 45 MINUTE), 20, 'PLANIFIEE', 1, 1, 1),
  (DATE_ADD(NOW(), INTERVAL 2 DAY),  DATE_ADD(NOW(), INTERVAL 2 DAY + INTERVAL 60 MINUTE), 12, 'PLANIFIEE', 2, 2, 2);

-- Members
INSERT INTO MEMBRE (nom, prenom, adresse, telephone, email, password_hash)
VALUES
  ('Petit', 'Emma', '10 rue Exemple, Paris', '0600000001', 'emma.membre@gym.local', SHA2(CONCAT('GYM_SALT_V1','Member123!'), 256)),
  ('Leroy', 'Hugo', '20 avenue Test, Paris', '0600000002', 'hugo.membre@gym.local', SHA2(CONCAT('GYM_SALT_V1','Member123!'), 256));

-- Membership requests: one validated, one pending
INSERT INTO ADHESION (date_demande, date_validation, date_debut, date_fin, statut, id_membre, id_admin_validateur)
VALUES
  (NOW(), NOW(), CURDATE(), DATE_ADD(CURDATE(), INTERVAL 30 DAY), 'VALIDEE', 1, 1);

INSERT INTO ADHESION (statut, id_membre)
VALUES
  ('EN_ATTENTE', 2);

-- A reservation for Emma on the first session
INSERT INTO RESERVATION (id_membre, id_seance)
VALUES
  (1, 1);
