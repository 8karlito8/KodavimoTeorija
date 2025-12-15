/**
 * REACT LEARNING EXAMPLES
 *
 * This app demonstrates core React concepts with simple examples.
 * Navigate through different examples using the buttons below.
 */

import { useState } from 'react';
// import Example1_Props from './examples/Example1_Props';
// import Example2_State from './examples/Example2_State';
// import Example3_UseEffect from './examples/Example3_UseEffect';
// import Example4_CustomHooks from './examples/Example4_CustomHooks';
// import Example5_API from './examples/Example5_API';
import './App.css';

// Type definition: This defines what values currentExample can have
// Example type is either 'props', 'state', 'useEffect', 'customHooks', 'api'
// type ExampleType = 'props' | 'state' | 'useEffect' | 'customHooks' | 'api';

function App() {
  // State hook: Keeps track of which example is currently being displayed
  // const [currentExample, setCurrentExample] = useState<ExampleType>('props');

  // Object mapping: Maps each example type to its component and title
  // const examples = {
  //   props: { component: Example1_Props, title: '1. Props' },
  //   state: { component: Example2_State, title: '2. State (useState)' },
  //   useEffect: { component: Example3_UseEffect, title: '3. useEffect Hook' },
  //   customHooks: { component: Example4_CustomHooks, title: '4. Custom Hooks' },
  //   api: { component: Example5_API, title: '5. API Calls' },
  // };

  // Dynamic component: Get the component to render based on currentExample
  // const CurrentComponent = examples[currentExample].component;

  return (
    <div>

    </div>
  );
}

export default App;
