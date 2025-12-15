import { useState, useEffect } from 'react';

const API_BASE = 'http://localhost:5081/golay';

export default function MatrixDisplay() {
  const [matrixG, setMatrixG] = useState<number[][] | null>(null);
  const [matrixB, setMatrixB] = useState<number[][] | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showG, setShowG] = useState(true);

  useEffect(() => {
    async function fetchMatrices() {
      try {
        const [resG, resB] = await Promise.all([
          fetch(`${API_BASE}/generator-matrix`),
          fetch(`${API_BASE}/matrix-b`)
        ]);
        const dataG = await resG.json();
        const dataB = await resB.json();
        setMatrixG(dataG);
        setMatrixB(dataB);
      } catch (e: any) {
        setError(e.message);
      } finally {
        setLoading(false);
      }
    }
    fetchMatrices();
  }, []);

  if (loading) return <div>Loading matrices...</div>;
  if (error) return <div style={{ color: 'red' }}>Error: {error}</div>;

  const matrix = showG ? matrixG : matrixB;
  const title = showG ? 'Generator Matrix G = [I | P] (12×23)' : 'B Matrix (12×12)';

  return (
    <div>
      <h2>Matrices</h2>
      <div style={{ marginBottom: '10px' }}>
        <button onClick={() => setShowG(true)} disabled={showG}>Generator G</button>
        <button onClick={() => setShowG(false)} disabled={!showG} style={{ marginLeft: '10px' }}>B Matrix</button>
      </div>
      <h3>{title}</h3>
      <table style={{ borderCollapse: 'collapse', fontFamily: 'monospace', fontSize: '12px' }}>
        <tbody>
          {matrix?.map((row, i) => (
            <tr key={i}>
              <td style={{ padding: '2px 5px', background: '#eee', fontWeight: 'bold' }}>{i}</td>
              {row.map((cell, j) => (
                <td
                  key={j}
                  style={{
                    padding: '2px 5px',
                    textAlign: 'center',
                    background: cell === 1 ? '#4a4' : '#ddd',
                    color: cell === 1 ? 'white' : '#666'
                  }}
                >
                  {cell}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
      {showG && (
        <div style={{ marginTop: '10px', fontSize: '0.9em', color: '#666' }}>
          <div>Columns 0-11: Identity matrix (message bits)</div>
          <div>Columns 12-22: Parity matrix (parity bits)</div>
        </div>
      )}
    </div>
  );
}
