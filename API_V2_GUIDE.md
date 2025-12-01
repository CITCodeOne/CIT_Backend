# API v2 Implementation

## Overview

This branch introduces a new REST-compliant API (v2) while maintaining full backward compatibility with the existing API (v1).

## Changes Made

### 1. Project Structure
```
Controllers/
  ├── V1/  (existing API - unchanged)
  │   ├── TitlesController.cs       → /api/titles
  │   ├── UserController.cs         → /api/user
  │   ├── BookmarksController.cs    → /api/bookmarks
  │   ├── RatingsController.cs      → /api/ratings
  │   └── IndividualsController.cs  → /api/individuals
  │
  └── V2/  (new REST-compliant API)
      ├── AuthController.cs         → /api/v2/auth
      ├── UsersController.cs        → /api/v2/users
      ├── TitlesController.cs       → /api/v2/titles
      └── IndividualsController.cs  → /api/v2/individuals
```

### 2. V1 API (Unchanged)
All existing endpoints remain functional. The team can continue using:
- `POST /api/user` (signup)
- `POST /api/user/login`
- `GET /api/bookmarks`
- `GET /api/titles/{id}`
- etc.

### 3. V2 API (New REST-Compliant Design)

#### Authentication Endpoints
```
POST /api/v2/auth/signup
  Body: { "username": "...", "password": "...", "email": "...", "name": "..." }
  
POST /api/v2/auth/login
  Body: { "username": "...", "password": "..." }
  Response: { "username": "...", "token": "..." }
```

#### User Resource Endpoints
```
GET /api/v2/users/{userId}
  Auth: Required
  Response: User profile information
```

#### User Bookmarks (Nested Resource)
```
GET /api/v2/users/{userId}/bookmarks
  Auth: Required
  Response: List of user's bookmarks
  
POST /api/v2/users/{userId}/bookmarks
  Auth: Required
  Body: { "pconst": 123 }
  Response: 201 Created
  
DELETE /api/v2/users/{userId}/bookmarks/{bookmarkId}
  Auth: Required
  Response: 204 No Content
```

#### User Ratings (Nested Resource)
```
GET /api/v2/users/{userId}/ratings
  Auth: Required
  Response: List of user's ratings
  
POST /api/v2/users/{userId}/ratings
  Auth: Required
  Body: { "titleId": "tt1234567", "rating": 8 }
  Response: 201 Created
  
PUT /api/v2/users/{userId}/ratings/{titleId}
  Auth: Required
  Body: { "rating": 9 }
  Response: 200 OK
  
DELETE /api/v2/users/{userId}/ratings/{titleId}
  Auth: Required
  Response: 204 No Content
```

#### Title Endpoints
```
GET /api/v2/titles?page=1&pageSize=20
  Response: List of titles (preview)
  
GET /api/v2/titles/{titleId}
  Response: Full title details
  
GET /api/v2/titles/{titleId}/ratings
  Response: All ratings for this title
```

#### Individual Endpoints
```
GET /api/v2/individuals?page=1&pageSize=20
  Response: List of individuals (reference)
  
GET /api/v2/individuals/{individualId}
  Response: Full individual details
```

## Key Improvements in V2

### 1. **Proper REST Structure**
- Resources are organized hierarchically
- Bookmarks and ratings are nested under users (they're user-owned resources)
- Authentication separated from user management

### 2. **Removed Confusing Endpoints**
- No more `/preview` endpoints (just use the appropriate endpoint)
- No more composite keys in URL paths
- No more confusing query parameters on ratings

### 3. **Better HTTP Semantics**
- `201 Created` responses for POST requests
- `204 No Content` for DELETE requests
- Proper use of PUT vs POST
- Location headers on created resources

### 4. **Authorization Checks**
- Users can only access/modify their own bookmarks and ratings
- Returns `403 Forbidden` if trying to access another user's resources

## Business Layer Changes

Added missing methods to services:

### UserService
- `GetUserById(int uconst)` - Get user by ID

### RatingService
- `CreateRating(int uconst, string tconst, int ratingValue)` - Create new rating
- `UpdateRating(int uconst, string tconst, int ratingValue)` - Update existing rating
- `DeleteRating(int uconst, string tconst)` - Delete rating

## Migration Guide for Frontend Team

### For Existing Code (V1)
**No changes required!** All existing endpoints continue to work.

### For New Features (V2)
Update your API calls to use the new structure:

**Before (V1):**
```javascript
// Login
POST /api/user/login

// Get bookmarks
GET /api/bookmarks  // uses token to identify user

// Add bookmark
POST /api/bookmarks
{ "pconst": 123 }
```

**After (V2):**
```javascript
// Login
POST /api/v2/auth/login

// Get bookmarks (explicit user ID)
GET /api/v2/users/{userId}/bookmarks

// Add bookmark (explicit user ID)
POST /api/v2/users/{userId}/bookmarks
{ "pconst": 123 }
```

### Getting User ID
After login, extract the user ID from the JWT token claims (it's in the `uid` claim) or store it from the login response.

## Testing the API

### Start the Server
```bash
dotnet run --project WebServiceLayer
```

### Test V1 (should still work)
```bash
# Get a title
curl http://localhost:5001/api/titles/tt0052520
```

### Test V2
```bash
# Login
curl -X POST http://localhost:5001/api/v2/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"test","password":"test123"}'

# Get user bookmarks (replace {token} and {userId})
curl http://localhost:5001/api/v2/users/1/bookmarks \
  -H "Authorization: Bearer {token}"

# Get a title
curl http://localhost:5001/api/v2/titles/tt0052520
```

## Next Steps

1. **Test the V2 endpoints** with your frontend
2. **Gradually migrate** from V1 to V2 as features are developed
3. **Provide feedback** on any missing endpoints or issues
4. **Eventually deprecate V1** once all frontends are migrated

## Notes

- Both V1 and V2 share the same business logic and database
- V2 doesn't change any data structures, only API design
- The HealthController remains unchanged (no versioning needed)
