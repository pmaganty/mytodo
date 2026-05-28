import axios from "axios";

/**
 * Configured Axios instance used for all API calls throughout the app.
 * Reads the API base URL from the VITE_API_URL environment variable in production,
 * falling back to localhost for local development.
 */
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? "http://localhost:5138",
});

/**
 * Request interceptor — runs before every API call.
 * Automatically attaches the JWT token to the Authorization header
 * so individual API calls never need to think about authentication.
 */
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

/**
 * Response interceptor — runs after every API response.
 * Handles expired or invalid tokens globally — if any request returns 401,
 * the token is cleared and the user is redirected to login.
 *
 * Note: A production improvement would be to attempt a token refresh here
 * before logging the user out (using a refresh token), so short-lived access
 * tokens can be renewed transparently without interrupting the user.
 */
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem("token");
      window.location.href = "/login";
    }
    return Promise.reject(error);
  }
);

export default api;
