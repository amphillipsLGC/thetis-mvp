using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var dbUsername = builder.AddParameter("db-username", secret: true);
var dbPassword = builder.AddParameter("db-password", secret: true);

var postgresdb = builder.AddPostgres("postgres", 
        userName: dbUsername, password: dbPassword, port: 5432)
    .WithPgAdmin(opts =>
    {
        opts.WithImage("dpage/pgadmin4:9.5.0");
        opts.WithHostPort(5050);
    })
    .WithDataVolume()
    .AddDatabase("thetis");

builder.AddProject<Thetis_Web>("thetis-web")
    .WithReference(postgresdb)
    .WaitFor(postgresdb);

builder.Build().Run();