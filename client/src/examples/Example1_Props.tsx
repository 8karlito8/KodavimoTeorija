/* eslint-disable react-refresh/only-export-components */
/**
 * EXAMPLE 1: PROPS (Properties)
 *
 * What are Props?
 * - Props are like function arguments - they let you pass data FROM a parent component TO a child component
 * - Props are READ-ONLY - the child component cannot change them
 * - Think of props as a one-way street: Parent -> Child
 *
 * Why use Props?
 * - Reuse the same component with different data
 * - Keep components flexible and customizable
 */

// ============================================
// CHILD COMPONENT - Receives props
// ============================================

// Define what props this component expects
interface GreetingProps {
  name: string;        // Required: person's name
  age: number;         // Required: person's age
  city?: string;       // Optional: person's city (? means optional)
}

// This component receives props and displays them
function Greeting(props: GreetingProps) {
  return (
    <div className="example-box">
      <h3>Hello, {props.name}!</h3>
      <p>You are {props.age} years old.</p>
      {/* Conditional rendering: Only show city if it exists */}
      {props.city && <p>You live in {props.city}.</p>}
    </div>
  );
}

// Alternative syntax: Destructuring props (more common and cleaner)
// Instead of using props.name, props.age, we extract them directly
function GreetingDestruct({ name, age, city }: GreetingProps) {
  return (
    <div className="example-box">
      <h3>Hello, {name}!</h3>
      <p>You are {age} years old.</p>
      {/* Conditional rendering: Only show city if it exists */}
      {city && <p>You live in {city}.</p>}
    </div>
  );
}

// ============================================
// PARENT COMPONENT - Passes props to children
// ============================================

export default function Example1_Props() {
  return (
    <div className="example-container">
      <h2>Example 1: Understanding Props</h2>

      {/* Info box: Explains what's happening in this example */}
      <div className="info-box">
        <h4>What's happening here:</h4>
        <ol>
          <li>The parent component (this one) sends data to child components</li>
          <li>Each child receives different data via props</li>
          <li>The child components display the data they received</li>
        </ol>
      </div>

      {/* Section 1: Using props.name notation */}
      <section>
        <h3>Using props with dot notation:</h3>
        {/* Passing props: name, age, and city are the props being passed */}
        <Greeting name="Karolis" age={25} city="Vilnius" />
        {/* This greeting has no city prop - it's optional so it works fine */}
        <Greeting name="Jonas" age={30} />
      </section>

      {/* Section 2: Using destructured props */}
      <section>
        <h3>Using props with destructuring:</h3>
        <GreetingDestruct name="Petras" age={22} city="Kaunas" />
        <GreetingDestruct name="Ona" age={28} />
      </section>
    </div>
  );
}

/**
 * KEY TAKEAWAYS:
 *
 * 1. Props = Data flowing from parent to child
 * 2. Defined using interfaces in TypeScript
 * 3. Used like: <Component name="value" age={123} />
 * 4. Cannot be changed by the child component
 * 5. Optional props use "?" in the interface
 */
