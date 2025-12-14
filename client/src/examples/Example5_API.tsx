/* eslint-disable react-refresh/only-export-components */
/**
 * EXAMPLE 5: API Calls with useEffect
 *
 * What are API calls?
 * - Fetching data from a server/website
 * - Sending data to a server
 * - Common in modern web apps (get users, posts, products, etc.)
 *
 * Why combine with useEffect?
 * - useEffect runs after component loads
 * - Perfect timing to fetch data from an API
 * - Can show loading state while waiting
 * - Can handle errors if something goes wrong
 *
 * We'll use JSONPlaceholder - a free fake API for testing
 * https://jsonplaceholder.typicode.com/
 */

import { useState, useEffect } from 'react';

// ============================================
// TYPE DEFINITION
// ============================================

// Interface: Defines the shape of a Post object from the API
// This helps TypeScript catch errors if the data doesn't match
interface Post {
  id: number;
  title: string;
  body: string;
  userId: number;
}

// ============================================
// EXAMPLE 1: Simple API Call
// ============================================

function SimpleFetch() {
  // State to store the fetched post
  // Post | null means it can be a Post object or null (when loading)
  const [post, setPost] = useState<Post | null>(null);

  useEffect(() => {
    // fetch() is a browser API for making HTTP requests
    // It returns a Promise that resolves to a Response object
    fetch('https://jsonplaceholder.typicode.com/posts/1')
      // .then() handles the response when it arrives
      // response.json() converts the response body to JavaScript object
      .then((response) => response.json())
      // The data is now available, so we save it to state
      .then((data) => setPost(data));
  }, []); // Empty array = run once when component mounts

  // Conditional rendering: if post is null, show loading
  if (!post) {
    return <div>Loading...</div>;
  }

  // Once post is loaded, show the data
  return (
    <div className="example-box">
      <h3>Simple Fetch Example</h3>
      <h4>{post.title}</h4>
      <p>{post.body}</p>
      <small>Post ID: {post.id}, User ID: {post.userId}</small>
    </div>
  );
}

// ============================================
// EXAMPLE 2: With Loading State
// ============================================

function FetchWithLoading() {
  const [post, setPost] = useState<Post | null>(null);
  // Separate loading state makes it clearer what's happening
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setLoading(true); // Start loading
    fetch('https://jsonplaceholder.typicode.com/posts/2')
      .then((response) => response.json())
      .then((data) => {
        setPost(data);
        setLoading(false); // Done loading
      });
  }, []);

  // Show loading message while fetching
  if (loading) {
    return (
      <div className="example-box">
        <h3>Loading...</h3>
      </div>
    );
  }

  // Show data when done
  return (
    <div className="example-box">
      <h3>Fetch with Loading State</h3>
      {/* Optional chaining (?.) safely accesses properties even if post is null */}
      <h4>{post?.title}</h4>
      <p>{post?.body}</p>
    </div>
  );
}

// ============================================
// EXAMPLE 3: With Error Handling
// ============================================

function FetchWithError() {
  const [post, setPost] = useState<Post | null>(null);
  const [loading, setLoading] = useState(true);
  // Error state to store error messages
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setLoading(true);
    setError(null); // Clear any previous errors

    fetch('https://jsonplaceholder.typicode.com/posts/3')
      .then((response) => {
        // Check if the response was successful (status 200-299)
        if (!response.ok) {
          throw new Error('Network response was not ok');
        }
        return response.json();
      })
      .then((data) => {
        setPost(data);
        setLoading(false);
      })
      // .catch() handles any errors that occurred
      .catch((err) => {
        setError(err.message);
        setLoading(false);
      });
  }, []);

  // Different UI for different states
  if (loading) return <div>Loading...</div>;
  if (error) return <div className="error">Error: {error}</div>;

  return (
    <div className="example-box">
      <h3>Fetch with Error Handling</h3>
      <h4>{post?.title}</h4>
      <p>{post?.body}</p>
    </div>
  );
}

// ============================================
// EXAMPLE 4: Fetch Multiple Items (List)
// ============================================

function FetchList() {
  // Post[] means an array of Post objects
  const [posts, setPosts] = useState<Post[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // _limit=5 is a query parameter to limit results to 5 posts
    fetch('https://jsonplaceholder.typicode.com/posts?_limit=5')
      .then((response) => response.json())
      .then((data) => {
        setPosts(data); // data is an array of posts
        setLoading(false);
      });
  }, []);

  if (loading) return <div>Loading posts...</div>;

  return (
    <div className="example-box">
      <h3>Fetch List Example (5 posts)</h3>
      {/* map() creates a div for each post in the array */}
      {/* key prop is required for list items - helps React track them */}
      {posts.map((post) => (
        <div key={post.id} className="post-item">
          <h4>{post.id}. {post.title}</h4>
          {/* substring() gets first 100 characters */}
          <p>{post.body.substring(0, 100)}...</p>
        </div>
      ))}
    </div>
  );
}

// ============================================
// EXAMPLE 5: Interactive - Fetch on Button Click
// ============================================

function FetchOnDemand() {
  const [postId, setPostId] = useState(1);
  const [post, setPost] = useState<Post | null>(null);
  const [loading, setLoading] = useState(false);

  // This function is called when the button is clicked
  // Unlike useEffect, this fetches on demand, not automatically
  const fetchPost = () => {
    setLoading(true);
    // Template literal: `...${postId}` inserts the postId into the URL
    fetch(`https://jsonplaceholder.typicode.com/posts/${postId}`)
      .then((response) => response.json())
      .then((data) => {
        setPost(data);
        setLoading(false);
      });
  };

  return (
    <div className="example-box">
      <h3>Fetch On Demand</h3>
      <div>
        <label>Post ID: </label>
        <input
          type="number"
          value={postId}
          onChange={(e) => setPostId(Number(e.target.value))}
          min="1"
          max="100"
        />
        {/* Button triggers the fetch manually */}
        <button onClick={fetchPost}>Fetch Post</button>
      </div>
      {/* Conditional rendering with && operator */}
      {loading && <p>Loading...</p>}
      {post && !loading && (
        <div className="result-box">
          <h4>{post.title}</h4>
          <p>{post.body}</p>
        </div>
      )}
    </div>
  );
}

// ============================================
// EXAMPLE 6: Custom Hook for API Calls
// ============================================

// Generic custom hook for fetching data
// <T> makes it work with any data type, not just Post
function useFetch<T>(url: string) {
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setLoading(true);
    setError(null);

    fetch(url)
      .then((response) => {
        if (!response.ok) throw new Error('Failed to fetch');
        return response.json();
      })
      .then((data) => {
        setData(data);
        setLoading(false);
      })
      .catch((err) => {
        setError(err.message);
        setLoading(false);
      });
  }, [url]); // Re-fetch if URL changes

  // Return all three pieces of state
  return { data, loading, error };
}

function CustomHookExample() {
  // Using our custom hook - much cleaner than writing all the fetch logic again!
  const { data, loading, error } = useFetch<Post>(
    'https://jsonplaceholder.typicode.com/posts/1'
  );

  if (loading) return <div>Loading with custom hook...</div>;
  if (error) return <div>Error: {error}</div>;

  return (
    <div className="example-box">
      <h3>Custom Hook Example</h3>
      <h4>{data?.title}</h4>
      <p>{data?.body}</p>
      <p className="hint">
        This uses a reusable useFetch hook!
      </p>
    </div>
  );
}

// ============================================
// PARENT COMPONENT
// ============================================

export default function Example5_API() {
  return (
    <div className="example-container">
      <h2>Example 5: Understanding API Calls</h2>

      <div className="info-box">
        <h4>What's happening here:</h4>
        <ol>
          <li>Using fetch() to get data from JSONPlaceholder API</li>
          <li>useEffect runs the fetch when component loads</li>
          <li>Store fetched data in state</li>
          <li>Show loading state while waiting</li>
          <li>Handle errors gracefully</li>
        </ol>
        <h4>API Flow:</h4>
        <pre>
{`1. Component loads
2. useEffect runs
3. fetch() sends request
4. Show "Loading..."
5. Response arrives
6. Update state with data
7. Component re-renders with data`}
        </pre>
      </div>

      {/* All the example components */}
      <SimpleFetch />
      <FetchWithLoading />
      <FetchWithError />
      <FetchList />
      <FetchOnDemand />
      <CustomHookExample />
    </div>
  );
}

/**
 * KEY TAKEAWAYS:
 *
 * 1. Use fetch() to get data from APIs
 * 2. Use useEffect to run fetch when component loads
 * 3. Store response in state with useState
 * 4. Always handle loading state
 * 5. Always handle errors
 * 6. Can create custom hooks for reusable fetch logic
 * 7. Use .then() to handle async responses
 * 8. Promises: fetch returns a Promise that resolves when data arrives
 */
