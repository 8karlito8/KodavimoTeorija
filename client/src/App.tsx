import { useState } from 'react';
import VectorDemo from './components/VectorDemo';
import TextDemo from './components/TextDemo';
import ImageDemo from './components/ImageDemo';
import MatrixDisplay from './components/MatrixDisplay';
import './App.css';

type Tab = 'vector' | 'text' | 'image' | 'matrices';

function App() {
  const [activeTab, setActiveTab] = useState<Tab>('vector');

  return (
    <div style={{ maxWidth: '1000px', margin: '0 auto', padding: '20px' }}>
      <h1>Golay (23,12,7) Error-Correcting Code</h1>
      <p>
        A perfect binary code that encodes 12 bits into 23 bits and can correct up to 3 bit errors.
      </p>

      {/* Tab Navigation */}
      <nav style={{ marginBottom: '20px', borderBottom: '2px solid #ccc', paddingBottom: '10px' }}>
        <button
          onClick={() => setActiveTab('vector')}
          style={{
            padding: '10px 20px',
            marginRight: '5px',
            background: activeTab === 'vector' ? '#4a4' : '#ddd',
            color: activeTab === 'vector' ? 'white' : 'black',
            border: 'none',
            cursor: 'pointer'
          }}
        >
          Vector Demo
        </button>
        <button
          onClick={() => setActiveTab('text')}
          style={{
            padding: '10px 20px',
            marginRight: '5px',
            background: activeTab === 'text' ? '#4a4' : '#ddd',
            color: activeTab === 'text' ? 'white' : 'black',
            border: 'none',
            cursor: 'pointer'
          }}
        >
          Text Demo
        </button>
        <button
          onClick={() => setActiveTab('image')}
          style={{
            padding: '10px 20px',
            marginRight: '5px',
            background: activeTab === 'image' ? '#4a4' : '#ddd',
            color: activeTab === 'image' ? 'white' : 'black',
            border: 'none',
            cursor: 'pointer'
          }}
        >
          Image Demo
        </button>
        <button
          onClick={() => setActiveTab('matrices')}
          style={{
            padding: '10px 20px',
            background: activeTab === 'matrices' ? '#4a4' : '#ddd',
            color: activeTab === 'matrices' ? 'white' : 'black',
            border: 'none',
            cursor: 'pointer'
          }}
        >
          Matrices
        </button>
      </nav>

      {/* Tab Content */}
      <main>
        {activeTab === 'vector' && <VectorDemo />}
        {activeTab === 'text' && <TextDemo />}
        {activeTab === 'image' && <ImageDemo />}
        {activeTab === 'matrices' && <MatrixDisplay />}
      </main>

      {/* Footer */}
      <footer style={{ marginTop: '40px', borderTop: '1px solid #ccc', paddingTop: '10px', fontSize: '0.9em', color: '#666' }}>
        <p>
          Backend API: <a href="http://localhost:5081" target="_blank" rel="noopener noreferrer">http://localhost:5081</a>
        </p>
        <p>
          References: literatura12.pdf (Algorithm 3.6.1 and 3.7.1)
        </p>
      </footer>
    </div>
  );
}

export default App;
