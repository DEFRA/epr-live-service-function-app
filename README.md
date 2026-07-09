# EPR Live Service Function App
 
An internal admin toolkit for the EPR (Extended Producer Responsibility) live service. Exposes a set of parameterised SQL queries as HTTP endpoints, each with an auto-generated HTML form and results rendered as an ASCII table (or CSV) — no separate frontend, no database client needed for common lookups.
 
Built as an Azure Function App

## How it works
 
Each query is defined by two files, added independently of any other code change:
 
```
Queries/
├── Definitions/
│   └── organisation_details.json   ← id, display name, description, target DB, parameters
└── Scripts/
    └── organisation_details.sql    ← the SQL itself, using @ParamName placeholders
```
 
The `QueryRegistry` loads both as embedded resources at startup and validates every definition has a matching script (fails fast if not). Nothing else in the app needs to change to add a new query.
 
### Adding a new query
 
1. Write the `.sql` script in `Queries/Scripts/{id}.sql`, using `@ParamName` placeholders for anything parameterised.
2. Write the matching `Queries/Definitions/{id}.json`:
```json
   {
     "id": "example_query",
     "displayName": "Example Query",
     "description": "What this query returns and why you'd run it",
     "target": "accounts",
     "parameters": [
       { "name": "ReferenceNumber", "label": "Reference Number", "type": "text", "required": true }
     ]
   }
```

`target` must match a key configured under `SqlTargets` (see below).

## Routes
 
| Route | Purpose |
|---|---|
| `GET /api/queries` | HTML list of all registered queries, linking to each form |
| `GET /api/query/{queryId}` | Auto-generated HTML form for the query's parameters |
| `GET /api/query/{queryId}/results?{params}` | Runs the query, returns results (`?output=ascii_table` default, or `?output=csv`) |

