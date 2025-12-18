namespace ESILV_A2_S1_ConceptionBD.App;

public enum AppRole
{
    AdminPrincipal = 1,
    AdminSecondaire = 2,
    Membre = 3,
    Reporting = 4,
}

public sealed record AdminUser(int Id, string Nom, string Prenom, string Email, string Role);
public sealed record MembreUser(int Id, string Nom, string Prenom, string Email);

public sealed record SeanceSummary(
    int Id,
    DateTime Debut,
    DateTime Fin,
    int Capacite,
    string Statut,
    string NomCours,
    string CoachNom,
    string CoachPrenom,
    string NomSalle);

public sealed record ReservationHistoryItem(
    int ReservationId,
    DateTime ReservationDate,
    string ReservationStatut,
    DateTime? ReservationAnnulation,
    int SeanceId,
    DateTime SeanceDebut,
    DateTime SeanceFin,
    string SeanceStatut,
    string NomCours);
