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
  serviceLine: false,
  dataSource: true,
};
