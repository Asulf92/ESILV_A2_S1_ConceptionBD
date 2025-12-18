using ESILV_A2_S1_ConceptionBD.App;

DatabaseConfig db = AppConfig.LoadDatabaseConfig();
var runner = new AppRunner(db);
await runner.RunAsync();
