/* eslint-disable react-refresh/only-export-components */
/**
 * EXAMPLE 2: STATE (with useState hook)
 *
 * What is State?
 * - State is data that belongs to a component and can CHANGE over time
 * - When state changes, React automatically re-renders the component
 * - Think of state as a component's memory
 *
 * Why use State?
 * - Track data that changes (clicks, inputs, toggles, etc.)
 * - Make your UI interactive and dynamic
 * - React to user actions
 */

import { useState } from 'react';

// ============================================
// EXAMPLE 1: Simple Counter
// ============================================

function Counter() {
  // useState creates a state variable
  // Syntax: const [value, setValue] = useState(initialValue);
  const [count, setCount] = useState(0);
  //       ^         ^           ^
  //       |         |           |
  //    current   function    starting
  //     value   to update     value

  return (
    <div className="example-box">
      <h3>Counter Example</h3>
      <p>Current count: {count}</p>
      {/* onClick handler: When button is clicked, update the state */}
      {/* Arrow function: () => setCount(...) is called when clicked */}
      <button onClick={() => setCount(count + 1)}>Increase (+1)</button>
      <button onClick={() => setCount(count - 1)}>Decrease (-1)</button>
      <button onClick={() => setCount(0)}>Reset</button>
    </div>
  );
}

// ============================================
// EXAMPLE 2: Text Input
// ============================================

function TextInput() {
  // State to track what user types in the input field
  const [text, setText] = useState('');

  return (
    <div className="example-box">
      <h3>Text Input Example</h3>
      {/* Controlled input: value comes from state, onChange updates state */}
      <input
        type="text"
        value={text}
        onChange={(e) => setText(e.target.value)}
        placeholder="Type something..."
      />
      {/* Display the current state value */}
      <p>You typed: {text}</p>
      <p>Length: {text.length} characters</p>
    </div>
  );
}

// ============================================
// EXAMPLE 3: Toggle Switch
// ============================================

function Toggle() {
  // Boolean state: can only be true or false
  const [isOn, setIsOn] = useState(false);

  return (
    <div className="example-box">
      <h3>Toggle Example</h3>
      {/* Ternary operator: condition ? valueIfTrue : valueIfFalse */}
      <p>The light is: <strong>{isOn ? 'ON 💡' : 'OFF 🌑'}</strong></p>
      {/* !isOn means "not isOn" - it flips true to false and vice versa */}
      <button onClick={() => setIsOn(!isOn)}>
        Toggle Switch
      </button>
    </div>
  );
}

// ============================================
// EXAMPLE 4: Multiple State Values
// ============================================

function Form() {
  // You can have multiple state variables in one component
  // Each state is independent and can be updated separately
  const [name, setName] = useState('');
  const [age, setAge] = useState(0);
  const [city, setCity] = useState('');

  return (
    <div className="example-box">
      <h3>Multiple States Example</h3>

      {/* Input group for name */}
      <div>
        <label>Name: </label>
        <input
          value={name}
          onChange={(e) => setName(e.target.value)}
        />
      </div>

      {/* Input group for age */}
      <div>
        <label>Age: </label>
        {/* type="number" makes it a number input */}
        {/* Number() converts the string value to a number */}
        <input
          type="number"
          value={age}
          onChange={(e) => setAge(Number(e.target.value))}
        />
      </div>

      {/* Input group for city */}
      <div>
        <label>City: </label>
        <input
          value={city}
          onChange={(e) => setCity(e.target.value)}
        />
      </div>

      {/* Summary box showing all current state values */}
      <div className="summary-box">
        <h4>Summary:</h4>
        {/* || operator: show '(empty)' if name is empty string */}
        <p>Name: {name || '(empty)'}</p>
        <p>Age: {age}</p>
        <p>City: {city || '(empty)'}</p>
      </div>
    </div>
  );
}

// ============================================
// PARENT COMPONENT
// ============================================

export default function Example2_State() {
  return (
    <div className="example-container">
      <h2>Example 2: Understanding State</h2>

      {/* Info box explaining the concept */}
      <div className="info-box">
        <h4>What's happening here:</h4>
        <ol>
          <li>Each component has its own state (memory)</li>
          <li>When you interact (click, type), state changes</li>
          <li>React automatically updates the display</li>
          <li>State persists until the component is removed</li>
        </ol>
      </div>

      {/* Render all the example components */}
      <Counter />
      <TextInput />
      <Toggle />
      <Form />
    </div>
  );
}

/**
 * KEY TAKEAWAYS:
 *
 * 1. State = Component's changeable data/memory
 * 2. Create with: const [value, setValue] = useState(initial)
 * 3. Read state: just use the variable (count)
 * 4. Update state: call the setter function (setCount(newValue))
 * 5. When state changes, component re-renders automatically
 * 6. Each component instance has its own separate state
 */
