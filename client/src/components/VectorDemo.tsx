import { useState } from 'react';

const API_BASE = 'http://localhost:5081/golay';

interface EncodeResult {
  message: number;
  messageBinary: string;
  codeword: number;
  codewordBinary: string;
}

interface ChannelResult {
  originalCodeword: number;
  corruptedCodeword: number;
  corruptedCodewordBinary: string;
  errorCount: number;
  errorPositions: number[];
  canCorrect: boolean;
  status: string;
}

interface DecodeDetailedResult {
  originalCodeword: number;
  syndromeS1: string;
  syndromeS1Weight: number;
  syndromeS2: string;
  syndromeS2Weight: number;
  errorPattern: string;
  errorCount: number;
  errorPositions: number[];
  decodedMessage: number;
  decodedMessageBinary: string;
  success: boolean;
  status: string;
}

export default function VectorDemo() {
  // Encode state
  const [message, setMessage] = useState<number>(42);
  const [encodeResult, setEncodeResult] = useState<EncodeResult | null>(null);

  // Channel state
  const [codewordInput, setCodewordInput] = useState<number>(0);
  const [errorProb, setErrorProb] = useState<number>(0.1);
  const [channelResult, setChannelResult] = useState<ChannelResult | null>(null);

  // Decode state
  const [decodeInput, setDecodeInput] = useState<number>(0);
  const [decodeResult, setDecodeResult] = useState<DecodeDetailedResult | null>(null);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Encode message
  async function handleEncode() {
    setLoading(true);
    setError(null);
    try {
      const res = await fetch(`${API_BASE}/encode`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ message })
      });
      const data = await res.json();
      if (!res.ok) throw new Error(data.error);
      setEncodeResult(data);
      setCodewordInput(data.codeword);
    } catch (e: any) {
      setError(e.message);
    } finally {
      setLoading(false);
    }
  }

  // Send through channel
  async function handleChannel() {
    setLoading(true);
    setError(null);
    try {
      const res = await fetch(`${API_BASE}/channel`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ codeword: codewordInput, errorProbability: errorProb })
      });
      const data = await res.json();
      if (!res.ok) throw new Error(data.error);
      setChannelResult(data);
      setDecodeInput(data.corruptedCodeword);
    } catch (e: any) {
      setError(e.message);
    } finally {
      setLoading(false);
    }
  }

  // Decode with details
  async function handleDecode() {
    setLoading(true);
    setError(null);
    try {
      const res = await fetch(`${API_BASE}/decode-detailed`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ codeword: decodeInput })
      });
      const data = await res.json();
      if (!res.ok) throw new Error(data.error);
      setDecodeResult(data);
    } catch (e: any) {
      setError(e.message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div>
      <h2>Single Vector Demo</h2>
      <p>Demonstrates encoding a 12-bit message, sending through channel, and decoding with error correction.</p>

      {error && <div style={{ color: 'red', padding: '10px', border: '1px solid red' }}>{error}</div>}

      {/* ENCODE SECTION */}
      <section style={{ marginBottom: '20px', padding: '10px', border: '1px solid #ccc' }}>
        <h3>Step 1: Encode</h3>
        <div>
          <label>
            Message (0-4095):
            <input
              type="number"
              min={0}
              max={4095}
              value={message}
              onChange={(e) => setMessage(Number(e.target.value))}
              style={{ marginLeft: '10px', width: '100px' }}
            />
          </label>
          <button onClick={handleEncode} disabled={loading} style={{ marginLeft: '10px' }}>
            Encode
          </button>
        </div>

        {encodeResult && (
          <div style={{ marginTop: '10px', fontFamily: 'monospace' }}>
            <div>Message: {encodeResult.message} = {encodeResult.messageBinary}</div>
            <div>Codeword: {encodeResult.codeword} = {encodeResult.codewordBinary}</div>
          </div>
        )}
      </section>

      {/* CHANNEL SECTION */}
      <section style={{ marginBottom: '20px', padding: '10px', border: '1px solid #ccc' }}>
        <h3>Step 2: Send Through Channel</h3>
        <div>
          <label>
            Codeword:
            <input
              type="number"
              value={codewordInput}
              onChange={(e) => setCodewordInput(Number(e.target.value))}
              style={{ marginLeft: '10px', width: '150px' }}
            />
          </label>
          <label style={{ marginLeft: '20px' }}>
            Error Probability:
            <input
              type="number"
              min={0}
              max={0.5}
              step={0.01}
              value={errorProb}
              onChange={(e) => setErrorProb(Number(e.target.value))}
              style={{ marginLeft: '10px', width: '80px' }}
            />
          </label>
          <button onClick={handleChannel} disabled={loading} style={{ marginLeft: '10px' }}>
            Send
          </button>
        </div>

        {channelResult && (
          <div style={{ marginTop: '10px', fontFamily: 'monospace' }}>
            <div>Original: {channelResult.originalCodeword}</div>
            <div>Corrupted: {channelResult.corruptedCodeword} = {channelResult.corruptedCodewordBinary}</div>
            <div>Errors: {channelResult.errorCount} at positions [{channelResult.errorPositions.join(', ')}]</div>
            <div style={{ color: channelResult.canCorrect ? 'green' : 'red' }}>{channelResult.status}</div>
          </div>
        )}
      </section>

      {/* DECODE SECTION */}
      <section style={{ marginBottom: '20px', padding: '10px', border: '1px solid #ccc' }}>
        <h3>Step 3: Decode (with Error Correction)</h3>
        <div>
          <label>
            Received Codeword:
            <input
              type="number"
              value={decodeInput}
              onChange={(e) => setDecodeInput(Number(e.target.value))}
              style={{ marginLeft: '10px', width: '150px' }}
            />
          </label>
          <button onClick={handleDecode} disabled={loading} style={{ marginLeft: '10px' }}>
            Decode
          </button>
        </div>

        {decodeResult && (
          <div style={{ marginTop: '10px', fontFamily: 'monospace' }}>
            <div>Syndrome S1: {decodeResult.syndromeS1} (weight: {decodeResult.syndromeS1Weight})</div>
            <div>Syndrome S2: {decodeResult.syndromeS2} (weight: {decodeResult.syndromeS2Weight})</div>
            <div>Error Pattern: {decodeResult.errorPattern}</div>
            <div>Errors Found: {decodeResult.errorCount} at [{decodeResult.errorPositions.join(', ')}]</div>
            <div style={{ fontWeight: 'bold' }}>
              Decoded Message: {decodeResult.decodedMessage} = {decodeResult.decodedMessageBinary}
            </div>
            <div style={{ color: decodeResult.success ? 'green' : 'red' }}>{decodeResult.status}</div>
          </div>
        )}
      </section>
    </div>
  );
}
