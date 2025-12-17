# My Project

## Export CSV Format

The admin export endpoint `POST /api/items/export` returns a CSV in the **canonical** format used by both download and upload.

The exact header order/labels are defined in `api/portalApi/Services/ItemCsvFormat.cs` and the upload preview requires an exact match.

Notes:
- Optional text fields are empty when values are not set.
- Boolean fields (PHI, PII, Has RLS) are nullable; when missing they export as `Missing Data`, otherwise `true`/`false`.
- Tags are semicolon-separated; Data Consumers is a single free-text field.
