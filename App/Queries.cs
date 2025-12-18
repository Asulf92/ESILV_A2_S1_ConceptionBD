namespace ESILV_A2_S1_ConceptionBD.App;

public static class Queries
{
    public const string AuthAdmin = @"
SELECT id_admin, nom, prenom, email, role
FROM ADMINISTRATEUR
WHERE email = ?email AND password_hash = ?password_hash;";

    public const string AuthMembre = @"
SELECT id_membre, nom, prenom, email
FROM MEMBRE
WHERE email = ?email AND password_hash = ?password_hash;";

    public const string JoinSchedule = @"
SELECT
  s.id_seance,
  s.date_heure_debut,
  s.date_heure_fin,
  s.capacite_max_seance,
  s.statut,
  tc.nom_cours,
  c.nom AS coach_nom,
  c.prenom AS coach_prenom,
  sa.nom_salle
FROM SEANCE s
JOIN TYPE_COURS tc ON tc.id_type_cours = s.id_type_cours
JOIN COACH c       ON c.id_coach       = s.id_coach
JOIN SALLE sa      ON sa.id_salle      = s.id_salle
WHERE s.date_heure_debut >= NOW()
ORDER BY s.date_heure_debut;";

    public const string JoinMemberHistory = @"
SELECT
  r.id_reservation,
  r.date_reservation,
  r.statut AS reservation_statut,
  r.date_annulation,
  s.id_seance,
  s.date_heure_debut,
  s.date_heure_fin,
  s.statut AS seance_statut,
  tc.nom_cours
FROM RESERVATION r
JOIN SEANCE s      ON s.id_seance = r.id_seance
JOIN TYPE_COURS tc ON tc.id_type_cours = s.id_type_cours
WHERE r.id_membre = ?id_membre
ORDER BY s.date_heure_debut DESC;";

    public const string IsMembreValide = @"
SELECT 1
FROM ADHESION a
WHERE a.id_membre = ?id_membre AND a.statut = 'VALIDEE'
ORDER BY a.date_validation DESC
LIMIT 1;";

    public const string InsertMembre = @"
INSERT INTO MEMBRE (nom, prenom, adresse, telephone, email, password_hash)
  VALUES (?nom, ?prenom, ?adresse, ?telephone, ?email, ?password_hash);";

    public const string InsertAdhesionDemande = @"
INSERT INTO ADHESION (statut, id_membre)
VALUES ('EN_ATTENTE', ?id_membre);";

    public const string AdminListAdhesionsEnAttente = @"
SELECT a.id_adhesion, a.date_demande, m.id_membre, m.nom, m.prenom, m.email
FROM ADHESION a
JOIN MEMBRE m ON m.id_membre = a.id_membre
WHERE a.statut = 'EN_ATTENTE'
ORDER BY a.date_demande;";

    public const string AdminValiderAdhesion = @"
UPDATE ADHESION
SET statut = 'VALIDEE',
    date_validation = NOW(),
    date_debut = CURDATE(),
    date_fin = DATE_ADD(CURDATE(), INTERVAL 30 DAY),
    id_admin_validateur = ?id_admin
WHERE id_adhesion = ?id_adhesion AND statut = 'EN_ATTENTE';";

    public const string AdminRefuserAdhesion = @"
UPDATE ADHESION
SET statut = 'REFUSEE',
    date_validation = NOW(),
    id_admin_validateur = ?id_admin
WHERE id_adhesion = ?id_adhesion AND statut = 'EN_ATTENTE';";

    public const string AdminAddCoach = @"
INSERT INTO COACH (nom, prenom, telephone, email, specialite, formations, password_hash)
VALUES (?nom, ?prenom, ?telephone, ?email, ?specialite, ?formations, ?password_hash);";

    public const string AdminAddTypeCours = @"
INSERT INTO TYPE_COURS (nom_cours, description, capacite_max_cours, id_admin_createur)
VALUES (?nom_cours, ?description, ?capacite_max_cours, ?id_admin_createur);";

    public const string AdminAddSalle = @"
INSERT INTO SALLE (nom_salle, capacite_max_salle)
VALUES (?nom_salle, ?capacite_max_salle);";

    public const string AdminAddSeance = @"
INSERT INTO SEANCE (
  date_heure_debut, date_heure_fin, capacite_max_seance, statut,
  id_coach, id_type_cours, id_salle
)
VALUES (
  ?date_heure_debut, ?date_heure_fin, ?capacite_max_seance, 'PLANIFIEE',
  ?id_coach, ?id_type_cours, ?id_salle
);";

    public const string AdminAnnulerSeance = @"
UPDATE SEANCE
SET statut = 'ANNULEE',
    date_annulation = NOW(),
    id_admin_annulation = ?id_admin
WHERE id_seance = ?id_seance AND statut = 'PLANIFIEE';";

    // Reservation transaction helpers
    public const string LockSeanceForUpdate = @"
SELECT capacite_max_seance
FROM SEANCE
WHERE id_seance = ?id_seance
  AND statut = 'PLANIFIEE'
  AND date_heure_debut >= NOW()
FOR UPDATE;";

    public const string CountActiveReservationsForSeance = @"
SELECT COUNT(*)
FROM RESERVATION
WHERE id_seance = ?id_seance AND statut = 'ACTIVE';";

    public const string InsertReservation = @"
INSERT INTO RESERVATION (id_membre, id_seance)
VALUES (?id_membre, ?id_seance);";

    public const string CancelReservation = @"
UPDATE RESERVATION
SET statut = 'ANNULEE', date_annulation = NOW()
WHERE id_reservation = ?id_reservation
  AND id_membre = ?id_membre
  AND statut = 'ACTIVE';";

    // Reporting queries (aggregates + requirements)
    public const string SetUnionEmails = @"
SELECT email, 'ADMIN' AS source FROM ADMINISTRATEUR
UNION
SELECT email, 'COACH' AS source FROM COACH
UNION
SELECT email, 'MEMBRE' AS source FROM MEMBRE
ORDER BY email;";

    public const string RightJoinCoachesFutureSessions = @"
SELECT
  c.id_coach,
  c.nom,
  c.prenom,
  COUNT(s.id_seance) AS nb_seances_futures
FROM SEANCE s
RIGHT JOIN COACH c
  ON c.id_coach = s.id_coach
  AND s.date_heure_debut >= NOW()
  AND s.statut = 'PLANIFIEE'
GROUP BY c.id_coach, c.nom, c.prenom
ORDER BY nb_seances_futures DESC, c.nom;";

    public const string LeftJoinSeancesWithCounts = @"
SELECT
  s.id_seance,
  s.date_heure_debut,
  tc.nom_cours,
  s.capacite_max_seance,
  COUNT(r.id_reservation) AS nb_reservations_actives
FROM SEANCE s
JOIN TYPE_COURS tc ON tc.id_type_cours = s.id_type_cours
LEFT JOIN RESERVATION r
  ON r.id_seance = s.id_seance AND r.statut = 'ACTIVE'
WHERE s.date_heure_debut >= NOW() AND s.statut = 'PLANIFIEE'
GROUP BY s.id_seance, s.date_heure_debut, tc.nom_cours, s.capacite_max_seance
ORDER BY s.date_heure_debut;";

    public const string SubqueryValidatedNoFutureReservation = @"
SELECT m.id_membre, m.nom, m.prenom, m.email
FROM MEMBRE m
WHERE EXISTS (
  SELECT 1
  FROM ADHESION a
  WHERE a.id_membre = m.id_membre AND a.statut = 'VALIDEE'
)
AND NOT EXISTS (
  SELECT 1
  FROM RESERVATION r
  JOIN SEANCE s ON s.id_seance = r.id_seance
  WHERE r.id_membre = m.id_membre
    AND r.statut = 'ACTIVE'
    AND s.date_heure_debut >= NOW()
    AND s.statut = 'PLANIFIEE'
);";

    public const string SubqueryCoachesAboveAverageReservations = @"
SELECT t.id_coach, t.coach_nom, t.coach_prenom, t.total_reservations
FROM (
  SELECT
    c.id_coach,
    c.nom AS coach_nom,
    c.prenom AS coach_prenom,
    COUNT(r.id_reservation) AS total_reservations
  FROM COACH c
  LEFT JOIN SEANCE s
    ON s.id_coach = c.id_coach
    AND s.date_heure_debut >= NOW()
    AND s.statut = 'PLANIFIEE'
  LEFT JOIN RESERVATION r
    ON r.id_seance = s.id_seance
    AND r.statut = 'ACTIVE'
  GROUP BY c.id_coach, c.nom, c.prenom
) t
WHERE t.total_reservations > (
  SELECT AVG(t2.total_reservations)
  FROM (
    SELECT
      c2.id_coach,
      COUNT(r2.id_reservation) AS total_reservations
    FROM COACH c2
    LEFT JOIN SEANCE s2
      ON s2.id_coach = c2.id_coach
      AND s2.date_heure_debut >= NOW()
      AND s2.statut = 'PLANIFIEE'
    LEFT JOIN RESERVATION r2
      ON r2.id_seance = s2.id_seance
      AND r2.statut = 'ACTIVE'
    GROUP BY c2.id_coach
  ) t2
);";

    // 6 aggregates
    public const string AggCountReservationsPerTypeCours = @"
SELECT tc.id_type_cours, tc.nom_cours, COUNT(r.id_reservation) AS nb_reservations
FROM TYPE_COURS tc
JOIN SEANCE s ON s.id_type_cours = tc.id_type_cours
LEFT JOIN RESERVATION r
  ON r.id_seance = s.id_seance AND r.statut = 'ACTIVE'
WHERE s.date_heure_debut >= NOW() AND s.statut = 'PLANIFIEE'
GROUP BY tc.id_type_cours, tc.nom_cours
ORDER BY nb_reservations DESC;";

    public const string AggSumMinutesPlanifiesParCoach = @"
SELECT c.id_coach, c.nom, c.prenom,
       SUM(TIMESTAMPDIFF(MINUTE, s.date_heure_debut, s.date_heure_fin)) AS total_minutes_planifies
FROM COACH c
LEFT JOIN SEANCE s
  ON s.id_coach = c.id_coach
  AND s.date_heure_debut >= NOW()
  AND s.statut = 'PLANIFIEE'
GROUP BY c.id_coach, c.nom, c.prenom
ORDER BY total_minutes_planifies DESC;";

    public const string AggAvgFillRateParTypeCours = @"
SELECT tc.id_type_cours, tc.nom_cours,
       AVG(x.fill_rate) AS taux_moyen_remplissage
FROM TYPE_COURS tc
JOIN (
  SELECT s.id_type_cours,
         (COUNT(r.id_reservation) / s.capacite_max_seance) AS fill_rate
  FROM SEANCE s
  LEFT JOIN RESERVATION r
    ON r.id_seance = s.id_seance AND r.statut = 'ACTIVE'
  WHERE s.date_heure_debut >= NOW() AND s.statut = 'PLANIFIEE'
  GROUP BY s.id_seance, s.id_type_cours, s.capacite_max_seance
) x ON x.id_type_cours = tc.id_type_cours
GROUP BY tc.id_type_cours, tc.nom_cours
ORDER BY taux_moyen_remplissage DESC;";

    public const string AggMinProchaineSeance = @"
SELECT MIN(date_heure_debut) AS prochaine_seance
FROM SEANCE
WHERE date_heure_debut >= NOW() AND statut = 'PLANIFIEE';";

    public const string AggMaxReservationsSurUneSeance = @"
SELECT MAX(y.nb_reservations) AS max_reservations_sur_une_seance
FROM (
  SELECT s.id_seance, COUNT(r.id_reservation) AS nb_reservations
  FROM SEANCE s
  LEFT JOIN RESERVATION r
    ON r.id_seance = s.id_seance AND r.statut = 'ACTIVE'
  WHERE s.date_heure_debut >= NOW() AND s.statut = 'PLANIFIEE'
  GROUP BY s.id_seance
) y;";

    public const string AggStdDevReservationsSurSeances = @"
SELECT STDDEV_SAMP(y.nb_reservations) AS ecart_type_reservations
FROM (
  SELECT s.id_seance, COUNT(r.id_reservation) AS nb_reservations
  FROM SEANCE s
  LEFT JOIN RESERVATION r
    ON r.id_seance = s.id_seance AND r.statut = 'ACTIVE'
  WHERE s.date_heure_debut >= NOW() AND s.statut = 'PLANIFIEE'
  GROUP BY s.id_seance
) y;";
}
