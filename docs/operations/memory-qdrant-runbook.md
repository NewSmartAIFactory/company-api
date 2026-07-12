# Memory and Qdrant Runbook

## Readiness

Check `GET /api/health/` and confirm `components.qdrant`, `components.postgres`,
`components.redis`, and `components.rabbitmq` are true.

## Re-indexing

Re-index active memories in pages:

```powershell
Invoke-RestMethod 'http://localhost:5000/api/memory/reindex?limit=200&offset=0' -Method Post
```

Use the returned `nextOffset` until `complete` is true.

## Recovery

If Qdrant loses its collection, recreate the local infrastructure service and
run the paginated re-index operation. PostgreSQL remains the source of truth.

## Search behavior

`/api/memory/semantic-search` uses the configured embedding provider. The local
default is deterministic and dependency-free; replace `IEmbeddingProvider` with
a production provider before enabling external embeddings.
