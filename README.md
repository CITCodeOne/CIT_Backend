# CIT_Backend

Git repository for Subproject 2

When running for the first time you need to generate a cert for https.
you need to type this in your terminal

```bash
dotnet dev-certs https --trust
```

After a cert has be generated, you should now run the backed server through the command:

```bash
dotnet run --project BackendSolution/WebServiceLayer/WebServiceLayer.csproj --launch-profile https
```

After some rearranging of the project, it is possible to simply run via the following command:

```bash
dotnet run --project webservicelayer
```
