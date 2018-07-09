namespace OpenAPIProvider.Common

type DateTimeZoneHandling =
    | Local = 0
    | Utc = 1
    | Unspecified = 2
    | RoundtripKind = 3