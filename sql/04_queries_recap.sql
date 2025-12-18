-- SQL recap: all queries used by the C# application
-- MySQL 8+
USE gymdb;

-- =============================
-- Authentication (used by app)
-- =============================

-- Q_AUTH_ADMIN: admin login
SELECT id_admin, nom, prenom, email, role
FROM ADMINISTRATEUR
WHERE email = ?email AND password_hash = ?password_hash;

-- Q_AUTH_MEMBRE: member login
SELECT id_membre, nom, prenom, email
FROM MEMBRE
WHERE email = ?email AND password_hash = ?password_hash;

-- =============================
-- Required: joins (2 queries)
-- =============================

-- Q_JOIN_1: detailed schedule (seance + type + coach + salle)
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
ORDER BY s.date_heure_debut;

-- Q_JOIN_2: member reservation history
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
JOIN SEANCE s     ON s.id_seance = r.id_seance
JOIN TYPE_COURS tc ON tc.id_type_cours = s.id_type_cours
WHERE r.id_membre = ?id_membre
ORDER BY s.date_heure_debut DESC;

-- =============================
-- Required: LEFT JOIN (1 query)
-- =============================

-- Q_LEFT_JOIN: sessions even with 0 reservation
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
ORDER BY s.date_heure_debut;

-- =============================
-- Required: RIGHT JOIN (1 query)
-- =============================

-- Q_RIGHT_JOIN: all coaches even if no session
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
ORDER BY nb_seances_futures DESC, c.nom;

-- =============================
-- Required: subqueries (2 queries)
-- =============================

-- Q_SUBQUERY_1: validated members without any future active reservation
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
);

-- Q_SUBQUERY_2: coaches above the average number of reservations (future sessions)
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
);

-- =============================
-- Required: ensemblist query (UNION)
-- =============================

-- Q_SET_UNION: all emails across account tables
SELECT email, 'ADMIN' AS source FROM ADMINISTRATEUR
UNION
SELECT email, 'COACH' AS source FROM COACH
UNION
SELECT email, 'MEMBRE' AS source FROM MEMBRE
ORDER BY email;

-- =============================
-- Required: 6 aggregate functions
-- =============================

-- Q_AGG_COUNT: number of active reservations per course type (future)
SELECT tc.id_type_cours, tc.nom_cours, COUNT(r.id_reservation) AS nb_reservations
FROM TYPE_COURS tc
JOIN SEANCE s ON s.id_type_cours = tc.id_type_cours
LEFT JOIN RESERVATION r
  ON r.id_seance = s.id_seance AND r.statut = 'ACTIVE'
WHERE s.date_heure_debut >= NOW() AND s.statut = 'PLANIFIEE'
GROUP BY tc.id_type_cours, tc.nom_cours
ORDER BY nb_reservations DESC;

-- Q_AGG_SUM: total planned minutes per coach (future)
SELECT c.id_coach, c.nom, c.prenom,
       SUM(TIMESTAMPDIFF(MINUTE, s.date_heure_debut, s.date_heure_fin)) AS total_minutes_planifies
FROM COACH c
LEFT JOIN SEANCE s
  ON s.id_coach = c.id_coach
  AND s.date_heure_debut >= NOW()
  AND s.statut = 'PLANIFIEE'
GROUP BY c.id_coach, c.nom, c.prenom
ORDER BY total_minutes_planifies DESC;

-- Q_AGG_AVG: average fill rate per course type (future)
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
ORDER BY taux_moyen_remplissage DESC;

-- Q_AGG_MIN: next session date (future)
SELECT MIN(date_heure_debut) AS prochaine_seance
FROM SEANCE
WHERE date_heure_debut >= NOW() AND statut = 'PLANIFIEE';

-- Q_AGG_MAX: max active reservations on a single session (future)
SELECT MAX(y.nb_reservations) AS max_reservations_sur_une_seance
FROM (
  SELECT s.id_seance, COUNT(r.id_reservation) AS nb_reservations
  FROM SEANCE s
  LEFT JOIN RESERVATION r
    ON r.id_seance = s.id_seance AND r.statut = 'ACTIVE'
  WHERE s.date_heure_debut >= NOW() AND s.statut = 'PLANIFIEE'
  GROUP BY s.id_seance
) y;

-- Q_AGG_STDDEV: standard deviation of active reservations per future session (dispersion)
SELECT STDDEV_SAMP(y.nb_reservations) AS ecart_type_reservations
FROM (
  SELECT s.id_seance, COUNT(r.id_reservation) AS nb_reservations
  FROM SEANCE s
  LEFT JOIN RESERVATION r
    ON r.id_seance = s.id_seance AND r.statut = 'ACTIVE'
  WHERE s.date_heure_debut >= NOW() AND s.statut = 'PLANIFIEE'
  GROUP BY s.id_seance
) y;
