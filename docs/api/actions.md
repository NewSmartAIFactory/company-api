# Company API Actions

## Update Task Status

```http
PATCH /api/tasks/{id}/status
Content-Type: application/json
```

```json
{
  "status": "Done"
}
```

Allowed statuses:

- Todo
- Doing
- Done
- Blocked

## Approve Decision

```http
POST /api/decisions/{id}/approve
Content-Type: application/json
```

```json
{
  "decidedBy": "CEO",
  "reason": "Approved for Sprint 1.2"
}
```

## Reject Decision

```http
POST /api/decisions/{id}/reject
Content-Type: application/json
```

```json
{
  "decidedBy": "CEO",
  "reason": "Rejected due to timing"
}
```