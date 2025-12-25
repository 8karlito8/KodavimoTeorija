import { useState, useRef } from 'react';
import { API_BASE } from '../config';

interface ImageDemoResult {
  stats: {
    originalSize: number;
    headerSize: number;
    pixelDataSize: number;
    codewordCount: number;
    errorProbability: number;
    channelErrors: number;
    correctedErrors: number;
    uncorrectableBlocks: number;
    rawBitErrors: number;
  };
  images: {
    original: string;
    corruptedWithoutCode: string;
    decodedWithCode: string;
  };
}

export default function ImageDemo() {
  const [errorProb, setErrorProb] = useState(0.02);
  const [result, setResult] = useState<ImageDemoResult | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  async function handleFileUpload(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;

    // Check if BMP
    if (!file.name.toLowerCase().endsWith('.bmp')) {
      setError('Please select a BMP file (24-bit BMP recommended)');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const arrayBuffer = await file.arrayBuffer();

      const res = await fetch(`${API_BASE}/image/full-demo?errorProbability=${errorProb}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/octet-stream' },
        body: arrayBuffer
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

  function base64ToBmpUrl(base64: string): string {
    return `data:image/bmp;base64,${base64}`;
  }

  return (
    <div>
      <h2>Image Demo</h2>
      <p>Compare image transmission with and without Golay error correction (BMP files only).</p>

      {error && <div style={{ color: 'red', padding: '10px', border: '1px solid red' }}>{error}</div>}

      <section style={{ marginBottom: '20px', padding: '10px', border: '1px solid #ccc' }}>
        <h3>Upload BMP Image</h3>
        <div>
          <label>
            Error Probability:
            <input
              type="number"
              min={0}
              max={0.2}
              step={0.005}
              value={errorProb}
              onChange={(e) => setErrorProb(Number(e.target.value))}
              style={{ marginLeft: '10px' }}
            />
            <span style={{ marginLeft: '10px' }}>{(errorProb * 100).toFixed(1)}%</span>
          </label>
        </div>
        <div style={{ marginTop: '10px' }}>
          <input
            ref={fileInputRef}
            type="file"
            accept=".bmp"
            onChange={handleFileUpload}
            disabled={loading}
          />
          {loading && <span style={{ marginLeft: '10px' }}>Processing...</span>}
        </div>
        <div style={{ fontSize: '0.8em', color: '#666', marginTop: '5px' }}>
          Note: Use small BMP images for faster processing. Header is preserved (not corrupted).
        </div>
      </section>

      {result && (
        <>
          <section style={{ marginBottom: '20px', padding: '10px', border: '1px solid #ccc' }}>
            <h3>Statistics</h3>
            <table style={{ borderCollapse: 'collapse' }}>
              <tbody>
                <tr><td>Original size:</td><td>{result.stats.originalSize} bytes</td></tr>
                <tr><td>Header size:</td><td>{result.stats.headerSize} bytes (preserved)</td></tr>
                <tr><td>Pixel data:</td><td>{result.stats.pixelDataSize} bytes</td></tr>
                <tr><td>Codewords:</td><td>{result.stats.codewordCount}</td></tr>
                <tr><td>Error probability:</td><td>{(result.stats.errorProbability * 100).toFixed(1)}%</td></tr>
                <tr><td style={{ color: 'red' }}>Raw bit errors (no code):</td><td>{result.stats.rawBitErrors}</td></tr>
                <tr><td style={{ color: 'orange' }}>Channel errors (with code):</td><td>{result.stats.channelErrors}</td></tr>
                <tr><td style={{ color: 'green' }}>Errors corrected:</td><td>{result.stats.correctedErrors}</td></tr>
                <tr><td>Uncorrectable blocks:</td><td>{result.stats.uncorrectableBlocks}</td></tr>
              </tbody>
            </table>
          </section>

          <section style={{ marginBottom: '20px' }}>
            <h3>Image Comparison</h3>
            <div style={{ display: 'flex', gap: '20px', flexWrap: 'wrap' }}>
              <div style={{ border: '1px solid #ccc', padding: '10px' }}>
                <h4>Original</h4>
                <img
                  src={base64ToBmpUrl(result.images.original)}
                  alt="Original"
                  style={{ maxWidth: '300px', maxHeight: '300px' }}
                />
              </div>
              <div style={{ border: '1px solid #f88', padding: '10px' }}>
                <h4>Without Error Correction</h4>
                <img
                  src={base64ToBmpUrl(result.images.corruptedWithoutCode)}
                  alt="Corrupted"
                  style={{ maxWidth: '300px', maxHeight: '300px' }}
                />
                <div style={{ color: 'red', fontSize: '0.9em' }}>
                  {result.stats.rawBitErrors} bit errors
                </div>
              </div>
              <div style={{ border: '1px solid #8f8', padding: '10px' }}>
                <h4>With Golay Error Correction</h4>
                <img
                  src={base64ToBmpUrl(result.images.decodedWithCode)}
                  alt="Decoded"
                  style={{ maxWidth: '300px', maxHeight: '300px' }}
                />
                <div style={{ color: 'green', fontSize: '0.9em' }}>
                  {result.stats.correctedErrors} errors corrected
                  {result.stats.uncorrectableBlocks > 0 &&
                    <span style={{ color: 'orange' }}>, {result.stats.uncorrectableBlocks} blocks failed</span>
                  }
                </div>
              </div>
            </div>
          </section>
        </>
      )}
    </div>
  );
}
