// src/config.js
export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

// Control which filter sections are visible in the UI.
// All sections are present here — set to `true` to show, `false` to hide.
// These can be toggled per-environment by editing this file or by
// later wiring them to Vite env variables if needed.
export const FILTER_SECTIONS = {
  assetTypes: true,
  privacy: false,
  domain: true,
  division: false,
  // serviceLine removed
  biPlatform: true,
};

// Toggle displaying counts next to filter values and whether
// to call the counts API on initial load. Can also be overridden
// via Vite env `VITE_FILTER_COUNT=true|false`.
export const FILTER_COUNT = String(import.meta.env.VITE_FILTER_COUNT || 'false').toLowerCase() === 'true';

// Feature flags to toggle UI capabilities without removing code
export const FEATURE_FLAGS = {
  allowManualAdd: false,
  allowManualEdit: false,
};
