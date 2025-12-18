```mermaid
erDiagram
    ADMINISTRATEUR {
        int     id_admin PK
        string  nom
        string  prenom
        string  email "UNIQUE"
        string  password_hash
        string  role "PRINCIPAL|SECONDAIRE"
    }

    MEMBRE {
        int     id_membre PK
        string  nom
        string  prenom
        string  adresse
        string  telephone
        string  email "UNIQUE"
        string  password_hash
    }

    ADHESION {
        int      id_adhesion PK
        datetime date_demande
        datetime date_validation "nullable"
        date     date_debut "nullable"
        date     date_fin "nullable"
        string   statut "EN_ATTENTE|VALIDEE|REFUSEE|EXPIREE|RESILIEE"
        int      id_membre FK
        int      id_admin_validateur FK "nullable"
    }

    COACH {
        int     id_coach PK
        string  nom
        string  prenom
        string  telephone
        string  email "UNIQUE"
        string  specialite
        text    formations
        string  password_hash
    }

    TYPE_COURS {
        int     id_type_cours PK
        string  nom_cours
        text    description
        int     capacite_max_cours
        int     id_admin_createur FK "nullable"
    }

    SALLE {
        int     id_salle PK
        string  nom_salle
        int     capacite_max_salle
    }

    SEANCE {
        int      id_seance PK
        datetime date_heure_debut
        datetime date_heure_fin
        int      capacite_max_seance
        string   statut "PLANIFIEE|ANNULEE"
        datetime date_annulation "nullable"
        int      id_admin_annulation FK "nullable"
        int      id_coach FK
        int      id_type_cours FK
        int      id_salle FK
    }

    RESERVATION {
        int      id_reservation PK
        datetime date_reservation
        string   statut "ACTIVE|ANNULEE"
        datetime date_annulation "nullable"
        int      id_membre FK
        int      id_seance FK
    }

    %% Relations principales
    COACH      ||--o{ SEANCE      : anime
    TYPE_COURS ||--o{ SEANCE      : planifie  
    SALLE      ||--o{ SEANCE      : accueille

    MEMBRE     ||--o{ RESERVATION : effectue
    SEANCE     ||--o{ RESERVATION : concerne

    MEMBRE     ||--o{ ADHESION    : souscrit

    %% Traçabilité admin (point 7 + 2 types)
    ADMINISTRATEUR o|--o{ ADHESION   : valide
    ADMINISTRATEUR o|--o{ SEANCE     : annule
    ADMINISTRATEUR o|--o{ TYPE_COURS : cree
