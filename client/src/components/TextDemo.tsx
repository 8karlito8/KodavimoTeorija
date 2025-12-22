import { useState } from 'react';

const API_BASE = 'http://localhost:5081/golay';

interface FullDemoResult {
  original: {
    text: string;
    byteCount: number;
    bitCount: number;
  };
  channel: {
    errorProbability: number;
    expectedErrorsPerBit: string;
  };
  withoutCode: {
    description: string;
    receivedText: string;
    bitErrors: number;
    status: string;
  };
  withCode: {
    description: string;
    codewordCount: number;
    totalBitsSent: number;
    totalBitErrors: number;
    errorsCorrected: number;
    uncorrectableBlocks: number;
    decodedText: string;
    status: string;
  };
  comparison: {
    withoutCodeMatch: boolean;
    withCodeMatch: boolean;
    conclusion: string;
  };
}

export default function TextDemo() {
  const [text, setText] = useState('Hello, World! This is a test of Golay error correction.');
  const [errorProb, setErrorProb] = useState(0.05);
  const [result, setResult] = useState<FullDemoResult | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleFullDemo() {
    setLoading(true);
    setError(null);
    try {
      const res = await fetch(`${API_BASE}/text/full-demo`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ text, errorProbability: errorProb })
      });
      const data = await res.json();
      if (!res.ok) throw new Error(data.error);
      setResult(data);
    } catch (e: any) {
      setError(e.message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div>
      <h2>Text Demo</h2>
      <p>Compare text transmission with and without Golay error correction.</p>

      {error && <div style={{ color: 'red', padding: '10px', border: '1px solid red' }}>{error}</div>}

      <section style={{ marginBottom: '20px', padding: '10px', border: '1px solid #ccc' }}>
        <h3>Input</h3>
        <div>
          <label>
            Text to send:
            <br />
            <textarea
              value={text}
              onChange={(e) => setText(e.target.value)}
              rows={3}
              cols={60}
              style={{ fontFamily: 'monospace' }}
            />
          </label>
        </div>
        <div style={{ marginTop: '10px' }}>
          <label>
            Error Probability:
            <input
              type="number"
              min={0}
              max={0.3}
              step={0.01}
              value={errorProb}
              onChange={(e) => setErrorProb(Number(e.target.value))}
              style={{ marginLeft: '10px' }}
            />
            <span style={{ marginLeft: '10px' }}>{(errorProb * 100).toFixed(0)}%</span>
          </label>
        </div>
        <button onClick={handleFullDemo} disabled={loading} style={{ marginTop: '10px' }}>
          {loading ? 'Processing...' : 'Send Through Channel'}
        </button>
      </section>

      {result && (
        <>
          <section style={{ marginBottom: '20px', padding: '10px', border: '1px solid #ccc' }}>
            <h3>Original Text</h3>
            <div style={{ fontFamily: 'monospace', background: '#f5f5f5', padding: '10px' }}>
              {result.original.text}
            </div>
            <div style={{ fontSize: '0.9em', color: '#666' }}>
              {result.original.byteCount} bytes, {result.original.bitCount} bits
            </div>
          </section>

          <section style={{ marginBottom: '20px', padding: '10px', border: '1px solid #f88' }}>
            <h3>WITHOUT Error Correction</h3>
            <div style={{ fontSize: '0.9em', marginBottom: '5px' }}>{result.withoutCode.description}</div>
            <div style={{ fontFamily: 'monospace', background: '#fff5f5', padding: '10px' }}>
              {result.withoutCode.receivedText}
            </div>
            <div>
              Bit Errors: {result.withoutCode.bitErrors}
              <span style={{ marginLeft: '20px', color: result.withoutCode.status.includes('✓') ? 'green' : 'red' }}>
                {result.withoutCode.status}
              </span>
            </div>
          </section>

          <section style={{ marginBottom: '20px', padding: '10px', border: '1px solid #8f8' }}>
            <h3>WITH Golay Error Correction</h3>
            <div style={{ fontSize: '0.9em', marginBottom: '5px' }}>{result.withCode.description}</div>
            <div style={{ fontFamily: 'monospace', background: '#f5fff5', padding: '10px' }}>
              {result.withCode.decodedText}
            </div>
            <div>
              <div>Codewords sent: {result.withCode.codewordCount}</div>
              <div>Total bits sent: {result.withCode.totalBitsSent}</div>
              <div>Bit errors in channel: {result.withCode.totalBitErrors}</div>
              <div>Errors corrected: {result.withCode.errorsCorrected}</div>
              <div>Uncorrectable blocks: {result.withCode.uncorrectableBlocks}</div>
              <span style={{ color: result.withCode.status.includes('✓') ? 'green' : 'red', fontWeight: 'bold' }}>
                {result.withCode.status}
              </span>
            </div>
          </section>

          <section style={{ marginBottom: '20px', padding: '10px', border: '1px solid #88f', background: '#f5f5ff' }}>
            <h3>Comparison</h3>
            <div>Without code matches original: {result.comparison.withoutCodeMatch ? '✓ Yes' : '✗ No'}</div>
            <div>With code matches original: {result.comparison.withCodeMatch ? '✓ Yes' : '✗ No'}</div>
            <div style={{ fontWeight: 'bold', marginTop: '10px' }}>{result.comparison.conclusion}</div>
          </section>
        </>
      )}
    </div>
  );
}
