/* eslint-disable react-refresh/only-export-components */
/**
 * EXAMPLE 4: Custom Hooks
 *
 * What are Custom Hooks?
 * - Custom hooks are YOUR OWN hooks that use built-in hooks (useState, useEffect, etc.)
 * - They let you reuse logic across multiple components
 * - Must start with "use" (e.g., useCounter, useForm, useFetch)
 *
 * Why use Custom Hooks?
 * - Share logic between components without copy-pasting
 * - Make code cleaner and more organized
 * - Easier to test and maintain
 */

import { useState, useEffect } from 'react';

// ============================================
// CUSTOM HOOK 1: Counter Logic
// ============================================

// This custom hook encapsulates counter logic
// It can be reused in any component that needs a counter
function useCounter(initialValue: number = 0) {
  const [count, setCount] = useState(initialValue);

  // These functions manipulate the count state
  const increment = () => setCount(count + 1);
  const decrement = () => setCount(count - 1);
  const reset = () => setCount(initialValue);

  // Return an object with the state and functions
  // Components can use these to interact with the counter
  return { count, increment, decrement, reset };
}

// Using the custom hook in component A
// Notice how we reuse the same logic without copying code
function CounterA() {
  // Destructure the values returned by useCounter
  const { count, increment, decrement, reset } = useCounter(0);

  return (
    <div className="example-box">
      <h3>Counter A (starts at 0)</h3>
      <p>Count: {count}</p>
      {/* We can pass functions directly as event handlers */}
      <button onClick={increment}>+</button>
      <button onClick={decrement}>-</button>
      <button onClick={reset}>Reset</button>
    </div>
  );
}

// Using the same hook in component B with a different initial value
// Each component has its own separate state
function CounterB() {
  const { count, increment, decrement, reset } = useCounter(100);

  return (
    <div className="example-box">
      <h3>Counter B (starts at 100)</h3>
      <p>Count: {count}</p>
      <button onClick={increment}>+</button>
      <button onClick={decrement}>-</button>
      <button onClick={reset}>Reset</button>
    </div>
  );
}

// ============================================
// CUSTOM HOOK 2: Toggle Logic
// ============================================

// This hook manages boolean state (on/off, true/false)
// Useful for modals, sidebars, checkboxes, etc.
function useToggle(initialState: boolean = false) {
  const [value, setValue] = useState(initialState);

  // toggle: flip the value
  const toggle = () => setValue(!value);
  // setTrue: force to true
  const setTrue = () => setValue(true);
  // setFalse: force to false
  const setFalse = () => setValue(false);

  return { value, toggle, setTrue, setFalse };
}

function ToggleDemo() {
  // Using the same hook twice for different purposes
  const light = useToggle(false);
  const sidebar = useToggle(true);

  return (
    <div className="example-box">
      <h3>Toggle Demo</h3>
      <div>
        {/* Ternary operator to show different states */}
        <p>Light: {light.value ? '💡 ON' : '🌑 OFF'}</p>
        <button onClick={light.toggle}>Toggle Light</button>
      </div>
      <div>
        <p>Sidebar: {sidebar.value ? '📖 Open' : '📕 Closed'}</p>
        <button onClick={sidebar.toggle}>Toggle Sidebar</button>
      </div>
    </div>
  );
}

// ============================================
// CUSTOM HOOK 3: Form Input
// ============================================

// This hook simplifies form input handling
// It manages the value and onChange handler
function useInput(initialValue: string = '') {
  const [value, setValue] = useState(initialValue);

  // Generic onChange handler for input elements
  const onChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setValue(e.target.value);
  };

  // Reset to initial value
  const reset = () => setValue(initialValue);

  // Return value and handlers that can be spread directly onto inputs
  return { value, onChange, reset };
}

function FormDemo() {
  // Create separate inputs for name and email
  const name = useInput('');
  const email = useInput('');

  return (
    <div className="example-box">
      <h3>Form Demo</h3>
      <div>
        <label>Name: </label>
        {/* Spread the value and onChange directly */}
        <input value={name.value} onChange={name.onChange} />
      </div>
      <div>
        <label>Email: </label>
        <input value={email.value} onChange={email.onChange} />
      </div>
      <div className="summary-box">
        <p>Name: {name.value || '(empty)'}</p>
        <p>Email: {email.value || '(empty)'}</p>
      </div>
      {/* Reset both inputs at once */}
      <button onClick={() => { name.reset(); email.reset(); }}>
        Clear Form
      </button>
    </div>
  );
}

// ============================================
// CUSTOM HOOK 4: Window Size
// ============================================

// This hook tracks the browser window size
// Uses useEffect to listen for resize events
function useWindowSize() {
  const [size, setSize] = useState({
    width: window.innerWidth,
    height: window.innerHeight,
  });

  useEffect(() => {
    // Event handler that updates size when window resizes
    const handleResize = () => {
      setSize({
        width: window.innerWidth,
        height: window.innerHeight,
      });
    };

    // Subscribe to resize events
    window.addEventListener('resize', handleResize);

    // Cleanup: Unsubscribe when component unmounts
    // This prevents memory leaks
    return () => window.removeEventListener('resize', handleResize);
  }, []); // Empty array = run once on mount

  return size;
}

function WindowSizeDemo() {
  const { width, height } = useWindowSize();

  return (
    <div className="example-box">
      <h3>Window Size Demo</h3>
      <p>Width: {width}px</p>
      <p>Height: {height}px</p>
      <p className="hint">
        Try resizing your browser window!
      </p>
    </div>
  );
}

// ============================================
// PARENT COMPONENT
// ============================================

export default function Example4_CustomHooks() {
  return (
    <div className="example-container">
      <h2>Example 4: Understanding Custom Hooks</h2>

      <div className="info-box">
        <h4>What's happening here:</h4>
        <ol>
          <li>Custom hooks are functions that start with "use"</li>
          <li>They can use other hooks inside (useState, useEffect, etc.)</li>
          <li>They return values and functions for components to use</li>
          <li>Same hook can be used in multiple components</li>
          <li>Each component gets its own separate state</li>
        </ol>
        <h4>Benefits:</h4>
        <ul>
          <li>Reuse logic without copy-paste</li>
          <li>Keep components clean and simple</li>
          <li>Easy to test and maintain</li>
        </ul>
      </div>

      {/* All these components use custom hooks */}
      <CounterA />
      <CounterB />
      <ToggleDemo />
      <FormDemo />
      <WindowSizeDemo />
    </div>
  );
}

/**
 * KEY TAKEAWAYS:
 *
 * 1. Custom hooks = Reusable logic functions
 * 2. Must start with "use" (useCounter, useToggle, etc.)
 * 3. Can use built-in hooks inside (useState, useEffect)
 * 4. Return values/functions for components to use
 * 5. Each component using the hook gets its own state
 * 6. Great for sharing logic between components
 */
