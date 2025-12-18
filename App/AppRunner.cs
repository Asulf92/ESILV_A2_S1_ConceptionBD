using MySqlConnector;

namespace ESILV_A2_S1_ConceptionBD.App;

public sealed class AppRunner
{
    private readonly DatabaseConfig _db;

    public AppRunner(DatabaseConfig db)
    {
        _db = db;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== Salle de sport - menu principal ===");
            Console.WriteLine("1) Admin principal");
            Console.WriteLine("2) Admin secondaire");
            Console.WriteLine("3) Membre");
            Console.WriteLine("4) Evaluation / Reporting");
            Console.WriteLine("0) Quitter");

            int choice = ConsoleHelpers.PromptInt("> ");
            if (choice == 0) return;

            AppRole role = (AppRole)choice;
            try
            {
                switch (role)
                {
                    case AppRole.AdminPrincipal:
                        await RunAdminAsync(_db.Connections.AdminPrincipal);
                        break;
                    case AppRole.AdminSecondaire:
                        await RunAdminAsync(_db.Connections.AdminSecondaire);
                        break;
                    case AppRole.Membre:
                        await RunMembreAsync(_db.Connections.Membre);
                        break;
                    case AppRole.Reporting:
                        await RunReportingAsync(_db.Connections.Reporting);
                        break;
                    default:
                        Console.WriteLine("Choix invalide.");
                        break;
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur MySQL: {ex.Number} - {ex.Message}");
            }
        }
    }

    private async Task RunAdminAsync(string connectionString)
    {
        using MySqlConnection connection = await Db.OpenAsync(connectionString);

        AdminUser? admin = await AuthenticateAdminAsync(connection);
        if (admin is null)
        {
            Console.WriteLine("Connexion admin échouée.");
            return;
        }

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine($"=== Admin ({admin.Role}) ===");
            Console.WriteLine("1) Lister adhésions en attente");
            Console.WriteLine("2) Valider une adhésion");
            Console.WriteLine("3) Refuser une adhésion");
            Console.WriteLine("4) Ajouter un coach");
            Console.WriteLine("5) Ajouter un type de cours");
            Console.WriteLine("6) Ajouter une salle");
            Console.WriteLine("7) Planifier une séance");
            Console.WriteLine("8) Annuler une séance");
            Console.WriteLine("9) (Requête ensembliste) Emails (UNION)");
            Console.WriteLine("0) Retour");

            int choice = ConsoleHelpers.PromptInt("> ");
            if (choice == 0) return;

            switch (choice)
            {
                case 1:
                    await AdminListAdhesionsEnAttenteAsync(connection);
                    break;
                case 2:
                    await AdminValiderAdhesionAsync(connection, admin.Id);
                    break;
                case 3:
                    await AdminRefuserAdhesionAsync(connection, admin.Id);
                    break;
                case 4:
                    await AdminAddCoachAsync(connection);
                    break;
                case 5:
                    await AdminAddTypeCoursAsync(connection, admin.Id);
                    break;
                case 6:
                    await AdminAddSalleAsync(connection);
                    break;
                case 7:
                    await AdminAddSeanceAsync(connection);
                    break;
                case 8:
                    await AdminAnnulerSeanceAsync(connection, admin.Id);
                    break;
                case 9:
                    await RunSimpleQueryAsync(connection, Queries.SetUnionEmails);
                    break;
                default:
                    Console.WriteLine("Choix invalide.");
                    break;
            }
        }
    }

    private async Task RunMembreAsync(string connectionString)
    {
        using MySqlConnection connection = await Db.OpenAsync(connectionString);

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== Membre ===");
            Console.WriteLine("1) Inscription (créer un compte + demande d'adhésion)");
            Console.WriteLine("2) Connexion");
            Console.WriteLine("0) Retour");

            int choice = ConsoleHelpers.PromptInt("> ");
            if (choice == 0) return;

            if (choice == 1)
            {
                await MembreInscriptionAsync(connection);
            }
            else if (choice == 2)
            {
                MembreUser? membre = await AuthenticateMembreAsync(connection);
                if (membre is null)
                {
                    Console.WriteLine("Connexion membre échouée.");
                    continue;
                }

                await RunMembreConnectedAsync(connection, membre);
            }
        }
    }

    private async Task RunMembreConnectedAsync(MySqlConnection connection, MembreUser membre)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine($"=== Membre connecté: {membre.Prenom} {membre.Nom} ===");
            Console.WriteLine("1) Voir les séances disponibles (JOIN)");
            Console.WriteLine("2) Réserver une séance (transaction + capacité)");
            Console.WriteLine("3) Annuler une réservation");
            Console.WriteLine("4) Historique des réservations (JOIN)");
            Console.WriteLine("0) Déconnexion");

            int choice = ConsoleHelpers.PromptInt("> ");
            if (choice == 0) return;

            switch (choice)
            {
                case 1:
                    await ShowScheduleAsync(connection);
                    break;
                case 2:
                    await MembreReserverAsync(connection, membre.Id);
                    break;
                case 3:
                    await MembreAnnulerReservationAsync(connection, membre.Id);
                    break;
                case 4:
                    await ShowMemberHistoryAsync(connection, membre.Id);
                    break;
                default:
                    Console.WriteLine("Choix invalide.");
                    break;
            }
        }
    }

    private async Task RunReportingAsync(string connectionString)
    {
        using MySqlConnection connection = await Db.OpenAsync(connectionString);

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== Evaluation / Reporting ===");
            Console.WriteLine("1) Requêtes obligatoires (LEFT/RIGHT/Subqueries/UNION)");
            Console.WriteLine("2) Agrégats (COUNT/SUM/AVG/MIN/MAX/STDDEV)");
            Console.WriteLine("0) Retour");

            int choice = ConsoleHelpers.PromptInt("> ");
            if (choice == 0) return;

            if (choice == 1)
            {
                await RunSimpleQueryAsync(connection, Queries.LeftJoinSeancesWithCounts);
                await RunSimpleQueryAsync(connection, Queries.RightJoinCoachesFutureSessions);
                await RunSimpleQueryAsync(connection, Queries.SubqueryValidatedNoFutureReservation);
                await RunSimpleQueryAsync(connection, Queries.SubqueryCoachesAboveAverageReservations);
                await RunSimpleQueryAsync(connection, Queries.SetUnionEmails);
            }
            else if (choice == 2)
            {
                await RunSimpleQueryAsync(connection, Queries.AggCountReservationsPerTypeCours);
                await RunSimpleQueryAsync(connection, Queries.AggSumMinutesPlanifiesParCoach);
                await RunSimpleQueryAsync(connection, Queries.AggAvgFillRateParTypeCours);
                await RunSimpleQueryAsync(connection, Queries.AggMinProchaineSeance);
                await RunSimpleQueryAsync(connection, Queries.AggMaxReservationsSurUneSeance);
                await RunSimpleQueryAsync(connection, Queries.AggStdDevReservationsSurSeances);
            }
            else
            {
                Console.WriteLine("Choix invalide.");
            }
        }
    }

    private async Task<AdminUser?> AuthenticateAdminAsync(MySqlConnection connection)
    {
        string email = ConsoleHelpers.Prompt("Email: ");
        string password = ConsoleHelpers.Prompt("Mot de passe: ");

        string hash = PasswordHash.Sha256Hex(_db.Salt, password);

        using var cmd = new MySqlCommand(Queries.AuthAdmin, connection);
        cmd.Parameters.AddWithValue("?email", email);
        cmd.Parameters.AddWithValue("?password_hash", hash);

        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new AdminUser(
            Id: reader.GetInt32("id_admin"),
            Nom: reader.GetString("nom"),
            Prenom: reader.GetString("prenom"),
            Email: reader.GetString("email"),
            Role: reader.GetString("role"));
    }

    private async Task<MembreUser?> AuthenticateMembreAsync(MySqlConnection connection)
    {
        string email = ConsoleHelpers.Prompt("Email: ");
        string password = ConsoleHelpers.Prompt("Mot de passe: ");

        string hash = PasswordHash.Sha256Hex(_db.Salt, password);

        using var cmd = new MySqlCommand(Queries.AuthMembre, connection);
        cmd.Parameters.AddWithValue("?email", email);
        cmd.Parameters.AddWithValue("?password_hash", hash);

        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new MembreUser(
            Id: reader.GetInt32("id_membre"),
            Nom: reader.GetString("nom"),
            Prenom: reader.GetString("prenom"),
            Email: reader.GetString("email"));
    }

    private async Task MembreInscriptionAsync(MySqlConnection connection)
    {
        Console.WriteLine("--- Inscription ---");
        string nom = ConsoleHelpers.Prompt("Nom: ");
        string prenom = ConsoleHelpers.Prompt("Prénom: ");
        string adresse = ConsoleHelpers.Prompt("Adresse: ");
        string telephone = ConsoleHelpers.Prompt("Téléphone: ");
        string email = ConsoleHelpers.Prompt("Email: ");
        string password = ConsoleHelpers.Prompt("Mot de passe: ");

        string hash = PasswordHash.Sha256Hex(_db.Salt, password);

        using var cmd = new MySqlCommand(Queries.InsertMembre, connection);
        cmd.Parameters.AddWithValue("?nom", nom);
        cmd.Parameters.AddWithValue("?prenom", prenom);
        cmd.Parameters.AddWithValue("?adresse", adresse);
        cmd.Parameters.AddWithValue("?telephone", telephone);
        cmd.Parameters.AddWithValue("?email", email);
        cmd.Parameters.AddWithValue("?password_hash", hash);

        await cmd.ExecuteNonQueryAsync();
        int idMembre = checked((int)cmd.LastInsertedId);

        using var cmdAdh = new MySqlCommand(Queries.InsertAdhesionDemande, connection);
        cmdAdh.Parameters.AddWithValue("?id_membre", idMembre);
        await cmdAdh.ExecuteNonQueryAsync();

        Console.WriteLine("Compte créé. Demande d'adhésion enregistrée (EN_ATTENTE). ");
    }

    private async Task ShowScheduleAsync(MySqlConnection connection)
    {
        using var cmd = new MySqlCommand(Queries.JoinSchedule, connection);
        using var reader = await cmd.ExecuteReaderAsync();

        Console.WriteLine("--- Séances futures ---");
        while (await reader.ReadAsync())
        {
            Console.WriteLine(
                $"#{reader.GetInt32("id_seance")} | {reader.GetDateTime("date_heure_debut"):yyyy-MM-dd HH:mm} - {reader.GetDateTime("date_heure_fin"):HH:mm} | {reader.GetString("nom_cours")} | {reader.GetString("coach_prenom")} {reader.GetString("coach_nom")} | {reader.GetString("nom_salle")} | cap={reader.GetInt32("capacite_max_seance")} | {reader.GetString("statut")}");
        }
    }

    private async Task ShowMemberHistoryAsync(MySqlConnection connection, int idMembre)
    {
        using var cmd = new MySqlCommand(Queries.JoinMemberHistory, connection);
        cmd.Parameters.AddWithValue("?id_membre", idMembre);

        using var reader = await cmd.ExecuteReaderAsync();
        Console.WriteLine("--- Historique ---");

        while (await reader.ReadAsync())
        {
            string rStatut = reader.GetString("reservation_statut");
            string seanceStatut = reader.GetString("seance_statut");
            Console.WriteLine(
                $"res#{reader.GetInt32("id_reservation")} | {reader.GetDateTime("date_heure_debut"):yyyy-MM-dd HH:mm} | {reader.GetString("nom_cours")} | res={rStatut} | seance={seanceStatut}");
        }
    }

    private async Task MembreReserverAsync(MySqlConnection connection, int idMembre)
    {
        int idSeance = ConsoleHelpers.PromptInt("Id séance à réserver: ");

        // Must have a validated membership
        using (var check = new MySqlCommand(Queries.IsMembreValide, connection))
        {
            check.Parameters.AddWithValue("?id_membre", idMembre);
            object? ok = await check.ExecuteScalarAsync();
            if (ok is null)
            {
                Console.WriteLine("Adhésion non validée. Réservation impossible.");
                return;
            }
        }

        using var tx = await connection.BeginTransactionAsync();
        try
        {
            int capacite;
            using (var lockCmd = new MySqlCommand(Queries.LockSeanceForUpdate, connection, tx))
            {
                lockCmd.Parameters.AddWithValue("?id_seance", idSeance);
                object? capObj = await lockCmd.ExecuteScalarAsync();
                if (capObj is null)
                {
                    Console.WriteLine("Séance inexistante/annulée/passée.");
                    await tx.RollbackAsync();
                    return;
                }

                capacite = Convert.ToInt32(capObj);
            }

            int count;
            using (var countCmd = new MySqlCommand(Queries.CountActiveReservationsForSeance, connection, tx))
            {
                countCmd.Parameters.AddWithValue("?id_seance", idSeance);
                object? countObj = await countCmd.ExecuteScalarAsync();
                count = Convert.ToInt32(countObj);
            }

            if (count >= capacite)
            {
                Console.WriteLine("Capacité atteinte. Réservation impossible.");
                await tx.RollbackAsync();
                return;
            }

            using (var insert = new MySqlCommand(Queries.InsertReservation, connection, tx))
            {
                insert.Parameters.AddWithValue("?id_membre", idMembre);
                insert.Parameters.AddWithValue("?id_seance", idSeance);
                await insert.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
            Console.WriteLine("Réservation créée.");
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            await tx.RollbackAsync();
            Console.WriteLine("Déjà réservé (doublon). ");
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    private async Task MembreAnnulerReservationAsync(MySqlConnection connection, int idMembre)
    {
        int idReservation = ConsoleHelpers.PromptInt("Id réservation à annuler: ");
        using var cmd = new MySqlCommand(Queries.CancelReservation, connection);
        cmd.Parameters.AddWithValue("?id_reservation", idReservation);
        cmd.Parameters.AddWithValue("?id_membre", idMembre);

        int rows = await cmd.ExecuteNonQueryAsync();
        Console.WriteLine(rows == 1 ? "Réservation annulée." : "Aucune réservation annulée (id invalide ou déjà annulée)." );
    }

    private async Task AdminListAdhesionsEnAttenteAsync(MySqlConnection connection)
    {
        using var cmd = new MySqlCommand(Queries.AdminListAdhesionsEnAttente, connection);
        using var reader = await cmd.ExecuteReaderAsync();

        Console.WriteLine("--- Adhésions en attente ---");
        while (await reader.ReadAsync())
        {
            Console.WriteLine(
                $"adh#{reader.GetInt32("id_adhesion")} | {reader.GetDateTime("date_demande"):yyyy-MM-dd HH:mm} | membre#{reader.GetInt32("id_membre")} {reader.GetString("prenom")} {reader.GetString("nom")} ({reader.GetString("email")})");
        }
    }

    private async Task AdminValiderAdhesionAsync(MySqlConnection connection, int idAdmin)
    {
        int idAdhesion = ConsoleHelpers.PromptInt("Id adhésion à valider: ");
        using var cmd = new MySqlCommand(Queries.AdminValiderAdhesion, connection);
        cmd.Parameters.AddWithValue("?id_admin", idAdmin);
        cmd.Parameters.AddWithValue("?id_adhesion", idAdhesion);

        int rows = await cmd.ExecuteNonQueryAsync();
        Console.WriteLine(rows == 1 ? "Adhésion validée." : "Adhésion non trouvée (ou pas en attente)." );
    }

    private async Task AdminRefuserAdhesionAsync(MySqlConnection connection, int idAdmin)
    {
        int idAdhesion = ConsoleHelpers.PromptInt("Id adhésion à refuser: ");
        using var cmd = new MySqlCommand(Queries.AdminRefuserAdhesion, connection);
        cmd.Parameters.AddWithValue("?id_admin", idAdmin);
        cmd.Parameters.AddWithValue("?id_adhesion", idAdhesion);

        int rows = await cmd.ExecuteNonQueryAsync();
        Console.WriteLine(rows == 1 ? "Adhésion refusée." : "Adhésion non trouvée (ou pas en attente)." );
    }

    private async Task AdminAddCoachAsync(MySqlConnection connection)
    {
        Console.WriteLine("--- Ajouter coach ---");
        string nom = ConsoleHelpers.Prompt("Nom: ");
        string prenom = ConsoleHelpers.Prompt("Prénom: ");
        string telephone = ConsoleHelpers.Prompt("Téléphone: ");
        string email = ConsoleHelpers.Prompt("Email: ");
        string specialite = ConsoleHelpers.Prompt("Spécialité: ");
        string formations = ConsoleHelpers.Prompt("Formations (texte): ");
        string password = ConsoleHelpers.Prompt("Mot de passe (coach): ");

        string hash = PasswordHash.Sha256Hex(_db.Salt, password);

        using var cmd = new MySqlCommand(Queries.AdminAddCoach, connection);
        cmd.Parameters.AddWithValue("?nom", nom);
        cmd.Parameters.AddWithValue("?prenom", prenom);
        cmd.Parameters.AddWithValue("?telephone", telephone);
        cmd.Parameters.AddWithValue("?email", email);
        cmd.Parameters.AddWithValue("?specialite", specialite);
        cmd.Parameters.AddWithValue("?formations", string.IsNullOrWhiteSpace(formations) ? null : formations);
        cmd.Parameters.AddWithValue("?password_hash", hash);

        await cmd.ExecuteNonQueryAsync();
        Console.WriteLine("Coach ajouté.");
    }

    private async Task AdminAddTypeCoursAsync(MySqlConnection connection, int idAdmin)
    {
        Console.WriteLine("--- Ajouter type de cours ---");
        string nomCours = ConsoleHelpers.Prompt("Nom cours: ");
        string description = ConsoleHelpers.Prompt("Description (texte): ");
        int capacite = ConsoleHelpers.PromptInt("Capacité max: ");

        using var cmd = new MySqlCommand(Queries.AdminAddTypeCours, connection);
        cmd.Parameters.AddWithValue("?nom_cours", nomCours);
        cmd.Parameters.AddWithValue("?description", string.IsNullOrWhiteSpace(description) ? null : description);
        cmd.Parameters.AddWithValue("?capacite_max_cours", capacite);
        cmd.Parameters.AddWithValue("?id_admin_createur", idAdmin);

        await cmd.ExecuteNonQueryAsync();
        Console.WriteLine("Type de cours ajouté.");
    }

    private async Task AdminAddSalleAsync(MySqlConnection connection)
    {
        Console.WriteLine("--- Ajouter salle ---");
        string nomSalle = ConsoleHelpers.Prompt("Nom salle: ");
        int capacite = ConsoleHelpers.PromptInt("Capacité max salle: ");

        using var cmd = new MySqlCommand(Queries.AdminAddSalle, connection);
        cmd.Parameters.AddWithValue("?nom_salle", nomSalle);
        cmd.Parameters.AddWithValue("?capacite_max_salle", capacite);
        await cmd.ExecuteNonQueryAsync();

        Console.WriteLine("Salle ajoutée.");
    }

    private async Task AdminAddSeanceAsync(MySqlConnection connection)
    {
        Console.WriteLine("--- Planifier séance ---");
        int idCoach = ConsoleHelpers.PromptInt("Id coach: ");
        int idTypeCours = ConsoleHelpers.PromptInt("Id type cours: ");
        int idSalle = ConsoleHelpers.PromptInt("Id salle: ");
        DateTime debut = ConsoleHelpers.PromptDateTime("Début (YYYY-MM-DD HH:MM): ");
        DateTime fin = ConsoleHelpers.PromptDateTime("Fin (YYYY-MM-DD HH:MM): ");
        int capacite = ConsoleHelpers.PromptInt("Capacité séance: ");

        using var cmd = new MySqlCommand(Queries.AdminAddSeance, connection);
        cmd.Parameters.AddWithValue("?id_coach", idCoach);
        cmd.Parameters.AddWithValue("?id_type_cours", idTypeCours);
        cmd.Parameters.AddWithValue("?id_salle", idSalle);
        cmd.Parameters.AddWithValue("?date_heure_debut", debut);
        cmd.Parameters.AddWithValue("?date_heure_fin", fin);
        cmd.Parameters.AddWithValue("?capacite_max_seance", capacite);

        await cmd.ExecuteNonQueryAsync();
        Console.WriteLine("Séance planifiée.");
    }

    private async Task AdminAnnulerSeanceAsync(MySqlConnection connection, int idAdmin)
    {
        int idSeance = ConsoleHelpers.PromptInt("Id séance à annuler: ");
        using var cmd = new MySqlCommand(Queries.AdminAnnulerSeance, connection);
        cmd.Parameters.AddWithValue("?id_admin", idAdmin);
        cmd.Parameters.AddWithValue("?id_seance", idSeance);

        int rows = await cmd.ExecuteNonQueryAsync();
        Console.WriteLine(rows == 1 ? "Séance annulée." : "Séance non trouvée (ou déjà annulée)." );
    }

    private static async Task RunSimpleQueryAsync(MySqlConnection connection, string sql)
    {
        Console.WriteLine();
        Console.WriteLine("--- Query ---");

        using var cmd = new MySqlCommand(sql, connection);
        using var reader = await cmd.ExecuteReaderAsync();

        int fieldCount = reader.FieldCount;
        while (await reader.ReadAsync())
        {
            for (int i = 0; i < fieldCount; i++)
            {
                string name = reader.GetName(i);
                object value = reader.IsDBNull(i) ? "NULL" : reader.GetValue(i);
                Console.Write($"{name}={value} ");
            }
            Console.WriteLine();
        }
    }
}
