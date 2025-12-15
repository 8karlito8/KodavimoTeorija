# Channel Simulation Implementation Guide

## Backend Implementation ✅ COMPLETE

I've added a **Binary Symmetric Channel (BSC)** simulator to your backend.

### New API Endpoint

```
POST http://localhost:5081/golay/channel
```

**Request:**
```json
{
  "codeword": 5678901,
  "errorProbability": 0.1
}
```

**Response:**
```json
{
  "originalCodeword": 5678901,
  "originalCodewordBinary": "10101101010101010101010",
  "errorProbability": 0.1,
  "corruptedCodeword": 5678905,
  "corruptedCodewordBinary": "10101101010101010101001",
  "errorCount": 2,
  "errorPositions": [0, 14],
  "canCorrect": true,
  "status": "✓ 2 error(s) - Can be corrected by Golay code"
}
```

---

## Frontend Implementation (React)

### Stage 5 Assignment Extension: Channel Simulator Component

Add this to your `ErrorSimulator.tsx` or create a new `ChannelSimulator.tsx`

---

### Option 1: Simple UI with Slider

```typescript
import { useState } from 'react';

interface ChannelSimulatorProps {
  codeword: number;
}

export default function ChannelSimulator({ codeword }: ChannelSimulatorProps) {
  // STATE: Error probability (0.0 to 0.5)
  // 0.0 = no errors, 0.1 = 10% chance per bit, 0.5 = 50% chance
  const [errorProbability, setErrorProbability] = useState(0.1);

  // STATE: Channel simulation result
  const [result, setResult] = useState<ChannelResult | null>(null);
  const [loading, setLoading] = useState(false);

  // FUNCTION: Send codeword through channel
  async function simulateChannel() {
    setLoading(true);
    try {
      const response = await fetch('http://localhost:5081/golay/channel', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          codeword: codeword,
          errorProbability: errorProbability
        })
      });
      const data = await response.json();
      setResult(data);
    } catch (error) {
      console.error('Channel simulation failed:', error);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="channel-simulator">
      <h3>Binary Symmetric Channel Simulator</h3>

      {/* SLIDER: Control error probability */}
      <div className="control-group">
        <label>
          Error Probability: {(errorProbability * 100).toFixed(1)}%
        </label>
        <input
          type="range"
          min="0"
          max="0.5"
          step="0.01"
          value={errorProbability}
          onChange={(e) => setErrorProbability(Number(e.target.value))}
        />
        <small>
          Each bit has a {(errorProbability * 100).toFixed(1)}% chance of flipping
        </small>
      </div>

      {/* BUTTON: Simulate channel */}
      <button onClick={simulateChannel} disabled={loading}>
        {loading ? 'Simulating...' : 'Send Through Channel'}
      </button>

      {/* RESULTS: Show what happened */}
      {result && (
        <div className="results">
          <h4>Channel Results</h4>

          {/* Original codeword */}
          <div>
            <strong>Original:</strong> {result.originalCodewordBinary}
          </div>

          {/* Corrupted codeword with highlighted errors */}
          <div>
            <strong>Received:</strong>{' '}
            {result.corruptedCodewordBinary.split('').map((bit, i) => (
              <span
                key={i}
                className={result.errorPositions.includes(i) ? 'error-bit' : ''}
              >
                {bit}
              </span>
            ))}
          </div>

          {/* Error information */}
          <div>
            <strong>Errors:</strong> {result.errorCount} at positions{' '}
            {result.errorPositions.join(', ')}
          </div>

          {/* Can we correct this? */}
          <div className={result.canCorrect ? 'success' : 'error'}>
            {result.status}
          </div>
        </div>
      )}
    </div>
  );
}

// TypeScript interface for API response
interface ChannelResult {
  originalCodeword: number;
  originalCodewordBinary: string;
  errorProbability: number;
  corruptedCodeword: number;
  corruptedCodewordBinary: string;
  errorCount: number;
  errorPositions: number[];
  canCorrect: boolean;
  status: string;
}
```

---

### Option 2: Advanced UI with Preset Error Rates

```typescript
export default function ChannelSimulator({ codeword }: ChannelSimulatorProps) {
  const [errorProbability, setErrorProbability] = useState(0.1);
  const [result, setResult] = useState<ChannelResult | null>(null);
  const [loading, setLoading] = useState(false);

  // PRESET ERROR RATES: Common channel conditions
  const presets = [
    { label: 'Perfect Channel', value: 0.0, description: 'No errors' },
    { label: 'Good (1%)', value: 0.01, description: 'Clean transmission' },
    { label: 'Fair (5%)', value: 0.05, description: 'Some interference' },
    { label: 'Poor (10%)', value: 0.1, description: 'Noisy channel' },
    { label: 'Bad (20%)', value: 0.2, description: 'Very noisy' },
    { label: 'Terrible (30%)', value: 0.3, description: 'Extreme noise' },
  ];

  async function simulateChannel() {
    setLoading(true);
    try {
      const response = await fetch('http://localhost:5081/golay/channel', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ codeword, errorProbability })
      });
      const data = await response.json();
      setResult(data);
    } catch (error) {
      console.error('Channel simulation failed:', error);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="channel-simulator">
      <h3>Channel Simulator</h3>

      {/* PRESET BUTTONS: Quick selection */}
      <div className="preset-buttons">
        {presets.map((preset) => (
          <button
            key={preset.value}
            onClick={() => setErrorProbability(preset.value)}
            className={errorProbability === preset.value ? 'active' : ''}
          >
            {preset.label}
          </button>
        ))}
      </div>

      {/* CUSTOM SLIDER: Fine-tune probability */}
      <div>
        <label>Custom: {(errorProbability * 100).toFixed(1)}%</label>
        <input
          type="range"
          min="0"
          max="0.5"
          step="0.01"
          value={errorProbability}
          onChange={(e) => setErrorProbability(Number(e.target.value))}
        />
      </div>

      {/* SIMULATE BUTTON */}
      <button onClick={simulateChannel} disabled={loading}>
        {loading ? 'Simulating...' : 'Send Through Channel'}
      </button>

      {/* RESULTS */}
      {result && <ChannelResults result={result} />}
    </div>
  );
}

// SEPARATE COMPONENT: Display results
function ChannelResults({ result }: { result: ChannelResult }) {
  return (
    <div className="channel-results">
      {/* Visual bit comparison */}
      <div className="bit-comparison">
        <div>
          <strong>Transmitted:</strong>
          <div className="bit-string">
            {result.originalCodewordBinary.split('').map((bit, i) => (
              <span key={i} className="bit">{bit}</span>
            ))}
          </div>
        </div>

        <div>
          <strong>Received:</strong>
          <div className="bit-string">
            {result.corruptedCodewordBinary.split('').map((bit, i) => {
              const hasError = result.errorPositions.includes(i);
              return (
                <span
                  key={i}
                  className={`bit ${hasError ? 'error' : ''}`}
                  title={hasError ? `Error at position ${i}` : ''}
                >
                  {bit}
                </span>
              );
            })}
          </div>
        </div>
      </div>

      {/* Error statistics */}
      <div className="stats">
        <div className="stat-item">
          <span className="stat-label">Errors Introduced:</span>
          <span className="stat-value">{result.errorCount}</span>
        </div>
        <div className="stat-item">
          <span className="stat-label">Error Positions:</span>
          <span className="stat-value">
            {result.errorPositions.length > 0
              ? result.errorPositions.join(', ')
              : 'None'}
          </span>
        </div>
        <div className="stat-item">
          <span className="stat-label">Correction Status:</span>
          <span className={result.canCorrect ? 'success' : 'error'}>
            {result.canCorrect ? '✓ Can Correct' : '✗ Cannot Correct'}
          </span>
        </div>
      </div>

      {/* Explanation */}
      <div className="explanation">
        <p>{result.status}</p>
        {result.canCorrect ? (
          <p>
            Golay (23,12,7) code can correct up to 3 errors.
            This codeword has {result.errorCount} error(s), which is within
            the correction capability.
          </p>
        ) : (
          <p>
            This codeword has {result.errorCount} errors, which exceeds the
            maximum of 3 that Golay code can correct. Some errors will remain
            after decoding.
          </p>
        )}
      </div>
    </div>
  );
}
```

---

### Option 3: Complete Demo Flow

```typescript
export default function CompleteChannelDemo() {
  const [message, setMessage] = useState(42);
  const [codeword, setCodeword] = useState<number | null>(null);
  const [errorProbability, setErrorProbability] = useState(0.1);
  const [channelResult, setChannelResult] = useState<ChannelResult | null>(null);
  const [decodedMessage, setDecodedMessage] = useState<number | null>(null);

  // STEP 1: Encode message
  async function encode() {
    const response = await fetch('http://localhost:5081/golay/encode', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ message })
    });
    const data = await response.json();
    setCodeword(data.codeword);
  }

  // STEP 2: Send through channel
  async function sendThroughChannel() {
    if (!codeword) return;

    const response = await fetch('http://localhost:5081/golay/channel', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ codeword, errorProbability })
    });
    const data = await response.json();
    setChannelResult(data);
  }

  // STEP 3: Decode received codeword
  async function decode() {
    if (!channelResult) return;

    const response = await fetch('http://localhost:5081/golay/decode', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ codeword: channelResult.corruptedCodeword })
    });
    const data = await response.json();
    setDecodedMessage(data.message);
  }

  return (
    <div className="complete-demo">
      <h2>Complete Error Correction Demo</h2>

      {/* STEP 1: Encode */}
      <section>
        <h3>Step 1: Encode Message</h3>
        <input
          type="number"
          value={message}
          onChange={(e) => setMessage(Number(e.target.value))}
          min="0"
          max="4095"
        />
        <button onClick={encode}>Encode</button>
        {codeword && <p>Codeword: {codeword}</p>}
      </section>

      {/* STEP 2: Channel */}
      {codeword && (
        <section>
          <h3>Step 2: Send Through Noisy Channel</h3>
          <label>Error Rate: {(errorProbability * 100).toFixed(0)}%</label>
          <input
            type="range"
            min="0"
            max="0.5"
            step="0.01"
            value={errorProbability}
            onChange={(e) => setErrorProbability(Number(e.target.value))}
          />
          <button onClick={sendThroughChannel}>Transmit</button>
          {channelResult && (
            <div>
              <p>Errors: {channelResult.errorCount}</p>
              <p>{channelResult.status}</p>
            </div>
          )}
        </section>
      )}

      {/* STEP 3: Decode */}
      {channelResult && (
        <section>
          <h3>Step 3: Decode (with Error Correction)</h3>
          <button onClick={decode}>Decode</button>
          {decodedMessage !== null && (
            <div>
              <p>Original Message: {message}</p>
              <p>Decoded Message: {decodedMessage}</p>
              <p className={message === decodedMessage ? 'success' : 'error'}>
                {message === decodedMessage
                  ? '✓ Successfully recovered original message!'
                  : '✗ Message corrupted (too many errors)'}
              </p>
            </div>
          )}
        </section>
      )}
    </div>
  );
}
```

---

## CSS Styling

```css
/* Channel Simulator Styles */
.channel-simulator {
  padding: 20px;
  border: 1px solid #ccc;
  margin: 20px 0;
}

.control-group {
  margin: 15px 0;
}

input[type="range"] {
  width: 100%;
  margin: 10px 0;
}

.bit-string {
  font-family: monospace;
  font-size: 18px;
  letter-spacing: 2px;
  margin: 10px 0;
}

.bit {
  display: inline-block;
  padding: 2px 4px;
  margin: 0 1px;
}

.bit.error {
  background-color: #ff6b6b;
  color: white;
  font-weight: bold;
}

.stats {
  margin: 20px 0;
  padding: 15px;
  border: 1px solid #ccc;
}

.stat-item {
  margin: 10px 0;
  display: flex;
  justify-content: space-between;
}

.success {
  color: green;
  font-weight: bold;
}

.error {
  color: red;
  font-weight: bold;
}

.preset-buttons button {
  margin: 5px;
  padding: 8px 12px;
}

.preset-buttons button.active {
  background-color: #4CAF50;
  color: white;
}
```

---

## Key React Concepts Used

This implementation teaches:

✅ **useState** - Managing error probability, results
✅ **async/await** - API calls for channel simulation
✅ **Conditional rendering** - Show results only when available
✅ **Event handlers** - Slider onChange, button onClick
✅ **Array.map()** - Rendering bits with error highlighting
✅ **CSS classes** - Dynamic styling for error bits
✅ **Props** - Passing codeword to simulator component

---

## Testing the Implementation

1. **Start your server:**
   ```bash
   cd server
   dotnet run
   ```

2. **Test the endpoint:**
   ```bash
   curl -X POST http://localhost:5081/golay/channel \
     -H "Content-Type: application/json" \
     -d '{"codeword": 123456, "errorProbability": 0.1}'
   ```

3. **Expected behavior:**
   - Low probability (0.01): Usually 0-1 errors
   - Medium probability (0.1): Usually 1-3 errors
   - High probability (0.3): Often >3 errors (uncorrectable)

---

## Educational Value

This demonstrates:

1. **Binary Symmetric Channel** - Standard noise model
2. **Error correction limits** - Golay can fix ≤3 errors
3. **Probability in action** - Random errors each time
4. **Real-world simulation** - How error correction works in practice

Students can:
- See error correction succeed (≤3 errors)
- See it fail (>3 errors)
- Understand probability visually
- Learn about channel capacity
