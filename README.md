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

After some rearranging of the project, it is possible to simply run via the
following command:

```bash
dotnet run --project webservicelayer
```

## API v2

The original v1 API provided as part of the second portfolio has been updated
to more closely follow RESTful principles. Therefore, a new version of the API
has been created. The v1 API is mostly still available, but it is recommended
to use the v2 API for new development.

### Structure

The v2 API is structured around resources, with each resource having its own
endpoint. As per *Mon Dec 8*, the resources are:

* API v2 Root: `/api/v2`
  * Health: **GET** `/api/v2/health`
  * Users: `/api/v2/users`
    * User: **GET** `/api/v2/users/{userId}`
      * Bookmarks: **GET, POST** `/api/v2/users/{userId}/bookmarks`
        * Bookmark: **GET, DELETE** `/api/v2/users/{userId}/bookmarks/{pageId}`
      * Ratings: **GET, POST** `/api/v2/users/{userId}/ratings`
        * Rating: **GET, PUT, DELETE** `/api/v2/users/{userId}/ratings/{titleId}`
  * Titles: **GET** `/api/v2/titles`
    * Title: **GET** `/api/v2/titles/{titleId}`
      * Ratings: **GET** `/api/v2/titles/{titleId}/ratings`
      * Individuals: **GET** `/api/v2/titles/{titleId}/individuals`
  * Individuals: **GET** `/api/v2/individuals`
    * Individual: **GET** `/api/v2/individuals/{individualId}`
      * Titles: **GET** `/api/v2/individuals/{individualId}/titles`
  * Auth: `/api/v2/auth`
    * Signup: **POST** `/api/v2/auth/signup`
    * Login: **POST** `/api/v2/auth/login`

> [!INFO]
> The structure above attempts to be RESTful compliant, whereby resources
> are nouns and HTTP methods are used to define the action to be performed on
> the resource.
>
> * **GET** is used to retrieve resources.
> * **POST** is used to create resources.
> * **PUT** is used to update resources.
> * **DELETE** is used to delete resources.
