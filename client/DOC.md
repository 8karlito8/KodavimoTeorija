# Golay Frontend - Assignment-Based Learning Path

**Project Goal:** Build a React frontend for Golay (23,12,7) encoding/decoding with error correction demonstration.

**Learning Approach:** Complete 6 stages sequentially. Each stage teaches specific React concepts while building real features.

**Backend API:** C# .NET server at `http://localhost:5081`

---

## 📋 Table of Contents

1. [Stage 1: Project Setup & Static Layout](#stage-1-project-setup--static-layout)
2. [Stage 2: Form Inputs & State](#stage-2-form-inputs--state)
3. [Stage 3: API Integration](#stage-3-api-integration)
4. [Stage 4: Custom Hooks](#stage-4-custom-hooks)
5. [Stage 5: Error Simulation](#stage-5-error-simulation)
6. [Stage 6: Polish & Enhancement](#stage-6-polish--enhancement)

---

## STAGE 1: Project Setup & Static Layout

**Learning Goals:** JSX, Functional Components, Props, Component Composition

### Assignment 1.1: Create Component Structure

**Requirements:**
- Create a clean component-based structure for the Golay application
- Remove or move existing example components to a separate folder
- Build the foundational layout

**Components to Create:**

#### 1. `src/components/Header.tsx`
```typescript
interface HeaderProps {
  title: string;
  subtitle?: string;
}
```
**What it should do:**
- Display application title
- Display optional subtitle
- Use props (no hardcoded text)

---

#### 2. `src/components/Encoder.tsx`
```typescript
export default function Encoder() {
  // For now, just render a placeholder
}
```
**What it should do:**
- Render a section with heading "Encoder"
- Display placeholder text: "Enter a 12-bit message (0-4095)"
- Add empty form structure (we'll fill it in Stage 2)

---

#### 3. `src/components/Decoder.tsx`
**What it should do:**
- Render a section with heading "Decoder"
- Display placeholder text: "Enter a 23-bit codeword (0-8388607)"
- Add empty form structure (we'll fill it in Stage 2)

---

#### 4. `src/components/Footer.tsx`
**What it should do:**
- Display brief explanation of Golay (23,12,7) code
- Show what it can do: "Encodes 12 bits → 23 bits, Corrects up to 3 errors"
- Include a note about educational purpose

---

#### 5. Update `src/App.tsx`
**What it should do:**
- Import and render all 4 components
- Use the existing minimal CSS classes from App.css
- Structure: Header → Encoder → Decoder → Footer

---

**React Concepts You'll Practice:**
- ✅ Functional component syntax
- ✅ JSX (mixing HTML-like syntax with JavaScript)
- ✅ Props (passing data to components)
- ✅ Optional props with `?`
- ✅ Component composition (building UI from small pieces)
- ✅ TypeScript interfaces for props

**Success Criteria:**
- ✅ All 4 components render without errors
- ✅ Header displays title from props
- ✅ No inline styles (use className)
- ✅ Clean file structure
- ✅ App builds successfully (`npm run build`)

**Files to Modify/Create:**
```
client/src/
├── components/          # Create this folder
│   ├── Header.tsx
│   ├── Encoder.tsx
│   ├── Decoder.tsx
│   └── Footer.tsx
├── App.tsx             # Update to use new components
└── App.css             # Add any needed classes
```

---

## STAGE 2: Form Inputs & State

**Learning Goals:** useState, Controlled Inputs, Event Handlers, Validation, Conditional Rendering

### Assignment 2.1: Encoder Form with State

**Requirements:**
- Add interactive input form to Encoder component
- Implement real-time validation
- Display input state to user

**What to Implement in `Encoder.tsx`:**

```typescript
// State to manage:
// 1. Input value (number)
// 2. Validation error (string or null)
```

**Form Elements:**
1. **Number Input:**
   - Label: "12-bit Message (0-4095)"
   - Type: number
   - Min: 0, Max: 4095
   - Controlled input (value from state)

2. **Validation Logic:**
   - If value < 0 or value > 4095: Show error "Message must be between 0 and 4095"
   - If value is empty: Show error "Please enter a message"
   - If valid: No error shown

3. **Display Current Value:**
   - Show decimal value
   - Show binary representation (12 bits with leading zeros)
   - Example: "Message: 42 (Binary: 000000101010)"

4. **Encode Button:**
   - Disabled when input is invalid
   - For now, just logs to console when clicked
   - Will connect to API in Stage 3

---

### Assignment 2.2: Decoder Form with State

**Requirements:**
- Same structure as Encoder, but for 23-bit codeword

**What to Implement in `Decoder.tsx`:**

**Form Elements:**
1. **Number Input:**
   - Label: "23-bit Codeword (0-8388607)"
   - Type: number
   - Min: 0, Max: 8388607

2. **Validation Logic:**
   - If value < 0 or value > 8388607: Show error
   - If empty: Show error

3. **Display Current Value:**
   - Show decimal value
   - Show binary representation (23 bits with leading zeros)

4. **Decode Button:**
   - Disabled when invalid
   - Logs to console when clicked

---

**React Concepts You'll Practice:**
- ✅ `useState` hook for managing data
- ✅ Controlled inputs (value + onChange)
- ✅ Event handlers (onChange, onClick)
- ✅ Conditional rendering (`{error && <div>...}`)
- ✅ Template literals for binary conversion
- ✅ Number validation logic
- ✅ Disabling buttons based on state

**Helper Functions You'll Need:**

```typescript
// Convert number to binary string with padding
function toBinaryString(num: number, bits: number): string {
  return num.toString(2).padStart(bits, '0');
}
```

**Success Criteria:**
- ✅ Input updates as user types
- ✅ Shows current value in both decimal and binary
- ✅ Validation errors display in real-time
- ✅ Button is disabled when input is invalid
- ✅ Console logs when button is clicked
- ✅ Both Encoder and Decoder work independently

---

## STAGE 3: API Integration

**Learning Goals:** useEffect, fetch API, async/await, Loading States, Error Handling

### Assignment 3.1: Matrix Display Component

**Requirements:**
- Create a new component that fetches and displays Golay matrices
- Handle loading and error states
- Render matrices as HTML tables

**Create `src/components/MatrixDisplay.tsx`:**

**API Endpoints to Use:**
```
GET http://localhost:5081/golay/matrix-p
GET http://localhost:5081/golay/matrix-identity
GET http://localhost:5081/golay/generator-matrix
```

**State to Manage:**
```typescript
// 1. matrixP: number[][] | null
// 2. matrixI: number[][] | null
// 3. matrixG: number[][] | null
// 4. loading: boolean
// 5. error: string | null
```

**What to Implement:**

1. **Fetch on Component Mount:**
   - Use `useEffect` with empty dependency array `[]`
   - Fetch all three matrices in parallel using `Promise.all()`
   - Set loading to true before fetch, false after

2. **Display Loading State:**
   - While loading: Show "Loading matrices..."

3. **Display Error State:**
   - If fetch fails: Show error message

4. **Display Matrices:**
   - Render each matrix as an HTML `<table>`
   - Matrix heading with description:
     - "Parity Matrix P (12×11)"
     - "Identity Matrix I (12×12)"
     - "Generator Matrix G (12×23) = [I | P]"
   - Each row in a `<tr>`, each cell in a `<td>`
   - Use `.map()` to iterate over rows and cells

5. **Add to App.tsx:**
   - Render MatrixDisplay between Decoder and Footer

---

### Assignment 3.2: Connect Encoder to API

**Requirements:**
- Make the Encode button actually call the backend API
- Display the encoding result

**Update `Encoder.tsx`:**

**API Endpoint:**
```
POST http://localhost:5081/golay/encode
Content-Type: application/json

Request Body: { "message": 42 }

Response: {
  "message": 42,
  "messageBinary": "000000101010",
  "codeword": 5678901,
  "codewordBinary": "10101101010101010101010"
}
```

**State to Add:**
```typescript
// 1. result: EncodeResponse | null
// 2. loading: boolean
// 3. apiError: string | null
```

**What to Implement:**

1. **Encode Button Handler:**
   ```typescript
   async function handleEncode() {
     // Set loading to true
     // Make POST request to /golay/encode
     // Parse JSON response
     // Update result state
     // Handle errors with try/catch
   }
   ```

2. **Display Result:**
   - Show result only if not null
   - Display in a nice format:
     ```
     Input Message: 42 (000000101010)
     Encoded Codeword: 5678901 (10101101010101010101010)
     ```

3. **Loading State:**
   - Disable button while loading
   - Show "Encoding..." text on button when loading

---

### Assignment 3.3: Connect Decoder to API

**Requirements:**
- Same as 3.2, but for decoder

**Update `Decoder.tsx`:**

**API Endpoint:**
```
POST http://localhost:5081/golay/decode
Content-Type: application/json

Request Body: { "codeword": 5678901 }

Response: {
  "codeword": 5678901,
  "codewordBinary": "10101101010101010101010",
  "message": 42,
  "messageBinary": "000000101010"
}
```

**Same pattern as Encoder:**
- Add result state
- Add loading state
- Create async handleDecode function
- Display result
- Show loading indicator

---

**React Concepts You'll Practice:**
- ✅ `useEffect` with dependency arrays
- ✅ `fetch()` API for HTTP requests
- ✅ `async/await` for handling promises
- ✅ `.then()` chains (alternative to async/await)
- ✅ Error handling with try/catch
- ✅ Loading states for better UX
- ✅ `Promise.all()` for parallel requests
- ✅ Array `.map()` for rendering lists
- ✅ Conditional rendering based on state

**Success Criteria:**
- ✅ Matrices load automatically when page loads
- ✅ Shows loading state while fetching
- ✅ Displays all three matrices correctly
- ✅ Encode button sends API request and shows result
- ✅ Decode button sends API request and shows result
- ✅ Loading indicators work during API calls
- ✅ Errors are handled gracefully
- ✅ Console shows no errors

---

## STAGE 4: Custom Hooks

**Learning Goals:** Custom Hooks, Code Reusability, DRY Principle, TypeScript Generics

### Assignment 4.1: Create useGolayAPI Hook

**Requirements:**
- Extract all API logic into a custom hook
- Make components cleaner and more focused on UI

**Create `src/hooks/useGolayAPI.ts`:**

```typescript
// This hook should encapsulate ALL Golay API operations
```

**What to Implement:**

1. **Hook Structure:**
   ```typescript
   export function useGolayAPI() {
     // State for matrices
     // State for loading/errors

     // Function: encode(message: number)
     // Function: decode(codeword: number)
     // Function: fetchMatrices()

     // useEffect to fetch matrices on mount

     // Return object with all functions and state
     return {
       matrices: { matrixP, matrixI, matrixG },
       encode,
       decode,
       loading,
       error
     };
   }
   ```

2. **Encode Function:**
   - Takes message number
   - Returns Promise with encode result
   - Handles loading and errors internally

3. **Decode Function:**
   - Takes codeword number
   - Returns Promise with decode result
   - Handles loading and errors internally

4. **Matrix Fetching:**
   - Automatically fetches matrices when hook is used
   - Stores in state
   - Provides to components

---

### Assignment 4.2: Refactor Components to Use Hook

**Requirements:**
- Update all components to use the custom hook
- Remove duplicate API logic

**Update These Components:**

1. **Encoder.tsx:**
   ```typescript
   function Encoder() {
     const { encode, loading, error } = useGolayAPI();
     const [result, setResult] = useState(null);

     async function handleEncode() {
       const data = await encode(inputValue);
       setResult(data);
     }
   }
   ```

2. **Decoder.tsx:**
   - Same pattern as Encoder

3. **MatrixDisplay.tsx:**
   ```typescript
   function MatrixDisplay() {
     const { matrices, loading, error } = useGolayAPI();
     // Just display the matrices, no fetching logic!
   }
   ```

---

**React Concepts You'll Practice:**
- ✅ Creating custom hooks (must start with "use")
- ✅ Extracting reusable logic
- ✅ Sharing state between components through hooks
- ✅ Keeping components simple and focused
- ✅ DRY principle (Don't Repeat Yourself)
- ✅ TypeScript return types for hooks

**Success Criteria:**
- ✅ All API logic is centralized in useGolayAPI hook
- ✅ Components are simpler and cleaner
- ✅ No duplicate fetch code
- ✅ Everything still works as before
- ✅ Easy to add new API endpoints later

---

## STAGE 5: Error Simulation

**Learning Goals:** Complex State Management, Array Manipulation, Derived State, Interactive UI

### Assignment 5.1: Bit Flipper Component

**Requirements:**
- Create interactive UI to introduce errors into codewords
- Demonstrate Golay's error correction capability

**Create `src/components/ErrorSimulator.tsx`:**

**What to Implement:**

1. **State Management:**
   ```typescript
   // Original codeword (23 bits as number)
   // Bit array (23 booleans: true=1, false=0)
   // Decode result after introducing errors
   ```

2. **Bit Display:**
   - Display all 23 bits as individual clickable elements
   - Each bit shows 0 or 1
   - Clicking a bit flips it (0→1 or 1→0)
   - Highlight bits that have been flipped (different from original)

3. **Error Counter:**
   - Count how many bits differ from original
   - Display: "Errors introduced: X/23"
   - Show warning if more than 3 errors (Golay can only correct up to 3)

4. **Workflow:**
   ```
   1. User enters or uses a codeword from encoder
   2. Display as 23 clickable bits
   3. User clicks bits to introduce errors
   4. Click "Decode with Errors" button
   5. Show decoded message
   6. Compare with original message
   ```

5. **Display Results:**
   ```
   Original Codeword: 5678901 (binary)
   Corrupted Codeword: 5678905 (binary)
   Errors Introduced: 2

   Decoded Message: 42 (000000101010)
   Original Message: 42 (000000101010)
   Status: ✓ Successfully corrected!
   ```

---

**React Concepts You'll Practice:**
- ✅ Managing array state
- ✅ Updating specific array elements
- ✅ Spread operator for immutable updates
- ✅ Event handlers with parameters
- ✅ Derived state (calculate from existing state)
- ✅ Conditional CSS classes
- ✅ Converting between number and binary array

**Helper Functions You'll Need:**

```typescript
// Convert number to array of bits
function numberToBitArray(num: number, length: number): boolean[] {
  const binary = num.toString(2).padStart(length, '0');
  return binary.split('').map(bit => bit === '1');
}

// Convert bit array to number
function bitArrayToNumber(bits: boolean[]): number {
  const binary = bits.map(b => b ? '1' : '0').join('');
  return parseInt(binary, 2);
}

// Count differences between two bit arrays
function countErrors(original: boolean[], modified: boolean[]): number {
  return original.filter((bit, i) => bit !== modified[i]).length;
}
```

**Success Criteria:**
- ✅ Can click individual bits to flip them
- ✅ Shows count of errors introduced
- ✅ Warns if more than 3 errors
- ✅ Successfully decodes corrupted codeword
- ✅ Shows comparison between original and decoded
- ✅ Visual indication of which bits were flipped

---

## STAGE 6: Polish & Enhancement

**Learning Goals:** Tab Navigation, Global State (Context API), Code Organization

### Assignment 6.1: Add Tab Navigation

**Requirements:**
- Organize the app into logical sections with tabs
- Only show one section at a time

**Update `App.tsx`:**

**Tabs to Create:**
1. Encoder
2. Decoder
3. Matrices
4. Error Simulator
5. About (explanation of Golay code)

**What to Implement:**

1. **Tab State:**
   ```typescript
   type Tab = 'encoder' | 'decoder' | 'matrices' | 'simulator' | 'about';
   const [activeTab, setActiveTab] = useState<Tab>('encoder');
   ```

2. **Tab Navigation UI:**
   - Horizontal buttons for each tab
   - Active tab has different style (use className="active")
   - Clicking tab switches content

3. **Conditional Rendering:**
   ```typescript
   {activeTab === 'encoder' && <Encoder />}
   {activeTab === 'decoder' && <Decoder />}
   {activeTab === 'matrices' && <MatrixDisplay />}
   {activeTab === 'simulator' && <ErrorSimulator />}
   {activeTab === 'about' && <About />}
   ```

4. **Create About Component:**
   - Explain what Golay (23,12,7) code is
   - Show the encoding formula: C = M × G
   - Explain error correction capability
   - Link to Wikipedia or other resources

---

### Assignment 6.2: Context API for Shared State (Optional)

**Requirements:**
- Use Context to share data between Encoder and Error Simulator
- Avoid prop drilling

**Create `src/context/GolayContext.tsx`:**

```typescript
interface GolayContextType {
  lastEncoded: EncodeResult | null;
  setLastEncoded: (result: EncodeResult) => void;
}

export const GolayContext = createContext<GolayContextType | null>(null);

export function GolayProvider({ children }) {
  const [lastEncoded, setLastEncoded] = useState(null);

  return (
    <GolayContext.Provider value={{ lastEncoded, setLastEncoded }}>
      {children}
    </GolayContext.Provider>
  );
}

export function useGolay() {
  const context = useContext(GolayContext);
  if (!context) throw new Error('useGolay must be used within GolayProvider');
  return context;
}
```

**What to Implement:**

1. **Wrap App in Provider:**
   ```typescript
   // main.tsx
   <GolayProvider>
     <App />
   </GolayProvider>
   ```

2. **Use in Encoder:**
   ```typescript
   const { setLastEncoded } = useGolay();

   async function handleEncode() {
     const result = await encode(message);
     setLastEncoded(result); // Save for other components
   }
   ```

3. **Use in Error Simulator:**
   ```typescript
   const { lastEncoded } = useGolay();

   // Pre-populate with last encoded codeword
   ```

---

**React Concepts You'll Practice:**
- ✅ Conditional rendering for tabs
- ✅ State management for UI state
- ✅ `createContext` and `useContext`
- ✅ Provider pattern
- ✅ Custom context hooks
- ✅ Avoiding prop drilling
- ✅ Global state management

**Success Criteria:**
- ✅ Tab navigation works smoothly
- ✅ Only one section visible at a time
- ✅ Active tab is visually distinct
- ✅ Context shares data between components
- ✅ Error Simulator can use last encoded value
- ✅ About page explains Golay code clearly

---

## 🎯 Final Project Structure

```
client/src/
├── components/
│   ├── Header.tsx
│   ├── Footer.tsx
│   ├── Encoder.tsx
│   ├── Decoder.tsx
│   ├── MatrixDisplay.tsx
│   ├── ErrorSimulator.tsx
│   └── About.tsx
├── hooks/
│   └── useGolayAPI.ts
├── context/
│   └── GolayContext.tsx
├── App.tsx
├── App.css
└── main.tsx
```

---

## 📚 React Concepts Summary

By completing all stages, you will have practiced:

**Core Concepts:**
- ✅ Functional Components
- ✅ JSX Syntax
- ✅ Props and TypeScript Interfaces
- ✅ useState Hook
- ✅ useEffect Hook
- ✅ Event Handlers
- ✅ Controlled Inputs

**Advanced Concepts:**
- ✅ Custom Hooks
- ✅ Context API
- ✅ Async/Await
- ✅ Error Handling
- ✅ Loading States
- ✅ Conditional Rendering
- ✅ Array Manipulation
- ✅ Derived State

**JavaScript/TypeScript:**
- ✅ Arrow Functions
- ✅ Destructuring
- ✅ Spread Operator
- ✅ Template Literals
- ✅ Array Methods (map, filter)
- ✅ Promises
- ✅ Type Safety

---

## 🚀 Getting Started

1. **Start with Stage 1** - Build the static layout
2. **Complete each assignment** in order
3. **Test after each stage** - Make sure everything works
4. **Ask questions** when stuck
5. **Experiment** - Try breaking things and fixing them

**Ready to begin? Start with Stage 1, Assignment 1.1!**
