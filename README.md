How to Run

Follow these steps to build and run the entire MatchMaking system using Docker Compose.

1. Requirements

Ensure the following are installed:

Docker Desktop

Docker Compose v2

.NET 9 SDK (only if running locally without Docker)

2. Start the Infrastructure

From the project root (where docker-compose.yml is located), run:

docker compose up --build


This command will start:

Zookeeper

Kafka

Redis

MatchMaking.Service

MatchMaking.Worker (2 instances)

3. Verify the Service is Running

Open:

http://localhost:8080/swagger


If Swagger is enabled, you will see the API documentation.

Otherwise, test manually using curl or Postman.

4. Test Matchmaking

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

