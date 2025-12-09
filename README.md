API base URL: http://localhost:8080

---

## Endpoints

### Search for match
```
POST /match/search?userId={id}
```

### Get last match
```
GET /match?userId={id}
```

Example response:
```
{
  "matchId": "guid",
  "userIds": ["user1", "user2", "user3"]
}
```

---

## Architecture

Service → Kafka(requests) → Workers → Kafka → Service → Redis → Client

---

## Quick Test
```
curl -X POST "http://localhost:8080/match/search?userId=a"
curl -X POST "http://localhost:8080/match/search?userId=b"
curl -X POST "http://localhost:8080/match/search?userId=c"
curl "http://localhost:8080/match?userId=a"
