/* eslint-disable react-hooks/set-state-in-effect */
/* eslint-disable react-refresh/only-export-components */
/**
 * EXAMPLE 3: useEffect Hook
 *
 * What is useEffect?
 * - useEffect lets you run code AFTER the component renders
 * - It's for "side effects" - things that happen outside of just displaying UI
 * - Examples: API calls, timers, subscriptions, DOM manipulation
 *
 * Why use useEffect?
 * - Fetch data from an API
 * - Set up timers or intervals
 * - Subscribe to events
 * - Update the document title
 * - Run code when specific values change
 */

import { useState, useEffect } from 'react';

// ============================================
// EXAMPLE 1: Run once when component loads
// ============================================

function RunOnce() {
  const [message, setMessage] = useState('Loading...');

  // This runs ONCE when the component first appears
  // Empty dependency array [] means "run only once on mount"
  useEffect(() => {
    console.log('Component loaded!');
    setMessage('Component loaded successfully!');
  }, []); // Empty array [] means "run only once"

  return (
    <div className="example-box">
      <h3>Run Once Example</h3>
      <p>{message}</p>
      <p className="hint">
        Check the browser console - you'll see "Component loaded!" printed once
      </p>
    </div>
  );
}

// ============================================
// EXAMPLE 2: Timer/Interval
// ============================================

function Timer() {
  const [seconds, setSeconds] = useState(0);

  useEffect(() => {
    // Start a timer that runs every 1 second (1000 milliseconds)
    const interval = setInterval(() => {
      // Use functional update to ensure we get the latest value
      setSeconds((prev) => prev + 1);
    }, 1000);

    // CLEANUP: This function runs when component is removed
    // It's important to stop timers to prevent memory leaks
    return () => {
      clearInterval(interval);
      console.log('Timer stopped');
    };
  }, []); // Run once on mount

  return (
    <div className="example-box">
      <h3>Timer Example</h3>
      <p>Time elapsed: {seconds} seconds</p>
    </div>
  );
}

// ============================================
// EXAMPLE 3: Run when state changes
// ============================================

function WatchState() {
  const [count, setCount] = useState(0);
  const [lastUpdate, setLastUpdate] = useState('Never');

  // This runs EVERY TIME count changes
  // The dependency array [count] tells React to watch count
  useEffect(() => {
    console.log(`Count changed to: ${count}`);
    const now = new Date().toLocaleTimeString();
    setLastUpdate(now);
  }, [count]); // [count] means "run when count changes"

  return (
    <div className="example-box">
      <h3>Watch State Example</h3>
      <p>Count: {count}</p>
      <p>Last updated: {lastUpdate}</p>
      <button onClick={() => setCount(count + 1)}>Increment</button>
      <p className="hint">
        Check console - effect runs every time you click
      </p>
    </div>
  );
}

// ============================================
// EXAMPLE 4: Multiple dependencies
// ============================================

function MultipleWatchers() {
  const [name, setName] = useState('');
  const [age, setAge] = useState(0);
  const [logs, setLogs] = useState<string[]>([]);

  // This runs when EITHER name OR age changes
  // Multiple dependencies are watched: [name, age]
  useEffect(() => {
    const log = `Updated: name="${name}", age=${age}`;
    // Spread operator: ...prev creates a copy of the array
    setLogs((prev) => [...prev, log]);
  }, [name, age]); // Multiple dependencies

  return (
    <div className="example-box">
      <h3>Multiple Dependencies Example</h3>
      <div>
        <input
          placeholder="Name"
          value={name}
          onChange={(e) => setName(e.target.value)}
        />
      </div>
      <div>
        <input
          type="number"
          placeholder="Age"
          value={age}
          onChange={(e) => setAge(Number(e.target.value))}
        />
      </div>
      <div className="log-box">
        <h4>Change Log:</h4>
        {/* map() creates a new element for each log entry */}
        {/* key prop helps React identify which items changed */}
        {logs.map((log, index) => (
          <div key={index}>
            {index + 1}. {log}
          </div>
        ))}
      </div>
    </div>
  );
}

// ============================================
// PARENT COMPONENT
// ============================================

export default function Example3_UseEffect() {
  return (
    <div className="example-container">
      <h2>Example 3: Understanding useEffect</h2>

      <div className="info-box">
        <h4>What's happening here:</h4>
        <ol>
          <li>useEffect runs code AFTER rendering</li>
          <li>Different behaviors based on dependencies:</li>
          <ul>
            <li><code>useEffect(() =&gt; {'{}'}, [])</code> - Run ONCE on load</li>
            <li><code>useEffect(() =&gt; {'{}'}, [count])</code> - Run when count changes</li>
            <li><code>useEffect(() =&gt; {'{}'}, [a, b])</code> - Run when a OR b changes</li>
            <li><code>useEffect(() =&gt; {'{}'})</code> - Run on EVERY render (rare)</li>
          </ul>
          <li>Return a cleanup function to undo side effects</li>
        </ol>
      </div>

      <RunOnce />
      <Timer />
      <WatchState />
      <MultipleWatchers />
    </div>
  );
}

/**
 * KEY TAKEAWAYS:
 *
 * 1. useEffect = Run code after rendering
 * 2. Syntax: useEffect(() => { code here }, [dependencies])
 * 3. Empty [] = run once when component loads
 * 4. [value] = run when value changes
 * 5. Return a cleanup function to stop timers, unsubscribe, etc.
 * 6. Great for API calls, timers, and subscriptions
 */
