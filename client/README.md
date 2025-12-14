# React Learning Examples

A collection of simple, interactive examples to understand core React concepts. Perfect for beginners!

## 📚 What You'll Learn

This project has 5 interactive examples that teach:

1. **Props** - Passing data from parent to child components
2. **State (useState)** - Managing component data that changes
3. **useEffect Hook** - Running code after rendering (timers, API calls)
4. **Custom Hooks** - Creating reusable logic
5. **API Calls** - Fetching data from servers

## 🚀 Quick Start

```bash
npm install
npm run dev
```

Then open `http://localhost:5173` in your browser.

## 📂 Project Structure

```
client/src/
├── examples/
│   ├── Example1_Props.tsx        # Learn Props
│   ├── Example2_State.tsx        # Learn State (useState)
│   ├── Example3_UseEffect.tsx    # Learn useEffect
│   ├── Example4_CustomHooks.tsx  # Learn Custom Hooks
│   └── Example5_API.tsx          # Learn API Calls
├── App.tsx                       # Main app with navigation
└── main.tsx
```

## 🧠 Core Concepts Explained

### 1. Props (Properties)
**What:** Data passed FROM parent TO child component
**Why:** Reuse components with different data
**Analogy:** Like function arguments

```tsx
// Parent sends data
<Greeting name="Karolis" age={25} />

// Child receives data
function Greeting({ name, age }: GreetingProps) {
  return <h1>Hello {name}, you are {age}</h1>;
}
```

**Key Point:** Props are READ-ONLY (child can't change them)

---

### 2. State (useState)
**What:** Component's changeable data/memory
**Why:** Make UI interactive and respond to user actions
**Analogy:** Like a variable that causes re-rendering when changed

```tsx
const [count, setCount] = useState(0);
//     ^         ^            ^
//  current   updater     initial
//   value   function      value

<button onClick={() => setCount(count + 1)}>
  Clicks: {count}
</button>
```

**Key Point:** Changing state triggers a re-render

---

### 3. useEffect Hook
**What:** Run code AFTER component renders
**Why:** Perfect for API calls, timers, subscriptions
**Analogy:** Like an event that runs at specific times

```tsx
useEffect(() => {
  // Code to run
}, [dependencies]);
```

**Dependency behaviors:**
- `[]` - Run ONCE when component loads
- `[count]` - Run when count changes
- `[a, b]` - Run when a OR b changes
- No array - Run on EVERY render (rare)

---

### 4. Custom Hooks
**What:** Your own reusable hooks using built-in hooks
**Why:** Share logic between components
**Analogy:** Like creating your own tool from existing tools

```tsx
function useCounter(initial = 0) {
  const [count, setCount] = useState(initial);
  const increment = () => setCount(count + 1);
  return { count, increment };
}

// Use in any component
const { count, increment } = useCounter(0);
```

**Key Point:** Must start with "use" prefix

---

### 5. API Calls
**What:** Fetching data from servers
**Why:** Get dynamic data (users, posts, products)
**Combination:** useEffect + useState for fetching

```tsx
const [data, setData] = useState(null);
const [loading, setLoading] = useState(true);

useEffect(() => {
  fetch('https://api.example.com/data')
    .then(res => res.json())
    .then(data => {
      setData(data);
      setLoading(false);
    });
}, []);
```

**Pattern:** Loading → Fetch → Update State → Display

## 🎯 How to Use These Examples

1. **Run the app:** `npm install && npm run dev`
2. **Click through examples** using the navigation buttons
3. **Read the code** in `src/examples/` - each file is heavily commented
4. **Interact with examples** - click buttons, type in inputs, see changes
5. **Check the console** (F12) - some examples log messages
6. **Experiment** - modify the code and see what happens!

## 🤔 Common Questions

**Q: What's the difference between props and state?**
A: Props come FROM parent (read-only). State is INSIDE component (changeable).

**Q: When should I use useEffect?**
A: When you need to do something AFTER rendering (fetch data, timers, subscriptions).

**Q: Why create custom hooks?**
A: To reuse logic across multiple components without copy-pasting.

**Q: How do I know when to use state?**
A: If data changes and affects what the user sees, it should be state.

## 📖 Learning Path

**Beginner:**
1. Start with Example 1 (Props) - understand data flow
2. Move to Example 2 (State) - learn interactivity
3. Try Example 3 (useEffect) - understand lifecycle

**Intermediate:**
4. Study Example 4 (Custom Hooks) - reusable logic
5. Explore Example 5 (API) - real-world data fetching

## 🛠 Technology

- React 19 + TypeScript
- Vite (fast dev server)
- JSONPlaceholder (free API for examples)

## 💡 Next Steps

After understanding these examples, you can:
- Build your own TODO app
- Create a weather app
- Make a blog with API
- Build a shopping cart

## 📝 Tips for Learning

1. **Read the code** - Every example has detailed comments
2. **Break things** - Try changing values, remove code, see what happens
3. **Console.log everything** - Print values to understand flow
4. **Start small** - Master one concept before moving to next
5. **Build something** - Apply what you learned in a small project
