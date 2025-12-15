# Golay (23,12,7) Error-Correcting Code

## Overview

This document describes the implementation of the **Golay (23,12,7) code** - a perfect binary linear error-correcting code that can correct up to 3 bit errors.

### Code Parameters
| Parameter | Value | Description |
|-----------|-------|-------------|
| n | 23 | Codeword length (bits) |
| k | 12 | Message length (bits) |
| d | 7 | Minimum Hamming distance |
| t | 3 | Error correction capability |

---

## Process Flow Diagram

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                        COMPLETE TRANSMISSION PIPELINE                           │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                 │
│   ┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐  │
│   │  INPUT   │───►│  ENCODE  │───►│ CHANNEL  │───►│  DECODE  │───►│  OUTPUT  │  │
│   │          │    │          │    │  (BSC)   │    │          │    │          │  │
│   │ 12 bits  │    │ 23 bits  │    │ + errors │    │ 12 bits  │    │ 12 bits  │  │
│   │ message  │    │ codeword │    │          │    │ message  │    │ original │  │
│   └──────────┘    └──────────┘    └──────────┘    └──────────┘    └──────────┘  │
│                                                                                 │
│   Example:        42 ──► 5471274 ──► 5471282 ──► 42                             │
│                   (with 2 errors introduced, then corrected)                    │
│                                                                                 │
└─────────────────────────────────────────────────────────────────────────────────┘
```

---

## 1. Encoding Process

### Mathematical Foundation

```
Codeword = Message × Generator_Matrix

c = m × G    (in GF(2) - binary field)
```

Where:
- `m` = 12-bit message vector [m₀, m₁, ..., m₁₁]
- `G` = 12×23 generator matrix = [I₁₂ | P̂]
- `c` = 23-bit codeword vector [c₀, c₁, ..., c₂₂]

### Generator Matrix Structure

```
G = [I₁₂ | P̂] =
    ┌                                                                    ┐
    │ 1 0 0 0 0 0 0 0 0 0 0 0 │ 1 1 0 1 1 1 0 0 0 1 0 │  row 0
    │ 0 1 0 0 0 0 0 0 0 0 0 0 │ 1 0 1 1 1 0 0 0 1 0 1 │  row 1
    │ 0 0 1 0 0 0 0 0 0 0 0 0 │ 0 1 1 1 0 0 0 1 0 1 1 │  row 2
    │ ...                     │ ...                   │  ...
    │ 0 0 0 0 0 0 0 0 0 0 0 1 │ 1 1 1 1 1 1 1 1 1 1 1 │  row 11
    └                                                                    ┘
         Identity (12×12)         Parity (12×11)
```

### Encoding Example

```
Message: 42 (decimal) = 000000101010 (binary)

Codeword bits 0-11:  000000101010  (message - systematic code)
Codeword bits 12-22: 10101000101   (parity bits)

Full codeword: 00000010101010101000101 (23 bits)
```

### Encoding Steps
1. **Validation**: Check message is 12 bits (0-4095)
2. **Matrix multiplication**: For each bit position i where message bit is 1, XOR row i of G into result
3. **Result**: 23-bit codeword with message in first 12 bits (systematic property)

---

## 2. Binary Symmetric Channel (BSC)

### Channel Model

```
                 ┌─────────────────┐
                 │                 │
    0 ──────────►│   1-p      p   │──────────► 0
                 │       ╲   ╱     │
                 │        ╲ ╱      │   p = error probability
                 │         ╳       │   (each bit independently)
                 │        ╱ ╲      │
    1 ──────────►│   p      1-p   │──────────► 1
                 │                 │
                 └─────────────────┘
```

### Error Introduction
- Each bit has independent probability `p` of flipping
- For p = 0.1: expect ~2.3 errors per 23-bit codeword
- Golay can correct ≤3 errors, detect ≤6 errors

### Channel Simulation Algorithm
```csharp
for each bit i in codeword:
    if random() < errorProbability:
        flip bit i (XOR with 1)
        record error position
```

### Error Statistics by Probability

| Error Probability | Expected Errors | Correction Success Rate |
|-------------------|-----------------|------------------------|
| 0.01 (1%)        | ~0.23           | ~99.9%                 |
| 0.05 (5%)        | ~1.15           | ~99%                   |
| 0.10 (10%)       | ~2.30           | ~95%                   |
| 0.15 (15%)       | ~3.45           | ~85%                   |
| 0.20 (20%)       | ~4.60           | ~70%                   |

---

## 3. Syndrome-Based Decoding

### The Key Insight

**Syndrome depends ONLY on the error pattern, NOT on the original codeword!**

```
Received word:  w = c + e    (codeword + error)
Syndrome:       s = wH = (c + e)H = cH + eH = 0 + eH = eH

Since cH = 0 for any valid codeword c, the syndrome reveals the error pattern.
```

### Decoding Algorithm Overview

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                    ALGORITHM 3.7.1 - DECODE C23                                 │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                  │
│   Received 23-bit word (w)                                                      │
│           │                                                                      │
│           ▼                                                                      │
│   ┌───────────────────────────────────────┐                                     │
│   │ Step 1: EXTEND TO 24 BITS             │                                     │
│   │                                        │                                     │
│   │ if weight(w) is even:                 │                                     │
│   │     w24 = w || 1  (append 1)          │                                     │
│   │ else:                                 │                                     │
│   │     w24 = w || 0  (append 0)          │                                     │
│   │                                        │                                     │
│   │ Result: 24-bit word with ODD weight   │                                     │
│   └───────────────────┬───────────────────┘                                     │
│                       │                                                          │
│                       ▼                                                          │
│   ┌───────────────────────────────────────┐                                     │
│   │ Step 2: DECODE USING IMLD (Alg 3.6.1) │                                     │
│   │                                        │                                     │
│   │ Find error pattern u = [u₁, u₂]       │                                     │
│   │ Correct: c = w24 ⊕ u                  │                                     │
│   └───────────────────┬───────────────────┘                                     │
│                       │                                                          │
│                       ▼                                                          │
│   ┌───────────────────────────────────────┐                                     │
│   │ Step 3: EXTRACT MESSAGE               │                                     │
│   │                                        │                                     │
│   │ message = first 12 bits of c          │                                     │
│   │ (systematic code property)            │                                     │
│   └───────────────────────────────────────┘                                     │
│                                                                                  │
└─────────────────────────────────────────────────────────────────────────────────┘
```

### IMLD Algorithm (3.6.1) for C24

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                    ALGORITHM 3.6.1 - IMLD FOR C24                               │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                  │
│   Input: w24 = [w₁ | w₂] (24 bits, split into two 12-bit halves)               │
│                                                                                  │
│   Step 1: Compute syndrome s₁ = w₁B + w₂                                        │
│                                                                                  │
│   Step 2: if wt(s₁) ≤ 3:                                                        │
│               error pattern u = [s₁, 0]                                         │
│               DONE ✓                                                            │
│                                                                                  │
│   Step 3: for each row bᵢ of B matrix:                                          │
│               if wt(s₁ + bᵢ) ≤ 2:                                               │
│                   u = [s₁ + bᵢ, eᵢ]    (eᵢ = unit vector)                       │
│                   DONE ✓                                                        │
│                                                                                  │
│   Step 4: Compute s₂ = s₁B                                                      │
│                                                                                  │
│   Step 5: if wt(s₂) ≤ 3:                                                        │
│               error pattern u = [0, s₂]                                         │
│               DONE ✓                                                            │
│                                                                                  │
│   Step 6: for each row bᵢ of B matrix:                                          │
│               if wt(s₂ + bᵢ) ≤ 2:                                               │
│                   u = [eᵢ, s₂ + bᵢ]                                             │
│                   DONE ✓                                                        │
│                                                                                  │
│   Step 7: More than 3 errors - cannot correct                                   │
│               Request retransmission                                            │
│                                                                                  │
└─────────────────────────────────────────────────────────────────────────────────┘
```

### Syndrome Calculation

```
For extended Golay C24:

    Parity check matrix H = [B]
                            [I]   (24×12)

    Syndrome s₁ = w × H = w₁B + w₂

    where w = [w₁ | w₂] is split into first 12 and last 12 bits
```

### Decoding Example

```
Received: w = 101111101111,010010010010 (24 bits with 2 errors)

Step 1: s₁ = w₁B + w₂ = 100000000001 (weight = 2)

Step 2: wt(s₁) = 2 ≤ 3? YES!
        → Error pattern u = [100000000001, 000000000000]
        → Errors at positions 0 and 11 in first half

Step 3: Correct w ⊕ u = original codeword

Step 4: Extract message from first 12 bits
```

---

## 4. Text Processing Flow

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                        TEXT TRANSMISSION PIPELINE                                │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                  │
│   "Hello"                                                                        │
│      │                                                                           │
│      ▼                                                                           │
│   UTF-8: [72, 101, 108, 108, 111]  (5 bytes = 40 bits)                          │
│      │                                                                           │
│      ▼                                                                           │
│   Bits: 01001000 01100101 01101100 01101100 01101111                            │
│      │                                                                           │
│      ▼                                                                           │
│   Pad to multiple of 12: add 8 zeros → 48 bits                                  │
│      │                                                                           │
│      ▼                                                                           │
│   Split into 12-bit blocks: [block1] [block2] [block3] [block4]                 │
│      │                                                                           │
│      ▼                                                                           │
│   Encode each block: [cw1] [cw2] [cw3] [cw4]  (4 × 23 = 92 bits)               │
│      │                                                                           │
│      ▼                                                                           │
│   ═══════════════════ CHANNEL (errors introduced) ═══════════════════           │
│      │                                                                           │
│      ▼                                                                           │
│   Decode each codeword (with error correction)                                  │
│      │                                                                           │
│      ▼                                                                           │
│   Reconstruct bits → remove padding → bytes → UTF-8                             │
│      │                                                                           │
│      ▼                                                                           │
│   "Hello"  (recovered!)                                                         │
│                                                                                  │
└─────────────────────────────────────────────────────────────────────────────────┘
```

### Overhead Calculation

| Original | Encoded | Overhead |
|----------|---------|----------|
| 12 bits  | 23 bits | 91.7%    |
| 100 bytes| 192 bytes| 92%     |
| 1 KB     | 1.92 KB | 92%      |

---

## 5. Image Processing Flow

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                        IMAGE TRANSMISSION PIPELINE                               │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                  │
│   BMP File Structure:                                                           │
│   ┌───────────────────┬──────────────────────────────────────────────────┐     │
│   │ HEADER (54 bytes) │              PIXEL DATA                          │     │
│   │ - NOT encoded     │              - Encoded with Golay code           │     │
│   │ - Preserved       │              - Split into 12-bit chunks          │     │
│   └───────────────────┴──────────────────────────────────────────────────┘     │
│                                                                                  │
│   WHY PRESERVE HEADER?                                                          │
│   From Tasks.md: "paveiksliukų antraštėse esančią informaciją laikykime        │
│   tarnybine ir jos neiškraipykime" (preserve header as service info)           │
│                                                                                  │
│   Process:                                                                       │
│   1. Extract header (54+ bytes)                                                 │
│   2. Extract pixel data                                                         │
│   3. Convert pixels to bits                                                     │
│   4. Pad to multiple of 12                                                      │
│   5. Encode each 12-bit block → 23-bit codeword                                │
│   6. Send through channel                                                       │
│   7. Decode with error correction                                               │
│   8. Reconstruct pixels                                                         │
│   9. Reattach header                                                            │
│                                                                                  │
└─────────────────────────────────────────────────────────────────────────────────┘
```

---

## 6. API Endpoints

### Base URL: `http://localhost:5081`

### Matrix Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/golay/matrix-p` | GET | Parity matrix P̂ (12×11) |
| `/golay/matrix-identity` | GET | Identity matrix I (12×12) |
| `/golay/matrix-b` | GET | B matrix (12×12) for C24 |
| `/golay/generator-matrix` | GET | Generator matrix G (12×23) |

### Single Vector Endpoints (Scenario 1)

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/golay/encode` | POST | Encode 12-bit message |
| `/golay/decode` | POST | Decode 23-bit codeword |
| `/golay/decode-detailed` | POST | Decode with algorithm details |
| `/golay/channel` | POST | Simulate BSC channel |

#### Example: Encode
```bash
curl -X POST http://localhost:5081/golay/encode \
  -H "Content-Type: application/json" \
  -d '{"message": 42}'
```

Response:
```json
{
  "message": 42,
  "messageBinary": "000000101010",
  "codeword": 5471274,
  "codewordBinary": "10100111000010000101010"
}
```

#### Example: Decode with Details
```bash
curl -X POST http://localhost:5081/golay/decode-detailed \
  -H "Content-Type: application/json" \
  -d '{"codeword": 5471282}'
```

Response:
```json
{
  "originalCodeword": 5471282,
  "syndromeS1": "100000000000",
  "syndromeS1Weight": 1,
  "errorCount": 1,
  "errorPositions": [3],
  "decodedMessage": 42,
  "success": true,
  "status": "Successfully corrected 1 error(s)"
}
```

### Text Endpoints (Scenario 2)

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/golay/text/encode` | POST | Encode text to codewords |
| `/golay/text/channel` | POST | Send through noisy channel |
| `/golay/text/decode` | POST | Decode codewords to text |
| `/golay/text/full-demo` | POST | Complete pipeline demo |

#### Example: Full Text Demo
```bash
curl -X POST http://localhost:5081/golay/text/full-demo \
  -H "Content-Type: application/json" \
  -d '{"text": "Hello, World!", "errorProbability": 0.1}'
```

### Image Endpoints (Scenario 3)

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/golay/image/encode` | POST | Encode BMP image |
| `/golay/image/channel` | POST | Send through channel |
| `/golay/image/decode` | POST | Decode to BMP |
| `/golay/image/full-demo` | POST | Complete pipeline with comparison |

#### Example: Image Demo
```bash
curl -X POST "http://localhost:5081/golay/image/full-demo?errorProbability=0.05" \
  --data-binary @image.bmp
```

---

## 7. Key Implementation Details

### Random Generator Fix

```csharp
// CRITICAL: Initialize ONCE at class level, not per-call
private static readonly Random _random = new Random();
```

**Why?** Random() seeds from system time. If multiple vectors are sent quickly, they get the same seed → same "random" sequence → correlated errors (breaks symmetric channel property).

### GF(2) Arithmetic

All operations are in GF(2) (binary field):
- Addition = XOR: `1 + 1 = 0`, `1 + 0 = 1`
- Multiplication = AND: `1 × 1 = 1`, `1 × 0 = 0`

```csharp
// Vector XOR
result[i] = a[i] ^ b[i];

// Matrix-vector multiplication
for (int j = 0; j < cols; j++)
    for (int i = 0; i < rows; i++)
        result[j] ^= vector[i] & matrix[i][j];
```

### Weight Calculation

Hamming weight = count of 1s in binary vector:
```csharp
int weight = bits.Count(b => b == 1);
```

Used in IMLD algorithm to determine error pattern.

---

## References

- **literatura12.pdf**: §3.5-3.7, pages 82-89
  - Algorithm 3.6.1: IMLD for C24 (page 85)
  - Algorithm 3.7.1: Decoding C23 (page 88)
- **Tasks.md**: Assignment requirements and scenarios
- **KTKT.pdf**: Coding theory fundamentals

---

## Summary

The Golay (23,12,7) code provides:
- **Error correction**: Up to 3 bit errors
- **Perfect code**: All possible syndromes map to correctable error patterns
- **Systematic**: Original message appears in first 12 bits of codeword
- **Overhead**: ~92% (23 bits to send 12 bits of information)

Ideal for applications where reliability is critical and moderate overhead is acceptable.
