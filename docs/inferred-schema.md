# Inferred and locally verified schema

No `schema.sql` dump is present in this workspace, and the original legacy
database is unavailable. The column inventory below was checked against the
local development database through `information_schema.columns`. That verifies
the current local schema only; it does not prove that the old database has the
same definitions.

**KESIN** marks a name directly required by executable source or observed in
the local database. **TAHMIN** marks an inferred type, nullability, constraint,
relationship, or required data value that cannot be established from the old
database.

## Database naming convention

Database identifiers use snake_case. Raw SQL and data-reader names in
`WebApi/Controllers/HomeController.cs` must match database column names.
C# property names and API/JSON names remain unchanged.

The applied `20261030000000_AddInferredTables` migration created columns that
were not consistently snake_case. The new
`20261030014527_AlignDatabaseNames` migration renames those columns without
dropping data:

| Table(s) | Previous name | Database name |
|---|---|---|
| `transformer`, `street`, `straight`, `subscriber` | `streetnumber` | `street_number` |
| same | `streetname` | `street_name` |
| `user_layer_settings` | `layerid` | `layer_id` |
| same | `userid` | `user_id` |
| same | `isopen` | `is_open` |
| same | `minzoom` | `min_zoom` |
| same | `maxzoom` | `max_zoom` |

`layers` already has `group_id`, `is_open`, `min_zoom`, and `max_zoom` in the
local database and EF mapping. `layer_group` uses `id` and `"group"`; the latter
is a single word and is quoted in SQL because it is a PostgreSQL keyword.
`straight.straight_type` is read using its database spelling.

## Newly added columns

The new migration adds the following nullable columns to each of `city`,
`county`, `neighborhood`, and `building`:

| Column | Confidence and inferred type | Evidence |
|---|---|---|
| `street_number` | KESIN name; TAHMIN `numeric NULL` | Generic `Put(ToDo)` writes `line.Number`; `ToDo.Number` is `decimal?`. |
| `street_name` | KESIN name; TAHMIN `text NULL` | Generic `Put(ToDo)` writes `line.Name`; `ToDo.Name` is `string?`. |

The previous schema did not have these columns in the local database. The
migration preserves existing table data and does not drop columns.

## Inferred tables used by raw SQL

`transformer`, `street`, `straight`, and `subscriber` were created by the
already-applied `AddInferredTables` migration. Their names and fields below
come from the insert, update, delete, and read SQL in
`WebApi/Controllers/HomeController.cs`. Their actual local column names and
types were checked against `information_schema.columns` before the new rename
migration.

| Table | Columns required by code |
|---|---|
| `transformer` | `id`, `number`, `name`, `coordinate`, `measurement`, `feature_type`, `capacity`, `located_city`, `located_county`, `located_neighborhood`, `street_number`, `street_name` |
| `street` | `id`, `number`, `name`, `coordinate`, `measurement`, `feature_type`, `located_city`, `located_county`, `located_neighborhood`, `street_number`, `street_name` |
| `straight` | `id`, `number`, `name`, `coordinate`, `measurement`, `feature_type`, `located_city`, `straight_type`, `street_number`, `street_name` |
| `subscriber` | `id`, `number`, `name`, `coordinate`, `measurement`, `feature_type`, `located_city`, `located_county`, `located_neighborhood`, `street_number`, `street_name` |
| `layer_group` | `id`, `"group"` |
| `user_layer_settings` | `layer_id`, `user_id`, `priority`, `is_open`, `min_zoom`, `max_zoom` |

The existing migration defines the first four tables' `id` as numeric primary
keys with sequences. It defines their location/description fields as nullable
text, numeric identifiers as nullable integers, and `straight_type` as nullable
text. These definitions are **KESIN for the applied local migration**, but
remain **TAHMIN for the inaccessible legacy database**.

`layer_group.id` is a numeric primary key. `user_layer_settings` has nullable
numeric `layer_id`, `user_id`, `priority`, `min_zoom`, `max_zoom` and nullable
boolean `is_open` in the local schema. The old database's nullability,
constraints, indexes, and exact types remain TAHMIN.

## Other source-derived requirements

* `layers` is queried by `id`, `group_id`, `layer`, `tag`, `priority`,
  `min_zoom`, and `max_zoom`; `DefaultLayerSettings` joins `layer_group.id`
  and user-specific settings. The local database and EF mapping contain the
  snake_case layer columns.
* `user_layer_settings` joins `layers.id` through `layer_id` and filters by
  `user_id`. No active source SQL proves foreign keys or uniqueness.
* `city`, `county`, `neighborhood`, and `building` are read for
  `id`, `number`, `name`, `coordinate`, `measurement`, `feature_type`, and
  `located_city`; feature-specific inserts/updates also use their dedicated
  fields. `GetInfoMap` reads `id`, `feature_type`, and `coordinate` from these
  feature tables.
* `map_info` is represented by an EF entity but the map-selection endpoint
  constructs response objects from feature-table rows; that endpoint does not
  require pre-populated `map_info` rows.
* `user` is used by EF authentication with `id`, `user_name`, and `password`.
  Registration hashes passwords with ASP.NET Core Identity's
  `PasswordHasher<User>`; login also upgrades legacy plaintext passwords.
* Geometry selection parses WKT from the `coordinate` text and uses .NET
  geometry buffering. The code does not establish whether the old database
  used text or a spatial type.

## Required data and unresolved assumptions

Migration `20261030020633_SeedCodeRequiredLayers` idempotently adds layer
definitions for `city`, `county`, `neighborhood`, `building`, `transformer`,
`street`, `straight`, and `subscriber` only when a matching `layers.layer`
entry does not already exist. These identifiers are defined by the migration
and correspond to the tables accepted by `HomeController.TableSelector`; the
`tag` uses the same identifier for the layer-control label. At runtime, the
web app obtains WMS layer names from the API rather than maintaining a second
hard-coded list. No coordinates,
population values, zoom limits, priority, or group/category assignment are
invented. `layer_group` records are not seeded because the code does not define
a default group. Missing group labels are read as null and remain usable in
the layer control.

`user_layer_settings` rows are synthesized as closed/default settings by
`DefaultLayerSettings`; the app writes a user's rows only when they save
settings. No per-user rows are seeded. `map_info` is not queried by the map
selection endpoint, so it needs no default rows.

The local database initially had no layer/group/feature rows. Runtime tests
used temporary, prefixed rows and removed them afterward. The production-style
seed contains only code-defined layer identifiers. No default user account or
credentials are seeded; create a local account through the API registration
endpoint.

`AlignDatabaseNames` and `SeedCodeRequiredLayers` deliberately reject
downgrades rather than drop columns, names, or existing layer records.

The following remain **TAHMIN** until checked against the original database or
verified with appropriate domain data:

1. Types/nullability of all legacy columns and the existing constraints,
   indexes, and sequence/default behavior.
2. `street_number numeric NULL` and `street_name text NULL` on the four
   existing feature tables; type choice follows `ToDo.Number` and `ToDo.Name`.
3. Whether layer-group and layer-user joins have foreign keys or uniqueness
   guarantees.
4. Which layer definitions/tags/groups and geographic records should be
   populated. No real-world coordinates, populations, or city data are seeded.
5. Any behavior relying on commented-out SQL; it is not an active endpoint.
