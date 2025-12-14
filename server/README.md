# Golay Code Server

A ASP.NET Core API server implementing the binary Golay (23,12,7) error-correcting code for encoding and decoding messages with error correction capabilities.

## What is Golay Code?

The Golay (23,12,7) code is a perfect error-correcting code that:
- Encodes **12 information bits** into **23 coded bits**
- Can **correct up to 3 bit errors**
- Adds **11 parity bits** for error detection and correction
- Used in real-world applications like Voyager spacecraft communications

### How It Works

1. **Encoding**: Takes a 12-bit message and generates 11 parity bits using a generator matrix
2. **Decoding**: Receives a 23-bit codeword and recovers the original 12-bit message, correcting errors if present

## Project Structure

```
server/
├── Controllers/           # HTTP API endpoints
│   └── GolayController.cs
├── Services/             # Business logic layer
│   └── GolayService.cs   # Encoding, decoding, matrix operations
├── Data/                 # Static data (matrices)
│   └── GolayMatrices.cs  # Identity and Parity matrix definitions
├── Program.cs            # Application entry point
└── README.md
```

### Architecture

- **Controllers Layer**: Handles HTTP requests and responses
- **Services Layer**: Contains business logic for Golay encoding/decoding
- **Data Layer**: Stores the mathematical matrices (Identity 12×12, Parity 12×11)

## API Endpoints

### GET /golay/matrix-p
Get the Golay parity matrix (12×11).

**Response:**
```json
[
  [1,1,0,1,1,1,0,0,0,1,0],
  [1,0,1,1,1,0,0,0,1,0,1],
  ...
]
```

### GET /golay/matrix-identity
Get the identity matrix (12×12).

### GET /golay/generator-matrix
Get the full generator matrix (12×23) = [Identity | Parity].

### POST /golay/encode
Encode a 12-bit message into a 23-bit Golay codeword.

**Request:**
```json
{
  "message": 2730
}
```

**Response:**
```json
{
  "message": 2730,
  "messageBinary": "101010101010",
  "codeword": 5678901,
  "codewordBinary": "10101101010101010101010"
}
```

**Constraints:** Message must be 0-4095 (12 bits).

### POST /golay/decode
Decode a 23-bit Golay codeword back to a 12-bit message.

**Request:**
```json
{
  "codeword": 5678901
}
```

**Response:**
```json
{
  "codeword": 5678901,
  "codewordBinary": "10101101010101010101010",
  "message": 2730,
  "messageBinary": "101010101010"
}
```

**Constraints:** Codeword must be 0-8388607 (23 bits).

## Running the Server

### Prerequisites
- .NET 10.0 SDK

### Start the Server

```bash
cd server
dotnet run
```

The server will start on:
- HTTP: `http://localhost:5081`
- HTTPS: `https://localhost:7040`

### Test Endpoints

```bash
# Get parity matrix
curl http://localhost:5081/golay/matrix-p

# Encode a message
curl -X POST http://localhost:5081/golay/encode \
  -H "Content-Type: application/json" \
  -d '{"message": 42}'

# Decode a codeword
curl -X POST http://localhost:5081/golay/decode \
  -H "Content-Type: application/json" \
  -d '{"codeword": 5678901}'
```

## Technology Stack

- **Framework**: ASP.NET Core 10.0
- **Language**: C# 12
- **API Documentation**: OpenAPI (Swagger)
- **Dependency Injection**: Built-in ASP.NET Core DI container

## Configuration

- **GolayMatrices**: Registered as **Singleton** (static data, shared across app)
- **GolayService**: Registered as **Scoped** (per-request instance)

## Development

### Adding New Features

1. Add business logic to `Services/GolayService.cs`
2. Add new endpoints to `Controllers/GolayController.cs`
3. Register new services in `Program.cs` if needed

### Matrix Definitions

The Golay matrices are defined in `Data/GolayMatrices.cs`:
- **Identity Matrix (I)**: 12×12 identity matrix
- **Parity Matrix (P)**: 12×11 parity check matrix
- **Generator Matrix (G)**: 12×23 matrix = [I | P]

## License

Educational project for Coding Theory course.
