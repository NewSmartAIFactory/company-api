param([string]$BaseUrl = "http://localhost:5000")
$ErrorActionPreference = "Stop"
$health = Invoke-RestMethod "$BaseUrl/api/health/"
if ($health.status -eq "unavailable") { throw "API health unavailable" }
$qdrant = Invoke-RestMethod "$BaseUrl/api/memory/qdrant/health"
if (-not $qdrant.healthy) { throw "Qdrant collection is unhealthy" }
$search = Invoke-RestMethod "$BaseUrl/api/memory/semantic-search?q=idempotent&limit=5"
$reindex = Invoke-RestMethod "$BaseUrl/api/memory/reindex?limit=10&offset=0" -Method Post
[pscustomobject]@{ api = $health.status; qdrant = $qdrant.healthy; semanticResults = $search.Count; reindexed = $reindex.indexed; complete = $reindex.complete } | ConvertTo-Json
