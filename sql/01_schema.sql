-- MySQL 8+
-- Schema creation for gymdb

USE gymdb;

-- Drop in reverse dependency order (optional for reset)
DROP TABLE IF EXISTS RESERVATION;
DROP TABLE IF EXISTS SEANCE;
DROP TABLE IF EXISTS ADHESION;
DROP TABLE IF EXISTS TYPE_COURS;
DROP TABLE IF EXISTS SALLE;
DROP TABLE IF EXISTS COACH;
DROP TABLE IF EXISTS MEMBRE;
DROP TABLE IF EXISTS ADMINISTRATEUR;

CREATE TABLE ADMINISTRATEUR (
  id_admin           INT AUTO_INCREMENT PRIMARY KEY,
  nom                VARCHAR(100) NOT NULL,
  prenom             VARCHAR(100) NOT NULL,
  email              VARCHAR(255) NOT NULL,
  password_hash      CHAR(64) NOT NULL,
  role               ENUM('PRINCIPAL','SECONDAIRE') NOT NULL,
  created_at         DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

  CONSTRAINT uq_admin_email UNIQUE (email)
) ENGINE=InnoDB;

CREATE TABLE MEMBRE (
  id_membre          INT AUTO_INCREMENT PRIMARY KEY,
  nom               VARCHAR(100) NOT NULL,
  prenom            VARCHAR(100) NOT NULL,
  adresse           VARCHAR(255) NOT NULL,
  telephone         VARCHAR(30) NOT NULL,
  email             VARCHAR(255) NOT NULL,
  password_hash     CHAR(64) NOT NULL,
  date_inscription  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

  CONSTRAINT uq_membre_email UNIQUE (email),
  CONSTRAINT ck_membre_tel CHECK (telephone REGEXP '^[0-9+ .()-]{6,30}$')
) ENGINE=InnoDB;

CREATE TABLE COACH (
  id_coach           INT AUTO_INCREMENT PRIMARY KEY,
  nom                VARCHAR(100) NOT NULL,
  prenom             VARCHAR(100) NOT NULL,
  telephone          VARCHAR(30) NOT NULL,
  email              VARCHAR(255) NOT NULL,
  specialite         VARCHAR(120) NOT NULL,
  formations         TEXT NULL,
  password_hash      CHAR(64) NOT NULL,
  created_at         DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

  CONSTRAINT uq_coach_email UNIQUE (email),
  CONSTRAINT ck_coach_tel CHECK (telephone REGEXP '^[0-9+ .()-]{6,30}$')
) ENGINE=InnoDB;

CREATE TABLE SALLE (
  id_salle           INT AUTO_INCREMENT PRIMARY KEY,
  nom_salle          VARCHAR(120) NOT NULL,
  capacite_max_salle INT NOT NULL,

  CONSTRAINT uq_salle_nom UNIQUE (nom_salle),
  CONSTRAINT ck_salle_cap CHECK (capacite_max_salle > 0)
) ENGINE=InnoDB;

CREATE TABLE TYPE_COURS (
  id_type_cours      INT AUTO_INCREMENT PRIMARY KEY,
  nom_cours          VARCHAR(120) NOT NULL,
  description        TEXT NULL,
  capacite_max_cours INT NOT NULL,
  id_admin_createur  INT NULL,

  CONSTRAINT uq_type_cours_nom UNIQUE (nom_cours),
  CONSTRAINT ck_type_cours_cap CHECK (capacite_max_cours > 0),
  CONSTRAINT fk_type_cours_admin FOREIGN KEY (id_admin_createur) REFERENCES ADMINISTRATEUR(id_admin)
    ON UPDATE CASCADE ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE ADHESION (
  id_adhesion          INT AUTO_INCREMENT PRIMARY KEY,
  date_demande         DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  date_validation      DATETIME NULL,
  date_debut           DATE NULL,
  date_fin             DATE NULL,
  statut               ENUM('EN_ATTENTE','VALIDEE','REFUSEE','EXPIREE','RESILIEE') NOT NULL DEFAULT 'EN_ATTENTE',
  id_membre            INT NOT NULL,
  id_admin_validateur  INT NULL,

  CONSTRAINT fk_adhesion_membre FOREIGN KEY (id_membre) REFERENCES MEMBRE(id_membre)
    ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_adhesion_admin FOREIGN KEY (id_admin_validateur) REFERENCES ADMINISTRATEUR(id_admin)
    ON UPDATE CASCADE ON DELETE RESTRICT,

  CONSTRAINT ck_adhesion_dates CHECK (
    date_debut IS NULL OR date_fin IS NULL OR date_fin >= date_debut
  ),
  CONSTRAINT ck_adhesion_validation CHECK (
    (statut <> 'VALIDEE') OR (date_validation IS NOT NULL AND id_admin_validateur IS NOT NULL)
  )
) ENGINE=InnoDB;

CREATE INDEX idx_adhesion_membre ON ADHESION(id_membre);
CREATE INDEX idx_adhesion_statut ON ADHESION(statut);

CREATE TABLE SEANCE (
  id_seance            INT AUTO_INCREMENT PRIMARY KEY,
  date_heure_debut     DATETIME NOT NULL,
  date_heure_fin       DATETIME NOT NULL,
  capacite_max_seance  INT NOT NULL,
  statut               ENUM('PLANIFIEE','ANNULEE') NOT NULL DEFAULT 'PLANIFIEE',
  date_annulation      DATETIME NULL,
  id_admin_annulation  INT NULL,
  id_coach             INT NOT NULL,
  id_type_cours        INT NOT NULL,
  id_salle             INT NOT NULL,

  CONSTRAINT fk_seance_coach FOREIGN KEY (id_coach) REFERENCES COACH(id_coach)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT fk_seance_type FOREIGN KEY (id_type_cours) REFERENCES TYPE_COURS(id_type_cours)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT fk_seance_salle FOREIGN KEY (id_salle) REFERENCES SALLE(id_salle)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT fk_seance_admin_annule FOREIGN KEY (id_admin_annulation) REFERENCES ADMINISTRATEUR(id_admin)
    ON UPDATE CASCADE ON DELETE RESTRICT,

  CONSTRAINT ck_seance_cap CHECK (capacite_max_seance > 0),
  CONSTRAINT ck_seance_horaires CHECK (date_heure_fin > date_heure_debut),
  CONSTRAINT ck_seance_annulation CHECK (
    (statut = 'PLANIFIEE' AND date_annulation IS NULL AND id_admin_annulation IS NULL)
    OR
    (statut = 'ANNULEE' AND date_annulation IS NOT NULL AND id_admin_annulation IS NOT NULL)
  )
) ENGINE=InnoDB;

CREATE INDEX idx_seance_debut ON SEANCE(date_heure_debut);
CREATE INDEX idx_seance_type ON SEANCE(id_type_cours);
CREATE INDEX idx_seance_coach ON SEANCE(id_coach);
CREATE INDEX idx_seance_salle ON SEANCE(id_salle);

CREATE TABLE RESERVATION (
  id_reservation     INT AUTO_INCREMENT PRIMARY KEY,
  date_reservation   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  statut             ENUM('ACTIVE','ANNULEE') NOT NULL DEFAULT 'ACTIVE',
  date_annulation    DATETIME NULL,
  id_membre          INT NOT NULL,
  id_seance          INT NOT NULL,

  CONSTRAINT fk_reservation_membre FOREIGN KEY (id_membre) REFERENCES MEMBRE(id_membre)
    ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_reservation_seance FOREIGN KEY (id_seance) REFERENCES SEANCE(id_seance)
    ON UPDATE CASCADE ON DELETE CASCADE,

  CONSTRAINT uq_reservation_unique UNIQUE (id_membre, id_seance),
  CONSTRAINT ck_reservation_annulation CHECK (
    (statut = 'ACTIVE' AND date_annulation IS NULL)
    OR
    (statut = 'ANNULEE' AND date_annulation IS NOT NULL)
  )
) ENGINE=InnoDB;

CREATE INDEX idx_reservation_seance ON RESERVATION(id_seance);
CREATE INDEX idx_reservation_membre ON RESERVATION(id_membre);
CREATE INDEX idx_reservation_statut ON RESERVATION(statut);
