/**
 * API Configuration
 *
 * In development: Uses localhost:5081
 * In production (Docker): Uses relative URL (proxied by nginx)
 */

const isDevelopment = import.meta.env.DEV;

export const API_BASE = isDevelopment
  ? 'http://localhost:5081/golay'
  : '/golay';

export default {
  API_BASE
};
