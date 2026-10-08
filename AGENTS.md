# Agent Instructions

## Logging PII in backticks

User fields and other PII (values, columns, file paths, etc.) that appear in log
messages and exception messages must be wrapped in backticks (`` ` ` ``),
e.g. `Field not found: \`schema/name\``.

Backtick-delimited content is replaced with a sentinel before data is sent to
Amplitude (our analytics provider), so anything NOT wrapped in backticks is sent
verbatim. Never interpolate user-controlled values into logs without wrapping
them in backticks.